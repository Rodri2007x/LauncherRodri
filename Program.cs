using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.Win32;

namespace LauncherRodri
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Launcher de Juegos";
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Clear();

    
            string steamPath = GetSteamExecutablePath();
            string epicPath = GetEpicGamesLauncherExecutablePath();

            
            if (string.IsNullOrEmpty(steamPath))
            {
                WriteColored("No se encontró la ruta de Steam.", ConsoleColor.Red);
                Console.Write("Introduce la ruta completa de Steam.exe: ");
                steamPath = Console.ReadLine();
            }

            if (string.IsNullOrEmpty(epicPath))
            {
                WriteColored("No se encontró la ruta del Epic Games Launcher.", ConsoleColor.Red);
                Console.Write("Introduce la ruta completa de EpicGamesLauncher.exe: ");
                epicPath = Console.ReadLine();
            }

            bool exit = false;
            while (!exit)
            {
                Console.Clear();
                WriteColored("=== Launcher de Juegos ===", ConsoleColor.Yellow);
                Console.WriteLine();
                WriteColored("1. Iniciar Steam", ConsoleColor.Cyan);
                WriteColored("2. Iniciar Epic Games Launcher", ConsoleColor.Cyan);
                WriteColored("3. Salir", ConsoleColor.Cyan);
                Console.WriteLine();
                Console.Write("Elige una opción (1-3): ");
                string opcion = Console.ReadLine();

                switch (opcion)
                {
                    case "1":
                        LaunchProcess(steamPath);
                        break;
                    case "2":
                        LaunchProcess(epicPath);
                        break;
                    case "3":
                        exit = true;
                        break;
                    default:
                        WriteColored("Opción inválida.", ConsoleColor.Red);
                        break;
                }
                if (!exit)
                {
                    WriteColored("Presiona Enter para continuar...", ConsoleColor.Green);
                    Console.ReadLine();
                }
            }
        }

        
        static void WriteColored(string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        
        static string GetSteamExecutablePath()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam"))
                {
                    if (key != null)
                    {
                        object value = key.GetValue("SteamPath");
                        if (value != null)
                        {
                            string steamDir = value.ToString();
                            string exePath = Path.Combine(steamDir, "Steam.exe");
                            if (File.Exists(exePath))
                                return exePath;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteColored("Error obteniendo la ruta de Steam: " + ex.Message, ConsoleColor.Red);
            }
            return string.Empty;
        }

        
        static string GetEpicGamesLauncherExecutablePath()
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Epic Games\EpicGamesLauncher"))
                {
                    if (key != null)
                    {
                        object value = key.GetValue("AppDataPath") ?? key.GetValue("InstallLocation");
                        if (value != null)
                        {
                            string epicDir = value.ToString();
                            string exePath = Path.Combine(epicDir, "EpicGamesLauncher.exe");
                            if (File.Exists(exePath))
                                return exePath;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteColored("Error obteniendo la ruta del Epic Games Launcher: " + ex.Message, ConsoleColor.Red);
            }
            
            string defaultEpicPath = @"C:\Program Files (x86)\Epic Games\Launcher\Portal\Binaries\Win64\EpicGamesLauncher.exe";
            return File.Exists(defaultEpicPath) ? defaultEpicPath : string.Empty;
        }

        
        static void LaunchProcess(string exePath)
        {
            try
            {
                string launcherName = Path.GetFileNameWithoutExtension(exePath);
                
                Process[] runningProcesses = Process.GetProcessesByName(launcherName);
                if (runningProcesses.Length > 0)
                {
                    WriteColored($"{launcherName} ya está en ejecución.", ConsoleColor.Yellow);
                    WriteColored("¿Desea cerrarlo y volver a iniciarlo? Presione Enter para confirmar, o cualquier otra tecla para cancelar.", ConsoleColor.Yellow);
                    var key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.Enter)
                    {
                        
                        foreach (var proc in runningProcesses)
                        {
                            try { proc.Kill(); }
                            catch (Exception ex) { WriteColored("Error al cerrar " + launcherName + ": " + ex.Message, ConsoleColor.Red); }
                        }
                        WriteColored("Procesos cerrados. Reiniciando...", ConsoleColor.Green);
                        Thread.Sleep(1000);
                    }
                    else
                    {
                        WriteColored("Operación cancelada. Cerrando la aplicación.", ConsoleColor.Red);
                        Environment.Exit(0);
                    }
                }

                WriteColored($"Iniciando {launcherName}...", ConsoleColor.Green);
                Process procNew = Process.Start(exePath);
                if (procNew != null)
                {
                    
                    Thread.Sleep(3000);
                    if (!procNew.HasExited)
                    {
                        WriteColored($"{launcherName} se está ejecutando. Cerrando el launcher de la consola.", ConsoleColor.Green);
                        Environment.Exit(0);
                    }
                    else
                    {
                        WriteColored($"{launcherName} no se inició correctamente.", ConsoleColor.Red);
                    }
                }
                else
                {
                    WriteColored("No se pudo iniciar el proceso.", ConsoleColor.Red);
                }
            }
            catch (Exception ex)
            {
                WriteColored("Error al iniciar el proceso: " + ex.Message, ConsoleColor.Red);
            }
        }
    }
}
