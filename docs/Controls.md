# Iron Veil — Controls

Both control schemes are fully supported and every gameplay and menu action is rebindable. Aiming is the
only asymmetric action: the mouse aims at the cursor, the right stick aims in a direction, and both are
reduced to one world-space direction by `InputService.ResolveAimDirection`.

## Gameplay — keyboard and mouse

| Action | Default |
|---|---|
| Move | `W` `A` `S` `D` (or arrow keys) |
| Aim | Mouse cursor (360°) |
| Fire | Left mouse button (or `Space`) |
| Reload | `R` |
| Ability 1 (primary) | `Q` |
| Ability 2 (secondary) | `E` |
| Swap weapon | `Tab` |
| Next weapon | Mouse wheel up |
| Previous weapon | Mouse wheel down |
| Use flask | `X` |
| Brace (guard, reduces damage) | Left `Ctrl` |
| Interact (savepoints, nodes, pickups) | `F` |
| Pause | `Escape` |

## Gameplay — gamepad

| Action | Default |
|---|---|
| Move | Left stick |
| Aim | Right stick |
| Fire | Right trigger |
| Reload | `X` |
| Ability 1 (primary) | Left shoulder |
| Ability 2 (secondary) | Right shoulder |
| Swap weapon | `Y` |
| Next weapon | D-pad right |
| Previous weapon | D-pad left |
| Use flask | D-pad down |
| Brace | Left trigger |
| Interact | `A` |
| Pause | `Start` |

## Menus

| Action | Keyboard / mouse | Gamepad |
|---|---|---|
| Navigate | Arrow keys / `W` `A` `S` `D`, or point with the mouse | D-pad / left stick |
| Submit | `Enter`, or click | `A` |
| Cancel / back | `Escape`, or right click | `B` |
| Tab / cycle value | `Q` / `E`, or left/right on a row | Shoulders |

Rows with values (sliders, toggles, option rows) adjust with left/right or the shoulders. Lists,
keypads of buttons and grids all navigate spatially: the nearest widget in the pressed direction takes
focus, and the mouse moves focus by hovering, so the two input paths stay in sync by construction.

## Aim assist and accessibility

- **Aim source** (Gameplay tab): `Auto` follows the device you last touched, or lock to mouse or stick.
- **Aim assist**: bends shots toward nearby targets within a cone that widens with the setting.
- **Aim snaps to nearby targets** (Accessibility): ignores the cone and takes the closest target.
- **Aim guide line** shows the firing direction at all times; **invert vertical aim** and the **stick
  dead zone** are both adjustable.

## Rebinding

**Main menu ▸ Controls** (or Pause ▸ Controls) lists every action for the selected binding set —
**Keyboard + mouse** or **Gamepad** — including each composite part, so `W` `A` `S` `D` can be split
across devices. Selecting a row listens for the next input:

- `Escape` cancels the rebind.
- Mouse position, mouse delta and scroll are excluded from binding (they are pointer inputs, not buttons).
- After binding, any other action using the same control is named in the status line, so conflicts are
  visible and fixable rather than silent.
- **Reset all bindings** restores defaults; single actions reset from the same list.

Overrides are stored as JSON in `bindings.json` next to your saves and loaded on start, so a rebind
survives restarts, and regenerating the input asset never invalidates them (GUIDs are deterministic).
