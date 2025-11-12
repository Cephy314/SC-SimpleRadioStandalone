namespace Ciribob.DCS.SimpleRadio.Standalone.Client.Input;

/// <summary>
/// Enum value for the kind of input trigger used.
/// </summary>
public enum InputTriggerType : ushort
{
    Unknown = 0,
    Keyboard = 10,
    MouseButton = 20,
    ControllerButton = 30,
    ControllerSwitch = 40,
    GamePadButton = 50,
}