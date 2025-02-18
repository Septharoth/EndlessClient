﻿using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Extensions;
using EOLib.Net;
using EOLib.Net.Handlers;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.Items
{
    [AutoMappedType]
    public class JunkItemHandler : InGameOnlyPacketHandler
    {
        private readonly ICharacterRepository _characterRepository;
        private readonly ICharacterInventoryRepository _inventoryRepository;
        private readonly IEnumerable<IMainCharacterEventNotifier> _mainCharacterEventNotifiers;

        public override PacketFamily Family => PacketFamily.Item;

        public override PacketAction Action => PacketAction.Junk;

        public JunkItemHandler(IPlayerInfoProvider playerInfoProvider,
                               ICharacterRepository characterRepository,
                               ICharacterInventoryRepository inventoryRepository,
                               IEnumerable<IMainCharacterEventNotifier> mainCharacterEventNotifiers)
            : base(playerInfoProvider)
        {
            _characterRepository = characterRepository;
            _inventoryRepository = inventoryRepository;
            _mainCharacterEventNotifiers = mainCharacterEventNotifiers;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var id = packet.ReadShort();
            var amountRemoved = packet.ReadThree();
            var amountRemaining = packet.ReadInt();
            var weight = packet.ReadChar();
            var maxWeight = packet.ReadChar();

            var inventoryItem = _inventoryRepository.ItemInventory.OptionalSingle(x => x.ItemID == id);
            if (inventoryItem.HasValue)
            {
                _inventoryRepository.ItemInventory.Remove(inventoryItem.Value);

                if (amountRemaining > 0)
                {
                    var updatedItem = inventoryItem.Value.WithAmount(amountRemaining);
                    _inventoryRepository.ItemInventory.Add(updatedItem);
                }
            }

            var stats = _characterRepository.MainCharacter.Stats;
            stats = stats.WithNewStat(CharacterStat.Weight, weight)
                .WithNewStat(CharacterStat.MaxWeight, maxWeight);

            _characterRepository.MainCharacter = _characterRepository.MainCharacter.WithStats(stats);

            foreach (var notifier in _mainCharacterEventNotifiers)
                notifier.JunkItem(id, amountRemoved);

            return true;
        }
    }
}
