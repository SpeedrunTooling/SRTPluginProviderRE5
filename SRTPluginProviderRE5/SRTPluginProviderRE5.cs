using SRTPluginBase;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace SRTPluginProviderRE5
{
    public class SRTPluginProviderRE5 : IPluginProvider
    {
        private Process process;
        private GameMemoryRE5Scanner gameMemoryScanner;
        private Stopwatch stopwatch;
        private IPluginHostDelegates hostDelegates;
        private GameVersion gameVersion;
        public IPluginInfo Info => new PluginInfo();
        public bool GameRunning
        {
            get
            {
                if (gameMemoryScanner != null && !gameMemoryScanner.ProcessRunning)
                {
                    process = GetProcess();
                    if (process != null)
                        gameMemoryScanner.Initialize(process, gameVersion); // Re-initialize and attempt to continue.
                }

                return gameMemoryScanner != null && gameMemoryScanner.ProcessRunning;
            }
        }

        private static readonly byte[] re5dx9WW_20200922_1 = new byte[32] { 0xF9, 0xD5, 0x04, 0x6D, 0x3C, 0x19, 0xC2, 0xDD, 0xE7, 0xB5, 0xAB, 0xC5, 0x11, 0x4A, 0x04, 0x2D, 0x6D, 0x36, 0xE7, 0x0E, 0x3F, 0xA2, 0x9D, 0x79, 0xDC, 0x53, 0x36, 0xD6, 0xE0, 0x3A, 0x0C, 0x1F }; // Steam WW RTM
        private static readonly byte[] re5dx9WW_20230807_1 = new byte[32] { 0x75, 0x17, 0x26, 0xF0, 0xEC, 0x8B, 0xD0, 0x1C, 0x00, 0xB0, 0x37, 0xAF, 0x8A, 0xE6, 0x4C, 0xDF, 0x0B, 0xC5, 0xCC, 0x5B, 0x90, 0x25, 0xF3, 0xF4, 0xD7, 0x82, 0xEF, 0x6F, 0x66, 0xE0, 0x4E, 0x26 };

        public int Startup(IPluginHostDelegates hostDelegates)
        {
            this.hostDelegates = hostDelegates;
            process = GetProcess();
            if (process != default)
            {
                string? filePath = process?.MainModule?.FileName;
                byte[] checksum;
                using (SHA256 hashFunc = SHA256.Create())
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
                    checksum = hashFunc.ComputeHash(fs);
                if (checksum.SequenceEqual(re5dx9WW_20200922_1))
                {
                    gameVersion = GameVersion.STEAM_1_1_0;
                }
                else if (checksum.SequenceEqual(re5dx9WW_20230807_1))
                {
                    gameVersion = GameVersion.STEAM_1_2_0;
                }
                gameMemoryScanner = new GameMemoryRE5Scanner(process, gameVersion);
                stopwatch = new Stopwatch();
                stopwatch.Start();
            }
            return 0;
        }

        public int Shutdown()
        {
            gameMemoryScanner?.Dispose();
            gameMemoryScanner = null;
            stopwatch?.Stop();
            stopwatch = null;
            return 0;
        }

        public object PullData()
        {
            try
            {
                if (!GameRunning) // Not running? Bail out!
                        return null;

                if (stopwatch.ElapsedMilliseconds >= 2000L)
                {
                    gameMemoryScanner.UpdatePointers();
                    stopwatch.Restart();
                }

                return gameMemoryScanner.Refresh(gameVersion);
            }
            catch (Win32Exception ex)
            {
                if ((ProcessMemory.Win32Error)ex.NativeErrorCode != ProcessMemory.Win32Error.ERROR_PARTIAL_COPY)
                    hostDelegates.ExceptionMessage(ex);// Only show the error if its not ERROR_PARTIAL_COPY. ERROR_PARTIAL_COPY is typically an issue with reading as the program exits or reading right as the pointers are changing (i.e. switching back to main menu).
            }
            catch (Exception ex)
            {
                hostDelegates.ExceptionMessage(ex);
            }

            return null;
        }

        private Process GetProcess() => Process.GetProcessesByName("re5dx9")?.FirstOrDefault();
    }
}
