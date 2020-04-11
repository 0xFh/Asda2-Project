using System;
using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.Items;
using WCell.RealmServer.Asda2BattleGround;
using WCell.RealmServer.Formulas;
using WCell.RealmServer.Guilds;
using WCell.Util;
using WCell.Util.Variables;

namespace WCell.RealmServer
{
    public static class CharacterFormulas
    {
        [NotVariable] public static float AuctionPushComission = 0.02f;
        [NotVariable] public static float AuctionSellComission = 0.1f;

        /// <summary>
        /// Сколько нужно очков на поднятие следующего уровня клана на первый 0, на 2й 30000 и т.д.
        /// </summary>
        [NotVariable] public static int[] GuildLevelUpCost = new int[10]
        {
            0,
            30000,
            70000,
            120000,
            350000,
            500000,
            800000,
            1200000,
            1750000,
            2500000
        };

        [NotVariable] public static float MaxToTotalMultiplier = 0.7f;
        [NotVariable] public static int OptionStatStartsWithEnchantValue = 7;
        [NotVariable] public static float ItemsDeffenceMultiplier = 2.5f;
        [NotVariable] public static float ItemsMagicDeffenceMultiplier = 4.5f;
        [NotVariable] public static float PetDeffenceMultiplier = 2f;
        [NotVariable] public static float PetMagicDeffenceMultiplier = 2f;
        [NotVariable] public static float KillPkExpPercentOfLoose = 0.5f;
        public static float GuildPointsMultiplier = 3f;

        [NotVariable]
        public static int FishingGuildPoints = (int) (5.0 * (double) CharacterFormulas.GuildPointsMultiplier);

        [NotVariable]
        public static int DiggingGuildPoints = (int) (2.0 * (double) CharacterFormulas.GuildPointsMultiplier);

        [NotVariable]
        public static int MobKillingGuildPoints = (int) (1.0 * (double) CharacterFormulas.GuildPointsMultiplier);

        [NotVariable] public static int CharacterKillingGuildPoints =
            (int) (10.0 * (double) CharacterFormulas.GuildPointsMultiplier);

        [NotVariable] public static int LevelupingGuildPointsPerLevel =
            (int) (2.0 * (double) CharacterFormulas.GuildPointsMultiplier);

        [NotVariable] public static int CraftingGuildPointsPerLevel =
            (int) (5.0 * (double) CharacterFormulas.GuildPointsMultiplier);

        [NotVariable] public static int BossKillingGuildPointsPerLevel =
            (int) (2.0 * (double) CharacterFormulas.GuildPointsMultiplier);

        [NotVariable] public static float MagicAtackSowelMultiplier = 1.15f;
        [NotVariable] public static float PhysicalAtackSowelMultiplier = 1f;
        [NotVariable] public static float EliteMobSocialAggrRange = 10f;
        [NotVariable] public static float NpcMoveUpdateDelay = 1500f;
        [NotVariable] public static double NpcSpellUpdateDelay = 600.0;
        [NotVariable] public static int DeffenceRow = 3500;
        [NotVariable] public static int MaxLootCount = 50;
        [NotVariable] public static float HpPotionsBoostPerStamina = 0.01f;
        [NotVariable] public static int MaxLvlMobCharDiff = 80;
        [NotVariable] public static int MaxDamagersDetailCount = 5;
        [NotVariable] public static int EventItemsForGuessEvent = 10;
        [NotVariable] public static int EventItemId = 36890;
        [NotVariable] public static int DonationItemId = 33800;
        [NotVariable] public static int RebornLevel = 60;
        [NotVariable] public static int DefenceTownLives = 100;
        [NotVariable] public static int ForeignLootPickupTimeout = 30;
        [NotVariable] public static int NpcUpdatesToScanAndAttack = 50;

        [NotVariable] public static List<CharacterFormulas.ItemIdAmounted> ItemIdsToAddOnReborn =
            new List<CharacterFormulas.ItemIdAmounted>()
            {
                new CharacterFormulas.ItemIdAmounted(21498, 1),
                new CharacterFormulas.ItemIdAmounted(21499, 1)
            };

        [NotVariable] public static float MaxDeffenceDownEventDifficulty = 30f;
        [NotVariable] public static int SaveChateterInterval = 600000;
        [NotVariable] public static int TimeBetweenImNotMovingPacketSendMillis = 1000;
        [NotVariable] public static int DropLiveMinutes = 3;
        [NotVariable] public static short FactionWarPointsPerTicForCapturedPoints = 5;
        [NotVariable] public static int DefaultCaptureTime = 60000;
        [NotVariable] public static int DefaultTimeToStartCapture = 10000;
        [NotVariable] public static int DefaultTimeGainExpReward = 10000;
        [NotVariable] public static float NearFriendDamageBonus = 0.15f;
        [NotVariable] public static float NearFriendDeffenceBonus = 0.1f;
        [NotVariable] public static float NearFriendSpeedBonus = 0.05f;
        [NotVariable] public static float FriendEmpowerDamageBonus = 0.25f;
        [NotVariable] public static float SoulmateSongStatBonusPrc = 0.5f;
        [NotVariable] public static float SoulmateSongDamageBonusPrc = 0.3f;
        [NotVariable] public static float SoulmateSongSpeedBonusPrc = 0.15f;
        [NotVariable] public static float SoulmateSongDeffenceBonusPrc = 0.3f;
        [NotVariable] public static int FreestatPointsOnStart = 15;
        [NotVariable] public static float SoulmatExpFromMonstrKilled = 0.3f;
        [NotVariable] public static float SoulmatExpFromAnyExp = 0.1f;
        [NotVariable] public static float SoulmateExpGainPerMinuteNearFriend = 0.01f;
        [NotVariable] public static float BattegroundGroupDisctributePrc = 0.4f;
        [NotVariable] public static int WaveCoinsDivider = 3;
        [NotVariable] public static float HonorCoinsDivider = 7f;
        [NotVariable] public static uint TimeBetweenPetExpGainSecs = 150;
        [NotVariable] public static uint TimeBetweenPetEatingsSecs = 80;
        [NotVariable] public static int ExpirienceLooseOnDeathPrc = 10;
        [NotVariable] public static int StatOnCreation = 1;
        [NotVariable] public static float OnePrcAtackTimeReducePerAgilityPoints = 0.01f;
        [NotVariable] public static float OnePrcCritPerLuck = 3f / 1000f;
        [NotVariable] public static float OnePrcDropChancePerLuck = 1.5E-05f;
        [NotVariable] public static float OnePrcGoldAmountPerLuckPoints = 1.5E-05f;
        [NotVariable] public static float OneMagicDefencePerSpiritPoints = 1f;
        [NotVariable] public static float ManaPointsPerOneSpirit = 2.54f;
        [NotVariable] public static float DamagePerIntelect = 0.28f;
        [NotVariable] public static float DamagePerAgility = 0.17f;
        [NotVariable] public static float DamagePerStrength = 0.15f;
        [NotVariable] public static float HealthPointsPerStrength = 0.6f;
        [NotVariable] public static float HealthPointsPerStamina = 14.5f;
        [NotVariable] public static float DefencePointsPerAgility = 10f;
        [NotVariable] public static float DodgePerAgility = 0.005f;
        [NotVariable] public static float SpeedPerAgility = 0.0001f;
        [NotVariable] public static float CritDamageBonusPerStrength = 1f / 1000f;
        [NotVariable] public static float CritDamageBonusPerIntellect = 1f / 1000f;
        [NotVariable] public static float EnchantPowValue = 0.3f;
        [NotVariable] public static float EnchantPowValueForNotDamageStats = 0.065f;
        [NotVariable] public static byte StandartFishingChance = 36;
        [NotVariable] public static int StandartFishingLevelUpChance = 60000;

