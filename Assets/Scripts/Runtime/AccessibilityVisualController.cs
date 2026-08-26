using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ProjetoJogosDigitais.Presentation
{
    public sealed class AccessibilityVisualController : MonoBehaviour
    {
        private readonly Dictionary<Text, Color> originalTextColors = new();
        private readonly Dictionary<Outline, Color> originalOutlineColors = new();

        private void Start()
        {
            GamePreferences.Changed += Apply;
            Apply();
        }

        private void OnDestroy()
        {
            GamePreferences.Changed -= Apply;
        }

        private void Apply()
        {
            foreach (Text text in GetComponentsInChildren<Text>(true))
            {
                if (!originalTextColors.ContainsKey(text))
                {
                    originalTextColors[text] = text.color;
                }
                Color original = originalTextColors[text];
                text.color = GamePreferences.HighContrast
                    ? new Color(
                        Mathf.Lerp(original.r, 1f, 0.38f),
                        Mathf.Lerp(original.g, 1f, 0.38f),
                        Mathf.Lerp(original.b, 1f, 0.38f),
                        1f)
                    : original;
            }

            foreach (Outline outline in GetComponentsInChildren<Outline>(true))
            {
                if (!originalOutlineColors.ContainsKey(outline))
                {
                    originalOutlineColors[outline] = outline.effectColor;
                }
                Color original = originalOutlineColors[outline];
                outline.effectColor = GamePreferences.HighContrast
                    ? new Color(original.r, original.g, original.b, Mathf.Max(0.80f, original.a))
                    : original;
            }
        }
    }
}
