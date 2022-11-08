using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using Newtonsoft.Json.Linq;

namespace start_protected_game
{
    internal class Setup
    {
        static Logger log = Logger.Instance("Setup");

        static string[] arg = null;
        
        public static void Run(string[] args = null)
        {
            if (arg == null)
            {
                arg = args;
            }

            Console.Title = "Setup";

            if (Amongus._args.Contains("-Debug"))
            {
                Thread thread = new Thread(Console_Size);
                thread.Start();
            }
            Console.WindowWidth = 160;

            Console.WriteLine("Please input an option.\n\n1: Add new program to launch on startup.\n2: Remove a program from startup\n3: Edit a existing programs settings\n4: Config\n5: Exit setup and launch\n");

            int input = 0;

            try { input = Convert.ToInt32(Console.ReadLine()); } catch { Console.Clear(); log.Error("You must input a number!\n"); Run(); }

            if (input == 1)
            {
                Console.Clear();
                AddProgram();
            }
            else if (input == 2)
            {
                Console.Clear();
                RemoveProgram();
            }
            else if (input == 3)
            {
                Console.Clear();
                EditProgram();
            }
            else if (input == 4)
            {
                Console.Clear();
                config();
            }
            else if (input == 5)
            {
                List<string> strings = new List<string>();

                foreach (string s in arg.ToList())
                {
                    if (s != "-setup")
                    {
                        strings.Add(s);
                    }
                }
                Console.Clear();
                System.Diagnostics.Process.Start($"{Environment.CurrentDirectory}\\start_protected_game.exe", String.Join(" ", strings));
                //Amongus.Main(strings.ToArray());
            }
            else
            {
                Console.Clear();
                log.Error("The option you selected was not valid, try again!\n");
                Run();
            }

        }

        public static void Console_Size()
        {
            while (true)
            {
                Console.Title = $"Width: {Console.WindowWidth} | Height: {Console.WindowHeight}";
                Thread.Sleep(100);
            }
        }

        #region Add Program
        static void AddProgram()
        {
            Console.Title = "Add Program:";
            AddProgram_Directory(ProgramDetails.New());
        }

        static void AddProgram_Directory(ProgramDetails details)
        {
            Console.Title = "Add Program: Directory (1/3)";

            Console.WriteLine("Enter or paste the directory of the program you want to add. Example: W:\\ProgramFiles\\Epic Games\\Fortnite\nType \"Cancel\" or \"C\" to return to the main menu\n");

            string directory = Console.ReadLine();

            if (directory.ToLower() == "cancel" || directory.ToLower() == "c")
            {
                Console.Clear();
                Run();
            } else if (!Directory.Exists(directory))
            {
                Console.Clear();

                log.Error("Directory does not exist, look for typos or see if the directory is real and try again.\n");

                AddProgram_Directory(details);
            } else
            {
                details.Directory = directory;

                Console.Clear();

                AddProgram_Name(details);
            }
        }

        static void AddProgram_Name(ProgramDetails details)
        {
            Console.Title = "Add Program: Name/Executable (2/3)";

            string[] programs = Directory.GetFiles(Amongus.programDir);
            bool isValid = true;

            Console.WriteLine("Enter or paste the name of the program. Must be a .exe! (\".exe\" in the name is not required)\nIf needed, you can return edit the directory by typing \"return\".\n");

            string name = Console.ReadLine();

            foreach (string program in programs)
            {
                string _name = program.Split("\\")[program.Split("\\").Length - 1];

                if (program.Contains(name))
                {
                    isValid = false;
                }
            }
            
            if (isValid)
            {
                if (name.ToLower() == "return")
                {
                    Console.Clear();

                    AddProgram_Directory(details);
                }
                else
                {
                    if (name.Contains(".exe"))
                    {
                        string[] temp = name.Split(".exe");
                        name = temp[0];
                    }

                    if (!File.Exists($"{details.Directory}\\{name}.exe"))
                    {
                        Console.Clear();

                        log.Error($"Could not find \"{name}.exe\" in \"{details.Directory}\".\nAre you sure that the directoy and name are correct?\n");

                        AddProgram_Name(details);
                    }
                    else
                    {
                        details.Name = name;
                        details.Executable = name + ".exe";

                        Console.Clear();

                        AddProgram_Config(details);
                    }
                }
            } else
            {
                Console.Clear();

                log.Error($"{name} already exists in the program list, did you mean to type a diffrent name?\n");

                AddProgram_Name(details);
            }
        }

