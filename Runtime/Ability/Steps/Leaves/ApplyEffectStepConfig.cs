using System;
using UnityEngine;

namespace unigame.staticecs.features {
    [Serializable]
    public sealed class ApplyEffectStepConfig : IAbilityStepConfig {
        [SerializeField] private string _nodeGuid;
        [SerializeField] private int _effectIdValue;
        [SerializeField] private AbilityTargetMode _mode = AbilityTargetMode.PrimaryTarget;
        [SerializeField] private float _duration;
        [SerializeField] private float _period;
        [SerializeField] private float _delay;
        [SerializeField] private float _magnitude;
        [SerializeField] private bool _excludeCaster = true;

        public ApplyEffectStepConfig() { }

        public ApplyEffectStepConfig(
            EffectId effectId,
            AbilityTargetMode mode = AbilityTargetMode.PrimaryTarget,
            float duration = 0f,
            float period = 0f,
            float delay = 0f,
            float magnitude = 0f,
            bool excludeCaster = true,
            string nodeGuid = null) {
            _effectIdValue = effectId.Value;
            _mode = mode;
            _duration = duration;
            _period = period;
            _delay = delay;
            _magnitude = magnitude;
            _excludeCaster = excludeCaster;
            _nodeGuid = nodeGuid;
        }

        public AbilityStepKind Kind => AbilityStepKind.ApplyEffect;
        public string NodeGuid => _nodeGuid;
        public EffectId EffectId => new(_effectIdValue);
        public AbilityTargetMode Mode => _mode;
        public float Duration => _duration;
        public float Period => _period;
        public float Delay => _delay;
        public float Magnitude => _magnitude;
        public bool ExcludeCaster => _excludeCaster;
    }
}
