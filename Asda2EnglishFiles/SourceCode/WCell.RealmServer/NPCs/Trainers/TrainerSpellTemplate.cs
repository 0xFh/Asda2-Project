using System.Collections.Generic;
using WCell.Constants.Skills;
using WCell.Constants.Spells;
using WCell.RealmServer.Content;
using WCell.RealmServer.Skills;
using WCell.RealmServer.Spells;
using WCell.Util.Data;

namespace WCell.RealmServer.NPCs.Trainers
{
    public class TrainerSpellTemplate : TrainerSpellEntry, IDataHolder
    {
        public uint TrainerTemplateId;

        public new void FinalizeDataHolder()
        {
            if ((this.Spell = SpellHandler.Get(this.SpellId)) == null)
                ContentMgr.OnInvalidDBData("SpellId is invalid in " + (object) this);
            else if (this.RequiredSpellId != SpellId.None && SpellHandler.Get(this.RequiredSpellId) == null)
                ContentMgr.OnInvalidDBData("RequiredSpellId is invalid in " + (object) this);
            else if (this.RequiredSkillId != SkillId.None && SkillHandler.Get(this.RequiredSkillId) == null)
            {
                ContentMgr.OnInvalidDBData("RequiredSkillId is invalid in " + (object) this);
            }
            else
            {
                if (this.RequiredLevel == 0)
                    this.RequiredLevel = this.Spell.Level;
                if (!NPCMgr.TrainerSpellTemplates.ContainsKey(this.TrainerTemplateId))
                    NPCMgr.TrainerSpellTemplates.Add(this.TrainerTemplateId, new List<TrainerSpellEntry>());
                NPCMgr.TrainerSpellTemplates[this.TrainerTemplateId].Add((TrainerSpellEntry) this);
            }
        }
    }
}