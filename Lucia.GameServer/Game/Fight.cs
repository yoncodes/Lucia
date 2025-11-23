using Lucia.Common.MsgPack;

namespace Lucia.GameServer.Game
{
    public class Fight
    {
        public PreFightRequest PreFight { get; set; }

        public Fight(PreFightRequest preFight)
        {
            PreFight = preFight;
        }
    }
}
