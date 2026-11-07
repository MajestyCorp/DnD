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
    [AttributeUsage(AttributeTargets.Field)] public class MinAttribute : Attribute { public MinAttribute(float min) { } }
    [AttributeUsage(AttributeTargets.Class)] public class RequireComponent : Attribute { public RequireComponent(params Type[] types) { } }
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
            if (clone is DnD.UI.Spells.SpellRowUI card)
                typeof(DnD.UI.Spells.SpellRowUI).GetField("titleText", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(card, new TMPro.TMP_Text());
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
        public bool enabled = true;
        public T GetComponent<T>() where T : Component => gameObject.GetComponent<T>();
        public T GetComponentInParent<T>(bool includeInactive = false) where T : Component
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
        public GameObject(string name, params Type[] types) : this()
        {
            this.name = name;
            foreach (var type in types)
            {
                if (type == typeof(RectTransform)) continue;
                var component = (Component)Activator.CreateInstance(type); component.owner = this; components.Add(component);
            }
        }
        public bool TryGetComponent<T>(out T value) where T : Component { value = GetComponent<T>(); return value != null; }
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
        public Vector3 position;
        public int childCount => children.Count;
        public Transform GetChild(int index) => children[index];
        public void SetParent(Transform value, bool worldPositionStays = true)
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
    public struct Vector2
    {
        public float x, y;
        public Vector2(float x, float y) { this.x = x; this.y = y; }
        public static Vector2 zero => new Vector2(0, 0);
    }
    public struct Vector3
    {
        public float x, y, z;
        public Vector3(float x, float y, float z) { this.x=x; this.y=y; this.z=z; }
        public static Vector3 operator +(Vector3 a, Vector3 b) => new Vector3(a.x+b.x,a.y+b.y,a.z+b.z);
        public static Vector3 operator -(Vector3 a, Vector3 b) => new Vector3(a.x-b.x,a.y-b.y,a.z-b.z);
        public static Vector3 operator *(Vector3 a, float scale) => new Vector3(a.x*scale,a.y*scale,a.z*scale);
    }
    public struct Rect { public float height; public float width; public Vector2 size => new Vector2(width,height); }
    public class Canvas : Component { public Canvas rootCanvas => this; }
    public class CanvasGroup : Component { public bool blocksRaycasts = true; public float alpha = 1; }
    public static class RectTransformUtility
    {
        public static bool ScreenPointToWorldPointInRectangle(RectTransform rect, Vector2 screen, object camera, out Vector3 point) { point = new Vector3(screen.x,screen.y,0); return true; }
        public static Vector2 WorldToScreenPoint(object camera, Vector3 point) => new Vector2(point.x,point.y);
    }
    public class RectTransform : Transform
    {
        public Vector2 anchorMin, anchorMax, pivot, sizeDelta, anchoredPosition;
        public Rect rect = new Rect { width = 500, height = 352 };
        public void GetWorldCorners(Vector3[] corners)
        {
            corners[0] = new Vector3(position.x,position.y-rect.height,position.z);
            corners[1] = position; corners[2] = new Vector3(position.x+rect.width,position.y,position.z);
            corners[3] = new Vector3(position.x+rect.width,position.y-rect.height,position.z);
        }
    }
    public static class Time { public static float unscaledTime; }
    public class TestEvent<T>
    {
        private readonly List<Action<T>> callbacks = new();
        public void AddListener(Action<T> callback) => callbacks.Add(callback);
        public void RemoveListener(Action<T> callback) => callbacks.Remove(callback);
        public void Invoke(T value) { foreach (var callback in callbacks.ToArray()) callback(value); }
    }
    public class TestEvent
    {
        private readonly List<Action> callbacks = new();
        public void AddListener(Action callback) => callbacks.Add(callback);
        public void RemoveListener(Action callback) => callbacks.Remove(callback);
        public void Invoke() { foreach (var callback in callbacks.ToArray()) callback(); }
    }
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
        public static float Max(float x, float y) => Math.Max(x, y);
        public static int Max(int x, int y) => Math.Max(x, y);
        public static int Min(int x, int y) => Math.Min(x, y);
        public static float Clamp(float value, float min, float max) => Math.Clamp(value, min, max);
        public static int Clamp(int value, int min, int max) => Math.Clamp(value, min, max);
    }
    public struct Color { public Color(float r, float g, float b, float a) { } }
    public class Sprite { }
}
namespace UnityEngine.EventSystems
{
    public interface IBeginDragHandler { void OnBeginDrag(PointerEventData data); }
    public interface IDragHandler { void OnDrag(PointerEventData data); }
    public interface IEndDragHandler { void OnEndDrag(PointerEventData data); }
    public interface IDropHandler { void OnDrop(PointerEventData data); }
    public class PointerEventData { public UnityEngine.Vector2 position; public object pressEventCamera; public UnityEngine.GameObject pointerDrag; }

