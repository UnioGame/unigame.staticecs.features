# View Feature

## Capabilities

The View Feature connects Static ECS entities to the shared View System runtime.
View System creates and owns views and view models; ECS owns requests, lifecycle
state, gameplay relations, and model synchronization.

The feature supports:

- layout-based creation through View System;
- binding a view already present on an entity prefab;
- dedicated presentation entities or a view attached to an existing entity;
- typed `ViewModelComponent<TModel>` queries;
- source, owner, and visual parent relationships;
- show, hide, close, filtered close, and owner-driven lifetime;
- named containers with capacity and optional busy-container reuse;
- FIFO request queues;
- lifecycle status and completion events;
- one ECS entity per `EcsUiToolkitElement` in a UI Toolkit view;
- generic worlds and adjacent Main-world operations.

`UiToolkitElementRegistryResource<TWorld>.TryGetView` provides read-only access
to a live UI Toolkit view by its `ViewKey` for presentation adapters.

ECS components never store `GameObject`, `Transform`, or `IView`. The
`UiToolkitElementComponent` stores the live child element as a presentation
reference; View System handles remain inside the View Feature runtime.

## Usage

### Runtime flow

1. A gameplay system sends a request through `ViewOperations`.
2. The request is consumed as a native one-frame event.
3. View System creates the view and its model, or binds an existing view.
4. The View Feature attaches lifecycle data and the typed model to an ECS entity.
5. Presentation systems copy gameplay data into public reactive model fields.
6. The view renders those fields and writes user intent into typed model signals.

### ECS-backed UI Toolkit elements

Derive a custom `[UxmlElement]` from `EcsUiToolkitElement` and place it inside
the UXML tree of an active `UiToolkitView`. Once the view root is available, the
View Feature creates a child entity with `UiToolkitElementComponent` and
`Link<UiToolkitElementParent>`. Deriving from
`EcsUiToolkitElement<TElement>` also adds a queryable
`UiToolkitElementComponent<TElement>` marker. Register each closed marker type
before world initialization in the owning feature's type registrar. Optional
`UiToolkitElementConverter` UXML objects can configure the new entity and
release converter-owned state when it is destroyed. The view entity exposes reverse
`Links<UiToolkitChildElements>`. The element's `Entity` is its stable
`EntityGID` while registered. Systems can query the component, use the visual
element for presentation, and attach their own ECS components to the entity.
Ordinary `VisualElement` descendants do not create entities.

The View Feature rebuilds the element entities when the view root is replaced
and deletes them when the view is released. An element added to an active panel
requests registration when it attaches. After adding an element to a view root
that is not attached to a panel, call `RequestEntityRegistration()` on it.
Removing an element from the view root or disposing it releases its entity on
the next ECS update. Reparenting inside the same view keeps the entity; moving to
another view transfers registration to that view.

`TryOpenNew`, `TryOpenOn`, and `TryBindExistingOn` confirm that the request event
was sent. They do not mean that asynchronous creation has completed. Observe
`ViewOpenedEvent`, `ViewOpenFailedEvent`, or `ViewComponent.Status` when the
completion result matters.

### Open and close by view class

Use the view class when the caller does not need a catalog ID or a compile-time
model type. View System resolves the prefab and model from its registered view
references, including entries whose `ViewName` is an alias:

```csharp
var request = ViewRequest.For<InventoryView>();
request.Owner = player.GID;
request.Source = player.GID;
request.Skin = "Default";

ViewOperations.TryOpenNewByType<Main, InventoryView>(request, out var key);
ViewOperations.CloseAllByType<Main, InventoryView>();
```

For a visible view that only needs placement, pass `ViewPlacement` directly:

```csharp
ViewOperations.TryOpenNewByType<Main, InventoryView>(
    ViewPlacement.InLayout(ViewType.Screen), out _);
ViewOperations.TryOpenOnByType<Main, InventoryView>(
    existingEntity, ViewPlacement.InLayout(ViewType.Overlay), out _);
```

