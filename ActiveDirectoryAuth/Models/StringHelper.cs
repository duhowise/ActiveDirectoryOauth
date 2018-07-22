using System;
using System.Linq;

namespace ActiveDirectoryAuth.Models
{
    public static class StringHelper
    {
	    private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

	    public static string ToBase32String(this byte[] secret)
        {
            var bits = secret.Select(b => Convert.ToString((byte) b, 2).PadLeft(8, '0')).Aggregate((a, b) => a + b);
 
            return Enumerable.Range(0, bits.Length / 5).Select(i => Alphabet.Substring(Convert.ToInt32(bits.Substring(i * 5, 5), 2), 1)).Aggregate((a, b) => a + b);
        }
 
        public static byte[] ToByteArray(this string secret)
        {
            var bits = secret.ToUpper().ToCharArray().Select(c => Convert.ToString(Alphabet.IndexOf(c), 2).PadLeft(5, '0')).Aggregate((a, b) => a + b);
 
            return Enumerable.Range(0, bits.Length / 8).Select(i => Convert.ToByte(bits.Substring(i * 8, 8), 2)).ToArray();
        }
 
    }
}