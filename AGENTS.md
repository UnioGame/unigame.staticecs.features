# AGENTS — unigame.staticecs.features

Use the repo-local `$build-static-ecs-features` skill when implementing, migrating, or reviewing a gameplay feature family.

## Layer and assemblies

- Gameplay families depend on `unigame.staticecs` and `unigame.staticecs.unity`; neither lower package may depend back on a feature family.
- Every independently enabled family owns one runtime asmdef. Editor code uses a family-local Editor-only asmdef; tests use family-local test asmdefs where practical.
- Cross-family dependencies must be explicit asmdef references and must not create cycles.

## Family structure

Use only directories that contain real files:

```text
<FeatureName>/
  Components/
  Events/
  Systems/
  Operations/
  Conversion/
  Authoring/
  Editor/
  Tests/Editor/
  <FeatureName>Feature.cs
  <FeatureName>Feature.Main.cs
  <FeatureName>FeatureAsset.cs
  unigame.staticecs.features.<feature>.asmdef
  README.md
```

- Omit empty categories. Use domain names such as `Registries`, `Filters`, or `Services` instead of a catch-all `Runtime` directory.
- A family may contain variants/subfeatures only when they share the same registered type set. Independently disabled blocks get their own asmdef and feature asset.
- Preserve namespaces, public names, and `.meta` GUIDs when reorganizing files.

## Features and systems

- A feature owns its resources, registries, and systems. Ordinary concrete ECS marker
  types are discovered automatically from the enabled asset assembly and, for generic
  adapters, from the programmatic feature inheritance assemblies. A variant in a
  separate asmdef must inherit the shared programmatic feature instead of manually
  registering its ordinary marker types.
- Every public generic-on-world class has a neighboring Main-default form, except an
  exact-type `IResource`: subclass aliases create a different Static ECS resource key.
  Exact-type resources use the closed `<Main>` type or a factory/operation returning it.
- Feature initialization is UniTask-based and may overlap with other features by default.
  Do not depend on completion order unless the world config explicitly selects sequential
  initialization. Do not serialize system lists.
- Initialization-owned subscriptions and disposables use the `ILifeTime` supplied to
  `InitializeAsync`; nested features receive the same instance. Runtime ownership stays in systems and is
  released through `ISystem.Destroy`; clearing an ECS Resource does not dispose it.
- Production systems are public and each lives in its own file under the owning
  feature's `Systems/` directory. Do not declare systems inside feature, asset,
  authoring, or configuration files.
- Struct and class systems are both supported. Never add reference-type constraints and never implement unused `ISystem` methods.
- Use native events for transient streams; retain components for persistent/queryable workflow state. Receiver cleanup belongs in system `Destroy`.
- Prefer family-local serializable converters. Use Mono converters only for behavior that genuinely needs a MonoBehaviour, and use converter preset assets for repeated or configuration-created entity recipes.

## ECS naming and formatting

- `IComponent` and `IMultiComponent` types end with `Component`; `ITag` types end with `Tag`; `IEvent` types end with `Event`; `ISystem` types end with `System`.
- Prefer the shortest unambiguous domain name. Do not retain technical filler such as `Entry`, `Data`, `Runtime`, `Binding`, or `Ref` when the ECS suffix and the remaining domain role already communicate the meaning.
- Keep qualifiers such as `Config`, `Source`, `Target`, `Owner`, `Parent`, `Channel`, `Tracker`, `Mask`, `Destination`, `Path`, `Agent`, and `Obstacle` only when they distinguish a real domain role or lifecycle.
- Public ECS component and event fields use PascalCase. Inspector and authoring fields use `public camelCase`; private runtime fields use `_camelCase`.
- Use Allman braces, four spaces, block-scoped namespaces, and `using` directives inside the namespace with `System` imports first.
- Empty tags use a normal multi-line declaration. Attributes stay above their declaration.
- At feature registration boundaries, write every type, resource, system, handler, and registry registration as a separate statement. Fluent registration chains are forbidden. Query construction follows its own formatting rules.
- Construct every resource in a named local before publishing it with `SetResource`.
  Constructors, object initializers, conditional expressions, and factory calls do not
  belong inside the registration call.
- Do not manually register ordinary concrete components, tags, events, links, multi markers, or entity types in feature code. Required closed generic constructions belong to one assembly-level registrar per world.
- Standard gameplay assets use `StaticEcsFeatureAsset<TWorld, TFeature>` or
  `StaticEcsMainFeatureAsset<TFeature>`. They serialize one parameterless C#
  feature and contain no forwarding lifecycle code.
- A standalone asset may derive from `StaticEcsFeatureAsset<TWorld>` when the
  ScriptableObject itself is the complete feature implementation.
- Features wait for asynchronous dependencies directly through
  `Resource<T>.GetAsync(lifeTime)`. Do not introduce dependency declarations,
  installation contexts, or validation wrappers.
- Package asmdef names are lowercase, dot-separated, start with `unigame.staticecs.features`, and match their filenames.

## Documentation

- Each feature family has an English README with Capabilities / Usage / Configuration.
- Public docs describe the current package without migration history and link to shared/upstream Static ECS documentation rather than restating it.
- Every public type and member has at least a one-line XML summary.

## Tests

- Feature tests use a dedicated isolated `IWorldType` by default. Install resources and
  register types explicitly, then initialize only the systems required by the scenario.
- Do not route a feature test through `EcsService` unless the startup pipeline itself is
  under test.
- Prefer `StaticEcsTestWorld<TWorld>` for deterministic world cleanup. The helper owns
  lifecycle only; the test remains responsible for visible feature/resource composition.
- Do not encode production inventories or expected alias lists in fixtures. Derive
  architecture assertions from compilations, source declarations, asmdefs, manifests, or
  runtime discovery.
