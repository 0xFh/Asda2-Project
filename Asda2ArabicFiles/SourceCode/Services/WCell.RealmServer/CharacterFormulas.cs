using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.Constants.Items;
using WCell.RealmServer.Asda2BattleGround;
using WCell.RealmServer.Formulas;
using WCell.RealmServer.Guilds;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.RacesClasses;
using WCell.Util;
using WCell.Util.NLog;
using WCell.Util.Variables;

namespace WCell.RealmServer
{

    public static class CharacterFormulas
    {

        #region Аукцион
        [NotVariable]
        /// <summary>
        /// Комиссия за выставление предмета на аукцион в %/100
        /// </summary>
        public static float AuctionPushComission = 0.02f;
        [NotVariable]
        /// <summary>
        /// Комиссия за продажу предмета с аукциона в %/100
        /// </summary>
        public static float AuctionSellComission = 0.1f;
        #endregion
        #region Клан
        /// <summary>
        /// Сколько нужно очков на поднятие следующего уровня клана на первый 0, на 2й 30000 и т.д.
        /// </summary>
        [NotVariable]
        public static int[] GuildLevelUpCost = new[] { 0, 30000, 70000, 120000, 350000, 500000, 800000, 1200000, 1750000, 2500000 };
        [NotVariable]
        public static float MaxToTotalMultiplier = 0.7f;
        [NotVariable]
        public static int OptionStatStartsWithEnchantValue = 7;
        [NotVariable]
        public static float ItemsDeffenceMultiplier = 1.5f;
        [NotVariable]
        public static float ItemsMagicDeffenceMultiplier = 1f;
        [NotVariable]
        public static float PetDeffenceMultiplier = 1f;
        [NotVariable]
        public static float PetMagicDeffenceMultiplier = 1f;
        [NotVariable]
        public static float KillPkExpPercentOfLoose = 0.5f;

        public static float GuildPointsMultiplier = 3f;
        [NotVariable]
        public static int FishingGuildPoints = (int)(5 * GuildPointsMultiplier);
        [NotVariable]
        public static int DiggingGuildPoints = (int)(2 * GuildPointsMultiplier);
        [NotVariable]
        public static int MobKillingGuildPoints = (int)(1 * GuildPointsMultiplier);
        [NotVariable]
        public static int CharacterKillingGuildPoints = (int)(10 * GuildPointsMultiplier);
        [NotVariable]
        public static int LevelupingGuildPointsPerLevel = (int)(2 * GuildPointsMultiplier);
        [NotVariable]
        public static int CraftingGuildPointsPerLevel = (int)(5 * GuildPointsMultiplier);
        [NotVariable]
        public static int BossKillingGuildPointsPerLevel = (int)(2 * GuildPointsMultiplier);
        [NotVariable]
        public static float MagicAtackSowelMultiplier = 1.15f;
        [NotVariable]
        public static float PhysicalAtackSowelMultiplier = 1.3f;
        [NotVariable]
        public static float EliteMobSocialAggrRange = 10f;
        [NotVariable]
        public static float NpcMoveUpdateDelay = 1500;
        [NotVariable]
        public static double NpcSpellUpdateDelay = 600;
        [NotVariable]
        /// <summary>
        /// Порог защиты, при такой зашиты будет срезаться 50% урона, ну и половина от первой половины)
        /// </summary>
        public static int DeffenceRow = 3500;
        [NotVariable]
        public static int MaxLootCount = 50;
        [NotVariable]
        public static float HpPotionsBoostPerStamina = 0.001f;
        [NotVariable]
        public static int MaxLvlMobCharDiff = 150;
        [NotVariable]
        public static int MaxDamagersDetailCount = 5;
        [NotVariable]
        public static int EventItemsForGuessEvent = 1;
        [NotVariable]
        public static int EventItemId = 56520;
        [NotVariable]
        public static int DonationItemId = 33800;
        [NotVariable]
        public static int RebornLevel = 150;
        [NotVariable]
        public static int DefenceTownLives = 1000;
        [NotVariable]
        public static int ForeignLootPickupTimeout = 30;
        [NotVariable]
        public static int NpcUpdatesToScanAndAttack = 50;

        public class ItemIdAmounted
        {
            public int ItemId { get; set; }
            public int Amount { get; set; }

            public ItemIdAmounted(int itemId, int amount)
            {
                ItemId = itemId;
                Amount = amount;
            }
        }
        [NotVariable]
        public static List<ItemIdAmounted> ItemIdsToAddOnReborn = new List<ItemIdAmounted>
        {
            new ItemIdAmounted(21498,1),
            new ItemIdAmounted(21499,1)
        };
        [NotVariable]
        public static float MaxDeffenceDownEventDifficulty = 30;
        [NotVariable]
        public static int SaveChateterInterval = 10 * 60 * 1000;


        [NotVariable]
        public static int TimeBetweenImNotMovingPacketSendMillis = 1000;
        //сколько живет лут перед удалением
        [NotVariable]
        public static int DropLiveMinutes = 3;

        public static void InitGuildSkills()
        {
            GuildSkillTemplate.Templates[(int)GuildSkillId.AtackPrc] = new GuildSkillTemplate()
            {
                BonusValuses = new[] { 0, 3, 4, 5, 6, 7, 8, 10 },
                LearnCosts = new[] { 0, 60000, 100000, 400000, 700000, 1000000, 2000000, 3000000 },
                ActivationCosts = new[] { 0, 6000, 10000, 40000, 70000, 100000, 200000, 300000 },
                MaitenceCosts = new[] { 0, 1200, 2000, 8000, 14000, 20000, 40000, 60000 },
                MaxLevel = 7
            };
            GuildSkillTemplate.Templates[(int)GuildSkillId.DeffencePrc] = new GuildSkillTemplate()
            {
                BonusValuses = new[] { 0, 3, 4, 5, 6, 7, 8, 10 },
                LearnCosts = new[] { 0, 25000, 60000, 100000, 300000, 450000, 1200000, 2000000 },
                ActivationCosts = new[] { 0, 2500, 6000, 10000, 30000, 45000, 120000, 200000 },
                MaitenceCosts = new[] { 0, 500, 1200, 2000, 6000, 9000, 24000, 40000 },
                MaxLevel = 7
            };
            GuildSkillTemplate.Templates[(int)GuildSkillId.MovingSpeedPrc] = new GuildSkillTemplate()
            {
                BonusValuses = new[] { 0, 3, 4, 5, 6, 7, 8, 10 },
                LearnCosts = new[] { 0, 100000, 300000, 400000, 700000, 1000000, 1500000, 2500000 },
                ActivationCosts = new[] { 0, 10000, 30000, 40000, 70000, 100000, 150000, 250000 },
                MaitenceCosts = new[] { 0, 2000, 6000, 8000, 14000, 20000, 30000, 50000 },
                MaxLevel = 7
            };
            GuildSkillTemplate.Templates[(int)GuildSkillId.AtackSpeedPrc] = new GuildSkillTemplate()
            {
                BonusValuses = new[] { 0, 1, 2, 3, 4, 5, 6, 7 },
                LearnCosts = new[] { 0, 550000, 1000000, 1350000, 1800000, 2400000, 3000000, 5000000 },
                ActivationCosts = new[] { 0, 55000, 100000, 135000, 180000, 240000, 300000, 500000 },
                MaitenceCosts = new[] { 0, 11000, 20000, 27000, 36000, 48000, 60000, 100000 },
                MaxLevel = 7
            };
            GuildSkillTemplate.Templates[(int)GuildSkillId.Expirience] = new GuildSkillTemplate()
            {
                BonusValuses = new[] { 0, 5, 6, 7, 10 },
                LearnCosts = new[] { 0, 500000, 1000000, 2000000, 3000000 },
                ActivationCosts = new[] { 0, 50000, 100000, 200000, 300000 },
                MaitenceCosts = new[] { 0, 10000, 20000, 40000, 60000 },
                MaxLevel = 4
            };
        }


