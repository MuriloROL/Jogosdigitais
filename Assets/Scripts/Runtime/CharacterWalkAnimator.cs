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

        private const int CellWidth = 16;
        private const int CellHeight = 32;
        private const int WalkFrameCount = 6;

        [SerializeField] private Texture2D characterSheet;
        [SerializeField, Min(1f)] private float framesPerSecond = 8f;

        private readonly List<Sprite> generatedSprites = new();
        private SpriteRenderer spriteRenderer;
        private Sprite[] idleFrames;
        private Sprite[][] walkFrames;
        private Facing facing = Facing.Down;
        private float animationTime;

        public void Configure(Texture2D sheet)
        {
            characterSheet = sheet;
        }

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            BuildFrames();
            ShowIdle();
        }

        public void SetMotion(Vector2 movement)
        {
            if (walkFrames == null)
            {
                return;
            }

            if (movement.sqrMagnitude <= 0.000001f)
            {
                animationTime = 0f;
                ShowIdle();
                return;
            }

            facing = ResolveFacing(movement);
            animationTime += Time.deltaTime;
            int frame = Mathf.FloorToInt(animationTime * framesPerSecond) % WalkFrameCount;
            spriteRenderer.sprite = walkFrames[(int)facing][frame];
        }

        private void BuildFrames()
        {
            if (characterSheet == null)
            {
                return;
            }

            idleFrames = new Sprite[4];
            idleFrames[(int)Facing.Left] = CreateFrame(2, 0, "idle-left");
            idleFrames[(int)Facing.Up] = CreateFrame(1, 0, "idle-up");
            idleFrames[(int)Facing.Right] = CreateFrame(0, 0, "idle-right");
            idleFrames[(int)Facing.Down] = CreateFrame(3, 0, "idle-down");

            walkFrames = new Sprite[4][];
            BuildDirection(Facing.Left, 12);
            BuildDirection(Facing.Up, 6);
            BuildDirection(Facing.Right, 0);
            BuildDirection(Facing.Down, 18);
        }

        private void BuildDirection(Facing direction, int firstColumn)
        {
            var frames = new Sprite[WalkFrameCount];
            for (int index = 0; index < WalkFrameCount; index++)
            {
                frames[index] = CreateFrame(firstColumn + index, 1, $"walk-{direction}-{index}");
            }

            walkFrames[(int)direction] = frames;
        }

        private Sprite CreateFrame(int column, int rowFromTop, string suffix)
        {
            float y = characterSheet.height - (rowFromTop + 1) * CellHeight;
            Sprite sprite = Sprite.Create(
                characterSheet,
                new Rect(column * CellWidth, y, CellWidth, CellHeight),
                Vector2.zero,
                16f,
                0,
                SpriteMeshType.FullRect);
            sprite.name = $"{characterSheet.name}-{suffix}";
            generatedSprites.Add(sprite);
            return sprite;
        }

        private static Facing ResolveFacing(Vector2 movement)
        {
            if (Mathf.Abs(movement.x) > Mathf.Abs(movement.y))
            {
                return movement.x < 0f ? Facing.Left : Facing.Right;
            }

            return movement.y < 0f ? Facing.Down : Facing.Up;
        }

        private void ShowIdle()
        {
            if (idleFrames != null)
            {
                spriteRenderer.sprite = idleFrames[(int)facing];
            }
        }

        private void OnDestroy()
        {
            foreach (Sprite sprite in generatedSprites)
            {
                if (sprite != null)
                {
                    Destroy(sprite);
                }
            }
        }
    }
}
