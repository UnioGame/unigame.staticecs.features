using System;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace UniGame.StaticEcs.Features {
    [Serializable]
    [MovedFrom(true, sourceNamespace: "unigame.staticecs.features", sourceAssembly: "unigame.staticecs.features")]
    public sealed class ConditionalStepConfig : IAbilityStepConfig {
        [SerializeField] private string _nodeGuid;
        [SerializeReference] private IAbilityStepCondition _condition;
        [SerializeReference] private IAbilityStepConfig _ifTrue;
        [SerializeReference] private IAbilityStepConfig _ifFalse;

        public ConditionalStepConfig() { }

        public ConditionalStepConfig(
            IAbilityStepCondition condition,
            IAbilityStepConfig ifTrue,
            IAbilityStepConfig ifFalse = null,
            string nodeGuid = null) {
            _condition = condition;
            _ifTrue = ifTrue;
            _ifFalse = ifFalse;
            _nodeGuid = nodeGuid;
        }

        public AbilityStepKind Kind => AbilityStepKind.Conditional;
        public string NodeGuid => _nodeGuid;
        public IAbilityStepCondition Condition => _condition;
        public IAbilityStepConfig IfTrue => _ifTrue;
        public IAbilityStepConfig IfFalse => _ifFalse;
    }
}
