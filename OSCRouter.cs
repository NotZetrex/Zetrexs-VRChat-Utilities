using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;
using CoreOSC;
using CoreOSC.IO;
using Newtonsoft.Json.Linq;
using VRCOSC.Game.Modules;

namespace start_protected_game.OSC
{
    public class Router
    {
        static Logger log = Logger.Instance("OSC");

        public static Router Listener;
        public static Router MainSender;

        public static bool isRunning = false;

        private string _ip;
        private int port;

        //Code originally from VRCOSC 
        private Socket? sendingClient;
        private Socket? receivingClient;
        private CancellationTokenSource? tokenSource;
        private Task? incomingTask;

        public Action<string, object>? OnParameterSent;
        public Action<string, object>? OnParameterReceived;

        public static void Create(string VRCOut, List<string> Outputs = null)
        {
            {
                string ip = VRCOut.Split(":")[1];
                int outport = Convert.ToInt32(VRCOut.Split(":")[2]);

                log.Info($"Starting listener on {ip}:{outport}...", InfoType.Loading);

                Listener = new Router();
                Listener.Initialise(ip, outport);

                Listener.Enable();

                Listener._ip = ip;
                Listener.port = outport;

                foreach (_Prosses prosses in _Prosses.Prosseses)
                {
                    Listener.OnParameterReceived += prosses.OscRestart;
                }

                log.Info($"Listener started on {ip}:{outport}!", InfoType.Complete);
            }
            {
                string ip = VRCOut.Split(":")[1];
                int inport = Convert.ToInt32(VRCOut.Split(":")[0]);

                log.Info($"Starting sender for {ip}:{inport}...", InfoType.Loading);

                MainSender = new Router();
                MainSender.Initialise_Sender(ip, inport);

                MainSender._ip = ip;
                MainSender.port = inport;

                MainSender.OnParameterSent += MainSender.OnSent;

                log.Info($"Sender for {ip}:{inport} started!", InfoType.Complete);
            }
            if (Outputs != null)
            {
                foreach (string s in Outputs)
                {
                    string ip = s.Split(":")[0];
                    int inport = Convert.ToInt32(s.Split(":")[1]);

                    log.Info($"Starting router for {ip}:{inport}...", InfoType.Loading);

                    Router Sender = new Router();
                    Sender.Initialise_Sender(ip, inport);

                    Sender._ip = ip;
                    Sender.port = inport;

                    Sender.OnParameterSent += Sender.OnSent;

                    Listener.OnParameterReceived += Sender.SendValue;

                    log.Info($"Router for {ip}:{inport} started!", InfoType.Complete);
                }
                Listener.OnParameterReceived += Listener.OnReceive;
            }
        }

        public void Initialise(string ipAddress, int receivePort)
        {
            receivingClient = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            receivingClient.Bind(new IPEndPoint(IPAddress.Parse(ipAddress), receivePort));
        }

        public void Initialise_Sender(string ipAddress, int sendPort)
        {
            sendingClient = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            sendingClient.Connect(new IPEndPoint(IPAddress.Parse(ipAddress), sendPort));
        }

        public void Enable()
        {
            tokenSource = new CancellationTokenSource();
            incomingTask = Task.Run(runReceiveLoop);
        }

        public async Task DisableReceive()
        {
            tokenSource?.Cancel();

            if (incomingTask is not null) await incomingTask;

            incomingTask?.Dispose();
            tokenSource?.Dispose();
            receivingClient?.Close();

            incomingTask = null;
            tokenSource = null;
            receivingClient = null;
        }

        public void OnSent(string Address, object value)
        {
            log.Info($"S {_ip}:{port}{Address} with value of {oscToPrimitive(value)} ({value.GetType()})", InfoType.Debug);
        }

        public void OnReceive(string Address, object value)
        {
            log.Info($"R {_ip}:{port}{Address} with value of {oscToPrimitive(value)} ({value.GetType()})", InfoType.Debug);
        }

        public void DisableSend()
        {
            sendingClient?.Close();
            sendingClient = null;
        }

        public void SendValue<T>(string oscAddress, T value)
        {
            sendingClient?.SendOscMessage(new OscMessage(new Address(oscAddress), new[] { primitiveToOsc(value) }));
            OnParameterSent?.Invoke(oscAddress, value);
        }
        public void SendChatboxMsg(string oscAddress, string text, bool sendImmediately)
        {

            sendingClient?.SendOscMessage(new OscMessage(new Address(oscAddress), new[] { primitiveToOsc(text), primitiveToOsc(sendImmediately) }));
            OnParameterSent?.Invoke(oscAddress, $"Text: {text}, Send Immediately: {sendImmediately}");
        }

        private async void runReceiveLoop()
        {
            isRunning = true;
            try
            {
                while (!tokenSource!.Token.IsCancellationRequested)
                {
                    var message = await receivingClient!.ReceiveOscMessage(tokenSource.Token);

                    // if values arrive too fast, vrc can occasionally send data without a value
                    if (!message.Arguments.Any()) continue;

                    OnParameterReceived?.Invoke(message.Address.Value, oscToPrimitive(message.Arguments.First()));
                }
            }
            catch (OperationCanceledException) { }
        }

        public static object primitiveToOsc(object value)
        {
            if (value is bool boolValue) return boolValue ? OscTrue.True : OscFalse.False;

            return value;
        }

        public static object oscToPrimitive(object value)
        {
            return value switch
            {
                OscTrue => true,
                OscFalse => false,
                _ => value
            };
        }
    }
}
