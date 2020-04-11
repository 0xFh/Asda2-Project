using Cell.Core;
using System;
using WCell.Constants.World;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Network;

namespace WCell.RealmServer.Global
{
    public class WorldStateCollection
    {
        public readonly IWorldSpace Area;
        public readonly WorldState[] States;
        public readonly byte[] CompiledState;

        public WorldStateCollection(IWorldSpace area, WorldState[] states)
        {
            this.Area = area;
            this.States = states;
            this.CompiledState = new byte[8 * this.States.Length];
            for (int index = 0; index < this.States.Length; ++index)
            {
                WorldState state = this.States[index];
                Array.Copy((Array) BitConverter.GetBytes((uint) state.Key), 0, (Array) this.CompiledState, index * 8,
                    4);
                Array.Copy((Array) BitConverter.GetBytes(state.DefaultValue), 0, (Array) this.CompiledState,
                    4 + index * 8, 4);
            }
        }

        public int FieldCount
        {
            get { return this.States.Length; }
        }

        public void SetInt32(WorldStateId id, int value)
        {
            this.SetInt32(WorldStates.GetState(id), value);
        }

        public void SetInt32(WorldState state, int value)
        {
            Array.Copy((Array) BitConverter.GetBytes(value), 0L, (Array) this.CompiledState,
                (long) (uint) (4 + (int) state.Index * 8), 4L);
            this.OnStateChanged(state, value);
        }

        public void SetUInt32(WorldStateId id, uint value)
        {
            this.SetUInt32(WorldStates.GetState(id), value);
        }

        public void SetUInt32(WorldState state, uint value)
        {
            Array.Copy((Array) BitConverter.GetBytes(value), 0L, (Array) this.CompiledState,
                (long) (uint) (4 + (int) state.Index * 8), 4L);
            this.OnStateChanged(state, (int) value);
        }

        public uint GetUInt32(WorldStateId id)
        {
            return this.GetUInt32(WorldStates.GetState(id).Index);
        }

        public uint GetUInt32(uint index)
        {
            return this.CompiledState.GetUInt32((uint) (1 + (int) index * 2));
        }

        public int GetInt32(WorldStateId id)
        {
            return this.GetInt32(WorldStates.GetState(id).Index);
        }

        public int GetInt32(uint index)
        {
            return this.CompiledState.GetInt32((uint) (1 + (int) index * 2));
        }

        internal void UpdateWorldState(uint index, int value)
        {
            Array.Copy((Array) BitConverter.GetBytes(value), 0L, (Array) this.CompiledState,
                (long) (uint) (4 + (int) index * 8), 4L);
        }

        private void OnStateChanged(WorldState state, int value)
        {
            this.Area.CallOnAllCharacters((Action<Character>) (chr =>
                WorldStateHandler.SendUpdateWorldState((IPacketReceiver) chr, state.Key, value)));
        }
    }
}