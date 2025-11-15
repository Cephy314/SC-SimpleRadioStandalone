using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ciribob.DCS.SimpleRadio.Standalone.Common.Settings;

namespace Ciribob.DCS.SimpleRadio.Standalone.Client.Input;

public interface IGameInputManager : IDisposable
{
    event EventHandler<List<GameInputBinding>> InputBindingChange;
    public bool Start();
    public void Stop();
    public Task<bool> CaptureBinding(InputBinding binding, int timeOut);
    public GameInputBinding GetBinding(InputBinding binding);
}