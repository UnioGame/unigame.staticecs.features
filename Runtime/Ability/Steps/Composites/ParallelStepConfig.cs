using System;
using System.Collections.Generic;
using UnityEngine;

namespace unigame.staticecs.features {
    [Serializable]
    public sealed class ParallelStepConfig : IAbilityStepConfig {
        [SerializeField] private string _nodeGuid;
        [SerializeReference] private List<IAbilityStepConfig> _children = new();
        [SerializeField] private ParallelJoinPolicy _joinPolicy;
        [SerializeField] private bool _cancelRemainingOnJoin = true;

        public ParallelStepConfig() { }

        public ParallelStepConfig(
            IList<IAbilityStepConfig> children,
            ParallelJoinPolicy joinPolicy = ParallelJoinPolicy.AllSuccess,
            bool cancelRemainingOnJoin = true,
            string nodeGuid = null) {
            _nodeGuid = nodeGuid;
            _joinPolicy = joinPolicy;
            _cancelRemainingOnJoin = cancelRemainingOnJoin;
            if (children != null) {
                _children = new List<IAbilityStepConfig>(children);
            }
        }

        public AbilityStepKind Kind => AbilityStepKind.Parallel;
        public string NodeGuid => _nodeGuid;
        public IReadOnlyList<IAbilityStepConfig> Children => _children;
        public ParallelJoinPolicy JoinPolicy => _joinPolicy;
        public bool CancelRemainingOnJoin => _cancelRemainingOnJoin;
        public int ChildCount => _children?.Count ?? 0;
        public IAbilityStepConfig GetChild(int index) => _children[index];
    }
}
