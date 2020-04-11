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
      Area = area;
      States = states;
      CompiledState = new byte[8 * States.Length];
      for(int index = 0; index < States.Length; ++index)
      {
        WorldState state = States[index];
        Array.Copy(BitConverter.GetBytes((uint) state.Key), 0, CompiledState, index * 8,
          4);
        Array.Copy(BitConverter.GetBytes(state.DefaultValue), 0, CompiledState,
          4 + index * 8, 4);
      }
    }

    public int FieldCount
    {
      get { return States.Length; }
    }

    public void SetInt32(WorldStateId id, int value)
    {
      SetInt32(WorldStates.GetState(id), value);
    }

    public void SetInt32(WorldState state, int value)
    {
      Array.Copy(BitConverter.GetBytes(value), 0L, CompiledState,
        (uint) (4 + (int) state.Index * 8), 4L);
      OnStateChanged(state, value);
    }

    public void SetUInt32(WorldStateId id, uint value)
    {
      SetUInt32(WorldStates.GetState(id), value);
    }

    public void SetUInt32(WorldState state, uint value)
    {
      Array.Copy(BitConverter.GetBytes(value), 0L, CompiledState,
        (uint) (4 + (int) state.Index * 8), 4L);
      OnStateChanged(state, (int) value);
    }

    public uint GetUInt32(WorldStateId id)
    {
      return GetUInt32(WorldStates.GetState(id).Index);
    }

    public uint GetUInt32(uint index)
    {
      return CompiledState.GetUInt32((uint) (1 + (int) index * 2));
    }

    public int GetInt32(WorldStateId id)
    {
      return GetInt32(WorldStates.GetState(id).Index);
    }

    public int GetInt32(uint index)
    {
      return CompiledState.GetInt32((uint) (1 + (int) index * 2));
    }

    internal void UpdateWorldState(uint index, int value)
    {
      Array.Copy(BitConverter.GetBytes(value), 0L, CompiledState,
        (uint) (4 + (int) index * 8), 4L);
    }

    private void OnStateChanged(WorldState state, int value)
    {
      Area.CallOnAllCharacters(chr =>
        WorldStateHandler.SendUpdateWorldState(chr, state.Key, value));
    }
  }
}