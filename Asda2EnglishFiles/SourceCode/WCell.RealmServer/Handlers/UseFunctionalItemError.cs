namespace WCell.RealmServer.Handlers
{
    public enum UseFunctionalItemError
    {
        FailedToUse = 0,
        Ok = 1,
        VeicheOk = 1,
        FunctionalItemDoesNotExist = 2,
        YorLevelIsNotHightEnoght = 3,
        YouCantUseItCauseYouHaveMagicProtection = 4,
        YouCantUseItCauseYourTargetHaveMagicProtection = 5,
        IncorectTargetInformaton = 6,
        WarehouseHasReachedMaxCapacity = 7,
        CoolingTimeRemain = 8,
        NotAunctionalItem = 9,
        CoordinateHasNotBeenTargetedInSkillScope = 10, // 0x0000000A
        TheUserTargetedByThisItemDoesNotExist = 11, // 0x0000000B
        ItemEnduranceIsAlready100Prc = 13, // 0x0000000D
        CannotUseItemToProtectAgainstExpirienceLostWhileYouRevivingYourSelf = 14, // 0x0000000E
        CannotUseableByUserWhoNotChangeHisJob = 15, // 0x0000000F
        AlreadyFeelingTheEffectOfSimilarSkillType = 16, // 0x00000010
        HpIs100Prc = 17, // 0x00000011
        MpIs100Prc = 18, // 0x00000012
        HpAdMpIs100Prc = 19, // 0x00000013
        YouCantUseThisItemWhileYourStatusIsSoulmate = 20, // 0x00000014
        YouCanonlyUseItWhenYouCompleteQuest = 21, // 0x00000015
        VeicheHasExprised = 22, // 0x00000016
        YouCantRideVeicheInDungeon = 23, // 0x00000017
        YouCanOnlyUseAMaxOf4WeightIncreaseItems = 25, // 0x00000019
        ItIsNotAllowedCauseShopIsAlreadyOpened = 27, // 0x0000001B
        TheDurationOfTheShopitemHaExprised = 28, // 0x0000001C
        ThisTypeOfItemIsAlreadyInUse = 29, // 0x0000001D
        UnvalidItemInformation = 30, // 0x0000001E
        YouCantChangeToTheSameJob = 31, // 0x0000001F
        UnavlidJobInformationToChanging = 32, // 0x00000020
        CannotChangeableToOtherClass = 33, // 0x00000021
        CannotChangeJobAnyMore = 34, // 0x00000022
    }
}