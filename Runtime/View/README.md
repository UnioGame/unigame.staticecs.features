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
- generic worlds and adjacent Main-world operations.

ECS components never store `GameObject`, `Transform`, or `IView`. Unity and View
System handles remain inside the lifecycle system.

## Usage

### Runtime flow

1. A gameplay system sends a request through `ViewOperations`.
2. The request is consumed as a native one-frame event.
3. View System creates the view and its model, or binds an existing view.
4. The View Feature attaches lifecycle data and the typed model to an ECS entity.
5. Presentation systems copy gameplay data into public reactive model fields.
6. The view renders those fields and writes user intent into typed model signals.

`TryOpenNew`, `TryOpenOn`, and `TryBindExistingOn` confirm that the request event
was sent. They do not mean that asynchronous creation has completed. Observe
`ViewOpenedEvent`, `ViewOpenFailedEvent`, or `ViewComponent.Status` when the
completion result matters.

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

An empty `ViewCloseFilter` matches nothing.

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
