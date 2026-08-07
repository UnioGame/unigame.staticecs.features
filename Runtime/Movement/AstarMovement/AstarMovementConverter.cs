namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;
    using FFS.Libraries.StaticEcs.Unity;
    using Pathfinding;
    using Unity;
    using UnityEngine;

    /// <summary>
    /// Sets <see cref="AstarAIComponent"/> on conversion by reading <see cref="IAstarAI"/>
    /// from the host <see cref="GameObject"/>.
    /// </summary>
    public class AstarMovementConverter<TWorld> : EcsMonoConverter<TWorld>, IEcsLinkResolver<TWorld>, IEcsConverterDependency<TWorld>
        where TWorld : struct, IWorldType
    {
        [SerializeField]
        private AbstractStaticEcsEntityProvider _graphProvider;

        /// <inheritdoc/>
        public override void Apply(World<TWorld>.Entity entity, GameObject host)
        {
            AstarMovementConverterUtility.Apply(entity, host, _graphProvider);
        }

        /// <inheritdoc/>
        public void ResolveLinks(World<TWorld>.Entity entity, GameObject host)
        {
            AstarMovementConverterUtility.ResolveLinks(entity, _graphProvider);
        }

        /// <inheritdoc/>
        public bool IsReady(GameObject host, out string reason) =>
            AstarGraphDependencyUtility.IsReady<TWorld>(_graphProvider, out reason);
    }

    internal static class AstarMovementConverterUtility
    {
        public static void Apply<TWorld>(
            World<TWorld>.Entity entity,
            GameObject host,
            AbstractStaticEcsEntityProvider graphProvider
        )
            where TWorld : struct, IWorldType
        {
            entity.Set(
                new AstarAIComponent
                {
                    AI = host != null ? host.GetComponent<IAstarAI>() : null,
                    Seeker = host != null ? host.GetComponent<Seeker>() : null,
                    GraphEntity = graphProvider != null ? graphProvider.EntityGid : default,
                }
            );
        }

        public static void ResolveLinks<TWorld>(
            World<TWorld>.Entity entity,
            AbstractStaticEcsEntityProvider graphProvider
        )
            where TWorld : struct, IWorldType
        {
            if (!entity.Has<AstarAIComponent>())
                return;

            entity.Mut<AstarAIComponent>().GraphEntity =
                graphProvider != null ? graphProvider.EntityGid : default;
        }
    }

    internal static class AstarGraphDependencyUtility
    {
        internal static bool IsReady<TWorld>(AbstractStaticEcsEntityProvider graphProvider,
            out string reason) where TWorld : struct, IWorldType
        {
            if (graphProvider == null)
            {
                reason = "A graph provider is required.";
                return false;
            }
            if (World<TWorld>.Status != WorldStatus.Initialized ||
                !graphProvider.EntityGid.TryUnpack<TWorld>(out var graph) ||
                !graph.Has<AstarPathComponent>() ||
                !graph.Has<AstarGridGraphConfigComponent>())
            {
                reason = "The graph provider is not active in the authority world.";
                return false;
            }
            reason = string.Empty;
            return true;
        }
    }
}
