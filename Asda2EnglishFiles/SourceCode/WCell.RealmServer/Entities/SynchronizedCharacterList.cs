using System;
using System.Collections.Generic;
using WCell.Constants.Factions;
using WCell.Core.Network;
using WCell.RealmServer.Network;
using WCell.Util.Collections;

namespace WCell.RealmServer.Entities
{
    public class SynchronizedCharacterList : SynchronizedList<Character>, ICharacterSet, IPacketReceiver
    {
        public SynchronizedCharacterList(FactionGroup group, ICollection<Character> chrs)
            : base((IEnumerable<Character>) chrs)
        {
            this.FactionGroup = group;
        }

        public SynchronizedCharacterList(FactionGroup group)
            : base(5)
        {
            this.FactionGroup = group;
        }

        public SynchronizedCharacterList(int capacity, FactionGroup group)
            : base(capacity)
        {
            this.FactionGroup = group;
        }

        public FactionGroup FactionGroup { get; protected set; }

        public int CharacterCount
        {
            get { return this.Count; }
        }

        /// <summary>Threadsafe iteration</summary>
        /// <param name="callback"></param>
        public void ForeachCharacter(Action<Character> callback)
        {
            this.EnterLock();
            try
            {
                for (int index = this.Count - 1; index >= 0; --index)
                {
                    Character character = this[index];
                    callback(character);
                    if (!character.IsInWorld)
                        this.RemoveUnlocked(index);
                }
            }
            finally
            {
                this.ExitLock();
            }
        }

        /// <summary>Creates a Copy of the set</summary>
        public Character[] GetAllCharacters()
        {
            return this.ToArray();
        }

        public bool IsRussianClient { get; set; }

        public Locale Locale { get; set; }

        public void Send(RealmPacketOut packet, bool addEnd = false)
        {
            byte[] finalizedPacket = packet.GetFinalizedPacket();
            this.ForeachCharacter((Action<Character>) (chr => chr.Send(finalizedPacket)));
        }
    }
}