        static void AddProgram_Config(ProgramDetails details)
        {
            Console.Title = "Add Program: Config (3/3)";

            Console.WriteLine("Last step! Mess with the config if you want.\n(What to type to edit)Discription | Value\n\nType \"Finish\" or \"F\" when you are done.\n");

            string args = "Empty";

            if (details.args != null)
            {
                for (int i = 0; i < details.args.Length; i++)
                {
                    if (args == "Empty")
                    {
                        args = details.args[i];
                    } else
                    {
                        args = args + $", {details.args[i]}";
                    }
                }
            }

            if (string.IsNullOrEmpty(args))
            {
                args = "Empty";
            }

            Console.WriteLine($"(Ad/Admin) Run program as admin | {details.runAdmin}");
            Console.WriteLine($"(A/Args) Launch arguments, mainly for advanced users. CURRENTLY NOT WORKING| {args}");
            Console.WriteLine($"(R/Relaunch) This program will be restarted if the prosses is not found running at any time. | {details.restartOnClose}");
            Console.WriteLine($"(V/VR) Launch program when in vr. | {details.launchWithVR}");
            Console.WriteLine($"(D/Desktop) Launch program when in desktop. | {details.launchWithDesktop}");

            Console.WriteLine();

            string input = Console.ReadLine().ToLower();

            if (input == "finish" || input == "f")
            {
                string jsonString = JsonSerializer.Serialize(details);

                File.WriteAllText($"{Amongus.programDir}\\{details.Name}.json", jsonString);

                Console.Clear();

                log.Info("Config Created!", InfoType.Complete);

                System.Diagnostics.Process.Start($"{Environment.CurrentDirectory}\\start_protected_game.exe", String.Join(" ", Amongus._args));
            } 
            else if (input == "admin" || input == "ad")
            {
                details.runAdmin = !details.runAdmin;

                Console.Clear();

                AddProgram_Config(details);
            } 
            else if (input == "args" || input == "a")
            {
                Console.WriteLine("Arguments are seperated by spaces.\n");

                string value = Console.ReadLine();

                

                details.args = value.Split(" ");

                Console.Clear();

                AddProgram_Config(details);
            }
            else if (input == "relaunch" || input == "r")
            {
                details.restartOnClose = !details.restartOnClose;

                Console.Clear();

                AddProgram_Config(details);
            }
            else if (input == "v" || input == "vr")
            {
                details.launchWithVR = !details.launchWithVR;

                Console.Clear();

                AddProgram_Config(details);
            }
            else if (input == "d" || input == "desktop")
            {
                details.launchWithDesktop = !details.launchWithDesktop;

                Console.Clear();

                AddProgram_Config(details);
            }
            else
            {
                Console.Clear();
                log.Error("Invalid Input!");
                AddProgram_Config(details);
            }
        }

        #endregion

        #region Remove Program
        static void RemoveProgram()
        {
            Console.Title = "Remove Program";

            Console.WriteLine("Type the name of a program to remove from the list.\nType \"Cancel\" or \"C\" to return to the main menu\n");

            string[] files = Directory.GetFiles(Amongus.programDir);
            List<String> list = new List<String>();

            if (files != null)
            {
                foreach (string file in files)
                {
                    string amog = $"{file.Split("\\")[file.Split("\\").Length - 1].Split(".json")[0]}";
                    list.Add(amog);

                    Console.WriteLine(amog);
                }
            }
            Console.WriteLine("\n");

            string input = Console.ReadLine();

            if (input.ToLower() == "cancel" || input.ToLower() == "c")
            {
                Console.Clear();
                Run();
            } else
            {
                bool fileDeleted = false;

                foreach (string file in files)
                {
                    if (input == file.Split("\\")[file.Split("\\").Length - 1].Split(".json")[0] && fileDeleted == false)
                    {
                        File.Delete(file);
                        fileDeleted = true;
                    }
                }
                if (fileDeleted)
                {
                    Console.Clear();
                    log.Info("Config Removed!", InfoType.Complete);
                    System.Diagnostics.Process.Start($"{Environment.CurrentDirectory}\\start_protected_game.exe", String.Join(" ", Amongus._args));

                }
                else
                {
                    Console.Clear();
                    log.Error($"{input} not found!\n");
                    RemoveProgram();
                }
            }
        }

