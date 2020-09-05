/* Rising Storm 2 Vietnam Server Vietnam Plugin made by s3nts
Copyright (c) 2020 s3nts - MIT License
setup help https://wiki.rs2vietnam.com/index.php?title=DedicatedServer 
and http://wiki.tripwireinteractive.com/index.php/RO2_Dedicated_Server
trust me you will use both for making the server be functioning
*/

using System;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using WindowsGSM.Functions;
using WindowsGSM.GameServer.Engine;
using WindowsGSM.GameServer.Query;
using System.IO;

namespace WindowsGSM.Plugins
{



    public class RS2 : SteamCMDAgent
    {
        // - Plugin Details
        public Plugin Plugin = new Plugin
        {
            name = "WindowsGSM.RS2", // WindowsGSM.XXXX
            author = "s3nts",
            description = "Rising Storm 2 Vietnam Server for all your war crime 🔥 needs",
            version = "1.0.5",
            url = "https://github.com/s3nts/WindowsGSM.RS2", // Github repository link (Best practice)
            color = "#9eff99" // Color Hex
        };


        // - Standard Constructor and properties
        public RS2(ServerConfig serverData) : base(serverData) => base.serverData = _serverData = serverData;
        private readonly ServerConfig _serverData; // Store server start metadata, such as start ip, port, start param, etc

        public override bool loginAnonymous => true;
        public override string AppId => "418480";


        //vars
        public override string StartPath => @"Binaries\win64\vngame.exe";
        public string FullName = "Rising Storm 2: Vietnam Dedicated Server";
        public bool AllowsEmbedConsole = true;
        public int PortIncrements = 2;
        public object QueryMethod = new A2S();



        // defualt values
        public string Port = "7777";
        public string QueryPort = "27105";
        //trust me its better map than starting on hamburger hill
        public string Defaultmap = "VNTE-CuChi";
        public string Maxplayers = "64";
        //web admin port since game has no rcon
        public string Additional = " -WebAdminPort=8080 ";



        // just do nothing 
        public async void CreateServerCFG() { }


        // - Start server 
        public async Task<Process> Start()
        {

            string shipExePath = ServerPath.GetServersServerFiles(_serverData.ServerID, StartPath);
            if (!File.Exists(shipExePath))
            {
                base.Error = $"{Path.GetFileName(shipExePath)} not found ({shipExePath})";
                return null;
            }
            // Prepare start parameter
           string param = string.IsNullOrWhiteSpace(_serverData.ServerMap) ? string.Empty : _serverData.ServerMap;
         param += string.IsNullOrWhiteSpace(_serverData.ServerPort) ? string.Empty : $" -Port={_serverData.ServerPort}";
           param += string.IsNullOrWhiteSpace(_serverData.ServerIP) ? string.Empty : $" -MultiHome={_serverData.ServerIP}";
          param += string.IsNullOrWhiteSpace(_serverData.ServerQueryPort) ? string.Empty : $" -QueryPort={_serverData.ServerQueryPort}";
             param += $"{_serverData.ServerParam}";
            param += $" ";
            //base.Error = param;
            //return null;
            Process p;
            // Prepare Process
            if (!AllowsEmbedConsole)
            {
                p = new Process
                {
                    StartInfo =
                   {
                       FileName = shipExePath,
                       Arguments = param,
                       WindowStyle = ProcessWindowStyle.Minimized,
                       UseShellExecute = false
                   },
                    EnableRaisingEvents = true
                };


                try
                {
                    p.Start();
                    return p;
                }
                catch (Exception e)
                {
                    base.Error = e.Message;
                    return null; // return null if fail to start
                }
            }
            else
            {
                p = new Process
                {
                    StartInfo =
                   {
                       FileName = shipExePath,
                       Arguments = param,
                       WindowStyle = ProcessWindowStyle.Hidden,
                       CreateNoWindow = true,
                       UseShellExecute = false,
                       RedirectStandardOutput = true,
                       RedirectStandardError = true
                   },
                    EnableRaisingEvents = true
                };

                try
                {
                    p.Start();
                    var serverConsole = new ServerConsole(_serverData.ServerID);
                    p.OutputDataReceived += serverConsole.AddOutput;
                    p.ErrorDataReceived += serverConsole.AddOutput;
                    p.BeginOutputReadLine();
                    p.BeginErrorReadLine();
                    return p;
                }
                catch (Exception e)
                {
                    base.Error = e.Message;
                    return null; // return null if fail to start
                }
            }


            // Start Process
         
        }

        public async Task Stop(Process p)
        {
            await Task.Run(() =>
            {
                if (p.StartInfo.CreateNoWindow)
                {
                    p.Kill();
                }
                else
                {
                    p.CloseMainWindow();
                }
            });
        }
    }
}