        /// <summary>
        /// Количество очков энергии для увелицчения регенирации маны на 1 в секунду
        /// </summary>
        [NotVariable] public static float ManaRegenPerSpirit = 0.01f;

        [NotVariable] public static int DecreaceRodDurabilityChance = 40000;
        [NotVariable] public static int AlpiaBaseHonorPoints = 5;
        [NotVariable] public static int SilarisBaseHonorPoints = 15;
        [NotVariable] public static int FlamioBaseHonorPoints = 30;
        [NotVariable] public static int AquatonBaseHonorPoints = 60;
        public static int[] HonorRankPoints = new int[21];
        [NotVariable] public static short BaseActPointsOnKill = 20;
        [NotVariable] public static int PKItemDropChance = 10000;
        [NotVariable] public static int ItemDropChance = 1000;

        public static void InitGuildSkills()
        {
            GuildSkillTemplate.Templates[0] = new GuildSkillTemplate()
            {
                BonusValuses = new int[8] {0, 3, 4, 5, 6, 7, 8, 10},
                LearnCosts = new int[8]
                {
                    0,
                    60000,
                    100000,
                    400000,
                    700000,
                    1000000,
                    2000000,
                    3000000
                },
                ActivationCosts = new int[8]
                {
                    0,
                    6000,
                    10000,
                    40000,
                    70000,
                    100000,
                    200000,
                    300000
                },
                MaitenceCosts = new int[8]
                {
                    0,
                    1200,
                    2000,
                    8000,
                    14000,
                    20000,
                    40000,
                    60000
                },
                MaxLevel = 7
            };
            GuildSkillTemplate.Templates[1] = new GuildSkillTemplate()
            {
                BonusValuses = new int[8] {0, 3, 4, 5, 6, 7, 8, 10},
                LearnCosts = new int[8]
                {
                    0,
                    25000,
                    60000,
                    100000,
                    300000,
                    450000,
                    1200000,
                    2000000
                },
                ActivationCosts = new int[8]
                {
                    0,
                    2500,
                    6000,
                    10000,
                    30000,
                    45000,
                    120000,
                    200000
                },
                MaitenceCosts = new int[8]
                {
                    0,
                    500,
                    1200,
                    2000,
                    6000,
                    9000,
                    24000,
                    40000
                },
                MaxLevel = 7
            };
            GuildSkillTemplate.Templates[2] = new GuildSkillTemplate()
            {
                BonusValuses = new int[8] {0, 3, 4, 5, 6, 7, 8, 10},
                LearnCosts = new int[8]
                {
                    0,
                    100000,
                    300000,
                    400000,
                    700000,
                    1000000,
                    1500000,
                    2500000
                },
                ActivationCosts = new int[8]
                {
                    0,
                    10000,
                    30000,
                    40000,
                    70000,
                    100000,
                    150000,
                    250000
                },
                MaitenceCosts = new int[8]
                {
                    0,
                    2000,
                    6000,
                    8000,
                    14000,
                    20000,
                    30000,
                    50000
                },
                MaxLevel = 7
            };
            GuildSkillTemplate.Templates[3] = new GuildSkillTemplate()
            {
                BonusValuses = new int[8] {0, 1, 2, 3, 4, 5, 6, 7},
                LearnCosts = new int[8]
                {
                    0,
                    550000,
                    1000000,
                    1350000,
                    1800000,
                    2400000,
                    3000000,
                    5000000
                },
                ActivationCosts = new int[8]
                {
                    0,
                    55000,
                    100000,
                    135000,
                    180000,
                    240000,
                    300000,
                    500000
                },
                MaitenceCosts = new int[8]
                {
                    0,
                    11000,
                    20000,
                    27000,
                    36000,
                    48000,
                    60000,
                    100000
                },
                MaxLevel = 7
            };
            GuildSkillTemplate.Templates[4] = new GuildSkillTemplate()
            {
                BonusValuses = new int[5] {0, 5, 6, 7, 10},
                LearnCosts = new int[5]
                {
                    0,
                    500000,
                    1000000,
                    2000000,
                    3000000
                },
                ActivationCosts = new int[5]
                {
                    0,
                    50000,
                    100000,
                    200000,
                    300000
                },
                MaitenceCosts = new int[5]
                {
                    0,
                    10000,
                    20000,
                    40000,
                    60000
                },
                MaxLevel = 4
            };
        }

        /// <summary>Расчет базового кол-ва жизни у персонажа</summary>
        /// <param name="level">Уровень</param>
        /// <param name="cl">Класс</param>
        /// <returns>базовое кол-во жизни</returns>
        public static int GetBaseHealth(int level, ClassId cl)
        {
            switch (cl)
            {
                case ClassId.NoClass:
                    return 100 + level * 13;
                case ClassId.OHS:
                    return 250 + level * 17;
                case ClassId.Spear:
                    return 180 + level * 15;
                case ClassId.THS:
                    return 200 + level * 16;
                case ClassId.Crossbow:
                    return 150 + level * 14;
                case ClassId.Bow:
                    return 150 + level * 14;
                case ClassId.Balista:
                    return 150 + level * 15;
                case ClassId.AtackMage:
                    return 90 + level * 13;
                case ClassId.SupportMage:
                    return 100 + level * 13;
                case ClassId.HealMage:
                    return 110 + level * 13;
                default:
                    return 100;
            }
        }

        /// <summary>Расчет бонуса жизни</summary>
        /// <param name="level"></param>
        /// <param name="asda2Strength"></param>
        /// <param name="asda2Stamina"></param>
        /// <param name="cl"></param>
        /// <returns></returns>
        public static float CalculateHealthBonus(int level, int asda2Strength, int asda2Stamina, ClassId cl)
        {
            switch (cl)
            {
                case ClassId.NoClass:
                    return (float) ((double) asda2Stamina * (double) CharacterFormulas.HealthPointsPerStamina +
                                    (double) asda2Strength * (double) CharacterFormulas.HealthPointsPerStrength);
                case ClassId.OHS:
                    return (float) ((double) asda2Stamina * (double) CharacterFormulas.HealthPointsPerStamina *
                                    1.60000002384186 + (double) asda2Strength *
                                    (double) CharacterFormulas.HealthPointsPerStrength * 1.0);
                case ClassId.Spear:
                    return (float) ((double) asda2Stamina * (double) CharacterFormulas.HealthPointsPerStamina *
                                    2.40000009536743 + (double) asda2Strength *
                                    (double) CharacterFormulas.HealthPointsPerStrength * 1.0);
                case ClassId.THS:
                    return (float) ((double) asda2Stamina * (double) CharacterFormulas.HealthPointsPerStamina *
                                    2.09999990463257 + (double) asda2Strength *
                                    (double) CharacterFormulas.HealthPointsPerStrength * 1.10000002384186);
                case ClassId.Crossbow:
                    return (float) ((double) asda2Stamina * (double) CharacterFormulas.HealthPointsPerStamina *
                                    1.70000004768372 + (double) asda2Strength *
                                    (double) CharacterFormulas.HealthPointsPerStrength * 0.800000011920929);
                case ClassId.Bow:
                    return (float) ((double) asda2Stamina * (double) CharacterFormulas.HealthPointsPerStamina *
                                    1.89999997615814 + (double) asda2Strength *
                                    (double) CharacterFormulas.HealthPointsPerStrength * 0.899999976158142);
                case ClassId.Balista:
                    return (float) ((double) asda2Stamina * (double) CharacterFormulas.HealthPointsPerStamina *
                                    1.89999997615814 + (double) asda2Strength *
                                    (double) CharacterFormulas.HealthPointsPerStrength * 0.899999976158142);
                case ClassId.AtackMage:
                    return (float) ((double) asda2Stamina * (double) CharacterFormulas.HealthPointsPerStamina * 1.25);
                case ClassId.SupportMage:
                    return (float) ((double) asda2Stamina * (double) CharacterFormulas.HealthPointsPerStamina *
                                    1.45000004768372);
                case ClassId.HealMage:
                    return (float) ((double) asda2Stamina * (double) CharacterFormulas.HealthPointsPerStamina *
                                    1.45000004768372);
                default:
                    return 50f;
            }
        }

