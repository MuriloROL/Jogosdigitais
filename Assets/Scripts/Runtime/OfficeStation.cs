using System.Collections;
using ProjetoJogosDigitais.Presentation;
using UnityEngine;

namespace ProjetoJogosDigitais.Gameplay
{
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class OfficeStation : MonoBehaviour
    {
        private const float CooldownSeconds = 30f;
        [SerializeField] private OfficeActionType actionType;
        [SerializeField] private Vector2 interactionOffset = new(0f, -1.2f);
        [SerializeField] private float feedbackDuration = 0.65f;

        private bool inUse;
        private float nextAvailableAt;

        public OfficeActionType ActionType => actionType;
        public string ActionLabel => CareerGame.ActionName(actionType);
        public Vector3 InteractionPosition => transform.position + (Vector3)interactionOffset;
        public bool IsAvailable => !inUse && Time.unscaledTime >= nextAvailableAt;
        public float CooldownRemaining => Mathf.Max(0f, nextAvailableAt - Time.unscaledTime);

        public void Configure(OfficeActionType type, Vector2 offset)
        {
            actionType = type;
            interactionOffset = offset;
        }

        private void Awake()
        {
            BuildAppearance();
        }

        public void Activate(OfficePlayerController player)
        {
            if (player == null || inUse)
            {
                return;
            }

            if (!IsAvailable)
            {
                player.ShowStatus($"{ActionLabel} estará disponível em {Mathf.CeilToInt(CooldownRemaining)}s.");
                GameAudioFeedback.Play(FeedbackSound.Button);
                return;
            }

            StartCoroutine(PerformRoutine(player));
        }

        private IEnumerator PerformRoutine(OfficePlayerController player)
        {
            inUse = true;
            player.BeginInteraction(this);
            Vector3 originalScale = transform.localScale;
            float startedAt = Time.realtimeSinceStartup;
            float duration = GamePreferences.ReduceMotion ? 0.08f : feedbackDuration;
            while (Time.realtimeSinceStartup - startedAt < duration)
            {
                float pulse = 1f + Mathf.Sin((Time.realtimeSinceStartup - startedAt) * 16f) * 0.025f;
                transform.localScale = originalScale * pulse;
                yield return null;
            }

            transform.localScale = originalScale;
            OfficeActionResult result = CareerGame.Current == null
                ? new OfficeActionResult(false, "Sistema de carreira indisponível.", 0)
                : CareerGame.Current.PerformAction(actionType);
            if (result.Succeeded)
            {
                nextAvailableAt = Time.unscaledTime + CooldownSeconds;
                GameAudioFeedback.Play(FeedbackSound.Work);
            }
            inUse = false;
            player.CompleteInteraction(result.Message);
        }

        private void BuildAppearance()
        {
            Color main = actionType switch
            {
                OfficeActionType.Code => new Color(0.18f, 0.35f, 0.52f),
                OfficeActionType.Meeting => new Color(0.35f, 0.30f, 0.52f),
                OfficeActionType.Study => new Color(0.20f, 0.45f, 0.39f),
                OfficeActionType.Coffee => new Color(0.52f, 0.32f, 0.22f),
                OfficeActionType.Break => new Color(0.25f, 0.47f, 0.53f),
                _ => Color.gray
            };

            Vector2 markerPosition = interactionOffset + Vector2.down * 0.08f;
            GameObject halo = AddBlock(
                "Área interativa",
                markerPosition,
                new Vector2(0.68f, 0.68f),
                new Color(main.r, main.g, main.b, 0.55f),
                12);
            halo.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);

            GameObject core = AddBlock(
                "Indicador",
                markerPosition,
                new Vector2(0.28f, 0.28f),
                new Color(0.82f, 0.98f, 0.96f, 0.95f),
                13);
            core.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);

            CreateLabel(ActionLabel.ToUpperInvariant(), markerPosition + Vector2.down * 0.52f, main);
        }

        private GameObject AddBlock(string objectName, Vector2 position, Vector2 size, Color color, int order)
        {
            return PixelShapes.CreateBlock(transform, objectName, position, size, color, order);
        }

        private void CreateLabel(string value, Vector2 position, Color accent)
        {
            PixelShapes.CreateBlock(
                transform,
                "Fundo da legenda",
                position,
                new Vector2(Mathf.Max(1.75f, value.Length * 0.105f), 0.42f),
                new Color(0.025f, 0.04f, 0.065f, 0.88f),
                13);
            PixelShapes.CreateBlock(
                transform,
                "Acento da legenda",
                position + Vector2.up * 0.19f,
                new Vector2(Mathf.Max(1.75f, value.Length * 0.105f), 0.035f),
                new Color(accent.r, accent.g, accent.b, 0.95f),
                14);

            var labelObject = new GameObject("Legenda");
            labelObject.transform.SetParent(transform, false);
            labelObject.transform.localPosition = position;
            TextMesh text = labelObject.AddComponent<TextMesh>();
            text.text = value;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 42;
            text.characterSize = 0.078f;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.color = new Color(0.88f, 0.93f, 0.98f);
            MeshRenderer renderer = text.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = text.font.material;
            renderer.sortingOrder = 15;
        }
    }
}
