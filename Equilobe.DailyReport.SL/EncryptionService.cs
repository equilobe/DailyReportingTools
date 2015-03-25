using Encryptamajig;
using Equilobe.DailyReport.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.SL
{
    public class EncryptionService : IEncryptionService
    {
        public string Encrypt(string inputString)
        {
            return AesEncryptamajig.Encrypt(inputString, GetEncriptedKey());
        }

        public string Decrypt(string encryptedString)
        {
            return AesEncryptamajig.Decrypt(encryptedString, GetEncriptedKey());
        }

        string GetEncriptedKey()
        {
            var byteKey = Convert.FromBase64String(ConfigurationManager.AppSettings["encKey"]);
            var encKey = Encoding.UTF8.GetString(byteKey);
            return encKey;
        }
    }
}