        /// <summary>Расчет базового кол-ва маны у персонажа</summary>
        /// <param name="level">Уровень</param>
        /// <param name="cl">Класс</param>
        /// <returns>базовое кол-во маны</returns>
        public static int GetBaseMana(int level, ClassId cl)
        {
            switch (cl)
            {
                case ClassId.NoClass:
                    return 100 + level * 5;
                case ClassId.OHS:
                    return 100 + level * 7;
                case ClassId.Spear:
                    return 100 + level * 7;
                case ClassId.THS:
                    return 100 + level * 7;
                case ClassId.Crossbow:
                    return 100 + level * 7;
                case ClassId.Bow:
                    return 100 + level * 7;
                case ClassId.Balista:
                    return 100 + level * 7;
                case ClassId.AtackMage:
                    return 250 + level * 10;
                case ClassId.SupportMage:
                    return 250 + level * 10;
                case ClassId.HealMage:
                    return 250 + level * 10;
                default:
                    return 50;
            }
        }

        /// <summary>Расчет увеличения кол-ва маны</summary>
        /// <param name="level">Уровень</param>
        /// <param name="cl">класс</param>
        /// <param name="asda2Spirit">Энергия</param>
        /// <returns></returns>
        public static int CalculateManaBonus(int level, ClassId cl, int asda2Spirit)
        {
            switch (cl)
            {
                case ClassId.NoClass:
                    return (int) ((double) asda2Spirit * (double) CharacterFormulas.ManaPointsPerOneSpirit);
                case ClassId.OHS:
                    return (int) ((double) asda2Spirit * (double) CharacterFormulas.ManaPointsPerOneSpirit);
                case ClassId.Spear:
                    return (int) ((double) asda2Spirit * (double) CharacterFormulas.ManaPointsPerOneSpirit);
                case ClassId.THS:
                    return (int) ((double) asda2Spirit * (double) CharacterFormulas.ManaPointsPerOneSpirit);
                case ClassId.Crossbow:
                    return (int) ((double) asda2Spirit * (double) CharacterFormulas.ManaPointsPerOneSpirit);
                case ClassId.Bow:
                    return (int) ((double) asda2Spirit * (double) CharacterFormulas.ManaPointsPerOneSpirit);
                case ClassId.Balista:
                    return (int) ((double) asda2Spirit * (double) CharacterFormulas.ManaPointsPerOneSpirit);
                case ClassId.AtackMage:
                    return (int) ((double) asda2Spirit * (double) CharacterFormulas.ManaPointsPerOneSpirit * 2.0);
                case ClassId.SupportMage:
                    return (int) ((double) asda2Spirit * (double) CharacterFormulas.ManaPointsPerOneSpirit * 2.0);
                case ClassId.HealMage:
                    return (int) ((double) asda2Spirit * (double) CharacterFormulas.ManaPointsPerOneSpirit * 2.0);
                default:
                    return 50;
            }
        }

        /// <summary>Рачет шанса критического урона для физ. атаки</summary>
        /// <param name="id">Класс</param>
        /// <param name="level">Уровень</param>
        /// <param name="agility">Кол-во Ловкости</param>
        /// <param name="luck">Кол-во удачи</param>
        /// <returns>Шанс физ. крит удара</returns>
        public static float CalculatePsysicCritChance(ClassId id, int level, int luck)
        {
            float num1;
            switch (id)
            {
                case ClassId.OHS:
                    num1 = (float) ((double) luck * (double) CharacterFormulas.OnePrcCritPerLuck * 1.10000002384186);
                    break;
                case ClassId.Spear:
                    num1 = (float) ((double) luck * (double) CharacterFormulas.OnePrcCritPerLuck * 1.20000004768372);
                    break;
                case ClassId.THS:
                    num1 = (float) ((double) luck * (double) CharacterFormulas.OnePrcCritPerLuck * 0.699999988079071);
                    break;
                case ClassId.Crossbow:
                    num1 = (float) ((double) luck * (double) CharacterFormulas.OnePrcCritPerLuck * 1.5);
                    break;
                case ClassId.Bow:
                    num1 = (float) ((double) luck * (double) CharacterFormulas.OnePrcCritPerLuck * 1.20000004768372);
                    break;
                case ClassId.Balista:
                    num1 = (float) ((double) luck * (double) CharacterFormulas.OnePrcCritPerLuck * 0.600000023841858);
                    break;
                case ClassId.AtackMage:
                    float num2 = (float) ((double) luck * (double) CharacterFormulas.OnePrcCritPerLuck * 1.0);
                    if ((double) num2 <= 35.0)
                        return num2;
                    return 35f;
                case ClassId.SupportMage:
                    float num3 = (float) ((double) luck * (double) CharacterFormulas.OnePrcCritPerLuck * 1.0);
                    if ((double) num3 <= 35.0)
                        return num3;
                    return 35f;
                case ClassId.HealMage:
                    float num4 = (float) ((double) luck * (double) CharacterFormulas.OnePrcCritPerLuck * 1.0);
                    if ((double) num4 <= 35.0)
                        return num4;
                    return 35f;
                default:
                    num1 = (float) ((double) luck * (double) CharacterFormulas.OnePrcCritPerLuck * 1.0);
                    break;
            }

            if ((double) num1 <= 50.0)
                return num1;
            return 50f;
        }

        /// <summary>Расчет увеличения маг атаки</summary>
        /// <param name="level"></param>
        /// <param name="cl"></param>
        /// <param name="asda2Intellect"></param>
        /// <returns></returns>
        public static float CalculateMagicDamageBonus(int level, ClassId cl, int asda2Intellect)
        {
            switch (cl)
            {
                case ClassId.AtackMage:
                    return (float) asda2Intellect * CharacterFormulas.DamagePerIntelect;
                case ClassId.SupportMage:
                    return (float) ((double) asda2Intellect * (double) CharacterFormulas.DamagePerIntelect *
                                    0.699999988079071);
                case ClassId.HealMage:
                    return (float) ((double) asda2Intellect * (double) CharacterFormulas.DamagePerIntelect *
                                    0.800000011920929);
                default:
                    return 0.0f;
            }
        }

