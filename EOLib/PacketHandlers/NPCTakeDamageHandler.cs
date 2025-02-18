﻿using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.Net;
using EOLib.Net.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EOLib.PacketHandlers
{
    public abstract class NPCTakeDamageHandler : InGameOnlyPacketHandler
    {
        private readonly ICharacterRepository _characterRepository;
        private readonly ICurrentMapStateRepository _currentMapStateRepository;
        private readonly IEnumerable<INPCActionNotifier> _npcNotifiers;

        public override PacketAction Action => PacketAction.Reply;

        public NPCTakeDamageHandler(IPlayerInfoProvider playerInfoProvider,
                                    ICharacterRepository characterRepository,
                                    ICurrentMapStateRepository currentMapStateRepository,
                                    IEnumerable<INPCActionNotifier> npcNotifiers)
            : base(playerInfoProvider)
        {
            _characterRepository = characterRepository;
            _currentMapStateRepository = currentMapStateRepository;
            _npcNotifiers = npcNotifiers;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var spellId = Optional<int>.Empty;
            if (Family == PacketFamily.Cast)
                spellId = packet.ReadShort();

            var fromPlayerId = packet.ReadShort();
            var fromDirection = (EODirection)packet.ReadChar();
            var npcIndex = packet.ReadShort();
            var damageToNpc = packet.ReadThree();
            var npcPctHealth = packet.ReadShort();

            var fromTp = -1;
            if (Family == PacketFamily.Cast)
                fromTp = packet.ReadShort();
            else if (packet.ReadChar() != 1) //some constant 1 in EOSERV
                return false;

            if (fromPlayerId == _characterRepository.MainCharacter.ID)
            {
                var renderProps = _characterRepository.MainCharacter.RenderProperties;
                var character = _characterRepository.MainCharacter.WithRenderProperties(renderProps.WithDirection(fromDirection));

                if (fromTp > 0)
                {
                    var stats = _characterRepository.MainCharacter.Stats;
                    character = character.WithStats(stats.WithNewStat(CharacterStat.TP, fromTp));
                }

                _characterRepository.MainCharacter = character;
            }
            else if(_currentMapStateRepository.Characters.ContainsKey(fromPlayerId))
            {
                var renderProps = _currentMapStateRepository.Characters[fromPlayerId].RenderProperties.WithDirection(fromDirection);
                var updatedCharacter = _currentMapStateRepository.Characters[fromPlayerId].WithRenderProperties(renderProps);
                _currentMapStateRepository.Characters[fromPlayerId] = updatedCharacter;
            }

            // todo: this has the potential to bug out if the opponent ID is never reset and the player dies/leaves
            try
            {
                var npc = _currentMapStateRepository.NPCs.Single(x => x.Index == npcIndex);
                var newNpc = npc.WithOpponentID(fromPlayerId);
                _currentMapStateRepository.NPCs.Remove(npc);
                _currentMapStateRepository.NPCs.Add(newNpc);
                foreach (var notifier in _npcNotifiers)
                    notifier.NPCTakeDamage(npcIndex, fromPlayerId, damageToNpc, npcPctHealth, spellId);
            }
            catch (InvalidOperationException) { return false; }

            return true;
        }
    }

    [AutoMappedType]
    public class NPCTakeWeaponDamageHandler : NPCTakeDamageHandler
    {
        public override PacketFamily Family => PacketFamily.NPC;

        public NPCTakeWeaponDamageHandler(IPlayerInfoProvider playerInfoProvider,
                                          ICharacterRepository characterRepository,
                                          ICurrentMapStateRepository currentMapStateRepository,
                                          IEnumerable<INPCActionNotifier> npcNotifiers)
            : base(playerInfoProvider, characterRepository, currentMapStateRepository, npcNotifiers) { }
    }

    [AutoMappedType]
    public class NPCTakeSpellDamageHandler : NPCTakeDamageHandler
    {
        public override PacketFamily Family => PacketFamily.Cast;

        public NPCTakeSpellDamageHandler(IPlayerInfoProvider playerInfoProvider,
                                         ICharacterRepository characterRepository,
                                         ICurrentMapStateRepository currentMapStateRepository,
                                         IEnumerable<INPCActionNotifier> npcNotifiers)
            : base(playerInfoProvider, characterRepository, currentMapStateRepository, npcNotifiers) { }
    }
}
