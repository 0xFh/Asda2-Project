using System;
using WCell.Constants.Looting;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Looting;

namespace WCell.RealmServer.Spells.Effects
{
  public abstract class ItemConvertEffectHandler : SpellEffectHandler
  {
    public ItemConvertEffectHandler(SpellCast cast, SpellEffect effect)
      : base(cast, effect)
    {
    }

    public override SpellFailedReason Initialize()
    {
      return m_cast.TargetItem.Amount < Effect.MinValue
        ? SpellFailedReason.NeedMoreItems
        : SpellFailedReason.Ok;
    }

    public override void Apply()
    {
      Character casterChar = m_cast.CasterChar;
      Item item = m_cast.TargetItem;
      ObjectLoot andSendObjectLoot =
        LootMgr.CreateAndSendObjectLoot(item, casterChar, LootEntryType, false);
      if(andSendObjectLoot == null)
        return;
      andSendObjectLoot.OnLootFinish = () =>
      {
        if(!item.IsInWorld)
          return;
        item.Amount -= Effect.MinValue;
      };
    }

    public abstract LootEntryType LootEntryType { get; }

    public override bool HasOwnTargets
    {
      get { return false; }
    }
  }
}