using System;
using System.Collections.Generic;
using System.Linq;

namespace Ryujinx.HLE
{
    public class JoyConAutoManager
    {
        // This method finds the INDIVIDUAL controller to get accurate input,
        // while leaving the original device connected for accurate vibration.
        public static dynamic GetHybridState(dynamic currentDevice, string type, dynamic inputManager)
        {
            try
            {
                // 1. Identify which side we need based on the Player Config
                string targetName = "";
                if (type.Contains("Right")) targetName = "(R)";
                else if (type.Contains("Left")) targetName = "(L)";

                // If it's not a Joy-Con, or inputManager is missing, just use the default
                if (string.IsNullOrEmpty(targetName) || inputManager == null) return currentDevice?.GetState();

                // 2. Scan the drivers for the INDIVIDUAL hardware
                var drivers = new List<dynamic>();
                try { if (inputManager.KeyboardDriver != null) drivers.Add(inputManager.KeyboardDriver); } catch {}
                try { if (inputManager.GamepadDriver != null) drivers.Add(inputManager.GamepadDriver); } catch {}

                foreach (var driver in drivers)
                {
                    try
                    {
                        IEnumerable<string> ids = driver.GetGamepadIds();
                        foreach (string id in ids)
                        {
                            var device = driver.GetGamepad(id);
                            if (device != null) 
                            {
                                // 3. THE MATCH MAKER
                                // We look for a device that has "(R)" or "(L)" in the name...
                                // BUT excludes the "L/R" combined device.
                                string dName = device.Name;
                                if (dName.Contains(targetName) && !dName.Contains("L/R"))
                                {
                                    // FOUND IT! We hijack the input from this individual stick.
                                    return device.GetState();
                                }
                            }
                        }
                    }
                    catch {}
                }
            }
            catch {}
            
            // Fallback: If we can't find the individual stick, use the combined one.
            return currentDevice?.GetState();
        }
    }
}
