/*************************************************************************
 *
 *   file		: Program.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-09-26 23:15:07 +0200 (lø, 26 sep 2009) $
 
 *   revision		: $Rev: 1115 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime;
using System.Linq;
using System.Threading;
using ICSharpCode.SharpZipLib.Zip.Compression;
using NLog;
//using WCell.RealmServer.Content;

namespace WCell.RealmServerConsole
{
	internal class Program
	{
	  private static bool _isRemoteMode;
	  static readonly Dictionary<string, Assembly> Assemblies = new Dictionary<string, Assembly>();
      private static void Main(string[] args)
      {
          try
          {
                     
              _isRemoteMode = File.Exists(Directory.GetCurrentDirectory() + @"\rm");
              if (_isRemoteMode)
              {
                  AppDomain currentDomain = AppDomain.CurrentDomain;
                  currentDomain.AssemblyResolve += MyResolveEventHandler;
                  var c = new GetFilesService.GetFilesServiceClient();
                  var content = c.GetContentData();
                  try
                  {
                      var contentFolder = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "..\\" + "Content");
                      contentFolder.Delete(true);
                  }
                  catch 
                  {
                  }
                  foreach (var file in content.Files)
                  {
                      var filePath = AppDomain.CurrentDomain.BaseDirectory+".."+file.Key;
                      var dir = Path.GetDirectoryName(filePath);
                      Directory.CreateDirectory(dir);
                      File.WriteAllBytes(filePath, DecodeAndDecompress(file.Value,KeyContent));
                  }
                  var d = c.GetDataUsingDataContract();
                  foreach (var file in d.Files)
                  {
                      var assData = DecodeAndDecompress(file.Value,Key);
                      var ass = Assembly.Load(assData);
                      Assemblies.Add(file.Key.Substring(0, file.Key.Length - 4), ass);
                  }
                  var wcellAss = Assemblies["WCell.RealmServer"];
                  var type = wcellAss.GetExportedTypes().First(t => t.Name.Contains("Program"));
                  type.InvokeMember("Start", BindingFlags.InvokeMethod, null, null, null);
              }
              else
              {
                  var wcellAss = Assembly.LoadFile(Path.GetFullPath("WCell.RealmServer.dll"));
                  var type = wcellAss.GetExportedTypes().First(t => t.Name.Contains("Program"));
                  type.InvokeMember("Start", BindingFlags.InvokeMethod, null, null, null);
              }
              if (GCSettings.IsServerGC)
              {
                  GCSettings.LatencyMode = GCLatencyMode.Batch;
              }
              else
              {
                  GCSettings.LatencyMode = GCLatencyMode.Interactive;
              }


              Thread.CurrentThread.IsBackground = true;

              //ContentMgr.ForceDataPresence = true;
              //RealmServer.Program.Start();
              //RealmServerConsole.Run();
              while (true)
              {
                  Thread.Sleep(1000);
              }
          }
          catch (Exception ex)
          {
              Console.WriteLine(ex);
              Console.ReadLine();
          }
      }
      
        private static readonly byte[] Key = new byte[]{5,6,3,2,6,22,6,4,2,2,2,5,43,12,98,123,12,1,2,4,1,2,5,1,1,122,82,93,42,1,1};
        private static readonly byte[] KeyContent = new byte[] { 3,3,3,54,21,12,3,5,5,33,6,54,3,2,234,234,55,21,86,42,73,45,2,2,3,5,67,233,1 };
	    private static byte[] DecodeAndDecompress(byte[] value,byte[]key)
	    {
	        //decode
	        var cipter = new Rc4Cipher();
            cipter.initialize_ServerCrypto(key);
            cipter.server_deCrypt(value);
	        var result = new byte[BitConverter.ToInt32(value,0)];
            DecompressZLib(value.Skip(4).ToArray(),result);
	        return result;
	    }
	    private static Assembly MyResolveEventHandler(object sender, ResolveEventArgs args)
	    {
            if (!_isRemoteMode)
                return null;
	       return Assemblies[args.Name.Contains(',')?args.Name.Substring(0,args.Name.IndexOf(',')):args.Name];
	    }
        /// <summary>
        /// Performs inflate decompression on the given data.
        /// </summary>
        /// <param name="input">the data to decompress</param>
        /// <param name="output">the decompressed data</param>
        public static void DecompressZLib(byte[] input, byte[] output)
        {
            Inflater item = new Inflater();

            item.SetInput(input, 0, input.Length);
            item.Inflate(output, 0, output.Length);
        }
	}
}