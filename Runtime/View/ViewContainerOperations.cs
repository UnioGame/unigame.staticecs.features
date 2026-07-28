namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;
    using UniGame.StaticEcs.Unity;
    using UnityEngine;

    /// <summary>Registers named transforms used by ViewPlacement containers.</summary>
    public static class ViewContainerOperations
    {
        /// <summary>Registers a named Main-world container.</summary>
        public static bool Register(string name, Transform parent, int capacity = 1)
        {
            return Register<Main>(name, parent, capacity);
        }

        /// <summary>Registers a named container.</summary>
        public static bool Register<TWorld>(
            string name,
            Transform parent,
            int capacity = 1)
            where TWorld : struct, IWorldType
        {
            if (!World<TWorld>.HasResource<ViewContainerRegistryResource<TWorld>>())
                return false;

            World<TWorld>.GetResource<ViewContainerRegistryResource<TWorld>>()
                .Register(name, parent, capacity);
            return true;
        }

        /// <summary>Unregisters a named Main-world container.</summary>
        public static bool Unregister(string name)
        {
            return Unregister<Main>(name);
        }

        /// <summary>Unregisters a named container.</summary>
        public static bool Unregister<TWorld>(string name)
            where TWorld : struct, IWorldType
        {
            return World<TWorld>.HasResource<ViewContainerRegistryResource<TWorld>>() &&
                   World<TWorld>.GetResource<ViewContainerRegistryResource<TWorld>>()
                       .Unregister(name);
        }
    }
}
