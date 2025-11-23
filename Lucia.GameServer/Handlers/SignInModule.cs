using Lucia.Common.MsgPack;

namespace Lucia.GameServer.Handlers
{
    internal class SignInModule
    {
        [RequestPacketHandler("SignInRequest")]
        public static void SignInRequestHandler(Session session, Packet.Request packet)
        {
            SignInResponse signInResponse = new();
            session.SendResponse(signInResponse, packet.Id);
        }
    }
}
