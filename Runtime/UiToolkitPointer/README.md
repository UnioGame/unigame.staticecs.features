# UI Toolkit Pointer Feature

## Capabilities

`UiToolkitPointerFeature<TWorld>` mirrors mouse hover state from ECS-backed UI
Toolkit elements into `IsHoveredTag` and publishes every mouse button down/up
transition as `UiToolkitPointerButtonEvent`. The event contains the element
`EntityGID`, `IsPressed`, pointer ID, button, and panel position. Other systems
can combine the hover tag or events with their own
element-specific components.

## Usage

Install the View Feature and `UiToolkitPointerFeature<TWorld>` in the same world.
The View Feature creates an entity for every `EcsUiToolkitElement`. The pointer
feature reads its current mouse hover state and queues button transitions from
UI Toolkit callbacks until the next ECS update. No per-view input wiring is
needed. `HoveredFeature<TWorld>` remains a compatibility name for the same
feature; compose only one of the two.

Button events are delivered to the innermost ECS-backed element in a nested
UI Toolkit hierarchy. A release is reported when UI Toolkit delivers the
pointer-up event to that element; this feature does not capture the pointer or
pair a release outside the element with an earlier press. A drag feature must
own that capture and cancellation policy separately. Only mouse input is
tracked; pointer movement does not produce events.

## Configuration

Both systems run at update order `10003`, after the default View Feature element
sync at `10001`. The tag reflects the latest pointer state at that update and
is removed when the pointer leaves or the element entity is destroyed. Button
transitions retain their order within the update and are discarded if the
element no longer has an entity in that world. Native events are
retained only for registered receivers.
