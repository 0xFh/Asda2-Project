using System;
using System.Collections.Generic;
using WCell.Constants.NPCs;
using WCell.Constants.Updates;
using WCell.RealmServer.AI.Actions;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;
using WCell.Util;

namespace WCell.RealmServer.AI.Brains
{
    /// <summary>
    /// TODO: Consider visibility of targets - Don't pursue target nor remove it from Threat list if not visible
    /// </summary>
    public class MobBrain : BaseBrain
    {
        private List<EmoteData>[] m_emoteData;

        public MobBrain(NPC owner)
            : base((Unit) owner)
        {
        }

        public MobBrain(NPC owner, BrainState defaultState)
            : base((Unit) owner, defaultState)
        {
        }

        public MobBrain(NPC owner, IAIActionCollection actions)
            : this(owner, actions, BrainState.Idle)
        {
        }

        public MobBrain(NPC owner, IAIActionCollection actions, BrainState defaultState)
            : base((Unit) owner, actions, defaultState)
        {
        }

        /// <summary>Makes npc Yell text.</summary>
        /// <param name="pText">Wanted text.</param>
        public void DoEmote(string pText)
        {
            this.NPC.Yell(pText);
        }

        /// <summary>Makes npc emote text.</summary>
        /// <param name="pText">Wanted Text.</param>
        /// <param name="pType">Type of emote, Yell, Say, etc.</param>
        public void DoEmote(string pText, NPCEmoteType pType)
        {
            switch (pType)
            {
                case NPCEmoteType.NPC_Yell:
                    this.NPC.Yell(pText);
                    break;
                case NPCEmoteType.NPC_Say:
                    this.NPC.Say(pText);
                    break;
                case NPCEmoteType.NPC_Emote:
                    this.NPC.Emote(pText);
                    break;
            }
        }

        /// <summary>Makes npc emote text. And play wanted sound.</summary>
        /// <param name="pText">Wanted Text,</param>
        /// <param name="pType">Type of emote, Yell, Say, etc.</param>
        /// <param name="pSoundId">Id of sound(Found in DBC).</param>
        public void DoEmote(string pText, NPCEmoteType pType, uint pSoundId)
        {
            if (pSoundId != 0U)
                this.NPC.PlaySound(pSoundId);
            this.DoEmote(pText, pType);
        }

        private void DoEmote(EmoteData emoteData)
        {
            this.DoEmote(emoteData.mText, emoteData.mType, emoteData.mSoundId);
        }

        private void DoEmoteForEvent(NPCBrainEvents pNPCBrainEvents)
        {
            uint num = (uint) pNPCBrainEvents;
            if (this.m_emoteData == null)
                return;
            int count = this.m_emoteData[num].Count;
            if (count < 1)
                return;
            this.DoEmote(this.m_emoteData[num][count == 1 ? 0 : Utility.Random(0, count - 1)]);
        }

        public void AddEmote(string pText, NPCBrainEvents pEvent, NPCEmoteType pType, uint pSoundId)
        {
            this.AddEmote(new EmoteData(pText, pEvent, pType, pSoundId));
        }

        public void AddEmote(string pText, NPCBrainEvents pEvent, NPCEmoteType pType)
        {
            this.AddEmote(new EmoteData(pText, pEvent, pType, 0U));
        }

        public void AddEmote(EmoteData pEmoteData)
        {
            if (this.m_emoteData == null)
                this.m_emoteData = new List<EmoteData>[6];
            uint mEvent = (uint) pEmoteData.mEvent;
            if (this.m_emoteData[mEvent] == null)
                this.m_emoteData[mEvent] = new List<EmoteData>();
            this.m_emoteData[mEvent].Add(pEmoteData);
        }

        public override void OnHeal(Unit healer, Unit healed, int amtHealed)
        {
            if (!(this.m_owner is NPC) || !this.m_owner.IsInCombat || !this.m_owner.CanBeAggroedBy(healer))
                return;
            ((NPC) this.m_owner).ThreatCollection[healer] += amtHealed / 2;
        }

        public override void OnEnterCombat()
        {
            this.DoEmoteForEvent(NPCBrainEvents.OnEnterCombat);
        }

        public override void OnLeaveCombat()
        {
            this.DoEmoteForEvent(NPCBrainEvents.OnLeaveCombat);
            NPC owner = this.m_owner as NPC;
            if (owner == null)
                return;
            owner.ThreatCollection.Clear();
        }

        public override void OnKilled(Unit killerUnit, Unit victimUnit)
        {
            if (victimUnit == this.Owner || killerUnit != this.Owner)
                return;
            this.DoEmoteForEvent(NPCBrainEvents.OnTargetDied);
        }

        public override void OnDeath()
        {
            this.DoEmoteForEvent(NPCBrainEvents.OnDeath);
            base.OnDeath();
        }

        /// <summary>
        /// Called when owner received a debuff by the given caster
        /// </summary>
        /// <param name="caster"></param>
        /// <param name="cast"></param>
        /// <param name="debuff"></param>
        public override void OnDebuff(Unit caster, SpellCast cast, Aura debuff)
        {
            if (!this.m_IsRunning || caster == null || (!(this.m_owner is NPC) || !this.m_owner.CanBeAggroedBy(caster)))
                return;
            ++((NPC) this.m_owner).ThreatCollection[caster];
        }

        /// <summary>
        /// Called whenever someone performs a harmful action on this Mob.
        /// </summary>
        /// <param name="action"></param>
        public override void OnDamageReceived(IDamageAction action)
        {
            if (!this.m_IsRunning || action.Attacker == null)
                return;
            NPC owner = this.m_owner as NPC;
            if (owner == null)
                return;
            owner.ThreatCollection[action.Attacker] += action.Attacker.GetGeneratedThreat(action);
            if (owner.Entry.Rank < CreatureRank.Elite || !this.IsFirstDamageReceived)
                return;
            this.IsFirstDamageReceived = false;
            foreach (Unit objectsInRadiu in (IEnumerable<WorldObject>) owner.GetObjectsInRadius<NPC>(
                CharacterFormulas.EliteMobSocialAggrRange, ObjectTypes.Unit, false, int.MaxValue))
            {
                NPC npc = objectsInRadiu as NPC;
                if (npc != null && npc.Entry.Rank >= CreatureRank.Elite)
                    npc.ThreatCollection[action.Attacker] += action.Attacker.GetGeneratedThreat(action);
            }
        }

        public override void OnCombatTargetOutOfRange()
        {
        }
    }
}