using System.Collections.Generic;
using UnityEngine;

namespace ProjetoJogosDigitais.Presentation
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class CharacterWalkAnimator : MonoBehaviour
    {
        private enum Facing
        {
            Down,
            Left,
            Right,
            Up
        }

        public const int CellSize = 24;
        public const int FramesPerDirection = 4;
        public const float PixelsPerUnit = 12f;

        // Preserve the existing character anchor at x=0.5; walk sheets have one blank sole row.
        public static Vector2 FramePivot(bool idle) => new(0.25f, idle ? 0f : 1f / CellSize);

        [SerializeField] private Texture2D characterSheet;
        [SerializeField] private Texture2D idleSheet;
        [SerializeField, Min(0.1f)] private float strideLength = 1.5f;

        private readonly List<Sprite> generatedSprites = new();
        private SpriteRenderer spriteRenderer;
        private Sprite[][] idleFrames;
        private Sprite[][] walkFrames;
        private Facing facing = Facing.Down;
        private float strideDistance;
        private GameObject contactShadow;
        private bool moving;
        private float idleTime;

        public void Configure(Texture2D sheet, Texture2D idle)
        {
            characterSheet = sheet;
            idleSheet = idle;
            if (spriteRenderer != null)
            {
                ReleaseFrames();
                BuildFrames();
                ShowIdle();
            }
        }

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            BuildFrames();
            ShowIdle();
            CreateContactShadow();
        }

        private void LateUpdate()
        {
            if (!moving)
            {
                idleTime += Time.deltaTime;
                ShowIdle();
            }
        }

        public void SetMotion(Vector2 movement)
        {
            if (walkFrames == null)
            {
                return;
            }

            if (movement.sqrMagnitude <= 0f)
            {
                if (moving)
                {
                    idleTime = 0f;
                }
                moving = false;
                strideDistance = 0f;
                ShowIdle();
                return;
            }

            moving = true;
            facing = ResolveFacing(movement);
            // Advance by ground travelled so the feet stay in sync at any speed or frame rate.
            float stride = Mathf.Max(0.1f, strideLength);
            strideDistance = Mathf.Repeat(strideDistance + movement.magnitude, stride);
            int frame = Mathf.FloorToInt(strideDistance / stride * FramesPerDirection) % FramesPerDirection;
            spriteRenderer.sprite = walkFrames[(int)facing][frame];
        }

        private void CreateContactShadow()
        {
            contactShadow = new GameObject("Sombra de contato");
            contactShadow.transform.SetParent(transform, false);
            // Match the frame pivot so the contact shadow stays directly under the soles.
            contactShadow.transform.localPosition = new Vector3(0.5f, 0.0625f, 0f);
            SpriteRenderer shadow = contactShadow.AddComponent<SpriteRenderer>();
            shadow.sprite = PixelShapes.ContactShadowSprite;
            shadow.color = new Color(0.04f, 0.06f, 0.09f, 0.32f);
            shadow.sortingLayerID = spriteRenderer.sortingLayerID;
            shadow.sortingOrder = spriteRenderer.sortingOrder - 1;
        }

        private void BuildFrames()
        {
            if (characterSheet == null || idleSheet == null)
            {
                return;
            }

            // ELV sheets are a 4x4 grid: Down, Left, Right, Up from top to bottom.
            idleFrames = new Sprite[4][];
            walkFrames = new Sprite[4][];
            for (int direction = 0; direction < 4; direction++)
            {
                BuildDirection((Facing)direction);
            }
        }

        private void BuildDirection(Facing direction)
        {
            var walk = new Sprite[FramesPerDirection];
            var idle = new Sprite[FramesPerDirection];
            for (int index = 0; index < FramesPerDirection; index++)
            {
                walk[index] = CreateFrame(characterSheet, index, (int)direction, false, $"walk-{direction}-{index}");
                idle[index] = CreateFrame(idleSheet, index, (int)direction, true, $"idle-{direction}-{index}");
            }

            walkFrames[(int)direction] = walk;
            idleFrames[(int)direction] = idle;
        }

        private Sprite CreateFrame(Texture2D sheet, int column, int rowFromTop, bool idle, string suffix)
        {
            float y = sheet.height - (rowFromTop + 1) * CellSize;
            Sprite sprite = Sprite.Create(
                sheet,
                new Rect(column * CellSize, y, CellSize, CellSize),
                FramePivot(idle),
                PixelsPerUnit,
                0,
                SpriteMeshType.FullRect);
            sprite.name = $"{sheet.name}-{suffix}";
            generatedSprites.Add(sprite);
            return sprite;
        }

        private Facing ResolveFacing(Vector2 movement)
        {
            float horizontal = Mathf.Abs(movement.x);
            float vertical = Mathf.Abs(movement.y);
            bool wasHorizontal = facing == Facing.Left || facing == Facing.Right;
            // Keep the current axis near a diagonal to avoid flickering between poses.
            if (horizontal > vertical * (wasHorizontal ? 0.85f : 1.15f))
            {
                return movement.x < 0f ? Facing.Left : Facing.Right;
            }

            return movement.y < 0f ? Facing.Down : Facing.Up;
        }

        private void ShowIdle()
        {
            if (idleFrames != null)
            {
                int frame = GamePreferences.ReduceMotion ? 0 : Mathf.FloorToInt(idleTime * 4f) % FramesPerDirection;
                spriteRenderer.sprite = idleFrames[(int)facing][frame];
            }
        }

        private void OnDestroy()
        {
            if (contactShadow != null)
            {
                Destroy(contactShadow);
            }

            ReleaseFrames();
        }

        private void ReleaseFrames()
        {
            foreach (Sprite sprite in generatedSprites)
            {
                if (sprite != null)
                {
                    Destroy(sprite);
                }
            }
            generatedSprites.Clear();
            idleFrames = null;
            walkFrames = null;
        }
    }
}