        #endregion

        #region Edit Program

        static void EditProgram()
        {
            Console.Title = "Edit Program";

            Console.Clear();
            log.Error("Feature not implemented yet");
            Run();
        }

        #endregion

        #region Config

        static void config()
        {
            Console.Title = "Config";

            Console.Clear();
            Config_Modify();
        }

        static void Config_Modify()
        {
            Console.Title = "Config: Main";

            string LoginTime = Amongus.config.TimeLimit.LoginTime;
            string LogoutTime = Amongus.config.TimeLimit.LogoutTime;

            {
                string[] strings = LoginTime.Split(":");
                int tempH = Convert.ToInt32(strings[0]);
                int tempM = Convert.ToInt32(strings[1]);
                string zone = "AM";

                if (tempH <= 12)
                {
                    tempH = tempH;
                }
                else
                {
                    tempH = tempH - 12;
                    zone = "PM";
                }
                LoginTime = $"{tempH}:{tempM.ToString("D2")}{zone}";
            }

            {
                string[] strings = LogoutTime.Split(":");
                int tempH = Convert.ToInt32(strings[0]);
                int tempM = Convert.ToInt32(strings[1]);
                string zone = "AM";

                if (tempH <= 12)
                {
                    tempH = tempH;
                }
                else
                {
                    tempH = tempH - 12;
                    zone = "PM";
                }
                LogoutTime = $"{tempH}:{tempM.ToString("D2")}{zone}";
            }
            Console.WriteLine("(What character/word to type)Discription | Value(s) (Default Value)\n\n");

            Console.WriteLine("(F/Finish) Save config and return to main menu");
            Console.WriteLine("(S/Save) Save config");
            Console.WriteLine("(D/Discard) Leave without saving");
            Console.WriteLine($"(R/Reset)Resets config to default settings\n\n");

            Console.WriteLine($"(T/Time)Hard limit on when the game can/will launch | Login: {LoginTime} (12:00AM), Logout: {LogoutTime} (12:00AM)");
            Console.WriteLine($"(O/OSC)Config a built-in OSC Re-router so all your osc apps can read data from vrc | Out IP/Port: {Amongus.config.OSC.OutIP_Port} (127.0.0.1:9001), Output IP/Ports: {String.Join(", ", Amongus.config.OSC.OptIP_Ports)} (Empty)");
            Console.WriteLine();
            

            string input = Console.ReadLine().ToLower();

            if (input == "f" || input == "finish" || input == "s" || input == "save")
            {
                try
                {
                    log.Info("Saving Config...", InfoType.Loading);
                    string _jsonString = JsonSerializer.Serialize(Amongus.config);

                    File.WriteAllText(Amongus._Config, _jsonString);

                    Console.Clear();

                    log.Info("Config Saved!", InfoType.Complete);

                    if (input == "f" || input == "finish")
                    {
                        Run();
                    } else
                    {
                        Config_Modify();
                    }
                }
                catch (Exception e)
                {
                    log.Info($"Could not save config!\nError: {e}", InfoType.Exception);
                }
            } else if (input == "r" || input == "reset")
            {
                log.Warn("Are you sure? (Y/N)\n");

                string yes = Console.ReadLine().ToLower();

                if (yes == "y")
                {
                    log.Info("Resetting Config...", InfoType.Loading);
                    Amongus.config = Config.Create();
                    string _jsonString = JsonSerializer.Serialize(Amongus.config);

                    File.WriteAllText(Amongus._Config, _jsonString);
                    Console.Clear();
                    log.Info("Config Reset!", InfoType.Complete);

                    Config_Modify();
                }

                if (yes == "n")
                {
                    Console.Clear();

                    Config_Modify();
                }
            } else if (input == "t" || input == "time")
            {
                Console.Clear();

                Config_Time(LoginTime, LogoutTime);
            }
            else if (input == "o" || input == "osc")
            {
                Console.Clear();

                Config_OSC(Amongus.config.OSC.OutIP_Port, Amongus.config.OSC.OptIP_Ports);
            }
            else if (input == "d" || input == "discard")
            {
                log.Warn("Are you sure? (Y/N)\n");

                string value = Console.ReadLine().ToLower();

                if (!string.IsNullOrEmpty(value))
                {
                    Console.Clear();
                    log.Error("Invalid Input!");
                    Config_Modify();
                }else if (value == "y")
                {
                    Console.Clear();

                    log.Info("Changes Discarded!", InfoType.Complete);

                    Run();
                } else if (value == "n")
                {
                    Console.Clear();
                    Config_Modify();
                } else
                {
                    Console.Clear();
                    log.Error("Invalid Input!");
                    Config_Modify();
                }

                
            }
            else
            {
                Console.Clear();
                log.Error("Invalid Input!");

                Config_Modify();
            }
        }

