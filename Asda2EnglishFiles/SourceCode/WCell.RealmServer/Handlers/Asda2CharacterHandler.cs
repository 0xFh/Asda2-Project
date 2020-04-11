using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants;
using WCell.Constants.Achievements;
using WCell.Constants.Items;
using WCell.Constants.World;
using WCell.Core;
using WCell.Core.Network;
using WCell.RealmServer.Achievements;
using WCell.RealmServer.Asda2_Items;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Formulas;
using WCell.RealmServer.Network;
using WCell.RealmServer.RacesClasses;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;
using WCell.Util.Threading;
using WCell.Util.Variables;

namespace WCell.RealmServer.Handlers
{
    internal class Asda2CharacterHandler
    {
        private static readonly byte[] stab15 = new byte[47]
        {
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 222,
            (byte) 7,
            (byte) 1,
            (byte) 0,
            (byte) 2,
            (byte) 0,
            (byte) 1,
            (byte) 84,
            (byte) 46,
            (byte) 0,
            (byte) 0
        };

        private static readonly byte[] unk13 = new byte[40]
        {
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue
        };

        private static readonly byte[] unk24 = new byte[361]
        {
            (byte) 1,
            (byte) 0,
            (byte) 0,
            (byte) 11,
            (byte) 0,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 1,
            (byte) 0,
            (byte) 3,
            (byte) 0,
            (byte) 0,
            (byte) 101,
            (byte) 0,
            (byte) 196,
            (byte) 240,
            (byte) 224,
            (byte) 234,
            (byte) 238,
            (byte) 237,
            (byte) 202,
            (byte) 240,
            (byte) 238,
            (byte) 226,
            (byte) 232,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 6,
            (byte) 240,
            (byte) 0,
            (byte) 0,
            (byte) 12,
            (byte) 90,
            (byte) 101,
            (byte) 107,
            (byte) 115,
            (byte) 116,
            (byte) 101,
            (byte) 114,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 1,
            (byte) 10,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 23,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 9,
            (byte) 0,
            (byte) 0,
            (byte) 101,
            (byte) 0,
            (byte) 196,
            (byte) 240,
            (byte) 224,
            (byte) 234,
            (byte) 238,
            (byte) 237,
            (byte) 202,
            (byte) 240,
            (byte) 238,
            (byte) 226,
            (byte) 232,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 6,
            (byte) 240,
            (byte) 0,
            (byte) 0,
            (byte) 12,
            (byte) 90,
            (byte) 101,
            (byte) 107,
            (byte) 115,
            (byte) 116,
            (byte) 101,
            (byte) 114,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 1,
            (byte) 10,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 23,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 1,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0
        };

        private static readonly byte[] stub33 = new byte[13]
        {
            (byte) 16,
            (byte) 1,
            (byte) 240,
            (byte) 119,
            (byte) 1,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 207,
            (byte) 207,
            (byte) 223
        };

        private static readonly byte[] stub34 = new byte[12]
        {
            (byte) 45,
            (byte) 155,
            (byte) 171,
            (byte) 4,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 36,
            (byte) 33,
            (byte) 109,
            (byte) 21
        };

        private static readonly byte[] Stub80 = new byte[44]
        {
            (byte) 16,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 15,
            (byte) 0,
            (byte) 5,
            (byte) 0,
            (byte) 2,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0
        };

        private static readonly byte[] stub40 = new byte[28];
        private static readonly byte[] stab14 = new byte[3];
        private static readonly byte[] stub13 = new byte[61];
        private static readonly byte[] stab6 = new byte[1];

        private static readonly byte[] stab31 = new byte[466]
        {
            (byte) 4,
            (byte) 1,
            (byte) 0,
            (byte) 2,
            (byte) 220,
            (byte) 1,
            (byte) 1,
            (byte) 0,
            (byte) 234,
            (byte) 105,
            (byte) 41,
            (byte) 81,
            (byte) 0,
            (byte) 0,
            (byte) 3,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 2,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 2,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            (byte) 252,
            (byte) 94,
            (byte) 0,
            (byte) 0,
            (byte) 1,
            (byte) 5,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 100,
            (byte) 124,
            (byte) 2,
            (byte) 148,
            (byte) 33,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 84,
            (byte) 0,
            (byte) 9,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 2,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 105,
            (byte) 80,
            (byte) 0,
            (byte) 0,
            (byte) 2,
            (byte) 13,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 10,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 120,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0
        };

        private static readonly byte[] stab501 = new byte[20]
        {
            (byte) 4,
            (byte) 1,
            (byte) 0,
            (byte) 2,
            (byte) 220,
            (byte) 1,
            (byte) 1,
            (byte) 0,
            (byte) 234,
            (byte) 105,
            (byte) 41,
            (byte) 81,
            (byte) 0,
            (byte) 0,
            (byte) 3,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue
        };

        private const int O1 = 32767000;
        [NotVariable] private static byte _emoteCnt;

