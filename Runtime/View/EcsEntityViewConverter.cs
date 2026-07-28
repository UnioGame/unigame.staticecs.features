namespace UniGame.StaticEcs.Features
{
    using System;
    using FFS.Libraries.StaticEcs;
    using UniGame.StaticEcs.Unity;
    using UniGame.UiSystem.Runtime;
    using UnityEngine;

    /// <summary>Binds an embedded View System view to the entity created by its provider.</summary>
    [Serializable]
    public class EcsEntityViewConverter<TWorld> :
        EcsSerializableConverter<TWorld>,
        IEcsConverterDestroyHandler<TWorld>
        where TWorld : struct, IWorldType
    {
        /// <summary>Existing View System view on the authored entity prefab.</summary>
        public ViewBase view;

        /// <summary>View System identifier assigned to the existing view.</summary>
        public string viewId;

        /// <inheritdoc />
        public override void Apply(World<TWorld>.Entity entity, GameObject host)
        {
            if (view == null || string.IsNullOrWhiteSpace(viewId))
                return;

            var request = new ViewRequest
            {
                ViewId = viewId,
                Owner = entity.GID,
                Source = entity.GID,
                Placement = ViewPlacement.Existing(),
                ShowOnOpen = true
            };
            ViewOperations.TryBindExistingOn<TWorld>(
                entity,
                view,
                request,
                out _);
        }

        /// <inheritdoc />
        public void OnEntityDestroyed(World<TWorld>.Entity entity, GameObject host)
        {
            if (entity.Has<ViewComponent>())
            {
                var key = entity.Read<ViewComponent>().Key;
                ViewOperations.Close<TWorld>(key);
            }
        }
    }
}
