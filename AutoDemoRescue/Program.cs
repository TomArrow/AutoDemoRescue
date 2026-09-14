using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using RunOnStartup;
using Salaros.Configuration;

namespace AutoDemoRescue
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("______________");
            Console.WriteLine("--------------");
            Console.WriteLine("AutoDemoRescue");
            Console.WriteLine("--------------");
            Console.WriteLine("______________");

            ConfigParser config = new ConfigParser("config.ini", new ConfigParserSettings()
            {
                 Culture = CultureInfo.InvariantCulture,
                 MultiLineValues = MultiLineValues.AllowEmptyTopSection
            });

            bool autoStart = false;
            string[] paths = new string[] { };
            try
            {
                autoStart = config.GetValue("main", "autoStart", false);
                paths = config.GetArrayValue("main", "path");
            } catch(Exception ex)
            {
                Console.WriteLine($"Error parsing config: {ex.ToString()}");
                Console.ReadKey();
                return;
            }

            if (paths is null)
            {
                Console.WriteLine($"Error parsing config: paths is null");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("Config successfully parsed. Settings:");
            Console.WriteLine($"Autostart: {autoStart}");

            try
            {
                const string autoStartUniqueId = "AutoDemoRescuer_AutoStart_23t43grdfh4SDFV";
                string currentBinary = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;

                if (autoStart)
                {
                    Console.WriteLine("Enabling autostart.");
                    RunOnStartupManager.Instance.Register(autoStartUniqueId, currentBinary, allUsers: false);
                }
                else
                {
                    if (RunOnStartupManager.Instance.Unregister(autoStartUniqueId, allUsers: false))
                    {
                        Console.WriteLine("Removed autostart.");
                    }
                }
            } catch(Exception ex)
            {
                Console.WriteLine($"Error handling autostart: {ex.ToString()}");
            }

            Console.WriteLine($"Watch paths:");
            List<string> existingPaths = new List<string>();
            foreach(string path in paths)
            {
                if (Directory.Exists(path))
                {
                    Console.WriteLine($"{path}");
                    existingPaths.Add(path);
                }
                else
                {
                    Console.WriteLine($"{path} (does not exist)");
                }
            }

            if(existingPaths.Count == 0)
            {
                Console.WriteLine($"None of the provided paths exist. Quitting.");
                Console.ReadKey();
                return;
            }

            List<FileSystemWatcher> watchers = new List<FileSystemWatcher>();

            foreach(string path in existingPaths)
            {
                try
                {
                    Console.WriteLine($"Checking {path}");
                    Console.WriteLine($"Rescueing existing files if needed...");
                    CheckFolderStatic(path);
                    Console.WriteLine($"Done.");
                    Console.WriteLine($"Creating watcher...");
                    FileSystemWatcher fsw = new FileSystemWatcher(path);
                    fsw.EnableRaisingEvents = true;
                    fsw.Created += Fsw_Event;
                    fsw.Changed += Fsw_Event;
                    fsw.Renamed += Fsw_Event;
                    watchers.Add(fsw);
                    Console.WriteLine($"Done.");
                } catch (Exception ex)
                {
                    Console.WriteLine($"Error creating watcher for path {path}. Quitting. Error: {ex.ToString()}");
                    Console.ReadKey();
                    return;
                }
            }

            Console.WriteLine("Watching for unsaved demos. Press any key to end the program.");
            Console.ReadKey();
        }

        static Regex lastDemoMatch = new Regex(@"LastDemo.dm_\d\d", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static void CheckFolderStatic(string folderPath)
        {
            string[] files = Directory.GetFiles(folderPath);
            foreach(string file in files)
            {
                if (lastDemoMatch.Match(file).Success)
                {
                    MoveFileToSafety(file);
                }
            }
        }

        private static void Fsw_Event(object sender, FileSystemEventArgs e)
        {
            if (lastDemoMatch.Match(e.Name).Success)
            {
                Console.WriteLine($"Found {e.FullPath}. Mode: {e.ChangeType}");
                MoveFileToSafety(e.FullPath);
                Console.WriteLine("Watching for unsaved demos. Press any key to end the program.");
            }
        }
        private static object rescueLock = new object();

        private static void MoveFileToSafety(string fullPath)
        {
            int tries = 0;
            goto actuallytry;
        retry:
            if(tries > 10)
            {
                Console.WriteLine($"Failed 10 times to rescue {fullPath}. Giving up.");
                return;
            }
            Thread.Sleep(500);
        actuallytry:
            tries++;
            lock (rescueLock)
            {
                if (!File.Exists(fullPath))
                {
                    Console.WriteLine("File doesn't exist though? Weird. Maybe we already handled it.");
                    return;
                }
                try
                {
                    DateTime dateModified = File.GetLastWriteTime(fullPath);
                    string newDir = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(fullPath), "..", "AutoDemoRescue"));
                    if (!Directory.Exists(newDir))
                    {
                        Console.WriteLine($"Creating {newDir}");
                        Directory.CreateDirectory(newDir);
                    }
                    string newName = Path.GetFileNameWithoutExtension(fullPath) + $"_rescue" + dateModified.ToString("yyyy-MM-dd_HH-mm-ss") + Path.GetExtension(fullPath);

                    string finalPath = GetUnusedFilename(Path.Combine(newDir, newName));

                    Console.WriteLine($"Moving file to {finalPath}");
                    File.Move(fullPath,finalPath,false);
                    if(!File.Exists(fullPath) && File.Exists(finalPath))
                    {
                        Console.WriteLine($"Done.");
                    }
                    else
                    {
                        Console.WriteLine($"Weird. Old path still exists or final path doesn't exist. Try again.");
                        goto retry;
                    }
                } catch(Exception ex)
                {
                    Console.WriteLine($"Error rescueing {fullPath}. Will retry. {ex.ToString()}");
                    goto retry;
                }

            }
        }

        public static string GetUnusedFilename(string baseFilename)
        {
            if (!File.Exists(baseFilename))
            {
                return baseFilename;
            }
            string extension = Path.GetExtension(baseFilename);

            int index = 1;
            while (File.Exists(Path.ChangeExtension(baseFilename, "." + (++index) + extension))) ;

            return Path.ChangeExtension(baseFilename, "." + (index) + extension);
        }
    }
}