        public static void SendUpdateDurabilityResponse(IRealmClient client, Asda2Item item)
        {
            if (item.Durability <= (byte) 50)
            {
                AchievementProgressRecord progressRecord =
                    client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(104U);
                switch (++progressRecord.Counter)
                {
                    case 50:
                        client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Damaged237);
                        break;
                    case 100:
                        client.ActiveCharacter.GetTitle(Asda2TitleId.Damaged237);
                        break;
                }

                progressRecord.SaveAndFlush();
            }

            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.UpdateDurability))
            {
                packet.WriteByte((byte) item.InventoryType);
                packet.WriteInt16(item.Slot);
                packet.WriteByte(item.Durability);
                client.Send(packet, true);
            }
        }

        public static void SendPetBoxSizeInitResponse(Character chr)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.PetBoxSizeInit))
            {
                packet.WriteInt32(chr.AccId);
                packet.WriteByte(((int) chr.Record.PetBoxEnchants + 1) * 6);
                chr.Send(packet, true);
            }
        }

        [PacketHandler((RealmServerOpCode) 6150)]
        public static void CommisProdRequest(IRealmClient client, RealmPacketIn packet)
        {
            short slotInq = packet.ReadInt16();
            ++packet.Position;
            int amount = packet.ReadInt32();
            Asda2Item regularItem = client.ActiveCharacter.Asda2Inventory.GetRegularItem(slotInq);
            if (regularItem == null || regularItem.Category != Asda2ItemCategory.Token)
                return;
            if (amount > regularItem.Amount)
            {
                client.ActiveCharacter.YouAreFuckingCheater("Incorrect item amount", 1);
            }
            else
            {
                switch (regularItem.ItemId)
                {
                    case 33418:
                        AchievementProgressRecord progressRecord1 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(23U);
                        progressRecord1.Counter += (uint) amount;
                        if (progressRecord1.Counter > 1U)
                            client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Wolf155);
                        if (progressRecord1.Counter > 100U)
                            client.ActiveCharacter.GetTitle(Asda2TitleId.Wolf155);
                        progressRecord1.SaveAndFlush();
                        break;
                    case 33419:
                        AchievementProgressRecord progressRecord2 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(24U);
                        progressRecord2.Counter += (uint) amount;
                        if (progressRecord2.Counter > 1U)
                            client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Parasol156);
                        if (progressRecord2.Counter > 100U)
                            client.ActiveCharacter.GetTitle(Asda2TitleId.Parasol156);
                        progressRecord2.SaveAndFlush();
                        break;
                    case 33420:
                        AchievementProgressRecord progressRecord3 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(25U);
                        progressRecord3.Counter += (uint) amount;
                        if (progressRecord3.Counter > 1U)
                            client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Crab157);
                        if (progressRecord3.Counter > 100U)
                            client.ActiveCharacter.GetTitle(Asda2TitleId.Crab157);
                        progressRecord3.SaveAndFlush();
                        break;
                    case 33421:
                        AchievementProgressRecord progressRecord4 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(26U);
                        progressRecord4.Counter += (uint) amount;
                        if (progressRecord4.Counter > 1U)
                            client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Duck158);
                        if (progressRecord4.Counter > 100U)
                            client.ActiveCharacter.GetTitle(Asda2TitleId.Duck158);
                        progressRecord4.SaveAndFlush();
                        break;
                    case 33422:
                        AchievementProgressRecord progressRecord5 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(27U);
                        progressRecord5.Counter += (uint) amount;
                        if (progressRecord5.Counter > 1U)
                            client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Stamp159);
                        if (progressRecord5.Counter > 100U)
                            client.ActiveCharacter.GetTitle(Asda2TitleId.Stamp159);
                        progressRecord5.SaveAndFlush();
                        break;
                    case 33423:
                        AchievementProgressRecord progressRecord6 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(28U);
                        progressRecord6.Counter += (uint) amount;
                        if (progressRecord6.Counter > 1U)
                            client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Cat160);
                        if (progressRecord6.Counter > 100U)
                            client.ActiveCharacter.GetTitle(Asda2TitleId.Cat160);
                        progressRecord6.SaveAndFlush();
                        break;
                    case 33424:
                        AchievementProgressRecord progressRecord7 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(29U);
                        progressRecord7.Counter += (uint) amount;
                        if (progressRecord7.Counter > 1U)
                            client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Mushroom161);
                        if (progressRecord7.Counter > 100U)
                            client.ActiveCharacter.GetTitle(Asda2TitleId.Mushroom161);
                        progressRecord7.SaveAndFlush();
                        break;
                    case 33425:
                        AchievementProgressRecord progressRecord8 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(30U);
                        progressRecord8.Counter += (uint) amount;
                        if (progressRecord8.Counter > 1U)
                            client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Pickle162);
                        if (progressRecord8.Counter > 100U)
                            client.ActiveCharacter.GetTitle(Asda2TitleId.Pickle162);
                        progressRecord8.SaveAndFlush();
                        break;
                    case 33426:
                        AchievementProgressRecord progressRecord9 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(31U);
                        progressRecord9.Counter += (uint) amount;
                        if (progressRecord9.Counter > 1U)
                            client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Woodfolk163);
                        if (progressRecord9.Counter > 100U)
                            client.ActiveCharacter.GetTitle(Asda2TitleId.Woodfolk163);
                        progressRecord9.SaveAndFlush();
                        break;
                    case 33427:
                        AchievementProgressRecord progressRecord10 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(32U);
                        progressRecord10.Counter += (uint) amount;
                        if (progressRecord10.Counter > 1U)
                            client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Parrot164);
                        if (progressRecord10.Counter > 100U)
                            client.ActiveCharacter.GetTitle(Asda2TitleId.Parrot164);
                        progressRecord10.SaveAndFlush();
                        break;
                    case 33428:
                        AchievementProgressRecord progressRecord11 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(33U);
                        progressRecord11.Counter += (uint) amount;
                        if (progressRecord11.Counter > 1U)
                            client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Rat165);
                        if (progressRecord11.Counter > 100U)
                            client.ActiveCharacter.GetTitle(Asda2TitleId.Rat165);
                        progressRecord11.SaveAndFlush();
                        break;
                    case 33429:
                        AchievementProgressRecord progressRecord12 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(34U);
                        progressRecord12.Counter += (uint) amount;
                        if (progressRecord12.Counter > 1U)
                            client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Golem167);
                        if (progressRecord12.Counter > 100U)
                            client.ActiveCharacter.GetTitle(Asda2TitleId.Golem167);
                        progressRecord12.SaveAndFlush();
                        break;
                    case 33430:
                        AchievementProgressRecord progressRecord13 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(35U);
                        progressRecord13.Counter += (uint) amount;
                        if (progressRecord13.Counter > 1U)
                            client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Junkman168);
                        if (progressRecord13.Counter > 100U)
                            client.ActiveCharacter.GetTitle(Asda2TitleId.Junkman168);
                        progressRecord13.SaveAndFlush();
                        break;
                    case 33431:
                        AchievementProgressRecord progressRecord14 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(36U);
                        progressRecord14.Counter += (uint) amount;
                        if (progressRecord14.Counter > 1U)
                            client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Slime169);
                        if (progressRecord14.Counter > 100U)
                            client.ActiveCharacter.GetTitle(Asda2TitleId.Slime169);
                        progressRecord14.SaveAndFlush();
                        break;
                    case 33432:
                        AchievementProgressRecord progressRecord15 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(37U);
                        progressRecord15.Counter += (uint) amount;
                        if (progressRecord15.Counter > 1U)
                            client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Scorpion170);
                        if (progressRecord15.Counter > 100U)
                            client.ActiveCharacter.GetTitle(Asda2TitleId.Scorpion170);
                        progressRecord15.SaveAndFlush();
                        break;
                    case 33433:
                        AchievementProgressRecord progressRecord16 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(38U);
                        progressRecord16.Counter += (uint) amount;
                        if (progressRecord16.Counter > 1U)
                            client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Gnom171);
                        if (progressRecord16.Counter > 100U)
                            client.ActiveCharacter.GetTitle(Asda2TitleId.Gnom171);
                        progressRecord16.SaveAndFlush();
                        break;
                    case 33434:
                        AchievementProgressRecord progressRecord17 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(39U);
                        progressRecord17.Counter += (uint) amount;
                        if (progressRecord17.Counter > 1U)
                            client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Lizard172);
                        if (progressRecord17.Counter > 100U)
                            client.ActiveCharacter.GetTitle(Asda2TitleId.Lizard172);
                        progressRecord17.SaveAndFlush();
                        break;
                    case 33435:
                        AchievementProgressRecord progressRecord18 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(40U);
                        progressRecord18.Counter += (uint) amount;
                        if (progressRecord18.Counter > 1U)
                            client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Serpent173);
                        if (progressRecord18.Counter > 100U)
                            client.ActiveCharacter.GetTitle(Asda2TitleId.Serpent173);
                        progressRecord18.SaveAndFlush();
                        break;
                    case 33436:
                        AchievementProgressRecord progressRecord19 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(41U);
                        progressRecord19.Counter += (uint) amount;
                        if (progressRecord19.Counter > 1U)
                            client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Pawn174);
                        if (progressRecord19.Counter > 100U)
                            client.ActiveCharacter.GetTitle(Asda2TitleId.Pawn174);
                        progressRecord19.SaveAndFlush();
                        break;
                    case 33437:
                        AchievementProgressRecord progressRecord20 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(42U);
                        progressRecord20.Counter += (uint) amount;
                        if (progressRecord20.Counter > 1U)
                            client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Rook175);
                        if (progressRecord20.Counter > 100U)
                            client.ActiveCharacter.GetTitle(Asda2TitleId.Rook175);
                        progressRecord20.SaveAndFlush();
                        break;
                    case 33438:
                        AchievementProgressRecord progressRecord21 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(43U);
                        progressRecord21.Counter += (uint) amount;
                        if (progressRecord21.Counter > 1U)
                            client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Bishop176);
                        if (progressRecord21.Counter > 100U)
                            client.ActiveCharacter.GetTitle(Asda2TitleId.Bishop176);
                        progressRecord21.SaveAndFlush();
                        break;
                    case 33439:
                        AchievementProgressRecord progressRecord22 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(44U);
                        progressRecord22.Counter += (uint) amount;
                        if (progressRecord22.Counter > 1U)
                            client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Knight177);
                        if (progressRecord22.Counter > 100U)
                            client.ActiveCharacter.GetTitle(Asda2TitleId.Knight177);
                        progressRecord22.SaveAndFlush();
                        break;
                    case 33440:
                        AchievementProgressRecord progressRecord23 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(45U);
                        progressRecord23.Counter += (uint) amount;
                        if (progressRecord23.Counter > 1U)
                            client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Bugly179);
                        if (progressRecord23.Counter > 100U)
                            client.ActiveCharacter.GetTitle(Asda2TitleId.Bugly179);
                        progressRecord23.SaveAndFlush();
                        break;
                    case 33441:
                        AchievementProgressRecord progressRecord24 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(46U);
                        progressRecord24.Counter += (uint) amount;
                        if (progressRecord24.Counter > 1U)
                            client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Mutant180);
                        if (progressRecord24.Counter > 100U)
                            client.ActiveCharacter.GetTitle(Asda2TitleId.Mutant180);
                        progressRecord24.SaveAndFlush();
                        break;
                    case 33442:
                        AchievementProgressRecord progressRecord25 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(47U);
                        progressRecord25.Counter += (uint) amount;
                        if (progressRecord25.Counter > 1U)
                            client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Deckron181);
                        if (progressRecord25.Counter > 100U)
                            client.ActiveCharacter.GetTitle(Asda2TitleId.Deckron181);
                        progressRecord25.SaveAndFlush();
                        break;
                    case 33443:
                        AchievementProgressRecord progressRecord26 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(48U);
                        progressRecord26.Counter += (uint) amount;
                        if (progressRecord26.Counter > 1U)
                            client.ActiveCharacter.DiscoverTitle(Asda2TitleId.River182);
                        if (progressRecord26.Counter > 100U)
                            client.ActiveCharacter.GetTitle(Asda2TitleId.River182);
                        progressRecord26.SaveAndFlush();
                        break;
                    case 33444:
                        AchievementProgressRecord progressRecord27 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(49U);
                        progressRecord27.Counter += (uint) amount;
                        if (progressRecord27.Counter > 1U)
                            client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Spring183);
                        if (progressRecord27.Counter > 100U)
                            client.ActiveCharacter.GetTitle(Asda2TitleId.Spring183);
                        progressRecord27.SaveAndFlush();
                        break;
                    case 33445:
                        AchievementProgressRecord progressRecord28 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(50U);
                        progressRecord28.Counter += (uint) amount;
                        if (progressRecord28.Counter > 1U)
                            client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Jungle184);
                        if (progressRecord28.Counter > 100U)
                            client.ActiveCharacter.GetTitle(Asda2TitleId.Jungle184);
                        progressRecord28.SaveAndFlush();
                        break;
                    case 33446:
                        AchievementProgressRecord progressRecord29 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(51U);
                        progressRecord29.Counter += (uint) amount;
                        if (progressRecord29.Counter > 1U)
                            client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Coast185);
                        if (progressRecord29.Counter > 100U)
                            client.ActiveCharacter.GetTitle(Asda2TitleId.Coast185);
                        progressRecord29.SaveAndFlush();
                        break;
                    case 33447:
                        AchievementProgressRecord progressRecord30 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(52U);
                        progressRecord30.Counter += (uint) amount;
                        if (progressRecord30.Counter > 1U)
                            client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Desert186);
                        if (progressRecord30.Counter > 100U)
                            client.ActiveCharacter.GetTitle(Asda2TitleId.Desert186);
                        progressRecord30.SaveAndFlush();
                        break;
                    case 33448:
                        AchievementProgressRecord progressRecord31 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(53U);
                        progressRecord31.Counter += (uint) amount;
                        if (progressRecord31.Counter > 1U)
                            client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Tyrant187);
                        if (progressRecord31.Counter > 100U)
                            client.ActiveCharacter.GetTitle(Asda2TitleId.Tyrant187);
                        progressRecord31.SaveAndFlush();
                        break;
                    case 33449:
                        AchievementProgressRecord progressRecord32 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(54U);
                        progressRecord32.Counter += (uint) amount;
                        if (progressRecord32.Counter > 1U)
                            client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Queen191);
                        if (progressRecord32.Counter > 100U)
                            client.ActiveCharacter.GetTitle(Asda2TitleId.Queen191);
                        progressRecord32.SaveAndFlush();
                        break;
                    case 33450:
                        AchievementProgressRecord progressRecord33 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(55U);
                        progressRecord33.Counter += (uint) amount;
                        if (progressRecord33.Counter > 1U)
                            client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Night192);
                        if (progressRecord33.Counter > 100U)
                            client.ActiveCharacter.GetTitle(Asda2TitleId.Night192);
                        progressRecord33.SaveAndFlush();
                        break;
                    case 33451:
                        AchievementProgressRecord progressRecord34 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(56U);
                        progressRecord34.Counter += (uint) amount;
                        if (progressRecord34.Counter > 1U)
                            client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Volcano193);
                        if (progressRecord34.Counter > 100U)
                            client.ActiveCharacter.GetTitle(Asda2TitleId.Volcano193);
                        progressRecord34.SaveAndFlush();
                        break;
                    case 33452:
                        AchievementProgressRecord progressRecord35 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(57U);
                        progressRecord35.Counter += (uint) amount;
                        if (progressRecord35.Counter > 1U)
                            client.ActiveCharacter.DiscoverTitle(Asda2TitleId.King194);
                        if (progressRecord35.Counter > 100U)
                            client.ActiveCharacter.GetTitle(Asda2TitleId.King194);
                        progressRecord35.SaveAndFlush();
                        break;
                    case 33453:
                        AchievementProgressRecord progressRecord36 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(58U);
                        progressRecord36.Counter += (uint) amount;
                        if (progressRecord36.Counter > 1U)
                            client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Sphinx195);
                        if (progressRecord36.Counter > 100U)
                            client.ActiveCharacter.GetTitle(Asda2TitleId.Sphinx195);
                        progressRecord36.SaveAndFlush();
                        break;
                    case 33454:
                        AchievementProgressRecord progressRecord37 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(59U);
                        progressRecord37.Counter += (uint) amount;
                        if (progressRecord37.Counter > 1U)
                            client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Dragon196);
                        if (progressRecord37.Counter > 100U)
                            client.ActiveCharacter.GetTitle(Asda2TitleId.Dragon196);
                        progressRecord37.SaveAndFlush();
                        break;
                    case 33455:
                        AchievementProgressRecord progressRecord38 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(60U);
                        progressRecord38.Counter += (uint) amount;
                        if (progressRecord38.Counter > 1U)
                            client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Reserved197);
                        if (progressRecord38.Counter > 100U)
                            client.ActiveCharacter.GetTitle(Asda2TitleId.Reserved197);
                        progressRecord38.SaveAndFlush();
                        break;
                    case 33456:
                        AchievementProgressRecord progressRecord39 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(61U);
                        progressRecord39.Counter += (uint) amount;
                        if (progressRecord39.Counter > 1U)
                            client.ActiveCharacter.DiscoverTitle(Asda2TitleId.U188);
                        if (progressRecord39.Counter > 100U)
                            client.ActiveCharacter.GetTitle(Asda2TitleId.U188);
                        progressRecord39.SaveAndFlush();
                        break;
                    case 33457:
                        AchievementProgressRecord progressRecord40 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(62U);
                        progressRecord40.Counter += (uint) amount;
                        if (progressRecord40.Counter > 1U)
                            client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Reserved198);
                        if (progressRecord40.Counter > 100U)
                            client.ActiveCharacter.GetTitle(Asda2TitleId.Reserved198);
                        progressRecord40.SaveAndFlush();
                        break;
                    case 33458:
                        AchievementProgressRecord progressRecord41 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(63U);
                        progressRecord41.Counter += (uint) amount;
                        if (progressRecord41.Counter > 1U)
                            client.ActiveCharacter.DiscoverTitle(Asda2TitleId.U189);
                        if (progressRecord41.Counter > 100U)
                            client.ActiveCharacter.GetTitle(Asda2TitleId.U189);
                        progressRecord41.SaveAndFlush();
                        break;
                    case 33459:
                        AchievementProgressRecord progressRecord42 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(64U);
                        progressRecord42.Counter += (uint) amount;
                        if (progressRecord42.Counter > 1U)
                            client.ActiveCharacter.DiscoverTitle(Asda2TitleId.U190);
                        if (progressRecord42.Counter > 100U)
                            client.ActiveCharacter.GetTitle(Asda2TitleId.U190);
                        progressRecord42.SaveAndFlush();
                        break;
                    case 36966:
                        AchievementProgressRecord progressRecord43 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(172U);
                        progressRecord43.Counter += (uint) amount;
                        if (progressRecord43.Counter > 1U)
                            client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Golem167 | Asda2TitleId.Elite256);
                        if (progressRecord43.Counter > 10U)
                            client.ActiveCharacter.GetTitle(Asda2TitleId.Golem167 | Asda2TitleId.Elite256);
                        progressRecord43.SaveAndFlush();
                        break;
                    case 36967:
                        AchievementProgressRecord progressRecord44 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(173U);
                        progressRecord44.Counter += (uint) amount;
                        if (progressRecord44.Counter > 1U)
                            client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Slime169 | Asda2TitleId.Elite256);
                        if (progressRecord44.Counter > 50U)
                            client.ActiveCharacter.GetTitle(Asda2TitleId.Slime169 | Asda2TitleId.Elite256);
                        progressRecord44.SaveAndFlush();
                        break;
                    case 36968:
                        client.ActiveCharacter.GetTitle(Asda2TitleId.Junkman168 | Asda2TitleId.Elite256);
                        break;
                    case 36969:
                        AchievementProgressRecord progressRecord45 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(174U);
                        progressRecord45.Counter += (uint) amount;
                        if (progressRecord45.Counter > 1U)
                            client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Scorpion170 | Asda2TitleId.Elite256);
                        if (progressRecord45.Counter > 50U)
                            client.ActiveCharacter.GetTitle(Asda2TitleId.Scorpion170 | Asda2TitleId.Elite256);
                        progressRecord45.SaveAndFlush();
                        break;
                    case 36982:
                        AchievementProgressRecord progressRecord46 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(180U);
                        progressRecord46.Counter += (uint) amount;
                        if (progressRecord46.Counter > 1U)
                            client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Serpent173 | Asda2TitleId.Elite256);
                        if (progressRecord46.Counter > 25U)
                            client.ActiveCharacter.GetTitle(Asda2TitleId.Serpent173 | Asda2TitleId.Elite256);
                        progressRecord46.SaveAndFlush();
                        break;
                    case 36983:
                        AchievementProgressRecord progressRecord47 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(181U);
                        progressRecord47.Counter += (uint) amount;
                        if (progressRecord47.Counter > 1U)
                            client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Pawn174 | Asda2TitleId.Elite256);
                        if (progressRecord47.Counter > 10U)
                            client.ActiveCharacter.GetTitle(Asda2TitleId.Pawn174 | Asda2TitleId.Elite256);
                        progressRecord47.SaveAndFlush();
                        break;
                    case 36984:
                        AchievementProgressRecord progressRecord48 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(182U);
                        progressRecord48.Counter += (uint) amount;
                        if (progressRecord48.Counter > 1U)
                            client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Rook175 | Asda2TitleId.Elite256);
                        if (progressRecord48.Counter > 25U)
                            client.ActiveCharacter.GetTitle(Asda2TitleId.Rook175 | Asda2TitleId.Elite256);
                        progressRecord48.SaveAndFlush();
                        break;
                    case 36985:
                        AchievementProgressRecord progressRecord49 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(183U);
                        progressRecord49.Counter += (uint) amount;
                        if (progressRecord49.Counter > 1U)
                            client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Bishop176 | Asda2TitleId.Elite256);
                        if (progressRecord49.Counter > 25U)
                            client.ActiveCharacter.GetTitle(Asda2TitleId.Bishop176 | Asda2TitleId.Elite256);
                        progressRecord49.SaveAndFlush();
                        break;
                    case 40600:
                        AchievementProgressRecord progressRecord50 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(175U);
                        progressRecord50.Counter += (uint) amount;
                        if (progressRecord50.Counter > 1U)
                            client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Chess178 | Asda2TitleId.Elite256);
                        if (progressRecord50.Counter > 50U)
                            client.ActiveCharacter.GetTitle(Asda2TitleId.Chess178 | Asda2TitleId.Elite256);
                        progressRecord50.SaveAndFlush();
                        break;
                    case 40601:
                        AchievementProgressRecord progressRecord51 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(178U);
                        progressRecord51.Counter += (uint) amount;
                        if (progressRecord51.Counter > 1U)
                            client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Deckron181 | Asda2TitleId.Elite256);
                        if (progressRecord51.Counter > 25U)
                            client.ActiveCharacter.GetTitle(Asda2TitleId.Deckron181 | Asda2TitleId.Elite256);
                        progressRecord51.SaveAndFlush();
                        break;
                    case 40602:
                        AchievementProgressRecord progressRecord52 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(176U);
                        progressRecord52.Counter += (uint) amount;
                        if (progressRecord52.Counter > 1U)
                            client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Bugly179 | Asda2TitleId.Elite256);
                        if (progressRecord52.Counter > 10U)
                            client.ActiveCharacter.GetTitle(Asda2TitleId.Bugly179 | Asda2TitleId.Elite256);
                        progressRecord52.SaveAndFlush();
                        break;
                    case 40603:
                        AchievementProgressRecord progressRecord53 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(177U);
                        progressRecord53.Counter += (uint) amount;
                        if (progressRecord53.Counter > 1U)
                            client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Mutant180 | Asda2TitleId.Elite256);
                        if (progressRecord53.Counter > 50U)
                            client.ActiveCharacter.GetTitle(Asda2TitleId.Mutant180 | Asda2TitleId.Elite256);
                        progressRecord53.SaveAndFlush();
                        break;
                }

                if (client.ActiveCharacter.isTitleGetted(Asda2TitleId.Wolf155) &&
                    client.ActiveCharacter.isTitleGetted(Asda2TitleId.Duck158) &&
                    (client.ActiveCharacter.isTitleGetted(Asda2TitleId.Pickle162) &&
                     client.ActiveCharacter.isTitleGetted(Asda2TitleId.Mushroom161)) &&
                    client.ActiveCharacter.isTitleGetted(Asda2TitleId.Queen191))
                    client.ActiveCharacter.GetTitle(Asda2TitleId.Monster166);
                if (client.ActiveCharacter.isTitleGetted(Asda2TitleId.Pawn174) &&
                    client.ActiveCharacter.isTitleGetted(Asda2TitleId.Rook175) &&
                    (client.ActiveCharacter.isTitleGetted(Asda2TitleId.Bishop176) &&
                     client.ActiveCharacter.isTitleGetted(Asda2TitleId.Knight177)))
                    client.ActiveCharacter.GetTitle(Asda2TitleId.Chess178);
                if (client.ActiveCharacter.isTitleGetted(Asda2TitleId.Golem167 | Asda2TitleId.Elite256) &&
                    client.ActiveCharacter.isTitleGetted(Asda2TitleId.Junkman168 | Asda2TitleId.Elite256) &&
                    (client.ActiveCharacter.isTitleGetted(Asda2TitleId.Slime169 | Asda2TitleId.Elite256) &&
                     client.ActiveCharacter.isTitleGetted(Asda2TitleId.Scorpion170 | Asda2TitleId.Elite256)))
                    client.ActiveCharacter.GetTitle(Asda2TitleId.Gnom171 | Asda2TitleId.Elite256);
                if (client.ActiveCharacter.isTitleGetted(Asda2TitleId.Chess178 | Asda2TitleId.Elite256) &&
                    client.ActiveCharacter.isTitleGetted(Asda2TitleId.Bugly179 | Asda2TitleId.Elite256) &&
                    (client.ActiveCharacter.isTitleGetted(Asda2TitleId.Mutant180 | Asda2TitleId.Elite256) &&
                     client.ActiveCharacter.isTitleGetted(Asda2TitleId.Deckron181 | Asda2TitleId.Elite256)))
                    client.ActiveCharacter.GetTitle(Asda2TitleId.Knight177 | Asda2TitleId.Elite256);
                if (client.ActiveCharacter.isTitleGetted(Asda2TitleId.Serpent173 | Asda2TitleId.Elite256) &&
                    client.ActiveCharacter.isTitleGetted(Asda2TitleId.Pawn174 | Asda2TitleId.Elite256) &&
                    (client.ActiveCharacter.isTitleGetted(Asda2TitleId.Rook175 | Asda2TitleId.Elite256) &&
                     client.ActiveCharacter.isTitleGetted(Asda2TitleId.Bishop176 | Asda2TitleId.Elite256)))
                    client.ActiveCharacter.GetTitle(Asda2TitleId.Lizard172 | Asda2TitleId.Elite256);
                Asda2CharacterHandler.CommisProdResponse(client, regularItem, amount);
            }
        }

        public static void CommisProdResponse(IRealmClient client, Asda2Item item, int amount)
        {
            using (RealmPacketOut packet =
                new RealmPacketOut(RealmServerOpCode.PetLevelLimitBreaked | RealmServerOpCode.CMSG_DBLOOKUP))
            {
                if (amount <= item.Amount)
                {
                    if (item.Amount - amount <= 0)
                    {
                        packet.WriteInt32(client.ActiveCharacter.AccId);
                        packet.WriteInt32(item.ItemId);
                        packet.WriteByte((byte) item.InventoryType);
                        packet.WriteInt32(item.Slot);
                        packet.WriteInt32(0);
                        packet.WriteByte(0);
                        packet.WriteByte(10);
                        packet.WriteSkip(Asda2CharacterHandler.stab15);
                        item.Amount -= amount;
                    }
                    else
                    {
                        item.Amount -= amount;
                        packet.WriteInt32(client.ActiveCharacter.AccId);
                        packet.WriteInt32(item.ItemId);
                        packet.WriteByte((byte) item.InventoryType);
                        packet.WriteInt32(item.Slot);
                        packet.WriteInt32(item.Amount);
                        packet.WriteByte(0);
                        packet.WriteByte(10);
                        packet.WriteSkip(Asda2CharacterHandler.stab15);
                    }
                }

                if (item.Amount <= 0)
                    Asda2InventoryHandler.ItemRemovedFromInventoryResponse(client.ActiveCharacter, item,
                        DeleteOrSellItemStatus.Ok, 0);
                client.ActiveCharacter.Send(packet, true);
            }
        }

        public static void SendSummonChar(Character summonerchr, Character targchr)
        {
            using (RealmPacketOut packet = new RealmPacketOut((RealmServerOpCode) 6136))
            {
                targchr.CanTeleportToFriend = true;
                targchr.TargetSummonMap = summonerchr.Map.MapId;
                targchr.TargetSummonPosition = summonerchr.Position;
                packet.WriteInt32(summonerchr.AccId);
                packet.WriteInt16(summonerchr.SessionId);
                packet.Position += 18L;
                packet.WriteFixedAsciiString(summonerchr.Name, 20, Locale.Start);
                packet.WriteByte(0);
                if (targchr.Client.AddrTemp.Contains("192.168."))
                    packet.WriteFixedAsciiString(RealmServerConfiguration.ExternalAddress, 16, Locale.Start);
                else
                    packet.WriteFixedAsciiString(RealmServerConfiguration.RealExternalAddress, 16, Locale.Start);
                packet.WriteInt16(ServerApp<WCell.RealmServer.RealmServer>.Instance.Port);
                targchr.Send(packet, false);
            }
        }

        [PacketHandler(RealmServerOpCode.UpdatePetExp | RealmServerOpCode.CMSG_UNDRESSPLAYER)]
        public static void SummonStatusRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position += 16;
            byte num = packet.ReadByte();
            if (num == (byte) 1 && client.ActiveCharacter.CanTeleportToFriend)
            {
                client.ActiveCharacter.CanTeleportToFriend = false;
                client.ActiveCharacter.TeleportTo(client.ActiveCharacter.TargetSummonMap,
                    client.ActiveCharacter.TargetSummonPosition);
            }

            if (num != (byte) 10 || !client.ActiveCharacter.CanTeleportToFriend)
                return;
            client.ActiveCharacter.CanTeleportToFriend = false;
        }

        public static void SendRates(Character chr, int drop, int exp)
        {
            if (chr == null)
                return;
            using (RealmPacketOut packet = new RealmPacketOut((RealmServerOpCode) 4301))
            {
                packet.WriteByte(1);
                packet.WriteInt32(drop * 100);
                packet.WriteInt32(exp * 100);
                chr.Send(packet, false);
            }
        }

        [PacketHandler(RealmServerOpCode.RepairItem)]
        public static void RepairItemRequest(IRealmClient client, RealmPacketIn packet)
        {
            byte[] numArray1 = new byte[82];
            short[] numArray2 = new short[82];
            List<Asda2Item> repairedItems = new List<Asda2Item>();
            for (int index = 0; index < 80; ++index)
                numArray1[index] = packet.ReadByte();
            short slotInq1 = packet.ReadInt16();
            for (int index = 0; index < 79; ++index)
                numArray2[index] = (short) ((int) packet.ReadInt16() - 1);
            if (numArray1[0] != (byte) 3 && numArray2[0] < (short) 0)
            {
                if (numArray1[0] == (byte) 0)
                {
                    Asda2Item asda2Item = (Asda2Item) null;
                    if (slotInq1 >= (short) 0 && slotInq1 < (short) 22)
                        asda2Item = client.ActiveCharacter.Asda2Inventory.Equipment[(int) slotInq1];
                    if (asda2Item != null && (int) asda2Item.Durability < (int) asda2Item.MaxDurability)
                        repairedItems.Add(asda2Item);
                }
                else
                {
                    Asda2Item shopShopItem = client.ActiveCharacter.Asda2Inventory.GetShopShopItem(slotInq1);
                    if (shopShopItem != null && (int) shopShopItem.Durability < (int) shopShopItem.MaxDurability)
                        repairedItems.Add(shopShopItem);
                }
            }
            else if (numArray1[0] != (byte) 3)
            {
                for (int index = 0; index < 21; ++index)
                {
                    Asda2Item asda2Item = client.ActiveCharacter.Asda2Inventory.Equipment[index];
                    if (asda2Item != null && (int) asda2Item.Durability < (int) asda2Item.MaxDurability)
                        repairedItems.Add(asda2Item);
                }
            }
            else
            {
                for (int index = 0; index < 21; ++index)
                {
                    Asda2Item asda2Item = client.ActiveCharacter.Asda2Inventory.Equipment[index];
                    if (asda2Item != null && (int) asda2Item.Durability < (int) asda2Item.MaxDurability)
                        repairedItems.Add(asda2Item);
                }

                for (short slotInq2 = 0; slotInq2 < (short) 60; ++slotInq2)
                {
                    Asda2Item shopShopItem = client.ActiveCharacter.Asda2Inventory.GetShopShopItem(slotInq2);
                    if (shopShopItem != null && (int) shopShopItem.Durability < (int) shopShopItem.MaxDurability)
                        repairedItems.Add(shopShopItem);
                }
            }

            uint amount = 0;
            foreach (Asda2Item asda2Item in repairedItems)
                amount += asda2Item == null ? 0U : asda2Item.RepairCost();
            client.ActiveCharacter.SendInfoMsg(string.Format("Total repair cost is {0}.", (object) amount));
            if (!client.ActiveCharacter.SubtractMoney(amount))
            {
                Asda2CharacterHandler.SendRepairItemResponseResponse(client, Asda2CharacterHandler.RepairStatus.Fail,
                    repairedItems);
            }
            else
            {
                foreach (Asda2Item asda2Item in repairedItems)
                {
                    if (asda2Item != null)
                        asda2Item.RepairDurability();
                }

                Asda2CharacterHandler.SendRepairItemResponseResponse(client, Asda2CharacterHandler.RepairStatus.Ok,
                    repairedItems);
                client.ActiveCharacter.SendMoneyUpdate();
            }
        }

        public static void SendRepairItemResponseResponse(IRealmClient client,
            Asda2CharacterHandler.RepairStatus status, List<Asda2Item> repairedItems)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.RepairItemResponse))
            {
                packet.WriteInt16((byte) status);
                for (int index = 0; index < 82; ++index)
                {
                    byte val = 0;
                    packet.WriteByte(val);
                }

                for (int index = 0; index < 79; ++index)
                {
                    short val = -1;
                    packet.WriteInt16(val);
                }

                for (int index = 0; index < 80; ++index)
                {
                    byte val = 0;
                    packet.WriteByte(val);
                }

                packet.WriteInt32(client.ActiveCharacter.Money);
                client.Send(packet, true);
            }

            foreach (Asda2Item repairedItem in repairedItems)
                Asda2CharacterHandler.SendUpdateDurabilityResponse(client, repairedItem);
        }

        public static void ChangeLocationResponse(Character chr, MapId map, short x, short y)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.ResurectWithChangeLocation))
            {
                packet.WriteInt16(chr.SessionId);
                packet.WriteByte((byte) map);
                if (chr.Client.AddrTemp.Contains("192.168."))
                    packet.WriteFixedAsciiString(RealmServerConfiguration.ExternalAddress, 20, Locale.Start);
                else
                    packet.WriteFixedAsciiString(RealmServerConfiguration.RealExternalAddress, 20, Locale.Start);
                packet.WriteInt16(ServerApp<WCell.RealmServer.RealmServer>.Instance.Port);
                packet.WriteInt16(x);
                packet.WriteInt16(y);
                packet.WriteInt32(chr.Health);
                packet.WriteInt16(chr.Power);
                packet.WriteSkip(Asda2CharacterHandler.stub34);
                chr.Send(packet, false);
            }
        }

        public static void SendExpGainedResponse(ushort npcId, Character chr, int xp, bool fromKillNpc = true)
        {
            if (xp == 0)
                return;
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.ExpGained))
            {
                packet.WriteByte(fromKillNpc ? 0 : 1);
                packet.WriteInt64((long) (chr.Experience + XpGenerator.GetStartXpForLevel(chr.Level)));
                packet.WriteInt64((long) xp);
                packet.WriteInt16(npcId);
                chr.Send(packet, true);
            }
        }

        public static void SendLvlUpResponse(Character chr)
        {
            if (chr.Level == 2)
                chr.DiscoverTitle(Asda2TitleId.Novice0);
            if (chr.Level == 5)
                chr.GetTitle(Asda2TitleId.Novice0);
            if (chr.Level == 10)
                chr.DiscoverTitle(Asda2TitleId.Amateur1);
            if (chr.Level == 20)
                chr.GetTitle(Asda2TitleId.Amateur1);
            if (chr.Level == 30)
                chr.DiscoverTitle(Asda2TitleId.Intermediate2);
            if (chr.Level == 40)
                chr.GetTitle(Asda2TitleId.Intermediate2);
            if (chr.Level == 50)
                chr.DiscoverTitle(Asda2TitleId.Trained3);
            if (chr.Level == 60)
                chr.GetTitle(Asda2TitleId.Trained3);
            if (chr.Level == 70)
                chr.DiscoverTitle(Asda2TitleId.Expert4);
            if (chr.Level == 80)
                chr.GetTitle(Asda2TitleId.Expert4);
            if (chr.Level == 85)
                chr.DiscoverTitle(Asda2TitleId.Lv905);
            if (chr.Level == 90)
                chr.GetTitle(Asda2TitleId.Lv905);
            if (chr.Level == 95)
                chr.DiscoverTitle(Asda2TitleId.Lv996);
            if (chr.Level == 99)
                chr.GetTitle(Asda2TitleId.Lv996);
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.LvlUp))
            {
                packet.WriteInt16(chr.SessionId);
                packet.WriteByte(chr.Level);
                packet.WriteInt64((long) (chr.Experience + XpGenerator.GetStartXpForLevel(chr.Level)));
                packet.WriteInt16(0);
                packet.WriteInt16(chr.Asda2Strength);
                packet.WriteInt16(chr.Asda2Agility);
                packet.WriteInt16(chr.Asda2Stamina);
                packet.WriteInt16(chr.Asda2Spirit);
                packet.WriteInt16(chr.Asda2Intellect);
                packet.WriteInt16(chr.Asda2Luck);
                packet.WriteInt16(0);
                packet.WriteInt16(0);
                packet.WriteInt16(0);
                packet.WriteInt16(0);
                packet.WriteInt16(0);
                packet.WriteInt16(0);
                packet.WriteInt16(chr.Asda2Strength);
                packet.WriteInt16(chr.Asda2Agility);
                packet.WriteInt16(chr.Asda2Stamina);
                packet.WriteInt16(chr.Asda2Spirit);
                packet.WriteInt16(chr.Asda2Intellect);
                packet.WriteInt16(chr.Asda2Luck);
                packet.WriteInt32(chr.MaxHealth);
                packet.WriteInt16(chr.MaxPower);
                packet.WriteInt32(chr.Health);
                packet.WriteInt16(chr.Power);
                packet.WriteInt16((short) chr.MinDamage);
                packet.WriteInt16((short) chr.MaxDamage);
                packet.WriteInt16(chr.MinMagicDamage);
                packet.WriteInt16(chr.MaxMagicDamage);
                packet.WriteInt32(chr.MagicDefence);
                packet.WriteSkip(Asda2CharacterHandler.Stub80);
                chr.SendPacketToArea(packet, true, true, Locale.Any, new float?());
            }
        }

        public static void SendHealthUpdate(Character chr, bool animate = false, bool animateGreenDights = false)
        {
            if (chr == null || chr.Map == null || (chr.IsDeleted || !chr.Client.IsGameServerConnection))
                return;
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.CharHpUpdate))
            {
                packet.WriteInt16(chr.SessionId);
                packet.WriteInt32(chr.MaxHealth);
                packet.WriteInt32(chr.Health);
                packet.WriteByte(animate ? 4 : (animateGreenDights ? 1 : 0));
                packet.WriteInt16(animate ? 276 : -1);
                packet.WriteInt32(chr.RegenHealth);
                chr.SendPacketToArea(packet, true, true, Locale.Any, new float?());
            }
        }

        public static void SendCharMpUpdateResponse(Character chr)
        {
            if (chr == null || chr.Map == null || (chr.IsDeleted || !chr.Client.IsGameServerConnection))
                return;
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.CharMpUpdate))
            {
                packet.WriteInt16(chr.SessionId);
                packet.WriteInt16(chr.MaxPower);
                packet.WriteInt16(chr.Power);
                packet.WriteByte(0);
                packet.WriteInt16(-1);
                packet.WriteInt16(chr.RegenMana);
                chr.Send(packet, true);
            }
        }

        public static void SendPowerUpdate(Unit unit, PowerType type, int value)
        {
            Character chr = unit as Character;
            if (type == PowerType.Health)
            {
                if (chr == null)
                    return;
                Asda2CharacterHandler.SendHealthUpdate(chr, false, false);
            }
            else
            {
                if (chr == null)
                    return;
                Asda2CharacterHandler.SendCharMpUpdateResponse(chr);
            }
        }

        public static void SendSelfDeathResponse(Character chr)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SelfDeath))
            {
                packet.WriteInt16(chr.SessionId);
                chr.SendPacketToArea(packet, true, true, Locale.Any, new float?());
            }
        }

        [PacketHandler(RealmServerOpCode.ResurectOnDeathPlace)]
        public static void ResurectOnDeathPlaceRequest(IRealmClient client, RealmPacketIn packet)
        {
            byte num = packet.ReadByte();
            if (num == (byte) 8 && client.ActiveCharacter.IsAsda2BattlegroundInProgress)
            {
                client.ActiveCharacter.CurrentBattleGround.TeleportToWar(client.ActiveCharacter);
                client.ActiveCharacter.Resurrect();
            }
            else
            {
                if (num != (byte) 0 && (client.ActiveCharacter.Level > 20 || client.ActiveCharacter.IsAlive) &&
                    !Asda2CharacterHandler.CheckResurectOnDeathPlaceAuraExists(client.ActiveCharacter))
                    return;
                if (num == (byte) 0)
                    client.ActiveCharacter.Map.CallDelayed(250,
                        (Action) (() => client.ActiveCharacter.TeleportToBindLocation()));
                client.ActiveCharacter.Resurrect();
            }
        }

        private static bool CheckResurectOnDeathPlaceAuraExists(Character activeCharacter)
        {
            foreach (Aura visibleAura in activeCharacter.Auras.VisibleAuras)
            {
                if (visibleAura != null &&
                    (visibleAura.Spell.RealId == (short) 189 || visibleAura.Spell.RealId == (short) 190))
                    return true;
            }

            return false;
        }

        public static void SendResurectResponse(Character chr)
        {
            Asda2CharacterHandler.SendPreResurectResponse(chr);
            chr.Map.CallDelayed(50, (Action) (() =>
            {
                using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.Resurect))
                {
                    packet.WriteInt16(chr.SessionId);
                    packet.WriteInt16((short) chr.Asda2X);
                    packet.WriteInt16((short) chr.Asda2Y);
                    packet.WriteInt32(chr.Health);
                    packet.WriteInt16(chr.Power);
                    packet.WriteUInt64(chr.Experience + XpGenerator.GetStartXpForLevel(chr.Level));
                    packet.WriteInt16(0);
                    chr.SendPacketToArea(packet, true, true, Locale.Any, new float?());
                }
            }));
        }

        public static void SendPreResurectResponse(Character chr)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.PreResurect))
                chr.Send(packet, true);
        }

        public static void SendUpdateStatsResponse(IRealmClient client)
        {
            if (client == null || !client.IsGameServerConnection || client.ActiveCharacter == null)
                return;
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.UpdateStats))
            {
                packet.WriteInt32(client.ActiveCharacter.MaxHealth);
                packet.WriteInt16(Asda2CharacterHandler.ProcessOwerFlow(client.ActiveCharacter.MaxPower));
                packet.WriteInt32(client.ActiveCharacter.Health);
                packet.WriteInt16(Asda2CharacterHandler.ProcessOwerFlow(client.ActiveCharacter.Power));
                packet.WriteInt16(Asda2CharacterHandler.ProcessOwerFlow((int) client.ActiveCharacter.MinDamage));
                packet.WriteInt16(Asda2CharacterHandler.ProcessOwerFlow((int) client.ActiveCharacter.MaxDamage));
                packet.WriteInt16(Asda2CharacterHandler.ProcessOwerFlow(client.ActiveCharacter.MinMagicDamage));
                packet.WriteInt16(Asda2CharacterHandler.ProcessOwerFlow(client.ActiveCharacter.MaxMagicDamage));
                packet.WriteInt16(
                    Asda2CharacterHandler.ProcessOwerFlow((int) client.ActiveCharacter.Asda2MagicDefence));
                packet.WriteInt16(Asda2CharacterHandler.ProcessOwerFlow((int) client.ActiveCharacter.Asda2Defence));
                packet.WriteInt16(Asda2CharacterHandler.ProcessOwerFlow((int) client.ActiveCharacter.Asda2Defence));
                packet.WriteInt32((int) client.ActiveCharacter.BlockChance);
                packet.WriteInt32(client.ActiveCharacter.BlockValue);
                packet.WriteInt16(15);
                packet.WriteInt16(7);
                packet.WriteInt16(4);
                packet.WriteSkip(Asda2CharacterHandler.stub40);
                client.Send(packet, true);
            }
        }

        public static void SendUpdateStatsOneResponse(IRealmClient client)
        {
            if (client == null || !client.IsGameServerConnection || client.ActiveCharacter == null)
                return;
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.UpdateStatsOne))
            {
                packet.WriteInt16(Asda2CharacterHandler.ProcessOwerFlow(client.ActiveCharacter.Asda2Strength));
                packet.WriteInt16(Asda2CharacterHandler.ProcessOwerFlow(client.ActiveCharacter.Asda2Agility));
                packet.WriteInt16(Asda2CharacterHandler.ProcessOwerFlow(client.ActiveCharacter.Asda2Stamina));
                packet.WriteInt16(Asda2CharacterHandler.ProcessOwerFlow(client.ActiveCharacter.Asda2Spirit));
                packet.WriteInt16(Asda2CharacterHandler.ProcessOwerFlow(client.ActiveCharacter.Asda2Intellect));
                packet.WriteInt16(Asda2CharacterHandler.ProcessOwerFlow(client.ActiveCharacter.Asda2Luck));
                packet.WriteInt16(0);
                packet.WriteInt16(0);
                packet.WriteInt16(0);
                packet.WriteInt16(0);
                packet.WriteInt16(0);
                packet.WriteInt16(0);
                packet.WriteInt16(Asda2CharacterHandler.ProcessOwerFlow(client.ActiveCharacter.Asda2Strength));
                packet.WriteInt16(Asda2CharacterHandler.ProcessOwerFlow(client.ActiveCharacter.Asda2Agility));
                packet.WriteInt16(Asda2CharacterHandler.ProcessOwerFlow(client.ActiveCharacter.Asda2Stamina));
                packet.WriteInt16(Asda2CharacterHandler.ProcessOwerFlow(client.ActiveCharacter.Asda2Spirit));
                packet.WriteInt16(Asda2CharacterHandler.ProcessOwerFlow(client.ActiveCharacter.Asda2Intellect));
                packet.WriteInt16(Asda2CharacterHandler.ProcessOwerFlow(client.ActiveCharacter.Asda2Luck));
                client.Send(packet, true);
            }
        }

        public static short ProcessOwerFlow(int value)
        {
            if (value < (int) short.MaxValue)
                return (short) value;
            if (value > 32767000)
                return (short) (value / 1000000);
            return (short) (value / 1000);
        }

        [PacketHandler(RealmServerOpCode.SelectCharacter)]
        public static void SelectCharacterRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.ReadInt32();
            ushort sessId = packet.ReadUInt16();
            Character activeCharacter = client.ActiveCharacter;
            Character characterBySessionId = WCell.RealmServer.Global.World.GetCharacterBySessionId(sessId);
            if (characterBySessionId == null || characterBySessionId.Map != activeCharacter.Map)
                return;
            client.ActiveCharacter.Target = (Unit) characterBySessionId;
            Asda2CharacterHandler.SendSelectCharacterResponse(client, characterBySessionId);
        }

        public static void SendSelectCharacterResponse(IRealmClient client, Character chr)
        {
            using (RealmPacketOut packet = Asda2CharacterHandler.SelectCharacterInfo(chr))
                client.Send(packet, false);
        }

        public static void SendSelectedCharacterInfoToMultipyTargets(Character chr, Character[] targets)
        {
            using (RealmPacketOut packet = Asda2CharacterHandler.SelectCharacterInfo(chr))
            {
                foreach (Character target in targets)
                    target.Send(packet, false);
            }
        }

        public static RealmPacketOut SelectCharacterInfo(Character selectedChr)
        {
            RealmPacketOut realmPacketOut = new RealmPacketOut(RealmServerOpCode.SelectCharacterRespone);
            realmPacketOut.WriteByte(1);
            realmPacketOut.WriteInt32(selectedChr.AccId);
            realmPacketOut.WriteInt32(selectedChr.MaxHealth);
            realmPacketOut.WriteInt32(selectedChr.Health);
            realmPacketOut.WriteInt16(selectedChr.MaxPower);
            realmPacketOut.WriteInt16(selectedChr.Power);
            realmPacketOut.WriteInt32(0);
            return realmPacketOut;
        }

        /// <summary>
        /// Handles an incoming character creation request.
        /// TODO: Add protection against char creation/deletion spam
        /// </summary>
        [PacketHandler(RealmServerOpCode.CreateCharacterRequest, IsGamePacket = false, RequiresLogin = false)]
        public static void CreateCharacterRequest(IRealmClient client, RealmPacketIn packet)
        {
            RealmAccount account = client.Account;
            if (account == null || client.ActiveCharacter != null)
                return;
            int num1 = (int) packet.ReadUInt32();
            packet.Position += 2;
            byte num2 = packet.ReadByte();
            if (num2 < (byte) 10 || num2 > (byte) 12)
            {
                Asda2CharacterHandler.SendCreateCharacterResponse(client, CharecterCreateResult.BadName);
            }
            else
            {
                string str = packet.ReadAsdaString(20, Locale.Start);
                byte num3 = packet.ReadByte();
                byte num4 = packet.ReadByte();
                byte num5 = packet.ReadByte();
                byte num6 = packet.ReadByte();
                byte num7 = packet.ReadByte();
                if (CharacterRecord.Exists(str))
                    Asda2CharacterHandler.SendCreateCharacterResponse(client, CharecterCreateResult.AlreadyInUse);
                else if (!Asda2CharacterHandler.IsNameValid(str) || account.Characters.Count<CharacterRecord>() > 2)
                {
                    Asda2CharacterHandler.SendCreateCharacterResponse(client, CharecterCreateResult.BadName);
                }
                else
                {
                    CharacterRecord newCharacterRecord = CharacterRecord.CreateNewCharacterRecord(client.Account, str);
                    if (newCharacterRecord == null)
                    {
                        Asda2CharacterHandler.SendCreateCharacterResponse(client, CharecterCreateResult.BadName);
                    }
                    else
                    {
                        newCharacterRecord.Gender = (GenderType) num3;
                        newCharacterRecord.Skin = (byte) 0;
                        newCharacterRecord.Face = num6;
                        newCharacterRecord.HairStyle = num4;
                        newCharacterRecord.HairColor = num5;
                        newCharacterRecord.FacialHair = (byte) 0;
                        newCharacterRecord.Outfit = (byte) 0;
                        newCharacterRecord.GodMode = account.Role.AppearAsGM;
                        newCharacterRecord.CharNum = num2;
                        newCharacterRecord.Zodiac = num7;
                        newCharacterRecord.EntityLowId =
                            Character.CharacterIdFromAccIdAndCharNum(account.AccountId, (short) num2);
                        newCharacterRecord.SetupNewRecord(ArchetypeMgr.GetArchetype(RaceId.Human, ClassId.NoClass));
                        ServerApp<WCell.RealmServer.RealmServer>.IOQueue.AddMessage(
                            (IMessage) new Message2<IRealmClient, CharacterRecord>()
                            {
                                Callback = new Action<IRealmClient, CharacterRecord>(Asda2CharacterHandler
                                    .CharCreateCallback),
                                Parameter1 = client,
                                Parameter2 = newCharacterRecord
                            });
                    }
                }
            }
        }

        public static void SendCreateCharacterResponseOneResponse(IRealmClient client, CharacterRecord newCharecter)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.CreateCharacterResponseOne))
            {
                packet.WriteInt32(newCharecter.AccountId);
                packet.WriteAsdaString(newCharecter.Name, 18, Locale.Start);
                packet.WriteInt16(0);
                packet.WriteInt16(newCharecter.CharNum);
                packet.WriteByte(0);
                packet.WriteByte((byte) newCharecter.Gender);
                packet.WriteInt16(0);
                packet.WriteInt64(1L);
                packet.WriteInt32(0);
                packet.WriteInt16(0);
                packet.WriteInt32(131807896);
                packet.WriteInt64(0L);
                packet.WriteByte(0);
                packet.WriteInt32(7683);
                packet.WriteInt16(0);
                packet.WriteByte(0);
                packet.WriteInt16(-1);
                packet.WriteInt16(0);
                packet.WriteByte(newCharecter.Zodiac);
                packet.WriteByte(newCharecter.HairStyle);
                packet.WriteByte(newCharecter.HairColor);
                packet.WriteByte(newCharecter.Face);
                client.Send(packet, false);
            }
        }

        public static void SendCreateCharacterResponse(IRealmClient client, CharecterCreateResult result)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.CreateCharacterResponse))
            {
                packet.WriteByte((byte) result);
                client.Send(packet, false);
            }
        }

        private static void CharCreateCallback(IRealmClient client, CharacterRecord newCharRecord)
        {
            if (CharacterRecord.Exists(newCharRecord.Name))
            {
                Asda2CharacterHandler.SendCreateCharacterResponse(client, CharecterCreateResult.BadName);
            }
            else
            {
                try
                {
                    newCharRecord.CreateAndFlush();
                }
                catch (Exception ex1)
                {
                    try
                    {
                        RealmDBMgr.OnDBError(ex1);
                        newCharRecord.CreateAndFlush();
                    }
                    catch (Exception ex2)
                    {
                        Asda2CharacterHandler.SendCreateCharacterResponse(client, CharecterCreateResult.BadName);
                        return;
                    }
                }

                Asda2CharacterHandler.SendCreateCharacterResponseOneResponse(client, newCharRecord);
                Asda2CharacterHandler.SendCreateCharacterResponse(client, CharecterCreateResult.Ok);
                client.Account.Characters.Add(newCharRecord);
            }
        }

        [PacketHandler(RealmServerOpCode.EnterGameRequset | RealmServerOpCode.CMSG_DBLOOKUP, IsGamePacket = false,
            RequiresLogin = false)]
        public static void DeleteCharacterRequest(IRealmClient client, RealmPacketIn packet)
        {
            int num1 = (int) packet.ReadUInt32();
            byte num2 = packet.ReadByte();
            foreach (CharacterRecord character in client.Account.Characters)
            {
                if ((int) character.CharNum == (int) num2)
                {
                    character.Delete();
                    break;
                }
            }

            Asda2CharacterHandler.SendCharacterDeleteResponse(client, 1);
        }

        public static void SendCharacterDeleteResponse(IRealmClient client, int result)
        {
            using (RealmPacketOut packet =
                new RealmPacketOut(RealmServerOpCode.AuthorizeRequest | RealmServerOpCode.CMSG_LEARN_SPELL))
            {
                packet.WriteByte((byte) result);
                client.Send(packet, false);
            }
        }

        public static void SendSomeInitGSResponse(IRealmClient client)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SomeInitGS))
                client.Send(packet, true);
        }

        public static void SendSomeInitGSOneResponse(IRealmClient client)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SomeInitGSOne))
                client.Send(packet, true);
        }

        public static void SendCharacterInfoSessIdPositionResponse(IRealmClient client)
        {
            Character activeCharacter = client.ActiveCharacter;
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.CharacterInfoSessIdPosition))
            {
                packet.WriteInt16(activeCharacter.SessionId);
                packet.WriteInt16(Convert.ToInt16(activeCharacter.Asda2X));
                packet.WriteInt16(Convert.ToInt16(activeCharacter.Asda2Y));
                packet.WriteInt16(-1);
                packet.WriteByte(client.ActiveCharacter.SettingsFlags[15]);
                packet.WriteByte(client.ActiveCharacter.AvatarMask);
                client.Send(packet, true);
            }
        }

        public static void SendMySessionIdResponse(IRealmClient client)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.GetedTitles))
            {
                packet.WriteInt16(client.ActiveCharacter.SessionId);
                packet.WriteInt32(client.ActiveCharacter.AccId);
                packet.WriteInt16(0);
                packet.WriteSkip(Asda2CharacterHandler.stab14);
                packet.WriteInt16((short) client.ActiveCharacter.Archetype.ClassId);
                packet.WriteSkip(Asda2CharacterHandler.stub13);
                client.Send(packet, true);
            }
        }

        [PacketHandler(RealmServerOpCode.Emote)]
        public static void EmoteRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position -= 6;
            short emote = packet.ReadInt16();
            switch (emote)
            {
                case 108:
                    client.ActiveCharacter.IsSitting = true;
                    break;
                case 109:
                    client.ActiveCharacter.IsSitting = false;
                    break;
                case 131:
                    break;
                default:
                    byte c = packet.ReadByte();
                    float a = packet.ReadSingle();
                    float b = packet.ReadSingle();
                    Asda2CharacterHandler.SendEmoteResponse(client.ActiveCharacter, emote, c, a, b);
                    break;
            }
        }

        public static void SendEmoteResponse(Character chr, short emote, byte c = 1, float a = 0.0596617f,
            float b = -0.9982219f)
        {
            AchievementProgressRecord progressRecord = chr.Achievements.GetOrCreateProgressRecord(4U);
            if (emote == (short) 112 || emote == (short) 113)
            {
                switch (++progressRecord.Counter)
                {
                    case 50:
                        chr.DiscoverTitle(Asda2TitleId.Dancer41);
                        break;
                    case 100:
                        chr.GetTitle(Asda2TitleId.Dancer41);
                        break;
                }
            }

            progressRecord.SaveAndFlush();
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.EmoteResponse))
            {
                packet.WriteInt16(chr.SessionId);
                packet.WriteInt32(chr.AccId);
                packet.WriteInt16(emote);
                packet.WriteByte(c);
                packet.WriteFloat(a);
                packet.WriteFloat(b);
                packet.WriteByte(Asda2CharacterHandler._emoteCnt++);
                chr.SendPacketToArea(packet, false, true, Locale.Any, new float?());
            }
        }

        public static void SendEmoteResponseToOneTarget(Character chr, short emote, byte c, float a, float b,
            IRealmClient rcv)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.EmoteResponse))
            {
                packet.WriteInt16(chr.SessionId);
                packet.WriteInt32(chr.AccId);
                packet.WriteInt16(emote);
                packet.WriteByte(c);
                packet.WriteFloat(a);
                packet.WriteFloat(b);
                packet.WriteByte(Asda2CharacterHandler._emoteCnt++);
                rcv.Send(packet, true);
            }
        }

        public static void SendUpdateAvatarMaskResponse(Character chr)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.EmoteResponse))
            {
                packet.WriteInt16(chr.SessionId);
                packet.WriteInt32(-1);
                packet.WriteInt16(111);
                packet.WriteByte(chr.SettingsFlags[15]);
                packet.WriteInt32(chr.AvatarMask);
                packet.WriteInt32(0);
                chr.SendPacketToArea(packet, false, true, Locale.Any, new float?());
            }
        }

        public static void SendLearnedSkillsInfo(Character character)
        {
            List<List<Spell>> spellListList = new List<List<Spell>>();
            int num = 0;
            bool flag = true;
            List<Spell> spellList1 = (List<Spell>) null;
            foreach (Spell spell in character.Spells)
            {
                if (flag)
                {
                    num = 0;
                    spellList1 = new List<Spell>();
                    spellListList.Add(spellList1);
                    flag = false;
                }

                spellList1.Add(spell);
                ++num;
                if (num >= 18)
                    flag = true;
            }

            foreach (List<Spell> spellList2 in spellListList)
            {
                using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SkillsInfo))
                {
                    foreach (Spell spell in spellList2)
                    {
                        packet.WriteUInt16(spell.RealId);
                        packet.WriteByte(spell.Level);
                        packet.WriteByte(1);
                        packet.WriteInt32(spell.CooldownTime);
                        packet.WriteInt16(256);
                        packet.WriteInt16(spell.PowerCost);
                        packet.WriteInt16(spell.Effect0_MiscValue);
                        packet.WriteByte(100);
                        packet.WriteByte(100);
                        packet.WriteInt16(4);
                        packet.WriteInt32(150000);
                        packet.WriteInt16(0);
                        packet.WriteInt64(0L);
                        packet.WriteInt16(0);
                    }

                    character.Send(packet, false);
                }
            }
        }

        public static void SendFactionAndHonorPointsInitResponse(IRealmClient client)
        {
            if (!client.IsGameServerConnection)
                return;
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.FactionAndHonorPointsInit))
            {
                packet.WriteInt16(client.ActiveCharacter.Asda2FactionId);
                packet.WriteInt32(client.ActiveCharacter.Asda2HonorPoints);
                packet.WriteByte(client.ActiveCharacter.Asda2FactionRank);
                client.Send(packet, false);
            }
        }

        public static void SendChangeProfessionResponse(IRealmClient client)
        {
            Character activeCharacter = client.ActiveCharacter;
            if (activeCharacter.Spells == null)
                return;
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.ChangeProfession))
            {
                packet.WriteSkip(Asda2CharacterHandler.stab6);
                packet.WriteInt32(activeCharacter.Account.AccountId);
                packet.WriteByte(2);
                packet.WriteInt16(32);
                packet.WriteInt16(0);
                packet.WriteByte(3);
                packet.WriteByte(activeCharacter.ProfessionLevel);
                packet.WriteByte((byte) activeCharacter.Archetype.ClassId);
                packet.WriteByte(activeCharacter.Spells.AvalibleSkillPoints < 0
                    ? 0
                    : activeCharacter.Spells.AvalibleSkillPoints);
                packet.WriteByte(0);
                packet.WriteInt32(2475);
                packet.WriteInt16(260);
                packet.WriteInt32(0);
                packet.WriteSkip(Asda2CharacterHandler.stab31);
                packet.WriteInt32(activeCharacter.Money);
                packet.WriteSkip(Asda2CharacterHandler.stab501);
                activeCharacter.Send(packet, false);
            }
        }

        [PacketHandler(RealmServerOpCode.IHaveLearnedTutorial)]
        public static void IHaveLearnedTutorialRequest(IRealmClient client, RealmPacketIn packet)
        {
        }

        [PacketHandler(RealmServerOpCode.SettingsFlags)]
        public static void SettingsFlagsRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position -= 12;
            byte[] numArray = new byte[16];
            for (int index = 0; index < 16; ++index)
            {
                byte num = packet.ReadByte();
                numArray[index] = num;
            }

            client.ActiveCharacter.SettingsFlags = numArray;
            client.ActiveCharacter.AvatarMask = packet.ReadInt32();
        }

        [PacketHandler(RealmServerOpCode.SelectFactionReq)]
        public static void SelectFactionReqRequest(IRealmClient client, RealmPacketIn packet)
        {
            byte num = packet.ReadByte();
            if (num > (byte) 1)
                client.ActiveCharacter.YouAreFuckingCheater("Trying to take wrong faction", 50);
            else if (client.ActiveCharacter.Asda2FactionId != (short) -1 && !client.ActiveCharacter.GodMode)
                Asda2CharacterHandler.SendSelectFactionResResponse(client, SelectFactionStatus.YouAlreadyHaveFaction);
            else if (client.ActiveCharacter.RealProffLevel < (byte) 1 && !client.ActiveCharacter.GodMode)
            {
                Asda2CharacterHandler.SendSelectFactionResResponse(client,
                    SelectFactionStatus.AllowedOnlyFor2JobCharacters);
            }
            else
            {
                client.ActiveCharacter.Asda2FactionId = (short) num;
                Asda2CharacterHandler.SendSelectFactionResResponse(client, SelectFactionStatus.Ok);
                GlobalHandler.SendCharacterFactionToNearbyCharacters(client.ActiveCharacter);
            }
        }

        public static void SendSelectFactionResResponse(IRealmClient client, SelectFactionStatus status)
        {
            if (client.ActiveCharacter.Asda2FactionId == (short) 0)
                client.ActiveCharacter.GetTitle(Asda2TitleId.Light120);
            if (client.ActiveCharacter.Asda2FactionId == (short) 1)
                client.ActiveCharacter.GetTitle(Asda2TitleId.Dark121);
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SelectFactionRes))
            {
                packet.WriteByte((byte) status);
                packet.WriteInt16(client.ActiveCharacter.Asda2FactionId);
                client.Send(packet, false);
            }
        }

        public static bool IsNameValid(string characterName)
        {
            if (characterName.Length == 0 || characterName.Length < 3 || characterName.Length > 18)
                return false;
            return Asda2EncodingHelper.IsPrueEnglishName(characterName);
        }

        internal enum RepairStatus
        {
            Fail,
            Ok,
        }
    }
}