using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.RealmServer.Asda2GuildWave
{
    struct Asda2GuildWaveNpc
    {
        private int _npcId;
        private int _npcCount;

        public int NpcId
        {
            get { return _npcId; }
        }

        public int NpcCount
        {
            get { return _npcCount; }
        }

        public Asda2GuildWaveNpc(int npcId, int npcCount)
        {
            this._npcId = npcId;
            this._npcCount = npcCount;
        }
    }
}