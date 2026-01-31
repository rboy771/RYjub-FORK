using System;
using System.Collections.Generic;
using System.Linq; 
namespace Ryujinx.HLE { 
    public class JoyConAutoManager { 
        // We bake the captured IDs directly into the C# code
        private static string SavedRightId = "0-00000005-057e-0000-0620-000000006801"; 
        private static string SavedLeftId = "0-00000005-057e-0000-0720-000000006802"; 

        public static dynamic HijackState(dynamic originalDevice, string type, dynamic inputManager) { 
            try { 
                // 1. Determine which ID we need
                string neededId = "";
                if (type.Contains("Right")) neededId = SavedRightId;
                else if (type.Contains("Left")) neededId = SavedLeftId;

                // If not a Joy-Con slot, ignore
                if (string.IsNullOrEmpty(neededId)) return originalDevice.GetState();

                // 2. Ask the driver: 'Give me the device with THIS specific ID'
                // We try both drivers (Keyboard/HID and Gamepad/SDL2)
                var drivers = new List<dynamic>();
                try { if (inputManager.KeyboardDriver != null) drivers.Add(inputManager.KeyboardDriver); } catch {}
                try { if (inputManager.GamepadDriver != null) drivers.Add(inputManager.GamepadDriver); } catch {}

                foreach(var driver in drivers) {
                    try {
                        // Direct lookup - fast and accurate
                        var device = driver.GetGamepad(neededId);
                        if (device != null) {
                            // Found it! Return the split input.
                            return device.GetState();
                        }
                        
                        // Fallback: Scan IDs in case of minor formatting differences
                        foreach(string id in driver.GetGamepadIds()) {
                            if (id == neededId || id.Contains(neededId)) {
                                var scanDev = driver.GetGamepad(id);
                                if (scanDev != null) return scanDev.GetState();
                            }
                        }
                    } catch {}
                }
            } catch {} 
            // Fallback to combined state
            return originalDevice.GetState(); 
        } 
    } 
}
