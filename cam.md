# Camera Controller Script Overview

The camera handling in this project revolves largely around the `CameraController.cs` script, which tracks the player, applies view bounds ("lock areas"), and deals with scene transitions and image effects.

## `CameraController.cs` snippet

```csharp
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // ... basic setup and properties ...
    public CameraLockArea CurrentLockArea { get; }
    public bool IsBloomForced { get; set; }

    public void GameInit()
    {
        this.gm = GameManager.instance;
        this.cam = base.GetComponent<Camera>();
        this.cameraParent = base.transform.parent.transform;
        this.ApplyEffectConfiguration();
        this.gm.UnloadingLevel += this.OnLevelUnload;
        this.gm.OnFinishedEnteringScene += this.OnFinishedEnteringScene;
    }

    public void SceneInit()
    {
        // Initializes lock zones and follows the hero.
        if (this.gm.IsGameplayScene())
        {
            this.hero_ctrl = HeroController.instance;
            this.hero_ctrl.heroInPosition += this.PositionToHero;
            // Evaluates scene boundaries and lock areas.
            this.xLockMin = 0f;
            this.xLockMax = this.xLimit;
            // ...
        }
    }

    private void LateUpdate()
    {
        // ... Check if time is paused ...
        if (Time.timeScale <= Mathf.Epsilon) return;

        // Uses SmoothDamp to follow the target with easing.
        // Also checks if the player is looking up or down to adjust offsets.
        if (this.hero_ctrl.cState.lookingUp || this.hero_ctrl.cState.lookingUpRing)
        {
            this.lookOffset = this.hero_ctrl.transform.position.y - position3.y + 6f;
        }
        else if (this.hero_ctrl.cState.lookingDown || this.hero_ctrl.cState.lookingDownRing)
        {
            this.lookOffset = this.hero_ctrl.transform.position.y - position3.y - 6f;
        }

        // Keeps the camera within scene limits or current lock areas
        if (this.mode == CameraController.CameraMode.FOLLOWING || this.mode == CameraController.CameraMode.LOCKED)
        {
            this.destination = this.KeepWithinSceneBounds(this.destination);
        }

        // Apply smooth dampening to the destination coordinates.
        position.x = Vector3.SmoothDamp(position, new Vector3(this.destination.x, num2, z), ref this.velocityX, this.dampTimeX).x;
        // ... applies similar logic for Y-axis with specific rising/falling speeds.
    }
}
```

## How it works

1. **Initialization (`GameInit` & `SceneInit`)**: When a scene loads, the camera tracks the `GameManager` and `HeroController`. It discovers map bounds (`xLimit`, `yLimit`) and attaches to player delegates like `heroInPosition` to snap into the correct starting spot.
2. **Post-Processing & Effects (`ApplyEffectConfiguration`)**: The script toggles standard image effects dynamically based on user graphics settings and the active zone (e.g., toggling bloom in memory scenes or forcing certain effects when configuring UI vs Gameplay).
3. **Tracking & Easing (`LateUpdate`)**: The engine uses `Vector3.SmoothDamp` to gently pan towards the player's predicted destination. This prevents jarring motions. It checks hero states (`lookingUp`, `lookingDown`) allowing the player to naturally peek at off-screen platforms and hazards.
4. **Camera Lock Areas (`CurrentLockArea`)**: When the hero moves into special "locked" rooms (such as boss rooms), the limits constrain the camera's `destination` logic, restricting `KeepWithinSceneBounds` to a localized box rather than the larger map.

## Related classes

- **`CameraTarget.cs`** & **`CameraLockArea.cs`**: Handles the physical invisible boundaries that govern camera clamps and where the target anchor rests.
- **`CameraShake.cs`**: Integrates into the controller to briefly displace the camera target for impact reactions without resetting its actual tracked destination.