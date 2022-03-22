using FiremonSystray.Utils;
using Newtonsoft.Json.Linq;
using SocketIOSharp.Common;
using SocketIOSharp.Server;
using SocketIOSharp.Server.Client;
using System;
using System.Collections.Generic;
using System.Deployment.Application;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using static FiremonSystray.Utils.HelperFunctions;

namespace FiremonSystray.Websocket
{
    public class SocketServer
    {
        private int port;
        private SocketIOServer socketServer;

        public SocketServer(int port)
        {
            this.port = port;
        }

        public void Start()
        {
            this.socketServer = new SocketIOServer(new SocketIOServerOption((ushort)this.port));
            socketServer.OnConnection(OnConnection);
            socketServer.Start();

            int HDMI_Monitors = HelperFunctions.GetHDMIMonitorCount();
            Console.WriteLine("Number of connected HDMI cables : " + HDMI_Monitors.ToString());
        }

        private void OnConnection(SocketIOSocket socket)
        {
            Console.WriteLine("Client connected!");

            socket.On("DISPLAY", OnDisplayHandler);
            socket.On("REBOOT", OnRebootHandler);
            socket.On("SHUTDOWN", OnShutdownHandler);
            socket.On("SELFUPDATE", OnSelfUpdateHandler);
            socket.On("RESETIDENT", OnResetIdentHandler);
            socket.On("HEARTBEAT", OnHeartBeatHandler);

            socket.On("VERSION", OnVersionHandler);
            socket.On("RESOLUTION", OnResolutionHandler);
            socket.On("CLEANUP", OnCleanUpHandler);
            socket.On("PRINT", OnPrintHandler);

            socket.On("input", (data) =>
            {
                foreach (JToken token in data)
                {
                    Console.Write(token + " ");
                }

                Console.WriteLine();
                socket.Emit("echo", data);
            });

            Timer heartbeatTimer = new Timer(5000);
            heartbeatTimer.Elapsed += heartbeatTick;
            heartbeatTimer.Start();

            socket.On(SocketIOEvent.DISCONNECT, () =>
            {
                Console.WriteLine("Client disconnected!");
            });
        }

        private void heartbeatTick(object sender, ElapsedEventArgs e)
        {
            SendMessageToAllClients("HEARTBEAT", "request");
        }

        private void SendMessageToAllClients(string method, string data)
        {
            socketServer.Clients.ForEach(x =>
            {
                try
                {
                    if (x.ReadyState == EngineIOSharp.Common.Enum.EngineIOReadyState.OPEN)
                        x.Emit(method, data);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });
        }

        private void OnPrintHandler()
        {
        }

        private void OnCleanUpHandler()
        {
        }

        private void OnResolutionHandler()
        {
            var width = System.Windows.SystemParameters.WorkArea.Width;
            var height = System.Windows.SystemParameters.WorkArea.Height;
            this.socketServer.Emit("RESOLUTION", "{" + $" \"width\": {width}, \"height\": {height}, \"tried_cec\": false, \"tried_legacy\": false, \"success\": false " + "}");
        }

        private void OnVersionHandler()
        {
            SendMessageToAllClients("VERSION", ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString());
        }

        private void OnHeartBeatHandler(JToken[] token)
        {
            Console.WriteLine("received HEARTBEAT");
        }

        private void OnResetIdentHandler(JToken[] token)
        {
            this.socketServer.Emit("RESETIDENT", token);
        }

        private void OnSelfUpdateHandler(JToken[] token)
        {
            HelperFunctions.InstallUpdate();
        }

        private void OnShutdownHandler(JToken[] token)
        {
            HelperFunctions.ShutdownPC();
        }

        private void OnRebootHandler(JToken[] token)
        {
            HelperFunctions.RebootPC();
        }

        private void OnDisplayHandler(JToken[] token)
        {
            foreach (JToken j in token)
            {
                switch (j.ToString())
                {
                    // HDMI-CEC
                    case "off":
                        HelperFunctions.SetMonitorInState(MonitorState.MonitorStateOff);
                        break;
                    case "on":
                        HelperFunctions.SetMonitorInState(MonitorState.MonitorStateOn);
                        break;

                    // VGA
                    case "pc-off":
                        //HelperFunctions.SetMonitorInState(MonitorState.MonitorStateStandBy);
                        HelperFunctions.SetMonitorInState(MonitorState.MonitorStateOff);

                        break;
                    case "pc-on":
                        HelperFunctions.SetMonitorInState(MonitorState.MonitorStateOn);

                        break;
                }
                Console.Write(j + " ");
            }
        }
    }
}
