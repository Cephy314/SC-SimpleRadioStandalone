using Ciribob.DCS.SimpleRadio.Standalone.Common.Settings;

namespace Ciribob.DCS.SimpleRadio.Standalone.Client.Input;

/// <summary>
/// Binding Record 
/// </summary>
public class GameInputBinding
{
    /// <summary>
    /// Command that is triggered
    /// </summary>
    public InputBinding Binding { get; set; }
    
    /// <summary>
    /// Primary combination of inputs
    /// </summary>
    public InputTrigger Input { get; set; }
    
    /// <summary>
    /// Secondary combination of inputs
    /// </summary>
    public InputTrigger Modifier { get; set; }

    /// <summary>
    /// Initialize Generic Binding, JSON Friendly
    /// </summary>
    public GameInputBinding()
    {
    }
}