        /// <summary>Расчет бонуса физ атаки</summary>
        /// <param name="level"></param>
        /// <param name="asda2Agility"></param>
        /// <param name="strength"></param>
        /// <param name="cl"></param>
        /// <returns></returns>
        public static float CalculatePsysicalDamageBonus(int level, int asda2Agility, int strength, ClassId cl)
        {
            switch (cl)
            {
                case ClassId.OHS:
                    return (float) (((double) asda2Agility * (double) CharacterFormulas.DamagePerAgility +
                                     (double) strength * (double) CharacterFormulas.DamagePerStrength) *
                                    0.899999976158142);
                case ClassId.Spear:
                    return (float) (((double) asda2Agility * (double) CharacterFormulas.DamagePerAgility +
                                     (double) strength * (double) CharacterFormulas.DamagePerStrength) *
                                    1.10000002384186);
                case ClassId.THS:
                    return (float) (((double) asda2Agility * (double) CharacterFormulas.DamagePerAgility +
                                     (double) strength * (double) CharacterFormulas.DamagePerStrength) *
                                    1.29999995231628);
                case ClassId.Crossbow:
                    return (float) (((double) asda2Agility * (double) CharacterFormulas.DamagePerAgility +
                                     (double) strength * (double) CharacterFormulas.DamagePerStrength) *
                                    1.04999995231628);
                case ClassId.Bow:
                    return (float) (((double) asda2Agility * (double) CharacterFormulas.DamagePerAgility +
                                     (double) strength * (double) CharacterFormulas.DamagePerStrength) *
                                    1.04999995231628);
                case ClassId.Balista:
                    return (float) (((double) asda2Agility * (double) CharacterFormulas.DamagePerAgility +
                                     (double) strength * (double) CharacterFormulas.DamagePerStrength) * 1.0);
                default:
                    return (float) (((double) asda2Agility * (double) CharacterFormulas.DamagePerAgility +
                                     (double) strength * (double) CharacterFormulas.DamagePerStrength) * 1.0);
            }
        }

        /// <summary>Расчет уменьшения времени между 2мя ударами</summary>
        /// <param name="level"></param>
        /// <param name="cl"></param>
        /// <param name="asda2Agility"></param>
        /// <returns></returns>
        public static float CalculateAtackTimeReduce(int level, ClassId cl, int asda2Agility)
        {
            return (float) ((double) asda2Agility * (double) CharacterFormulas.OnePrcAtackTimeReducePerAgilityPoints *
                            0.00999999977648258);
        }

        /// <summary>Расчет усиления крит урона в %</summary>
        /// <param name="level"></param>
        /// <param name="cl"></param>
        /// <param name="asda2Agility"></param>
        /// <param name="luck"></param>
        /// <param name="intelect"></param>
        /// <param name="strength"></param>
        /// <returns></returns>
        public static int CalculateCriticalDamageBonus(int level, ClassId cl, int asda2Agility, int luck, int intelect,
            int strength)
        {
            switch (cl)
            {
                case ClassId.NoClass:
                    return (int) ((double) CharacterFormulas.CritDamageBonusPerStrength * (double) strength);
                case ClassId.OHS:
                    return (int) ((double) CharacterFormulas.CritDamageBonusPerStrength * (double) strength);
                case ClassId.Spear:
                    return (int) ((double) CharacterFormulas.CritDamageBonusPerStrength * (double) strength);
                case ClassId.THS:
                    return (int) ((double) CharacterFormulas.CritDamageBonusPerStrength * (double) strength);
                case ClassId.Crossbow:
                    return (int) ((double) CharacterFormulas.CritDamageBonusPerStrength * (double) strength);
                case ClassId.Bow:
                    return (int) ((double) CharacterFormulas.CritDamageBonusPerStrength * (double) strength);
                case ClassId.Balista:
                    return (int) ((double) CharacterFormulas.CritDamageBonusPerStrength * (double) strength);
                case ClassId.AtackMage:
                    return (int) ((double) intelect * (double) CharacterFormulas.CritDamageBonusPerIntellect);
                case ClassId.SupportMage:
                    return (int) ((double) intelect * (double) CharacterFormulas.CritDamageBonusPerIntellect);
                case ClassId.HealMage:
                    return (int) ((double) intelect * (double) CharacterFormulas.CritDamageBonusPerIntellect);
                default:
                    return 0;
            }
        }

        /// <summary>Расчет увеличения маг. защиты</summary>
        /// <param name="level">Уровень</param>
        /// <param name="cl">Класс</param>
        /// <param name="asda2Spirit">Энергия</param>
        /// <returns>бонус маг защиты</returns>
        public static float CalculateMagicDefencePointsBonus(int level, ClassId cl, int asda2Spirit)
        {
            switch (cl)
            {
                case ClassId.NoClass:
                    return (float) asda2Spirit * CharacterFormulas.OneMagicDefencePerSpiritPoints;
                case ClassId.OHS:
                    return (float) ((double) asda2Spirit * (double) CharacterFormulas.OneMagicDefencePerSpiritPoints *
                                    2.20000004768372);
                case ClassId.Spear:
                    return (float) asda2Spirit * CharacterFormulas.OneMagicDefencePerSpiritPoints;
                case ClassId.THS:
                    return (float) asda2Spirit * CharacterFormulas.OneMagicDefencePerSpiritPoints;
                case ClassId.Crossbow:
                    return (float) asda2Spirit * CharacterFormulas.OneMagicDefencePerSpiritPoints;
                case ClassId.Bow:
                    return (float) asda2Spirit * CharacterFormulas.OneMagicDefencePerSpiritPoints;
                case ClassId.Balista:
                    return (float) asda2Spirit * CharacterFormulas.OneMagicDefencePerSpiritPoints;
                case ClassId.AtackMage:
                    return (float) asda2Spirit * CharacterFormulas.OneMagicDefencePerSpiritPoints;
                case ClassId.SupportMage:
                    return (float) ((double) asda2Spirit * (double) CharacterFormulas.OneMagicDefencePerSpiritPoints *
                                    1.29999995231628);
                case ClassId.HealMage:
                    return (float) ((double) asda2Spirit * (double) CharacterFormulas.OneMagicDefencePerSpiritPoints *
                                    1.29999995231628);
                default:
                    return 50f;
            }
        }

        /// <summary>Расчет бонуса защиты</summary>
        /// <param name="level"></param>
        /// <param name="cl"></param>
        /// <param name="asda2Agility"></param>
        /// <returns></returns>
        public static float ClaculateDefenceBonus(int level, ClassId cl, int asda2Agility)
        {
            return 0.0f;
        }

