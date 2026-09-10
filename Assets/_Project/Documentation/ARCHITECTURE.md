# A Caldeira: Sobrevivencia de Ferro - Architecture Skeleton

> Documento da primeira etapa. A implementação e os limites atuais estão em [IMPLEMENTATION.md](IMPLEMENTATION.md).

Target: Unity 2022.3 LTS+, URP 2D, Input System, TextMeshPro and Newtonsoft JSON available.

## Assets tree

```text
Assets/
`-- _Project/
    |-- Animations/
    |-- Art/
    |   |-- Materials/
    |   |-- Shaders/
    |   |-- Sprites/
    |   |-- Tiles/
    |   `-- VFX/
    |-- Audio/
    |   |-- Mixers/
    |   |-- Music/
    |   `-- SFX/
    |-- Data/
    |   |-- Enemies/
    |   |-- Events/
    |   |-- Pooling/
    |   |-- Stages/
    |   |-- Upgrades/
    |   `-- Weapons/
    |-- Documentation/
    |-- Prefabs/
    |   |-- Collectibles/
    |   |-- Enemies/
    |   |-- Hazards/
    |   |-- Player/
    |   |-- Projectiles/
    |   `-- Weapons/
    |-- Scenes/
    |   |-- 00_Bootstrap.unity
    |   |-- 01_MainMenu.unity
    |   |-- 10_PatioTriagem.unity
    |   `-- 11_LinhasMontagem.unity
    |-- Scripts/
    |   |-- Audio/
    |   |-- Collectibles/
    |   |-- Combat/
    |   |-- Core/
    |   |-- Data/
    |   |-- Enemies/
    |   |-- Events/
    |   |-- Input/
    |   |-- Persistence/
    |   |-- Player/
    |   |-- Pooling/
    |   |-- Progression/
    |   |-- Simulation/
    |   |-- UI/
    |   `-- World/
    |-- Settings/
    |   |-- Input/
    |   `-- Rendering/
    `-- Tests/
        |-- EditMode/
        `-- PlayMode/
```

Keep third-party content outside `_Project` (for example, `Assets/ThirdParty`).

Dependencies flow inward: presentation/input -> gameplay services -> immutable data and event contracts. `Data`, `Events` and `Pooling` do not depend on UI. Scene behaviours issue commands to services; they do not own global state.

## Scene and lifetime model

- `00_Bootstrap` is first in Build Settings and owns one persistent root containing `GameManager`, `PoolManager`, `WaveSpawner`, `WeaponManager` and `SettingsManager`.
- On startup, `GameManager` loads `01_MainMenu` while its root survives the single-scene transition.
- `RuntimeServicesSO` bridges scene boundaries: Bootstrap binds the live services once and scene UI reads them without `Find`, statics, or cross-scene Inspector references.
- Pool banks are children of that root. Every pooled object is placed in the Bootstrap scene, assigned to exactly one `PoolRegistration`, and begins inactive after `PoolManager.Initialize`.
- `01_MainMenu` and gameplay stages contain presentation, stage geometry, camera and hazards, but not duplicate services.
- Stage content is selected through `StageSO`; runtime objects retain references to immutable ScriptableObject definitions.

## Strict pooling contract

`GenericObjectPool<T>` has fixed capacity. It never grows and never calls `Instantiate` or `Destroy`. Exhaustion returns `false`; tune capacities from profiler telemetry. Instances must not disable themselves directly: they return through `PoolManager.Despawn`.

Suggested initial budgets for a stress-test scene:

| Pool | Capacity |
|---|---:|
| Light enemies | 1,200 |
| Heavy enemies | 160 |
| Projectiles | 2,048 |
| Collectibles | 1,024 |
| Hazards/VFX | 256 |

## Event-driven relationships

```text
MainMenuManager
    | RuntimeServicesSO -> direct command: StartRun(StageSO)
    v
GameManager ---------------- GameStateEventChannelSO ----------------> HUD / Pause UI
    |                         (Loading/Playing/Paused/GameOver)
    | Begin/Stop                         ^
    +----------------> WaveSpawner       |
    |                       |             |
    | BeginRun              | TrySpawn   |
    +----------------> WeaponManager     |
                            | TrySpawn   |
                            v            |
                        PoolManager      |
                            ^            |
Enemy / Projectile / Collectible --------+-- return commands

Collectible pickup ---- ExperienceEventChannelSO ----> Progression service (future)
                                                    `-> HUD XP bar
Enemy defeated ------- Void/typed channels (future) --> Audio, VFX, quests
SettingsManager ------ C# Changed event --------------> Settings UI
```

Direct calls are reserved for commands with one clear owner. Broadcast facts use event channels. UI never polls combat objects.

## Frame-time rules

- Allocate arrays and dictionaries only during Bootstrap, loading, or `BeginRun`.
- No LINQ, iterator creation, boxing, string formatting, scene lookup, or collection growth in gameplay loops.
- Cache component references during authoring/Awake; never use `GameObject.Find` and never call `GetComponent` in `Update`.
- Use non-alloc physics queries and fixed buffers when collision/combat is implemented.
- `WaveSpawner` is already a central scheduler. For the production 1,000+ actor pass, add central simulation systems backed by flat arrays/NativeArrays, spatial hashing, batched animation, and Jobs/Burst where profiling justifies it; avoid one `Update` per enemy/projectile.
- Run the Profiler with `GC.Alloc` recording and a repeatable 1,000-enemy stress scene before content production.

## Authoring order

1. Create PoolKey assets for every enemy, projectile, collectible and hazard family.
2. Create Weapon, Enemy, Upgrade and Stage assets.
3. Place the fixed pool banks under the Bootstrap root and assign registrations.
4. Wire the manager references and event-channel assets in the Inspector.
5. Add scenes to Build Settings in the documented order.
