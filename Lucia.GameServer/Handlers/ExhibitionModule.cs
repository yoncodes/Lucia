using MessagePack;
using Lucia.Common.MsgPack;
using Lucia.Common.Util;
using Lucia.Table.V2.share.exhibition;
using Lucia.Table.V2.share.reward;

namespace Lucia.GameServer.Handlers
{
    #region MsgPackScheme
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    [MessagePackObject(true)]
    public class GatherRewardRequest
    {
        public int Id;
    }

    [MessagePackObject(true)]
    public class GatherRewardResponse
    {
        public int Code;
        public List<RewardGoods> RewardGoods { get; set; } = new();
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    #endregion

    internal class ExhibitionModule
    {
        [RequestPacketHandler("GatherRewardRequest")]
        public static void HandleGatherRewardRequestHandler(Session session, Packet.Request packet)
        {
            GatherRewardRequest req = MessagePackSerializer.Deserialize<GatherRewardRequest>(packet.Content);

            var exhibitionReward = TableReaderV2.Parse<ExhibitionRewardTable>().Find(x => x.Id == req.Id);

            var subIds = TableReaderV2.Parse<RewardTable>()
                .Find(x => x.Id == exhibitionReward?.RewardId)?
                .SubIds
                .ParseIntList()
                ?? new List<int>();

            var rewards = TableReaderV2.Parse<RewardGoodsTable>()
                .Where(x => subIds.Contains(x.Id));

            GatherRewardResponse rsp = new();

            foreach (var rewardGoods in rewards)
            {
                int rewardTypeVal = (int)MathF.Floor((rewardGoods.TemplateId > 0 ? rewardGoods.TemplateId : rewardGoods.Id) / 1000000) + 1;

                rsp.RewardGoods.Add(new()
                {
                    Id = rewardGoods.Id,
                    TemplateId = rewardGoods.TemplateId,
                    Count = rewardGoods.Count ?? 0,
                    RewardType = rewardTypeVal
                });

                if ((RewardType)rewardTypeVal == RewardType.Item)
                {
                    NotifyItemDataList notifyItemData = new()
                    {
                        ItemDataList = { session.inventory.Do(rewardGoods.TemplateId, rewardGoods.Count ?? 0) }
                    };
                    session.SendPush(notifyItemData);
                }
            }

            session.player.GatherRewards.Add(req.Id);
            session.SendPush(new NotifyGatherReward() { Id = req.Id });
            session.SendResponse(rsp, packet.Id);
        }

    }
}
