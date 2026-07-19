using UnityEditor.Experimental.GraphView;

namespace UniGame.StaticEcs.Features.Editor.AbilityGraph {
    internal sealed class AbilityGraphContextCommandContext {
        public AbilityAsset Asset;
        public AbilityGraphCanvasView CanvasView;
        public AbilityGraphNodeView NodeView;
        public Edge EdgeView;
        public AbilityGraphProjection.Node Node;
        public AbilityGraphProjection.Edge? Edge;
        public AbilityGraphContextTarget Target;

        public IAbilityStepConfig NodeConfig => Node?.Config;
    }
}