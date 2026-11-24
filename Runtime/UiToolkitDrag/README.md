# UI Toolkit Drag Feature

## Capabilities

`UiToolkitDragFeature<TWorld>` observes left-mouse presses on element entities with
`DragableComponent`, promotes the press after eight screen pixels, and renders a
temporary copy next to the pointer. Releasing over another live
`DragSlotComponent` publishes `UiToolkitDragDropEvent` when the item's mask
overlaps the target mask. An occupied slot is accepted only when its item also
matches the source slot. The feature does not move gameplay entities.
Masks are local UI rules and are not serialized into transfer commands.
While an item is being dragged, only the slot beneath the cursor receives
drop feedback: green for an accepted move or swap, red for a rejected target.
The feedback clears on release, cancellation, or pointer exit.
The feature sets `drag-slot--drop-allowed` / `drag-slot--drop-blocked` on the
slot only. The consuming View stylesheet supplies the colors.

## Usage

Add `DragableElementConverter` to an `EcsUiToolkitElement` and
`DragSlotElementConverter` to a slot element. Add
`DragSlotMembershipElementConverter` to the source element and set its `slot`
reference to that slot before registration. The slot is registered first and
the converter writes `DragSlotMembershipComponent` to the source entity.
The drag systems read this ECS relation and ECS occupant relations; a visual
wrapper between slot and item does not change the drop rules. The slot may be
any registered `EcsUiToolkitElement` with `DragSlotComponent`. A domain View
feature receives `UiToolkitDragDropEvent` and decides which gameplay command
to send. `DragBlockedTag` on a slot prevents new drags and drops while an
application operation is pending. `IsDragStateTag` exists only after the
distance threshold and is removed on release or cancellation.
When a domain View moves an existing element between slots, it updates the
element's `DragSlotMembershipComponent`; removing and registering a new element
applies the converter again.
The optional `IUiToolkitDragPreviewSource` contract lets a source element
build its visual copy. Sources without it use a neutral drag rectangle.
The copy switches to the highest registered View panel under the pointer,
including the background and header between drop slots.

## Configuration

Install `UiToolkitPointerFeature<TWorld>` and `ViewFeature<TWorld>` before this
feature. Drag systems run in order 10004–10010, after pointer event publication
at 10003. The runtime mouse comes from Unity Input System. In isolated tests,
set `UiToolkitDragInputResource<TWorld>.RuntimePolling` to false and supply the
pointer state directly.
Target resolution runs at 10007 and writes `DragTargetComponent`. Completion,
preview rendering, and feedback read that result in subsequent systems.
