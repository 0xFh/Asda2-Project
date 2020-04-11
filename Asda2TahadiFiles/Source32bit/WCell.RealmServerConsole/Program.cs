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
      5,
      6,
      3,
      2,
      6,
      22,
      6,
      4,
      2,
      2,
      2,
      5,
      43,
      12,
      98,
      123,
      12,
      1,
      2,
      4,
      1,
      2,
      5,
      1,
      1,
      122,
      82,
      93,
      42,
      1,
      1
    };

    private static readonly byte[] KeyContent = new byte[29]
    {
      3,
      3,
      3,
      54,
      21,
      12,
      3,
      5,
      5,
      33,
      6,
      54,
      3,
      2,
      234,
      234,
      55,
      21,
      86,
      42,
      73,
      45,
      2,
      2,
      3,
      5,
      67,
      233,
      1
    };

    private static bool _isRemoteMode;

    private static void Main(string[] args)
    {
      try
      {
        _isRemoteMode = File.Exists(Directory.GetCurrentDirectory() + "\\rm");
        if(_isRemoteMode)
        {
          AppDomain.CurrentDomain.AssemblyResolve += MyResolveEventHandler;
          GetFilesServiceClient filesServiceClient = new GetFilesServiceClient();
          CompositeType contentData = filesServiceClient.GetContentData();
          try
          {
            new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "..\\Content").Delete(true);
          }
          catch
          {
          }

          foreach(KeyValuePair<string, byte[]> file in contentData.Files)
          {
            string path = AppDomain.CurrentDomain.BaseDirectory + ".." + file.Key;
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllBytes(path, DecodeAndDecompress(file.Value, KeyContent));
          }

          foreach(KeyValuePair<string, byte[]> file in filesServiceClient.GetDataUsingDataContract().Files)
          {
            Assembly assembly = Assembly.Load(DecodeAndDecompress(file.Value, Key));
            Assemblies.Add(file.Key.Substring(0, file.Key.Length - 4), assembly);
          }

          Assemblies["WCell.RealmServer"].GetExportedTypes()
            .First(t => t.Name.Contains(nameof(Program))).InvokeMember("Start",
              BindingFlags.InvokeMethod, null, null, null);
        }
        else
          Assembly.LoadFile(Path.GetFullPath("WCell.RealmServer.dll"))
            .GetExportedTypes().First(t => t.Name.Contains(nameof(Program)))
            .InvokeMember("Start", BindingFlags.InvokeMethod, null, null,
              null);

        GCSettings.LatencyMode = !GCSettings.IsServerGC ? GCLatencyMode.Interactive : GCLatencyMode.Batch;
        Thread.CurrentThread.IsBackground = true;
        while(true)
          Thread.Sleep(1000);
      }
      catch(Exception ex)
      {
        Console.WriteLine(ex);
        Console.ReadLine();
      }
    }

    private static byte[] DecodeAndDecompress(byte[] value, byte[] key)
    {
      Rc4Cipher rc4Cipher = new Rc4Cipher();
      rc4Cipher.initialize_ServerCrypto(key);
      rc4Cipher.server_deCrypt(value);
      byte[] output = new byte[BitConverter.ToInt32(value, 0)];
      DecompressZLib(value.Skip(4).ToArray(), output);
      return output;
    }

    private static Assembly MyResolveEventHandler(object sender, ResolveEventArgs args)
    {
      if(!_isRemoteMode)
        return null;
      return Assemblies[
        args.Name.Contains(',') ? args.Name.Substring(0, args.Name.IndexOf(',')) : args.Name];
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