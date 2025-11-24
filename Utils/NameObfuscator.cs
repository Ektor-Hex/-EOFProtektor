using System;
using System.Text;

namespace EOFProtektor.Utils
{
    public static class NameObfuscator
    {
        public static string GenerateObfuscatedName(string baseName, int seed)
        {
            var rng = new Random(seed);
            var chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_";
            var result = new StringBuilder();
            
            var prefixes = new[] { "Init", "Setup", "Config", "Helper", "Util", "Core", "Base" };
            result.Append(prefixes[rng.Next(prefixes.Length)]);
            
            for (int i = 0; i < 8; i++)
            {
                result.Append(chars[rng.Next(chars.Length)]);
            }
            
            return result.ToString();
        }
    }
}