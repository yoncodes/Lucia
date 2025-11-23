using Lucia.Common.Database;
using Lucia.Common.Util;
using Lucia.Common.MsgPack;
using Lucia.GameServer.Handlers;
using Lucia.Table.V2.share.character;

namespace Lucia.GameServer.Commands
{
    [CommandName("character")]
    internal class CharacterCommand : Command
    {
        public CharacterCommand(Session session, string[] args, bool validate = true) : base(session, args, validate) { }

        public override string Help => "Command to modify characters.";

        [Argument(0, @"^add$", "The operation selected (add)")]
        string Op { get; set; } = string.Empty;

        [Argument(1, @"^[0-9]+$|^all$", "The target character, value is character id or 'all'")]
        string Target { get; set; } = string.Empty;

        public override void Execute()
        {
            int id = Miscs.ParseIntOr(Target);
            List<AddCharacterRet> rets = new();

            switch (Op)
            {
                case "add":
                    if (Target == "all")
                    {
                        foreach (var characterData in TableReaderV2.Parse<CharacterTable>().Where(x => !session.character.Characters.Any(y => y.Id == x.Id)))
                            rets.Add(session.character.AddCharacter((uint)characterData.Id));
                    }
                    else
                    {
                        if (!session.character.Characters.Any(c => c.Id == id))
                            rets.Add(session.character.AddCharacter((uint)id));
                    }

                    NotifyEquipDataList notifyEquipData = new();
                    FashionSyncNotify fashionSync = new();
                    NotifyCharacterDataList notifyCharacterData = new();

                    notifyEquipData.EquipDataList.AddRange(rets.Select(x => x.Equip));
                    fashionSync.FashionList.AddRange(rets.Select(x => x.Fashion));
                    notifyCharacterData.CharacterDataList.AddRange(rets.Select(x => x.Character));
                    session.SendPush(notifyEquipData);
                    session.SendPush(fashionSync);
                    session.SendPush(notifyCharacterData);
                    break;
                default:
                    throw new InvalidOperationException("Invalid operation!");
            }
        }
    }
}
