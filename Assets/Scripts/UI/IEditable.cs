using DnD.Model;
using UnityEngine;

namespace DnD.UI
{
    public interface IEditable
    {
        void SetEditable(bool value);
        void Invalidate(CharacterData data);
    }
}