namespace UniGame.StaticEcs.Features
{
    using System;
    using UnityEngine;
    using UnityEngine.Scripting.APIUpdating;

    /// <summary>
    /// Synchronous leaf step that raises a damage / healing event through
    /// <see cref="DamageOperations"/>. Always completes inside the activator
    /// (Damage pipeline consumes the event in its own system in a later tick).
    /// </summary>
    [Serializable]
    [MovedFrom(
        true,
        sourceNamespace: "unigame.staticecs.features",
        sourceAssembly: "unigame.staticecs.features"
    )]
    public sealed class ApplyDamageStepConfig : IAbilityStepConfig
    {
        [SerializeField]
        private string _nodeGuid;

        [SerializeField]
        private float _amount;

        [SerializeField]
        private DamageType _type = DamageType.Physical;

        [SerializeField]
        private AbilityTargetMode _mode = AbilityTargetMode.PrimaryTarget;

        [SerializeField]
        private bool _excludeCaster = true;

        public ApplyDamageStepConfig() { }

        public ApplyDamageStepConfig(
            float amount,
            AbilityTargetMode mode,
            DamageType type = DamageType.Physical,
            bool excludeCaster = true,
            string nodeGuid = null
        )
        {
            _amount = amount;
            _mode = mode;
            _type = type;
            _excludeCaster = excludeCaster;
            _nodeGuid = nodeGuid;
        }

        public AbilityStepKind Kind => AbilityStepKind.ApplyDamage;
        public string NodeGuid => _nodeGuid;
        public float Amount => _amount;
        public DamageType Type => _type;
        public AbilityTargetMode Mode => _mode;
        public bool ExcludeCaster => _excludeCaster;
    }
}