`For<TView>()` uses the class name as `ViewId`, defaults to a visible Window,
and checks the created view's exact type. `TryOpenNewByType<TWorld, TView>` and
`TryOpenOnByType<TWorld, TView>` also have overloads without request options or
with placement alone. Placement can also select a registered container or a
valid parent. Use a full request when skin, owner, source, queue, order, or
visibility must be specified.
`Order` controls presentation sorting rather than the Transform sibling index.
Leave it unset to use the View System stack position. For built-in Screen,
Window, and Overlay layouts, an explicit value from 0 to 99 is an offset within
that layout's reserved range: Window `Order = 10` produces sorting order 210.
For custom layouts and parent or container placement, the value is used directly.
Setting `Order = 0` explicitly is different from leaving it unset.
Absolute values must fit Unity's sorting order range (-32768 to 32767). An
explicit order requires a root `PanelRenderer` or `Canvas`; otherwise opening
fails with `ViewOpenFailedEvent`.
When options are supplied, their `ShowOnOpen` value is retained; create them
with `For<TView>()` for the visible default. The type operation replaces an
options `ViewId` with the class name and leaves placement, skin, owner,
source, queue, and order intact. Use a typed request with an explicit `ViewId`
when a specific catalog alias is required.

Requests created manually also default to the `Window` layout when `Placement`
is unset or selects a layout with no name. Opening operations normalize the
request before sending its event, and the lifecycle normalizes directly sent
events when accepting them. Explicit layouts, containers, and parents are
preserved. `ShowOnOpen` remains independent: a manually constructed request
must set it to `true` to show the view immediately.

`CloseAllByType` closes every exact-type instance in the selected world,
including pending queue items and instances opened through an alias. Use
`Close(key)` to address only one instance. Both close operations return whether
their event was sent, not a count of matching views.

### Open a screen from a typed request component

Register one screen system for each request component in its owning feature.
The generic arguments keep the request component, view, and expected model
together. The request component is consumed after the open event is sent; its
entity becomes the default owner and source. Async creation failures are
reported through `ViewOpenFailedEvent`.

```csharp
public struct OpenInventoryRequestComponent : IComponent
{
}

var request = ViewRequest.For<InventoryView, InventoryViewModel>();
request.Skin = "Default";
request.Placement = ViewPlacement.InLayout(ViewType.Window);

World<Main>.Systems<StaticEcsUpdateSystems>.Add(
    new ShowScreenSystem<
        OpenInventoryRequestComponent,
        InventoryView,
        InventoryViewModel>(request),
    openInventoryOrder);
```

Add `OpenInventoryRequestComponent` to an entity to request the screen. The
system uses `nameof(InventoryView)` as the default View System ID; pass an ID to
`ViewRequest.For<InventoryView, InventoryViewModel>("inventory-screen")` when
the content catalog uses an alias. The View System settings must map that ID to
the requested view and `InventoryViewModel`, and the model must be present in the
View Feature model catalog. A mismatch fails the view lifecycle with a diagnostic.
The system uses `TryOpenNew`, so multiple view requests can be owned by the same
source.

### Open a registered View by type

Use the View and ViewModel types when both are known to the caller:

```csharp
var request = ViewRequest.For<InventoryView, InventoryViewModel>();
request.Owner = player.GID;
request.Source = player.GID;
request.Placement = ViewPlacement.InLayout(ViewType.Window);
ViewOperations.TryOpenNew(request, out var viewKey);
```

The View System catalog indexes registered views by C# type name as well as by
configured alias. Keep type names unique within a catalog when opening by type.

### Define a view and its model

Keep a model next to its view unless multiple views reuse it. The model is a
simple View System model with public reactive fields. Inspector configuration is
also public.

```csharp
namespace Game.Ecs.Views.Inventory
{
    using System;
    using Cysharp.Threading.Tasks;
    using R3;
    using TMPro;
    using UniGame.Runtime.Common;
    using UniGame.Runtime.Rx;
    using UniGame.StaticEcs.Features;
    using UniGame.UiSystem.Runtime;
    using UniGame.ViewSystem.Runtime.Extensions;
    using UnityEngine.UI;
    using static UniGame.Runtime.Rx.Runtime.Extensions.ReactiveBindingExtensions;

    public class InventoryView : View<InventoryViewModel>
    {
        public TMP_Text titleLabel;
        public Button selectButton;

        protected override UniTask OnInitialize(InventoryViewModel model)
        {
            this.Bind(model.Title, titleLabel);
            this.Bind(selectButton, model.ActivateSlot);
            return UniTask.CompletedTask;
        }
    }

    [Serializable]
    public class InventoryViewModel : ViewModel
    {
        public ReactiveValue<string> Title = new();
        public ReactiveValue<int> SelectedSlot = new();
        public SignalValueProperty<bool> ActivateSlot = new();
    }
}
```

