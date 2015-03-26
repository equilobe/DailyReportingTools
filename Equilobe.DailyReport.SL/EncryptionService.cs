using Encryptamajig;
using Equilobe.DailyReport.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.SL
{
    public class EncryptionService : IEncryptionService
    {
        public IConfigurationService ConfigurationService { get; set; }

        public string Encrypt(string inputString)
        {
            return AesEncryptamajig.Encrypt(inputString, ConfigurationService.GetEncriptedKey());
        }

        public string Decrypt(string encryptedString)
        {
            return AesEncryptamajig.Decrypt(encryptedString, ConfigurationService.GetEncriptedKey());
        }
    }
}