    public class EventSystem { public static EventSystem current; public void SetSelectedGameObject(UnityEngine.GameObject value) { } }
}
namespace UnityEngine.UI
{
    public class LayoutElement : UnityEngine.Component { public float preferredHeight, preferredWidth, flexibleHeight; }
    public class Toggle : UnityEngine.Component
    {
        private bool value;
        public UnityEngine.TestEvent<bool> onValueChanged = new();
        public bool isOn { get => value; set { if (this.value == value) return; this.value = value; onValueChanged.Invoke(value); } }
        public void SetIsOnWithoutNotify(bool value) { this.value = value; }
    }
    public class Button : UnityEngine.Component { public bool interactable; public UnityEngine.TestEvent onClick = new(); }
    public class Image : UnityEngine.Component { public UnityEngine.Sprite sprite; }
    public class ScrollRect : UnityEngine.Component
    {
        public float verticalNormalizedPosition;
        public UnityEngine.RectTransform content = new(), viewport = new();
        public UnityEngine.TestEvent<UnityEngine.Vector2> onValueChanged = new();
        public void StopMovement() { }
    }
    public static class LayoutRebuilder { public static void ForceRebuildLayoutImmediate(UnityEngine.RectTransform value) { } }
}
namespace DnD
{
    public class TooltipManager
    {
        public static TooltipManager Instance = new(); public string Text;
        public void ShowFor(UnityEngine.RectTransform target, string text) { Text = text; }
    }
    public static class SpellLibrary { public static DnD.Model.Spells.SpellCatalog Catalog; public static string Language { get; set; } = "ru"; }
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
    public class TMP_Text : UnityEngine.Component { public string text; public bool richText; }
    public class TMP_InputField : UnityEngine.Component
    {
        public string text = "";
        public UnityEngine.TestEvent<string> onValueChanged = new();
        public void SetTextWithoutNotify(string value) => text = value;
    }
    public class TMP_Dropdown : UnityEngine.Component
    {
        public class OptionData { public string text; }
        public readonly List<OptionData> options = new();
        public int value;
        public UnityEngine.TestEvent<int> onValueChanged = new();
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
        protected virtual void Awake() { }
        protected virtual void OnDestroy() { }
        protected virtual void OnHideAction() { }
        protected virtual void OnShowAction() { }
        public bool IsVisible;
        public void Show() { IsVisible = true; OnShowAction(); }
        public void ButtonShow() => Show();
        public void ButtonHide() => Hide();
        public void Hide() { OnHideAction(); IsVisible = false; }
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
    public abstract class DraggableItem : UnityEngine.MonoBehaviour
    {
        public static event Action<DraggableItem> DragStarted, DragEnded;
        protected UnityEngine.CanvasGroup canvasGroup;
        protected void NotifyDragStarted() => DragStarted?.Invoke(this);
        protected void NotifyDragEnded() => DragEnded?.Invoke(this);
        public virtual bool CanPerformAction(EDropAction action) => true;
        public virtual void PerformAction(EDropAction action) { }
        public virtual void OnBeginDrag(UnityEngine.EventSystems.PointerEventData data) { }
        public virtual void OnDrag(UnityEngine.EventSystems.PointerEventData data) { }
        public virtual void OnEndDrag(UnityEngine.EventSystems.PointerEventData data) { }
    }

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
