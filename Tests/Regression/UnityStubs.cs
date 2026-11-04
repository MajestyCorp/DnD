using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DnD.Model;
using DnD.Model.Inventory;

namespace UnityEngine
{
    [AttributeUsage(AttributeTargets.Field)] public class SerializeField : Attribute { }
    [AttributeUsage(AttributeTargets.Field)] public class HeaderAttribute : Attribute { public HeaderAttribute(string value) { } }
    public enum FindObjectsInactive { Include }
    public class Object
    {
        public string name;
        public static T FindFirstObjectByType<T>(FindObjectsInactive inactive) where T : Object => null;
        public static T Instantiate<T>(T original, Transform parent) where T : Component, new()
        {
            var clone = new T();
            clone.transform.SetParent(parent);
            if (clone is DnD.UI.Tabs.TitleButton)
            {
                var button = clone.gameObject.AddComponent<DnD.UI.TabButton>();
                var group = parent.GetComponentInParent<DnD.UI.TabGroup>();
                group?.Register(button);
            }
            return clone;
        }
        public static void Destroy(GameObject value) { value.SetActive(false); value.transform.SetParent(null); }
        public static implicit operator bool(Object value) => value != null;
    }
    public class ScriptableObject : Object
    {
        public static T CreateInstance<T>() where T : ScriptableObject, new() => new T();
    }
    [AttributeUsage(AttributeTargets.Class)]
    public class CreateAssetMenuAttribute : Attribute { public string fileName; public string menuName; }
    public class Component : Object
    {
        internal GameObject owner;
        public GameObject gameObject => owner ??= new GameObject(this);
        public Transform transform => gameObject.transform;
        public T GetComponent<T>() where T : Component => gameObject.GetComponent<T>();
        public T GetComponentInParent<T>() where T : Component
        {
            for (var node = transform; node != null; node = node.parent)
                if (node.gameObject.GetComponent<T>() is T value) return value;
            return null;
        }
    }
    public class GameObject : Object
    {
        private readonly List<Component> components = new();
        public bool activeSelf = true;
        public Transform transform { get; }
        public GameObject(Component initial = null)
        {
            transform = initial as Transform ?? new RectTransform();
            transform.owner = this;
            components.Add(transform);
            if (initial != null && initial != transform) { initial.owner = this; components.Add(initial); }
        }
        public void SetActive(bool value) => activeSelf = value;
        public T AddComponent<T>() where T : Component, new()
        {
            var value = new T { owner = this };
            components.Add(value);
            return value;
        }
        public T GetComponent<T>() where T : Component => components.OfType<T>().FirstOrDefault();
    }
    public class Transform : Component
    {
        private readonly List<Transform> children = new();
        public Transform parent { get; private set; }
        public int childCount => children.Count;
        public Transform GetChild(int index) => children[index];
        public void SetParent(Transform value)
        {
            parent?.children.Remove(this);
            parent = value;
            value?.children.Add(this);
        }
        public void SetAsLastSibling() => SetSiblingIndex(int.MaxValue);
        public int GetSiblingIndex() => parent?.children.IndexOf(this) ?? 0;
        public void SetSiblingIndex(int index)
        {
            if (parent == null) return;
            parent.children.Remove(this);
            parent.children.Insert(Math.Clamp(index, 0, parent.children.Count), this);
        }
    }
    public class RectTransform : Transform { }
    public class MonoBehaviour : Component
    {
        public object StartCoroutine(IEnumerator routine) => null;
    }
    public static class Application { public static string persistentDataPath; }
    public static class GUIUtility { public static string systemCopyBuffer; }
    public static class PlayerPrefs
    {
        private static readonly Dictionary<string, string> values = new();
        public static string GetString(string key, string fallback) => values.TryGetValue(key, out var value) ? value : fallback;
        public static void SetString(string key, string value) => values[key] = value;
        public static void Save() { }
    }
    public static class Debug
    {
        public static int Errors;
        public static void LogError(object message) { Errors++; }
        public static void LogWarning(object message) { }
    }
    public static class Mathf
    {
        public static int FloorToInt(float value) => (int)Math.Floor(value);
        public static int CeilToInt(float value) => (int)Math.Ceiling(value);
        public static int Max(int x, int y) => Math.Max(x, y);
        public static int Min(int x, int y) => Math.Min(x, y);
        public static int Clamp(int value, int min, int max) => Math.Clamp(value, min, max);
    }
    public struct Color { public Color(float r, float g, float b, float a) { } }
    public class Sprite { }
}
namespace UnityEngine.EventSystems
{
    public class EventSystem { public static EventSystem current; public void SetSelectedGameObject(UnityEngine.GameObject value) { } }
}
namespace UnityEngine.UI
{
    public class Toggle : UnityEngine.Component { public bool isOn; }
    public class Button : UnityEngine.Component { public bool interactable; }
    public class Image : UnityEngine.Component { public UnityEngine.Sprite sprite; }
    public class ScrollRect : UnityEngine.Component { public float verticalNormalizedPosition; }
    public static class LayoutRebuilder { public static void ForceRebuildLayoutImmediate(UnityEngine.RectTransform value) { } }
}
namespace DnD
{
    public class SoundManager
    {
        public static readonly SoundManager Instance = new();
        public void PlayClick() { }
        public void PlayNegative() { }
    }
}
namespace DnD.Profiles
{
    public class ItemIconsProfile { public UnityEngine.Sprite GetIcon(int group, int id) => null; }
}
namespace TMPro
{
    public class TMP_Text : UnityEngine.Component { public string text; }
    public class TMP_InputField : UnityEngine.Component { public string text = ""; }
    public class TMP_Dropdown : UnityEngine.Component
    {
        public class OptionData { public string text; }
        public readonly List<OptionData> options = new();
        public int value;
        public void ClearOptions() { options.Clear(); value = 0; }
        public void AddOptions(List<string> labels) { options.AddRange(labels.Select(label => new OptionData { text = label })); }
        public void SetValueWithoutNotify(int selected) => value = selected;
        public void RefreshShownValue() { }
    }
}
namespace DnD.UI
{
    public static class UpdatePropPopup
    {
        public static void Popup(string title, SkillAttribute attribute, int masteryBonus, Action callback) { }
    }
    public class ConfirmPopup { public static bool TryShowMessage(string value) => true; }
    public class Avatar : UnityEngine.Component
    {
        public int CurrentIndex => 0;
        public void Invalidate(CharacterData data) { }
        public void SetEditMode(bool value) { }
    }
    public class Frame : UnityEngine.MonoBehaviour
    {
        protected virtual void OnShowAction() { }
        public bool IsVisible;
        public void Show() { IsVisible = true; OnShowAction(); }
        public void ButtonShow() => Show();
        public void ButtonHide() => Hide();
        public void Hide() { IsVisible = false; }
        public void ForceShowEvent() { }
    }
    public class ToggleCharacter : UnityEngine.Component
    {
        public CharacterData Character { get; private set; }
        public bool IsOn { get; private set; }
        public void Init(CharacterData character) { Character = character; gameObject.SetActive(true); }
        public void SetToggle(bool value) => IsOn = value;
    }
    public static class SelectIconPopup { public static void Popup(DnD.Profiles.ItemIconsProfile profile, Action<int, int> callback, Action cancelled) { } }
    public class ItemsLibraryPopup { public static void Popup(Action<Item> callback) { } }
    public class TabButton : UnityEngine.Component
    {
        public bool Selected;
        public void SetSelected(bool value) => Selected = value;
    }
}
namespace DnD.UI.Inventory
{
    public class EffectItem : UnityEngine.Component { public void Initialize(string value) { name = value; gameObject.SetActive(true); } }
    public class InventoryItem : UnityEngine.Component
    {
        public Item Item { get; private set; }
        public void Initialize(Item item) { Item = item; gameObject.SetActive(true); }
    }
}
namespace DnD.UI.Tabs
{
    public class TitleButton : UnityEngine.Component
    {
        public void Init(string value) { name = value; gameObject.SetActive(true); }
    }
    public class CharacterTabContent : UnityEngine.Component
    {
        public TabData Data;
        public bool Editable;
        public void FillContent(TabData value) => Data = value;
        public void SetEditable(bool value) => Editable = value;
    }
    public static class ManageTabPopup { public static void PopupCreate(Action<string> callback) { } }
    public static class ManageTabsPopup { public static void Popup(Action<List<TabData>> callback) { } }
}
