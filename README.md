# Homework 4 — Shadow Switch

**Unity 6.3.25f1 | 2D core game + 3D physics bonus | C#**

## Overview

Shadow Switch is a physics-based platform puzzle. The player must collect a crystal, operate a lamp, correctly aim its beam toward a designated obelisk, create a temporary solid shadow bridge, and cross the gap before the timer expires. Falling into the pit or running out of time causes a game over.

The **Physics Bonus (3D)** is a separate, small physics challenge that demonstrates Unity's 3D `CharacterController`: move around the arena, push a Rigidbody crate onto a pressure switch, then reach the exit portal.

> **Scope:** This README documents the Shadow Switch project only. If the assignment requires N+1 *original game ideas*, document and submit the other required projects separately as well.

## How to run

1. Open the Unity project folder in **Unity 6000.3.25f1** (or a compatible Unity 6 editor).
2. Open `Assets/Scenes/MainMenu.unity`.
3. Press **Play** in the **Game** window (not Device Simulator for keyboard testing).
4. Choose **PLAY LEVEL 2** or **PHYSICS BONUS (3D)**.

**Shadow Switch controls:** A/D or arrow keys = move; Space = jump; E = lamp on/off; J/L or mouse = aim; R = restart; Escape = main menu.

**Physics Bonus 3D controls:** W/A/S/D = move; Space = jump; E = push the crate by raycast; R = restart; Escape = main menu.

The repository contains five scenes: `MainMenu`, `Level02`, `Victory`, `GameOver`, `PhysicsBonus3D`.

## Core game loop and win/loss conditions

1. In `Level02`, `LevelController` starts a new attempt through `GameSession.BeginLevel()`.
2. The player moves with `Rigidbody2D`. Jumping uses an impulse and ground checks use 2D raycasts.
3. Touching `CrystalPickup` raises `GameEvents.CrystalCollected(energyReward)`. Both `GameSession` (crystal count) and `LampController` (recharge) react to the same event.
4. The player lights and aims the lamp. `LampController` notifies `ShadowBridge` when its conditions change. The bridge collider is enabled only when the beam is correctly aligned and the lamp has energy.
5. Touching the exit raises `GameEvents.GoalReached`. `GameSession` checks the required crystal count and loads `Victory` only if the requirement is met.
6. Falling into the pit or reaching the time limit raises `GameEvents.PlayerDied`, leading to `GameOver`.

The 3D bonus is independent: a physical crate activates `PhysicsSwitch3D`; `PhysicsChallengeManager` allows completion when the player enters `ExitPortal3D` **while the plate is active**.

## Code organization

```text
Assets/
  Art/
  Data/
    LevelSettings.asset
  Editor/
    Homework3ProjectBuilder.cs
    Homework3BuildExporter.cs
    Homework4ProjectSetup.cs
    Homework4BuildExporter.cs
  Prefabs/
  Scenes/
    MainMenu.unity
    Level02.unity
    Victory.unity
    GameOver.unity
    PhysicsBonus3D.unity
  Scripts/
    Player/        PlayerController.cs, MobileInputState.cs, CharacterController3D.cs
    Game/          GameSession.cs, LevelController.cs, GoalTrigger.cs,
                   PitTrigger.cs, PhysicsChallengeManager.cs
    World/         LampController.cs, ShadowBridge.cs, MoteSpawner.cs,
                   MoteLife.cs, PrefabSpawner.cs, SwingingEnergyOrb.cs,
                   PhysicsSwitch3D.cs, ExitPortal3D.cs
    Collectibles/  CrystalPickup.cs
    UI/            GameMenuUI.cs, TouchControlButton.cs
    Events/        GameEvents.cs
    Data/          LevelSettings.cs
```

## Class relationships (UML)

```mermaid
classDiagram
    class LevelSettings {
      <<ScriptableObject>>
      +float moveSpeed
      +float jumpSpeed
      +float startingEnergy
      +float timeLimitSeconds
      +int requiredCrystals
    }
    class LevelController {
      +LevelSettings Settings
      +Configure(LevelSettings)
    }
    class GameSession {
      +int crystalsCollected
      +BeginLevel()
      +FinishLevel(float)
      +FailLevel(float)
    }
    class GameEvents {
      <<static event bus>>
      +CrystalCollected(float)
      +GoalReached()
      +PlayerDied()
      +LampEnergyChanged(float)
    }
    class PlayerController {
      +FixedUpdate()
      +OnCollisionEnter2D(Collision2D)
    }
    class CrystalPickup {
      +OnTriggerEnter2D(Collider2D)
    }
    class SwingingEnergyOrb {
      +OnCollisionEnter2D(Collision2D)
    }
    class LampController {
      +Recharge(float)
      +BridgeConditionsChanged(bool)
    }
    class ShadowBridge {
      +IsSolid
    }
    class CharacterController3D {
      +Update()
      +OnControllerColliderHit(ControllerColliderHit)
    }
    class PhysicsSwitch3D {
      +IsPressed
    }
    class PhysicsChallengeManager {
      +TryFinish()
    }
    class ExitPortal3D {
      +OnTriggerEnter(Collider)
    }

    LevelController --> LevelSettings : reads configuration
    GameSession ..> GameEvents : subscribes to results
    CrystalPickup ..> GameEvents : publishes collection
    SwingingEnergyOrb ..> GameEvents : publishes collection
    LampController ..> GameEvents : subscribes and publishes energy
    ShadowBridge --> LampController : subscribes to bridge state
    PlayerController --> LevelController : reads player settings
    LampController --> LevelController : reads lamp settings
    PhysicsChallengeManager --> PhysicsSwitch3D : reads switch state
    ExitPortal3D --> PhysicsChallengeManager : requests completion
```

