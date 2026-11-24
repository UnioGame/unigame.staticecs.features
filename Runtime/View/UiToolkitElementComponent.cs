namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    /// <summary>Connects a UI Toolkit child entity to its live visual element.</summary>
    public struct UiToolkitElementComponent : IComponent
    {
        public EcsUiToolkitElement Element;
    }

    /// <summary>Marks the ECS entity of a specific UI Toolkit element type.</summary>
    public struct UiToolkitElementComponent<TElement> : IComponent
        where TElement : EcsUiToolkitElement
    {
    }
}
