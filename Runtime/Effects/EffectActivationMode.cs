namespace UniGame.StaticEcs.Features
{
    /// <summary>Defines when a status effect activates its handler.</summary>
    public enum EffectActivationMode
    {
        /// <summary>Activates once per <see cref="StatusEffectSettings.TickSeconds"/>.</summary>
        Tick = 0,

        /// <summary>Activates once per custom period in seconds.</summary>
        CustomInterval = 1,

        /// <summary>Activates while <see cref="IEffectCondition{TWorld,TEffect}"/> reports the condition is met.</summary>
        Condition = 2,
    }
}
