namespace UniGame.StaticEcs.Features
{
    using System;
    using Unity;

    /// <summary>Main-world alias for <see cref="AstarMovementFeature{TWorld}"/>.</summary>
    [Serializable]
    public sealed class AstarMovementFeature : AstarMovementFeature<Main>
    {
        /// <summary>Creates the Main-world feature with default configuration.</summary>
        public AstarMovementFeature() { }

        /// <summary>Creates the Main-world feature with optional system registration.</summary>
        public AstarMovementFeature(
            bool registerGraphSystem = true,
            bool registerMovementSystem = true,
            short graphOrder = DefaultGraphOrder,
            short movementOrder = DefaultMovementOrder
        )
            : base(registerGraphSystem, registerMovementSystem, graphOrder, movementOrder) { }
    }
}
