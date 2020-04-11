using System.Collections.Generic;

namespace Im.DAL
{
    public class CharacterRec
    {
        public string Name { get; set; }
        public byte Level { get; set; }
        public int GuildPoints { get; set; }
        public List<SkillRec> Skills { get; set; }
        public List<HotKey> HotKeys { get; set; }
    }

    public class HotKey
    {

    }

    public class SkillRec
    {
        public int Id { get; set; }
        public byte Level { get; set; }
        public int Cooldown { get; set; }
    }


}
