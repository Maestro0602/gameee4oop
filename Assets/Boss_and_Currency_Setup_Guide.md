# Boss and Currency System - Setup Guide

This guide details how to configure the newly added Currency system and Boss/Miniboss behaviors in your Unity scene, leveraging both the provided C# scripts and the HutongGames PlayMaker asset.

---

## 1. PlayMaker Pre-requisites

Since you have PlayMaker in your project, ensure the custom actions we created compile correctly:
1. Open Unity and wait for scripts to compile.
2. If you see errors regarding `PLAYMAKER` not being defined, go to **Edit > Project Settings > Player**.
3. Under **Other Settings > Scripting Define Symbols**, add `PLAYMAKER` to the list.
4. Click Apply. 

*Note: You can also just remove the `#if PLAYMAKER` lines from `AddCurrency.cs` and `FireGrimmBall.cs`.*

---

## 2. Currency System Setup

The Currency System allows players to pick up "Geo" or "Shards", tracking them in a persistent save data container.

### Step A: The PlayerData Manager
1. In your **first scene** (or a persistent Main Menu scene), create an Empty GameObject and name it `GameManager`.
2. Attach the `PlayerData` script to this GameObject.
3. This script has `DontDestroyOnLoad`, meaning it will carry your Geo and Shards across all scene transitions.

### Step B: Setting up the Geo (Coin) Drop
1. Drag your Geo/Coin sprite into the scene.
2. Add a **Rigidbody2D** to it. 
   - Set it to **Dynamic**.
   - Add a little bit of Bounciness to its Physics Material 2D if you want it to bounce off floors.
3. Add a **CircleCollider2D** to it.
   - Check the **Is Trigger** box.
4. Attach the `GeoControl` script.
   - Set the `valueReference` (e.g., `1` for small geo, `5` for large geo).
5. Drag this GameObject from the Hierarchy into your Project panel to make it a **Prefab**. You can now spawn this prefab whenever an enemy dies.

---

## 3. Miniboss Setup: Tiny Moss Fly

The Tiny Moss Fly is a purely C#-driven enemy that buzzes around dynamically.

1. Drag your Moss Fly sprite into the scene.
2. Add a **Rigidbody2D**.
   - **Important:** Set the Gravity Scale to `0` so the fly doesn't fall to the ground.
3. Add a **BoxCollider2D** or **PolygonCollider2D** for its hitbox.
   - Check **Is Trigger**.
4. Attach the `TinyMossFly` script.
5. **Tuning the flight:**
   - **Roaming Range:** How far from its spawn point it can travel.
   - **Speed Max:** Its top flying speed.
   - **Acceleration Max:** How jerky/erratic its movement is.
6. Make sure your Player (with the `HeroController`) is tagged as `Player` so the Moss Fly's `OnTriggerEnter2D` registers the hit and deals damage!

---

## 4. Miniboss Setup: Hive Knight Stinger

The Hive Knight shoots stingers at a specific angle using mathematical trajectories.

1. Drag your Stinger/Spike sprite into the scene.
2. Add a **Rigidbody2D** (Gravity Scale = 0) and a **Collider2D** (Is Trigger = true).
3. Attach the `HiveKnightStinger` script.
4. Set the default `speed` (e.g., `20`) and `timer` (how long until it destroys itself, e.g., `3` seconds).
5. Turn this into a **Prefab**.
6. When your Hive Knight boss wants to attack, you can instantiate this prefab and modify its `direction` value (in degrees, 0-360) to shoot it at the player.

---

## 5. Boss Setup: Troupe Master Grimm (PlayMaker Integration)

Grimm uses PlayMaker to control his state machine, and pure C# for the complex fireball movement.

### Step A: The Grimmball Prefab
1. Drag your Fireball sprite into the scene.
2. Add a **Rigidbody2D** (Gravity Scale = 0) and a **Collider2D** (Is Trigger = true).
3. Attach the `GrimmballControl` script.
   - Set `force` to control horizontal speed.
   - Set `tweenY` to control the height of its vertical sine-wave sway.
4. Turn this into a **Prefab** and delete it from the scene.

