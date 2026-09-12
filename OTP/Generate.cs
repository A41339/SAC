using OtpNet;
using System;
using System.Text;

namespace OTP
{
   
    public static class Generate
    {
        private const string dev_key = "1VJ1zaVBqUf__jCs";
        private const string  secret = "mr_ZqK4wCFr_EwfS";

        public static string GenerateOTP(String id)
        {
            var key = Encoding.UTF8.GetBytes(secret + dev_key + id);
            var otp = new Totp(key);
            var code = otp.ComputeTotp();

            return code;
        }

    }
}
