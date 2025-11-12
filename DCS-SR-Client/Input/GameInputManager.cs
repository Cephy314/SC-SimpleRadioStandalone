using System;
using System.Collections.Generic;

using System.Threading;
using System.Threading.Tasks;
using Ciribob.DCS.SimpleRadio.Standalone.Common.Settings;
using GameInputDotNet;
using GameInputDotNet.Interop.Enums;
using NLog;

namespace Ciribob.DCS.SimpleRadio.Standalone.Client.Input;

/// <summary>
/// Handle game input and trigger events for matching bindings, as well as the storing of bindings.
/// </summary>
public class GameInputManager
{
    private Logger _logger = LogManager.GetCurrentClassLogger();
    private readonly TimeSpan _interval;
    private CancellationTokenSource _cancellationTokenSource;
    
    
    /// <summary>
    /// Buffer to contain the inputs captured in a comparable class form.
    /// </summary>
    
    private readonly List<InputTrigger> _inputBuffer = [];
    /// <summary>
    /// Relationship of Commands to Bindings for quick unique lookup.
    /// </summary>
    
    private Dictionary<InputBinding, GameInputBinding> _inputBindings = [];
    /// <summary>
    /// Manage input handling through Microsoft GameInput.
    /// </summary>
    /// <param name="interval">Frequency of input polling.</param>
    
    public GameInputManager(TimeSpan interval)
    {
        _interval = interval;
    }

    /// <summary>
    /// Start async periodic task to read inputs and raise input events
    /// </summary>
    public void Start()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        _ = RunPeriodicTaskAsync(_cancellationTokenSource.Token);
    }

    /// <summary>
    /// Stop periodic events and all input polling.
    /// </summary>
    public void Stop()
    {
        _cancellationTokenSource?.Cancel();
    }

    /// <summary>
    /// Internal task to handle periodic timer.
    /// </summary>
    /// <param name="cancellationToken"></param>
    private async Task RunPeriodicTaskAsync(CancellationToken cancellationToken)
    {
        using var timer = new PeriodicTimer(_interval);
        try
        {
            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                ProcessInput();
            }
        }
        catch (OperationCanceledException e)
        {
            // Graceful exit!
        }
        catch (Exception e)
        {
            // Not graceful...
            _logger.Error(e);
        }
    }

    /// <summary>
    /// Process all input via GameInput.Net
    /// </summary>
    private void ProcessInput()
    {
        using var gameInput = GameInput.Create();

        _inputBuffer.Clear();

        try
        {
            using (var reading = gameInput.GetCurrentReading(GameInputKind.Keyboard))
            {
                if (reading != null)
                {
                    for (var i = 0; i < reading.GetKeyboardState().Keys.Count; i++)
                    {
                        _inputBuffer.Add(new KeyboardTrigger
                        {
                            Type = InputTriggerType.Keyboard, Virtualkey = reading.GetKeyboardState().Keys[i].VirtualKey
                        });
                    }
                }
            }

            using (var reading = gameInput.GetCurrentReading(GameInputKind.Mouse))
            {
                if (reading != null)
                {
                    var buttons = reading.GetMouseState().Buttons;
                    foreach (var button in Enum.GetValues<GameInputMouseButtons>())
                    {
                        if (buttons.HasFlag(button))
                        {
                            _inputBuffer.Add(new MouseButtonTrigger
                                { Type = InputTriggerType.MouseButton, Button = button });
                        }
                    }

                    // @TODO Add support for mouse wheel movements.
                }
            }

            var devices = gameInput.EnumerateDevices(GameInputKind.Controller);

            for (var i = 0; i < devices.Count; i++)
            {
                var device = devices[i];
                var deviceId = device.GetDeviceInfo().DeviceId;
                using (var reading = gameInput.GetCurrentReading(GameInputKind.Controller, device))
                {
                    if (reading == null) continue;

                    var state = reading.GetControllerState();
                    var buttons = state.Buttons;
                    var switches = state.Switches;

                    // Iterate through buttons
                    for (byte j = 0; j < buttons.Count; j++)
                    {
                        if (!buttons[j]) continue;

                        _inputBuffer.Add(new ControllerButtonTrigger
                            { Type = InputTriggerType.ControllerButton, Id = deviceId, Button = j });
                    }

                    // Interate through switches
                    for (byte j = 0; j < switches.Count; j++)
                    {
                        if (switches[j] == GameInputSwitchPosition.Center) continue;

                        _inputBuffer.Add(new ControllerSwitchTrigger
                        {
                            Type = InputTriggerType.ControllerSwitch, Id = deviceId, Index = j, Position = switches[j]
                        });
                    }
                }
            }
        }
        catch (Exception e)
        {
            _logger.Error($"Error processing inputs: {e.Message}");
        }
    }

    /// <summary>
    /// Iterates over bound <see cref="InputBinding" /> and raises events when active.
    /// </summary>
    private void ProcessBoundEvents()
    {
        foreach (var commandBinding in _inputBindings)
        {
            if (commandBinding.Value != null) continue;

            if (IsBindingActivated(commandBinding.Value))
            {
                // TODO: Raise event for command.
            }
        }

    }

    /// <summary>
    /// Detect if a binding has all of its inputs activated.
    /// </summary>
    /// <param name="binding"><see cref="GameInputBinding"/> containing the function and the inputs that it is bound to</param>
    /// <returns>TRUE if the inputs are activated</returns>
    private bool IsBindingActivated(GameInputBinding binding)
    {
        // Ensure GameInputBinding and Input property are not null
        if (binding?.Input == null) return false;
        
        // Is our input activated in this frame.
        var input = _inputBuffer.Contains(binding.Input);

        // If we don't have a modifier then the input is all that matters.
        if (binding.Modifier == null) return input;

        return _inputBuffer.Contains(binding.Modifier) && input;
    }
}