        /// <summary>Расчет шанса уворота</summary>
        /// <param name="level"></param>
        /// <param name="cl"></param>
        /// <param name="asda2Agility"></param>
        /// <returns></returns>
        public static float CalcDodgeChanceBonus(int level, ClassId cl, int asda2Agility)
        {
            switch (cl)
            {
                case ClassId.NoClass:
                    return (float) asda2Agility * CharacterFormulas.DodgePerAgility;
                case ClassId.OHS:
                    return (float) ((double) asda2Agility * (double) CharacterFormulas.DodgePerAgility *
                                    0.699999988079071);
                case ClassId.Spear:
                    return (float) ((double) asda2Agility * (double) CharacterFormulas.DodgePerAgility *
                                    1.20000004768372);
                case ClassId.THS:
                    return (float) asda2Agility * CharacterFormulas.DodgePerAgility;
                case ClassId.Crossbow:
                    return (float) ((double) asda2Agility * (double) CharacterFormulas.DodgePerAgility * 1.5);
                case ClassId.Bow:
                    return (float) ((double) asda2Agility * (double) CharacterFormulas.DodgePerAgility * 1.5);
                case ClassId.Balista:
                    return (float) ((double) asda2Agility * (double) CharacterFormulas.DodgePerAgility * 1.5);
                case ClassId.AtackMage:
                    return (float) asda2Agility * CharacterFormulas.DodgePerAgility;
                case ClassId.SupportMage:
                    return (float) asda2Agility * CharacterFormulas.DodgePerAgility;
                case ClassId.HealMage:
                    return (float) asda2Agility * CharacterFormulas.DodgePerAgility;
                default:
                    return 0.0f;
            }
        }

        /// <summary>Расчет бонуса скорости %</summary>
        /// <param name="level"></param>
        /// <param name="cl"></param>
        /// <param name="asda2Agility"></param>
        /// <returns></returns>
        public static float CalcSpeedBonus(int level, ClassId cl, int asda2Agility)
        {
            switch (cl)
            {
                case ClassId.NoClass:
                    return (float) ((double) asda2Agility * (double) CharacterFormulas.SpeedPerAgility *
                                    0.699999988079071);
                case ClassId.OHS:
                    return (float) ((double) asda2Agility * (double) CharacterFormulas.SpeedPerAgility *
                                    0.699999988079071);
                case ClassId.Spear:
                    return (float) ((double) asda2Agility * (double) CharacterFormulas.SpeedPerAgility *
                                    0.699999988079071);
                case ClassId.THS:
                    return (float) ((double) asda2Agility * (double) CharacterFormulas.SpeedPerAgility *
                                    0.699999988079071);
                case ClassId.Crossbow:
                    return (float) asda2Agility * CharacterFormulas.SpeedPerAgility;
                case ClassId.Bow:
                    return (float) asda2Agility * CharacterFormulas.SpeedPerAgility;
                case ClassId.Balista:
                    return (float) asda2Agility * CharacterFormulas.SpeedPerAgility;
                case ClassId.AtackMage:
                    return (float) ((double) asda2Agility * (double) CharacterFormulas.SpeedPerAgility *
                                    0.699999988079071);
                case ClassId.SupportMage:
                    return (float) ((double) asda2Agility * (double) CharacterFormulas.SpeedPerAgility *
                                    0.699999988079071);
                case ClassId.HealMage:
                    return (float) ((double) asda2Agility * (double) CharacterFormulas.SpeedPerAgility *
                                    0.699999988079071);
                default:
                    return 0.0f;
            }
        }

        /// <summary>Расчет увеличения шанса выпадения предметов</summary>
        /// <param name="level">Уровень</param>
        /// <param name="cl">Класс</param>
        /// <param name="asda2Luck">Удача</param>
        /// <returns>Увелицение шанса %</returns>
        public static float CalculateDropChanceBoost(int asda2Luck)
        {
            return (float) asda2Luck * CharacterFormulas.OnePrcDropChancePerLuck;
        }

        /// <summary>Расчет увеличения кол-ва выпадаемого золота</summary>
        /// <param name="level">Уровень</param>
        /// <param name="cl">Класс</param>
        /// <param name="asda2Luck">Удача</param>
        /// <returns>Увелицение шанса</returns>
        public static float CalculateGoldAmountDropBoost(int level, ClassId cl, int asda2Luck)
        {
            return (float) asda2Luck * CharacterFormulas.OnePrcGoldAmountPerLuckPoints;
        }

        /// <summary>Расчет шанса заточки предмета</summary>
        /// <param name="stoneQuality">Грейд камня которым точат</param>
        /// <param name="itemQuality">Грейд предмета </param>
        /// <param name="enchant">На сколько сейчас точим</param>
        /// <param name="requiredLevel">Уровень предмета</param>
        /// <param name="ownerLuck">Удача точильщика</param>
        /// <param name="groupLuck">Удача группы</param>
        /// <param name="nearblyCharactersLuck">Сумарная удача Всех персонажей который находятся рядом с точильщиком (Таже фрация дает + противоположная -) Мимимальное значение 0</param>
        /// <param name="useProtect">Использовать защиту предмета</param>
        /// <param name="useChanceBoost">Использовать предмет увеличения шанса</param>
        /// <param name="noEnchantLose"> </param>
        /// <returns>Результат заточки</returns>
        public static ItemUpgradeResult CalculateItemUpgradeResult(Asda2ItemQuality stoneQuality,
            Asda2ItemQuality itemQuality, byte enchant, byte requiredLevel, int ownerLuck, int groupLuck,
            int nearblyCharactersLuck, bool useProtect, int useChanceBoost, bool noEnchantLose)
        {
            float num1 = 200f;
            switch (itemQuality)
            {
                case Asda2ItemQuality.White:
                    num1 -= 5f;
                    break;
                case Asda2ItemQuality.Yello:
                    num1 -= 10f;
                    break;
                case Asda2ItemQuality.Purple:
                    num1 -= 25f;
                    break;
                case Asda2ItemQuality.Green:
                    num1 -= 40f;
                    break;
                case Asda2ItemQuality.Orange:
                    num1 -= 50f;
                    break;
            }

            switch (stoneQuality)
            {
                case Asda2ItemQuality.White:
                    num1 += 0.0f;
                    break;
                case Asda2ItemQuality.Yello:
                    num1 += 10f;
                    break;
                case Asda2ItemQuality.Purple:
                    num1 += 30f;
                    break;
                case Asda2ItemQuality.Green:
                    num1 += 1000f;
                    break;
                case Asda2ItemQuality.Orange:
                    num1 += 1000f;
                    break;
            }

            int num2 = 0;
            int num3 = 0;
            int num4 = 0;
            float num5 = num1 / (float) Math.Pow((double) enchant + 0.100000001490116, 0.75);
            if (enchant < (byte) 10)
                num5 = (float) ((double) num5 * 1.29999995231628 + (double) enchant * 0.899999976158142);
            if (enchant > (byte) 15)
                num5 *= 0.4f;
            double chance =
                ((double) num5 + (double) num5 * ((double) (num2 + num3 + num4) -
                                                  Math.Pow((double) requiredLevel, 0.850000023841858) / 100.0)) *
                (1.0 + (double) useChanceBoost / 100.0);
            if (chance > 100.0)
                chance = 100.0;
            if (chance < 0.0)
                chance = 0.0;
            if (enchant >= (byte) 20)
                chance = 0.0;
            float num6 = Utility.Random(0.0f, 100f);
            if ((double) num6 <= chance)
                return new ItemUpgradeResult(ItemUpgradeResultStatus.Success, (float) num3, (float) num4, (float) num2,
                    chance);
            if ((double) num6 < chance + (100.0 - chance) / 6.0)
            {
                if (useProtect)
                {
                    if (enchant <= (byte) 10)
                        return new ItemUpgradeResult(ItemUpgradeResultStatus.Fail, (float) num3, (float) num4,
                            (float) num2, chance);
                    if (noEnchantLose)
                        return new ItemUpgradeResult(ItemUpgradeResultStatus.Fail, (float) num3, (float) num4,
                            (float) num2, chance);
                    return new ItemUpgradeResult(ItemUpgradeResultStatus.ReduceOneLevel, (float) num3, (float) num4,
                        (float) num2, chance);
                }

                if (enchant > (byte) 7)
                    return new ItemUpgradeResult(ItemUpgradeResultStatus.BreakItem, (float) num3, (float) num4,
                        (float) num2, chance);
                if (enchant > (byte) 4)
                    return new ItemUpgradeResult(ItemUpgradeResultStatus.ReduceOneLevel, (float) num3, (float) num4,
                        (float) num2, chance);
                return new ItemUpgradeResult(ItemUpgradeResultStatus.Fail, (float) num3, (float) num4, (float) num2,
                    chance);
            }

            if ((double) num6 < chance + (100.0 - chance) / 4.0)
            {
                if (useProtect)
                {
                    if (enchant <= (byte) 10)
                        return new ItemUpgradeResult(ItemUpgradeResultStatus.Fail, (float) num3, (float) num4,
                            (float) num2, chance);
                    if (noEnchantLose)
                        return new ItemUpgradeResult(ItemUpgradeResultStatus.Fail, (float) num3, (float) num4,
                            (float) num2, chance);
                    return new ItemUpgradeResult(ItemUpgradeResultStatus.ReduceOneLevel, (float) num3, (float) num4,
                        (float) num2, chance);
                }

                if (enchant > (byte) 7)
                    return new ItemUpgradeResult(ItemUpgradeResultStatus.ReduceLevelToZero, (float) num3, (float) num4,
                        (float) num2, chance);
                return new ItemUpgradeResult(ItemUpgradeResultStatus.Fail, (float) num3, (float) num4, (float) num2,
                    chance);
            }

            if ((double) num6 >= chance + (100.0 - chance) / 2.0)
                return new ItemUpgradeResult(ItemUpgradeResultStatus.Fail, (float) num3, (float) num4, (float) num2,
                    chance);
            if (useProtect)
                return new ItemUpgradeResult(ItemUpgradeResultStatus.Fail, (float) num3, (float) num4, (float) num2,
                    chance);
            if (enchant > (byte) 7)
                return new ItemUpgradeResult(ItemUpgradeResultStatus.ReduceOneLevel, (float) num3, (float) num4,
                    (float) num2, chance);
            return new ItemUpgradeResult(ItemUpgradeResultStatus.Fail, (float) num3, (float) num4, (float) num2,
                chance);
        }

