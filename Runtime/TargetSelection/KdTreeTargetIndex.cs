namespace UniGame.StaticEcs.Features
{
    using System;
    using System.Buffers;
    using System.Collections.Generic;
    using FFS.Libraries.StaticEcs;
    using Unity;
    using UnityEngine;

    /// <summary>
    /// Managed v1 KD-tree implementation of <see cref="ITargetIndex{TWorld}"/>. Rebuilds from
    /// every entity carrying <see cref="TargetableTag"/> + <see cref="TransformComponent"/>.
    /// The tree is rebuilt in full each call (no incremental updates); a DOD refactor is tracked
    /// separately. Allocations during query are bounded to the recursion stack.
    /// </summary>
    public class KdTreeTargetIndex<TWorld> : ITargetIndex<TWorld>
        where TWorld : struct, IWorldType
    {
        private const int StackDistanceCapacity = 256;

        private readonly List<EntityGID> _ids = new();
        private readonly List<Vector3> _positions = new();
        private int[] _indices = Array.Empty<int>();
        private int _root = -1;
        private KdNode[] _nodes = Array.Empty<KdNode>();
        private int _nodeCount;

        public int Count => _ids.Count;

        public void Rebuild()
        {
            _ids.Clear();
            _positions.Clear();

            foreach (
                var entity in World<TWorld>
                    .Query<All<TargetableTag, TransformComponent>>()
                    .Entities()
            )
            {
                ref readonly var binding = ref entity.Read<TransformComponent>();
                if (binding.Transform == null)
                {
                    continue;
                }

                _ids.Add(entity.GID);
                _positions.Add(binding.Transform.position);
            }

            var n = _ids.Count;
            if (_indices.Length < n)
            {
                _indices = new int[System.Math.Max(8, n)];
            }

            if (_nodes.Length < n)
            {
                _nodes = new KdNode[System.Math.Max(8, n)];
            }

            for (var i = 0; i < n; i++)
            {
                _indices[i] = i;
            }

            _nodeCount = 0;
            _root = n == 0 ? -1 : Build(0, n - 1, 0);
        }

        public int FillSphere(Vector3 center, float radius, Span<EntityGID> output)
        {
            if (output.Length == 0 || _ids.Count == 0 || _root < 0)
            {
                return 0;
            }

            var written = 0;
            var radiusSq = radius * radius;
            QuerySphere(_root, center, radiusSq, output, ref written);
            return written;
        }

        /// <inheritdoc />
        public int FillNearestSphere(
            Vector3 center,
            float radius,
            Span<EntityGID> output,
            EntityGID excluded = default)
        {
            if (output.Length == 0 || _ids.Count == 0 || _root < 0)
            {
                return 0;
            }

            float[] rentedDistances = null;
            Span<float> distances = output.Length <= StackDistanceCapacity
                ? stackalloc float[output.Length]
                : (rentedDistances = ArrayPool<float>.Shared.Rent(output.Length)).AsSpan(
                    0,
                    output.Length);
            try
            {
                var written = 0;
                var radiusSq = radius * radius;
                QueryNearestSphere(
                    _root,
                    center,
                    radiusSq,
                    excluded,
                    output,
                    distances,
                    ref written);
                return written;
            }
            finally
            {
                if (rentedDistances != null)
                {
                    ArrayPool<float>.Shared.Return(rentedDistances);
                }
            }
        }

        private int Build(int from, int to, int depth)
        {
            if (from > to)
            {
                return -1;
            }

            var axis = depth % 3;
            var mid = (from + to) >> 1;
            QuickSelect(from, to, mid, axis);

            var nodeIndex = _nodeCount++;
            _nodes[nodeIndex] = new KdNode
            {
                Axis = axis,
                PointIndex = _indices[mid],
                Left = Build(from, mid - 1, depth + 1),
                Right = Build(mid + 1, to, depth + 1),
            };
            return nodeIndex;
        }

        private void QuickSelect(int left, int right, int k, int axis)
        {
            while (left < right)
            {
                var pivotIndex = Partition(left, right, axis);
                if (pivotIndex == k)
                {
                    return;
                }

                if (k < pivotIndex)
                {
                    right = pivotIndex - 1;
                }
                else
                {
                    left = pivotIndex + 1;
                }
            }
        }

        private int Partition(int left, int right, int axis)
        {
            var pivotValue = AxisValue(_indices[right], axis);
            var store = left;
            for (var i = left; i < right; i++)
            {
                if (AxisValue(_indices[i], axis) < pivotValue)
                {
                    (_indices[i], _indices[store]) = (_indices[store], _indices[i]);
                    store++;
                }
            }

            (_indices[store], _indices[right]) = (_indices[right], _indices[store]);
            return store;
        }

        private float AxisValue(int pointIndex, int axis)
        {
            var p = _positions[pointIndex];
            return axis == 0 ? p.x : (axis == 1 ? p.y : p.z);
        }

        private void QuerySphere(
            int nodeIndex,
            Vector3 center,
            float radiusSq,
            Span<EntityGID> output,
            ref int written
        )
        {
            if (nodeIndex < 0 || written >= output.Length)
            {
                return;
            }

            ref var node = ref _nodes[nodeIndex];
            var pos = _positions[node.PointIndex];
            var dx = pos.x - center.x;
            var dy = pos.y - center.y;
            var dz = pos.z - center.z;
            if (dx * dx + dy * dy + dz * dz <= radiusSq)
            {
                output[written++] = _ids[node.PointIndex];
                if (written >= output.Length)
                {
                    return;
                }
            }

            var centerAxis = node.Axis == 0 ? center.x : (node.Axis == 1 ? center.y : center.z);
            var pointAxis = node.Axis == 0 ? pos.x : (node.Axis == 1 ? pos.y : pos.z);
            var diff = centerAxis - pointAxis;

            if (diff <= 0f)
            {
                QuerySphere(node.Left, center, radiusSq, output, ref written);
                if (diff * diff <= radiusSq)
                {
                    QuerySphere(node.Right, center, radiusSq, output, ref written);
                }
            }
            else
            {
                QuerySphere(node.Right, center, radiusSq, output, ref written);
                if (diff * diff <= radiusSq)
                {
                    QuerySphere(node.Left, center, radiusSq, output, ref written);
                }
            }
        }

        private void QueryNearestSphere(
            int nodeIndex,
            Vector3 center,
            float radiusSq,
            EntityGID excluded,
            Span<EntityGID> output,
            Span<float> distances,
            ref int written)
        {
            if (nodeIndex < 0)
            {
                return;
            }

            ref var node = ref _nodes[nodeIndex];
            var pos = _positions[node.PointIndex];
            var dx = pos.x - center.x;
            var dy = pos.y - center.y;
            var dz = pos.z - center.z;
            var distanceSq = dx * dx + dy * dy + dz * dz;
            var id = _ids[node.PointIndex];
            if (distanceSq <= radiusSq && !id.Equals(excluded))
            {
                InsertNearest(id, distanceSq, output, distances, ref written);
            }

            var centerAxis = node.Axis == 0 ? center.x : (node.Axis == 1 ? center.y : center.z);
            var pointAxis = node.Axis == 0 ? pos.x : (node.Axis == 1 ? pos.y : pos.z);
            var diff = centerAxis - pointAxis;

            if (diff <= 0f)
            {
                QueryNearestSphere(
                    node.Left,
                    center,
                    radiusSq,
                    excluded,
                    output,
                    distances,
                    ref written);
                if (diff * diff <= radiusSq)
                {
                    QueryNearestSphere(
                        node.Right,
                        center,
                        radiusSq,
                        excluded,
                        output,
                        distances,
                        ref written);
                }
            }
            else
            {
                QueryNearestSphere(
                    node.Right,
                    center,
                    radiusSq,
                    excluded,
                    output,
                    distances,
                    ref written);
                if (diff * diff <= radiusSq)
                {
                    QueryNearestSphere(
                        node.Left,
                        center,
                        radiusSq,
                        excluded,
                        output,
                        distances,
                        ref written);
                }
            }
        }

        private static void InsertNearest(
            EntityGID id,
            float distanceSq,
            Span<EntityGID> output,
            Span<float> distances,
            ref int written)
        {
            var insertAt = written;
            for (var i = 0; i < written; i++)
            {
                if (distanceSq < distances[i] ||
                    (distanceSq.Equals(distances[i]) && id.Raw < output[i].Raw))
                {
                    insertAt = i;
                    break;
                }
            }

            if (insertAt >= output.Length)
            {
                return;
            }

            var newWritten = written < output.Length ? written + 1 : written;
            for (var i = newWritten - 1; i > insertAt; i--)
            {
                output[i] = output[i - 1];
                distances[i] = distances[i - 1];
            }

            output[insertAt] = id;
            distances[insertAt] = distanceSq;
            written = newWritten;
        }

        private struct KdNode
        {
            public int Axis;
            public int PointIndex;
            public int Left;
            public int Right;
        }
    }
}
