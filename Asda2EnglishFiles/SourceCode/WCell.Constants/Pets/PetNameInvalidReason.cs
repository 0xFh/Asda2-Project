namespace WCell.Constants.Pets
{
    public enum PetNameInvalidReason : uint
    {
        Ok = 0,
        Invalid = 1,
        NoName = 2,
        TooShort = 3,
        TooLong = 4,
        MixedLanguages = 6,
        Profane = 7,
        Reserved = 8,
        ThreeConsecutive = 11, // 0x0000000B
        InvalidSpace = 12, // 0x0000000C
        ConsecutiveSpaces = 13, // 0x0000000D
        RussianConsecutiveSilentChars = 14, // 0x0000000E
        RussianSilentCharAtBeginOrEnd = 15, // 0x0000000F
        DeclensionDoesntMatchBaseName = 16, // 0x00000010
    }
}