using System.Collections.Generic;
using Ciribob.DCS.SimpleRadio.Standalone.Common.Settings;

namespace Ciribob.DCS.SimpleRadio.Standalone.Client.Input;

public class BindingProfile
{
    public string ProfileName { get; set; }
    public string ProfileVersion { get; set; } = "1.0";
    public Dictionary<InputBinding, GameInputBinding> Bindings { get; set; } = new();
}