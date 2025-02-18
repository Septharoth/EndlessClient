﻿using System.Collections.Generic;
using System.Linq;
using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;

namespace EOLib.Net.Translators
{
    [AutoMappedType]
    public class LoginRequestCompletedPacketTranslator : MapStatePacketTranslator<ILoginRequestCompletedData>
    {
        private const int MAX_NEWS_LINES = 9;

        public LoginRequestCompletedPacketTranslator(ICharacterFromPacketFactory characterFromPacketFactory)
            : base(characterFromPacketFactory) { }

        public override ILoginRequestCompletedData TranslatePacket(IPacket packet)
        {
            var reply = (CharacterLoginReply)packet.ReadShort();

            if (reply == CharacterLoginReply.RequestDenied)
            {
                if (packet.ReadEndString() != "NO")
                    throw new MalformedPacketException("Expected NO bytes in CharacterLoginReply login", packet);

                return new LoginRequestCompletedData().WithError(reply);
            }
            else if (reply != CharacterLoginReply.RequestCompleted)
            {
                throw new MalformedPacketException("Unexpected welcome response in packet: " + reply, packet);
            }

            if (packet.ReadByte() != 255)
                throw new MalformedPacketException("Missing 255 byte separator after CharacterLoginReply type", packet);

            var news = GetNews(packet).ToList();

            var weight = packet.ReadChar();
            var maxWeight = packet.ReadChar();

            var inventoryItems = GetInventoryItems(packet).ToList();
            var inventorySpells = GetInventorySpells(packet).ToList();

            if (inventoryItems.All(x => x.ItemID != 1))
                inventoryItems.Add(new InventoryItem(1, 0));

            var characters = GetCharacters(packet).ToList();
            var npcs = GetNPCs(packet).ToList();
            var items = GetMapItems(packet).ToList();

            return new LoginRequestCompletedData()
                .WithNews(news)
                .WithWeight(weight)
                .WithMaxWeight(maxWeight)
                .WithInventory(inventoryItems)
                .WithSpells(inventorySpells)
                .WithCharacters(characters)
                .WithNPCs(npcs)
                .WithItems(items);
        }

        private IEnumerable<string> GetNews(IPacket packet)
        {
            for (int i = 0; i < MAX_NEWS_LINES; ++i)
                yield return packet.ReadBreakString();
        }

        private IEnumerable<IInventoryItem> GetInventoryItems(IPacket packet)
        {
            while (packet.PeekByte() != 255)
                yield return new InventoryItem(packet.ReadShort(), packet.ReadInt());

            packet.ReadByte();
        }

        private IEnumerable<IInventorySpell> GetInventorySpells(IPacket packet)
        {
            while (packet.PeekByte() != 255)
                yield return new InventorySpell(packet.ReadShort(), packet.ReadShort());

            packet.ReadByte();
        }
    }
}
