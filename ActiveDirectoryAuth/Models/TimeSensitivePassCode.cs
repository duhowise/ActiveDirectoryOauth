using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace ActiveDirectoryAuth.Models
{
    public static class TimeSensitivePassCode
    {
        public static string GeneratePresharedKey()
        {
            byte[] key = new byte[10]; // 80 bits
            using (var rngProvider = new RNGCryptoServiceProvider())
            {
                rngProvider.GetBytes(key);
            }
 
            return key.ToBase32String();
        }
        public static IList<string> GetListOfOtPs(string base32EncodedSecret)
        {
            DateTime epochStart = new DateTime(1970, 01, 01, 0, 0, 0, 0, DateTimeKind.Utc);
 
            long counter = (long)Math.Floor((DateTime.UtcNow - epochStart).TotalSeconds / 30);
            var otps = new List<string>();
 
            otps.Add(GetHotp(base32EncodedSecret, counter - 1));
            otps.Add(GetHotp(base32EncodedSecret, counter)); 
            otps.Add(GetHotp(base32EncodedSecret, counter + 1)); 
 
            return otps;
        }
 
        private static string GetHotp(string base32EncodedSecret, long counter)
        {
            byte[] message = BitConverter.GetBytes(counter).Reverse().ToArray(); //Intel machine (little endian) 
            byte[] secret = base32EncodedSecret.ToByteArray();
 
            HMACSHA1 hmac = new HMACSHA1(secret, true);
 
            byte[] hash = hmac.ComputeHash(message);
            int offset = hash[hash.Length - 1] & 0xf;
            int truncatedHash = ((hash[offset] & 0x7f) << 24) |
                                ((hash[offset + 1] & 0xff) << 16) |
                                ((hash[offset + 2] & 0xff) << 8) |
                                (hash[offset + 3] & 0xff);
 
            int hotp = truncatedHash % 1000000; 
            return hotp.ToString().PadLeft(6, '0');
        }
    }
}