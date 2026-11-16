using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace DnD.Model
{
    // A file that could not be loaded, not a playable character or a save target.
    public sealed class DamagedCharacterFile
    {
        private readonly string path;
        public string FileName => Path.GetFileName(File.Exists(path) ? path : path + ".bak");

        internal DamagedCharacterFile(string path) { this.path = path; }

        public bool TryReadText(out string text)
        {
            text = null;
            try
            {
                // Do not deserialize or reserialize: preserve even syntactically invalid JSON.
                var source = File.Exists(path) ? path : path + ".bak";
                text = File.ReadAllText(source, new UTF8Encoding(false, true));
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogError(exception);
                JsonStorage.Notify("Не удалось прочитать исходный файл персонажа. Файл оставлен без изменений.");
                return false;
            }
        }

        public bool Delete() => JsonStorage.Delete(path);
    }
}
