using ProjetoJogosDigitais.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace ProjetoJogosDigitais.Presentation
{
    public sealed class MissionBriefingController : MonoBehaviour
    {
        private GameObject overlay;
        private Text eyebrowLabel;
        private Text titleLabel;
        private Text bodyLabel;
        private Text checklistLabel;
        private Text rewardLabel;
        private Button startButton;
        private CareerGame game;

        public bool IsVisible => overlay != null && overlay.activeSelf;

        private void Start()
        {
            BuildInterface();
            overlay.SetActive(false);
        }

        private void Update()
        {
            game ??= CareerGame.Current;
            if (game == null || IsVisible || !game.MissionBriefingPending || game.HasPendingCareerDecision)
            {
                return;
            }

            CareerTutorialController tutorial = GetComponent<CareerTutorialController>();
            CareerDecisionController decision = GetComponent<CareerDecisionController>();
            if (tutorial != null && tutorial.IsVisible || decision != null && decision.IsVisible)
            {
                return;
            }

            ShowBriefing();
        }

        private void OnDestroy()
        {
            startButton?.onClick.RemoveListener(StartMission);
        }

        private void ShowBriefing()
        {
            eyebrowLabel.text = $"BRIEFING • {game.RankName.ToUpperInvariant()} • MISSÃO {game.MissionNumber}/{game.MissionsPerRank}";
            titleLabel.text = game.MissionTitle;
            bodyLabel.text = game.MissionBriefing;
            checklistLabel.text = game.MissionChecklistText();
            rewardLabel.text = $"RECOMPENSA  R$ {game.MissionRewardMoney}  •  +{game.MissionRewardXp} XP";
            overlay.SetActive(true);
            overlay.transform.SetAsLastSibling();
            GameAudioFeedback.Play(FeedbackSound.Mission);
        }

        private void StartMission()
        {
            game?.AcknowledgeMissionBriefing();
            overlay.SetActive(false);
            GameAudioFeedback.Play(FeedbackSound.Button);
        }

        private void BuildInterface()
        {
            overlay = CreateImage(transform, "Briefing da missão", new Color(0.006f, 0.012f, 0.024f, 0.90f)).gameObject;
            SetRect((RectTransform)overlay.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            overlay.GetComponent<Image>().raycastTarget = true;

            Image panel = CreateImage(overlay.transform, "Cartão da missão", Color.white);
            SetRect(panel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(780f, 500f));
            panel.gameObject.AddComponent<UiRoundedGraphic>().Configure(22);
            panel.gameObject.AddComponent<UiVerticalGradient>().Configure(
                new Color(0.075f, 0.13f, 0.17f, 0.995f),
                new Color(0.025f, 0.05f, 0.085f, 0.995f));
            Outline outline = panel.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.24f, 0.86f, 0.76f, 0.55f);
            outline.effectDistance = new Vector2(2f, -2f);

            Image accent = CreateImage(panel.transform, "Linha do briefing", new Color(0.24f, 0.86f, 0.76f));
            SetRect(accent.rectTransform, new Vector2(0f, 1f), Vector2.one, new Vector2(18f, -3f), new Vector2(-36f, 5f), new Vector2(0.5f, 1f));
            accent.gameObject.AddComponent<UiRoundedGraphic>().Configure(4);

            eyebrowLabel = CreateText(panel.transform, "BRIEFING", 12, TextAnchor.MiddleLeft, new Color(0.24f, 0.86f, 0.76f), FontStyle.Bold);
            SetRect(eyebrowLabel.rectTransform, new Vector2(0f, 1f), Vector2.one, new Vector2(34f, -42f), new Vector2(-68f, 24f));
            titleLabel = CreateText(panel.transform, "MISSÃO", 28, TextAnchor.MiddleLeft, Color.white, FontStyle.Bold);
            SetRect(titleLabel.rectTransform, new Vector2(0f, 1f), Vector2.one, new Vector2(34f, -85f), new Vector2(-68f, 44f));
            bodyLabel = CreateText(panel.transform, "Briefing da missão.", 16, TextAnchor.UpperLeft, new Color(0.82f, 0.88f, 0.94f));
            bodyLabel.lineSpacing = 1.22f;
            SetRect(bodyLabel.rectTransform, new Vector2(0f, 1f), Vector2.one, new Vector2(34f, -154f), new Vector2(-68f, 86f));

            Image checklistPanel = CreateImage(panel.transform, "Checklist do briefing", new Color(0.025f, 0.045f, 0.075f, 0.96f));
            SetRect(checklistPanel.rectTransform, new Vector2(0f, 1f), Vector2.one, new Vector2(34f, -292f), new Vector2(-68f, 128f));
            checklistPanel.gameObject.AddComponent<UiRoundedGraphic>().Configure(14);
            checklistLabel = CreateText(checklistPanel.transform, "□ Objetivo", 15, TextAnchor.MiddleLeft, Color.white, FontStyle.Bold);
            checklistLabel.lineSpacing = 1.32f;
            SetRect(checklistLabel.rectTransform, Vector2.zero, Vector2.one, new Vector2(22f, 10f), new Vector2(-44f, -20f));

            rewardLabel = CreateText(panel.transform, "RECOMPENSA", 13, TextAnchor.MiddleLeft, new Color(0.98f, 0.69f, 0.27f), FontStyle.Bold);
            SetRect(rewardLabel.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(34f, 82f), new Vector2(-350f, 28f));
            startButton = CreateButton(panel.transform, "COMEÇAR MISSÃO", new Vector2(-34f, 34f), new Vector2(270f, 56f), new Vector2(1f, 0f));
            startButton.onClick.AddListener(StartMission);
        }

        private static Image CreateImage(Transform parent, string objectName, Color color)
        {
            var gameObject = new GameObject(objectName, typeof(RectTransform));
            gameObject.transform.SetParent(parent, false);
            Image image = gameObject.AddComponent<Image>();
            image.color = color;
            return image;
        }

        private static Text CreateText(Transform parent, string value, int size, TextAnchor alignment, Color color, FontStyle style = FontStyle.Normal)
        {
            var gameObject = new GameObject(value, typeof(RectTransform));
            gameObject.transform.SetParent(parent, false);
            Text text = gameObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = value;
            text.fontSize = size;
            text.alignment = alignment;
            text.color = color;
            text.fontStyle = style;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }

        private static Button CreateButton(Transform parent, string label, Vector2 position, Vector2 size, Vector2 anchor)
        {
            Image image = CreateImage(parent, label, Color.white);
            SetRect(image.rectTransform, anchor, anchor, position, size, anchor);
            image.gameObject.AddComponent<UiRoundedGraphic>().Configure(13);
            image.gameObject.AddComponent<UiVerticalGradient>().Configure(
                new Color(0.30f, 0.91f, 0.82f),
                new Color(0.12f, 0.60f, 0.58f));
            Button button = image.gameObject.AddComponent<Button>();
            image.gameObject.AddComponent<UiButtonMotion>();
            Text text = CreateText(image.transform, label, 14, TextAnchor.MiddleCenter, Color.white, FontStyle.Bold);
            SetRect(text.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            return button;
        }

        private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size, Vector2? pivot = null)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot ?? new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }
    }
}