Do not query ECS from the view or model. An ECS system updates the model, and the
view only renders it.

Use the component-aware `Bind` overloads before writing a delegate. When custom
rendering is required, pass an existing method group and keep the binding
expression on one line:

```csharp
this.Bind(model.HealthRatio, RenderHealth);
this.Bind(model.CastDescription, RenderCast);
```

`Bind` owns the subscription lifetime. Do not expose `Subscribe`,
`OnClickAsObservable`, or listener cleanup in application View code. Add a
focused View System binding extension when a recurring UI component and model
field pair is not supported yet. Do not wrap an existing render method in a
sender-aware lambda. Reserve static state callbacks for binding infrastructure
and use component `SetValue` extensions for direct UI updates.

### Open a dedicated presentation entity

Use `TryOpenNew` for HUDs, dialogs, windows, notifications, and multiple views
that visualize the same gameplay source.

```csharp
var request = new ViewRequest
{
    ViewId = nameof(InventoryView),
    Owner = player,
    Source = player,
    Placement = ViewPlacement.InLayout(ViewType.Overlay),
    ShowOnOpen = true
};

var requestSent =
    ViewOperations.TryOpenNew(request, out var inventoryKey);
```

The lifecycle system creates a dedicated ECS view entity containing:

```text
ViewComponent
ViewModelComponent<InventoryViewModel>
Link<ViewSource> -> player
```

`Owner` and `Source` are independent:

- `Owner` controls lifetime. The view closes when a non-empty owner disappears.
- `Source` identifies the gameplay entity read by synchronization systems.

They commonly reference the same entity, but a squad window can be owned by the
player while reading a selected squad entity.

### Open on an existing ECS entity

Use `TryOpenOn` when the existing entity itself should become the view entity.

```csharp
var request = new ViewRequest
{
    ViewId = nameof(UnitDetailsView),
    Owner = unit.GID,
    Source = unit.GID,
    Placement = ViewPlacement.InLayout(ViewType.Overlay),
    ShowOnOpen = true
};

ViewOperations.TryOpenOn(unit, request, out var detailsKey);
```

`ViewComponent`, `ViewModelComponent<UnitDetailsViewModel>`, and
`Link<ViewSource>` are added to `unit`. `TryOpenOn` rejects an entity that
already has `ViewComponent`. Use separate `TryOpenNew` requests when one source
needs multiple views.

### Synchronize a typed model

View System creates the model. The View Feature discovers its concrete type and
attaches the matching typed component after creation.

```csharp
public class SynchronizeInventorySystem : ISystem
{
    public void Update()
    {
        var filter = default(All<ViewModelComponent<InventoryViewModel>,
            World<Main>.Link<ViewSource>>);

        foreach (var viewEntity in World<Main>.Query(filter).Entities())
        {
            var model =
                viewEntity.Read<ViewModelComponent<InventoryViewModel>>().Model;
            var source =
                viewEntity.Read<World<Main>.Link<ViewSource>>().Value;

            if (model == null || !source.TryUnpack<Main>(out var sourceEntity))
            {
                Reset(model);
                continue;
            }

            var inventory = sourceEntity.Read<InventoryComponent>();
            model.Title.Value = inventory.DisplayName;
        }
    }

    private static void Reset(InventoryViewModel model)
    {
        if (model != null)
            model.Title.Value = string.Empty;
    }
}
```

Reset presentation fields when the source is missing. A stale source should not
leave old values visible.

### Send user intent back to ECS

Use `SignalValueProperty<T>` for input consumed during the next ECS update.

```csharp
this.Bind(selectButton, model.ActivateSlot);
```

Consume the signal from a dedicated gameplay-facing system:

