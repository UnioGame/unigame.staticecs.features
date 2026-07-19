using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace UniGame.StaticEcs.Features {
    /// <summary>
    /// Composite step that runs its <see cref="Children"/> one after another. On a child Failed
    /// the sequence propagates Failed up the stack. The progression system advances cursor on
    /// child completion; composites never have an activator (handled inline in
    /// <c>AbilityStepProgressionSystem</c>).
    /// </summary>
    [Serializable]
    [MovedFrom(true, sourceNamespace: "unigame.staticecs.features", sourceAssembly: "unigame.staticecs.features")]
    public sealed class SequenceStepConfig : IAbilityStepConfig {
        [SerializeField] private string _nodeGuid;
        [SerializeReference] private List<IAbilityStepConfig> _children = new();

        public SequenceStepConfig() { }

        public SequenceStepConfig(IList<IAbilityStepConfig> children, string nodeGuid = null) {
            _nodeGuid = nodeGuid;
            if (children != null) {
                _children = new List<IAbilityStepConfig>(children);
            }
        }

        public AbilityStepKind Kind => AbilityStepKind.Sequence;
        public string NodeGuid => _nodeGuid;

        public IReadOnlyList<IAbilityStepConfig> Children => _children;

        public int ChildCount => _children?.Count ?? 0;

        public IAbilityStepConfig GetChild(int index) => _children[index];
    }
}
