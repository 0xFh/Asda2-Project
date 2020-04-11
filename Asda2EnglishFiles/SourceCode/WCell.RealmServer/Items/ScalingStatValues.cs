namespace WCell.RealmServer.Items
{
    public class ScalingStatValues
    {
        public uint[] SsdMultiplier = new uint[6];
        public uint[] ArmorMod = new uint[8];
        public uint[] DpsMod = new uint[6];
        public uint Id;
        public uint Level;
        public uint SpellBonus;

        public uint GetSsdMultiplier(uint mask)
        {
            if (((int) mask & 262175) != 0)
            {
                if (((int) mask & 1) != 0)
                    return this.SsdMultiplier[0];
                if (((int) mask & 2) != 0)
                    return this.SsdMultiplier[1];
                if (((int) mask & 4) != 0)
                    return this.SsdMultiplier[2];
                if (((int) mask & 8) != 0)
                    return this.SsdMultiplier[4];
                if (((int) mask & 16) != 0)
                    return this.SsdMultiplier[3];
                if (((int) mask & 262144) != 0)
                    return this.SsdMultiplier[5];
            }

            return 0;
        }

        public uint GetArmorMod(uint mask)
        {
            if (((int) mask & 15729120) != 0)
            {
                if (((int) mask & 32) != 0)
                    return this.ArmorMod[0];
                if (((int) mask & 64) != 0)
                    return this.ArmorMod[1];
                if (((int) mask & 128) != 0)
                    return this.ArmorMod[2];
                if (((int) mask & 256) != 0)
                    return this.ArmorMod[3];
                if (((int) mask & 1048576) != 0)
                    return this.ArmorMod[4];
                if (((int) mask & 2097152) != 0)
                    return this.ArmorMod[5];
                if (((int) mask & 4194304) != 0)
                    return this.ArmorMod[6];
                if (((int) mask & 8388608) != 0)
                    return this.ArmorMod[7];
            }

            return 0;
        }

        public uint GetDpsMod(uint mask)
        {
            if (((int) mask & 32256) != 0)
            {
                if (((int) mask & 512) != 0)
                    return this.DpsMod[0];
                if (((int) mask & 1024) != 0)
                    return this.DpsMod[1];
                if (((int) mask & 2048) != 0)
                    return this.DpsMod[2];
                if (((int) mask & 4096) != 0)
                    return this.DpsMod[3];
                if (((int) mask & 8192) != 0)
                    return this.DpsMod[4];
                if (((int) mask & 16384) != 0)
                    return this.DpsMod[5];
            }

            return 0;
        }

        public uint GetSpellBonus(uint mask)
        {
            if (((int) mask & 32768) != 0)
                return this.SpellBonus;
            return 0;
        }
    }
}