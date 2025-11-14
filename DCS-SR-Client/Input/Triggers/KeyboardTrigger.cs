using System;

namespace Ciribob.DCS.SimpleRadio.Standalone.Client.Input;

public class KeyboardTrigger : InputTrigger, IEquatable<KeyboardTrigger>
{
    public byte Virtualkey { get; set; }

    public bool Equals(KeyboardTrigger other)
    {
        if (other is null) return false;
       
        return Virtualkey == other.Virtualkey;
    }

    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        if (obj.GetType() != GetType()) return false;
        return Equals((KeyboardTrigger)obj);
    }

    public override int GetHashCode()
    {
        return Virtualkey.GetHashCode();
    }
    
    public static bool operator ==(KeyboardTrigger left, KeyboardTrigger right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(KeyboardTrigger left, KeyboardTrigger right)
    {
        return !Equals(left, right);
    }

    public override string ToString()
    {
        return $"{base.ToString()} {Virtualkey}";
    }
}