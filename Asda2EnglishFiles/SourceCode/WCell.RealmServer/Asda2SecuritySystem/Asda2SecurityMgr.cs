using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using WCell.Core;
using WCell.Core.Initialization;
using WCell.Core.Timers;
using WCell.RealmServer.Global;

namespace WCell.RealmServer.Asda2SecuritySystem
{
    internal class Asda2SecurityMgr : IUpdatable
    {
        private static int timeToAuthorization = 300000;
        private static Random random = new Random();

        private static int[] CryptKey1 = new int[64]
        {
            232,
            104,
            103,
            194,
            180,
            4,
            16,
            23,
            227,
            11,
            38,
            220,
            186,
            249,
            141,
            141,
            185,
            172,
            63,
            192,
            118,
            72,
            125,
            188,
            141,
            82,
            72,
            17,
            167,
            249,
            33,
            6,
            0,
            178,
            53,
            238,
            105,
            15,
            13,
            32,
            185,
            69,
            39,
            143,
            60,
            144,
            185,
            9,
            176,
            43,
            35,
            51,
            31,
            4,
            99,
            168,
            183,
            2,
            227,
            4,
            216,
            183,
            191,
            79
        };

        private static int[] CryptKey2 = new int[64]
        {
            35,
            118,
            4,
            216,
            93,
            180,
            220,
            49,
            192,
            48,
            120,
            41,
            104,
            7,
            104,
            61,
            232,
            1,
            217,
            6,
            197,
            121,
            253,
            87,
            162,
            133,
            64,
            24,
            152,
            213,
            155,
            45,
            250,
            165,
            124,
            156,
            196,
            86,
            (int) byte.MaxValue,
            20,
            251,
            219,
            6,
            233,
            177,
            202,
            39,
            129,
            73,
            213,
            157,
            164,
            93,
            242,
            37,
            217,
            28,
            52,
            41,
            169,
            197,
            170,
            59,
            189
        };

        private const string phpScrypt = "http://akerusproj.esy.es/";
        private const int defaultTimeToAuthorization = 300000;

        private static int[] GenerateRandomData()
        {
            int[] numArray = new int[160];
            for (int index = 0; index < numArray.Length; ++index)
                numArray[index] = Asda2SecurityMgr.random.Next(256);
            return numArray;
        }

        private static int[] EncryptData(int[] data, int[] key)
        {
            for (int index = 0; index < data.Length; ++index)
                data[index] ^= key[index % key.Length];
            return data;
        }

        public static bool CompareData(int[] data1, int[] data2)
        {
            if (data1.Length != data2.Length)
                return false;
            for (int index = 0; index < data1.Length; ++index)
            {
                if (data1[index] != data2[index])
                    return false;
            }

            return true;
        }

        public static bool Authorize()
        {
            //int[] data2 = Asda2SecurityMgr.EncryptData(Asda2SecurityMgr.GenerateRandomData(), Asda2SecurityMgr.CryptKey1);
            //string str = "?";
            //if (data2.Length > 0)
            //  str = str + "i[]=" + (object) data2[0];
            //for (int index = 1; index < data2.Length; ++index)
            //  str = str + "&i[]=" + (object) data2[index];
            //HttpWebResponse response = (HttpWebResponse) WebRequest.Create("http://akerusproj.esy.es/" + str).GetResponse();
            //string input = (string) null;
            //using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
            //  input = streamReader.ReadLine();
            //if (input != null)
            //{
            //  string[] strArray = Regex.Replace(input, "(o\\[\\]=)", "").Split('&');
            //  int[] data = new int[strArray.Length];
            //  for (int index = 0; index < data.Length; ++index)
            //    data[index] = int.Parse(strArray[index]);
            //  if (Asda2SecurityMgr.CompareData(Asda2SecurityMgr.EncryptData(data, Asda2SecurityMgr.CryptKey2), data2))
            //    return true;
            //}
            //return false;
            return true;
        }

        [WCell.Core.Initialization.Initialization(InitializationPass.First, "Asda2 Security System")]
        public static void InitSecuritySystem()
        {
            try
            {
                if (!Asda2SecurityMgr.Authorize())
                {
                    Console.WriteLine("Запуск сервера невозможен, он будет остановлен.");
                    ServerApp<WCell.RealmServer.RealmServer>.Instance.ShutdownIn(0U);
                }
                else
                    World.TaskQueue.RegisterUpdatableLater((IUpdatable) new Asda2SecurityMgr());
            }
            catch (Exception ex)
            {
                Console.WriteLine("Запуск сервера невозможен, он будет остановлен.\n" + (object) ex);
                ServerApp<WCell.RealmServer.RealmServer>.Instance.ShutdownIn(0U);
            }
        }

        public void Update(int dt)
        {
            Asda2SecurityMgr.timeToAuthorization -= dt;
            if (Asda2SecurityMgr.timeToAuthorization > 0)
                return;
            if (!Asda2SecurityMgr.Authorize())
            {
                Console.WriteLine("Работа сервера невозможна, он будет остановлен.");
                ServerApp<WCell.RealmServer.RealmServer>.Instance.ShutdownIn(0U);
            }

            Asda2SecurityMgr.timeToAuthorization = 300000;
        }
    }
}