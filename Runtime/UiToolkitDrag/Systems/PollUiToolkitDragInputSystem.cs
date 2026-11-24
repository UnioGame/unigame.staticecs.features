namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;
    using UnityEngine.InputSystem;

    /// <summary>Samples the runtime mouse once for all drag systems in this update.</summary>
    public class PollUiToolkitDragInputSystem<TWorld> : ISystem
        where TWorld : struct, IWorldType
    {
        public void Update()
        {
            var input = World<TWorld>.GetResource<UiToolkitDragInputResource<TWorld>>();
            if (!input.RuntimePolling)
                return;

            var mouse = Mouse.current;
            input.Screen = mouse != null ? mouse.position.ReadValue() : default;
            input.IsPressed = mouse != null && mouse.leftButton.isPressed;
            input.Released = mouse != null && mouse.leftButton.wasReleasedThisFrame;
            input.Cancelled = Keyboard.current?.escapeKey.wasPressedThisFrame == true;
        }
    }
}
