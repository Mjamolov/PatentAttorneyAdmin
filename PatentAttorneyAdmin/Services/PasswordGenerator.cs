using System.Security.Cryptography;

namespace PatentAttorneyAdmin.Services;

public static class PasswordGenerator
{
  private const string Chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789!@#$%";

  public static string Generate(int length = 12)
  {
    var bytes = RandomNumberGenerator.GetBytes(length);
    var result = new char[length];
    for (var i = 0; i < length; i++)
      result[i] = Chars[bytes[i] % Chars.Length];
    return new string(result);
  }
}
