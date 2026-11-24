namespace UniGame.StaticEcs.Features
{
    using System;
    using FFS.Libraries.StaticEcs;
    using UnityEngine.UIElements;

    /// <summary>Configures an element entity from a UXML object and releases owned state.</summary>
    [Serializable]
    [UxmlObject]
    public abstract partial class UiToolkitElementConverter
    {
        [UxmlAttribute]
        public bool isEnabled = true;

        public abstract void Apply<TWorld>(World<TWorld>.Entity entity)
            where TWorld : struct, IWorldType;
    }

    /// <summary>Receives cleanup before an element entity is destroyed.</summary>
    public interface IUiToolkitElementDestroyHandler
    {
        void OnEntityDestroy<TWorld>(World<TWorld>.Entity entity)
            where TWorld : struct, IWorldType;
    }
}
