# Joy-Con Input Mapping Bug Fix

## Problem
When Player 1 is configured as 'Joycon Right' using the Combined 'Joy-Con (L/R)' device:
- Vibration is correctly sent to the Right controller ✓
- Input (Buttons, Stick, and Shake) is incorrectly read from the Left Joy-Con ✗

## Root Cause
The `SDL3JoyConPair` class handles the combined Joy-Con (L/R) device. It previously had logic that:
- `IsPressed()` used OR logic: `left.IsPressed(inputId) || right.IsPressed(inputId)`
- `GetStick()` always mapped Left stick to left controller and Right stick to right controller
- `GetMotionData()` always read from both controllers

This meant that when Player 1 was configured as JoyconRight, the input would still be read from BOTH controllers (or the wrong one due to OR logic), even though the configuration specified JoyconRight.

## Solution
Modified [src/Ryujinx.Input.SDL3/SDL3JoyConPair.cs](src/Ryujinx.Input.SDL3/SDL3JoyConPair.cs) to:

1. **Store the configuration**: Added `_configuration` field to track the current InputConfig
2. **Update SetConfiguration()**: Now stores the configuration when it's set
3. **Fix IsPressed()**: Check controller type and read from appropriate controller:
   - If JoyconLeft: Only read from left controller
   - If JoyconRight: Only read from right controller
   - If JoyconPair: Read from both (OR logic)

4. **Fix GetStick()**: Check controller type and read stick from appropriate controller:
   - If JoyconLeft: Read both stick positions from left controller
   - If JoyconRight: Read both stick positions from right controller
   - If JoyconPair: Left stick from left, Right stick from right

5. **Fix GetMotionData()**: Check controller type and read motion from appropriate controller:
   - If JoyconLeft: Read primary and secondary accelerometer/gyroscope from left
   - If JoyconRight: Read primary and secondary accelerometer/gyroscope from right
   - If JoyconPair: Left from left controller, Right from right controller

## Result
Now when Player 1 is configured as 'Joycon Right':
- Vibration goes to Right controller ✓
- Button input is read from Right controller ✓
- Stick input is read from Right controller ✓
- Motion/Shake input is read from Right controller ✓

The same logic applies for JoyconLeft configuration.
