using System;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Targeting
{
    [Serializable]
    public class TargetDefinition
    {
        public readonly TargetAdder Adder;
        public TargetFilter Filter;

        public TargetDefinition(TargetAdder adder, params TargetFilter[] filters)
        {
            this.Adder = adder;
            if (filters == null)
                return;
            foreach (TargetFilter filter in filters)
                this.AddFilter(filter);
        }

        internal void Collect(SpellTargetCollection targets, ref SpellFailedReason failReason)
        {
            if (this.Adder == null)
                return;
            this.Adder(targets, this.Filter, ref failReason);
        }

        /// <summary>Composites the given filter into the existing filter</summary>
        public void AddFilter(TargetFilter filter)
        {
            if (this.Filter == null)
            {
                this.Filter = filter;
            }
            else
            {
                TargetFilter oldFilter = this.Filter;
                this.Filter = (TargetFilter) ((SpellEffectHandler effectHandler, WorldObject target,
                    ref SpellFailedReason failReason) =>
                {
                    oldFilter(effectHandler, target, ref failReason);
                    if (failReason != SpellFailedReason.Ok)
                        return;
                    filter(effectHandler, target, ref failReason);
                });
            }
        }

        public override bool Equals(object obj)
        {
            if (!(obj is TargetDefinition))
                return false;
            TargetDefinition targetDefinition = (TargetDefinition) obj;
            if (targetDefinition.Adder == this.Adder)
                return targetDefinition.Filter == this.Filter;
            return false;
        }

        public override int GetHashCode()
        {
            return this.Adder.GetHashCode() * this.Filter.GetHashCode();
        }
    }
}