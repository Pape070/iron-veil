#!/usr/bin/env python3
"""Generates Assets/_Project/Resources/GameInput.inputactions.

The Unity Input System asset format is JSON with a GUID per action and binding.
Hand editing it is error prone, so it is generated from the tables below and the
output is committed. Re-run after changing the action tables:

    python3 tools/inputmap/generate_inputactions.py

Two control schemes are authored: KeyboardMouse and Gamepad (Xbox layout). Every
gameplay action is reachable from both, except aiming, which uses the mouse
position on keyboard and the right stick on a pad.
"""

import hashlib
import json
import os

# Deterministic GUIDs: derived from a namespace + key so regenerating the file
# never reshuffles ids (which would break saved binding overrides).
NAMESPACE = "ironveil.input"


def guid(*parts):
    digest = hashlib.sha1((NAMESPACE + "|" + "|".join(parts)).encode("utf-8")).hexdigest()
    return "{}-{}-{}-{}-{}".format(digest[0:8], digest[8:12], digest[12:16], digest[16:20], digest[20:32])


# name, type, control type, initial state check
GAMEPLAY_ACTIONS = [
    ("Move", "Value", "Vector2", True),
    ("Aim", "Value", "Vector2", True),
    ("AimPoint", "Value", "Vector2", True),
    ("Fire", "Button", None, False),
    ("Reload", "Button", None, False),
    ("AbilityPrimary", "Button", None, False),
    ("AbilitySecondary", "Button", None, False),
    ("SwapWeapon", "Button", None, False),
    ("NextWeapon", "Button", None, False),
    ("PreviousWeapon", "Button", None, False),
    ("UseFlask", "Button", None, False),
    ("Brace", "Button", None, False),
    ("Interact", "Button", None, False),
    ("Pause", "Button", None, False),
]

UI_ACTIONS = [
    ("Navigate", "Value", "Vector2", True),
    ("Submit", "Button", None, False),
    ("Cancel", "Button", None, False),
    ("Point", "PassThrough", "Vector2", True),
    ("Click", "PassThrough", None, False),
    ("ScrollWheel", "PassThrough", "Vector2", True),
    ("TabLeft", "Button", None, False),
    ("TabRight", "Button", None, False),
]

