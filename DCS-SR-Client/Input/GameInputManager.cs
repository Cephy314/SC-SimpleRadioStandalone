using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameInputDotNet;
using GameInputDotNet.Interop.Enums;
using GameInputDotNet.Interop.Structs;
using NLog;

namespace Ciribob.DCS.SimpleRadio.Standalone.Client.Input;

/// <summary>
/// Handle game input and trigger events for matching bindings, as well as the storing of bindings.
/// </summary>
public class GameInputManager
{
    /// <summary>
    /// Time in milliseconds between GameInput polling is done.
    /// </summary>
    private const int _frequency = 10;
    
    /// <summary>
    /// Relationship of Commands to Bindings for quick unique lookup.
    /// </summary>
    private Dictionary<BindingCommands, Binding> _commands;

    public GameInputManager()
    {
        
    }

    public Task Start()
    {
        using var gameInput = GameInput.Create();

        while (true)
        {
            // Delay  between polling
            Task.Delay(_frequency).Wait();

            try
            {
                using (var reading = gameInput.GetCurrentReading(GameInputKind.Keyboard))
                {
                    if (reading != null)
                    {
                        
                    }
                }
            }
            
        }
    }

    /// <summary>
    /// Get all currently pressed keyboard keys.
    /// </summary>
    /// <param name="gameInput"></param>
    /// <returns></returns>
    public List<InputTrigger> GetKeyboardTriggers(GameInput gameInput)
    {
        try
        {
            using var reading = gameInput.GetCurrentReading(GameInputKind.Keyboard);
            if (reading == null)
                return [];

            var state = reading.GetKeyboardState();

            if (state is { Keys.Count: > 0 })
            {
                return state.Keys;
            }
        }
        catch (Exception e)
        {
            return [];
        }
        
        return [];
    }
    
    /// <summary>
    /// Get all currently pressed mouse buttons.
    /// </summary>
    /// <param name="gameInput"></param>
    /// <returns></returns>
    public List<InputTrigger> GetMouseTriggers(GameInput gameInput)
    {
        try
        {
            foreach(deviceId)
            var inputs = new List<InputTrigger>();
            using var reading = gameInput.GetCurrentReading(GameInputKind.Mouse);
            if (reading == null)
                return GameInputMouseButtons.None;

            var state = reading.GetMouseState();
            return state?.Buttons ?? GameInputMouseButtons.None;
        }
        catch (Exception e)
        {
            return GameInputMouseButtons.None;
        }
    }

    /// <summary>
    /// Get the current active triggers from Controller type devices (Most joysticks/fancy interfaces).
    /// </summary>
    /// <param name="gameInput">GameInput.Net object</param>
    /// <returns></returns>
    public List<InputTrigger> GetControllerTriggers(GameInput gameInput)
    {
        try
        {
            var inputs = new List<InputTrigger>();
            
            // Enumerate the devices so we can capture specific info about the device and know which one the
            // trigger is coming from.
            foreach (var controller in gameInput.EnumerateDevices(GameInputKind.Controller))
            {
                var deviceId = controller.GetDeviceInfo().DeviceId.ToHexString(true);
                
                // Get the reading from the device.
                using var reading = gameInput.GetCurrentReading(GameInputKind.Controller, controller);
                if (reading == null)
                    return null;
                var state = reading.GetControllerState();
                
                if (state.Buttons.Count == 0) continue;
                
                for (var i = 0; i < state.Buttons.Count; i++)
                {
                    inputs.Add(new InputTrigger
                    {
                        ContainerId = deviceId,
                        DeviceKind = GameInputKind.Controller, ButtonIndex = i
                    });
                }

                inputs.AddRange(state.Switches.Select(t => new InputTrigger { ContainerId = deviceId, DeviceKind = GameInputKind.Controller, SwitchPosition = t }));
            }
            
            return inputs;
            
        }
        catch (Exception e)
        {
            // @TODO Add proper logging here.
        }
        return null;
    }
}