using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace WaterServer.DataAccess;

public static class SecurityUtils
{
    private static HashAlgorithm hashAlgorithm;

    public static string GetPasswordHash(string password, string salt)
    {
        hashAlgorithm ??= SHA256.Create();

        string strToEncode = $"{password} {salt}";
        byte[] strBytes = Encoding.UTF8.GetBytes(strToEncode);
        byte[] hash = hashAlgorithm.ComputeHash(strBytes);
        string result = Convert.ToBase64String(hash);
        return result;
    }
}
