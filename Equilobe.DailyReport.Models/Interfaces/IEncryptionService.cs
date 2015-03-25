using System;
namespace Equilobe.DailyReport.Models.Interfaces
{
    public interface IEncryptionService : IService
    {
        string Decrypt(string encryptedString);
        string Encrypt(string inputString);
    }
}
