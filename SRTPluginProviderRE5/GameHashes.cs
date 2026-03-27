using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace SRTPluginProviderRE5
{
    /// <summary>
    /// SHA256 hashes for the RE5/BIO5 game executables.
    /// </summary>
    /// 
    public enum GameVersion : int
    {
        Unknown,
        STEAM_GFWL,
        STEAM_1_1_0,
        STEAM_1_2_0
    }
    public static class GameHashes
    {
        private static readonly byte[] re5dx9WW_20200922_1 = new byte[32] { 0xF9, 0xD5, 0x04, 0x6D, 0x3C, 0x19, 0xC2, 0xDD, 0xE7, 0xB5, 0xAB, 0xC5, 0x11, 0x4A, 0x04, 0x2D, 0x6D, 0x36, 0xE7, 0x0E, 0x3F, 0xA2, 0x9D, 0x79, 0xDC, 0x53, 0x36, 0xD6, 0xE0, 0x3A, 0x0C, 0x1F }; // Steam WW RTM
        private static readonly byte[] RE5DX9WW_20090707_1 = new byte[32] { 0x3D, 0x27, 0x99, 0xD3, 0x34, 0xEE, 0x40, 0xE2, 0xBB, 0x28, 0xCA, 0xF8, 0x7C, 0x35, 0xCE, 0xDF, 0xC3, 0x16, 0x2E, 0x78, 0xEE, 0xCB, 0x69, 0x51, 0x0D, 0x83, 0x77, 0xCB, 0x5E, 0x0A, 0x49, 0xA0 };
        private static readonly byte[] re5dx9WW_20230807_1 = new byte[32] { 0x75, 0x17, 0x26, 0xF0, 0xEC, 0x8B, 0xD0, 0x1C, 0x00, 0xB0, 0x37, 0xAF, 0x8A, 0xE6, 0x4C, 0xDF, 0x0B, 0xC5, 0xCC, 0x5B, 0x90, 0x25, 0xF3, 0xF4, 0xD7, 0x82, 0xEF, 0x6F, 0x66, 0xE0, 0x4E, 0x26 };

        public static GameVersion DetectVersion(string filePath)
        {
            byte[] checksum;
            using (SHA256 hashFunc = SHA256.Create())
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
                checksum = hashFunc.ComputeHash(fs);

            if (checksum.SequenceEqual(re5dx9WW_20200922_1))
            {
                Console.WriteLine("1.1.0");
                return GameVersion.STEAM_1_1_0;
            } else if (checksum.SequenceEqual(RE5DX9WW_20090707_1))
            {
                Console.WriteLine("GFWL");
                return GameVersion.STEAM_GFWL;
            } else if(checksum.SequenceEqual(re5dx9WW_20230807_1))
            {
                Console.WriteLine("1.2.0");
                return GameVersion.STEAM_1_2_0;
            }

            Console.WriteLine("Unknown Version");
            return GameVersion.Unknown;
        }
    }
}
