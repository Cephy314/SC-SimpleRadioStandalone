using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GameInputDotNet;
using GameInputDotNet.Interop;
using GameInputDotNet.Interop.Enums;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using InputBinding = Ciribob.DCS.SimpleRadio.Standalone.Common.Settings.InputBinding;

namespace Ciribob.DCS.SimpleRadio.Standalone.Client.Input;

/// <summary>
/// Handle game input and trigger events for matching bindings, as well as the storing of bindings.
/// </summary>
public sealed class GameInputManager : IGameInputManager
{
    /// <summary>
    /// Raised when changes in bindings are detected such as pressed or released.
    /// </summary>
    public event EventHandler<List<GameInputBinding>> InputBindingChange;
    
    /// <summary>
    /// Helper field holding the desired generic input types
    /// </summary>
    private const GameInputKind GenericInputKinds =  GameInputKind.Gamepad | GameInputKind.Mouse | GameInputKind.Keyboard;

    /// <summary>
    /// Logger Instance
    /// </summary>
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Interval between input polling
    /// </summary>
    private readonly TimeSpan _interval;

    private bool _raiseEvents = true;

    /// <summary>
    /// Cancellation token to handle async interrupts 
    /// </summary>
    private CancellationTokenSource _cancellationTokenSource;

    /// <summary>
    /// GameInput.Net wrapper on Microsoft.GameInput
    /// </summary>
    private readonly GameInput _gameInput;

    /// <summary>
    /// Last GameInput reading to use as index for continued reading each tick.
    /// </summary>
    private GameInputReading _lastReading;
    private readonly Dictionary<AppLocalDeviceId, GameInputReading> _controllerReadings = new();

    /// <summary>
    /// Buffer to contain the inputs captured in a comparable class form.
    /// </summary>
    private readonly HashSet<InputTrigger> _inputBuffer = [];
    
    private BindingProfile _profile;
    
    /// <summary>
    /// Holds the last frames active bindings for comparison.
    /// </summary>
    private HashSet<GameInputBinding> _activeBindings = [];

    /// <summary>
    /// Buffer of active bindings populated in the current frame.
    /// </summary>
    private readonly HashSet<GameInputBinding> _activeBindingsBuffer = [];
    
    /// <summary>
    /// Reference to the BindingManager so we may subscribe to events like binding changes.
    /// </summary>
    private readonly IBindingManager _bindingManager;
    
    /// <summary>
    /// Manage input handling through Microsoft GameInput.
    /// </summary>
    /// <param name="interval">Frequency of input polling.</param>
    public GameInputManager()
    {
        _gameInput = GameInput.Create();

        // TODO: Get interval from some internal configuration, dont hard code.
        _interval = new TimeSpan(50);

        // TODO: Should be passed as a service injection so accessible by others.  Temp for testing.
        _bindingManager = App.Services.GetRequiredService<IBindingManager>();
    }

    /// <summary>
    /// Start async periodic task to read inputs and raise input events
    /// </summary>
    /// <returns>TRUE if started successfully, FALSE if there is an error.</returns>
    public bool Start()
    {
        // No point in running without a profile set.
        if (_profile == null)
        {
            _logger.Error("Cannot start game input manager with no profile set");
            return false;
        }
        
        _cancellationTokenSource = new CancellationTokenSource();
        _ = RunPeriodicTaskAsync(_cancellationTokenSource.Token);
        return true;
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
        catch (OperationCanceledException)
        {
            // Graceful exit!
        }
        catch (Exception e)
        {
            // Not graceful...
            _logger.Error(e);
        }
    }

    public async Task<bool> CaptureBinding(InputBinding binding, int timeOut)
    {
        // TODO: Replace this with more elegant binding system.
        // Figure out if we have a modifier or normal binding.
        var isModifier = (int)binding >= 200 ?  true : false;
        
        // How long will we wait for the 
        var maxElapsed = new TimeSpan(timeOut);
        var start = DateTime.Now;
        _raiseEvents = false;

        // Wait for an input from the main loop. or the time to run out.
        while (true)
        {
            await Task.Delay(10);

            InputTrigger input;
            if (DateTime.Now - start > maxElapsed)
            {
                input = null;
                return false;
            }

            if (_inputBuffer.Count == 0)
            {
                continue;
            }

            input = _inputBuffer.First();
            _bindingManager.SetBinding(binding, input);
            return true;
        }
    }

    /// <summary>
    /// Gets the binding associated with a <see cref="InputBinding"/>
    /// </summary>
    /// <param name="binding">The binding you want to retreive</param>
    /// <returns>A <see cref="GameInputBinding"/> or null if not found.</returns>
    public GameInputBinding GetBinding(InputBinding binding)
    {
        if (_profile == null)
        {
            _logger.Error("You must load an input profile before getting a binding.");
            return null;
        }
        
        return _profile.Bindings.TryGetValue(binding, out var input) ? input : null;
    }
    
    /// <summary>
    /// Process all input via GameInput.Net
    /// </summary>
    private void ProcessInput()
    {
        _inputBuffer.Clear();
        ProcessGenericDevices();
        ProcessControllerDevices();
        ProcessBindings();
    }

