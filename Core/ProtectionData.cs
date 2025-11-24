using System;

namespace EOFProtektor.Core
{
    public class ProtectionData
    {
        public string StartMarker { get; set; } = "";
        public string EndMarker { get; set; } = "";
        public int ChecksumPosition { get; set; }
        public int Seed { get; set; }
        public string ValidationKey { get; set; } = "";

        public ProtectionData()
        {
            var rng = new Random();
            Seed = rng.Next(1000, 99999);
            
            StartMarker = GenerateRandomMarker("START", Seed);
            EndMarker = GenerateRandomMarker("END", Seed + 1);
            ChecksumPosition = rng.Next(100, 500);
            ValidationKey = GenerateValidationKey(Seed);
        }

        private string GenerateRandomMarker(string prefix, int seed)
        {
            var rng = new Random(seed);
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var result = prefix + "_";
            
            for (int i = 0; i < 16; i++)
            {
                result += chars[rng.Next(chars.Length)];
            }
            
            return result;
        }

        private string GenerateValidationKey(int seed)
        {
            var rng = new Random(seed);
            var key = "";
            
            for (int i = 0; i < 32; i++)
            {
                key += rng.Next(0, 256).ToString("X2");
            }
            
            return key;
        }
    }
}