### Step B: The PlayMaker FSM
1. Select your Grimm Boss GameObject in the scene.
2. Add an **FSM** component.
3. Create standard boss states: `Idle`, `Telegraph`, `Attack`, `Cooldown`.
4. In the `Attack` state:
   - Open the **Action Browser**.
   - Search for **Fire Grimm Ball** (under the "Custom Boss" category).
   - Add the action to the state.
   - Drag your **Grimmball Prefab** into the `grimmballPrefab` slot.
   - *(Optional)* Create an empty child GameObject on Grimm to act as the `spawnPoint` and drag it into the slot, so fireballs spawn from his hand rather than his center.
5. In the `Death` state (when the boss is defeated):
   - Open the **Action Browser**.
   - Search for **Add Currency** (under the "Custom" category).
   - Set CurrencyType to `Money` and Amount to `500` (or however much you want the boss to drop).

---

## 6. Player Attack Integration

To ensure the player can hit these enemies:
1. Open your `HeroController` script setup.
2. The `ArcMeleeHitbox` will overlap with enemy colliders.
3. Ensure your enemies (Moss Fly, Grimm, etc.) have colliders that are **NOT** set to `IsTrigger` (or have a separate hurtbox collider) so Physics2D.Overlap methods used by your melee hitbox can detect them.
4. If your player's sword hits an enemy, the `ArcMeleeHitbox.cs` will register it, and you can trigger a "Take Damage" FSM event on the boss!

---

## 7. Player Knockback & Health Setup

We recently added built-in C# functionality for when the player gets hit by enemies, taking the heavy lifting out of PlayMaker.

### Step A: Configuring Knockback
1. Select your Player (the GameObject with the `HeroController` script).
2. Look at the `HeroController` component in the Inspector.
3. You will see a new header: **Combat - Knockback**.
   - **Knockback Force**: How hard the player is launched backwards upon touching an enemy (default is ~15).
   - **Knockback Duration**: How long the player loses movement control while airborne (default is 0.25 seconds).

### Step B: How Knockback is Triggered
- For the knockback to work automatically, your enemy or hazard colliders **must** either:
  1. Have their Tag set to `Enemy` or `Hazard`.
  2. Or be placed on a Unity Layer named `Enemy`.
- When the `HeroController` collides with them (via `OnCollisionEnter2D` or `OnTriggerEnter2D`), it will cancel any current dashes or wall slides and launch the player.

### Step C: Player Health
- The `PlayerData.cs` attached to your `GameManager` also tracks your `health`.
- If you use PlayMaker on your enemies to deal damage to the player, you can use a "Send Message" action or a custom action to call `PlayerData.instance.TakeHealth(1, false, true)` to subtract from this value.

---

## 8. Boss Sprite & Animation Setup (Manual Slicing)

When you import a new boss sprite sheet, you must slice it before animating:
1. Select the imported sprite sheet image in your `Assets` folder.
2. In the Inspector, change **Sprite Mode** from `Single` to `Multiple`.
3. Click the **Sprite Editor** button.
4. Click the **Slice** dropdown at the top left. Set Type to **Automatic** (or Grid by Cell Size).
5. Click **Slice**, then click **Apply** at the top right.
6. Drag the first frame into your Scene to create the Boss GameObject.
7. Open the **Animation** window, select your Boss, click "Create", and drag the frames into the timeline to create your Idle, Walk, and Attack clips.

---

## 9. PlayMaker Death & Respawn Setup

