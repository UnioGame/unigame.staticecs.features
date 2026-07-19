using System;
using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using UnityEngine;


namespace UniGame.StaticEcs.Features
{
    using Unity;

    /// <summary>
    /// Managed v1 KD-tree implementation of <see cref="ITargetIndex{TWorld}"/>. Rebuilds from
    /// every entity carrying <see cref="TargetableTag"/> + <see cref="TransformBindingComponent"/>.
    /// The tree is rebuilt in full each call (no incremental updates); a DOD refactor is tracked
    /// separately. Allocations during query are bounded to the recursion stack.
    /// </summary>
    public sealed class KdTreeTargetIndex<TWorld> : ITargetIndex<TWorld>
        where TWorld : struct, IWorldType
    {
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

            foreach (var entity in World<TWorld>.Query<All<TargetableTag, TransformBindingComponent>>()
                         .Entities())
            {
                ref readonly var binding = ref entity.Read<TransformBindingComponent>();
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

        private void QuerySphere(int nodeIndex, Vector3 center, float radiusSq, Span<EntityGID> output, ref int written)
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

        private struct KdNode
        {
            public int Axis;
            public int PointIndex;
            public int Left;
            public int Right;
        }
    }
}