        static void Config_Time(string login, string logout)
        {
            Console.Title = "Config: Time";

            Console.WriteLine("(What character/word to type)Discription | Value(s) (Default Value)\n");

            Console.WriteLine("(R/Return) Returns to previous menu\n");

            Console.WriteLine($"(I/Login)Change the time you can open vrc | {login} (12:00AM)");
            Console.WriteLine($"(O/Logout)Change the time VRChat closes and wont launch | {logout} (12:00AM)");

            Console.WriteLine();

            string input = Console.ReadLine().ToLower();

            if (input == "i" || input == "login")
            {
                bool valid = false;
                int _hour = 0;
                int _min = 0;
                string zone = "";

                Console.WriteLine("\nInput time. Example: \"12:15:PM\". Formating needs to be *EXACT* (Not case sensitive though).\n");

                string value = Console.ReadLine().ToLower();

                try
                {
                    string[] strings = value.Split(":");
                    _hour = int.Parse(strings[0]);
                    _min = int.Parse(strings[1]);
                    zone = strings[2];
                    if (zone == "pm" || zone == "am")
                        valid = true;
                    else
                        throw new Exception("AM/PM was not provided");
                }
                catch (Exception e)
                {
                    Console.Clear();
                    log.Error($"An error occured, please try again.\nError: {e}");
                    Config_Time(login, logout);
                }

                if (valid == true)
                {
                    if (zone == "pm")
                    {
                        if (_hour == 12)
                        {
                            _hour = 12;
                        } else
                        {
                            _hour = _hour + 12;
                        }
                    } else
                    {
                        if (_hour == 12)
                        {
                            _hour = 0;
                        }
                    }

                    Amongus.config.TimeLimit.LoginTime = $"{_hour}:{_min}";

                    Console.Clear();

                    log.Info("Login time updated! Remember to type \"Finish\" or \"F\" to save!", InfoType.Complete);
                    Config_Modify();
                }
            } else if (input == "o" || input == "logout")
            {
                bool valid = false;
                int _hour = 0;
                int _min = 0;
                string zone = "";

                Console.WriteLine("\nInput time. Example: \"12:15:PM\". Formating needs to be *EXACT* (Not case sensitive though).\n");

                string value = Console.ReadLine().ToLower();

                try
                {
                    string[] strings = value.Split(":");
                    _hour = int.Parse(strings[0]);
                    _min = int.Parse(strings[1]);
                    zone = strings[2];
                    if (zone == "pm" || zone == "am")
                        valid = true;
                    else
                        throw new Exception("AM/PM was not provided");
                }
                catch (Exception e)
                {
                    Console.Clear();
                    log.Error($"An error occured, please try again.\nError: {e}");
                    Config_Time(login, logout);
                }

                if (valid == true)
                {
                    if (zone == "pm")
                    {
                        if (_hour == 12)
                        {
                            _hour = 12;
                        }
                        else
                        {
                            _hour = _hour + 12;
                        }
                    }
                    else
                    {
                        if (_hour == 12)
                        {
                            _hour = 0;
                        }
                    }

                    Amongus.config.TimeLimit.LogoutTime = $"{_hour}:{_min}";

                    Console.Clear();

                    log.Info("Logout time updated! Remember to type \"Finish\" or \"F\" to save!", InfoType.Complete);
                    Config_Modify();
                }
            } else if (input == "r" || input == "return")
            {
                Console.Clear();

                Config_Modify();
            } else
            {
                Console.Clear();
                log.Error("Invalid Input!");

                Config_Time(login, logout);
            }


        }

