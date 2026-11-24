namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    /// <summary>Compatibility name for the UI Toolkit pointer feature.</summary>
    public class HoveredFeature<TWorld> : UiToolkitPointerFeature<TWorld>
        where TWorld : struct, IWorldType
    {
    }
}
