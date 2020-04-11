using ICSharpCode.SharpZipLib.Zip.Compression;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Threading;
using WCell.RealmServerConsole.GetFilesService;

namespace WCell.RealmServerConsole
{
    internal class Program
    {
        private static readonly Dictionary<string, Assembly> Assemblies = new Dictionary<string, Assembly>();

        private static readonly byte[] Key = new byte[31]
        {
            (byte) 5,
            (byte) 6,
            (byte) 3,
            (byte) 2,
            (byte) 6,
            (byte) 22,
            (byte) 6,
            (byte) 4,
            (byte) 2,
            (byte) 2,
            (byte) 2,
            (byte) 5,
            (byte) 43,
            (byte) 12,
            (byte) 98,
            (byte) 123,
            (byte) 12,
            (byte) 1,
            (byte) 2,
            (byte) 4,
            (byte) 1,
            (byte) 2,
            (byte) 5,
            (byte) 1,
            (byte) 1,
            (byte) 122,
            (byte) 82,
            (byte) 93,
            (byte) 42,
            (byte) 1,
            (byte) 1
        };

        private static readonly byte[] KeyContent = new byte[29]
        {
            (byte) 3,
            (byte) 3,
            (byte) 3,
            (byte) 54,
            (byte) 21,
            (byte) 12,
            (byte) 3,
            (byte) 5,
            (byte) 5,
            (byte) 33,
            (byte) 6,
            (byte) 54,
            (byte) 3,
            (byte) 2,
            (byte) 234,
            (byte) 234,
            (byte) 55,
            (byte) 21,
            (byte) 86,
            (byte) 42,
            (byte) 73,
            (byte) 45,
            (byte) 2,
            (byte) 2,
            (byte) 3,
            (byte) 5,
            (byte) 67,
            (byte) 233,
            (byte) 1
        };

        private static bool _isRemoteMode;

        private static void Main(string[] args)
        {
            try
            {
                Program._isRemoteMode = File.Exists(Directory.GetCurrentDirectory() + "\\rm");
                if (Program._isRemoteMode)
                {
                    AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(Program.MyResolveEventHandler);
                    GetFilesServiceClient filesServiceClient = new GetFilesServiceClient();
                    CompositeType contentData = filesServiceClient.GetContentData();
                    try
                    {
                        new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "..\\Content").Delete(true);
                    }
                    catch
                    {
                    }

                    foreach (KeyValuePair<string, byte[]> file in contentData.Files)
                    {
                        string path = AppDomain.CurrentDomain.BaseDirectory + ".." + file.Key;
                        Directory.CreateDirectory(Path.GetDirectoryName(path));
                        File.WriteAllBytes(path, Program.DecodeAndDecompress(file.Value, Program.KeyContent));
                    }

                    foreach (KeyValuePair<string, byte[]> file in filesServiceClient.GetDataUsingDataContract().Files)
                    {
                        Assembly assembly = Assembly.Load(Program.DecodeAndDecompress(file.Value, Program.Key));
                        Program.Assemblies.Add(file.Key.Substring(0, file.Key.Length - 4), assembly);
                    }

                    ((IEnumerable<Type>)Program.Assemblies["WCell.RealmServer"].GetExportedTypes())
                        .First<Type>((Func<Type, bool>)(t => t.Name.Contains(nameof(Program)))).InvokeMember("Start",
                            BindingFlags.InvokeMethod, (Binder)null, (object)null, (object[])null);
                }
                else
                    ((IEnumerable<Type>)Assembly.LoadFile(Path.GetFullPath("WCell.RealmServer.dll"))
                            .GetExportedTypes()).First<Type>((Func<Type, bool>)(t => t.Name.Contains(nameof(Program))))
                        .InvokeMember("Start", BindingFlags.InvokeMethod, (Binder)null, (object)null,
                            (object[])null);

                GCSettings.LatencyMode = !GCSettings.IsServerGC ? GCLatencyMode.Interactive : GCLatencyMode.Batch;
                Thread.CurrentThread.IsBackground = true;
                while (true)
                    Thread.Sleep(1000);
            }
            catch (Exception ex)
            {
                Console.WriteLine((object)ex);
                Console.ReadLine();
            }
        }

        private static byte[] DecodeAndDecompress(byte[] value, byte[] key)
        {
            Rc4Cipher rc4Cipher = new Rc4Cipher();
            rc4Cipher.initialize_ServerCrypto(key);
            rc4Cipher.server_deCrypt(value);
            byte[] output = new byte[BitConverter.ToInt32(value, 0)];
            Program.DecompressZLib(((IEnumerable<byte>)value).Skip<byte>(4).ToArray<byte>(), output);
            return output;
        }

        private static Assembly MyResolveEventHandler(object sender, ResolveEventArgs args)
        {
            if (!Program._isRemoteMode)
                return (Assembly)null;
            return Program.Assemblies[
                args.Name.Contains<char>(',') ? args.Name.Substring(0, args.Name.IndexOf(',')) : args.Name];
        }

        /// <summary>Performs inflate decompression on the given data.</summary>
        /// <param name="input">the data to decompress</param>
        /// <param name="output">the decompressed data</param>
        public static void DecompressZLib(byte[] input, byte[] output)
        {
            Inflater inflater = new Inflater();
            inflater.SetInput(input, 0, input.Length);
            inflater.Inflate(output, 0, output.Length);
        }
    }
}