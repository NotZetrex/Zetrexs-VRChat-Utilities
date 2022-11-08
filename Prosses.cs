using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace start_protected_game
{
    internal class _Prosses
    {
        public static List<_Prosses> Prosseses = new List<_Prosses>();

        public string name;
        string directory;
        string[] args;

        bool runAdmin;
        bool restartOnClose;
        bool launchInVR;
        bool launchInDesktop;

        Logger log;

        void Launch()
        {
            //Checks if prosses is already running. If so, kill it.
            Process[] pname = Process.GetProcessesByName(name);
            if (pname.Length != 0)
                foreach (Process p in pname)
                    p.Kill(); 

            if (Amongus.isVrRunning && launchInVR || !Amongus.isVrRunning && launchInDesktop)
            {
                log.Info("Starting...", InfoType.Loading);

                try
                {
                    ProcessStartInfo info = new ProcessStartInfo($"{directory}\\{name}.exe");


                    if (runAdmin)
                    {
                        info.UseShellExecute = true;
                        info.Verb = "runas";
                    }
                    Process.Start(info);


                    Process[] pnamee = Process.GetProcessesByName(name);
                    while (pnamee.Length == 0)
                    {
                        pnamee = Process.GetProcessesByName(name);
                        Thread.Sleep(1000);
                    }

                    log.Info("Started!", InfoType.Complete);
                }
                catch (Exception e)
                {
                    log.Error($"Failed to start prosses!\nError:{e}");
                    throw;
                }

                Thread ProssesCheckThread = new Thread(ProssesCheck);
                ProssesCheckThread.Start();
            }
            else if (!Amongus.isVrRunning && launchInVR)
            {
                log.Info("Did not start because VR is not running.", InfoType.Complete);
            }
            else if (!Amongus.isVrRunning && launchInDesktop)
            {
                log.Info("Did not start because VR is running.", InfoType.Complete);
            }
        }

        void ProssesCheck()
        {
            bool isStarted = true;

            while (isStarted)
            {
                Process[] pname = Process.GetProcessesByName(name);

                if (pname.Length == 0)
                {
                    isStarted = false;

                    log.Info("Prosses not found! Restarting...", InfoType.Exception);
                    Launch();
                }

                Thread.Sleep(1000);
            }
        }

        public void OscRestart(string address, object value)
        {
            if (address == $"/avatar/parameters/Restart{name}" && OSC.Router.oscToPrimitive(value) is bool && (bool)OSC.Router.oscToPrimitive(value) == true)
            {
                log.Info("Restarting!", InfoType.Loading);
                Launch();
            }
        }

        public static void Create(string name, string directory, bool runAdmin, bool restartOnClose, string[] args, bool vr, bool desktop)
        {
            _Prosses prosses = new _Prosses();

            prosses.name = name;
            prosses.runAdmin = runAdmin;
            prosses.directory = directory;
            prosses.args = args;
            prosses.restartOnClose = restartOnClose;
            prosses.launchInVR = vr;
            prosses.launchInDesktop = desktop;
            prosses.log = Logger.Instance(name);

            Prosseses.Add(prosses);
            prosses.Launch();
        }

        public static _Prosses Create_Get(string name, string directory, bool runAdmin, bool restartOnClose, string[] args, bool vr, bool desktop)
        {
            _Prosses prosses = new _Prosses();

            prosses.name = name;
            prosses.runAdmin = runAdmin;
            prosses.directory = directory;
            prosses.args = args;
            prosses.restartOnClose = restartOnClose;
            prosses.launchInVR = vr;
            prosses.launchInDesktop = desktop;
            prosses.log = Logger.Instance(name);

            Prosseses.Add(prosses);

            return prosses;
        }
    }
}
