using UnityEngine;

namespace ProjetoJogosDigitais.Presentation
{
    public static class PixelShapes
    {
        private static Sprite whiteSprite;
        private static Sprite contactShadowSprite;

        public static Sprite ContactShadowSprite
        {
            get
            {
                if (contactShadowSprite != null)
                {
                    return contactShadowSprite;
                }

                const int width = 12;
                const int height = 4;
                var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
                {
                    name = "Contact Shadow",
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp,
                    hideFlags = HideFlags.HideAndDontSave
                };
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        float dx = (x + 0.5f - width * 0.5f) / (width * 0.5f);
                        float dy = (y + 0.5f - height * 0.5f) / (height * 0.5f);
                        texture.SetPixel(x, y, dx * dx + dy * dy <= 1f ? Color.white : Color.clear);
                    }
                }
                texture.Apply();
                contactShadowSprite = Sprite.Create(texture, new Rect(0, 0, width, height),
                    new Vector2(0.5f, 0.5f), 16f);
                contactShadowSprite.name = "Contact Shadow Sprite";
                contactShadowSprite.hideFlags = HideFlags.HideAndDontSave;
                return contactShadowSprite;
            }
        }

        public static Sprite WhiteSprite
        {
            get
            {
                if (whiteSprite != null)
                {
                    return whiteSprite;
                }

                var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
                {
                    name = "Runtime Pixel",
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp,
                    hideFlags = HideFlags.HideAndDontSave
                };
                texture.SetPixel(0, 0, Color.white);
                texture.Apply();

                whiteSprite = Sprite.Create(
                    texture,
                    new Rect(0f, 0f, 1f, 1f),
                    new Vector2(0.5f, 0.5f),
                    1f);
                whiteSprite.name = "Runtime Pixel Sprite";
                whiteSprite.hideFlags = HideFlags.HideAndDontSave;
                return whiteSprite;
            }
        }

        public static GameObject CreateBlock(
            Transform parent,
            string name,
            Vector3 localPosition,
            Vector2 size,
            Color color,
            int sortingOrder)
        {
            var block = new GameObject(name);
            block.transform.SetParent(parent, false);
            block.transform.localPosition = localPosition;
            block.transform.localScale = new Vector3(size.x, size.y, 1f);

            var renderer = block.AddComponent<SpriteRenderer>();
            renderer.sprite = WhiteSprite;
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            return block;
        }
    }
}
