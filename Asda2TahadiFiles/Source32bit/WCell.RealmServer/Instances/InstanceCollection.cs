﻿using NLog;
using System;
using System.Collections.Generic;
using WCell.Constants.World;
using WCell.RealmServer.Chat;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.Util.ObjectPools;

namespace WCell.RealmServer.Instances
{
  /// <summary>Manages all Instance-relations of a Character.</summary>
  public class InstanceCollection
  {
    public static readonly ObjectPool<List<InstanceBinding>> InstanceBindingListPool =
      new ObjectPool<List<InstanceBinding>>(() => new List<InstanceBinding>(4));

    private static readonly Logger log = LogManager.GetCurrentClassLogger();

    /// <summary>Our character id</summary>
    private uint m_characterId;

    /// <summary>Our character</summary>
    private Character m_character;

    /// <summary>Bindings of normal, resettable instances</summary>
    private readonly List<InstanceBinding>[] m_bindings;

    public InstanceCollection(Character player)
      : this(player.EntityId.Low)
    {
      m_bindings = new List<InstanceBinding>[2];
      m_character = player;
    }

    public InstanceCollection(uint lowId)
    {
      m_characterId = lowId;
    }

    public bool HasFreeInstanceSlot
    {
      get
      {
        List<InstanceBinding> binding = m_bindings[0];
        if(binding == null || binding.Count < InstanceMgr.MaxInstancesPerHour)
          return true;
        RemoveExpiredSoftBindings();
        return binding.Count < InstanceMgr.MaxInstancesPerHour;
      }
    }

    public Character Character
    {
      get { return m_character; }
      internal set
      {
        m_character = value;
        if(m_character == null)
          return;
        m_characterId = m_character.EntityId.Low;
      }
    }

    /// <summary>EntityId.Low of the Owner of this log</summary>
    public uint CharacterId
    {
      get { return m_characterId; }
    }

    public void ClearBindings()
    {
      foreach(List<InstanceBinding> binding in m_bindings)
      {
        if(binding != null)
        {
          lock(binding)
            binding.Clear();
        }
      }
    }

    /// <summary>Binds the instance</summary>
    public void BindTo(BaseInstance instance)
    {
      List<InstanceBinding> bindingList = GetOrCreateBindingList(instance.Difficulty.BindingType);
      lock(bindingList)
      {
        if(bindingList.Count >= InstanceMgr.MaxInstancesPerHour)
          log.Error(
            "{0} was saved to \"{1}\" but exceeded the MaxInstancesPerCharPerHour limit.",
            m_character, instance);
        bindingList.Add(new InstanceBinding(instance.InstanceId, instance.Id, instance.Difficulty.Index));
      }
    }

    /// <summary>
    /// Returns the Cooldown object for the Instance with the given MapId.
    /// </summary>
    /// <param name="map">The MapId of the Instance in question.</param>
    /// <returns></returns>
    public InstanceBinding GetBinding(MapId map, BindingType type)
    {
      List<InstanceBinding> binding = m_bindings[(int) type];
      if(binding == null)
        return null;
      lock(binding)
      {
        foreach(InstanceBinding instanceBinding in binding)
        {
          if(instanceBinding.MapId == map)
            return instanceBinding;
        }
      }

      return null;
    }

    /// <summary>
    /// Checks the list of stored Raid and Heroic instances and the list of recently run Normal
    /// instances for a reference to the given map.
    /// </summary>
    /// <param name="template">The MapInfo of the Instance in question.</param>
    /// <returns>The Instance if found, else null.</returns>
    public BaseInstance GetActiveInstance(MapTemplate template)
    {
      Character character = Character;
      if(character == null)
        return null;
      InstanceBinding binding = GetBinding(template.Id,
        template.GetDifficulty(character.GetInstanceDifficulty(template.IsRaid)).BindingType);
      if(binding != null)
      {
        BaseInstance instance = InstanceMgr.Instances.GetInstance(binding.MapId, binding.InstanceId);
        if(instance != null && instance.IsActive)
          return instance;
      }

      return null;
    }

    /// <summary>
    /// Tries to reset all owned Instances.
    /// Requires to be in Character's context if online.
    /// </summary>
    public bool TryResetInstances()
    {
      Character character = Character;
      return true;
    }

    /// <summary>
    /// Sends the list of Raids completed and In progress and when they will reset.
    /// </summary>
    public void SendRaidTimes()
    {
      if(Character == null)
        return;
      SendRaidTimes(Character);
    }

    /// <summary>
    /// Sends the list of Raids completed and In progress and when they will reset.
    /// </summary>
    public void SendRaidTimes(IChatTarget listener)
    {
      List<InstanceBinding> binding = m_bindings[1];
      if(binding == null)
        return;
      foreach(InstanceBinding instanceBinding in binding)
        listener.SendMessage("Raid {0} #{1}, Until: {1}", (object) instanceBinding.MapId,
          (object) instanceBinding.InstanceId, (object) instanceBinding.NextResetTime);
    }

    /// <summary>
    /// Warning: Requires Character to be logged in and to be in Character's context.
    /// Often you might want to use ForeachBinding() instead.
    /// </summary>
    public void ForeachBinding(BindingType type, Action<InstanceBinding> callback)
    {
      List<InstanceBinding> binding = m_bindings[(int) type];
      if(binding == null)
        return;
      lock(binding)
      {
        foreach(InstanceBinding instanceBinding in binding)
          callback(instanceBinding);
      }
    }

    /// <summary>
    /// Updates the List of stored InstanceCooldowns, removing the expired ones.
    /// </summary>
    private void RemoveExpiredSoftBindings()
    {
      List<InstanceBinding> binding = m_bindings[0];
      if(binding == null)
        return;
      lock(binding)
      {
        for(int index = binding.Count - 1; index >= 0; --index)
        {
          InstanceBinding record = binding[index];
          if(record.BindTime.AddMinutes(InstanceMgr.DungeonExpiryMinutes) > DateTime.Now)
          {
            binding.RemoveAt(index);
            record.DeleteLater();
          }
        }
      }
    }

    private List<InstanceBinding> GetOrCreateBindingList(BindingType type)
    {
      lock(m_bindings)
      {
        List<InstanceBinding> binding = m_bindings[(int) type];
        if(binding == null)
          m_bindings[(int) type] = binding = InstanceBindingListPool.Obtain();
        return binding;
      }
    }

    internal void Dispose()
    {
      foreach(List<InstanceBinding> binding in m_bindings)
      {
        if(binding != null)
          InstanceBindingListPool.Recycle(binding);
      }
    }
  }
}