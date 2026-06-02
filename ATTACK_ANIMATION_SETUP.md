# Frame-Perfect 2D Combat: Unity Animator & Animation Events

To make combat feel snappy and responsive—just like top-tier 2D action games—you should link your physical hitboxes (triggers) exactly to your visual animation frames. This is done using **Animation Events**. 

This prevents enemies from taking damage during the "wind-up" or "recovery" animations. Here is the industry-standard, "proper" way to implement this in Unity.

## 1. Prepare Your Hitbox Object
Instead of dynamically generating shapes in code at runtime (which is expensive and error-prone), you pre-fabricate them.

1. Right-click your overall **Hero** GameObject -> **Create Empty**. Name it `AttackHitbox_Normal`.
2. Attach these components to it:
   - `PolygonCollider2D` (Check **Is Trigger**)
   - `NailSlash`
   - `DamageEnemies`
3. Click the **Edit Collider** button on the `PolygonCollider2D`. Drag the points to shape it into the wide sweeping crescent/arch that matches your slash visual.
4. Disable the `PolygonCollider2D` component (uncheck the box next to its name). We only want it turning on precisely when the attack lands.

## 2. Using the Animation Window
1. Open the Unity Animation window (`Window > Animation > Animation`).
2. Select your Hero GameObject in the hierarchy.
3. In the Animation window dropdown, pick your attack animation clip (e.g., `Hero_Slash_Normal`).

## 3. Adding Animation Events (The "Proper" Method)
Animation Events let you fire C# functions on exact frames of an animation clip.

Since the Animator is usually on your Hero root, and `NailSlash` is on a child object, you can bounce the call through your `HeroController`, or animate the child object's state directly.

Assuming you want the simplest way (activating it via an event on the player):
1. **The Wind-up (Frame 1):** 
   - For a short 4-frame animation, frame 1 is the wind-up. The character pulls the sword back. Do nothing here.
2. **The Active Frames - The Hit (Frame 2):**
   - Move the timeline slider to frame 2, where the bright white slash crescent first appears visually.
   - Click the **Add Event** button (the small white ribbon/marker icon just below the frame timeline).
   - In the inspector for this Event, select your function: `AnimEvent_EnableNormalSlash()`. 
3. **The Active Frames - Extension (Frame 3):**
   - The sword is fully extended. The hitbox remains active automatically.
4. **The Recovery (Frame 4):**
   - The slash has faded out. The character is returning to idle.
   - Click **Add Event** again.
   - Select `AnimEvent_DisableNormalSlash()`.

> **Note on architecture:** In your `HeroController.cs`, we have set up the helper functions to match your `GameObject` references (`normalSlash`, `upSlash`, `downSlash`, etc.). Make sure these GameObjects are children of your Hero, rather than prefabs being instantiated, so the Animator can toggle their `NailSlash` components!
> ```csharp
> // Inside HeroController.cs
> public void AnimEvent_EnableNormalSlash() => EnableSlash(normalSlash);
> public void AnimEvent_DisableNormalSlash() => DisableSlash(normalSlash);
> // ...and so on for upSlash, downSlash
> 
> private void EnableSlash(GameObject slashObj) {
>     if (slashObj != null && slashObj.TryGetComponent<NailSlash>(out var slash))
>         slash.EnableHitbox();
> }
> ```

## 4. Handling Directional Attacks (Up / Down)
Do not try to mathematically rotate one hitbox using code. Game engines prefer pre-cached setup.
1. Assign the appropriate child objects (that have `NailSlash` + `PolygonCollider2D` attached) to the inspector slots on your `HeroController`:
   - `normalSlash`
   - `upSlash`
   - `downSlash`
2. Draw a unique `PolygonCollider2D` arch for each one.
3. In your **Animator**, when you play the `Slash_Up` animation, just put the Animation Events on that clip to call `AnimEvent_EnableUpSlash()`.

## 5. Why this is the "Right" Way
* **Zero Physics Overhead:** Translating sprite bounds to polygons in code frame-by-frame takes a lot of CPU power. Toggling a pre-calculated box off and on is nearly free computationally.
* **Game Feel & Fairness:** If a boss lunges at you while you are in the "wind-up" animation frame of your sword, they hit your hurtbox before your sword hitbox exists. 
* **Scalable Timing:** If you later decide your character feels too slow and you speed up the Animator to 1.5x speed, the Animation Events automatically scale with it. The hitbox will always match the visual perfectly without changing any code timers.