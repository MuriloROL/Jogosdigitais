using System;
using System.Collections;
using UnityEngine;

namespace ProjetoJogosDigitais.Gameplay
{
    [RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
    public sealed class FridgeInteractable : MonoBehaviour
    {
        [SerializeField] private Vector2 interactionOffset = new(-0.65f, -0.2f);
        [SerializeField, Min(0f)] private float hungerRestored = 0.48f;
        [SerializeField] private Sprite[] animationFrames;

        private SpriteRenderer spriteRenderer;
        private bool inUse;

        public Vector3 InteractionPosition => transform.position + (Vector3)interactionOffset;
        public bool InUse => inUse;
        public event Action UseCompleted;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void Configure(Sprite[] frames, Vector2 offset)
        {
            animationFrames = frames;
            interactionOffset = offset;
        }

        public void Use(SimNeeds needs)
        {
            if (!inUse)
            {
                StartCoroutine(EatRoutine(needs));
            }
        }

        private IEnumerator EatRoutine(SimNeeds needs)
        {
            inUse = true;
            spriteRenderer ??= GetComponent<SpriteRenderer>();
            Sprite original = spriteRenderer.sprite;
            int lastOpeningFrame = animationFrames == null
                ? -1
                : Mathf.Min(4, animationFrames.Length - 1);
            const float secondsPerOpeningFrame = 0.11f;
            const float holdSeconds = 0.45f;
            const float secondsPerClosingFrame = 0.08f;
            float openingDuration = (lastOpeningFrame + 1) * secondsPerOpeningFrame;
            float closingDuration = (lastOpeningFrame + 1) * secondsPerClosingFrame;
            float restoreAt = openingDuration + holdSeconds;
            float totalDuration = restoreAt + closingDuration;
            float startedAt = Time.realtimeSinceStartup;
            bool restored = false;

            while (Time.realtimeSinceStartup - startedAt < totalDuration)
            {
                float elapsed = Time.realtimeSinceStartup - startedAt;
                if (!restored && elapsed >= restoreAt)
                {
                    needs.Eat(hungerRestored);
                    restored = true;
                }

                if (lastOpeningFrame >= 0)
                {
                    int frameIndex;
                    if (elapsed < openingDuration)
                    {
                        frameIndex = Mathf.Min(
                            Mathf.FloorToInt(elapsed / secondsPerOpeningFrame),
                            lastOpeningFrame);
                    }
                    else if (elapsed < restoreAt)
                    {
                        frameIndex = lastOpeningFrame;
                    }
                    else
                    {
                        int closingStep = Mathf.FloorToInt((elapsed - restoreAt) / secondsPerClosingFrame);
                        frameIndex = Mathf.Max(lastOpeningFrame - closingStep, 0);
                    }

                    if (animationFrames[frameIndex] != null)
                    {
                        spriteRenderer.sprite = animationFrames[frameIndex];
                    }
                }

                yield return null;
            }

            if (!restored)
            {
                needs.Eat(hungerRestored);
            }

            spriteRenderer.sprite = original;
            inUse = false;
            UseCompleted?.Invoke();
        }
    }
}
