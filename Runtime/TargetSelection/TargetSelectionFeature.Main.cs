namespace UniGame.StaticEcs.Features
{
    using System;
    using Unity;

    /// <summary>Main-world target selection feature.</summary>
    [Serializable]
    public sealed class TargetSelectionFeature : TargetSelectionFeature<Main>
    {
        /// <summary>Creates the Main-world target selection feature with default configuration.</summary>
        public TargetSelectionFeature() { }

        /// <summary>Creates the Main-world target selection feature.</summary>
        public TargetSelectionFeature(
            bool registerRebuildSystem = true,
            short rebuildOrder = DefaultRebuildOrder
        )
            : base(registerRebuildSystem, rebuildOrder) { }
    }
}
