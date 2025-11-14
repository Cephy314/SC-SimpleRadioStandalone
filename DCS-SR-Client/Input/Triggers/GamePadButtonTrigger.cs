using System;
using GameInputDotNet.Interop.Enums;

namespace Ciribob.DCS.SimpleRadio.Standalone.Client.Input;

public class GamePadButtonTrigger : InputTrigger, IEquatable<GamePadButtonTrigger>
{
    public GameInputGamepadButtons Button { get; set; }

    public bool Equals(GamePadButtonTrigger other)
    {
        if (other is null) return false;

        return base.Equals(other) && Button == other.Button;
    }

    public override bool Equals(object obj)
    {
        if (obj is null) return false;

        return obj.GetType() == GetType() && Equals((GamePadButtonTrigger)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), (int)Button);
    }

    public static bool operator ==(GamePadButtonTrigger left, GamePadButtonTrigger right)
    {
        if (left is null && right is null) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }
    
    public static bool operator !=(GamePadButtonTrigger left, GamePadButtonTrigger right)
    {
        return !(left == right);
    }

    public override string ToString()
    {
        return $"{base.ToString()} {Enum.GetName(Button)}";
    }
}