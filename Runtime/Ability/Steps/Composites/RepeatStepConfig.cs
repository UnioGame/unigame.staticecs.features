using System;
using UnityEngine;

namespace unigame.staticecs.features {
    [Serializable]
    public sealed class RepeatStepConfig : IAbilityStepConfig {
        [SerializeField] private string _nodeGuid;
        [SerializeReference] private IAbilityStepConfig _body;
        [SerializeField] private int _maxIterations;
        [SerializeReference] private IAbilityStepCondition _whileCondition;

        public RepeatStepConfig() { }

        public RepeatStepConfig(
            IAbilityStepConfig body,
            int maxIterations,
            IAbilityStepCondition whileCondition = null,
            string nodeGuid = null) {
            _body = body;
            _maxIterations = maxIterations;
            _whileCondition = whileCondition;
            _nodeGuid = nodeGuid;
        }

        public AbilityStepKind Kind => AbilityStepKind.Repeat;
        public string NodeGuid => _nodeGuid;
        public IAbilityStepConfig Body => _body;
        public int MaxIterations => _maxIterations;
        public IAbilityStepCondition WhileCondition => _whileCondition;
    }
}