### Event-driven communication

```mermaid
flowchart TD
    A[Player touches crystal or swinging orb] --> B[GameEvents.CrystalCollected]
    B --> C[GameSession: increment crystal count]
    B --> D[LampController: add lamp energy]
    E[Player reaches goal] --> F[GameEvents.GoalReached]
    F --> G{Enough crystals?}
    G -->|Yes| H[Victory scene]
    G -->|No| I[Show status message]
    J[Player falls or time ends] --> K[GameEvents.PlayerDied]
    K --> L[GameOver scene]
```

## Architecture decisions and tradeoffs

1. **Event bus instead of tightly coupled pickups:** `CrystalPickup` previously called both `GameSession` and `LampController` directly. It now raises one `GameEvents.CrystalCollected` event; the session and lamp react independently. This makes a new collectible easier to introduce without rewriting the receiver classes. The cost is that event subscriptions need lifecycle discipline; listeners register and unregister in `OnEnable`/`OnDisable`.
2. **Configuration separated from behavior:** `LevelSettings` is a `ScriptableObject` asset (`Assets/Data/LevelSettings.asset`) containing player speeds, lamp energy/drain, timer, required crystals and reward values. Tweaking a level does not require editing hard-coded numeric values in C#.
3. **Different physics models have different responsibilities:** The 2D game uses `Rigidbody2D`, `Physics2D` and `DistanceJoint2D`; the isolated 3D challenge uses `CharacterController`, `Rigidbody` and `Physics.Raycast`. Splitting these systems keeps the original game playable instead of mixing incompatible 2D/3D physics APIs in one player script.

**Static and singleton choices:** `GameEvents` is a static event bus shared by the scene components. `GameSession.Instance` is a persistent singleton (`DontDestroyOnLoad`) because results must survive scene transitions. `LampController.Instance` is used for the active level's lamp by a few legacy consumers. `MobileInputState` is static transient input shared between touch UI and gameplay, reset on level start. Other gameplay communication is preferably through local references and events.

**Clean-code responsibilities:** `PlayerController` handles player movement and grounded state; `CrystalPickup` detects pickup; `LampController` handles lamp state/aim/energy; `ShadowBridge` handles bridge collider/visual state; `GameSession` handles win/loss and result recording; `LevelController` handles the timer and level UI. The 3D puzzle has separate `CharacterController3D`, `PhysicsSwitch3D` and `PhysicsChallengeManager` classes.

## Adding a new collectible: what changes?

For another energy pickup, duplicate the existing Crystal prefab, adjust its sprite and `energyReward`, and keep the `GameEvents.RaiseCrystalCollected(reward)` mechanism. No change is needed in `GameSession` or `LampController`. For a level-wide change to normal crystal rewards, edit `crystalEnergyReward` in the `LevelSettings` asset. If a fundamentally different reward is needed (not energy and crystal count), introduce a new event and listeners instead of overloading `CrystalCollected`.

## Physics requirements — where to find them

| Requirement | Implementation | Evidence |
| --- | --- | --- |
| `CharacterController` | 3D bonus player | `CharacterController3D.cs` with `CharacterController.Move()` |
| `Rigidbody` / `Rigidbody2D` | 2D player, swinging orb, 3D crate | `PlayerController.cs`; `SwingingEnergyOrb.cs`; `PhysicsBonus3D.unity` |
| `AddForce` / `AddTorque` | 2D player/orb impulse and 3D crate push | `PlayerController.FixedUpdate()`; `SwingingEnergyOrb.Start()`; `CharacterController3D.Push()` |
| `Raycast` / `SphereCast` | 2D ground checks and 3D forward interaction | `PlayerController.IsGrounded()`; `CharacterController3D.Update()` |
| `Collision2D` / `Collision` | Landing and orb-player collisions | `PlayerController.OnCollisionEnter2D()`; `SwingingEnergyOrb.OnCollisionEnter2D()` |
| `Joint` / `Effector` | Hanging energy orb | `DistanceJoint2D` added by `Homework4ProjectSetup` |

## Manual test record (21 Nov 2026)

The following were confirmed during manual Unity Editor testing, as reported by the developer:

- Shadow Switch gameplay: working.
- Physics Bonus 3D player movement and jump in the **Game** tab: working.
- Crate pushing with E: working.
- Green pressure switch: working.
- Purple exit portal: working.
- Unity Console: no red errors observed during those checks.

Still verify separately before submission: time-limit loss, pit loss, victory requiring a crystal, lamp energy exhaustion, swinging orb collection, WebGL build and browser gameplay.

## Export and publish

1. If needed, run `Homework 4 > Upgrade Existing Project` in the Editor to generate the settings asset and bonus scene. Avoid running the Homework 3 scene generator afterward: it can overwrite upgraded scenes.
2. Run `Homework 4 > Export WebGL (all 5 scenes)` and confirm a successful build in the Console.
3. Zip the **contents of** `Builds/WebGL`, not the parent folder. The ZIP root must contain `index.html` and the rest of the generated build files.
4. Upload the ZIP on itch.io as an HTML/browser game, then test both game modes in an ordinary browser.
5. Commit the full Unity source project to GitHub (`Assets`, `Packages`, `ProjectSettings`, and this README). Do not commit generated `Library`, `Temp`, `Obj` or `Builds` directories.

**itch.io playable version:** add/update the Homework 4 playable link here after publishing.

## Repository preparation

Place this file at the repository root as `README.md` so GitHub renders the Mermaid diagrams automatically. If a root README already exists, merge any required course/repository links instead of losing them. Use a Unity-appropriate `.gitignore`.
