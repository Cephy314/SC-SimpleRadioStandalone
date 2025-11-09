using System.Collections.Generic;

namespace Ciribob.DCS.SimpleRadio.Standalone.Client.Input;

/// <summary>
/// Binding Record 
/// </summary>
public class Binding
{
    /// <summary>
    /// Command that is triggered
    /// </summary>
    public BindingCommands Command { get; set; }
    
    /// <summary>
    /// Primary combination of inputs
    /// </summary>
    public List<InputTrigger> PrimaryTriggers { get; set; }
    
    /// <summary>
    /// Secondary combination of inputs
    /// </summary>
    public List<InputTrigger> SecondaryTriggers { get; set; }

    /// <summary>
    /// Initialize Generic Binding, JSON Friendly
    /// </summary>
    public Binding()
    {
    }
}