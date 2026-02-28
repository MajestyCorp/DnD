using UnityEngine;
using System;
using TMPro;
using System.Text.RegularExpressions;

namespace DnD.UI
{
    [Serializable]
    [CreateAssetMenu(fileName = "InputValidator - Modificator.asset", menuName = "TextMeshPro/Input Validators/Modificator", order = 100)]
    public class ModificatorValidator : TMP_InputValidator
    {
        public override char Validate(ref string text, ref int pos, char ch)
        {
            if (ch == '+' || ch == '-')
            {
                if (pos == 0)
                {
                    text += ch;
                    pos += 1;
                    return ch;
                }
            }

            if (char.IsDigit(ch))
            {
                text += ch;
                pos += 1;
                return ch;
            }
            
            return '\0';
        }
    }
}