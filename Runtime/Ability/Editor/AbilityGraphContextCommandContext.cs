namespace UniGame.StaticEcs.Features.Editor.AbilityGraph
{
    using UnityEditor.Experimental.GraphView;

    internal sealed class AbilityGraphContextCommandContext
    {
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
