using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DnD.Converters;
using Newtonsoft.Json;
using UnityEngine;

namespace DnD
{
    public static class JsonStorage
    {
        private static readonly Queue<string> messages = new();

        public static JsonSerializerSettings CreateSettings()
        {
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Include,
                ObjectCreationHandling = ObjectCreationHandling.Replace
            };
            settings.Converters.Add(new TabContentConverter());
            return settings;
        }

        public static string Serialize(object value)
        {
            return JsonConvert.SerializeObject(value, Formatting.None, CreateSettings());
        }

        public static T Deserialize<T>(string json) where T : class
        {
            return JsonConvert.DeserializeObject<T>(json, CreateSettings())
                ?? throw new JsonSerializationException("JSON contains no data.");
        }

        public static bool Write(string path, object value)
        {
            var temporaryPath = path + ".tmp";
            try
            {
                var json = Serialize(value);
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                using (var stream = new FileStream(temporaryPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    using (var writer = new StreamWriter(stream, new UTF8Encoding(false), 4096, true))
                    {
                        writer.Write(json);
                        writer.Flush();
                    }
                    stream.Flush(true);
                }

                if (File.Exists(path))
                    File.Replace(temporaryPath, path, path + ".bak");
                else
                    File.Move(temporaryPath, path);
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogError($"Save failed: {path}\n{exception}");
                Notify("Не удалось сохранить изменения. Проверьте свободное место и попробуйте ещё раз.");
                return false;
            }
        }

        public static T Read<T>(string path, Func<string, T> deserialize) where T : class
        {
            if (!File.Exists(path) && !File.Exists(path + ".bak"))
                return null;

            try
            {
                return deserialize(File.ReadAllText(path))
                    ?? throw new JsonSerializationException("JSON contains no data.");
            }
            catch (Exception exception)
            {
                Debug.LogError($"Load failed: {path}\n{exception}");
            }

            try
            {
                var recovered = deserialize(File.ReadAllText(path + ".bak"))
                    ?? throw new JsonSerializationException("Backup contains no data.");
                // Preserve the damaged file and keep the good backup out of the next rotation.
                if (File.Exists(path))
                    File.Move(path, path + ".corrupt_" + Guid.NewGuid().ToString("N"));
                Notify("Один из файлов не удалось прочитать. Данные восстановлены из резервной копии.");
                return recovered;
            }
            catch (Exception exception)
            {
                Debug.LogError($"Recovery failed: {path}\n{exception}");
                Notify("Один из файлов не удалось загрузить. Остальные данные доступны; исходный файл сохранён.");
                return null;
            }
        }

        public static bool Delete(string path)
        {
            try
            {
                // Remove the backup too, so deleted profiles cannot reappear on restart.
                File.Delete(path + ".bak");
                File.Delete(path + ".tmp");
                File.Delete(path);
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogError($"Delete failed: {path}\n{exception}");
                Notify("Не удалось удалить данные. Попробуйте ещё раз.");
                return false;
            }
        }

        public static void Notify(string message)
        {
            if (!messages.Contains(message))
                messages.Enqueue(message);
        }

        public static void ShowPendingMessage()
        {
            if (messages.Count > 0 && UI.ConfirmPopup.TryShowMessage(messages.Peek()))
                messages.Dequeue();
        }
    }
}
