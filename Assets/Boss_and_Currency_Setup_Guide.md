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
