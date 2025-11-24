using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using EOFProtektor.Core;

namespace EOFProtektor.Protection
{
    public static class IntegrityProtection
    {
        public static void ApplyCustomPatch(string filePath, byte[] patchData, ProtectionData data)
        {
            Console.WriteLine("Aplicando patch personalizado...");
            
            using (var fs = new FileStream(filePath, FileMode.Append, FileAccess.Write))
            {
                // Escribir marcador de inicio
                var startMarkerBytes = Encoding.UTF8.GetBytes(data.StartMarker);
                fs.Write(startMarkerBytes, 0, startMarkerBytes.Length);
                
                // Escribir datos del patch
                fs.Write(patchData, 0, patchData.Length);
                
                // Calcular y escribir checksum
                using (var sha256 = SHA256.Create())
                {
                    var checksum = sha256.ComputeHash(patchData);
                    fs.Write(checksum, 0, checksum.Length);
                }
                
                // Escribir marcador de fin
                var endMarkerBytes = Encoding.UTF8.GetBytes(data.EndMarker);
                fs.Write(endMarkerBytes, 0, endMarkerBytes.Length);
            }
            
            Console.WriteLine("✓ Patch personalizado aplicado");
        }
        
        public static void ApplyMultiLayerProtection(string filePath, ProtectionData data)
        {
            Console.WriteLine("Aplicando protección multicapa...");
            
            var fileBytes = File.ReadAllBytes(filePath);
            
            using (var fs = new FileStream(filePath, FileMode.Append, FileAccess.Write))
            {
                using (var sha256 = SHA256.Create())
                {
                    // Hash completo del archivo
                    var fullHash = sha256.ComputeHash(fileBytes);
                    fs.Write(fullHash, 0, fullHash.Length);
                    
                    // Hash parcial (primeros 1024 bytes)
                    var partialBytes = new byte[Math.Min(1024, fileBytes.Length)];
                    Array.Copy(fileBytes, partialBytes, partialBytes.Length);
                    var partialHash = sha256.ComputeHash(partialBytes);
                    fs.Write(partialHash, 0, partialHash.Length);
                    
                    // Hash de metadatos (últimos 512 bytes)
                    var metadataStart = Math.Max(0, fileBytes.Length - 512);
                    var metadataBytes = new byte[fileBytes.Length - metadataStart];
                    Array.Copy(fileBytes, metadataStart, metadataBytes, 0, metadataBytes.Length);
                    var metadataHash = sha256.ComputeHash(metadataBytes);
                    fs.Write(metadataHash, 0, metadataHash.Length);
                }
                
                // Clave de validación ofuscada
                var obfuscatedKey = ObfuscateValidationKey(data.ValidationKey, data.Seed);
                var keyBytes = Encoding.UTF8.GetBytes(obfuscatedKey);
                fs.Write(keyBytes, 0, keyBytes.Length);
            }
            
            Console.WriteLine("✓ Protección multicapa aplicada");
        }
        
        private static string ObfuscateValidationKey(string key, int seed)
        {
            var rng = new Random(seed);
            var result = new StringBuilder();
            
            for (int i = 0; i < key.Length; i++)
            {
                var obfuscatedChar = (char)(key[i] ^ rng.Next(1, 256));
                result.Append(((int)obfuscatedChar).ToString("X2"));
            }
            
            return result.ToString();
        }
    }
}