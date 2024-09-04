using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace ado
{
    class Program
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct COMMAND_LINE_ARGS
        {
            public bool ShowHelp;
            public bool Wait;
            public bool StartComspec;
            public bool Install;
            public bool Uninstall;
            public string ApplicationName;
            public string CommandLine;
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern uint GetEnvironmentVariable(string lpName, StringBuilder lpBuffer, int nSize);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern bool SetEnvironmentVariable(string lpName, string lpValue);

        static int Launch(string applicationName, string commandLine, bool wait)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = applicationName,
                Arguments = commandLine,
                Verb = "runas",
                UseShellExecute = true,
                CreateNoWindow = false
            };

            try
            {
                using (var process = Process.Start(startInfo))
                {
                    if (wait)
                    {
                        process.WaitForExit();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(String.Format(ado.Properties.Resources._0CouldNotBeLaunched1, applicationName, ex.Message));
                return 1;
            }

            return 0;
        }

        static int DispatchCommand(ref COMMAND_LINE_ARGS args)
        {
            StringBuilder appNameBuffer = new StringBuilder(260);
            StringBuilder cmdLineBuffer = new StringBuilder(520);
            String adoVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            if (args.ShowHelp)
            {
                Console.WriteLine("\n" +
                    String.Format(ado.Properties.Resources.AdoAdministratorDo2024ManfredMueller, adoVersion) + "\n\n" +
                    ado.Properties.Resources.ExecuteAProcessOnTheCommandLineElevatedViaUAC + "\n" +
                    ado.Properties.Resources.UsageAdoWaitKProgArgs + "\n" +
                    "-?\t" + ado.Properties.Resources.ShowsThisHelp + "\n" +
                    "-wait\t" + ado.Properties.Resources.WaitWaitsUntilProgTerminates + "\n" +
                    "-k\t" + ado.Properties.Resources.KStartsTheTheCOMSPECEnvironmentVariableValueAnd + "\n" +
                    "\t" + ado.Properties.Resources.ExecutesProgInItCMDEXEEtc + "\n" +
                    "-i\t" + ado.Properties.Resources.InstallsTheProgramToCurrentUserSProgramFilesAnd + "\n" +
                    "\t" + ado.Properties.Resources.AddsItToTheUserSPATHVariable + "\n" +
                    "-u\t" + ado.Properties.Resources.UninstallsTheProgramFromCurrentUserSProgramFilesAnd + "\n" +
                    "\t" + ado.Properties.Resources.RemovesItFromTheUserSPATHVariable + "\n" +
                    "prog\t" + ado.Properties.Resources.ProgTheProgramToExecute + "\n" +
                    "[args]\t" + ado.Properties.Resources.ArgsOptionalCommandLineArgumentsToProg + "\n"
                );

                return 0;
            }

            if (args.Install)
            {
                return InstallProgram();
            }

            if (args.Uninstall)
            {
                return UninstallProgram();
            }

            if (args.StartComspec)
            {
                if (GetEnvironmentVariable("COMSPEC", appNameBuffer, appNameBuffer.Capacity) == 0)
                {
                    Console.Error.WriteLine(ado.Properties.Resources.COMSPECIsNotDefined);
                    return 1;
                }
                args.ApplicationName = appNameBuffer.ToString();

                if (cmdLineBuffer.Append("/K \"").Append(args.CommandLine).Append("\"").Length > cmdLineBuffer.Capacity)
                {
                    Console.Error.WriteLine(ado.Properties.Resources.CreatingCommandLineFailed);
                    return 1;
                }

                args.CommandLine = cmdLineBuffer.ToString();
            }

            return Launch(args.ApplicationName, args.CommandLine, args.Wait);
        }

        static int InstallProgram()
        {
            try
            {
                string sourcePath = Process.GetCurrentProcess().MainModule.FileName;
                string programFilesPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string destPath = Path.Combine(programFilesPath, "Ado", Path.GetFileName(sourcePath));

                Directory.CreateDirectory(Path.GetDirectoryName(destPath));
                File.Copy(sourcePath, destPath, true);

                AddToPath(Path.GetDirectoryName(destPath));
                Console.WriteLine(ado.Properties.Resources.ProgramInstalledSuccessfully);
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ado.Properties.Resources.InstallationFailed + ex.Message);
                return 1;
            }
        }

        static int UninstallProgram()
        {
            try
            {
                string programFilesPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string destPath = Path.Combine(programFilesPath, "Ado");

                if (Directory.Exists(destPath))
                {
                    Directory.Delete(destPath, true);
                }

                RemoveFromPath(destPath);
                Console.WriteLine(ado.Properties.Resources.ProgramUninstalledSuccessfully);
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ado.Properties.Resources.UninstallationFailed + ex.Message);
                return 1;
            }
        }

        static void AddToPath(string path)
        {
            string oldPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User);
            if (!oldPath.Contains(path))
            {
                string newPath = oldPath + ";" + path;
                Environment.SetEnvironmentVariable("PATH", newPath, EnvironmentVariableTarget.User);
                Console.WriteLine(ado.Properties.Resources.AddedToPATH + path);
            }
        }

        static void RemoveFromPath(string path)
        {
            string oldPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User);
            if (oldPath.Contains(path))
            {
                string newPath = oldPath.Replace(path + ";", "").Replace(";" + path, "").Replace(path, "");
                Environment.SetEnvironmentVariable("PATH", newPath, EnvironmentVariableTarget.User);
                Console.WriteLine(ado.Properties.Resources.RemovedFromPATH + path);
            }
        }

        static int Main(string[] args)
        {
            COMMAND_LINE_ARGS commandLineArgs = new COMMAND_LINE_ARGS();
            StringBuilder commandLineBuffer = new StringBuilder();

            bool flagsRead = false;

            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];
                if (!flagsRead && (arg.StartsWith("-") || arg.StartsWith("/")))
                {
                    string flagName = arg.Substring(1).ToLowerInvariant();
                    if (flagName == "?")
                    {
                        commandLineArgs.ShowHelp = true;
                    }
                    else if (flagName == "wait")
                    {
                        commandLineArgs.Wait = true;
                    }
                    else if (flagName == "k")
                    {
                        commandLineArgs.StartComspec = true;
                    }
                    else if (flagName == "i")
                    {
                        commandLineArgs.Install = true;
                    }
                    else if (flagName == "u")
                    {
                        commandLineArgs.Uninstall = true;
                    }
                    else
                    {
                        Console.Error.WriteLine(String.Format(ado.Properties.Resources.UnrecognizedFlag0, flagName));
                        return 1;
                    }
                }
                else
                {
                    flagsRead = true;
                    if (commandLineArgs.ApplicationName == null && !commandLineArgs.StartComspec && !commandLineArgs.Install && !commandLineArgs.Uninstall)
                    {
                        commandLineArgs.ApplicationName = arg;
                    }
                    else
                    {
                        commandLineBuffer.Append(arg).Append(" ");
                    }
                }
            }

            commandLineArgs.CommandLine = commandLineBuffer.ToString().TrimEnd();

            if (args.Length == 0)
            {
                commandLineArgs.ShowHelp = true;
            }

            if (!commandLineArgs.ShowHelp &&
                ((commandLineArgs.StartComspec && commandLineBuffer.Length == 0) ||
                 (!commandLineArgs.StartComspec && commandLineArgs.ApplicationName == null) &&
                 !commandLineArgs.Install && !commandLineArgs.Uninstall))
            {
                Console.Error.WriteLine(ado.Properties.Resources.InvalidArguments);
                return 1;
            }

            return DispatchCommand(ref commandLineArgs);
        }
    }
}