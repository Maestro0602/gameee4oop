# Enemy Mechanics Overview

This document explains the movement, attack patterns, and special behaviors of the enemies found in the game.

## Bombardier Beetle (`BombardierBeetle.cs`)
- **Movement:** Remains stationary until the player comes within its `aggroRange`. 
- **Attack (Charge):** Once triggered, it charges horizontally towards the player's last direction at a fixed `chargeSpeed`.
- **Other Mechanics:** 
  - Uses its Rigidbody2D to control velocity.
  - Flips its local scale visually so the sprite always faces the direction of the charge.
  - Stops charging immediately when colliding with walls or surfaces.

## Centipede (`Centipede.cs`)
- **Movement:** Moves continuously back and forth, patrolling along platforms. It crawls at a steady `speed` using a coroutine state machine.
- **Attack:** No dedicated attack script; it relies on standard collision/hitbox damage to hurt the player upon contact.
- **Other Mechanics:**
  - Employs a downward raycast (`CheckGround`) to detect the edge of platforms and a horizontal raycast (`CheckWall`) to detect walls. 
  - If it hits a wall or ledge, it switches directions (`IsTurning = true`) and reverses its visual scale to face the opposite way.

## Golden Wasp Boss Stinger (`GoldenWaspBossStinger.cs`)
- **Movement / Attack:** Functions as a projectile rather than a full enemy AI. It calculates a linear trajectory by converting a specified flight angle (`direction` in degrees) into a 2D velocity vector using Sine and Cosine math.
- **Other Mechanics:** 
  - Initialized with a `direction` and `initialSpeed` when fired by a boss.
  - Features an internal `timer` set to 2 seconds. When the timer hits 0, the stinger automatically destroys/disables itself to clean up missed projectiles.

## Luminescent Cocoon (`LuminescentCocoon.cs`)
- **Movement:** Completely stationary (static hazard/object).
- **Attack:** Works like an environmental hazard or a mine. If touched by the player (`HeroBox`), a weapon attack (`Nail Attack`), or a spell (`Hero Spell`), it triggers.
- **Other Mechanics:**
  - Upon being triggered, it calls `Burst()`. 
  - If the `bomb` flag is active, it spawns an `explosionPrefab` before destroying its own game object.

## Moss Moth (`MossMoth.cs`)
- **Movement (Buzzing):** Uses a fluid, physics-like buzzing motion. Instead of directly moving to random spots, it applies random directional thrusts (`accelerationMax`), which is artificially slowed via a `dampener` variable to mimic small insect flight. A weak gravitational pull continually draws it back to its starting coordinate so it doesn't wander off the screen.
- **Attack:** Defensive/Ambient. It lacks an active attack sequence.
- **Other Mechanics:** 
  - Supports a special `songMode`. When activated, rather than buzzing freely, it locks strictly into its starting location and rapidly vibrates in place for a chaotic fluttering effect.
