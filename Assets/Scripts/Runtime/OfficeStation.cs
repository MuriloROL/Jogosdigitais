using System.Collections;
using ProjetoJogosDigitais.Presentation;
using UnityEngine;

namespace ProjetoJogosDigitais.Gameplay
{
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class OfficeStation : MonoBehaviour
    {
        private const float InteractionOffsetScale = 0.82f;
        [SerializeField] private OfficeActionType actionType;
        [SerializeField] private Vector2 interactionOffset = new(0f, -1.2f);
        [SerializeField] private float feedbackDuration = 0.65f;

        private bool inUse;
        private Transform interactionMarker;
        private BoxCollider2D interactionCollider;
        private SpriteRenderer markerHalo;
        private SpriteRenderer markerCore;
        private SpriteRenderer markerLabelBackground;
        private Color markerColor;
        private bool pointerFocused;
        private bool missionFocused;

        public OfficeActionType ActionType => actionType;
        public string ActionLabel => CareerGame.ActionName(actionType);
        public Vector3 InteractionPosition => transform.position + (Vector3)EffectiveInteractionOffset;
        public Collider2D InteractionCollider => interactionCollider;
        public bool IsPointerFocused => pointerFocused;
        public bool IsAvailable => !inUse;
        public float CooldownRemaining => 0f;
        private Vector2 EffectiveInteractionOffset => interactionOffset * InteractionOffsetScale;

        public void Configure(OfficeActionType type, Vector2 offset)
        {
            actionType = type;
            interactionOffset = offset;
        }

        private void Awake()
        {
            BuildAppearance();
        }

        private void Update()
        {
            if (interactionMarker == null)
            {
                return;
            }

            float pulse = GamePreferences.ReduceMotion
                ? 1f
                : 1f + Mathf.Sin(Time.unscaledTime * 4.5f) * 0.035f;
            float focusScale = pointerFocused ? 1.12f : missionFocused ? 1.06f : 1f;
            interactionMarker.localScale = Vector3.one * pulse * focusScale;
        }

        public void SetPointerFocus(bool focused)
        {
            if (pointerFocused == focused)
            {
                return;
            }

            pointerFocused = focused;
            RefreshMarkerState();
        }

        public void SetMissionFocus(bool focused)
        {
            if (missionFocused == focused)
            {
                return;
            }

            missionFocused = focused;
            RefreshMarkerState();
        }

        private void RefreshMarkerState()
        {
            if (markerHalo != null)
            {
                markerHalo.color = pointerFocused
                    ? new Color(0.36f, 1f, 0.90f, 0.92f)
                    : missionFocused
                        ? new Color(markerColor.r, markerColor.g, markerColor.b, 0.95f)
                    : new Color(markerColor.r, markerColor.g, markerColor.b, 0.68f);
            }
            if (markerCore != null)
            {
                markerCore.color = pointerFocused
                    ? Color.white
                    : missionFocused
                        ? new Color(1f, 0.76f, 0.24f, 1f)
                        : new Color(0.84f, 0.99f, 0.96f, 0.98f);
            }
            if (markerLabelBackground != null)
            {
                markerLabelBackground.color = pointerFocused || missionFocused
                    ? new Color(markerColor.r, markerColor.g, markerColor.b, 0.98f)
                    : new Color(0.025f, 0.055f, 0.08f, 0.94f);
            }
        }

        public void Activate(OfficePlayerController player)
        {
            if (player == null || inUse)
            {
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
                GameAudioFeedback.PlayForAction(actionType);
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

            markerColor = main;
            Vector2 markerPosition = EffectiveInteractionOffset;
            float connectorHeight = Mathf.Max(0.2f, Mathf.Abs(markerPosition.y) - 0.48f);
            AddBlock(
                "Linha até a estação",
                new Vector2(markerPosition.x, markerPosition.y * 0.5f),
                new Vector2(0.07f, connectorHeight),
                new Color(main.r, main.g, main.b, 0.32f),
                11);

            var markerObject = new GameObject($"Clique aqui • {ActionLabel}");
            markerObject.transform.SetParent(transform, false);
            markerObject.transform.localPosition = markerPosition;
            interactionMarker = markerObject.transform;

            string clickLabel = $"CLIQUE • {ShortActionLabel()}";
            float labelWidth = Mathf.Max(2.20f, clickLabel.Length * 0.105f);
            float labelDirection = markerPosition.y < 0f ? -1f : 1f;
            Vector2 labelPosition = Vector2.up * labelDirection * 0.60f;

            interactionCollider = markerObject.AddComponent<BoxCollider2D>();
            interactionCollider.isTrigger = true;
            interactionCollider.size = new Vector2(labelWidth + 0.50f, 1.55f);
            interactionCollider.offset = Vector2.up * labelDirection * 0.25f;

            GameObject halo = PixelShapes.CreateBlock(
                interactionMarker,
                "Alvo clicável",
                Vector3.zero,
                new Vector2(0.68f, 0.68f),
                new Color(main.r, main.g, main.b, 0.68f),
                12);
            halo.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
            markerHalo = halo.GetComponent<SpriteRenderer>();

            GameObject core = PixelShapes.CreateBlock(
                interactionMarker,
                "Centro do clique",
                Vector3.zero,
                new Vector2(0.26f, 0.26f),
                new Color(0.84f, 0.99f, 0.96f, 0.98f),
                13);
            core.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
            markerCore = core.GetComponent<SpriteRenderer>();

            GameObject labelBackground = PixelShapes.CreateBlock(
                interactionMarker,
                "Botão da atividade",
                labelPosition,
                new Vector2(labelWidth + 0.25f, 0.54f),
                new Color(0.025f, 0.055f, 0.08f, 0.94f),
                13);
            markerLabelBackground = labelBackground.GetComponent<SpriteRenderer>();
            CreateLabel(clickLabel, interactionMarker, labelPosition, main);
        }

        private GameObject AddBlock(string objectName, Vector2 position, Vector2 size, Color color, int order)
        {
            return PixelShapes.CreateBlock(transform, objectName, position, size, color, order);
        }

        private string ShortActionLabel()
        {
            return actionType switch
            {
                OfficeActionType.Code => "PROGRAMAR",
                OfficeActionType.Meeting => "REUNIÃO",
                OfficeActionType.Study => "ESTUDAR",
                OfficeActionType.Coffee => "CAFÉ",
                OfficeActionType.Break => "PAUSA",
                _ => ActionLabel.ToUpperInvariant()
            };
        }

        private void CreateLabel(string value, Transform parent, Vector2 position, Color accent)
        {
            PixelShapes.CreateBlock(
                parent,
                "Acento da legenda",
                position + Vector2.up * 0.16f,
                new Vector2(Mathf.Max(2.20f, value.Length * 0.105f), 0.035f),
                new Color(accent.r, accent.g, accent.b, 0.95f),
                14);

            var labelObject = new GameObject("Legenda");
            labelObject.transform.SetParent(parent, false);
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
