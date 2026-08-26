using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ProjetoJogosDigitais.Presentation
{
    [RequireComponent(typeof(Image))]
    public sealed class UiRoundedGraphic : MonoBehaviour
    {
        private static readonly Dictionary<int, Sprite> Sprites = new();
        [SerializeField, Range(4, 28)] private int radius = 16;

        public void Configure(int cornerRadius)
        {
            radius = Mathf.Clamp(cornerRadius, 4, 28);
            if (Application.isPlaying)
            {
                Apply();
            }
        }

        private void OnEnable()
        {
            if (Application.isPlaying)
            {
                Apply();
            }
        }

        private void Apply()
        {
            Image image = GetComponent<Image>();
            if (image == null)
            {
                return;
            }

            image.sprite = GetSprite(radius);
            image.type = Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = 1f;
        }

        private static Sprite GetSprite(int cornerRadius)
        {
            if (Sprites.TryGetValue(cornerRadius, out Sprite sprite) && sprite != null)
            {
                return sprite;
            }

            const int size = 64;
            Color32[] pixels = new Color32[size * size];
            float radiusValue = cornerRadius;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nearestX = Mathf.Clamp(x + 0.5f, radiusValue, size - radiusValue);
                    float nearestY = Mathf.Clamp(y + 0.5f, radiusValue, size - radiusValue);
                    float distance = Vector2.Distance(
                        new Vector2(x + 0.5f, y + 0.5f),
                        new Vector2(nearestX, nearestY));
                    byte alpha = distance <= radiusValue - 1f ? (byte)255 : (byte)0;
                    pixels[y * size + x] = new Color32(255, 255, 255, alpha);
                }
            }

            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = $"UI Rounded {cornerRadius}",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };
            texture.SetPixels32(pixels);
            texture.Apply();
            sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, size, size),
                new Vector2(0.5f, 0.5f),
                100f,
                0,
                SpriteMeshType.FullRect,
                new Vector4(cornerRadius, cornerRadius, cornerRadius, cornerRadius));
            sprite.name = texture.name;
            sprite.hideFlags = HideFlags.HideAndDontSave;
            Sprites[cornerRadius] = sprite;
            return sprite;
        }
    }
}
