using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DnD.UI
{
    public class IconButton2 : Button
    {
        [SerializeField]
        protected Graphic icon;
        [SerializeField]
        protected Graphic icon2;
        [SerializeField]
        protected bool dontChangeIconAlpha = false;

        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            base.DoStateTransition(state, instant);

            if (icon == null)
                return;

            var iconColor = GetIconColor(icon.color, state);

            icon.CrossFadeColor(iconColor, instant ? 0f : colors.fadeDuration, true, true);
            icon2.CrossFadeColor(iconColor, instant ? 0f : colors.fadeDuration, true, true);
        }

        protected virtual Color GetIconColor(Color iconColor, SelectionState state)
        {
            switch (state)
            {
                case SelectionState.Normal:
                    iconColor.a = colors.normalColor.a;
                    break;
                case SelectionState.Highlighted:
                    iconColor.a = 1f;
                    break;
                case SelectionState.Pressed:
                    iconColor.a = 1f;
                    break;
                case SelectionState.Disabled:
                    iconColor.a = 0.2f;
                    break;
                default:
                    iconColor.a = 1f;
                    break;
            }

            if (dontChangeIconAlpha)
                iconColor.a = 1f;

            return iconColor;
        }
    }
}