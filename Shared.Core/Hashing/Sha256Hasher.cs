using System.Security.Cryptography;
using System.Text;

namespace Shared.Core.Hashing
{
    public static class Sha256Hasher
    {
        public static async Task<string> ComputeHashAsync(Stream stream)
        {
            using (var sha256 = SHA256.Create())
            {
                if (stream.CanSeek)
                {
                    stream.Position = 0;
                }
                byte[] hashBytes = await sha256.ComputeHashAsync(stream);
                if (stream.CanSeek)
                {
                    stream.Position = 0;
                }
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
        }

        public static string ComputeHash(string content)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(content);
                byte[] hashBytes = sha256.ComputeHash(bytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}