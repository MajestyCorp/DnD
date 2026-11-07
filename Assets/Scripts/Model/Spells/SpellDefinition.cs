using System;
using System.Collections.Generic;

namespace DnD.Model.Spells
{
    [Serializable]
    public class SpellText
    {
        public string name;
        public string description;
        public string school;
        public string castingTime;
        public string range;
        public string components;
        public string materials;
        public string duration;
    }

    [Serializable]
    public class SpellDefinition
    {
        public string id;
        public int level;
        public string schoolId;
        public List<string> searchAliases = new();
        public List<string> classIds = new();
        public bool ritual;
        public bool concentration;
        public Dictionary<string, SpellText> texts = new();

        public SpellText GetText(string language)
        {
            if (language != null && texts.TryGetValue(language, out var translated) && !string.IsNullOrWhiteSpace(translated?.name))
                return translated;
            if (texts.TryGetValue("en", out var english) && !string.IsNullOrWhiteSpace(english?.name))
                return english;
            foreach (var text in texts.Values)
                if (!string.IsNullOrWhiteSpace(text?.name)) return text;
            return new SpellText { name = "Без названия" };
        }
    }

    [Serializable]
    public class SpellClass
    {
        public string id;
        public Dictionary<string, string> names = new();
        public string GetName(string language)
        {
            if (names.TryGetValue(language ?? "ru", out var translated)) return translated;
            return names.TryGetValue("en", out var english) ? english : id;
        }
    }

    [Serializable]
    public class SpellSubclassList : SpellClass
    {
        public string classId;
        public List<string> spellIds = new();
    }

    [Serializable]
    public class SpellCatalogData
    {
        public int schemaVersion;
        public List<SpellClass> classes = new();
        public List<SpellSubclassList> subclassLists = new();
        public List<SpellDefinition> spells = new();
    }
}
