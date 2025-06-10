using System;
using System.Diagnostics.CodeAnalysis;

namespace BliveHelper.Utils.Structs
{
    public struct Color : IEquatable<Color>
    {
        public static readonly Color Black = new Color(0, 0, 0);
        public static readonly Color White = new Color(255, 255, 255);

        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }
        public byte A { get; set; }

        public Color(byte r, byte g, byte b, byte a = 255)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public override bool Equals([NotNullWhen(true)] object obj) => obj is Color other && Equals(other);
        public bool Equals(Color other) => R == other.R && G == other.G && B == other.B && A == other.A;
        public override int GetHashCode()
        {
            var hash = 17L;
            hash = hash * 31 + R.GetHashCode();
            hash = hash * 31 + G.GetHashCode();
            hash = hash * 31 + B.GetHashCode();
            hash = hash * 31 + A.GetHashCode();
            return (int)hash;
        }
    }
}
