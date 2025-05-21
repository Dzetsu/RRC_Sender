using System.Security.Cryptography;
using System.Text;

namespace RRC_Sender.Services.BusinessLogic;

public class TokenGenerator
{
    public string Generate(string username, string nameItem, long amount)
    {
        using var sha256 = SHA256.Create();
        var input = $"{username}{nameItem}{amount}{DateTime.Now.Second}";
        var inputBytes = Encoding.UTF8.GetBytes(input);
        var hashBytes = sha256.ComputeHash(inputBytes);
        string token = Convert.ToBase64String(hashBytes);
        return token;
    }
}