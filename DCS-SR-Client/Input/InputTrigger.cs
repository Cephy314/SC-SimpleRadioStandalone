using System;
using GameInputDotNet;
using GameInputDotNet.Interop.Enums;

namespace Ciribob.DCS.SimpleRadio.Standalone.Client.Input;

/// <summary>
/// Generic Input Trigger that identifies the device, kind and numeric value for the button or key.
/// </summary>
public class InputTrigger
{
    /// <summary>
    /// The Container ID from GameInput.Net to avoid many device ID.
    /// </summary>
    public Guid ContainerId { get; set; }
    
    /// <summary>
    /// Type of input device
    /// </summary>
    public GameInputKind DeviceKind { get; set; }
    
    /// <summary>
    /// Type of input (Key, Button, Switch)
    /// </summary>
    public int InputKind { get; set; }
    
    /// <summary>
    /// Value representing the button or key index
    /// </summary>
    public int ButtonIndex { get; set; }
    
    /// <summary>
    /// Keycode for key handling.  Use Virtual for sanity
    /// </summary>
    public uint VirtualKeyCode { get; set; }
    
    /// <summary>
    /// Switch position.
    /// </summary>
    public GameInputSwitchPosition SwitchPosition { get; set; }
}

internal enum InputTriggerKind
{
    Unknown = 0,
    Button = 1,
    VirtualKey = 2,
    SwitchPosition = 3,
}