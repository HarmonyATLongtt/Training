using Newtonsoft.Json;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;

namespace WPF_Sample.Utils
{ /// <summary>
  /// The utility support for interactive with json format
  /// </summary>
    internal static class JsonUtils
    {
        public static bool ExportJsonNet(object obj, string path)
        {
            bool success;
            try
            {
                var setting = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };
                string data = JsonConvert.SerializeObject(obj, Formatting.Indented, setting);
                File.WriteAllText(path, data);
                success = true;
            }
            catch //(Exception ex)
            {
                throw;  // MessageBox.Show(ex.Message);
            }
            return success;
        }

        public static T? ImportJsonNet<T>(string jsonString)
        {
            T? result = default(T);

            try
            {
                result = JsonConvert.DeserializeObject<T>(jsonString);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return result;
        }

        public static string? SerializeJsonNet(object obj)
        {
            string data = String.Empty;
            try
            {
                var setting = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };
                data = JsonConvert.SerializeObject(obj, Formatting.Indented, setting);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return data;
        }

        #region Json with System.Text.Json for test

        /// <summary>
        /// error
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        public static T? Deserialize<T>(string path)
        {
            T? result = default(T);
            if (path == null)
            {
                throw new NullReferenceException(nameof(path));
            }
            else if (!File.Exists(path))
            {
                throw new FileNotFoundException(nameof(path));
            }
            try
            {
                JsonSerializerOptions? opt = new JsonSerializerOptions()
                {
                    WriteIndented = true,
                    ReferenceHandler = ReferenceHandler.Preserve
                };
                result = System.Text.Json.JsonSerializer.Deserialize<T>(File.ReadAllText(path), opt);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return result;
        }

        /// <summary>
        /// error
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string? Serialize(object obj)
        {
            string data = String.Empty;
            try
            {
                JsonSerializerOptions? opt = new JsonSerializerOptions()
                {
                    WriteIndented = true,
                    ReferenceHandler = ReferenceHandler.Preserve
                };
                data = System.Text.Json.JsonSerializer.Serialize(obj, obj.GetType(), opt);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return data;
        }

        #endregion Json with System.Text.Json for test
    }
}