        #region скилы

        #endregion
        #endregion
        #region Конфигурируемые константы

        [NotVariable]
        /// <summary>
        /// Делим Средний уровень гильд вэйв на это число и получаем колво вейв койнов.
        /// </summary>
        public static int WaveCoinsDivider = 3;
        [NotVariable]
        /// <summary>
        /// Сколько очков дает захваченая точка на войне за 1 тик
        /// </summary>
        public static short FactionWarPointsPerTicForCapturedPoints = 5;
        [NotVariable]
        /// <summary>
        /// Сколько времени длится захват точки
        /// </summary>
        public static int DefaultCaptureTime = 60 * 1000;
        [NotVariable]
        /// <summary>
        /// Сколько времени должен персонаж кастовать захват точки до применения
        /// </summary>
        public static int DefaultTimeToStartCapture = 10 * 1000;
        [NotVariable]
        /// <summary>
        /// Время между получениями очков за захваченую точку
        /// </summary>
        public static int DefaultTimeGainExpReward = 10 * 1000;
        [NotVariable]
        public static float NearFriendDamageBonus = 0.15f;
        [NotVariable]
        public static float NearFriendDeffenceBonus = 0.10f;
        [NotVariable]
        public static float NearFriendSpeedBonus = 0.05f;
        [NotVariable]
        public static float FriendEmpowerDamageBonus = 0.25f;
        [NotVariable]
        public static float SoulmateSongStatBonusPrc = 0.5f;
        [NotVariable]
        public static float SoulmateSongDamageBonusPrc = 0.3f;
        [NotVariable]
        public static float SoulmateSongSpeedBonusPrc = 0.15f;
        [NotVariable]
        public static float SoulmateSongDeffenceBonusPrc = 0.3f;
        [NotVariable]

        /// <summary>
        /// Кол-во очков распределения при начале игры.
        /// </summary>
        public static int FreestatPointsOnStart = 15;
        [NotVariable]
        /// <summary>
        /// Сколько дается опыта дружбы за убийство монстра
        /// </summary>
        public static float SoulmatExpFromMonstrKilled = 1.8f;
        [NotVariable]
        /// /// <summary>
        /// Сколько дается опыта дружбы за любое получение опыта в игре(рыбалка копка крафт и т.д)
        /// </summary>
        public static float SoulmatExpFromAnyExp = 1.5f;
        [NotVariable]
        /// <summary>
        /// Опыт дружбы за нахождение рядом
        /// </summary>
        public static float SoulmateExpGainPerMinuteNearFriend = 1.1f;
        [NotVariable]
        /// <summary>
        /// Сколько процентов очков Войны отдавать пати мемберам. 0.4 = 40%
        /// </summary>
        public static float BattegroundGroupDisctributePrc = 0.4f;
        [NotVariable]

        /// <summary>
        /// Делим хонор пойнты на это число и получаем колво хонор койнов.
        /// </summary>
        public static float HonorCoinsDivider = 7;
        [NotVariable]
        /// <summary>
        /// Время между получениями опыта петом в секундах.
        /// </summary>
        public static uint TimeBetweenPetExpGainSecs = 150;
        [NotVariable]
        /// <summary>
        /// Время между приемами еды питомцем в секундах.
        /// </summary>
        public static uint TimeBetweenPetEatingsSecs = 80;
        [NotVariable]
        /// <summary>
        /// Количество опыта в процентах теряемого при смерти. Если у вас сейчас 55%опыта то вы потеряете 5.5% если 99% - 9.9%
        /// </summary>
        public static int ExpirienceLooseOnDeathPrc = 10;
        [NotVariable]
        /// <summary>
        /// Начальное значение характеристик персонажа (Сила, ловкость...удача) при создании
        /// </summary>
        public static int StatOnCreation = 1;
        [NotVariable]
        /// <summary>
        /// Количество ед. ловкости для снижения времени между 2мя ударами на 1%
        /// </summary>
        public static float OnePrcAtackTimeReducePerAgilityPoints = 0.0001f;
        [NotVariable]
        /// <summary>
        /// Кол-во ед. удачи для увеличения шанса критического удара на 1%
        /// </summary>
        public static float OnePrcCritPerLuck = 0.000007f;
        [NotVariable]
        /// <summary>
        /// Кол-во ед. удачи для увеличения шанса выпадения предмета на 1%
        /// </summary>
        public static float OnePrcDropChancePerLuck = 0.000015f;
        [NotVariable]
        /// <summary>
        /// Кол-во ед. удачи для увеличения кол-ва выпадаемого с монстра золота на 1%
        /// </summary>
        public static float OnePrcGoldAmountPerLuckPoints = 0.000015f;
        [NotVariable]
        /// <summary>
        /// Кол-во ед. Энергии для увеличения магической защиты на 1 ед.
        /// </summary>
        public static float OneMagicDefencePerSpiritPoints = 0.05f;
        [NotVariable]
        /// <summary>
        /// Кол-во ед. маны за каждое очко Энергии
        /// </summary>
        public static float ManaPointsPerOneSpirit = 0.7f;
        [NotVariable]
        /// <summary>
        /// Кол-во ед. Интелекта для увеличения маг атаки на 1ед.
        /// </summary>
        public static float DamagePerIntelect = 0.0003f;
        [NotVariable]
        /// <summary>
        /// Кол-во ед. Ловкости для увеличения атаки атаки на 1ед.
        /// </summary>
        public static float DamagePerAgility = 0.0003f;
        [NotVariable]
        /// <summary>
        /// Кол-во ед. Силы для увеличения атаки атаки на 1ед.
        /// </summary>
        public static float DamagePerStrength = 0.0003f;
        [NotVariable]
        /// <summary>
        /// Кол-во ед. жизни за каждое очко Силы
        /// </summary>
        public static float HealthPointsPerStrength = 0.25f;
        [NotVariable]
        /// <summary>
        /// Кол-во ед. жизни за каждое очко Выносливости
        /// </summary>
        public static float HealthPointsPerStamina = 2.1f;
        [NotVariable]
        /// <summary>
        /// Кол-во ед. защиты за каждое очко Ловкости
        /// </summary>
        public static float DefencePointsPerAgility = 0.00025f;
        [NotVariable]
        /// <summary>
        /// увеличение шанса уворота за 1 ловкость
        /// </summary>
        public static float DodgePerAgility = 0.05f;
        [NotVariable]
        /// <summary>
        ///увеличения скорости бега за 1 ловкость
        /// </summary>
        public static float SpeedPerAgility = 0.0001f;
        [NotVariable]
        /// <summary>
        /// Усиление крит урона в % за 1ну ед. Силы
        /// </summary>
        public static float CritDamageBonusPerStrength = 0.0005f;
        [NotVariable]
        /// <summary>
        /// Усиление крит урона в % за 1ну ед. Интелекта
        /// </summary>
        public static float CritDamageBonusPerIntellect = 0.0001f;
        [NotVariable]
        /// <summary>
        /// Увеличение степени усиления характеристик предмета (Урона) при заточке.
        /// </summary>
        public static float EnchantPowValue = 0.25f;
        [NotVariable]
        /// <summary>
        /// Увеличение степени усиления характеристик предмета (Побочне зарактеристики + ЖК) при заточке.
        /// </summary>
        public static float EnchantPowValueForNotDamageStats = 0.065f;
        [NotVariable]
        /// <summary>
        /// Шанс выловиить рыбу по умолчанию
        /// </summary>
        public static byte StandartFishingChance = 75;
        [NotVariable]
        /// <summary>
        /// Шанс на то что уровень рыбалки повысится. 100 000 - 100%
        /// </summary>
        public static int StandartFishingLevelUpChance = 60000;
        /// <summary>
        /// Количество очков энергии для увелицчения регенирации маны на 1 в секунду
        /// </summary>
        [NotVariable]
        public static float ManaRegenPerSpirit = 0.01f;

