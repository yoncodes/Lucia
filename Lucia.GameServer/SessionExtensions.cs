using Lucia.Common.Database;
using Lucia.Common.MsgPack;
using Lucia.Common.Util;
using Lucia.Table.V2.share.player;

namespace Lucia.GameServer
{
    public static class SessionExtensions
    {
        /// <summary>
        /// Please invoke after messing with TeamExp(commandant exp) count!
        /// </summary>
        /// <param name="session"></param>
        public static void ExpSanityCheck(this Session session)
        {
            Item? expItem = session.inventory.Items.FirstOrDefault(x => x.Id == Inventory.TeamExp);
            if (expItem is null)
                return;

            PlayerTable? playerLevel = TableReaderV2.Parse<PlayerTable>().FirstOrDefault(x => x.Level == session.player.PlayerData.Level);
            if (playerLevel is null)
                return;

            if (expItem.Count < playerLevel.MaxExp)
                return;

            expItem.Count -= playerLevel.MaxExp;
            session.player.PlayerData.Level++;

            NotifyPlayerLevel notifyPlayerLevel = new()
            {
                Level = (int)session.player.PlayerData.Level
            };
            NotifyItemDataList notifyItemDataList = new();
            notifyItemDataList.ItemDataList.Add(expItem);
            notifyItemDataList.ItemDataList.Add(session.inventory.Do(Inventory.ActionPoint, playerLevel.FreeActionPoint));

            session.SendPush(notifyPlayerLevel);
            session.SendPush(notifyItemDataList);

            session.ExpSanityCheck();
        }
    }
}
