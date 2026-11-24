namespace UniGame.StaticEcs.Features
{
    using System;
    using FFS.Libraries.StaticEcs;

    /// <summary>
    /// A dash waiting to be carried out by the movement of the project. The mover removes it once it has taken
    /// over the displacement; the package never moves the entity itself.
    /// </summary>
    [Serializable]
    public struct DashComponent : IComponent
    {
        public float DirectionX;

        public float DirectionZ;

        public float Distance;

        public float Duration;
    }
}
