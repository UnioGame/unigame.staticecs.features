namespace UniGame.StaticEcs.Features
{
    using UnityEngine.UIElements;

    /// <summary>Provides a visual copy without exposing the source element's layout.</summary>
    public interface IUiToolkitDragPreviewSource
    {
        VisualElement CreateDragPreview();
    }
}
