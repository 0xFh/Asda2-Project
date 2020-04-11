namespace WCell.Constants.Updates
{
    public static class UpdateFields
    {
        public static readonly UpdateField[][] AllFields = new UpdateField[9][]
        {
            new UpdateField[6]
            {
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Object,
                    Name = "GUID",
                    Offset = 0U,
                    Size = 2U,
                    Type = UpdateFieldType.Guid
                },
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Object,
                    Name = "TYPE",
                    Offset = 2U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Object,
                    Name = "ENTRY",
                    Offset = 3U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Object,
                    Name = "SCALE_X",
                    Offset = 4U,
                    Size = 1U,
                    Type = UpdateFieldType.Float
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.None,
                    Group = ObjectTypeId.Object,
                    Name = "PADDING",
                    Offset = 5U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                }
            },
            new UpdateField[64]
            {
                null,
                null,
                null,
                null,
                null,
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Item,
                    Name = "OWNER",
                    Offset = 6U,
                    Size = 2U,
                    Type = UpdateFieldType.Guid
                },
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Item,
                    Name = "CONTAINED",
                    Offset = 8U,
                    Size = 2U,
                    Type = UpdateFieldType.Guid
                },
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Item,
                    Name = "CREATOR",
                    Offset = 10U,
                    Size = 2U,
                    Type = UpdateFieldType.Guid
                },
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Item,
                    Name = "GIFTCREATOR",
                    Offset = 12U,
                    Size = 2U,
                    Type = UpdateFieldType.Guid
                },
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.OwnerOnly | UpdateFieldFlags.ItemOwner,
                    Group = ObjectTypeId.Item,
                    Name = "STACK_COUNT",
                    Offset = 14U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.OwnerOnly | UpdateFieldFlags.ItemOwner,
                    Group = ObjectTypeId.Item,
                    Name = "DURATION",
                    Offset = 15U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.OwnerOnly | UpdateFieldFlags.ItemOwner,
                    Group = ObjectTypeId.Item,
                    Name = "SPELL_CHARGES",
                    Offset = 16U,
                    Size = 5U,
                    Type = UpdateFieldType.UInt32
                },
                null,
                null,
                null,
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Item,
                    Name = "FLAGS",
                    Offset = 21U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Item,
                    Name = "ENCHANTMENT_1_1",
                    Offset = 22U,
                    Size = 2U,
                    Type = UpdateFieldType.UInt32
                },
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Item,
                    Name = "ENCHANTMENT_1_3",
                    Offset = 24U,
                    Size = 1U,
                    Type = UpdateFieldType.TwoInt16
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Item,
                    Name = "ENCHANTMENT_2_1",
                    Offset = 25U,
                    Size = 2U,
                    Type = UpdateFieldType.UInt32
                },
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Item,
                    Name = "ENCHANTMENT_2_3",
                    Offset = 27U,
                    Size = 1U,
                    Type = UpdateFieldType.TwoInt16
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Item,
                    Name = "ENCHANTMENT_3_1",
                    Offset = 28U,
                    Size = 2U,
                    Type = UpdateFieldType.UInt32
                },
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Item,
                    Name = "ENCHANTMENT_3_3",
                    Offset = 30U,
                    Size = 1U,
                    Type = UpdateFieldType.TwoInt16
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Item,
                    Name = "ENCHANTMENT_4_1",
                    Offset = 31U,
                    Size = 2U,
                    Type = UpdateFieldType.UInt32
                },
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Item,
                    Name = "ENCHANTMENT_4_3",
                    Offset = 33U,
                    Size = 1U,
                    Type = UpdateFieldType.TwoInt16
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Item,
                    Name = "ENCHANTMENT_5_1",
                    Offset = 34U,
                    Size = 2U,
                    Type = UpdateFieldType.UInt32
                },
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Item,
                    Name = "ENCHANTMENT_5_3",
                    Offset = 36U,
                    Size = 1U,
                    Type = UpdateFieldType.TwoInt16
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Item,
                    Name = "ENCHANTMENT_6_1",
                    Offset = 37U,
                    Size = 2U,
                    Type = UpdateFieldType.UInt32
                },
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Item,
                    Name = "ENCHANTMENT_6_3",
                    Offset = 39U,
                    Size = 1U,
                    Type = UpdateFieldType.TwoInt16
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Item,
                    Name = "ENCHANTMENT_7_1",
                    Offset = 40U,
                    Size = 2U,
                    Type = UpdateFieldType.UInt32
                },
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Item,
                    Name = "ENCHANTMENT_7_3",
                    Offset = 42U,
                    Size = 1U,
                    Type = UpdateFieldType.TwoInt16
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Item,
                    Name = "ENCHANTMENT_8_1",
                    Offset = 43U,
                    Size = 2U,
                    Type = UpdateFieldType.UInt32
                },
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Item,
                    Name = "ENCHANTMENT_8_3",
                    Offset = 45U,
                    Size = 1U,
                    Type = UpdateFieldType.TwoInt16
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Item,
                    Name = "ENCHANTMENT_9_1",
                    Offset = 46U,
                    Size = 2U,
                    Type = UpdateFieldType.UInt32
                },
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Item,
                    Name = "ENCHANTMENT_9_3",
                    Offset = 48U,
                    Size = 1U,
                    Type = UpdateFieldType.TwoInt16
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Item,
                    Name = "ENCHANTMENT_10_1",
                    Offset = 49U,
                    Size = 2U,
                    Type = UpdateFieldType.UInt32
                },
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Item,
                    Name = "ENCHANTMENT_10_3",
                    Offset = 51U,
                    Size = 1U,
                    Type = UpdateFieldType.TwoInt16
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Item,
                    Name = "ENCHANTMENT_11_1",
                    Offset = 52U,
                    Size = 2U,
                    Type = UpdateFieldType.UInt32
                },
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Item,
                    Name = "ENCHANTMENT_11_3",
                    Offset = 54U,
                    Size = 1U,
                    Type = UpdateFieldType.TwoInt16
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Item,
                    Name = "ENCHANTMENT_12_1",
                    Offset = 55U,
                    Size = 2U,
                    Type = UpdateFieldType.UInt32
                },
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Item,
                    Name = "ENCHANTMENT_12_3",
                    Offset = 57U,
                    Size = 1U,
                    Type = UpdateFieldType.TwoInt16
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Item,
                    Name = "PROPERTY_SEED",
                    Offset = 58U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Item,
                    Name = "RANDOM_PROPERTIES_ID",
                    Offset = 59U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.OwnerOnly | UpdateFieldFlags.ItemOwner,
                    Group = ObjectTypeId.Item,
                    Name = "DURABILITY",
                    Offset = 60U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.OwnerOnly | UpdateFieldFlags.ItemOwner,
                    Group = ObjectTypeId.Item,
                    Name = "MAXDURABILITY",
                    Offset = 61U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Item,
                    Name = "CREATE_PLAYED_TIME",
                    Offset = 62U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.None,
                    Group = ObjectTypeId.Item,
                    Name = "PAD",
                    Offset = 63U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                }
            },
            new UpdateField[138]
            {
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Container,
                    Name = "NUM_SLOTS",
                    Offset = 64U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.None,
                    Group = ObjectTypeId.Container,
                    Name = "ALIGN_PAD",
                    Offset = 65U,
                    Size = 1U,
                    Type = UpdateFieldType.ByteArray
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Container,
                    Name = "SLOT_1",
                    Offset = 66U,
                    Size = 72U,
                    Type = UpdateFieldType.Guid
                },
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null
            },
            new UpdateField[148]
            {
                null,
                null,
                null,
                null,
                null,
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Unit,
                    Name = "CHARM",
                    Offset = 6U,
                    Size = 2U,
                    Type = UpdateFieldType.Guid
                },
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Unit,
                    Name = "SUMMON",
                    Offset = 8U,
                    Size = 2U,
                    Type = UpdateFieldType.Guid
                },
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Unit,
                    Name = "CRITTER",
                    Offset = 10U,
                    Size = 2U,
                    Type = UpdateFieldType.Guid
                },
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Unit,
                    Name = "CHARMEDBY",
                    Offset = 12U,
                    Size = 2U,
                    Type = UpdateFieldType.Guid
                },
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Unit,
                    Name = "SUMMONEDBY",
                    Offset = 14U,
                    Size = 2U,
                    Type = UpdateFieldType.Guid
                },
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Unit,
                    Name = "CREATEDBY",
                    Offset = 16U,
                    Size = 2U,
                    Type = UpdateFieldType.Guid
                },
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Unit,
                    Name = "TARGET",
                    Offset = 18U,
                    Size = 2U,
                    Type = UpdateFieldType.Guid
                },
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Unit,
                    Name = "CHANNEL_OBJECT",
                    Offset = 20U,
                    Size = 2U,
                    Type = UpdateFieldType.Guid
                },
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Unit,
                    Name = "CHANNEL_SPELL",
                    Offset = 22U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Unit,
                    Name = "BYTES_0",
                    Offset = 23U,
                    Size = 1U,
                    Type = UpdateFieldType.ByteArray
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Unit,
                    Name = "HEALTH",
                    Offset = 24U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Unit,
                    Name = "POWER1",
                    Offset = 25U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Unit,
                    Name = "POWER2",
                    Offset = 26U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Unit,
                    Name = "POWER3",
                    Offset = 27U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Unit,
                    Name = "POWER4",
                    Offset = 28U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Unit,
                    Name = "POWER5",
                    Offset = 29U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Unit,
                    Name = "POWER6",
                    Offset = 30U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Unit,
                    Name = "POWER7",
                    Offset = 31U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Unit,
                    Name = "MAXHEALTH",
                    Offset = 32U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Unit,
                    Name = "MAXPOWER1",
                    Offset = 33U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Unit,
                    Name = "MAXPOWER2",
                    Offset = 34U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Unit,
                    Name = "MAXPOWER3",
                    Offset = 35U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Unit,
                    Name = "MAXPOWER4",
                    Offset = 36U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Unit,
                    Name = "MAXPOWER5",
                    Offset = 37U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Unit,
                    Name = "MAXPOWER6",
                    Offset = 38U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Unit,
                    Name = "MAXPOWER7",
                    Offset = 39U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
                    Group = ObjectTypeId.Unit,
                    Name = "POWER_REGEN_FLAT_MODIFIER",
                    Offset = 40U,
                    Size = 7U,
                    Type = UpdateFieldType.Float
                },
                null,
                null,
                null,
                null,
                null,
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
                    Group = ObjectTypeId.Unit,
                    Name = "POWER_REGEN_INTERRUPTED_FLAT_MODIFIER",
                    Offset = 47U,
                    Size = 7U,
                    Type = UpdateFieldType.Float
                },
                null,
                null,
                null,
                null,
                null,
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Unit,
                    Name = "LEVEL",
                    Offset = 54U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Unit,
                    Name = "FACTIONTEMPLATE",
                    Offset = 55U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Unit,
                    Name = "VIRTUAL_ITEM_SLOT_ID",
                    Offset = 56U,
                    Size = 3U,
                    Type = UpdateFieldType.UInt32
                },
                null,
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Unit,
                    Name = "FLAGS",
                    Offset = 59U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Unit,
                    Name = "FLAGS_2",
                    Offset = 60U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Dynamic,
                    Group = ObjectTypeId.Unit,
                    Name = "AURASTATE",
                    Offset = 61U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Unit,
                    Name = "BASEATTACKTIME",
                    Offset = 62U,
                    Size = 2U,
                    Type = UpdateFieldType.UInt32
                },
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Unit,
                    Name = "RANGEDATTACKTIME",
                    Offset = 64U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Unit,
                    Name = "BOUNDINGRADIUS",
                    Offset = 65U,
                    Size = 1U,
                    Type = UpdateFieldType.Float
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Unit,
                    Name = "COMBATREACH",
                    Offset = 66U,
                    Size = 1U,
                    Type = UpdateFieldType.Float
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Unit,
                    Name = "DISPLAYID",
                    Offset = 67U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Unit,
                    Name = "NATIVEDISPLAYID",
                    Offset = 68U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Unit,
                    Name = "MOUNTDISPLAYID",
                    Offset = 69U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly | UpdateFieldFlags.BeastLore,
                    Group = ObjectTypeId.Unit,
                    Name = "MINDAMAGE",
                    Offset = 70U,
                    Size = 1U,
                    Type = UpdateFieldType.Float
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly | UpdateFieldFlags.BeastLore,
                    Group = ObjectTypeId.Unit,
                    Name = "MAXDAMAGE",
                    Offset = 71U,
                    Size = 1U,
                    Type = UpdateFieldType.Float
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly | UpdateFieldFlags.BeastLore,
                    Group = ObjectTypeId.Unit,
                    Name = "MINOFFHANDDAMAGE",
                    Offset = 72U,
                    Size = 1U,
                    Type = UpdateFieldType.Float
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly | UpdateFieldFlags.BeastLore,
                    Group = ObjectTypeId.Unit,
                    Name = "MAXOFFHANDDAMAGE",
                    Offset = 73U,
                    Size = 1U,
                    Type = UpdateFieldType.Float
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Unit,
                    Name = "BYTES_1",
                    Offset = 74U,
                    Size = 1U,
                    Type = UpdateFieldType.ByteArray
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Unit,
                    Name = "PETNUMBER",
                    Offset = 75U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Unit,
                    Name = "PET_NAME_TIMESTAMP",
                    Offset = 76U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.OwnerOnly,
                    Group = ObjectTypeId.Unit,
                    Name = "PETEXPERIENCE",
                    Offset = 77U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.OwnerOnly,
                    Group = ObjectTypeId.Unit,
                    Name = "PETNEXTLEVELEXP",
                    Offset = 78U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Dynamic,
                    Group = ObjectTypeId.Unit,
                    Name = "DYNAMIC_FLAGS",
                    Offset = 79U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Unit,
                    Name = "MOD_CAST_SPEED",
                    Offset = 80U,
                    Size = 1U,
                    Type = UpdateFieldType.Float
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Unit,
                    Name = "CREATED_BY_SPELL",
                    Offset = 81U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Dynamic,
                    Group = ObjectTypeId.Unit,
                    Name = "NPC_FLAGS",
                    Offset = 82U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Unit,
                    Name = "NPC_EMOTESTATE",
                    Offset = 83U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
                    Group = ObjectTypeId.Unit,
                    Name = "STAT0",
                    Offset = 84U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
                    Group = ObjectTypeId.Unit,
                    Name = "STAT1",
                    Offset = 85U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
                    Group = ObjectTypeId.Unit,
                    Name = "STAT2",
                    Offset = 86U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
                    Group = ObjectTypeId.Unit,
                    Name = "STAT3",
                    Offset = 87U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
                    Group = ObjectTypeId.Unit,
                    Name = "STAT4",
                    Offset = 88U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
                    Group = ObjectTypeId.Unit,
                    Name = "POSSTAT0",
                    Offset = 89U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
                    Group = ObjectTypeId.Unit,
                    Name = "POSSTAT1",
                    Offset = 90U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
                    Group = ObjectTypeId.Unit,
                    Name = "POSSTAT2",
                    Offset = 91U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
                    Group = ObjectTypeId.Unit,
                    Name = "POSSTAT3",
                    Offset = 92U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
                    Group = ObjectTypeId.Unit,
                    Name = "POSSTAT4",
                    Offset = 93U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
                    Group = ObjectTypeId.Unit,
                    Name = "NEGSTAT0",
                    Offset = 94U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
                    Group = ObjectTypeId.Unit,
                    Name = "NEGSTAT1",
                    Offset = 95U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
                    Group = ObjectTypeId.Unit,
                    Name = "NEGSTAT2",
                    Offset = 96U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
                    Group = ObjectTypeId.Unit,
                    Name = "NEGSTAT3",
                    Offset = 97U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
                    Group = ObjectTypeId.Unit,
                    Name = "NEGSTAT4",
                    Offset = 98U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly | UpdateFieldFlags.BeastLore,
                    Group = ObjectTypeId.Unit,
                    Name = "RESISTANCES",
                    Offset = 99U,
                    Size = 7U,
                    Type = UpdateFieldType.UInt32
                },
                null,
                null,
                null,
                null,
                null,
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
                    Group = ObjectTypeId.Unit,
                    Name = "RESISTANCEBUFFMODSPOSITIVE",
                    Offset = 106U,
                    Size = 7U,
                    Type = UpdateFieldType.UInt32
                },
                null,
                null,
                null,
                null,
                null,
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
                    Group = ObjectTypeId.Unit,
                    Name = "RESISTANCEBUFFMODSNEGATIVE",
                    Offset = 113U,
                    Size = 7U,
                    Type = UpdateFieldType.UInt32
                },
                null,
                null,
                null,
                null,
                null,
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Unit,
                    Name = "BASE_MANA",
                    Offset = 120U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
                    Group = ObjectTypeId.Unit,
                    Name = "BASE_HEALTH",
                    Offset = 121U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Unit,
                    Name = "BYTES_2",
                    Offset = 122U,
                    Size = 1U,
                    Type = UpdateFieldType.ByteArray
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
                    Group = ObjectTypeId.Unit,
                    Name = "ATTACK_POWER",
                    Offset = 123U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
                    Group = ObjectTypeId.Unit,
                    Name = "ATTACK_POWER_MODS",
                    Offset = 124U,
                    Size = 1U,
                    Type = UpdateFieldType.TwoInt16
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
                    Group = ObjectTypeId.Unit,
                    Name = "ATTACK_POWER_MULTIPLIER",
                    Offset = 125U,
                    Size = 1U,
                    Type = UpdateFieldType.Float
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
                    Group = ObjectTypeId.Unit,
                    Name = "RANGED_ATTACK_POWER",
                    Offset = 126U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
                    Group = ObjectTypeId.Unit,
                    Name = "RANGED_ATTACK_POWER_MODS",
                    Offset = (uint) sbyte.MaxValue,
                    Size = 1U,
                    Type = UpdateFieldType.TwoInt16
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
                    Group = ObjectTypeId.Unit,
                    Name = "RANGED_ATTACK_POWER_MULTIPLIER",
                    Offset = 128U,
                    Size = 1U,
                    Type = UpdateFieldType.Float
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
                    Group = ObjectTypeId.Unit,
                    Name = "MINRANGEDDAMAGE",
                    Offset = 129U,
                    Size = 1U,
                    Type = UpdateFieldType.Float
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
                    Group = ObjectTypeId.Unit,
                    Name = "MAXRANGEDDAMAGE",
                    Offset = 130U,
                    Size = 1U,
                    Type = UpdateFieldType.Float
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
                    Group = ObjectTypeId.Unit,
                    Name = "POWER_COST_MODIFIER",
                    Offset = 131U,
                    Size = 7U,
                    Type = UpdateFieldType.UInt32
                },
                null,
                null,
                null,
                null,
                null,
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
                    Group = ObjectTypeId.Unit,
                    Name = "POWER_COST_MULTIPLIER",
                    Offset = 138U,
                    Size = 7U,
                    Type = UpdateFieldType.Float
                },
                null,
                null,
                null,
                null,
                null,
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly,
                    Group = ObjectTypeId.Unit,
                    Name = "MAXHEALTHMODIFIER",
                    Offset = 145U,
                    Size = 1U,
                    Type = UpdateFieldType.Float
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Unit,
                    Name = "HOVERHEIGHT",
                    Offset = 146U,
                    Size = 1U,
                    Type = UpdateFieldType.Float
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.None,
                    Group = ObjectTypeId.Unit,
                    Name = "PADDING",
                    Offset = 147U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                }
            },
            new UpdateField[1326]
            {
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Player,
                    Name = "DUEL_ARBITER",
                    Offset = 148U,
                    Size = 2U,
                    Type = UpdateFieldType.Guid
                },
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Player,
                    Name = "FLAGS",
                    Offset = 150U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Player,
                    Name = "GUILDID",
                    Offset = 151U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Player,
                    Name = "GUILDRANK",
                    Offset = 152U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Player,
                    Name = "BYTES",
                    Offset = 153U,
                    Size = 1U,
                    Type = UpdateFieldType.ByteArray
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Player,
                    Name = "BYTES_2",
                    Offset = 154U,
                    Size = 1U,
                    Type = UpdateFieldType.ByteArray
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Player,
                    Name = "BYTES_3",
                    Offset = 155U,
                    Size = 1U,
                    Type = UpdateFieldType.ByteArray
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Player,
                    Name = "DUEL_TEAM",
                    Offset = 156U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Player,
                    Name = "GUILD_TIMESTAMP",
                    Offset = 157U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.GroupOnly,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_1_1",
                    Offset = 158U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_1_2",
                    Offset = 159U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_1_3",
                    Offset = 160U,
                    Size = 2U,
                    Type = UpdateFieldType.TwoInt16
                },
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_1_4",
                    Offset = 162U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.GroupOnly,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_2_1",
                    Offset = 163U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_2_2",
                    Offset = 164U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_2_3",
                    Offset = 165U,
                    Size = 2U,
                    Type = UpdateFieldType.TwoInt16
                },
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_2_5",
                    Offset = 167U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.GroupOnly,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_3_1",
                    Offset = 168U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_3_2",
                    Offset = 169U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_3_3",
                    Offset = 170U,
                    Size = 2U,
                    Type = UpdateFieldType.TwoInt16
                },
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_3_5",
                    Offset = 172U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.GroupOnly,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_4_1",
                    Offset = 173U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_4_2",
                    Offset = 174U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_4_3",
                    Offset = 175U,
                    Size = 2U,
                    Type = UpdateFieldType.TwoInt16
                },
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_4_5",
                    Offset = 177U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.GroupOnly,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_5_1",
                    Offset = 178U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_5_2",
                    Offset = 179U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_5_3",
                    Offset = 180U,
                    Size = 2U,
                    Type = UpdateFieldType.TwoInt16
                },
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_5_5",
                    Offset = 182U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.GroupOnly,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_6_1",
                    Offset = 183U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_6_2",
                    Offset = 184U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_6_3",
                    Offset = 185U,
                    Size = 2U,
                    Type = UpdateFieldType.TwoInt16
                },
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_6_5",
                    Offset = 187U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.GroupOnly,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_7_1",
                    Offset = 188U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_7_2",
                    Offset = 189U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_7_3",
                    Offset = 190U,
                    Size = 2U,
                    Type = UpdateFieldType.TwoInt16
                },
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_7_5",
                    Offset = 192U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.GroupOnly,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_8_1",
                    Offset = 193U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_8_2",
                    Offset = 194U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_8_3",
                    Offset = 195U,
                    Size = 2U,
                    Type = UpdateFieldType.TwoInt16
                },
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_8_5",
                    Offset = 197U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.GroupOnly,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_9_1",
                    Offset = 198U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_9_2",
                    Offset = 199U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_9_3",
                    Offset = 200U,
                    Size = 2U,
                    Type = UpdateFieldType.TwoInt16
                },
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_9_5",
                    Offset = 202U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.GroupOnly,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_10_1",
                    Offset = 203U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_10_2",
                    Offset = 204U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_10_3",
                    Offset = 205U,
                    Size = 2U,
                    Type = UpdateFieldType.TwoInt16
                },
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_10_5",
                    Offset = 207U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.GroupOnly,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_11_1",
                    Offset = 208U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_11_2",
                    Offset = 209U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_11_3",
                    Offset = 210U,
                    Size = 2U,
                    Type = UpdateFieldType.TwoInt16
                },
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_11_5",
                    Offset = 212U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.GroupOnly,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_12_1",
                    Offset = 213U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_12_2",
                    Offset = 214U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_12_3",
                    Offset = 215U,
                    Size = 2U,
                    Type = UpdateFieldType.TwoInt16
                },
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_12_5",
                    Offset = 217U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.GroupOnly,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_13_1",
                    Offset = 218U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_13_2",
                    Offset = 219U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_13_3",
                    Offset = 220U,
                    Size = 2U,
                    Type = UpdateFieldType.TwoInt16
                },
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_13_5",
                    Offset = 222U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.GroupOnly,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_14_1",
                    Offset = 223U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_14_2",
                    Offset = 224U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_14_3",
                    Offset = 225U,
                    Size = 2U,
                    Type = UpdateFieldType.TwoInt16
                },
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_14_5",
                    Offset = 227U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.GroupOnly,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_15_1",
                    Offset = 228U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_15_2",
                    Offset = 229U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_15_3",
                    Offset = 230U,
                    Size = 2U,
                    Type = UpdateFieldType.TwoInt16
                },
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_15_5",
                    Offset = 232U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.GroupOnly,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_16_1",
                    Offset = 233U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_16_2",
                    Offset = 234U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_16_3",
                    Offset = 235U,
                    Size = 2U,
                    Type = UpdateFieldType.TwoInt16
                },
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_16_5",
                    Offset = 237U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.GroupOnly,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_17_1",
                    Offset = 238U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_17_2",
                    Offset = 239U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_17_3",
                    Offset = 240U,
                    Size = 2U,
                    Type = UpdateFieldType.TwoInt16
                },
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_17_5",
                    Offset = 242U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.GroupOnly,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_18_1",
                    Offset = 243U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_18_2",
                    Offset = 244U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_18_3",
                    Offset = 245U,
                    Size = 2U,
                    Type = UpdateFieldType.TwoInt16
                },
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_18_5",
                    Offset = 247U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.GroupOnly,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_19_1",
                    Offset = 248U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_19_2",
                    Offset = 249U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_19_3",
                    Offset = 250U,
                    Size = 2U,
                    Type = UpdateFieldType.TwoInt16
                },
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_19_5",
                    Offset = 252U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.GroupOnly,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_20_1",
                    Offset = 253U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_20_2",
                    Offset = 254U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_20_3",
                    Offset = (uint) byte.MaxValue,
                    Size = 2U,
                    Type = UpdateFieldType.TwoInt16
                },
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_20_5",
                    Offset = 257U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.GroupOnly,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_21_1",
                    Offset = 258U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_21_2",
                    Offset = 259U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_21_3",
                    Offset = 260U,
                    Size = 2U,
                    Type = UpdateFieldType.TwoInt16
                },
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_21_5",
                    Offset = 262U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.GroupOnly,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_22_1",
                    Offset = 263U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_22_2",
                    Offset = 264U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_22_3",
                    Offset = 265U,
                    Size = 2U,
                    Type = UpdateFieldType.TwoInt16
                },
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_22_5",
                    Offset = 267U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.GroupOnly,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_23_1",
                    Offset = 268U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_23_2",
                    Offset = 269U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_23_3",
                    Offset = 270U,
                    Size = 2U,
                    Type = UpdateFieldType.TwoInt16
                },
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_23_5",
                    Offset = 272U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.GroupOnly,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_24_1",
                    Offset = 273U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_24_2",
                    Offset = 274U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_24_3",
                    Offset = 275U,
                    Size = 2U,
                    Type = UpdateFieldType.TwoInt16
                },
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_24_5",
                    Offset = 277U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.GroupOnly,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_25_1",
                    Offset = 278U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_25_2",
                    Offset = 279U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_25_3",
                    Offset = 280U,
                    Size = 2U,
                    Type = UpdateFieldType.TwoInt16
                },
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "QUEST_LOG_25_5",
                    Offset = 282U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Player,
                    Name = "VISIBLE_ITEM_1_ENTRYID",
                    Offset = 283U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Player,
                    Name = "VISIBLE_ITEM_1_ENCHANTMENT",
                    Offset = 284U,
                    Size = 1U,
                    Type = UpdateFieldType.TwoInt16
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Player,
                    Name = "VISIBLE_ITEM_2_ENTRYID",
                    Offset = 285U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Player,
                    Name = "VISIBLE_ITEM_2_ENCHANTMENT",
                    Offset = 286U,
                    Size = 1U,
                    Type = UpdateFieldType.TwoInt16
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Player,
                    Name = "VISIBLE_ITEM_3_ENTRYID",
                    Offset = 287U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Player,
                    Name = "VISIBLE_ITEM_3_ENCHANTMENT",
                    Offset = 288U,
                    Size = 1U,
                    Type = UpdateFieldType.TwoInt16
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Player,
                    Name = "VISIBLE_ITEM_4_ENTRYID",
                    Offset = 289U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Player,
                    Name = "VISIBLE_ITEM_4_ENCHANTMENT",
                    Offset = 290U,
                    Size = 1U,
                    Type = UpdateFieldType.TwoInt16
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Player,
                    Name = "VISIBLE_ITEM_5_ENTRYID",
                    Offset = 291U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Player,
                    Name = "VISIBLE_ITEM_5_ENCHANTMENT",
                    Offset = 292U,
                    Size = 1U,
                    Type = UpdateFieldType.TwoInt16
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Player,
                    Name = "VISIBLE_ITEM_6_ENTRYID",
                    Offset = 293U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Player,
                    Name = "VISIBLE_ITEM_6_ENCHANTMENT",
                    Offset = 294U,
                    Size = 1U,
                    Type = UpdateFieldType.TwoInt16
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Player,
                    Name = "VISIBLE_ITEM_7_ENTRYID",
                    Offset = 295U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Player,
                    Name = "VISIBLE_ITEM_7_ENCHANTMENT",
                    Offset = 296U,
                    Size = 1U,
                    Type = UpdateFieldType.TwoInt16
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Player,
                    Name = "VISIBLE_ITEM_8_ENTRYID",
                    Offset = 297U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Player,
                    Name = "VISIBLE_ITEM_8_ENCHANTMENT",
                    Offset = 298U,
                    Size = 1U,
                    Type = UpdateFieldType.TwoInt16
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Player,
                    Name = "VISIBLE_ITEM_9_ENTRYID",
                    Offset = 299U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Player,
                    Name = "VISIBLE_ITEM_9_ENCHANTMENT",
                    Offset = 300U,
                    Size = 1U,
                    Type = UpdateFieldType.TwoInt16
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Player,
                    Name = "VISIBLE_ITEM_10_ENTRYID",
                    Offset = 301U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Player,
                    Name = "VISIBLE_ITEM_10_ENCHANTMENT",
                    Offset = 302U,
                    Size = 1U,
                    Type = UpdateFieldType.TwoInt16
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Player,
                    Name = "VISIBLE_ITEM_11_ENTRYID",
                    Offset = 303U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Player,
                    Name = "VISIBLE_ITEM_11_ENCHANTMENT",
                    Offset = 304U,
                    Size = 1U,
                    Type = UpdateFieldType.TwoInt16
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Player,
                    Name = "VISIBLE_ITEM_12_ENTRYID",
                    Offset = 305U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Player,
                    Name = "VISIBLE_ITEM_12_ENCHANTMENT",
                    Offset = 306U,
                    Size = 1U,
                    Type = UpdateFieldType.TwoInt16
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Player,
                    Name = "VISIBLE_ITEM_13_ENTRYID",
                    Offset = 307U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Player,
                    Name = "VISIBLE_ITEM_13_ENCHANTMENT",
                    Offset = 308U,
                    Size = 1U,
                    Type = UpdateFieldType.TwoInt16
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Player,
                    Name = "VISIBLE_ITEM_14_ENTRYID",
                    Offset = 309U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Player,
                    Name = "VISIBLE_ITEM_14_ENCHANTMENT",
                    Offset = 310U,
                    Size = 1U,
                    Type = UpdateFieldType.TwoInt16
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Player,
                    Name = "VISIBLE_ITEM_15_ENTRYID",
                    Offset = 311U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Player,
                    Name = "VISIBLE_ITEM_15_ENCHANTMENT",
                    Offset = 312U,
                    Size = 1U,
                    Type = UpdateFieldType.TwoInt16
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Player,
                    Name = "VISIBLE_ITEM_16_ENTRYID",
                    Offset = 313U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Player,
                    Name = "VISIBLE_ITEM_16_ENCHANTMENT",
                    Offset = 314U,
                    Size = 1U,
                    Type = UpdateFieldType.TwoInt16
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Player,
                    Name = "VISIBLE_ITEM_17_ENTRYID",
                    Offset = 315U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Player,
                    Name = "VISIBLE_ITEM_17_ENCHANTMENT",
                    Offset = 316U,
                    Size = 1U,
                    Type = UpdateFieldType.TwoInt16
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Player,
                    Name = "VISIBLE_ITEM_18_ENTRYID",
                    Offset = 317U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Player,
                    Name = "VISIBLE_ITEM_18_ENCHANTMENT",
                    Offset = 318U,
                    Size = 1U,
                    Type = UpdateFieldType.TwoInt16
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Player,
                    Name = "VISIBLE_ITEM_19_ENTRYID",
                    Offset = 319U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Player,
                    Name = "VISIBLE_ITEM_19_ENCHANTMENT",
                    Offset = 320U,
                    Size = 1U,
                    Type = UpdateFieldType.TwoInt16
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Player,
                    Name = "CHOSEN_TITLE",
                    Offset = 321U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Player,
                    Name = "FAKE_INEBRIATION",
                    Offset = 322U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.None,
                    Group = ObjectTypeId.Player,
                    Name = "PAD_0",
                    Offset = 323U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "INV_SLOT_HEAD",
                    Offset = 324U,
                    Size = 46U,
                    Type = UpdateFieldType.Guid
                },
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "PACK_SLOT_1",
                    Offset = 370U,
                    Size = 32U,
                    Type = UpdateFieldType.Guid
                },
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "BANK_SLOT_1",
                    Offset = 402U,
                    Size = 56U,
                    Type = UpdateFieldType.Guid
                },
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "BANKBAG_SLOT_1",
                    Offset = 458U,
                    Size = 14U,
                    Type = UpdateFieldType.Guid
                },
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "VENDORBUYBACK_SLOT_1",
                    Offset = 472U,
                    Size = 24U,
                    Type = UpdateFieldType.Guid
                },
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "KEYRING_SLOT_1",
                    Offset = 496U,
                    Size = 64U,
                    Type = UpdateFieldType.Guid
                },
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "CURRENCYTOKEN_SLOT_1",
                    Offset = 560U,
                    Size = 64U,
                    Type = UpdateFieldType.Guid
                },
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "FARSIGHT",
                    Offset = 624U,
                    Size = 2U,
                    Type = UpdateFieldType.Guid
                },
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "_FIELD_KNOWN_TITLES",
                    Offset = 626U,
                    Size = 2U,
                    Type = UpdateFieldType.Guid
                },
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "_FIELD_KNOWN_TITLES1",
                    Offset = 628U,
                    Size = 2U,
                    Type = UpdateFieldType.Guid
                },
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "_FIELD_KNOWN_TITLES2",
                    Offset = 630U,
                    Size = 2U,
                    Type = UpdateFieldType.Guid
                },
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "KNOWN_CURRENCIES",
                    Offset = 632U,
                    Size = 2U,
                    Type = UpdateFieldType.Guid
                },
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "XP",
                    Offset = 634U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "NEXT_LEVEL_XP",
                    Offset = 635U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "SKILL_INFO_1_1",
                    Offset = 636U,
                    Size = 384U,
                    Type = UpdateFieldType.TwoInt16
                },
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "CHARACTER_POINTS1",
                    Offset = 1020U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "CHARACTER_POINTS2",
                    Offset = 1021U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "TRACK_CREATURES",
                    Offset = 1022U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "TRACK_RESOURCES",
                    Offset = 1023U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "BLOCK_PERCENTAGE",
                    Offset = 1024U,
                    Size = 1U,
                    Type = UpdateFieldType.Float
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "DODGE_PERCENTAGE",
                    Offset = 1025U,
                    Size = 1U,
                    Type = UpdateFieldType.Float
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "PARRY_PERCENTAGE",
                    Offset = 1026U,
                    Size = 1U,
                    Type = UpdateFieldType.Float
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "EXPERTISE",
                    Offset = 1027U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "OFFHAND_EXPERTISE",
                    Offset = 1028U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "CRIT_PERCENTAGE",
                    Offset = 1029U,
                    Size = 1U,
                    Type = UpdateFieldType.Float
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "RANGED_CRIT_PERCENTAGE",
                    Offset = 1030U,
                    Size = 1U,
                    Type = UpdateFieldType.Float
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "OFFHAND_CRIT_PERCENTAGE",
                    Offset = 1031U,
                    Size = 1U,
                    Type = UpdateFieldType.Float
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "SPELL_CRIT_PERCENTAGE1",
                    Offset = 1032U,
                    Size = 7U,
                    Type = UpdateFieldType.Float
                },
                null,
                null,
                null,
                null,
                null,
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "SHIELD_BLOCK",
                    Offset = 1039U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "SHIELD_BLOCK_CRIT_PERCENTAGE",
                    Offset = 1040U,
                    Size = 1U,
                    Type = UpdateFieldType.Float
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "EXPLORED_ZONES_1",
                    Offset = 1041U,
                    Size = 128U,
                    Type = UpdateFieldType.ByteArray
                },
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "REST_STATE_EXPERIENCE",
                    Offset = 1169U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "COINAGE",
                    Offset = 1170U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "MOD_DAMAGE_DONE_POS",
                    Offset = 1171U,
                    Size = 7U,
                    Type = UpdateFieldType.UInt32
                },
                null,
                null,
                null,
                null,
                null,
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "MOD_DAMAGE_DONE_NEG",
                    Offset = 1178U,
                    Size = 7U,
                    Type = UpdateFieldType.UInt32
                },
                null,
                null,
                null,
                null,
                null,
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "MOD_DAMAGE_DONE_PCT",
                    Offset = 1185U,
                    Size = 7U,
                    Type = UpdateFieldType.UInt32
                },
                null,
                null,
                null,
                null,
                null,
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "MOD_HEALING_DONE_POS",
                    Offset = 1192U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "MOD_HEALING_PCT",
                    Offset = 1193U,
                    Size = 1U,
                    Type = UpdateFieldType.Float
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "MOD_HEALING_DONE_PCT",
                    Offset = 1194U,
                    Size = 1U,
                    Type = UpdateFieldType.Float
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "MOD_TARGET_RESISTANCE",
                    Offset = 1195U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "MOD_TARGET_PHYSICAL_RESISTANCE",
                    Offset = 1196U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "PLAYER_FIELD_BYTES",
                    Offset = 1197U,
                    Size = 1U,
                    Type = UpdateFieldType.ByteArray
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "AMMO_ID",
                    Offset = 1198U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "SELF_RES_SPELL",
                    Offset = 1199U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "PVP_MEDALS",
                    Offset = 1200U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "BUYBACK_PRICE_1",
                    Offset = 1201U,
                    Size = 12U,
                    Type = UpdateFieldType.UInt32
                },
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "BUYBACK_TIMESTAMP_1",
                    Offset = 1213U,
                    Size = 12U,
                    Type = UpdateFieldType.UInt32
                },
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "KILLS",
                    Offset = 1225U,
                    Size = 1U,
                    Type = UpdateFieldType.TwoInt16
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "TODAY_CONTRIBUTION",
                    Offset = 1226U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "YESTERDAY_CONTRIBUTION",
                    Offset = 1227U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "LIFETIME_HONORBALE_KILLS",
                    Offset = 1228U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "PLAYER_FIELD_BYTES2",
                    Offset = 1229U,
                    Size = 1U,
                    Type = UpdateFieldType.Unk322
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "WATCHED_FACTION_INDEX",
                    Offset = 1230U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "COMBAT_RATING_1",
                    Offset = 1231U,
                    Size = 25U,
                    Type = UpdateFieldType.UInt32
                },
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "ARENA_TEAM_INFO_1_1",
                    Offset = 1256U,
                    Size = 21U,
                    Type = UpdateFieldType.UInt32
                },
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "HONOR_CURRENCY",
                    Offset = 1277U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "ARENA_CURRENCY",
                    Offset = 1278U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "MAX_LEVEL",
                    Offset = 1279U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "DAILY_QUESTS_1",
                    Offset = 1280U,
                    Size = 25U,
                    Type = UpdateFieldType.UInt32
                },
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "RUNE_REGEN_1",
                    Offset = 1305U,
                    Size = 4U,
                    Type = UpdateFieldType.Float
                },
                null,
                null,
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "NO_REAGENT_COST_1",
                    Offset = 1309U,
                    Size = 3U,
                    Type = UpdateFieldType.UInt32
                },
                null,
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "GLYPH_SLOTS_1",
                    Offset = 1312U,
                    Size = 6U,
                    Type = UpdateFieldType.UInt32
                },
                null,
                null,
                null,
                null,
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "GLYPHS_1",
                    Offset = 1318U,
                    Size = 6U,
                    Type = UpdateFieldType.UInt32
                },
                null,
                null,
                null,
                null,
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "GLYPHS_ENABLED",
                    Offset = 1324U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Private,
                    Group = ObjectTypeId.Player,
                    Name = "PET_SPELL_POWER",
                    Offset = 1325U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                }
            },
            new UpdateField[18]
            {
                null,
                null,
                null,
                null,
                null,
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.GameObject,
                    Name = "OBJECT_FIELD_CREATED_BY",
                    Offset = 6U,
                    Size = 2U,
                    Type = UpdateFieldType.Guid
                },
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.GameObject,
                    Name = "DISPLAYID",
                    Offset = 8U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.GameObject,
                    Name = "FLAGS",
                    Offset = 9U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.GameObject,
                    Name = "PARENTROTATION",
                    Offset = 10U,
                    Size = 4U,
                    Type = UpdateFieldType.Float
                },
                null,
                null,
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Dynamic,
                    Group = ObjectTypeId.GameObject,
                    Name = "DYNAMIC",
                    Offset = 14U,
                    Size = 1U,
                    Type = UpdateFieldType.TwoInt16
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.GameObject,
                    Name = "FACTION",
                    Offset = 15U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.GameObject,
                    Name = "LEVEL",
                    Offset = 16U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.GameObject,
                    Name = "BYTES_1",
                    Offset = 17U,
                    Size = 1U,
                    Type = UpdateFieldType.ByteArray
                }
            },
            new UpdateField[12]
            {
                null,
                null,
                null,
                null,
                null,
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.DynamicObject,
                    Name = "CASTER",
                    Offset = 6U,
                    Size = 2U,
                    Type = UpdateFieldType.Guid
                },
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.DynamicObject,
                    Name = "BYTES",
                    Offset = 8U,
                    Size = 1U,
                    Type = UpdateFieldType.ByteArray
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.DynamicObject,
                    Name = "SPELLID",
                    Offset = 9U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.DynamicObject,
                    Name = "RADIUS",
                    Offset = 10U,
                    Size = 1U,
                    Type = UpdateFieldType.Float
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.DynamicObject,
                    Name = "CASTTIME",
                    Offset = 11U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                }
            },
            new UpdateField[36]
            {
                null,
                null,
                null,
                null,
                null,
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Corpse,
                    Name = "OWNER",
                    Offset = 6U,
                    Size = 2U,
                    Type = UpdateFieldType.Guid
                },
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Corpse,
                    Name = "PARTY",
                    Offset = 8U,
                    Size = 2U,
                    Type = UpdateFieldType.Guid
                },
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Corpse,
                    Name = "DISPLAY_ID",
                    Offset = 10U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Corpse,
                    Name = "ITEM",
                    Offset = 11U,
                    Size = 19U,
                    Type = UpdateFieldType.UInt32
                },
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Corpse,
                    Name = "BYTES_1",
                    Offset = 30U,
                    Size = 1U,
                    Type = UpdateFieldType.ByteArray
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Corpse,
                    Name = "BYTES_2",
                    Offset = 31U,
                    Size = 1U,
                    Type = UpdateFieldType.ByteArray
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Corpse,
                    Name = "GUILD",
                    Offset = 32U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Corpse,
                    Name = "FLAGS",
                    Offset = 33U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Dynamic,
                    Group = ObjectTypeId.Corpse,
                    Name = "DYNAMIC_FLAGS",
                    Offset = 34U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.None,
                    Group = ObjectTypeId.Corpse,
                    Name = "PAD",
                    Offset = 35U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                }
            },
            new UpdateField[6]
            {
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Object,
                    Name = "GUID",
                    Offset = 0U,
                    Size = 2U,
                    Type = UpdateFieldType.Guid
                },
                null,
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Object,
                    Name = "TYPE",
                    Offset = 2U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Object,
                    Name = "ENTRY",
                    Offset = 3U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.Public,
                    Group = ObjectTypeId.Object,
                    Name = "SCALE_X",
                    Offset = 4U,
                    Size = 1U,
                    Type = UpdateFieldType.Float
                },
                new UpdateField()
                {
                    Flags = UpdateFieldFlags.None,
                    Group = ObjectTypeId.Object,
                    Name = "PADDING",
                    Offset = 5U,
                    Size = 1U,
                    Type = UpdateFieldType.UInt32
                }
            }
        };
    }
}