# (map, action, path, groups)  groups: which control scheme the binding belongs to
SIMPLE_BINDINGS = [
    # ---- Gameplay: keyboard + mouse ----
    ("Gameplay", "AimPoint", "<Mouse>/position", "KeyboardMouse"),
    ("Gameplay", "Fire", "<Mouse>/leftButton", "KeyboardMouse"),
    ("Gameplay", "Fire", "<Keyboard>/space", "KeyboardMouse"),
    ("Gameplay", "Reload", "<Keyboard>/r", "KeyboardMouse"),
    ("Gameplay", "AbilityPrimary", "<Keyboard>/q", "KeyboardMouse"),
    ("Gameplay", "AbilitySecondary", "<Keyboard>/e", "KeyboardMouse"),
    ("Gameplay", "SwapWeapon", "<Keyboard>/tab", "KeyboardMouse"),
    ("Gameplay", "NextWeapon", "<Mouse>/scroll/up", "KeyboardMouse"),
    ("Gameplay", "PreviousWeapon", "<Mouse>/scroll/down", "KeyboardMouse"),
    ("Gameplay", "UseFlask", "<Keyboard>/x", "KeyboardMouse"),
    ("Gameplay", "Brace", "<Keyboard>/leftCtrl", "KeyboardMouse"),
    ("Gameplay", "Interact", "<Keyboard>/f", "KeyboardMouse"),
    ("Gameplay", "Pause", "<Keyboard>/escape", "KeyboardMouse"),

    # ---- Gameplay: Xbox layout ----
    ("Gameplay", "Aim", "<Gamepad>/rightStick", "Gamepad"),
    ("Gameplay", "Fire", "<Gamepad>/rightTrigger", "Gamepad"),
    ("Gameplay", "Reload", "<Gamepad>/buttonWest", "Gamepad"),
    ("Gameplay", "AbilityPrimary", "<Gamepad>/leftShoulder", "Gamepad"),
    ("Gameplay", "AbilitySecondary", "<Gamepad>/rightShoulder", "Gamepad"),
    ("Gameplay", "SwapWeapon", "<Gamepad>/buttonNorth", "Gamepad"),
    ("Gameplay", "NextWeapon", "<Gamepad>/dpad/right", "Gamepad"),
    ("Gameplay", "PreviousWeapon", "<Gamepad>/dpad/left", "Gamepad"),
    ("Gameplay", "UseFlask", "<Gamepad>/dpad/down", "Gamepad"),
    ("Gameplay", "Brace", "<Gamepad>/leftTrigger", "Gamepad"),
    ("Gameplay", "Interact", "<Gamepad>/buttonSouth", "Gamepad"),
    ("Gameplay", "Pause", "<Gamepad>/start", "Gamepad"),

    # ---- UI: keyboard + mouse ----
    ("UI", "Submit", "<Keyboard>/enter", "KeyboardMouse"),
    ("UI", "Cancel", "<Keyboard>/escape", "KeyboardMouse"),
    ("UI", "Point", "<Mouse>/position", "KeyboardMouse"),
    ("UI", "Click", "<Mouse>/leftButton", "KeyboardMouse"),
    ("UI", "ScrollWheel", "<Mouse>/scroll", "KeyboardMouse"),
    ("UI", "TabLeft", "<Keyboard>/q", "KeyboardMouse"),
    ("UI", "TabRight", "<Keyboard>/e", "KeyboardMouse"),

    # ---- UI: Xbox layout ----
    ("UI", "Submit", "<Gamepad>/buttonSouth", "Gamepad"),
    ("UI", "Cancel", "<Gamepad>/buttonEast", "Gamepad"),
    ("UI", "TabLeft", "<Gamepad>/leftShoulder", "Gamepad"),
    ("UI", "TabRight", "<Gamepad>/rightShoulder", "Gamepad"),
]

# Composite 2D vectors: (map, action, composite name, [(part name, path, groups), ...])
COMPOSITES = [
    ("Gameplay", "Move", "WASD",
     [("up", "<Keyboard>/w", "KeyboardMouse"),
      ("down", "<Keyboard>/s", "KeyboardMouse"),
      ("left", "<Keyboard>/a", "KeyboardMouse"),
      ("right", "<Keyboard>/d", "KeyboardMouse")]),

    ("Gameplay", "Move", "Arrows",
     [("up", "<Keyboard>/upArrow", "KeyboardMouse"),
      ("down", "<Keyboard>/downArrow", "KeyboardMouse"),
      ("left", "<Keyboard>/leftArrow", "KeyboardMouse"),
      ("right", "<Keyboard>/rightArrow", "KeyboardMouse")]),

    ("Gameplay", "Move", "Left Stick",
     [("up", "<Gamepad>/leftStick/up", "Gamepad"),
      ("down", "<Gamepad>/leftStick/down", "Gamepad"),
      ("left", "<Gamepad>/leftStick/left", "Gamepad"),
      ("right", "<Gamepad>/leftStick/right", "Gamepad")]),

    ("UI", "Navigate", "WASD",
     [("up", "<Keyboard>/w", "KeyboardMouse"),
      ("down", "<Keyboard>/s", "KeyboardMouse"),
      ("left", "<Keyboard>/a", "KeyboardMouse"),
      ("right", "<Keyboard>/d", "KeyboardMouse")]),

    ("UI", "Navigate", "Arrows",
     [("up", "<Keyboard>/upArrow", "KeyboardMouse"),
      ("down", "<Keyboard>/downArrow", "KeyboardMouse"),
      ("left", "<Keyboard>/leftArrow", "KeyboardMouse"),
      ("right", "<Keyboard>/rightArrow", "KeyboardMouse")]),

    ("UI", "Navigate", "D-Pad",
     [("up", "<Gamepad>/dpad/up", "Gamepad"),
      ("down", "<Gamepad>/dpad/down", "Gamepad"),
      ("left", "<Gamepad>/dpad/left", "Gamepad"),
      ("right", "<Gamepad>/dpad/right", "Gamepad")]),

    ("UI", "Navigate", "Left Stick",
     [("up", "<Gamepad>/leftStick/up", "Gamepad"),
      ("down", "<Gamepad>/leftStick/down", "Gamepad"),
      ("left", "<Gamepad>/leftStick/left", "Gamepad"),
      ("right", "<Gamepad>/leftStick/right", "Gamepad")]),
]