        static void Config_OSC(string output, List<string> outputs)
        {
            Console.Title = "Config: OSC Re-router | If you need to configure VRChat's OSC ports and ip, use \"--\"InPort\":\"IP\":\"OutPort\"\" in steam launch parameters, without the \" of course.";

            Console.WriteLine("(What character/word to type)Discription | Value(s) (Default Value)\n");

            Console.WriteLine("(F/Finish) Return\n");

            Console.WriteLine($"(I/Input)VRChat Output IP/Port | {output} (9000:127.0.0.1:9001)");
            Console.WriteLine($"(O/Outputs)List of IP/Ports that \"{output.Split(":")[1] + ":" + output.Split(":")[2]}\" sends data to | {String.Join(", ", outputs)} (Empty)");

            Console.WriteLine();

            string input = Console.ReadLine().ToLower();

            if (input == "f" || input == "finish")
            {
                Console.Clear();

                log.Info("Config updated! Make sure to save before closing the program!", InfoType.Complete);

                Config_Modify();
            } else if (input == "i" || input == "input")
            {
                Console.WriteLine($"\nInput an in port, ip, and output port. Example: 1336:127.0.0.1:1337\n");

                string value = Console.ReadLine();

                if (!value.Contains(":"))
                {
                    Console.Clear();

                    log.Error("\':\' Not found!");

                    Config_OSC(output, outputs);
                } else
                {
                    Console.Clear();

                    Amongus.config.OSC.OutIP_Port = value;
                    Config_OSC(value, outputs);
                }
            } else if (input == "o" || input == "outputs")
            {
                List<string> list;

                if (outputs.Count == 1 && outputs[0] == "")
                    list = new List<string>();
                else
                    list = outputs;

                Console.WriteLine($"\nInput a ip and port. Example: 127.0.0.1:1337\n");
                Console.WriteLine($"Current IP/Ports\n\n{String.Join("\n", list)}\n");
                Console.WriteLine($"You can type an existing IP/Port from the list to remove it. You can also use \',\' to input multible IP/Ports. Example: 127.0.0.1:1337,127.0.0.1:1338\n");

                string value = Console.ReadLine().Replace(" ", "");

                string[] values = null;

                if (value.Contains(","))
                    values = value.Split(',');

                if (!value.Contains(":"))
                {
                    Console.Clear();

                    log.Error("\':\' Not found!");

                    Config_OSC(output, outputs);
                }
                else
                {
                    if (values != null)
                    {
                        foreach (string val in values)
                        {
                            list.Add(val);
                        }
                        Console.Clear();

                        Amongus.config.OSC.OptIP_Ports = list;
                        Config_OSC(output, list);
                    } 
                    else if (list.Contains(value))
                    {
                        Console.Clear();
                        
                        list.Remove(value);

                        Amongus.config.OSC.OptIP_Ports = list;
                        Config_OSC(output, list);
                    } else
                    {
                        Console.Clear();

                        list.Add(value);
                        Amongus.config.OSC.OptIP_Ports = list;
                        Config_OSC(output, list);
                    }
                    
                }
            }
        }

        #endregion
    }

    #region Json Classes

    public class ProgramDetails
    {
        public string Name { get; set; }
        public string Executable { get; set; }
        public string Directory { get; set; }

        public bool runAdmin { get; set; }
        public bool restartOnClose { get; set; }
        public bool launchWithVR { get; set; }
        public bool launchWithDesktop { get; set; }
        public string[] args { get; set; }

        public static ProgramDetails New()
        {
            ProgramDetails programDetails = new ProgramDetails();
            programDetails.restartOnClose = true;
            programDetails.launchWithVR = true;
            programDetails.launchWithDesktop = true;

            return programDetails;
        }
    }

    public class Config
    {
        public Config_Time TimeLimit { get; set; }
        public Config_OSC OSC { get; set; }

        public static Config Create()
        {
            Config config = new Config();

            config.TimeLimit = Config_Time.Create();
            config.OSC = Config_OSC.Create();

            return config;
        }
    }

    public class Config_Time
    {
        public string LoginTime { get; set; }
        public string LogoutTime { get; set; }

        public static Config_Time Create()
        {
            Config_Time config = new Config_Time();
            config.LoginTime = "12:00";
            config.LogoutTime = "12:00";

            return config;
        }
    }

    public class Config_OSC
    {
        public string OutIP_Port { get; set; }
        public List<string> OptIP_Ports { get; set; }

        public static Config_OSC Create()
        {
            Config_OSC oSC = new Config_OSC();
            oSC.OutIP_Port = "9000:127.0.0.1:9001";
            oSC.OptIP_Ports = new List<string>();
            oSC.OptIP_Ports.Add("");

            return oSC;
        }
    }

    #endregion

}
