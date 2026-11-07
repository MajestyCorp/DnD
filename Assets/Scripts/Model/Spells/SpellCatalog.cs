using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace DnD.Model.Spells
{
    public class SpellCatalog
    {
        private readonly Dictionary<string, SpellDefinition> byId = new(StringComparer.Ordinal);
        private readonly Dictionary<string, string> searchText = new(StringComparer.Ordinal);
        public SpellCatalogData Data { get; }

        public SpellCatalog(SpellCatalogData data)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
            if (data.schemaVersion != 1 || data.spells == null || data.classes == null || data.subclassLists == null)
                throw new JsonSerializationException("Unsupported spell catalog.");
            var classes = new HashSet<string>(StringComparer.Ordinal);
            foreach (var entry in data.classes)
                if (entry == null || string.IsNullOrWhiteSpace(entry.id) || !classes.Add(entry.id))
                    throw new JsonSerializationException("Invalid or duplicate spell class.");
            foreach (var spell in data.spells)
            {
                if (spell == null || string.IsNullOrWhiteSpace(spell.id) || spell.level < 0 || spell.level > 9
                    || spell.texts == null || spell.texts.Count == 0 || spell.classIds == null 
                    || spell.texts.Values.Any(text => text == null || string.IsNullOrWhiteSpace(text.name))
                    || spell.classIds.Any(id => !classes.Contains(id)) || !byId.TryAdd(spell.id, spell))
                    throw new JsonSerializationException("Invalid spell definition or class reference.");
                searchText.Add(spell.id, Normalize(string.Join(" ", spell.texts.Values.Select(text => text.name + " " + text.description).Concat(spell.searchAliases ?? new List<string>()))));
            }
            foreach (var subclass in data.subclassLists)
                if (subclass == null || !classes.Contains(subclass.classId) || subclass.spellIds == null
                    || subclass.spellIds.Any(id => !byId.ContainsKey(id)))
                    throw new JsonSerializationException("Invalid subclass spell reference.");
        }

        public SpellDefinition Find(string id) => id != null && byId.TryGetValue(id, out var spell) ? spell : null;

        public List<SpellDefinition> Search(string classId, string text, string language = "ru")
        {
            var terms = Normalize(text).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return byId.Values.Where(spell => (string.IsNullOrEmpty(classId) || spell.classIds.Contains(classId))
                    && terms.All(term => searchText[spell.id].Contains(term)))
                .OrderBy(spell => spell.level)
                .ThenBy(spell => spell.GetText(language).name, StringComparer.OrdinalIgnoreCase).ToList();
        }

        private static string Normalize(string text)
        {
            return Regex.Replace((text ?? "").Normalize(NormalizationForm.FormKC).ToLowerInvariant()
                .Replace('ё', 'е').Replace('’', '\''), @"\s+", " ").Trim();
        }
    }
}
