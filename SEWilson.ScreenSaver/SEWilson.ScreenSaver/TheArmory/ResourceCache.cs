using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace SEWilson.ScreenSaver.TheArmory
{
    public static class ResourceCache
    {
        private static IDictionary<string, Stream> resourceTable;

        static ResourceCache()
        {
            resourceTable = new Dictionary<string, Stream>();
        }

        public static void Create(string key, Stream value)
        {
            MemoryStream cachedStream = MakeMemoryStream(value);
            lock (resourceTable)
            {
                resourceTable.Add(key, cachedStream);
            }
        }

        public static Stream Read(string key)
        {
            Stream stream;
            lock (resourceTable)
            {
                if (resourceTable.TryGetValue(key, out stream))
                {
                    byte[] blob = new byte[stream.Length];
                    stream.Seek(0, SeekOrigin.Begin);
                    stream.Read(blob, 0, blob.Length);
                    stream = new MemoryStream(blob, true);
                    stream.Flush();
                    stream.Seek(0, SeekOrigin.Begin);
                    return stream;
                }
            }
            return null;
        }

        public static void Update(string key, Stream stream)
        {
            MemoryStream cachedStream = MakeMemoryStream(stream);
            lock (resourceTable)
            {
                resourceTable[key] = cachedStream;
            }
        }

        public static void Delete(string key)
        {
            lock (resourceTable)
            {
                if (resourceTable.ContainsKey(key))
                {
                    resourceTable.Remove(key);
                }
            }
        }

        public static MemoryStream MakeMemoryStream(Stream stream)
        {
            MemoryStream cachedStream = new MemoryStream();
            byte[] blob = new byte[8192];
            int cb = stream.Read(blob, 0, blob.Length);
            while (cb > 0)
            {
                cachedStream.Write(blob, 0, cb);
                cb = stream.Read(blob, 0, blob.Length);
            };
            return cachedStream;
        }

        public static void CreateOrUpdate(string key, Stream stream)
        {
            MemoryStream cachedStream = MakeMemoryStream(stream);
            try
            {
                cachedStream.Seek(0, SeekOrigin.Begin);
                Create(key, cachedStream);
            }
            catch (Exception ex)
            {
                cachedStream.Seek(0, SeekOrigin.Begin);
                Update(key, cachedStream);
            }
        }

        public static void Save()
        {
            lock (resourceTable)
            {
                foreach (KeyValuePair<string, Stream> item in resourceTable)
                {
                    try
                    {
                        byte[] blob = new byte[item.Value.Length];
                        item.Value.Seek(0, SeekOrigin.Begin);
                        item.Value.Read(blob, 0, blob.Length);
                        string key = Convert.ToBase64String(item.Key.ToCharArray().Cast<byte>().ToArray());
                        string value = Convert.ToBase64String(blob, Base64FormattingOptions.None);
                        Microsoft.Win32.Registry.CurrentUser.CreateSubKey("Software").CreateSubKey("SEWilson").CreateSubKey("TheArmory").CreateSubKey(".cache").SetValue(key, value);
                    }
                    catch (Exception ex)
                    {
                        // TODO log
                        Debug.WriteLine(ex.Message + "|" + ex.StackTrace);
                    }
                }
            }
        }

        public static void Populate()
        {
            lock (resourceTable)
            {
                string[] keys = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("Software").CreateSubKey("SEWilson").CreateSubKey("TheArmory").CreateSubKey(".cache").GetValueNames();
                foreach (string key in keys)
                {
                    string value = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("Software").CreateSubKey("SEWilson").CreateSubKey("TheArmory").CreateSubKey(".cache").GetValue(key, null) as string;
                    if (value != null)
                    {
                        ResourceCache.Create(
                            new string(Convert.FromBase64String(key).Cast<char>().ToArray()),
                            new MemoryStream(Convert.FromBase64String(value).Cast<byte>().ToArray()));
                    }
                }
            }
        }


        public static void Reset()
        {
            lock (resourceTable)
            {
                Microsoft.Win32.Registry.CurrentUser.CreateSubKey("Software").CreateSubKey("SEWilson").CreateSubKey("TheArmory").DeleteSubKey(".cache", false);
                resourceTable.Clear();
            }
        }
    }
}
