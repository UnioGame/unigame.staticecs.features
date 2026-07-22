namespace UniGame.StaticEcs.Features
{
    using System;
    using FFS.Libraries.StaticEcs;

    [Serializable]
    public struct ManaRegenComponent : IComponent
    {
        public float Rate;
    }
}
