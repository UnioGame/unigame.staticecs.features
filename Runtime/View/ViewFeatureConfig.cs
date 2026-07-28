namespace UniGame.StaticEcs.Features
{
    using System;

    /// <summary>Configures View Feature system scheduling.</summary>
    [Serializable]
    public struct ViewFeatureConfig
    {
        /// <summary>Late update order used after gameplay systems.</summary>
        public short lifecycleOrder;

        /// <summary>Default View Feature configuration.</summary>
        public static readonly ViewFeatureConfig Default = new()
        {
            lifecycleOrder = 10000
        };
    }
}
