namespace UniGame.StaticEcs.Features
{
    using System;
    using Sirenix.OdinInspector;
    using UnityEngine;

    /// <summary>
    /// Authored defaults of one status effect type: lifetime and activation modes, their parameters,
    /// stacking caps and the conflict group. Only the fields of the selected modes are shown in the inspector.
    /// </summary>
    [Serializable]
    public class StatusEffectSettings
    {
        /// <summary>Seconds between activations of <see cref="EffectActivationMode.Tick"/> effects.</summary>
        public const float TickSeconds = 1f;

        [LabelText("Duration Mode")]
        public EffectDurationMode durationMode = EffectDurationMode.Time;

        [ShowIf(nameof(IsTimeMode)), MinValue(0.01f), SuffixLabel("s")]
        public float duration = 3f;

        [ShowIf(nameof(IsTimeMode)), MinValue(0.01f), SuffixLabel("s")]
        [Tooltip("Reapplying adds duration up to this cap.")]
        public float maxDuration = 10f;

        [ShowIf(nameof(IsChargesMode)), MinValue(1)]
        public int charges = 3;

        [ShowIf(nameof(IsChargesMode)), MinValue(1)]
        [Tooltip("Reapplying adds charges up to this cap.")]
        public int maxCharges = 10;

        [LabelText("Activation Mode")]
        public EffectActivationMode activationMode = EffectActivationMode.Tick;

        [ShowIf(nameof(IsCustomInterval)), MinValue(0.01f), SuffixLabel("s")]
        public float period = 1f;

        [Tooltip("Effects of one group exclude each other; the higher priority wins, an equal priority is replaced.")]
        public string conflictGroup = string.Empty;

        [ShowIf(nameof(HasConflictGroup))]
        public int conflictPriority;

        [Tooltip("Removes the effect when the entity that applied it is destroyed.")]
        public bool removeWithSource;

        private bool IsTimeMode => durationMode == EffectDurationMode.Time;

        private bool IsChargesMode => durationMode == EffectDurationMode.Charges;

        private bool IsCustomInterval => activationMode == EffectActivationMode.CustomInterval;

        private bool HasConflictGroup => !string.IsNullOrEmpty(conflictGroup);
    }
}
