// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// See README.txt for a link to the original source code.

using System;
using System.Linq;
using System.Text;

namespace TheDailyWtf.Common
{
    public static partial class Html
    {
        // Package atom provides integer codes (also known as atoms) for a fixed set of
        // frequently occurring HTML strings: tag names and attribute keys such as "p"
        // and "id".
        //
        // Sharing an atom's name between all elements with the same tag can result in
        // fewer string allocations when tokenizing and parsing HTML. Integer
        // comparisons are also generally faster than string comparisons.
        //
        // The value of an atom's particular code is not guaranteed to stay the same
        // between versions of this package. Neither is any ordering guaranteed:
        // whether atom.H1 < atom.H2 may also change. The codes are not guaranteed to
        // be dense. The only guarantees are that e.g. looking up "div" will yield
        // atom.Div, calling atom.Div.String will return "div", and atom.Div != 0.
        public static partial class Atom
        {
            // Atom is an integer code for a string. The zero value maps to "".
            public struct AtomType : IEquatable<AtomType>, IEquatable<uint>
            {
                public AtomType(uint value)
                {
                    this.Value = value;
                }

                public uint Value { get; }

                // ToString returns the atom's name.
                public override string ToString()
                {
                    var start = this.Value >> 8;
                    var n = this.Value & 0xff;
                    if (start + n > atomText.Length)
                    {
                        return "";
                    }
                    return atomText.Substring((int)start, (int)n);
                }

                internal string ToStringUnsafe()
                {
                    return atomText.Substring((int)(this.Value >> 8), (int)(this.Value & 0xff));
                }

                bool IEquatable<AtomType>.Equals(AtomType other)
                {
                    return this.Value == other.Value;
                }

                bool IEquatable<uint>.Equals(uint other)
                {
                    return this.Value == other;
                }

                public override int GetHashCode()
                {
                    return (int)this.Value;
                }

                public override bool Equals(object obj)
                {
                    if (obj is AtomType a)
                    {
                        return this.Value == a.Value;
                    }
                    if (obj is uint i)
                    {
                        return this.Value == i;
                    }
                    return false;
                }

                public static implicit operator uint(AtomType a)
                {
                    return a.Value;
                }

                public static implicit operator AtomType(uint a)
                {
                    return new AtomType(a);
                }

                public static bool operator ==(AtomType a, AtomType b)
                {
                    return a.Value == b.Value;
                }

                public static bool operator !=(AtomType a, AtomType b)
                {
                    return a.Value != b.Value;
                }
            }

            // fnv computes the FNV hash with an arbitrary starting value h.
            private static uint fnv(uint h, byte[] s)
            {
                foreach (var b in s)
                {
                    h ^= b;
                    h *= 16777619;
                }
                return h;
            }

            private static bool match(string s, byte[] t)
            {
                return Encoding.UTF8.GetBytes(s).SequenceEqual(t);
            }

            // Lookup returns the atom whose name is s. It returns zero if there is no
            // such atom. The lookup is case sensitive.
            public static AtomType Lookup(byte[] s)
            {
                if (s.Length == 0 || s.Length > maxAtomLen)
                {
                    return 0;
                }
                var h = fnv(hash0, s);
                AtomType a = table[h & (uint)(table.Length - 1)];
                if ((a&0xff) == s.Length && match(a.ToStringUnsafe(), s))
                {
                    return a;
                }
                a = table[(h >> 16) & (uint)(table.Length - 1)];
                if ((a&0xff) == s.Length && match(a.ToStringUnsafe(), s))
                {
                    return a;
                }
                return 0;
            }

            // String returns a string whose contents are equal to s. In that sense, it is
            // equivalent to string(s) but may be more efficient.
            public static string String(byte[] s)
            {
                /*
                var a = Lookup(s);
                if (a != 0)
                {
                    return a.ToString()
                }
                */
                return Encoding.UTF8.GetString(s);
            }
        }
    }
}