CONTROL_SCHEMES = [
    ("KeyboardMouse", "KeyboardMouse",
     [("<Keyboard>", False, False), ("<Mouse>", False, False)]),
    ("Gamepad", "Gamepad",
     [("<Gamepad>", False, False)]),
]


def build_map(map_name, actions):
    bindings = []
    for map_key, action, path, groups in SIMPLE_BINDINGS:
        if map_key != map_name:
            continue
        bindings.append({
            "name": "",
            "id": guid("simple", map_name, action, path),
            "path": path,
            "interactions": "",
            "processors": "",
            "groups": groups,
            "action": action,
            "isComposite": False,
            "isPartOfComposite": False,
        })

    for map_key, action, composite_name, parts in COMPOSITES:
        if map_key != map_name:
            continue
        groups = "&".join(sorted({groups for _, _, groups in parts}))
        bindings.append({
            "name": composite_name,
            "id": guid("composite", map_name, action, composite_name),
            "path": "2DVector",
            "interactions": "",
            "processors": "",
            "groups": "",
            "action": action,
            "isComposite": True,
            "isPartOfComposite": False,
        })
        for part_name, path, part_groups in parts:
            bindings.append({
                "name": part_name,
                "id": guid("part", map_name, action, composite_name, part_name),
                "path": path,
                "interactions": "",
                "processors": "",
                "groups": part_groups,
                "action": action,
                "isComposite": False,
                "isPartOfComposite": True,
            })

    return {
        "name": map_name,
        "id": guid("map", map_name),
        "actions": [
            {
                "name": name,
                "type": action_type,
                "id": guid("action", map_name, name),
                "expectedControlType": control_type or "",
                "processors": "",
                "interactions": "",
                "initialStateCheck": initial_state,
            }
            for name, action_type, control_type, initial_state in actions
        ],
        "bindings": bindings,
    }


def main():
    asset = {
        "name": "GameInput",
        "maps": [
            build_map("Gameplay", GAMEPLAY_ACTIONS),
            build_map("UI", UI_ACTIONS),
        ],
        "controlSchemes": [
            {
                "name": scheme_name,
                "bindingGroup": binding_group,
                "devices": [
                    {"devicePath": path, "isOptional": optional, "isOR": is_or}
                    for path, optional, is_or in devices
                ],
            }
            for scheme_name, binding_group, devices in CONTROL_SCHEMES
        ],
    }

    root = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
    target = os.path.join(root, "Assets", "_Project", "Resources", "GameInput.inputactions")
    os.makedirs(os.path.dirname(target), exist_ok=True)
    with open(target, "w", encoding="utf-8") as handle:
        json.dump(asset, handle, indent=4)
        handle.write("\n")

    # Sanity checks that would otherwise only fail inside the Editor.
    ids = []
    for map_data in asset["maps"]:
        ids.append(map_data["id"])
        for action in map_data["actions"]:
            ids.append(action["id"])
        for binding in map_data["bindings"]:
            ids.append(binding["id"])
        for binding in map_data["bindings"]:
            assert binding["path"] or binding["isComposite"], "binding without a path"
            actions = {a["name"] for a in map_data["actions"]}
            assert binding["action"] in actions, "binding references unknown action"
    assert len(ids) == len(set(ids)), "duplicate GUIDs generated"

    print("wrote", target)
    print("maps:", [m["name"] for m in asset["maps"]])
    for map_data in asset["maps"]:
        print("  {0}: {1} actions, {2} bindings".format(
            map_data["name"], len(map_data["actions"]), len(map_data["bindings"])))


if __name__ == "__main__":
    main()
