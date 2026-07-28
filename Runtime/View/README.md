# View Feature

## Capabilities

The View Feature connects Static ECS entities to the shared View System runtime. It
supports layout-based and embedded views, typed model synchronization, owner lifetime,
view hierarchy metadata, visibility transitions, filtering, and FIFO request queues.

View System remains responsible for creating views and view models. ECS stores only
backend-free lifecycle data, typed model components, and native entity relations.

## Usage

Create a `ViewRequest`, select placement with `ViewPlacement`, and submit it through
`ViewOperations`. Use `TryOpenOn` when the view belongs on an existing gameplay entity,
or `TryOpenNew` when it needs a dedicated presentation entity. Presentation systems
query `ViewModelComponent<TModel>` directly and resolve gameplay data through
`Link<ViewSource>`.

Embedded entity views use `EcsEntityViewConverter` from the provider's
serializable converter list. The converter binds the existing View System view
without storing per-entity runtime state.

## Configuration

Create a game-owned `ViewFeatureAsset`, assign the same `ViewSystemSettings` used by the
game View System source, and place the asset before gameplay UI features in the Static
ECS source. The asset maintains a flattened model catalog so closed generic model
components are registered before world initialization.