        /// <summary>
        /// Рачет усиления характеристик предмета в зависимости от заточки(Усиливаются Урон)
        /// </summary>
        /// <param name="enchant"></param>
        /// <returns></returns>
        public static float CalculateEnchantMultiplier(byte enchant)
        {
            float num = enchant == (byte) 0
                ? 1f
                : (float) Math.Pow((double) enchant, (double) CharacterFormulas.EnchantPowValue);
            switch (enchant)
            {
                case 16:
                    num *= 1.2f;
                    break;
                case 17:
                    num *= 1.3f;
                    break;
                case 18:
                    num *= 1.4f;
                    break;
                case 19:
                    num *= 1.5f;
                    break;
                case 20:
                    num *= 2.5f;
                    break;
            }

            return num;
        }

        /// <summary>
        /// Рачет усиления характеристик предмета в зависимости от заточки(Усиливаются Эфекти от ЖК и параметры)
        /// </summary>
        /// <param name="enchant"></param>
        /// <returns></returns>
        public static float CalculateEnchantMultiplierNotDamageItemStats(byte enchant)
        {
            return enchant == (byte) 0
                ? 1f
                : (float) Math.Pow((double) enchant, (double) CharacterFormulas.EnchantPowValueForNotDamageStats);
        }

        public static int GetWaveRewardItems(List<KeyValuePair<int, int>> items)
        {
            int num = Utility.Random(0, 100000);
            foreach (KeyValuePair<int, int> keyValuePair in items)
            {
                if (num < keyValuePair.Value)
                    return keyValuePair.Key;
            }

            return 1;
        }

        /// <summary>Раритетность шмотки которую мы крафтим</summary>
        /// <returns></returns>
        public static byte GetCraftedRarity()
        {
            int num = Utility.Random(0, 100000);
            if (num < 15000)
                return 0;
            if (num < 50000)
                return 1;
            if (num < 85000)
                return 2;
            if (num < 98000)
                return 3;
            return num < 99900 ? (byte) 4 : (byte) 5;
        }

        /// <summary>Расчет опыта за крафт (Прокачка крафта)</summary>
        /// <param name="diffLvl"></param>
        /// <param name="currentCraftLevel"></param>
        /// <returns></returns>
        public static float CalcCraftingExp(int diffLvl, byte currentCraftLevel)
        {
            float num = 0.0f;
            switch (diffLvl)
            {
                case 0:
                    num = 1f;
                    break;
                case 1:
                    num = 0.5f;
                    break;
                case 2:
                    num = 0.25f;
                    break;
                case 3:
                    num = 0.1f;
                    break;
                case 4:
                    num = 0.05f;
                    break;
                case 5:
                    num = 0.01f;
                    break;
                case 6:
                    num = 0.005f;
                    break;
                case 7:
                    num = 1f / 1000f;
                    break;
            }

            return num / (float) Math.Pow((double) currentCraftLevel, 2.0);
        }

        /// <summary>Расчет опыта за крафт (Прокачка персонажа)</summary>
        /// <param name="diffLvl"></param>
        /// <param name="currentCraftLevel"></param>
        /// <param name="currentCharacterLevel"></param>
        /// <returns></returns>
        public static int CalcExpForCrafting(int diffLvl, byte currentCraftLevel, byte currentCharacterLevel)
        {
            if (diffLvl == 0)
                diffLvl = 1;
            return (int) ((double) XpGenerator.GetBaseExpForLevel((int) currentCharacterLevel) *
                          (double) currentCraftLevel / (double) diffLvl);
        }

        /// <summary>
        /// Расчитывает раритетность полученого в результате синтеза пета.
        /// </summary>
        /// <param name="rarity">Раритетность первого пета</param>
        /// <param name="rarity2">Раритетность 2го пета</param>
        /// <returns>белый желтый фиолетовый или зеленый 0 1 2 3</returns>
        public static int CalcResultSyntesPetRarity(int rarity, int rarity2)
        {
            float num1 = (float) (((double) rarity + (double) rarity2) / 2.0);
            float num2 = Utility.Random(0.5f, 3f);
            float num3 = num1 + num2;
            if ((double) num3 < (double) num1 && (double) num1 > 1.0)
                return (int) ((double) num1 - 1.0);
            if ((double) num1 < 0.0)
                return 0;
            if ((double) num1 > 3.0)
                return 3;
            return (int) num3;
        }

        /// <summary>
        /// Расчитывает раритетность полученого в результате эволюции пета.
        /// </summary>
        /// <param name="rarity">Раритетность первого пета</param>
        /// <param name="rarity2">Раритетность 2го пета</param>
        /// <returns>белый желтый фиолетовый или зеленый 0 1 2 3</returns>
        public static int CalcResultEvolutionPetRarity(int rarity, int rarity2)
        {
            float num1 = (float) (((double) rarity + (double) rarity2) / 2.0);
            float num2 = Utility.Random(0.9f, 2.03f);
            float num3 = num1 + num2;
            if ((double) num3 < (double) num1 && (double) num1 > 1.0)
                return (int) ((double) num1 - 1.0);
            if ((double) num1 < 0.0)
                return 0;
            if ((double) num1 > 3.0)
                return 3;
            return (int) num3;
        }

