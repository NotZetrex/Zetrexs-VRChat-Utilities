using System.Text.Json;
using start_protected_game;
using start_protected_game.OSC;
using System.Diagnostics;
using Newtonsoft.Json;
using System;
using Newtonsoft.Json.Linq;
using JsonSerializer = System.Text.Json.JsonSerializer;
using Microsoft.Win32;
using System.Net;
using System.Runtime.InteropServices;
using Microsoft.VisualBasic;

class Amongus
{
    static string EACImg;
    static string EACImgRepo;

    public static List<string> _args = new List<string>();

    public static List<ProgramDetails> programs = new List<ProgramDetails>();

    public static Config config = Config.Create();

    public static ConsoleComponents consoleComponents = new ConsoleComponents();

    #region Arguments

    static bool canRunVRC = true;
    static bool randomImg = false;
    static bool runSetup = false;
    static bool timeLimit = false;
    static bool oscrouter = false;
    static bool launchPrograms = true;
    public static bool isVrRunning = true;

    #endregion

    public static string programDir;
    public static string _Config;

    static Logger log;
    public static void Main(string[] args)
    {
        log = Logger.Instance("System");

        //Converts the args array to a List so it can be easily read to grab args used here
        foreach (string arg in args) _args.Add(arg.ToLower());

        if (_args.Contains("-debug")) Logger.isDebug = true;

        if (!Directory.Exists($"{Environment.CurrentDirectory}\\Zetrex's VRC Utils"))
            Directory.CreateDirectory($"{Environment.CurrentDirectory}\\Zetrex's VRC Utils");

        if (!Directory.Exists($"{Environment.CurrentDirectory}\\Zetrex's VRC Utils\\Programs"))
            Directory.CreateDirectory($"{Environment.CurrentDirectory}\\Zetrex's VRC Utils\\Programs");

        if (!File.Exists($"{Environment.CurrentDirectory}\\Zetrex's VRC Utils\\Config.json"))
            File.Create($"{Environment.CurrentDirectory}\\Zetrex's VRC Utils\\Config.json");

        programDir = $"{Environment.CurrentDirectory}\\Zetrex's VRC Utils\\Programs";
        _Config = $"{Environment.CurrentDirectory}\\Zetrex's VRC Utils\\Config.json";

        LoadConfig();

        #region Check Arguments

        //Checks args
        if (_args.Contains("-setup"))
        {
            Setup.Run(_args.ToArray());
            runSetup = true;
        } else
        {
            runSetup = false;
        }

        if (_args.Contains("-nolaunch")) launchPrograms = false;

        if (_args.Contains("-novrc")) canRunVRC = false;

        if (_args.Contains("-randimg")) randomImg = true;

        if (_args.Contains("-timelimit") && runSetup == false)
        {
            timeLimit = true;
            CanLaunch();
        }

        if (Process.GetProcessesByName("vrserver").Length == 0)
        {
            log.Info("VR not running.", InfoType.Debug);
            var argss = args.ToList();
            argss.Add("--no-vr");
            args = argss.ToArray();
            isVrRunning = false;
        }
        if (_args.Contains("--no-vr"))
        {
            log.Info("VR not running.", InfoType.Debug);
            isVrRunning = false;
        }

        #endregion



        if (runSetup == false)
        {
            LoadPrograms();

            //What you think it do?
            consoleComponents.Title = "Zetrex's VRC Utils";
            consoleComponents.Tip = "(Put \"-Setup\" in steam launch parameters for help/configuration)";

            Thread thread = new Thread(ConsoleTitle);
            thread.Start();

            if (launchPrograms) LaunchPrograms();

            LaunchVRChat(args);

            log.Info("Prosses Checks Starting...", InfoType.Loading);

            Thread VRChatCheck = new Thread(CheckVRChat);

            if (canRunVRC)
                VRChatCheck.Start();

            Console.ForegroundColor = ConsoleColor.Green;
            log.Info("Prosses Checks Initiated", InfoType.Complete);

            AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);

            Console.ReadKey();
        }
    }

    //Checks time, if time is 9:25pm or later, either crash the game or dont let it launch (This is mainly just for me, I like sleep lol. Is disabled by default)

    static void CanLaunch()
    {
        string LoginTime = config.TimeLimit.LoginTime;
        string LogoutTime = config.TimeLimit.LogoutTime;

        int inH;
        int outH;
        int inM;
        int outM;


        {
            string[] strings = LoginTime.Split(":");
            int tempH = Convert.ToInt32(strings[0]);
            inH = Convert.ToInt32(strings[0]);
            int tempM = Convert.ToInt32(strings[1]);
            inM = Convert.ToInt32(strings[1]);
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
            outH = Convert.ToInt32(strings[0]);
            outM = Convert.ToInt32(strings[1]);
        }

        int hour = DateTime.Now.Hour;
        int minute = DateTime.Now.Minute;

        DateTime date1 = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, inH, inM, 0);
        DateTime date2 = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, outH, outM, 0);
        DateTime now = DateTime.Now;

        int resultIn = DateTime.Compare(date1, now);
        int resultOut = DateTime.Compare(date2, now);

        if (resultIn > 0 || resultOut < 0)
        {
            log.Msg($"ITS TOO LATE TO GET ON! \nConsole will close in 5 seconds \n\nVRChat will *NOT* launch until {LoginTime}", ConsoleColor.Red);

            Thread.Sleep(5000);
            Environment.Exit(0);
        } else
        {
            Thread thread = new Thread(CheckTime);
            thread.Start();
        }
    }
    private static void CheckTime()
    {
        try
        {
            {
                int tries = 0;
                while (Convert.ToInt32(DateTime.Now.ToString("ffff")) != 0)
                {
                    //log.Info("Waiting for time to sync...", InfoType.Debug);
                    tries++;
                }

                log.Info($"Time Synced! Took {tries} tries.", InfoType.Complete);
            }

            while (true)
            {
                int tries = 0;
                while (Convert.ToInt32(DateTime.Now.ToString("ff")) != 0)
                {
                    tries++;
                }

                int outH;
                int outM;

                string[] strings = config.TimeLimit.LogoutTime.Split(":");
                outH = Convert.ToInt32(strings[0]);
                outM = Convert.ToInt32(strings[1]);

                DateTime date2 = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, outH, outM, 0);
                int hour = DateTime.Now.Hour;
                int minute = DateTime.Now.Minute;

                int secLeft = date2.Subtract(DateTime.Now).Seconds;
                int minLeft = date2.Subtract(DateTime.Now).Minutes;
                int hourLeft = date2.Subtract(DateTime.Now).Hours;

                float totalLeft = 1.00f / (float)date2.Subtract(DateTime.Now).TotalSeconds;

                string timeleft = $"{hourLeft.ToString("D2")}:{minLeft.ToString("D2")}:{secLeft.ToString("D2")}";

                consoleComponents.Timer = timeleft;

                if (oscrouter && Router.MainSender != null && Router.isRunning == true)
                {
                    Router.MainSender.SendValue("/avatar/parameters/TimeLeftSec", secLeft);
                    Router.MainSender.SendValue("/avatar/parameters/TimeLeftMin", minLeft);
                    Router.MainSender.SendValue("/avatar/parameters/TimeLeftHour", hourLeft);
                    Router.MainSender.SendValue("/avatar/parameters/TimeLeftFloat", totalLeft);

                    Task.Run(() =>
                    {
                        if (hourLeft == 1 && minLeft == 0 && secLeft == 0 || hourLeft == 0 && minLeft == 30 && secLeft == 0 || hourLeft == 0 && minLeft == 15 && secLeft == 0 || hourLeft == 0 && minLeft == 10 && secLeft == 0 || hourLeft == 0 && minLeft == 5 && secLeft == 0 || hourLeft == 0 && minLeft == 1 && secLeft == 0 || hourLeft == 0 && minLeft == 0 && secLeft == 30 || hourLeft == 0 && minLeft == 0 && secLeft == 15 || hourLeft == 0 && minLeft == 0 && secLeft < 6)
                            Router.MainSender.SendChatboxMsg("/chatbox/input", $"Time Left: {timeleft}", true);
                    });
                }

                if (DateTime.Compare(date2, DateTime.Now) <= 0)
                {
                    Environment.Exit(0);
                }
            }
        }
        catch (Exception ex)
        {
            log.Error($"Failed Tick! Error: {ex}");
            Thread.Sleep(5000);
        }
    }
    static void LaunchPrograms()
    {
        string[] files = Directory.GetFiles(programDir);

        if (files != null)
        {
            foreach (ProgramDetails program in programs)
            {
                _Prosses.Create(program.Name, program.Directory, program.runAdmin, program.restartOnClose, program.args, program.launchWithVR, program.launchWithDesktop);
            }
        }
    }

    //Launches VRChat
    static void LaunchVRChat(string[] args)
    {
        Process[] _pname = Process.GetProcessesByName("VRChat");
        if (_pname.Length == 0)
        {
            if (_args.Count > 0 && canRunVRC)
            {
                if (randomImg)
                    RandomImg();

                Thread.Sleep(2000);

                log.Info("Starting VRChat...", InfoType.Loading);

                try
                {
                    Process.Start($"{Environment.CurrentDirectory}\\EAC_Launch.exe", args);

                    Process[] pname = Process.GetProcessesByName("VRChat");
                    while (pname.Length == 0)
                    {
                        pname = Process.GetProcessesByName("VRChat");
                        Thread.Sleep(1000);
                    }

                    log.Info("VRChat Started", InfoType.Complete);

                    if (_args.Contains("-oscrouter"))
                    {
                        if (config.OSC.OptIP_Ports.Count == 1 && config.OSC.OptIP_Ports[0] == "" || config.OSC.OptIP_Ports == null)
                            Router.Create(config.OSC.OutIP_Port);
                        else
                            Router.Create(config.OSC.OutIP_Port, config.OSC.OptIP_Ports);
                        oscrouter = true;
                    }
                }
                catch (Exception e)
                {
                    log.Error($"Failed to start VRChat!\nError:{e}");
                    Console.ReadLine();
                }
            }
            else
            {
                if (canRunVRC)
                {
                    if (randomImg)
                        RandomImg();

                    Thread.Sleep(2000);

                    log.Info("Starting VRChat...", InfoType.Loading);

                    try
                    {
                        Process.Start($"{Environment.CurrentDirectory}\\EAC_Launch.exe");

                        Process[] pname = Process.GetProcessesByName("VRChat");
                        while (pname.Length == 0)
                        {
                            pname = Process.GetProcessesByName("VRChat");
                            Thread.Sleep(1000);
                        }

                        log.Info("VRChat Started", InfoType.Complete);

                        if (_args.Contains("-oscrouter"))
                        {
                            if (config.OSC.OptIP_Ports.Count == 1 && config.OSC.OptIP_Ports[0] == "" || config.OSC.OptIP_Ports == null)
                                Router.Create(config.OSC.OutIP_Port);
                            else
                                Router.Create(config.OSC.OutIP_Port, config.OSC.OptIP_Ports);
                            oscrouter = true;
                        }
                    }
                    catch (Exception e)
                    {
                        log.Error($"Failed to start VRChat!\nError:{e}");
                        Console.ReadLine();
                    }

                }
            }
        } else
        {
            if (_args.Contains("-oscrouter"))
            {
                if (config.OSC.OptIP_Ports.Count == 1 && config.OSC.OptIP_Ports[0] == "" || config.OSC.OptIP_Ports == null)
                    Router.Create(config.OSC.OutIP_Port);
                else
                    Router.Create(config.OSC.OutIP_Port, config.OSC.OptIP_Ports);
                oscrouter = true;
            }
        }
    }

    //Checks if VRChat is still running every 5 seconds. If not, close this.
    static void CheckVRChat()
    {
        while (true)
        {
            Process[] pname = Process.GetProcessesByName("VRChat");
            if (pname.Length == 0)
            {
                log.Msg("VRChat Not Found! Closing Application...");
                Environment.Exit(0);
            }
            Thread.Sleep(500);
        }
    }

    //Randomises the EAC backround
    static void RandomImg()
    {
        if (!Directory.Exists($"{Environment.CurrentDirectory}\\EasyAntiCheat\\RandomImg"))
        {
            Directory.CreateDirectory($"{Environment.CurrentDirectory}\\EasyAntiCheat\\RandomImg");
            File.Copy($"{Environment.CurrentDirectory}\\EasyAntiCheat\\SplashScreen.png", $"{Environment.CurrentDirectory}\\EasyAntiCheat\\RandomImg\\SplashScreen.png");
        }
            

        EACImg = $"{Environment.CurrentDirectory}\\EasyAntiCheat\\SplashScreen.png";
        EACImgRepo = $"{Environment.CurrentDirectory}\\EasyAntiCheat\\RandomImg";

        string[] images = Directory.GetFiles(EACImgRepo);

        Random rand = new Random();

        log.Msg("Getting random img from:\n");

        foreach (string image in images)
        {
            string[] name = image.Split('\\');

            ConsoleColor color = (ConsoleColor)rand.Next(1, 16);

            log.Msg(name[name.Length - 1], color);
        }

        {
            int winner = rand.Next(0, images.Length);

            string[] name = images[winner].Split('\\');

            File.Copy(images[winner], EACImg, true);

            Console.WriteLine("\n");
        }
    }

    static void CurrentDomain_ProcessExit(object sender, EventArgs e)
    {
        {
            Process[] pname = Process.GetProcessesByName("VRChat");
            if (pname.Length != 0)
                foreach (Process p in pname)
                    p.Kill();
        }

        foreach (_Prosses prosses in _Prosses.Prosseses)
        {
            Process[] pname = Process.GetProcessesByName(prosses.name);
            if (pname.Length != 0)
                foreach (Process p in pname)
                    p.Kill();
        }
    }

    static void LoadPrograms()
    {
        string[] files = Directory.GetFiles(programDir);

        if (files != null)
        {
            foreach (string file in files)
            {
                log.Info($"Loading: {file.Split("\\")[file.Split("\\").Length - 1]}", InfoType.Loading);
                if (file.Contains(".json"))
                {
                    ProgramDetails program = JsonConvert.DeserializeObject<ProgramDetails>(File.ReadAllText(file));

                    programs.Add(program);
                    log.Info($"{file.Split("\\")[file.Split("\\").Length - 1]} Loaded!", InfoType.Complete);
                } else
                {
                    log.Info($"{file.Split("\\")[file.Split("\\").Length - 1]} is not a valid program config. Skipping.", InfoType.Exception);
                }
            }
        }
    }

    public static void LoadConfig()
    {
        try
        {
            log.Info("Loading Config...", InfoType.Loading);
            string file = File.ReadAllText(_Config);

            if (string.IsNullOrEmpty(file) || file == "{}")
            {
                log.Warn($"Config invalid! Resetting to default.");
                config = Config.Create();
                string jsonString = JsonSerializer.Serialize(Amongus.config);

                File.WriteAllText(Amongus._Config, jsonString);

                log.Info(jsonString, InfoType.Debug);
            } else
            {
                config = JsonConvert.DeserializeObject<Config>(file);
                log.Info("Config Loaded!", InfoType.Complete);

                string jsonString = JsonSerializer.Serialize(Amongus.config);

                log.Info(jsonString, InfoType.Debug);
            }
        }
        catch (Exception ex)
        {
            log.Info($"Config invalid! Resetting to default.\nError: {ex}", InfoType.Exception);
            config = Config.Create();
            string jsonString = JsonSerializer.Serialize(Amongus.config);

            File.WriteAllText(Amongus._Config, jsonString);
        }
    }

    public class ConsoleComponents
    {
        public string Timer { get; set; }
        public string Title { get; set; }
        public string Tip { get; set; }
    }

    public static void ConsoleTitle()
    {
        if (timeLimit)
        {
            while (true)
            {
                Console.Title = $"Time left: {consoleComponents.Timer} | {consoleComponents.Title} | {consoleComponents.Tip}";
                Thread.Sleep(100);
            }
        }
        else
        {
            while (true)
            {
                Console.Title = $"{consoleComponents.Title} | {consoleComponents.Tip}";
                Thread.Sleep(100);
            }
        }
    }
}

