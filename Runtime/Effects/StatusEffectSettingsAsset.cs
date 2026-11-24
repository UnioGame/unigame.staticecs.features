namespace UniGame.StaticEcs.Features
{
    using UnityEngine;

    /// <summary>
    /// ScriptableObject that stores the defaults of one status effect type. Derive one asset class per
    /// status so the effect installer can load its own defaults.
    /// </summary>
    public abstract class StatusEffectSettingsAsset : ScriptableObject
    {
        public StatusEffectSettings settings = new();
    }
}