        public static bool CalcPetLevelBreakSuccess()
        {
            return Utility.Random(0, 100000) < 70000;
        }

        /// <summary>
        /// Расчет увеличился ли уровень рыбалки. Расчет производится только если рыбался удалась.
        /// </summary>
        /// <param name="curFishingLevel">текущий уровень рыбалки рыбака</param>
        /// <returns></returns>
        public static bool CalcFishingLevelRised(int curFishingLevel)
        {
            return (double) CharacterFormulas.StandartFishingLevelUpChance / Math.Pow((double) curFishingLevel, 0.4) <
                   (double) Utility.Random(0, 100000);
        }

        /// <summary>Расчет удачно ли выловилась рыба</summary>
        /// <param name="fishingLevel">уровень рыбалки рыбака</param>
        /// <param name="requiredFishingLevel">необходимый уровень рыбалки на месте</param>
        /// <param name="asda2Luck"></param>
        /// <returns></returns>
        public static bool CalcFishingSuccess(int fishingLevel, int requiredFishingLevel, int asda2Luck)
        {
            float num = (float) (fishingLevel - requiredFishingLevel) / 3.5f;
            return ((double) CharacterFormulas.StandartFishingChance + (double) num) *
                   Math.Pow((double) (asda2Luck + 1), 0.0199999995529652) > (double) Utility.Random(0, 100);
        }

        /// <summary>Уменьшится ли прочность удочки на 1ед.</summary>
        /// <returns></returns>
        public static bool DecraseRodDurability()
        {
            return CharacterFormulas.DecreaceRodDurabilityChance > Utility.Random(0, 100000);
        }

        /// <summary>Опыт получаемый от рыбалки.</summary>
        /// <param name="level">уровень персонажа</param>
        /// <param name="fishingLevel">уромень рыбалки</param>
        /// <param name="quality">качетво выловленой рыбы</param>
        /// <param name="requiredFishingLevel">требуемый уровень локации</param>
        /// <param name="fishSize">размер рыбы</param>
        /// <returns></returns>
        public static int CalcExpForFishing(int level, int fishingLevel, Asda2ItemQuality quality,
            int requiredFishingLevel, short fishSize)
        {
            return (int) ((double) XpGenerator.GetBaseExpForLevel(level) /
                          Math.Pow((double) (fishingLevel - requiredFishingLevel), 0.2) * (double) fishSize / 50.0);
        }

        /// <summary>Расчет опыта за копку</summary>
        /// <param name="level">Уровень персонажа</param>
        /// <param name="minLocationLevel">Минимальный уровень копки в локации.</param>
        /// <returns></returns>
        public static int CalcDiggingExp(int level, int minLocationLevel)
        {
            return (int) ((double) XpGenerator.GetBaseExpForLevel(level) *
                          Math.Pow((double) minLocationLevel, 0.200000002980232) / 4.0);
        }

        public static int CalcHonorPoints(int level, short battlegroundActPoints, bool isWiner, int battlegroundDeathes,
            int battlegroundKills, bool isMvp, Asda2BattlegroundTown town)
        {
            switch (town)
            {
                case Asda2BattlegroundTown.Alpia:
                    return (int) ((double) battlegroundActPoints * Math.Pow((double) (30 - level), 0.200000002980232) *
                                  Math.Pow(
                                      battlegroundKills <= battlegroundDeathes
                                          ? 1.0
                                          : (double) (battlegroundKills - battlegroundDeathes), 0.150000005960464) *
                                  (isMvp ? 1.5 : 1.0) * (isWiner ? 2.0 : 1.0) *
                                  (double) CharacterFormulas.AlpiaBaseHonorPoints);
                case Asda2BattlegroundTown.Silaris:
                    return (int) ((double) battlegroundActPoints * Math.Pow((double) (50 - level), 0.200000002980232) *
                                  Math.Pow(
                                      battlegroundKills <= battlegroundDeathes
                                          ? 1.0
                                          : (double) (battlegroundKills - battlegroundDeathes), 0.150000005960464) *
                                  (isMvp ? 1.5 : 1.0) * (isWiner ? 2.0 : 1.0) *
                                  (double) CharacterFormulas.SilarisBaseHonorPoints);
                case Asda2BattlegroundTown.Flamio:
                    return (int) ((double) battlegroundActPoints * Math.Pow((double) (70 - level), 0.200000002980232) *
                                  Math.Pow(
                                      battlegroundKills <= battlegroundDeathes
                                          ? 1.0
                                          : (double) (battlegroundKills - battlegroundDeathes), 0.150000005960464) *
                                  (isMvp ? 1.5 : 1.0) * (isWiner ? 2.0 : 1.0) *
                                  (double) CharacterFormulas.FlamioBaseHonorPoints);
                case Asda2BattlegroundTown.Aquaton:
                    return (int) ((double) battlegroundActPoints * Math.Pow((double) (100 - level), 0.200000002980232) *
                                  Math.Pow(
                                      battlegroundKills <= battlegroundDeathes
                                          ? 1.0
                                          : (double) (battlegroundKills - battlegroundDeathes), 0.150000005960464) *
                                  (isMvp ? 1.5 : 1.0) * (isWiner ? 2.0 : 1.0) *
                                  (double) CharacterFormulas.FlamioBaseHonorPoints);
                default:
                    return 0;
            }
        }

        static CharacterFormulas()
        {
            for (int index = 0; index < 21; ++index)
                CharacterFormulas.HonorRankPoints[index] = (int) (1000.0 * Math.Pow((double) index, 3.0));
        }

        /// <summary>
        /// Возвращает ранг персонажа по количеству его хонор пойнтов
        /// </summary>
        /// <param name="asda2HonorPoints">кол-во хонор пойнтов</param>
        /// <returns>Ранг</returns>
        public static int GetFactionRank(int asda2HonorPoints)
        {
            for (int index = 1; index < 21; ++index)
            {
                if (CharacterFormulas.HonorRankPoints[index] > asda2HonorPoints)
                    return index - 1;
            }

            return 0;
        }

        public static short CalcBattlegrounActPointsOnKill(int killerLevel, int victimLevel, short killerActPoints,
            short victimActPoints)
        {
            int num1 = victimLevel - killerLevel;
            if (num1 > 0 && num1 < 3 || num1 < 0 && num1 > -5)
                num1 = 0;
            int num2 = (int) victimActPoints - (int) killerActPoints;
            if (num2 > 0 && num2 < 20 || num2 < 0 && num2 > -30)
                num2 = 0;
            bool flag1 = num1 < 0;
            bool flag2 = num2 < 0;
            double num3 = 2.5 * Math.Pow((double) Math.Abs(num1), 0.300000011920929);
            double num4 = 3.0 * Math.Pow((double) Math.Abs(num2), 0.349999994039536);
            short num5 = (short) ((double) CharacterFormulas.BaseActPointsOnKill + (flag1 ? -num3 : num3) +
                                  (flag2 ? -num4 : num4));
            if (num5 <= (short) 5)
                return 5;
            return num5;
        }