To handle the player dying and respawning entirely within PlayMaker (without writing C# code), set up a "Death Manager" FSM on your Hero:

### Step A: The Death FSM
1. Add a new **PlayMakerFSM** to your Hero (name it `Death Manager`).
2. **State 1 (Alive Check):**
   * Use `Get Property` on your `PlayerData` script to store the `health` variable in a new PlayMaker Integer Variable called `CurrentHealth`.
   * Use `Int Compare` to check if `CurrentHealth` is Equal to or Less Than `0`.
   * If true, send a `HERO_DIED` event. If false, use a `Next Frame Event` to loop back to this state so it checks constantly.
3. **State 2 (Dying):** *(Transition here on `HERO_DIED`)*
   * First, make sure you created a `Death` animation clip in your Animation window!
   * Use `Animator Set Trigger` (or `Animator Play`) to play your Hero's `Death` animation.
   * Use `Set Velocity 2D` (X=0, Y=0) to stop the player from sliding.
   * Use `Wait` for 2 seconds to let the animation finish playing.
   * Send a `FINISHED` event.
4. **State 3 (Reload Scene):** *(Transition here on `FINISHED`)*
   * Use `Load Level` (or `Load Scene`) to reload your current scene name. This completely resets the room and enemies!

### Step B: Resetting Health on Load
Because your `PlayerData` uses `DontDestroyOnLoad`, your health will still be 0 when the scene reloads!
* Create a **Start State** in this FSM before the "Alive Check". 
* Use `Set Property` to set your `PlayerData` health back to max (e.g., 5).
* Use `Set Animator Trigger` to play your "Idle" animation (so they aren't stuck on the death frame).
* Transition from this Start state immediately into your "Alive Check" state.

### Step C: Setting a Custom Respawn Point (Checkpoints)
If you just reload the scene, the player will start wherever you placed them in the Unity Editor. To make them spawn at a specific Checkpoint (like a Bench):
1. **The Global Variables:** In the PlayMaker Editor, go to the **Globals** tab. Create two Global Float variables: `Respawn_X` and `Respawn_Y`.
2. **The Checkpoint Object:** Create a Bench or Checkpoint GameObject. Give it a Collider2D (Is Trigger = True) and an FSM.
   * When the player enters the trigger (using `Trigger Event`), use `Get Position` on the Checkpoint to get its X and Y coordinates.
   * Use `Set Float Value` to save those coordinates into your Global `Respawn_X` and `Respawn_Y` variables!
3. **The Teleport:** Go back to your Hero's Death Manager FSM. In the **Start State** (from Step B), add a `Set Position` action. 
   * Set the Hero's X to `Respawn_X` and Y to `Respawn_Y`. 
   * Now, whenever the scene reloads after death, the Hero instantly teleports to the last bench they touched!

---

## 10. Complete Boss Setup From Scratch

If you have a sprite sheet and want to build a boss completely from scratch, follow these exact steps:

### Step 1: The Visuals (Sprites & Animation)
1. **Slice the Sprite Sheet:** Click your imported sprite sheet in the Project window. Set **Sprite Mode** to `Multiple`. Click **Sprite Editor** -> Slice -> Automatic -> Slice -> Apply.
2. **Create the GameObject:** Drag the very first sliced frame (usually an idle frame) from the Project window into your Scene. Name it `Boss`.
3. **Animate It:** 
   * Open the **Animation** window (`Window -> Animation -> Animation`).
   * With the `Boss` selected, click **Create** to make an `Idle` animation clip.
   * Drag all your Idle frames into the timeline. 
   * Repeat this process to create clips for `Walk`, `Attack`, `Hurt`, and `Death`.

### Step 2: The Physics & Hitbox
1. Select your `Boss` GameObject.
2. Add a **Rigidbody2D**. Set Gravity Scale to 1 (or 3 if you want it to fall faster) and check the **Freeze Rotation Z** box under Constraints so it doesn't fall over.
3. Add a **CapsuleCollider2D** or **BoxCollider2D**. Adjust the green box to fit the boss's body. 
   * Do **NOT** check "Is Trigger". This is the physical body that the player will bump into.
   * Make sure the GameObject's Tag is set to **Enemy** so the player takes knockback when touching it.

### Step 3: Health & Currency System
1. Add the **`HealthManager`** script to your `Boss` GameObject.
2. Set the `Max Health` (e.g., 500).
3. Under the **Drops** section, drag your Geo/Coin Prefab into the `Drop Prefab` slot. Set Min Drop Count to 50 and Max to 100. (When the boss's health reaches 0, it will automatically scatter the money and destroy itself!).

### Step 4: The Boss AI (PlayMaker)
1. Add a **PlayMakerFSM** component to your `Boss`.
2. Name the FSM `Boss AI`.
3. Create the following States:
   * **State 1 (Idle):** Use `Animator Play` to play the `Idle` animation. Use `Wait` for 2 seconds, then transition to Chase.
   * **State 2 (Chase):** Use `Animator Play` to play `Walk`. Use `Move Towards` to make the boss walk towards the Player GameObject. Use `Distance To` to check how far the player is. If distance < 2, transition to Attack.
   * **State 3 (Attack):** Use `Animator Play` to play `Attack`. Use `Wait` to let the animation finish, then transition back to Idle.

### Step 5: Dealing Damage to the Player
If you want the boss's physical body to deal damage just by touching the player:
1. Since the boss is tagged as `Enemy`, your `HeroController`'s built-in `OnCollisionEnter2D` will automatically knock the player back and remove 1 health when they touch! You don't have to code anything else for contact damage.

You now have a fully functional boss that can be hit, takes damage, chases you, hurts you on contact, and explodes into money when defeated!
