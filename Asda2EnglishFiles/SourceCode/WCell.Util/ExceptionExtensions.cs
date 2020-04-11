using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace WCell.Util
{
    public static class ExceptionExtensions
    {
        public static List<string> GetAllMessages(this Exception ex)
        {
            List<string> stringList = new List<string>();
            do
            {
                if (!(ex is TargetInvocationException))
                {
                    stringList.Add(ex.Message);
                    if (ex is ReflectionTypeLoadException)
                    {
                        stringList.Add("###########################################");
                        stringList.Add("LoaderExceptions:");
                        foreach (Exception loaderException in ((ReflectionTypeLoadException) ex).LoaderExceptions)
                        {
                            stringList.Add(loaderException.GetType().FullName + ":");
                            stringList.AddRange((IEnumerable<string>) loaderException.GetAllMessages());
                            if (loaderException is FileNotFoundException)
                            {
                                IEnumerable<Assembly> matchingAssemblies =
                                    Utility.GetMatchingAssemblies(((FileNotFoundException) loaderException).FileName);
                                if (matchingAssemblies.Count<Assembly>() > 0)
                                    stringList.Add("Found matching Assembly: " +
                                                   matchingAssemblies.ToString<Assembly>("; ") +
                                                   " - Make sure to compile against the correct version.");
                                else
                                    stringList.Add(
                                        "Did not find any matching Assembly - Make sure to load the required Assemblies before loading this one.");
                            }

                            stringList.Add("");
                        }

                        stringList.Add("#############################################");
                    }
                }

                ex = ex.InnerException;
            } while (ex != null);

            return stringList;
        }
    }
}