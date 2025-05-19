# Input System Integration Guide for First Person Controller

This guide will help you set up the First Person Controller with Unity's new Input System.

## Steps to Set Up

1. **Open the FirstPersonController prefab or scene object**
   - Select the GameObject with the `FirstPersonController` component

2. **Assign the InputSystem_Actions asset**
   - In the Inspector, find the `Input Actions` field at the top of the FirstPersonController component
   - Drag and drop the `InputSystem_Actions` asset from your Assets folder (located at `Assets/InputSystem_Actions.inputactions`)

3. **Make sure your Input Actions asset has the following actions defined:**
   - **Move** (Vector2) - For WASD/analog stick movement
   - **Look** (Vector2) - For mouse/stick camera rotation
   - **Jump** (Button) - For jumping
   - **Sprint** (Button) - For sprinting
   - **Crouch** (Button) - For crouching
   - **Zoom** (Button) - For zooming/aiming

4. **Ensure you have a PlayerInput component**
   - Add a `PlayerInput` component to the same GameObject if it doesn't already have one
   - Set its `Actions` field to the same `InputSystem_Actions` asset
   - Set `Default Map` to "Player"
   - Set `Behavior` to "Invoke Unity Events"

## Troubleshooting

- If camera movement isn't working, check that the `Look` action is properly mapped
- If movement isn't working, ensure the `Move` action is configured correctly
- For any actions that aren't working, verify they have the exact names expected by the controller: Move, Look, Jump, Sprint, Crouch, Zoom

## Workflow Tip

After modifying the Input Actions asset, remember to save it and generate the C# class by right-clicking on the asset and selecting "Generate C# Class". 