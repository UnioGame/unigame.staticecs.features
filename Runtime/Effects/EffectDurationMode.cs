namespace UniGame.StaticEcs.Features
{
    /// <summary>Defines what limits the lifetime of an effect.</summary>
    public enum EffectDurationMode
    {
        /// <summary>The effect lasts a number of seconds.</summary>
        Time = 0,

        /// <summary>The effect lasts a number of activations; each activation spends one charge.</summary>
        Charges = 1,

        /// <summary>
        /// The effect lives only for its application: it is applied, then removed as expired in the same call. It is
        /// never written to the target, never listed in the roster, never ticks and keeps no back-reference to its source.
        /// </summary>
        Instant = 2,
    }
}