        /// <summary>Удачен ли синтез аватара</summary>
        /// <param name="curEnchant">текущая заточка аватара</param>
        /// <param name="useSupl">используется ли увеличение шанса</param>
        /// <returns>да или нет</returns>
        public static bool IsAvatarSyntesSuccess(byte curEnchant, bool useSupl, Asda2ItemQuality quality)
        {
            int num1 = Utility.Random(0, 100000);
            int num2 = 0;
            switch (curEnchant)
            {
                case 0:
                    num2 = (int) (quality + 1) * 10000 + (useSupl ? 30000 : 0);
                    break;
                case 1:
                    num2 = (int) (quality + 1) * 3000 + (useSupl ? 10000 : 0);
                    break;
            }

            return num2 > num1;
        }

        /// <summary>рачет опыта за съедание яблока</summary>
        /// <param name="firstChrLvl">сумма уровней персов 1го перса</param>
        /// <param name="soulmatingLvl">уровень дружбы</param>
        /// <returns></returns>
        public static int CalcFriendAppleExp(int lvl, byte soulmatingLvl)
        {
            return (int) ((double) XpGenerator.GetBaseExpForLevel(lvl) *
                          Math.Pow((double) soulmatingLvl, 0.300000011920929) * (double) Utility.Random(5, 20));
        }

        /// <summary>Расчет шанса раскопок</summary>
        /// <param name="initialChance">Изначальный шанс лопаты</param>
        /// <param name="soulmatePoints">очки друга</param>
        /// <param name="luck">очки удачи</param>
        /// <returns>шанс 0 - 100000</returns>
        public static int CalculateDiggingChance(int initialChance, byte soulmatePoints, int luck)
        {
            return (int) ((double) initialChance * Math.Pow((double) ((int) soulmatePoints + 1), 0.0500000007450581) *
                          Math.Pow((double) (luck + 1), 0.0199999995529652));
        }

        /// <summary>Количество очков за перерождение.</summary>
        /// <returns></returns>
        public static int CalculateStatPointsBonusOnReset(int rebornsCount)
        {
            if (rebornsCount == 0)
                rebornsCount = 1;
            return (int) ((double) Utility.Random(400, 600) / Math.Pow((double) rebornsCount, 0.300000011920929));
        }

        /// <summary>Расчет очков распределения за каждый уровень</summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public static int CalculateFreeStatPointForLevel(int level, int rebornCount)
        {
            double rebornBoost = CharacterFormulas.CalcRebornBoost(rebornCount);
            double num = 15.0 * rebornBoost;
            for (int index = 2; index <= level; ++index)
                num += CharacterFormulas.CalcStatBonusPerLevel(level, rebornBoost);
            return (int) num;
        }

        private static double CalcRebornBoost(int rebornCount)
        {
            double num = 1.0;
            for (int index = 1; index <= rebornCount; ++index)
            {
                switch (index)
                {
                    case 1:
                        num += 0.25;
                        break;
                    case 2:
                        num += 0.200000002980232;
                        break;
                    case 3:
                        num += 0.150000005960464;
                        break;
                    default:
                        num += 0.100000001490116;
                        break;
                }
            }

            return num;
        }

        public static double CalcStatBonusPerLevel(int level, double rebornBoost)
        {
            return Math.Pow((double) level, 0.600000023841858) * 4.0 * rebornBoost;
        }

        public static double CalcStatBonusPerLevel(int level, int rebornCount)
        {
            return Math.Pow((double) level, 0.600000023841858) * 4.0 * CharacterFormulas.CalcRebornBoost(rebornCount);
        }

        public static uint CalculteItemRepairCost(byte maxDurability, byte durability, uint sellPrice, byte enchant,
            byte levelCriterion, byte qualityCriterion)
        {
            return (uint) ((double) ((int) maxDurability - (int) durability) * (double) sellPrice / 100.0 *
                           (1.0 + 0.05 * (double) qualityCriterion) * (1.0 + 0.05 * (double) levelCriterion) *
                           (1.0 + 0.05 * (double) enchant));
        }

        public static int GetSowelDeffence(int value, Asda2Profession requiredProfession)
        {
            switch (requiredProfession)
            {
                case Asda2Profession.NoProfession:
                    return (int) ((double) value * 5.0 * (double) CharacterFormulas.ItemsDeffenceMultiplier);
                case Asda2Profession.Warrior:
                    return (int) ((double) value * 5.0 * (double) CharacterFormulas.ItemsDeffenceMultiplier);
                case Asda2Profession.Archer:
                    return (int) ((double) value * 4.5 * (double) CharacterFormulas.ItemsDeffenceMultiplier);
                case Asda2Profession.Mage:
                    return (int) ((double) value * 3.90000009536743 *
                                  (double) CharacterFormulas.ItemsDeffenceMultiplier);
                case Asda2Profession.Any:
                    return 0;
                default:
                    return 0;
            }
        }

        public static float CalcWeaponTypeMultiplier(Asda2ItemCategory category, ClassId classId)
        {
            switch (category)
            {
                case Asda2ItemCategory.OneHandedSword:
                    return 16.5f;
                case Asda2ItemCategory.TwoHandedSword:
                    return 14.5f;
                case Asda2ItemCategory.Staff:
                    switch (classId)
                    {
                        case ClassId.AtackMage:
                            return 15.2f;
                        case ClassId.SupportMage:
                            return 13.45f;
                        case ClassId.HealMage:
                            return 13.45f;
                        default:
                            return 0.0f;
                    }
                case Asda2ItemCategory.Crossbow:
                    return 17.1f;
                case Asda2ItemCategory.Bow:
                    return 17.5f;
                case Asda2ItemCategory.Spear:
                    return 16.2f;
                default:
                    return 1f;
            }
        }

        public static int CalcGoldAmountToResetStats(int str, int dex, int sta, int spi, int luck, int intillect,
            byte chrLevel, int resetsCount)
        {
            return (str + dex + sta + spi + luck + intillect) * 10;
        }

        public static double CalcShieldBlockPrc(Asda2ItemQuality quality, uint requiredLevel)
        {
            double num = Math.Pow((double) requiredLevel, 0.5) * 0.0199999995529652;
            switch (quality)
            {
                case Asda2ItemQuality.White:
                    return 0.100000001490116 + num;
                case Asda2ItemQuality.Yello:
                    return 0.150000005960464 + num;
                case Asda2ItemQuality.Purple:
                    return 0.200000002980232 + num;
                case Asda2ItemQuality.Green:
                    return 0.25 + num;
                case Asda2ItemQuality.Orange:
                    return 0.300000011920929 + num;
                default:
                    return 0.0500000007450581 + num;
            }
        }

        public static int CalcExpForGuessWordEvent(int level)
        {
            return XpGenerator.GetBaseExpForLevel(level) * 25;
        }

        public static float CalcHpPotionBoost(int asda2Stamina)
        {
            return (float) (1.0 + (double) CharacterFormulas.HpPotionsBoostPerStamina * (double) asda2Stamina);
        }

        public class ItemIdAmounted
        {
            public int ItemId { get; set; }

            public int Amount { get; set; }

            public ItemIdAmounted(int itemId, int amount)
            {
                this.ItemId = itemId;
                this.Amount = amount;
            }
        }
    }
}