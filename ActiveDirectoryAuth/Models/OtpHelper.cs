﻿using System.Linq;
using System.Net.Http;

namespace ActiveDirectoryAuth.Models
{
    public static class OtpHelper
    {
        private const string OTP_HEADER = "X-OTP";

        public static bool HasValidTotp(this HttpRequestMessage request, string key)
        {
            if (request.Headers.Contains(OTP_HEADER))
            {
                string otp = request.Headers.GetValues(OTP_HEADER).First();
                
                // We need to check the passcode against the past, current, and future passcodes
               
                if (!string.IsNullOrWhiteSpace(otp))
                {
                    if (TimeSensitivePassCode.GetListOfOtPs(key).Any(t => t.Equals(otp)))
                    {
                        return true;
                    }
                }

            }
            return false;
        }
    }
}