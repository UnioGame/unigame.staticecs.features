using System;
using UnityEngine;

namespace unigame.staticecs.features {
    [Serializable]
    public sealed class SetPrimaryTargetFromAoeStepConfig : IAbilityStepConfig {
        [SerializeField] private string _nodeGuid;
        [SerializeField] private AoeTargetSelector _selector;

        public SetPrimaryTargetFromAoeStepConfig() { }

        public SetPrimaryTargetFromAoeStepConfig(AoeTargetSelector selector, string nodeGuid = null) {
            _selector = selector;
            _nodeGuid = nodeGuid;
        }

        public AbilityStepKind Kind => AbilityStepKind.SetPrimaryTargetFromAoe;
        public string NodeGuid => _nodeGuid;
        public AoeTargetSelector Selector => _selector;
    }
}
