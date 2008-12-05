using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlServerCe;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Data;

namespace SEWilson.ScreenSaver.Caching
{
    public class LocalObjectCache<TKey, TValue>
        where TValue : class
    {        
        private IFormatter formatter = new BinaryFormatter();

        public TValue Get(TKey key)
        {
            string keyString = ConvertToIndexKey(key);
            using (SqlCeConnection connection = new SqlCeConnection(GetConnectionString()))
            {
                using (SqlCeCommand command = new SqlCeCommand())
                {
                    command.CommandText = string.Format("SELECT [V] FROM [{0}] WHERE [K] = @K", GetTableName());
                    command.Parameters.Add("@K", SqlDbType.NChar, keyString.Length).Value = keyString;
                    command.Connection = connection;
                    connection.Open();
                    object obj = command.ExecuteScalar();
                    if (obj != DBNull.Value)
                    {
                        string blob = obj as string;
                        if (blob != null)
                        {
                            byte[] buffer = Convert.FromBase64String(blob);
                            lock (formatter)
                            {
                                MemoryStream memoryStream = new MemoryStream(buffer);
                                return formatter.Deserialize(memoryStream) as TValue;
                            }
                        }
                    }
                }
            }
            return null;
        }

        public TValue Get(TKey key, double ttlSeconds)
        {
            string keyString = ConvertToIndexKey(key);
            object obj = null;
            using (SqlCeConnection connection = new SqlCeConnection(GetConnectionString()))
            {
                connection.Open();
                using (SqlCeCommand command = new SqlCeCommand())
                {
                    command.CommandText = string.Format("SELECT [V] FROM [{0}] WHERE ([K] = @K) AND ([T] > @T)", GetTableName());
                    command.Parameters.Add("@K", SqlDbType.NChar, keyString.Length).Value = keyString;
                    command.Parameters.Add("@T", SqlDbType.DateTime).Value = DateTime.Now.Subtract(TimeSpan.FromSeconds(ttlSeconds));
                    command.Connection = connection;
                    obj = command.ExecuteScalar();
                }
            }
            if ((obj != null) && (obj != DBNull.Value))
            {
                string blob = obj as string;
                if (blob != null)
                {
                    byte[] buffer = Convert.FromBase64String(blob);
                    lock (formatter)
                    {
                        MemoryStream memoryStream = new MemoryStream(buffer);
                        return formatter.Deserialize(memoryStream) as TValue;
                    }
                }
            }
            return null;
        }

        public void Set(TKey key, TValue value)
        {
            string keyString = ConvertToIndexKey(key);
            if (!typeof(TValue).IsDefined(typeof(SerializableAttribute), true))
                throw new SerializationException("Type is not marked [Serializable]: " + typeof(TValue).Name);

            MemoryStream memoryStream = new MemoryStream();
            lock (formatter)
            {
                formatter.Serialize(memoryStream, value);
            }
            string blob = Convert.ToBase64String(memoryStream.GetBuffer(), Base64FormattingOptions.None);
            using (SqlCeConnection connection = new SqlCeConnection(GetConnectionString()))
            {
                connection.Open();
                using (SqlCeCommand command = new SqlCeCommand())
                {
                    command.CommandText = string.Format("DELETE FROM [{0}] WHERE [K] = @K", GetTableName());
                    command.Parameters.Add("@K", SqlDbType.NChar, keyString.Length).Value = keyString;
                    command.Connection = connection;
                    command.ExecuteNonQuery();
                }
                using (SqlCeCommand command = new SqlCeCommand())
                {
                    command.CommandText = string.Format("INSERT INTO [{0}]([K],[V],[T]) values(@K,@V,getdate())", GetTableName());
                    command.Parameters.Add("@K", SqlDbType.NChar, keyString.Length).Value = keyString;
                    command.Parameters.Add("@V", SqlDbType.NText, blob.Length).Value = blob;
                    command.Connection = connection;
                    command.ExecuteNonQuery();
                }
            }
        }

        private static System.Security.Cryptography.SHA1Managed sha = new System.Security.Cryptography.SHA1Managed();

        public static string ConvertToIndexKey(TKey key)
        {
            byte[] buffer = null;
            if (key is byte[])
            {
                buffer = key as byte[];
            }
            else
            {
                buffer = System.Text.UTF8Encoding.UTF8.GetBytes(Convert.ToString(key));
            }
            lock (sha)
            {
                buffer = sha.ComputeHash(buffer);
            }
            return System.Text.ASCIIEncoding.ASCII.GetString(buffer);
        }

        public void Reset()
        {
            try
            {
                using (SqlCeConnection connection = new SqlCeConnection(GetConnectionString()))
                {
                    connection.Open();
                    using (SqlCeCommand command = new SqlCeCommand())
                    {
                        command.CommandText = string.Format("DELETE FROM [{0}]", GetTableName());
                        command.Connection = connection;
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message + "|" + ex.StackTrace);
            }
        }

        public LocalObjectCache()
        {
            int retryCount = 0;
            try
            {
            retry_LOCDBALLOC:
                if (retryCount < 10)
                {
                    retryCount++;

                    string path = GetDatabasePath();
                    if (!File.Exists(path))
                    {
                        using (SqlCeEngine sqlce = new SqlCeEngine(GetConnectionString("Exclusive")))
                        {
                            try
                            {
                                sqlce.CreateDatabase();
                                using (SqlCeConnection connection = new SqlCeConnection(GetConnectionString()))
                                {
                                    connection.Open();
                                    using (SqlCeCommand command = new SqlCeCommand())
                                    {
                                        command.CommandText = string.Format("CREATE TABLE [Settings] ([K] nchar(256), [V] nchar(256))", GetTableName());
                                        command.Connection = connection;
                                        command.ExecuteNonQuery();
                                    }
                                    using (SqlCeCommand command = new SqlCeCommand())
                                    {
                                        command.CommandText = string.Format("INSERT INTO [Settings]([K],[V]) values(@K,@V)");
                                        command.Parameters.Add("@K", SqlDbType.NChar, "CurrentVersion".Length).Value = "CurrentVersion";
                                        command.Parameters.Add("@V", SqlDbType.NText, Assembly.GetEntryAssembly().GetName().Version.ToString().Length).Value = Assembly.GetEntryAssembly().GetName().Version.ToString();
                                        command.Connection = connection;
                                        command.ExecuteNonQuery();
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex.Message + "|" + ex.StackTrace);
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            using (SqlCeConnection connection = new SqlCeConnection(GetConnectionString()))
                            {
                                connection.Open();
                                using (SqlCeCommand command = new SqlCeCommand())
                                {
                                    command.CommandText = string.Format("SELECT COALESCE([V], '0.0.0.0') FROM [Settings] WHERE [K] = 'CurrentVersion'", GetTableName());
                                    command.Connection = connection;
                                    object o = command.ExecuteScalar();
                                    string versionString = o as string;
                                    if (versionString.Trim() != Assembly.GetEntryAssembly().GetName().Version.ToString())
                                    {
                                        throw new Exception("DB Version Mismatch, Will Rebuild Database");
                                    }
                                }

                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message + "|" + ex.StackTrace);
                            if (File.Exists(path))
                            {
                                File.Delete(path);
                            }
                            goto retry_LOCDBALLOC;
                        }
                    }
                }

                // enforce table existence, expects failure (already exists) and continues on fail
                // TODO we should query systables for the table object instead, but i dont know sqlce limits at the moment
                try
                {
                    using (SqlCeConnection connection = new SqlCeConnection(GetConnectionString()))
                    {
                        connection.Open();
                        using (SqlCeCommand command = new SqlCeCommand())
                        {
                            command.CommandText = string.Format("CREATE TABLE [{0}] ([K] nchar(20), [V] ntext, [T] datetime)", GetTableName());
                            command.Connection = connection;
                            command.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message + "|" + ex.StackTrace);
                }
            }
            catch (Exception ex)
            {
                Util.UI.ExceptionInspectorWindow.Inspect(ex);
            }
        }

        private string GetConnectionString()
        {
            return GetConnectionString(null);
        }

        private string GetConnectionString(string mode)
        {
            return string.Format("Data Source='{0}';max buffer size={1};max database size={2};mode={3};",
                         GetDatabasePath(),
                         65536,
                         1024,
                         mode ?? "Read Write");
        }

        private string GetDatabasePath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"wasscache.sdf");
        }

        private string GetTableName()
        {
            return (typeof(TValue)).Name;
        }
    }
}
