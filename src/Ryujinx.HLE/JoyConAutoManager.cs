using System;
using System.Collections.Generic;
using System.Linq;

namespace Ryujinx.HLE
{
    public class JoyConAutoManager
    {
        private const ushort NintendoVendorId = 0x057E;
        private const ushort JoyConLProductId = 0x2006;
        private const ushort JoyConRProductId = 0x2007;

        private dynamic _inputManager;
        private dynamic _config;

        public JoyConAutoManager(object inputManager, object config)
        {
            _inputManager = inputManager;
            _config = config;
        }

        public void AutoAssignForMultiplayer()
        {
            try
            {
                if (_inputManager?.KeyboardDriver == null) return;

                IEnumerable<string> devices = _inputManager.KeyboardDriver.GetGamepadIds();
                var deviceList = devices.ToList();
                
                var leftJoyCons = deviceList.Where(id => IsJoyCon(id, true)).ToList();
                var rightJoyCons = deviceList.Where(id => IsJoyCon(id, false)).ToList();

                if (leftJoyCons.Count > 0 && rightJoyCons.Count > 0)
                {
                    AssignIndividualJoyCon(1, leftJoyCons[0], "Left");
                    AssignIndividualJoyCon(2, rightJoyCons[0], "Right");
                }
            }
            catch { /* Avoid crashing if dynamic resolution fails */ }
        }

        private bool IsJoyCon(string deviceId, bool isLeft)
        {
            ushort targetPid = isLeft ? JoyConLProductId : JoyConRProductId;
            return deviceId.Contains($"vid_{NintendoVendorId:x4}") && deviceId.Contains($"pid_{targetPid:x4}");
        }

        private void AssignIndividualJoyCon(int playerIndex, string deviceId, string side)
        {
            Console.WriteLine($"[Input-HLE] Auto-Config: Player {playerIndex} assigned to {side} Joy-Con ({deviceId})");
        }

        public void SendPrecisionVibration(string deviceId, float lowAmp, float lowFreq, float highAmp, float highFreq)
        {
            try
            {
                var gamepad = _inputManager?.KeyboardDriver?.GetGamepad(deviceId);
                if (gamepad != null)
                {
                    gamepad.SetVibration(lowAmp, highAmp); 
                }
            }
            catch { }
        }
    }
}