        #endregion
        #region Формулы
        #region Health/Mana
        /// <summary>
        /// Расчет базового кол-ва жизни у персонажа
        /// </summary>
        /// <param name="level">Уровень</param>
        /// <param name="cl">Класс</param>
        /// <returns>базовое кол-во жизни</returns>
        public static int GetBaseHealth(int level, ClassId cl)
        {
            switch (cl)
            {
                case ClassId.OHS:
                    return 250 + level * 18;
                case ClassId.THS:
                    return 230 + level * 18;
                case ClassId.Spear:
                    return 220 + level * 18;
                case ClassId.Crossbow:
                    return 180 + level * 15;
                case ClassId.Bow:
                    return 180 + level * 15;
                case ClassId.Balista:
                    return 150 + level * 15;
                case ClassId.AtackMage:
                    return 110 + level * 12;
                case ClassId.SupportMage:
                    return 110 + level * 12;
                case ClassId.HealMage:
                    return 110 + level * 12;
                case ClassId.NoClass:
                    return 100 + level * 12;
            }
            return 100;
        }
        /// <summary>
        /// Расчет бонуса жизни
        /// </summary>
        /// <param name="level"></param>
        /// <param name="asda2Strength"></param>
        /// <param name="asda2Stamina"></param>
        /// <param name="cl"></param>
        /// <returns></returns>
        public static float CalculateHealthBonus(int level, int asda2Strength, int asda2Stamina, ClassId cl)
        {
            switch (cl)
            {
                case ClassId.OHS:
                    return asda2Stamina * HealthPointsPerStamina * 2.2f + asda2Strength * HealthPointsPerStrength * 1.25f;
                case ClassId.THS:
                    return asda2Stamina * HealthPointsPerStamina * 2.1f + asda2Strength * HealthPointsPerStrength * 1.05f;
                case ClassId.Spear:
                    return asda2Stamina * HealthPointsPerStamina * 2.1f + asda2Strength * HealthPointsPerStrength * 1f;
                case ClassId.Crossbow:
                    return asda2Stamina * HealthPointsPerStamina * 1.9f + asda2Strength * HealthPointsPerStrength * 0.7f;
                case ClassId.Bow:
                    return asda2Stamina * HealthPointsPerStamina * 1.9f + asda2Strength * HealthPointsPerStrength * 0.7f;
                case ClassId.Balista:
                    return asda2Stamina * HealthPointsPerStamina * 1.9f + asda2Strength * HealthPointsPerStrength * 0.7f;
                case ClassId.AtackMage:
                    return asda2Stamina * HealthPointsPerStamina * 1.65f;
                case ClassId.SupportMage:
                    return asda2Stamina * HealthPointsPerStamina * 1.65f;
                case ClassId.HealMage:
                    return asda2Stamina * HealthPointsPerStamina * 1.65f;
                case ClassId.NoClass:
                    return asda2Stamina * HealthPointsPerStamina + asda2Strength * HealthPointsPerStrength;
            }
            return 50;
        }
        /// <summary>
        /// Расчет базового кол-ва маны у персонажа
        /// </summary>
        /// <param name="level">Уровень</param>
        /// <param name="cl">Класс</param>
        /// <returns>базовое кол-во маны</returns>
        public static int GetBaseMana(int level, ClassId cl)
        {
            switch (cl)
            {
                case ClassId.OHS:
                    return 100 + level * 7;
                case ClassId.THS:
                    return 100 + level * 7;
                case ClassId.Spear:
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
                case ClassId.NoClass:
                    return 100 + level * 5;
            }
            return 50;
        }
        /// <summary>
        /// Расчет увеличения кол-ва маны
        /// </summary>
        /// <param name="level">Уровень</param>
        /// <param name="cl">класс</param>
        /// <param name="asda2Spirit">Энергия</param>
        /// <returns></returns>
        public static int CalculateManaBonus(int level, ClassId cl, int asda2Spirit)
        {
            switch (cl)
            {
                case ClassId.OHS:
                    return (int)(asda2Spirit * ManaPointsPerOneSpirit);
                case ClassId.THS:
                    return (int)(asda2Spirit * ManaPointsPerOneSpirit);
                case ClassId.Spear:
                    return (int)(asda2Spirit * ManaPointsPerOneSpirit);
                case ClassId.Crossbow:
                    return (int)(asda2Spirit * ManaPointsPerOneSpirit);
                case ClassId.Bow:
                    return (int)(asda2Spirit * ManaPointsPerOneSpirit);
                case ClassId.Balista:
                    return (int)(asda2Spirit * ManaPointsPerOneSpirit);
                case ClassId.AtackMage:
                    return (int)(asda2Spirit * ManaPointsPerOneSpirit * 2);
                case ClassId.SupportMage:
                    return (int)(asda2Spirit * ManaPointsPerOneSpirit * 2);
                case ClassId.HealMage:
                    return (int)(asda2Spirit * ManaPointsPerOneSpirit * 2);
                case ClassId.NoClass:
                    return (int)(asda2Spirit * ManaPointsPerOneSpirit);
            }
            return 50;

        }
        #endregion
        #region Physical Attack\Magical Attack\Critical damage\Critical chance\Time beetwen two physical attacks\SkillCooldownTime
        /// <summary>
        /// Рачет шанса критического урона для физ. атаки
        /// </summary>
        /// <param name="id">Класс</param>
        /// <param name="level">Уровень</param>
        /// <param name="agility">Кол-во Ловкости</param>
        /// <param name="luck">Кол-во удачи</param>
        /// <returns>Шанс физ. крит удара</returns>
        public static float CalculatePsysicCritChance(ClassId id, int level, int luck)
        {
            float r;
            switch (id)
            {
                case ClassId.OHS:
                    r = luck * OnePrcCritPerLuck * 0.5f;
                    break;
                case ClassId.THS:
                    r = luck * OnePrcCritPerLuck * 0.9f; break;
                case ClassId.Spear:
                    r = luck * OnePrcCritPerLuck * 1.2f; break;
                case ClassId.Crossbow:
                    r = luck * OnePrcCritPerLuck * 1.2f; break;
                case ClassId.Bow:
                    r = luck * OnePrcCritPerLuck * 0.9f; break;
                case ClassId.Balista:
                    r = luck * OnePrcCritPerLuck * 0.6f; break;
                case ClassId.AtackMage:
                    r = luck * OnePrcCritPerLuck * 0.3f;
                    return r > 35 ? 35 : r;
                case ClassId.SupportMage:
                    r = luck * OnePrcCritPerLuck * 0.5f;
                    return r > 35 ? 35 : r;
                case ClassId.HealMage:
                    r = luck * OnePrcCritPerLuck * 0.5f;
                    return r > 35 ? 35 : r;
                default:
                    r = luck * OnePrcCritPerLuck * 1f; break;
            }
            return r > 50 ? 50 : r;
        }
        /// <summary>
        /// Расчет увеличения маг атаки
        /// </summary>
        /// <param name="level"></param>
        /// <param name="cl"></param>
        /// <param name="asda2Intellect"></param>
        /// <returns></returns>
        public static float CalculateMagicDamageBonus(int level, ClassId cl, int asda2Intellect)
        {
            switch (cl)
            {
                case ClassId.AtackMage:
                    return asda2Intellect * DamagePerIntelect * 0.7f;
                case ClassId.HealMage:
                    return asda2Intellect * DamagePerIntelect * 0.7f;
                case ClassId.SupportMage:
                    return asda2Intellect * DamagePerIntelect * 0.7f;
                default:
                    return 0;
            }
        }
        /// <summary>
        /// Расчет бонуса физ атаки
        /// </summary>
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
                    return (asda2Agility * DamagePerAgility + strength * DamagePerStrength) * 1.2f;
                case ClassId.THS:
                    return (asda2Agility * DamagePerAgility + strength * DamagePerStrength) * 1.2f;
                case ClassId.Spear:
                    return (asda2Agility * DamagePerAgility + strength * DamagePerStrength) * 1.2f;
                case ClassId.Crossbow:
                    return (asda2Agility * DamagePerAgility + strength * DamagePerStrength) * 1.2f;
                case ClassId.Bow:
                    return (asda2Agility * DamagePerAgility + strength * DamagePerStrength) * 1.2f;
                case ClassId.Balista:
                    return (asda2Agility * DamagePerAgility + strength * DamagePerStrength) * 1.2f;
                default:
                    return (asda2Agility * DamagePerAgility + strength * DamagePerStrength) * 1.2f;
            }
        }
        /// <summary>
        /// Расчет уменьшения времени между 2мя ударами
        /// </summary>
        /// <param name="level"></param>
        /// <param name="cl"></param>
        /// <param name="asda2Agility"></param>
        /// <returns></returns>
        public static float CalculateAtackTimeReduce(int level, ClassId cl, int asda2Agility)
        {
            return asda2Agility * OnePrcAtackTimeReducePerAgilityPoints * 0.01f;
        }

        /// <summary>
        /// Расчет усиления крит урона в %
        /// </summary>
        /// <param name="level"></param>
        /// <param name="cl"></param>
        /// <param name="asda2Agility"></param>
        /// <param name="luck"></param>
        /// <param name="intelect"></param>
        /// <param name="strength"></param>
        /// <returns></returns>
        public static int CalculateCriticalDamageBonus(int level, ClassId cl, int asda2Agility, int luck, int intelect, int strength)
        {
            switch (cl)
            {
                case ClassId.OHS:
                    return (int)(CritDamageBonusPerStrength * strength);
                case ClassId.THS:
                    return (int)(CritDamageBonusPerStrength * strength);
                case ClassId.Spear:
                    return (int)(CritDamageBonusPerStrength * strength);
                case ClassId.Crossbow:
                    return (int)(CritDamageBonusPerStrength * strength);
                case ClassId.Bow:
                    return (int)(CritDamageBonusPerStrength * strength);
                case ClassId.Balista:
                    return (int)(CritDamageBonusPerStrength * strength);
                case ClassId.AtackMage:
                    return (int)(intelect * CritDamageBonusPerIntellect);
                case ClassId.SupportMage:
                    return (int)(intelect * CritDamageBonusPerIntellect);
                case ClassId.HealMage:
                    return (int)(intelect * CritDamageBonusPerIntellect);
                case ClassId.NoClass:
                    return (int)(CritDamageBonusPerStrength * strength);
            }
            return 0;
        }

        #endregion
        #region Physycal deffence\Magical deffence\Dodge chance\Moving speed
        /// <summary>
        /// Расчет увеличения маг. защиты
        /// </summary>
        /// <param name="level">Уровень</param>
        /// <param name="cl">Класс</param>
        /// <param name="asda2Spirit">Энергия</param>
        /// <returns>бонус маг защиты</returns>
        public static float CalculateMagicDefencePointsBonus(int level, ClassId cl, int asda2Spirit)
        {
            switch (cl)
            {
                case ClassId.OHS:
                    return asda2Spirit * OneMagicDefencePerSpiritPoints * 1.5f;
                case ClassId.THS:
                    return asda2Spirit * OneMagicDefencePerSpiritPoints;
                case ClassId.Spear:
                    return asda2Spirit * OneMagicDefencePerSpiritPoints;
                case ClassId.Crossbow:
                    return asda2Spirit * OneMagicDefencePerSpiritPoints;
                case ClassId.Bow:
                    return asda2Spirit * OneMagicDefencePerSpiritPoints;
                case ClassId.Balista:
                    return asda2Spirit * OneMagicDefencePerSpiritPoints;
                case ClassId.AtackMage:
                    return asda2Spirit * OneMagicDefencePerSpiritPoints;
                case ClassId.SupportMage:
                    return asda2Spirit * OneMagicDefencePerSpiritPoints * 1.3f;
                case ClassId.HealMage:
                    return asda2Spirit * OneMagicDefencePerSpiritPoints * 1.3f;
                case ClassId.NoClass:
                    return asda2Spirit * OneMagicDefencePerSpiritPoints;
            }
            return 50;

        }
        /// <summary>
        /// Расчет бонуса защиты
        /// </summary>
        /// <param name="level"></param>
        /// <param name="cl"></param>
        /// <param name="asda2Agility"></param>
        /// <returns></returns>
        public static float ClaculateDefenceBonus(int level, ClassId cl, int asda2Agility)
        {
            return 0;
        }
        /// <summary>
        /// Расчет шанса уворота
        /// </summary>
        /// <param name="level"></param>
        /// <param name="cl"></param>
        /// <param name="asda2Agility"></param>
        /// <returns></returns>
        public static float CalcDodgeChanceBonus(int level, ClassId cl, int asda2Agility)
        {
            switch (cl)
            {
                case ClassId.OHS:
                    return asda2Agility * DodgePerAgility * 0.7f;
                case ClassId.THS:
                    return asda2Agility * DodgePerAgility;
                case ClassId.Spear:
                    return asda2Agility * DodgePerAgility * 1.2f;
                case ClassId.Crossbow:
                    return asda2Agility * DodgePerAgility * 1.5f;
                case ClassId.Bow:
                    return asda2Agility * DodgePerAgility * 1.5f;
                case ClassId.Balista:
                    return asda2Agility * DodgePerAgility * 1.5f;
                case ClassId.AtackMage:
                    return asda2Agility * DodgePerAgility;
                case ClassId.SupportMage:
                    return asda2Agility * DodgePerAgility;
                case ClassId.HealMage:
                    return asda2Agility * DodgePerAgility;
                case ClassId.NoClass:
                    return asda2Agility * DodgePerAgility;
            }
            return 0;
        }
        /// <summary>
        /// Расчет бонуса скорости %
        /// </summary>
        /// <param name="level"></param>
        /// <param name="cl"></param>
        /// <param name="asda2Agility"></param>
        /// <returns></returns>
        public static float CalcSpeedBonus(int level, ClassId cl, int asda2Agility)
        {
            switch (cl)
            {
                case ClassId.OHS:
                    return asda2Agility * SpeedPerAgility * 0.7f;
                case ClassId.THS:
                    return asda2Agility * SpeedPerAgility * 0.7f;
                case ClassId.Spear:
                    return asda2Agility * SpeedPerAgility * 0.7f;
                case ClassId.Crossbow:
                    return asda2Agility * SpeedPerAgility * 0.7f;
                case ClassId.Bow:
                    return asda2Agility * SpeedPerAgility * 0.7f;
                case ClassId.Balista:
                    return asda2Agility * SpeedPerAgility * 0.7f;
                case ClassId.AtackMage:
                    return asda2Agility * SpeedPerAgility * 0.7f;
                case ClassId.SupportMage:
                    return asda2Agility * SpeedPerAgility * 0.7f;
                case ClassId.HealMage:
                    return asda2Agility * SpeedPerAgility * 0.7f;
                case ClassId.NoClass:
                    return asda2Agility * SpeedPerAgility * 0.7f;
            }
            return 0;
        }
        #endregion
        #region Item drop chance\Gold drop amount
        /// <summary>
        /// Расчет увеличения шанса выпадения предметов
        /// </summary>
        /// <param name="level">Уровень</param>
        /// <param name="cl">Класс</param>
        /// <param name="asda2Luck">Удача</param>
        /// <returns>Увелицение шанса %</returns>
        public static float CalculateDropChanceBoost(int asda2Luck)
        {
            return asda2Luck * OnePrcDropChancePerLuck;
        }
        /// <summary>
        /// Расчет увеличения кол-ва выпадаемого золота
        /// </summary>
        /// <param name="level">Уровень</param>
        /// <param name="cl">Класс</param>
        /// <param name="asda2Luck">Удача</param>
        /// <returns>Увелицение шанса</returns>
        public static float CalculateGoldAmountDropBoost(int level, ClassId cl, int asda2Luck)
        {
            return asda2Luck * OnePrcGoldAmountPerLuckPoints;
        }
        #endregion
        #region Item upgrade

        /// <summary>
        /// Расчет шанса заточки предмета
        /// </summary>
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
        public static ItemUpgradeResult CalculateItemUpgradeResult(Asda2ItemQuality stoneQuality, Asda2ItemQuality itemQuality, byte enchant, byte requiredLevel, int ownerLuck, int groupLuck, int nearblyCharactersLuck, bool useProtect, int useChanceBoost, bool noEnchantLose)
        {
            var seed = 170f;
            switch (itemQuality)
            {
                case Asda2ItemQuality.White:
                    seed -= 10;
                    break;
                case Asda2ItemQuality.Yello:
                    seed -= 20;
                    break;
                case Asda2ItemQuality.Purple:
                    seed -= 25;
                    break;
                case Asda2ItemQuality.Green:
                    seed -= 35;
                    break;
                case Asda2ItemQuality.Orange:
                    seed -= 50;
                    break;
            }
            switch (stoneQuality)
            {
                case Asda2ItemQuality.White:
                    seed += 0;
                    break;
                case Asda2ItemQuality.Yello:
                    seed += 10;
                    break;
                case Asda2ItemQuality.Purple:
                    seed += 30;
                    break;
                case Asda2ItemQuality.Green:
                    seed += 10000;
                    break;
                case Asda2ItemQuality.Orange:
                    seed += 10000;
                    break;
            }
            var boostFromOwnerLuck = 0;// ownerLuck* ItemEnchantChancePerOneOwnerLuck;
            var boostFormGroupLuck = 0;//groupLuck * ItemEnchantChancePerOnePartyLuck;
            var boostFromNearbyCharactersLuck = 0;// nearblyCharactersLuck * ItemEnchantChancePerOneNearbluCharacerLuck;
            seed = (float)(seed / Math.Pow(enchant + 0.1f, 0.75f));
            if (enchant < 10)
                seed = seed * 3.3f + enchant * 2.9f;
            if (enchant < 15)
                seed = seed * 1.3f;
            if (enchant > 15)
                seed = seed * 0.4f;
            var chance = (seed + seed * (boostFromOwnerLuck + boostFormGroupLuck + boostFromNearbyCharactersLuck - Math.Pow(requiredLevel, 0.85f) / 100)) * (1 + (float)useChanceBoost / 100);
            if (chance > 100)
                chance = 100;
            if (chance < 0)
                chance = 0;
            if (enchant >= 20)
                    chance = 0;

            var random = Utility.Random(0, 100f);
            if (random > chance)
            {
                //enchantFailed
                if (random < chance + (100 - chance) / 6)
                {
                    //break item
                    if (useProtect)
                    {
                        if (enchant > 10)
                        {
                            if (noEnchantLose)
                                return new ItemUpgradeResult(ItemUpgradeResultStatus.Fail, boostFormGroupLuck, boostFromNearbyCharactersLuck, boostFromOwnerLuck, chance);
                            return new ItemUpgradeResult(ItemUpgradeResultStatus.ReduceOneLevel, boostFormGroupLuck, boostFromNearbyCharactersLuck, boostFromOwnerLuck, chance);
                        }
                        return new ItemUpgradeResult(ItemUpgradeResultStatus.Fail, boostFormGroupLuck, boostFromNearbyCharactersLuck, boostFromOwnerLuck, chance);
                    }
                    if (enchant > 7)
                        return new ItemUpgradeResult(ItemUpgradeResultStatus.BreakItem, boostFormGroupLuck, boostFromNearbyCharactersLuck, boostFromOwnerLuck, chance);
                    if (enchant > 4)
                        return new ItemUpgradeResult(ItemUpgradeResultStatus.ReduceOneLevel, boostFormGroupLuck, boostFromNearbyCharactersLuck, boostFromOwnerLuck, chance);
                    return new ItemUpgradeResult(ItemUpgradeResultStatus.Fail, boostFormGroupLuck, boostFromNearbyCharactersLuck, boostFromOwnerLuck, chance);
                }
                if (random < chance + (100 - chance) / 4)
                {
                    if (useProtect)
                    {
                        if (enchant > 10)
                        {
                            if (noEnchantLose)
                                return new ItemUpgradeResult(ItemUpgradeResultStatus.Fail, boostFormGroupLuck, boostFromNearbyCharactersLuck, boostFromOwnerLuck, chance);
                            return new ItemUpgradeResult(ItemUpgradeResultStatus.ReduceOneLevel, boostFormGroupLuck, boostFromNearbyCharactersLuck, boostFromOwnerLuck, chance);
                        }
                        return new ItemUpgradeResult(ItemUpgradeResultStatus.Fail, boostFormGroupLuck, boostFromNearbyCharactersLuck, boostFromOwnerLuck, chance);
                    }
                    if (enchant > 7)
                        return new ItemUpgradeResult(ItemUpgradeResultStatus.ReduceLevelToZero, boostFormGroupLuck, boostFromNearbyCharactersLuck, boostFromOwnerLuck, chance);
                    return new ItemUpgradeResult(ItemUpgradeResultStatus.Fail, boostFormGroupLuck, boostFromNearbyCharactersLuck, boostFromOwnerLuck, chance);
                    //downgrade to 0
                }
                if (random < chance + (100 - chance) / 2)
                {
                    //downgrade one point
                    if (useProtect)
                    {
                        return new ItemUpgradeResult(ItemUpgradeResultStatus.Fail, boostFormGroupLuck, boostFromNearbyCharactersLuck, boostFromOwnerLuck, chance);
                    }
                    if (enchant > 7)
                        return new ItemUpgradeResult(ItemUpgradeResultStatus.ReduceOneLevel, boostFormGroupLuck, boostFromNearbyCharactersLuck, boostFromOwnerLuck, chance);
                    return new ItemUpgradeResult(ItemUpgradeResultStatus.Fail, boostFormGroupLuck, boostFromNearbyCharactersLuck, boostFromOwnerLuck, chance);
                }
                return new ItemUpgradeResult(ItemUpgradeResultStatus.Fail, boostFormGroupLuck, boostFromNearbyCharactersLuck, boostFromOwnerLuck, chance);
            }
            return new ItemUpgradeResult(ItemUpgradeResultStatus.Success, boostFormGroupLuck, boostFromNearbyCharactersLuck, boostFromOwnerLuck, chance);
        } /// <summary>
        /// Рачет усиления характеристик предмета в зависимости от заточки(Усиливаются Урон)
        /// </summary>
        /// <param name="enchant"></param>
        /// <returns></returns>
        public static float CalculateEnchantMultiplier(byte enchant)
        {
            var val = (float)(enchant == 0 ? 1 : Math.Pow(enchant, EnchantPowValue));
           
                switch (enchant)
            {
                default:
                    break;
                case 16:
                    val = val * 1.2f;
                break;
                case 17:
                    val = val * 1.3f;
                break;
                case 18:
                    val = val * 1.4f;
                break;
                case 19:
                    val = val * 1.5f;
                break;
                case 20:
                    val = val * 2f;
                break;
           

                
            }
               
            return val;
        }
        /// <summary>
        /// Рачет усиления характеристик предмета в зависимости от заточки(Усиливаются Эфекти от ЖК и параметры)
        /// </summary>
        /// <param name="enchant"></param>
        /// <returns></returns>
        public static float CalculateEnchantMultiplierNotDamageItemStats(byte enchant)
        {
            return (float)(enchant == 0 ? 1 : Math.Pow(enchant, EnchantPowValueForNotDamageStats));
        }
        #endregion
        #endregion

        /// <summary>
        /// Раритетность шмотки которую мы крафтим
        /// </summary>
        /// <returns></returns>
        public static byte GetCraftedRarity()
        {
            var rnd = Utility.Random(0, 100000);
            if (rnd < 15000)
                return 1;
            if (rnd < 50000)
                return 1;
            if (rnd < 85000)
                return 2;
            if (rnd < 98000)
                return 3;
            if (rnd < 99900)
                return 4;
            return 5;
        }
        /// <summary>
        /// Расчет опыта за крафт (Прокачка крафта)
        /// </summary>
        /// <param name="diffLvl"></param>
        /// <param name="currentCraftLevel"></param>
        /// <returns></returns>
        public static float CalcCraftingExp(int diffLvl, byte currentCraftLevel)
        {
            var exp = 0f;
            if (diffLvl == 0)
                exp = 3f;
            else if (diffLvl == 1)
                exp = 2.5f;
            else if (diffLvl == 2)
                exp = 2.25f;
            else if (diffLvl == 3)
                exp = 2.1f;
            else if (diffLvl == 4)
                exp = 2.05f;
            else if (diffLvl == 5)
                exp = 1.5f;
            else if (diffLvl == 6)
                exp = 1.3f;
            else if (diffLvl == 7)
                exp = 1.1f;
            else if (diffLvl == 8)
                exp = 0.5f;
            else if (diffLvl == 9)
                exp = 0.1f;

            exp = (float)(exp / Math.Pow(currentCraftLevel, 2f));
            return exp;
        }

        /// <summary>
        /// Расчет опыта за крафт (Прокачка персонажа)
        /// </summary>
        /// <param name="diffLvl"></param>
        /// <param name="currentCraftLevel"></param>
        /// <param name="currentCharacterLevel"></param>
        /// <returns></returns>
        public static int CalcExpForCrafting(int diffLvl, byte currentCraftLevel, byte currentCharacterLevel)
        {
            if (diffLvl == 0)
                diffLvl = 1;
            return (int)((float)XpGenerator.GetBaseExpForLevel(currentCharacterLevel) * currentCraftLevel / diffLvl);
        }

        /// <summary>
        /// Расчитывает раритетность полученого в результате синтеза пета.
        /// </summary>
        /// <param name="rarity">Раритетность первого пета</param>
        /// <param name="rarity2">Раритетность 2го пета</param>
        /// <returns>белый желтый фиолетовый или зеленый 0 1 2 3</returns>
        public static int CalcResultSyntesPetRarity(int rarity, int rarity2)
        {
            var startRatiry = ((float)rarity + rarity2) / 2;
            var rnd = Utility.Random(0.5f, 3f);
            var r = startRatiry + rnd;
            if (r < startRatiry && startRatiry > 1)
                return (int)(startRatiry - 1);
            if (startRatiry < 0)
                return 0;
            if (startRatiry > 3)
                return 3;
            return (int)r;
        }
        /// <summary>
        /// Расчитывает раритетность полученого в результате эволюции пета.
        /// </summary>
        /// <param name="rarity">Раритетность первого пета</param>
        /// <param name="rarity2">Раритетность 2го пета</param>
        /// <returns>белый желтый фиолетовый или зеленый 0 1 2 3</returns>
        public static int CalcResultEvolutionPetRarity(int rarity, int rarity2)
        {
            var startRatiry = ((float)rarity + rarity2) / 2;
            var rnd = Utility.Random(0.9f, 2.03f);
            var r = startRatiry + rnd;
            if (r < startRatiry && startRatiry > 1)
                return (int)(startRatiry - 1);
            if (startRatiry < 0)
                return 0;
            if (startRatiry > 3)
                return 3;
            return (int)r;
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
            return (StandartFishingLevelUpChance / Math.Pow(curFishingLevel, 0.4)) < Utility.Random(0, 100000);
        }

        /// <summary>
        /// Расчет удачно ли выловилась рыба
        /// </summary>
        /// <param name="fishingLevel">уровень рыбалки рыбака</param>
        /// <param name="requiredFishingLevel">необходимый уровень рыбалки на месте</param>
        /// <param name="asda2Luck"></param>
        /// <returns></returns>
        public static bool CalcFishingSuccess(int fishingLevel, int requiredFishingLevel, int asda2Luck)
        {
            var boost = (fishingLevel - requiredFishingLevel) / 3.5f;
            return ((StandartFishingChance + boost) * Math.Pow(asda2Luck + 1, 0.02f) > Utility.Random(0, 100));
        }
        [NotVariable]
        /// <summary>
        /// Шанс уменьшить прочность удочки на 1ед.
        /// </summary>
        public static int DecreaceRodDurabilityChance = 40000;

        /// <summary>
        /// Уменьшится ли прочность удочки на 1ед.
        /// </summary>
        /// <returns></returns>
        public static bool DecraseRodDurability()
        {
            return DecreaceRodDurabilityChance > Utility.Random(0, 100000);
        }

        /// <summary>
        /// Опыт получаемый от рыбалки.
        /// </summary>
        /// <param name="level">уровень персонажа</param>
        /// <param name="fishingLevel">уромень рыбалки</param>
        /// <param name="quality">качетво выловленой рыбы</param>
        /// <param name="requiredFishingLevel">требуемый уровень локации</param>
        /// <param name="fishSize">размер рыбы</param>
        /// <returns></returns>
        public static int CalcExpForFishing(int level, int fishingLevel, Asda2ItemQuality quality, int requiredFishingLevel, short fishSize)
        {
            return (int)((float)XpGenerator.GetBaseExpForLevel(level) / Math.Pow(fishingLevel - requiredFishingLevel, 0.2) * fishSize / 50);
        }

        /// <summary>
        /// Расчет опыта за копку
        /// </summary>
        /// <param name="level">Уровень персонажа</param>
        /// <param name="minLocationLevel">Минимальный уровень копки в локации.</param>
        /// <returns></returns>
        public static int CalcDiggingExp(int level, int minLocationLevel)
        {
            return (int)((float)XpGenerator.GetBaseExpForLevel(level) * Math.Pow(minLocationLevel, 0.2f) / 4);
        }
        [NotVariable]
        public static int AlpiaBaseHonorPoints = 1;
        [NotVariable]
        public static int SilarisBaseHonorPoints = 1;
        [NotVariable]
        public static int FlamioBaseHonorPoints = 1;
        [NotVariable]
        public static int AquatonBaseHonorPoints = 1;
        public static int CalcHonorPoints(int level, short battlegroundActPoints, bool isWiner, int battlegroundDeathes, int battlegroundKills, bool isMvp, Asda2BattlegroundTown town)
        {
            switch (town)
            {
                case Asda2BattlegroundTown.Alpia:
                    return (int)(battlegroundActPoints * Math.Pow(30 - level, 0.2f) *
                                  Math.Pow(
                                      (battlegroundKills <= battlegroundDeathes ? 1 : battlegroundKills - battlegroundDeathes),
                                      0.15f) * (isMvp ? 1.5f : 1) * (isWiner ? 2 : 1) * AlpiaBaseHonorPoints);
                case Asda2BattlegroundTown.Silaris:
                    return (int)(battlegroundActPoints * Math.Pow(50 - level, 0.2f) *
                                  Math.Pow(
                                      (battlegroundKills <= battlegroundDeathes ? 1 : battlegroundKills - battlegroundDeathes),
                                      0.15f) * (isMvp ? 1.5f : 1) * (isWiner ? 2 : 1) * SilarisBaseHonorPoints);
                case Asda2BattlegroundTown.Flamio:
                    return (int)(battlegroundActPoints * Math.Pow(101 - level, 0.2f) *
                                  Math.Pow(
                                      (battlegroundKills <= battlegroundDeathes ? 1 : battlegroundKills - battlegroundDeathes),
                                      0.15f) * (isMvp ? 1.5f : 1) * (isWiner ? 2 : 1) * FlamioBaseHonorPoints);
                case Asda2BattlegroundTown.Aquaton:
                    return (int)(battlegroundActPoints * Math.Pow(101 - level, 0.2f) *
                                  Math.Pow(
                                      (battlegroundKills <= battlegroundDeathes ? 1 : battlegroundKills - battlegroundDeathes),
                                      0.15f) * (isMvp ? 1.5f : 1) * (isWiner ? 2 : 1) * FlamioBaseHonorPoints);
            }
            return 0;
        }
        public static int[] HonorRankPoints = new int[21];

        static CharacterFormulas()
        {
            for (int i = 0; i < 21; i++)
            {
                HonorRankPoints[i] = (int)(1000 * Math.Pow(i, 3));
            }
        }
        /// <summary>
        /// Возвращает ранг персонажа по количеству его хонор пойнтов
        /// </summary>
        /// <param name="asda2HonorPoints">кол-во хонор пойнтов</param>
        /// <returns>Ранг</returns>
        public static int GetFactionRank(int asda2HonorPoints)
        {
            for (int i = 1; i < 21; i++)
            {
                if (HonorRankPoints[i] > asda2HonorPoints)
                    return i - 1;
            }
            return 0;
        }
        [NotVariable]
        public static short BaseActPointsOnKill = 20;
        //Шанс выпадения каждой из вещей ПКшника 100% - 100000
        [NotVariable]
        public static int PKItemDropChance = 10000;
        [NotVariable]
        public static int ItemDropChance = 1000;

        public static short CalcBattlegrounActPointsOnKill(int killerLevel, int victimLevel, short killerActPoints, short victimActPoints)
        {
            var lvlDiff = victimLevel - killerLevel;
            if (lvlDiff > 0 && lvlDiff < 3 || lvlDiff < 0 && lvlDiff > -5)
                lvlDiff = 0;
            var actPointsDiff = victimActPoints - killerActPoints;
            if (actPointsDiff > 0 && actPointsDiff < 20 || actPointsDiff < 0 && actPointsDiff > -30)
                actPointsDiff = 0;
            bool minusLvl = lvlDiff < 0;
            bool minusPoints = actPointsDiff < 0;
            var lvl = 2.5f * Math.Pow(Math.Abs(lvlDiff), 0.3f);
            var points = 4 * Math.Pow(Math.Abs(actPointsDiff), 0.35f);
            var r = (short)(BaseActPointsOnKill + (minusLvl ? -lvl : lvl) + (minusPoints ? -points : points));
            if (r <= 5)
                return 35;
            return r;
        }

        /// <summary>
        /// Удачен ли синтез аватара
        /// </summary>
        /// <param name="curEnchant">текущая заточка аватара</param>
        /// <param name="useSupl">используется ли увеличение шанса</param>
        /// <returns>да или нет</returns>
        public static bool IsAvatarSyntesSuccess(byte curEnchant, bool useSupl, Asda2ItemQuality quality)
        {
            var rnd = Utility.Random(0, 100000);
            var chance = 0;
            switch (curEnchant)
            {
                case 0:
                    chance = ((int)quality + 1) * 10000 + (useSupl ? 30000 : 0);
                    break;
                case 1:
                    chance = ((int)quality + 1) * 3000 + (useSupl ? 10000 : 0);
                    break;
            }
            return chance > rnd;
        }
        /// <summary>
        /// рачет опыта за съедание яблока
        /// </summary>
        /// <param name="firstChrLvl">сумма уровней персов 1го перса</param>
        /// <param name="soulmatingLvl">уровень дружбы</param>
        /// <returns></returns>
        public static int CalcFriendAppleExp(int lvl, byte soulmatingLvl)
        {
            return (int)(XpGenerator.GetBaseExpForLevel(lvl) * Math.Pow(soulmatingLvl, 0.3f) * Utility.Random(5, 20));
        }
        /// <summary>
        /// Расчет шанса раскопок
        /// </summary>
        /// <param name="initialChance">Изначальный шанс лопаты</param>
        /// <param name="soulmatePoints">очки друга</param>
        /// <param name="luck">очки удачи</param>
        /// <returns>шанс 0 - 100000</returns>
        public static int CalculateDiggingChance(int initialChance, byte soulmatePoints, int luck)
        {
            return (int)(initialChance * Math.Pow(soulmatePoints + 1, 0.05f) * Math.Pow(luck + 1, 0.02f));
        }
        /// <summary>
        /// Количество очков за перерождение.
        /// </summary>
        /// <returns></returns>
        public static int CalculateStatPointsBonusOnReset(int rebornsCount)
        {
            if (rebornsCount == 0)
                rebornsCount = 1;
            return (int)(Utility.Random(400, 600) / Math.Pow(rebornsCount, 0.3f));
        }
        /// <summary>
        /// Расчет очков распределения за каждый уровень
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public static int CalculateFreeStatPointForLevel(int level, int rebornCount)
        {
            var rebornBoost = CalcRebornBoost(rebornCount);
            var stats = 15 * rebornBoost;

            for (int i = 2; i <= level; i++)
            {
                stats += CalcStatBonusPerLevel(level, rebornBoost);
            }
            return (int)stats;
        }

        private static double CalcRebornBoost(int rebornCount)
        {
            var rebornBoost = 1d;
            for (int i = 1; i <= rebornCount; i++)
            {
                switch (i)
                {
                    case 1:
                        rebornBoost += 0.25f;
                        break;
                    case 2:
                        rebornBoost += 0.20f;
                        break;
                    case 3:
                        rebornBoost += 0.15f;
                        break;
                    default:
                        rebornBoost += 0.10f;
                        break;
                }
            }
            return rebornBoost;
        }

        public static double CalcStatBonusPerLevel(int level, double rebornBoost)
        {
            return Math.Pow(level, 0.6f) * 4f * rebornBoost;
        }

        public static double CalcStatBonusPerLevel(int level, int rebornCount)
        {
            return Math.Pow(level, 0.6f) * 4f * CalcRebornBoost(rebornCount);
        }
        public static uint CalculteItemRepairCost(byte maxDurability, byte durability, uint sellPrice, byte enchant, byte levelCriterion, byte qualityCriterion)
        {
            return (uint)
                   ((float)(maxDurability - durability) * sellPrice / 100 * (1 + 0.05 * qualityCriterion) *
                    (1 + 0.05 * levelCriterion) * (1 + 0.05 * enchant));
        }

        public static int GetSowelDeffence(int value, Asda2Profession requiredProfession)
        {
            switch (requiredProfession)
            {
                case Asda2Profession.Any:
                    return 0;
                case Asda2Profession.Archer:
                    return (int)(value * 0.375f * ItemsDeffenceMultiplier);
                case Asda2Profession.Mage:
                    return (int)(value * 0.25f * ItemsDeffenceMultiplier);
                case Asda2Profession.Warrior:
                    return (int)(value * 0.5f * ItemsDeffenceMultiplier);
                case Asda2Profession.NoProfession:
                    return (int)(value * 0.25f * ItemsDeffenceMultiplier);
                default:
                    return 0;
            }
        }

        public static float CalcWeaponTypeMultiplier(Asda2ItemCategory category, ClassId classId)
        {
            switch (category)
            {
                case Asda2ItemCategory.Staff:
                    switch (classId)
                    {
                        case ClassId.AtackMage:
                            return 2.7f;
                        case ClassId.HealMage:
                            return 3.0f;
                        case ClassId.SupportMage:
                            return 3.0f;
                        default:
                            return 1f;
                    }
                case Asda2ItemCategory.TwoHandedSword:
                    return 3.7f;
                case Asda2ItemCategory.OneHandedSword:
                    return 2.1f;
                case Asda2ItemCategory.Spear:
                    return 3.7f;
                case Asda2ItemCategory.Crossbow:
                    return 3.5f;
                case Asda2ItemCategory.Bow:
                    return 3.5f;
                default:
                    return 1f;
            }
        }

        public static int CalcGoldAmountToResetStats(int str, int dex, int sta, int spi, int luck, int intillect, byte chrLevel, int resetsCount)
        {
            return (str + dex + sta + spi + luck + intillect) * 10;
        }

        public static double CalcShieldBlockPrc(Asda2ItemQuality quality, uint requiredLevel)
        {
            var lvlBonus = Math.Pow(requiredLevel, 0.5f) * 0.02f;
            switch (quality)
            {
                case Asda2ItemQuality.Orange:
                    return 0.3f + lvlBonus;
                case Asda2ItemQuality.Green:
                    return 0.25f + lvlBonus;
                case Asda2ItemQuality.Purple:
                    return 0.20f + lvlBonus;
                case Asda2ItemQuality.Yello:
                    return 0.15f + lvlBonus;
                case Asda2ItemQuality.White:
                    return 0.10f + lvlBonus;
                default:
                    return 0.05f + lvlBonus;
            }
        }

        public static int CalcExpForGuessWordEvent(int level)
        {
            return XpGenerator.GetBaseExpForLevel(level) * 25;
        }

        public static float CalcHpPotionBoost(int asda2Stamina)
        {
            return 1f + HpPotionsBoostPerStamina * asda2Stamina;
        }

        /// <summary>
        /// Получаем лимиты волн по лвл гильдии
        /// </summary>
        /// <returns></returns>
        public static int GetWaveLimit(int lvl)
        {
            int waveLimit = 0;
            switch (lvl)
            {
                case 1: waveLimit = 0; break;
                case 2:
                case 3: waveLimit = 1; break;
                case 4: waveLimit = 2; break;
                case 5:
                case 6:
                case 7: waveLimit = 3; break;
                case 8:
                case 9: waveLimit = 4; break;
                case 10: waveLimit = 5; break;
            }
            return waveLimit;
        }

        /// <summary>
        /// Рандом для Guild Wave Reward Table
        /// </summary>
        /// <returns></returns>
        public static int GetWaveRewardItems(List<KeyValuePair<int, int>> items)
        {
            var rnd = Utility.Random(0, 100000);
            foreach (var val in items)
            {
                if (rnd < val.Value)
                {
                    return val.Key;
                }
            }

            return 1;
        }

        public static double MinPingDelay = 500;

        public static int MaxBadPings = 1000;
    }
}
