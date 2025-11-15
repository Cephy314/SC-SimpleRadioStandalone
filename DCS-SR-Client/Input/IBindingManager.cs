using System;
using Ciribob.DCS.SimpleRadio.Standalone.Common.Settings;

namespace Ciribob.DCS.SimpleRadio.Standalone.Client.Input;

public interface IBindingManager : IDisposable
{
    public bool SetBinding(InputBinding bind, InputTrigger trigger);
    public bool ClearBinding(InputBinding bind);
    public bool LoadBindingProfile(string profileName);
}