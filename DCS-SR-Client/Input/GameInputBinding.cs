using System;
using Ciribob.DCS.SimpleRadio.Standalone.Common.Settings;

namespace Ciribob.DCS.SimpleRadio.Standalone.Client.Input;

/// <summary>
/// Binding Record 
/// </summary>
public class GameInputBinding : IEquatable<GameInputBinding>
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
    /// Is binding currently pressed.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Initialize Generic Binding, JSON Friendly
    /// </summary>
    public GameInputBinding()
    {
    }

    public bool Equals(GameInputBinding other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Binding == other.Binding;
    }

    public override bool Equals(object obj)
    {
        if (obj is null)
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((GameInputBinding)obj);
    }

    public override int GetHashCode()
    {
        return (int)Binding;
    }
    
    public static bool  operator ==(GameInputBinding left, GameInputBinding right)
    {
        return Equals(left, right);
    }
    
    public static bool operator !=(GameInputBinding left, GameInputBinding right)
    {
        return !Equals(left, right);
    }
}