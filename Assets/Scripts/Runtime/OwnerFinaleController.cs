using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ProjetoJogosDigitais.Presentation
{
    public sealed class OwnerFinaleController : MonoBehaviour
    {
        private const float WalkSpeed = 3.4f;

        private enum FinalePhase
        {
            Intro,
            Walking,
            Celebration,
            Finished
        }

        [SerializeField] private Transform hero;
        [SerializeField] private CharacterWalkAnimator animator;
        [SerializeField] private Text title;
        [SerializeField] private Text subtitle;
        [SerializeField] private Vector3 throneApproach = new(4.4f, -0.65f, 0f);

        private readonly List<Transform> confetti = new();
        private FinalePhase phase;
        private float finaleStartedAt;
        private float phaseStartedAt;
        private GameObject throneGlow;

        public bool HasFinished => phase == FinalePhase.Finished;

        public void Configure(Transform character, CharacterWalkAnimator walkAnimator, Text titleText, Text subtitleText)
        {
            hero = character;
            animator = walkAnimator;
            title = titleText;
            subtitle = subtitleText;
        }

        private void Awake()
        {
            BuildFinaleSet();
            phase = FinalePhase.Intro;
            finaleStartedAt = Time.unscaledTime;
            phaseStartedAt = finaleStartedAt;
            title.text = "O ÚLTIMO DIA COMO FUNCIONÁRIO";
            subtitle.text = "Uma jornada que começou com um estágio...";
        }

        private void Update()
        {
            if (GamePreferences.ReduceMotion && phase != FinalePhase.Finished && Time.unscaledTime - finaleStartedAt >= 0.5f)
            {
                FinishFinale();
                return;
            }

            // The finale must always conclude even if the editor or a slow device
            // drops frames while loading the scene.
            if (phase != FinalePhase.Finished && Time.unscaledTime - finaleStartedAt >= 8f)
            {
                FinishFinale();
                return;
            }

            switch (phase)
            {
                case FinalePhase.Intro:
                    if (Time.unscaledTime - phaseStartedAt >= 1.4f)
                    {
                        phase = FinalePhase.Walking;
                        phaseStartedAt = Time.unscaledTime;
                        subtitle.text = "Alex entra na sede que ajudou a construir.";
                    }
                    break;
                case FinalePhase.Walking:
                    TickWalkToThrone();
                    break;
                case FinalePhase.Celebration:
                    TickCelebration();
                    break;
                case FinalePhase.Finished:
                    PulseThrone();
                    TickConfetti();
                    break;
            }
        }

        private void TickWalkToThrone()
        {
            Vector2 before = hero.position;
            hero.position = Vector3.MoveTowards(hero.position, throneApproach, WalkSpeed * Time.deltaTime);
            animator.SetMotion((Vector2)hero.position - before);
            if (Vector2.Distance(hero.position, throneApproach) > 0.05f)
            {
                return;
            }

            animator.SetMotion(Vector2.zero);
            hero.position = throneApproach;
            hero.localScale = Vector3.one * 1.12f;
            phase = FinalePhase.Celebration;
            phaseStartedAt = Time.unscaledTime;
            title.text = "DE ESTAGIÁRIO A DONO";
            subtitle.text = "A ByteForge agora é sua.";
            ActivateConfetti();
        }

        private void TickCelebration()
        {
            PulseThrone();
            TickConfetti();
            if (Time.unscaledTime - phaseStartedAt < 2.4f)
            {
                return;
            }

            FinishFinale();
        }

        private void FinishFinale()
        {
            phase = FinalePhase.Finished;
            phaseStartedAt = Time.unscaledTime;
            animator.SetMotion(Vector2.zero);
            title.text = "VOCÊ CONSTRUIU A BYTEFORGE";
            subtitle.text = "FIM  •  Sua carreira deixou um legado.";
        }

        private void PulseThrone()
        {
            if (throneGlow == null)
            {
                return;
            }
            float pulse = 1f + Mathf.Sin(Time.unscaledTime * 3.5f) * 0.08f;
            throneGlow.transform.localScale = new Vector3(3.8f * pulse, 4.8f * pulse, 1f);
        }

        private void TickConfetti()
        {
            for (int index = 0; index < confetti.Count; index++)
            {
                Transform piece = confetti[index];
                piece.localPosition += Vector3.down * ((0.7f + index % 4 * 0.16f) * Time.unscaledDeltaTime);
                piece.Rotate(0f, 0f, (35f + index * 4f) * Time.unscaledDeltaTime);
                if (piece.localPosition.y < -5.6f)
                {
                    Vector3 reset = piece.localPosition;
                    reset.y = 5.8f;
                    piece.localPosition = reset;
                }
            }
        }

        private void BuildFinaleSet()
        {
            PixelShapes.CreateBlock(transform, "Salão da empresa", Vector3.zero, new Vector2(23f, 12.5f), new Color(0.045f, 0.055f, 0.09f), -30);
            PixelShapes.CreateBlock(transform, "Tapete do legado", new Vector3(0f, -0.65f), new Vector2(20f, 2.7f), new Color(0.32f, 0.07f, 0.10f), -20);
            PixelShapes.CreateBlock(transform, "Borda dourada superior", new Vector3(0f, 0.67f), new Vector2(20f, 0.12f), new Color(0.88f, 0.60f, 0.18f), -19);
            PixelShapes.CreateBlock(transform, "Borda dourada inferior", new Vector3(0f, -1.97f), new Vector2(20f, 0.12f), new Color(0.88f, 0.60f, 0.18f), -19);
            PixelShapes.CreateBlock(transform, "Parede superior", new Vector3(0f, 6f), new Vector2(23f, 0.6f), new Color(0.14f, 0.16f, 0.24f), -10);
            PixelShapes.CreateBlock(transform, "Parede inferior", new Vector3(0f, -6f), new Vector2(23f, 0.6f), new Color(0.14f, 0.16f, 0.24f), -10);

            for (int index = 0; index < 7; index++)
            {
                float x = -9f + index * 3f;
                PixelShapes.CreateBlock(transform, $"Janela da cidade {index}", new Vector3(x, 4.45f), new Vector2(2.4f, 2.4f), new Color(0.08f, 0.23f, 0.36f), -15);
                PixelShapes.CreateBlock(transform, $"Luz da cidade {index}", new Vector3(x - 0.45f, 4.1f + (index % 2) * 0.5f), new Vector2(0.18f, 0.18f), new Color(1f, 0.72f, 0.25f), -14);
            }

            BuildThrone(new Vector2(5.6f, 0.35f));
            BuildCareerTotems();
            BuildConfetti();
        }

        private void BuildThrone(Vector2 position)
        {
            Transform throne = new GameObject("Trono da ByteForge").transform;
            throne.SetParent(transform, false);
            throne.localPosition = position;
            throneGlow = PixelShapes.CreateBlock(throne, "Aura dourada", Vector3.zero, new Vector2(3.8f, 4.8f), new Color(0.88f, 0.56f, 0.16f, 0.18f), 4);
            PixelShapes.CreateBlock(throne, "Encosto", new Vector3(0f, 0.55f), new Vector2(2.45f, 3.7f), new Color(0.30f, 0.08f, 0.12f), 6);
            PixelShapes.CreateBlock(throne, "Centro do encosto", new Vector3(0f, 0.55f), new Vector2(1.72f, 3.05f), new Color(0.52f, 0.12f, 0.18f), 7);
            PixelShapes.CreateBlock(throne, "Assento", new Vector3(0f, -0.8f), new Vector2(2.7f, 1.1f), new Color(0.45f, 0.10f, 0.15f), 9);
            PixelShapes.CreateBlock(throne, "Braço esquerdo", new Vector3(-1.45f, -0.25f), new Vector2(0.46f, 1.55f), new Color(0.85f, 0.57f, 0.16f), 8);
            PixelShapes.CreateBlock(throne, "Braço direito", new Vector3(1.45f, -0.25f), new Vector2(0.46f, 1.55f), new Color(0.85f, 0.57f, 0.16f), 8);
            PixelShapes.CreateBlock(throne, "Coroa esquerda", new Vector3(-0.75f, 2.6f), new Vector2(0.5f, 0.9f), new Color(0.95f, 0.70f, 0.20f), 9);
            PixelShapes.CreateBlock(throne, "Coroa centro", new Vector3(0f, 2.82f), new Vector2(0.58f, 1.15f), new Color(1f, 0.76f, 0.24f), 9);
            PixelShapes.CreateBlock(throne, "Coroa direita", new Vector3(0.75f, 2.6f), new Vector2(0.5f, 0.9f), new Color(0.95f, 0.70f, 0.20f), 9);
        }

        private void BuildCareerTotems()
        {
            string[] labels = { "ESTÁGIO", "JÚNIOR", "PLENO", "SÊNIOR", "LEAD", "CTO" };
            for (int index = 0; index < labels.Length; index++)
            {
                float x = -8.4f + index * 2.3f;
                float height = 0.75f + index * 0.22f;
                PixelShapes.CreateBlock(transform, $"Marco {labels[index]}", new Vector3(x, 2f), new Vector2(1.4f, height), Color.Lerp(new Color(0.18f, 0.31f, 0.46f), new Color(0.86f, 0.58f, 0.18f), index / 5f), -5);
                CreateWorldText(labels[index], new Vector2(x, 2f), Color.white, 2, 0.052f);
            }
        }

        private void BuildConfetti()
        {
            Color[] colors =
            {
                new Color(0.24f, 0.86f, 0.76f),
                new Color(0.95f, 0.66f, 0.20f),
                new Color(0.70f, 0.42f, 0.92f),
                new Color(0.94f, 0.30f, 0.38f)
            };
            for (int index = 0; index < 28; index++)
            {
                float x = -10f + (index * 3.7f) % 20f;
                float y = 1.5f + (index * 1.9f) % 4.5f;
                GameObject piece = PixelShapes.CreateBlock(transform, $"Confete {index}", new Vector3(x, y), new Vector2(0.14f, 0.32f), colors[index % colors.Length], 50);
                piece.SetActive(false);
                confetti.Add(piece.transform);
            }
        }

        private void ActivateConfetti()
        {
            foreach (Transform piece in confetti)
            {
                piece.gameObject.SetActive(true);
            }
        }

        private void CreateWorldText(string value, Vector2 position, Color color, int order, float size)
        {
            var textObject = new GameObject(value);
            textObject.transform.SetParent(transform, false);
            textObject.transform.localPosition = position;
            TextMesh text = textObject.AddComponent<TextMesh>();
            text.text = value;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 44;
            text.characterSize = size;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.color = color;
            MeshRenderer renderer = text.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = text.font.material;
            renderer.sortingOrder = order;
        }
    }
}
