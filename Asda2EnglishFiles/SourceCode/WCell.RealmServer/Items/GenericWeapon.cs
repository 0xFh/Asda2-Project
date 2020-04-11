using WCell.Constants;
using WCell.Constants.Items;
using WCell.Constants.Skills;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Items
{
    /// <summary>A weapon that can be completely customized</summary>
    public class GenericWeapon : IAsda2Weapon
    {
        public static readonly DamageInfo[] FistDamage = new DamageInfo[1]
        {
            new DamageInfo(DamageSchoolMask.Physical, 1f, 2f)
        };

        public static readonly DamageInfo[] RangedDamage = new DamageInfo[1]
        {
            new DamageInfo(DamageSchoolMask.Physical, 1f, 2f)
        };

        public static readonly DamageInfo[] DefaultDamage = new DamageInfo[1]
        {
            new DamageInfo(DamageSchoolMask.Physical, 1f, 3f)
        };

        /// <summary>Default Fists</summary>
        public static GenericWeapon Fists = new GenericWeapon(InventorySlotTypeMask.WeaponMainHand,
            GenericWeapon.FistDamage, SkillId.Unarmed, 0.0f, Unit.DefaultMeleeAttackRange, 2000);

        /// <summary>Default Ranged Weapon</summary>
        public static GenericWeapon Ranged = new GenericWeapon(InventorySlotTypeMask.WeaponRanged,
            GenericWeapon.RangedDamage, SkillId.Bows, Unit.DefaultMeleeAttackRange, Unit.DefaultRangedAttackRange,
            2000);

        /// <summary>No damage weapon</summary>
        public static GenericWeapon Peace = new GenericWeapon(InventorySlotTypeMask.WeaponMainHand,
            GenericWeapon.FistDamage, SkillId.None, 0.0f, 0.0f, 10000);

        public GenericWeapon(InventorySlotTypeMask slot, int damageCount)
        {
            this.InventorySlotMask = slot;
            this.Damages = new DamageInfo[damageCount];
            this.IsWeapon = true;
        }

        public GenericWeapon(InventorySlotTypeMask slot, float minDmg, float maxDmg)
            : this(slot, minDmg, maxDmg, DamageSchoolMask.Physical)
        {
            this.IsWeapon = true;
        }

        public GenericWeapon(InventorySlotTypeMask slot, float minDmg, float maxDmg, DamageSchoolMask dmgType)
            : this(slot, minDmg, maxDmg, GenericWeapon.Fists.AttackTime, dmgType)
        {
            this.IsWeapon = true;
        }

        public GenericWeapon(InventorySlotTypeMask slot, float minDmg, float maxDmg, int attackTime)
            : this(slot, minDmg, maxDmg, attackTime, DamageSchoolMask.Physical)
        {
            this.IsWeapon = true;
        }

        public GenericWeapon(InventorySlotTypeMask slot, float minDmg, float maxDmg, int attackTime,
            DamageSchoolMask dmgType)
        {
            this.InventorySlotMask = slot;
            this.AttackTime = attackTime;
            this.MaxRange = Unit.DefaultMeleeAttackRange;
            this.Damages = new DamageInfo[1]
            {
                new DamageInfo(dmgType, minDmg, maxDmg)
            };
            this.IsWeapon = true;
        }

        public GenericWeapon(InventorySlotTypeMask slot, DamageInfo[] damages, SkillId skill, float minRange,
            float maxRange, int attackTime)
        {
            this.InventorySlotMask = slot;
            this.Damages = damages;
            this.Skill = skill;
            this.MinRange = minRange;
            this.MaxRange = maxRange;
            this.AttackTime = attackTime;
            this.IsWeapon = true;
        }

        public GenericWeapon(InventorySlotTypeMask slot, float minDmg, float maxDmg, int attackTime, float atackRange)
            : this(slot, minDmg, maxDmg, attackTime, DamageSchoolMask.Physical)
        {
            this.IsWeapon = true;
            this.MaxRange = atackRange;
            this.MinRange = 0.0f;
        }

        public DamageInfo[] Damages { get; set; }

        public bool IsWeapon { get; set; }

        public int BonusDamage
        {
            get { return 0; }
            set { }
        }

        public SkillId Skill { get; set; }

        public bool IsRanged
        {
            get { return this.InventorySlotMask == InventorySlotTypeMask.WeaponRanged; }
        }

        public bool IsMelee
        {
            get { return !this.IsRanged; }
        }

        /// <summary>The minimum Range of this weapon</summary>
        public float MinRange { get; set; }

        /// <summary>The maximum Range of this Weapon</summary>
        public float MaxRange { get; set; }

        /// <summary>The time in milliseconds between 2 attacks</summary>
        public int AttackTime { get; set; }

        public InventorySlotTypeMask InventorySlotMask { get; private set; }
    }
}