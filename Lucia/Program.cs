using Lucia.GameServer;
using Lucia.GameServer.Handlers;
using Lucia.GameServer.Commands;
using Lucia.Logging;

namespace Lucia
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // TODO: Add LogLevel parsing from appsettings file
            LoggerFactory.InitializeLogger(new Logger(typeof(Program), LogLevel.DEBUG, LogLevel.DEBUG));
            LoggerFactory.Logger.Info("Starting...");

#if DEBUG
            if (Common.Common.config.VerboseLevel < Common.VerboseLevel.Debug)
                Common.Common.config.VerboseLevel = Common.VerboseLevel.Debug;
#endif

            PacketFactory.LoadPacketHandlers();
            CommandFactory.LoadCommands();

            // Start game server
            _ = Task.Run(Server.Instance.Start);

            // Start SDK/HTTP server 
            await Task.Run(() => SDKServer.SDKServer.Main(args));

            AppDomain.CurrentDomain.ProcessExit += new EventHandler(KillProtocol);
        }

        static void KillProtocol(object? sender, EventArgs e)
        {
            LoggerFactory.Logger.Info("Shutting down...");

            foreach (var session in Server.Instance.Sessions)
            {
                session.Value.SendPush(new ShutdownNotify());
                session.Value.DisconnectProtocol();
            }
        }
    }
}