```csharp
foreach (var viewEntity in
         World<Main>.Query<All<ViewModelComponent<InventoryViewModel>>>().Entities())
{
    var model =
        viewEntity.Read<ViewModelComponent<InventoryViewModel>>().Model;

    if (model != null && model.ActivateSlot.Take(out _))
        InventoryOperations.Select(model.SelectedSlot.Value);
}
```

Repeated writes to one `SignalValueProperty<T>` are last-write-wins until
`Take` consumes it. Separate signals are independent and can both be consumed
during the same ECS update. Use a native event or a dedicated async feature
when every write to one signal must be retained.

### Bind an embedded entity view

World-space bars and other entity-owned visuals already exist on their prefab.
Add `EcsEntityViewConverter` to the entity provider's serializable converter
list:

```csharp
new EcsEntityViewConverter
{
    view = healthBarView,
    viewId = nameof(HealthBarView)
}
```

The converter submits a request with `ViewPlacement.Existing()`, using the
provider entity as both owner and source. It binds the existing view, attaches
the typed model to the same entity, and sends a close request from the provider
destroy hook. The converter stores no per-entity runtime key.

Use `EcsEntityViewConverter<TWorld>` for a custom world and
`EcsEntityViewConverter` for Main.

### Build a view hierarchy

Place a child below another managed view:

```csharp
var request = new ViewRequest
{
    ViewId = nameof(ItemTooltipView),
    Owner = inventoryOwner,
    Source = item,
    Placement = ViewPlacement.UnderView(inventoryKey),
    ShowOnOpen = true
};

ViewOperations.TryOpenNew(request, out var tooltipKey);
```

The child entity receives `Link<ViewParent>` to the parent view entity. Reverse
lookup is available through `ViewChildren`.

Place a view below an entity:

```csharp
Placement = ViewPlacement.UnderEntity(character.GID);
```

This creates `Link<ViewParent>` to the entity. Set that entity as `Owner` as
well when it should control the child view lifetime.

`ViewPlacement.UnderTransform` supplies a Unity parent but does not create an
ECS parent relation.

### Select placement

Use a built-in View System layout:

```csharp
Placement = ViewPlacement.InLayout(ViewType.Overlay);
```

Use a custom layout:

```csharp
Placement = ViewPlacement.InLayout("InventoryOverlay");
```

Apply optional transform overrides through the nested placement:

```csharp
Placement = new ViewPlacement
{
    Mode = ViewPlacementMode.ParentEntity,
    ParentEntity = character.GID,
    ApplyPosition = true,
    Position = new Vector3(0f, 1.5f, 0f),
    ApplyScale = true,
    Scale = Vector3.one * 0.01f,
    Space = ViewPlacementSpace.Local
};
```

### Use named containers and FIFO queues

Register a named container before opening views into it:

```csharp
ViewContainerOperations.Register(
    "Notifications",
    notificationRoot,
    capacity: 3);
```

```csharp
var request = new ViewRequest
{
    ViewId = nameof(NotificationView),
    Placement = ViewPlacement.InContainer("Notifications"),
    ShowOnOpen = true
};
```

Pass `useBusyContainer: true` only when reusing a full container is intentional.

Requests with the same non-empty queue name open sequentially:

```csharp
var request = new ViewRequest
{
    ViewId = nameof(DialogView),
    Queue = "StoryDialogs",
    Placement = ViewPlacement.InLayout(ViewType.Overlay),
    ShowOnOpen = true
};
```

The next request starts after the active request closes or fails.

### Control lifecycle

All operations send native one-frame events:

```csharp
ViewOperations.Show(viewKey);
ViewOperations.Hide(viewKey);
ViewOperations.Close(viewKey);
ViewOperations.CloseOwned(owner);
```

An initialized `IView` also exposes `Close()` and `CloseAsync()`. The standard
`ViewModel.Close()` emits `CloseCommand`, which `View<TModel>` binds to the
view's `Close()` method, including for UI Toolkit views. A presentation system
with an attached `ViewModelComponent<TModel>` may call `model.Close()` to
close that instance without its key. ECS lifecycle observes the view closing
and sends `ViewClosedEvent`. Use `ViewOperations.Close(key)` for queued or
still-opening requests, before a bound model exists. Calling `model.Close()`
does not emit an application-specific input signal such as `CloseRequested`.

Close matching views:

