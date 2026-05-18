# Remaining Missing / Compile Notes

## Compile check (current scope)
The following files were checked and have **no compile errors** at this time:

- `Assets/Scripts/PlayerController2D.cs`
- `Assets/GlobalSettings/GlobalSettings.cs`
- `Assets/GlobalSettings/GlobalSettingsBase.cs`
- `Assets/Scripts/Enum/DownSlashTypes.cs`
- `Assets/Scripts/Shared/TeamCherrySharedUtilsStubs.cs`
- `Assets/Scripts/Combat/AttackDefinition2D.cs`
- `Assets/Scripts/Combat/Debug/CombatHitboxViewer2D.cs`
- `Assets/Scripts/Combat/Visuals/AttackPlaceholderVfx2D.cs`

If Unity still shows errors, paste the new Console list and I’ll fix those next.

## Still missing / TODO items

- `Assets/Scripts/Enum/SkipBehaviours.cs` is **empty**.
  - Only needed if you want a standalone enum outside `PlayerController2D`.

- Movement logic is **placeholder** in `PlayerController2D`:
  - Ground checks / slope checks / spike recovery are stubbed by the `cState` flags.
  - You must update `cState` values from collision logic before these checks mean anything.

## External namespaces

- `TeamCherry.SharedUtils` is stubbed by:
  - `Assets/Scripts/Shared/TeamCherrySharedUtilsStubs.cs`

---

If you want, I can implement the collision/ground checks to update `cState` next.
