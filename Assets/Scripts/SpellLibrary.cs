using System;
using DnD.Model.Spells;
using UnityEngine;

namespace DnD
{
    public static class SpellLibrary
    {
        private static SpellCatalog catalog;
        private static bool loaded;
        public static string Language { get; set; } = "ru";

        public static SpellCatalog Catalog
        {
            get
            {
                if (loaded) return catalog;
                loaded = true;
                try
                {
                    var asset = Resources.Load<TextAsset>("Spells/spells");
                    if (asset == null) throw new InvalidOperationException("Resources/Spells/spells.json is missing.");
                    catalog = new SpellCatalog(JsonStorage.Deserialize<SpellCatalogData>(asset.text));
                }
                catch (Exception exception)
                {
                    Debug.LogError(exception);
                    JsonStorage.Notify("Не удалось загрузить библиотеку заклинаний.");
                }
                return catalog;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            loaded = false;
            catalog = null;
            Language = "ru";
        }
    }
}