```csharp
ViewOperations.CloseAll(new ViewCloseFilter
{
    ViewId = nameof(NotificationView),
    Layout = ViewType.Overlay.ToString()
});
```

Close every view explicitly:

```csharp
ViewOperations.CloseAll(new ViewCloseFilter
{
    All = true
});
```

`CloseAll<TWorld>(filter)` only examines views managed by that ECS world. It
also covers accepted requests that are queued or still opening. Filter fields
compare against the original opening request, except `ViewType`, which uses
the created view's exact assembly-qualified class name. Before creation it
uses the request's expected type, or resolves the registered type using the
request's view ID and skin.

| Field | Match |
| --- | --- |
| `Owner` | Exact `request.Owner`; equivalent to `CloseOwned<TWorld>(owner)` when used alone. |
| `Source` | Exact `request.Source`, independent of `Owner`. |
| `ViewId` | Exact, case-sensitive `request.ViewId`; an alias differs from the class-name ID. |
| `ViewType` | Exact class, including assembly; use `CloseAllByType<TWorld, TView>()` to avoid constructing the string. |
| `Layout` | Exact, case-sensitive `request.Placement.Layout`; container and parent placement factories leave it empty. |
| `All` | `true` matches every view in the selected world and ignores other fields. |

Specified fields are combined with AND. Default `EntityGID` and null or empty
string fields do not restrict the selection; an empty filter matches nothing.
This API cannot select views with no owner or no source by setting those fields
to default. There is no skin or queue field in `ViewCloseFilter`. Use `Close(key)`
for one instance, especially when several instances share the same type and
owner. Group-close methods report event submission, not a match count;
observe `ViewClosedEvent` for actual completion.

Systems may receive:

- `ViewOpenedEvent`;
- `ViewStatusChangedEvent`;
- `ViewClosedEvent`;
- `ViewOpenFailedEvent`.

`ViewLifecycleStatus` exposes `Queued`, `Opening`, visibility transitions,
`Closing`, `Closed`, and `Failed`.

### Project examples

- [Ability HUD](../../../../Assets/Game.ECS.Views/AbilityHud/README.md) opens a
  dedicated player-owned view, synchronizes a typed model, and consumes
  independent one-update slot signals.
- [Health Bar](../../../../Assets/Game.ECS.Views/HealthBar/README.md) binds an
  embedded world-space view through `EcsEntityViewConverter` and reads health
  and mana through `Link<ViewSource>`.

## Configuration

### Install the runtime

1. Publish the game View System through the same startup context used by Static
   ECS.
2. Create a game-owned `ViewFeatureAsset`.
3. Assign the game's `ViewSystemSettings` to `viewSystemSettings`.
4. Place `ViewFeatureAsset` before gameplay presentation features in the Static
   ECS source.
5. Keep the Update group enabled for worlds that install the View Feature.

The asset waits for `IGameViewSystem` through `StaticEcsContext`, creates the
model binders, and installs one late lifecycle system.

### Register a new view and model

For every dynamically created or embedded view:

1. Create the view prefab and populate its public Inspector fields.
2. Register the view ID, concrete view type, and model type in the game
   `ViewSystemSettings`.
3. Register dynamically loaded prefabs according to the View System content
   source, such as Addressables.
4. Refresh or validate `ViewFeatureAsset` so the model appears in
   `viewModelTypes`.
5. Add the gameplay synchronization and command systems with explicit orders.

`viewModelTypes` is a flattened pre-initialization catalog. It registers every
closed `ViewModelComponent<TModel>` before the world is initialized. A model
missing from this catalog cannot be attached as a typed ECS component.

### Organize game presentation code

Keep each independently composed game view block in its own assembly:

```text
Game.ECS.Views/
  Inventory/
    Data/
      InventoryViewConfig.cs
    Systems/
      OpenInventorySystem.cs
      SynchronizeInventorySystem.cs
      ProcessInventoryCommandsSystem.cs
    Views/
      InventoryView.cs
    InventoryViewFeature.cs
    game.ecs.views.inventory.runtime.asmdef
    README.md
```

The View and ViewModel stay in the same file by default. Passive configuration
and DTOs belong in `Data/`. Gameplay and business assemblies must not depend on
the presentation assembly.
