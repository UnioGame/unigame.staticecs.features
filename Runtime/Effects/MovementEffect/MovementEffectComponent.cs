namespace UniGame.StaticEcs.Features
{
    using System;
    using FFS.Libraries.StaticEcs;

    /// <summary>
    /// Planar direction chosen for the next movement effect of an entity, such as a dash. The caller places it
    /// right before <c>EffectOperations.Apply</c>, because the effect parameters carry no direction; the handler
    /// of the effect consumes it. Push and pull effects share it.
    /// </summary>
    [Serializable]
    public struct MovementEffectComponent : IComponent
    {
        public float DirectionX;

        public float DirectionZ;
    }
}
