﻿using System;
using System.Collections.Generic;
using System.Linq;
using EOLib.Domain.Character;
using EOLib.Domain.Map;
using EOLib.Domain.NPC;

namespace EOLib.Domain.Login
{
    public class LoginRequestCompletedData : ILoginRequestCompletedData
    {
        public IReadOnlyList<string> News { get; private set; }

        public byte CharacterWeight { get; private set; }

        public byte CharacterMaxWeight { get; private set; }

        public IReadOnlyList<IInventoryItem> CharacterItemInventory { get; private set; }

        public IReadOnlyList<IInventorySpell> CharacterSpellInventory { get; private set; }

        public IReadOnlyList<ICharacter> MapCharacters { get; private set; }

        public IReadOnlyList<INPC> MapNPCs { get; private set; }

        public IReadOnlyList<IItem> MapItems { get; private set; }

        public CharacterLoginReply Error { get; private set; }

        public ILoginRequestCompletedData WithNews(IEnumerable<string> newsStrings)
        {
            var copy = MakeCopy(this);
            copy.News = newsStrings.ToList();
            return copy;
        }

        public ILoginRequestCompletedData WithWeight(byte weight)
        {
            var copy = MakeCopy(this);
            copy.CharacterWeight = weight;
            return copy;
        }

        public ILoginRequestCompletedData WithMaxWeight(byte maxWeight)
        {
            var copy = MakeCopy(this);
            copy.CharacterMaxWeight = maxWeight;
            return copy;
        }

        public ILoginRequestCompletedData WithInventory(IEnumerable<IInventoryItem> inventoryItems)
        {
            var copy = MakeCopy(this);
            copy.CharacterItemInventory = inventoryItems.ToList();
            return copy;
        }

        public ILoginRequestCompletedData WithSpells(IEnumerable<IInventorySpell> inventorySpells)
        {
            var copy = MakeCopy(this);
            copy.CharacterSpellInventory = inventorySpells.ToList();
            return copy;
        }

        public ILoginRequestCompletedData WithCharacters(IEnumerable<ICharacter> characters)
        {
            var copy = MakeCopy(this);
            copy.MapCharacters = characters.ToList();
            return copy;
        }

        public ILoginRequestCompletedData WithNPCs(IEnumerable<INPC> npcs)
        {
            var copy = MakeCopy(this);
            copy.MapNPCs = npcs.ToList();
            return copy;
        }

        public ILoginRequestCompletedData WithItems(IEnumerable<IItem> items)
        {
            var copy = MakeCopy(this);
            copy.MapItems = items.ToList();
            return copy;
        }

        public ILoginRequestCompletedData WithError(CharacterLoginReply error)
        {
            var copy = MakeCopy(this);
            copy.Error = error;
            return copy;
        }

        private static LoginRequestCompletedData MakeCopy(ILoginRequestCompletedData source)
        {
            return new LoginRequestCompletedData
            {
                News = source.News,
                CharacterWeight = source.CharacterWeight,
                CharacterMaxWeight = source.CharacterMaxWeight,
                CharacterItemInventory = source.CharacterItemInventory,
                CharacterSpellInventory = source.CharacterSpellInventory,
                MapCharacters = source.MapCharacters,
                MapNPCs = source.MapNPCs,
                MapItems = source.MapItems,
                Error = source.Error,
            };
        }
    }
}