    /// <summary>
    /// Process each controller device with its own readings to create input/device association
    /// </summary>
    private void ProcessControllerDevices()
    {
        // Handle controllers differently as we want to attach their ID to their binds since we want distinct
        // input from multiple controllers.
        var devices = _gameInput.EnumerateDevices(GameInputKind.Controller);

        for (var i = 0; i < devices.Count; i++)
        {
            var device = devices[i];
            var deviceId = device.GetDeviceInfo().DeviceId;
            // Loop through readings for the device.
            GameInputReading reading;
            if (_controllerReadings.TryGetValue(deviceId, out var value))
            {
                reading = _gameInput.GetNextReading(value, GameInputKind.Controller, device);
            }
            else
            {
                reading = _gameInput.GetCurrentReading(GameInputKind.Controller, device);
            }
            
            // Skip if the reading is the same or null.
            if (reading == null || reading == _controllerReadings[deviceId]) continue;

            while (reading != null)
            {
                var state = reading.GetControllerState();
                var buttons = state.Buttons;
                var switches = state.Switches;

                // Iterate through buttons
                for (byte j = 0; j < buttons.Count; j++)
                {
                    if (!buttons[j]) continue;

                    _inputBuffer.Add(new ControllerButtonTrigger
                    {
                        Type = InputTriggerType.ControllerButton, Id = deviceId, Button = j
                    });
                }

                // Iterate through switches
                for (byte j = 0; j < switches.Count; j++)
                {
                    if (switches[j] == GameInputSwitchPosition.Center) continue;

                    _inputBuffer.Add(new ControllerSwitchTrigger
                    {
                        Type = InputTriggerType.ControllerSwitch, Id = deviceId, Index = j, Position = switches[j]
                    });
                }
                
                // CRITICAL must dispose as it holds refs to unmanaged objects, will leak badly if we don't clean.
                _controllerReadings[deviceId].Dispose();
                _controllerReadings[deviceId] = reading;
                reading = _gameInput.GetNextReading(_controllerReadings[deviceId], GameInputKind.Controller, device);
            }
        }
    }

    /// <summary>
    /// Process generic devices that do not need input associated, we don't care if the key comes from different keyboards.
    /// </summary>
    private void ProcessGenericDevices()
    {
        GameInputReading reading;
        if (_lastReading == null)
        {
            reading = _gameInput.GetCurrentReading(GenericInputKinds);
        }
        else
        {
            reading = _gameInput.GetNextReading(_lastReading,  GenericInputKinds);
        }
        
        if (reading == null || reading == _lastReading) return;
        
        // Go through all readings since we last polled GameInput
        while(reading != null)
        {
            // Gather keyboard inputs
            for (var i = 0; i < reading.GetKeyboardState().Keys.Count; i++)
            {
                _inputBuffer.Add(new KeyboardTrigger
                {
                    Type = InputTriggerType.Keyboard, Virtualkey = reading.GetKeyboardState().Keys[i].VirtualKey
                });
            }
            
            // Gather mouse button readings
            var mouseButtons = reading.GetMouseState()?.Buttons ?? GameInputMouseButtons.None; 
            foreach (var button in Enum.GetValues<GameInputMouseButtons>())
            {
                if (button == 0 || mouseButtons == 0) continue; // Skip "None" value
                if (mouseButtons.HasFlag(button))
                {
                    _inputBuffer.Add(new MouseButtonTrigger { Type = InputTriggerType.MouseButton, Button = button });
                }
            }
            
            // Gather GamePad readings
            var gamepadButtons = reading.GetGamepadState()?.Buttons  ?? GameInputGamepadButtons.None;
            foreach (var button in Enum.GetValues<GameInputGamepadButtons>())
            {
                if (button == 0 || gamepadButtons == 0) continue;
                if (gamepadButtons.HasFlag(button))
                {
                    _inputBuffer.Add( new GamePadButtonTrigger
                    {
                        Type = InputTriggerType.GamePadButton, 
                        Button = button
                    });
                }
            }

            // @TODO Add support for mouse wheel movements.
            
            // Dispose of old reading and save new reading as last.
            // CRITICAL must dispose as it holds refs to unmanaged objects, will leak badly if we don't clean.
            _lastReading?.Dispose();
            _lastReading = reading;
            
            // Get the next reading in the GameInput buffer. When there are no more we will get null
            reading = _gameInput.GetNextReading(_lastReading,GenericInputKinds);
        }
    }
    /// <summary>
    /// Iterates over bound <see cref="InputBinding" /> and raises events when active.
    /// </summary>
    private void ProcessBindings()
    {
        // Allow for temporary suspension of event raising.
        if (!_raiseEvents) return;
        if (_profile == null) return;
        
        foreach (var commandBinding in _profile.Bindings)
        {
            _activeBindingsBuffer.Clear();
            
            if (commandBinding.Value != null) continue;

            if (IsBindingActivated(commandBinding.Value))
            {
                _activeBindingsBuffer.Add(commandBinding.Value);
            }
        }
        
        // Compare _activeBindings and _activeBindingsBuffer and see what is new in the buffer and what is missing.
        var addedBindings = _activeBindingsBuffer.Except(_activeBindings).ToList();
        var droppedBindings = _activeBindings.Except(_activeBindingsBuffer).ToList();
        
        // Update the active bindings for the next iteration.
        _activeBindings = _activeBindingsBuffer;

        var change = new List<GameInputBinding>();
        
        // raise events for the new bindings.
        foreach (var binding in droppedBindings)
        {
            binding.IsActive = false;
            change.Add(binding);
        }

        foreach (var binding in addedBindings)
        {
            binding.IsActive = true;
            change.Add(binding);
        }

        OnInputBindingChange(change);
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

    /// <summary>
    /// Cleanup managed objects or we get big leak.
    /// </summary>
    public void Dispose()
    {
        _cancellationTokenSource?.Dispose();
        _gameInput?.Dispose();
        _lastReading.Dispose();
        foreach (var reading in _controllerReadings.Values)
        {
            reading.Dispose();
        }
    }

    private void OnInputBindingChange(List<GameInputBinding> bindings)
    {
        InputBindingChange?.Invoke(this, bindings);
    }
}