using ProjetoJogosDigitais.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace ProjetoJogosDigitais.Presentation
{
    public sealed class CareerTutorialController : MonoBehaviour
    {
        public const string SeenPlayerPrefsKey = "byteforge.tutorial.seen.v1";

        private static readonly string[] StepTitles =
        {
            "BEM-VINDO À BYTEFORGE",
            "ENTREGA E XP",
            "CUIDE DO SEU RITMO",
            "CONSTRUA SUA CARREIRA"
        };

        private static readonly string[] StepBodies =
        {
            "Clique nos marcadores do escritório para Alex caminhar até uma estação e realizar uma atividade.",
            "Entrega mede a meta do dia e volta a zero no próximo expediente. XP é permanente e libera promoções.",
            "Programar consome energia e aumenta o estresse. Use café e pausas para manter a produtividade.",
            "Complete a tarefa extra do dia, reaja aos eventos da carreira e use Salvar. Quando terminar, encerre o dia."
        };

        private GameObject overlay;
        private Text titleLabel;
        private Text bodyLabel;
        private Text progressLabel;
        private Button nextButton;
        private Button skipButton;
        private int currentStep;

        public bool IsVisible => overlay != null && overlay.activeSelf;
        public int CurrentStep => currentStep;
        public int StepCount => StepTitles.Length;

        private void Start()
        {
            CareerGame game = CareerGame.Current;
            if (game != null && game.Rank == CareerRank.Intern && game.Day == 1 &&
                PlayerPrefs.GetInt(SeenPlayerPrefsKey, 0) == 0)
            {
                ShowTutorial();
            }
        }

        private void OnDestroy()
        {
            nextButton?.onClick.RemoveListener(NextStep);
            skipButton?.onClick.RemoveListener(SkipTutorial);
        }

        public void ShowTutorial()
        {
            if (overlay == null)
            {
                BuildInterface();
            }

            currentStep = 0;
            overlay.SetActive(true);
            RefreshStep();
        }

        public void NextStep()
        {
            currentStep++;
            if (currentStep >= StepTitles.Length)
            {
                CompleteTutorial();
                return;
            }

            RefreshStep();
        }

        public void SkipTutorial()
        {
            CompleteTutorial();
        }

        private void CompleteTutorial()
        {
            PlayerPrefs.SetInt(SeenPlayerPrefsKey, 1);
            PlayerPrefs.Save();
            overlay.SetActive(false);
        }

        private void RefreshStep()
        {
            titleLabel.text = StepTitles[currentStep];
            bodyLabel.text = StepBodies[currentStep];
            progressLabel.text = $"PASSO {currentStep + 1}/{StepTitles.Length}";
            nextButton.GetComponentInChildren<Text>().text = currentStep == StepTitles.Length - 1
                ? "COMEÇAR"
                : "PRÓXIMO";
        }

        private void BuildInterface()
        {
            overlay = CreateImage(transform, "Tutorial inicial", new Color(0.008f, 0.015f, 0.028f, 0.88f)).gameObject;
            SetRect((RectTransform)overlay.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            overlay.GetComponent<Image>().raycastTarget = true;
            overlay.transform.SetAsLastSibling();

            Image panel = CreateImage(overlay.transform, "Painel do tutorial", new Color(0.05f, 0.075f, 0.115f, 0.99f));
            SetRect(panel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(640f, 390f));
            Outline outline = panel.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.24f, 0.82f, 0.76f, 0.8f);
            outline.effectDistance = new Vector2(2f, -2f);

            Image accent = CreateImage(panel.transform, "Linha de destaque", new Color(0.24f, 0.82f, 0.76f));
            SetRect(accent.rectTransform, new Vector2(0f, 1f), Vector2.one, Vector2.zero, new Vector2(0f, 5f));
            accent.rectTransform.pivot = new Vector2(0.5f, 1f);

            progressLabel = CreateText(panel.transform, "PASSO 1/4", 12, TextAnchor.MiddleLeft, new Color(0.24f, 0.82f, 0.76f), FontStyle.Bold);
            SetRect(progressLabel.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(34f, -38f), new Vector2(-68f, 24f));

            titleLabel = CreateText(panel.transform, StepTitles[0], 30, TextAnchor.MiddleLeft, Color.white, FontStyle.Bold);
            SetRect(titleLabel.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(34f, -88f), new Vector2(-68f, 52f));

            bodyLabel = CreateText(panel.transform, StepBodies[0], 19, TextAnchor.UpperLeft, new Color(0.78f, 0.86f, 0.94f));
            bodyLabel.lineSpacing = 1.25f;
            SetRect(bodyLabel.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(34f, -150f), new Vector2(-68f, 125f));

            skipButton = CreateButton(panel.transform, "PULAR TUTORIAL", new Vector2(34f, 28f), new Vector2(190f, 48f), new Vector2(0f, 0f), new Color(0.20f, 0.28f, 0.40f));
            skipButton.onClick.AddListener(SkipTutorial);
            nextButton = CreateButton(panel.transform, "PRÓXIMO", new Vector2(-34f, 28f), new Vector2(190f, 48f), new Vector2(1f, 0f), new Color(0.24f, 0.82f, 0.76f));
            nextButton.onClick.AddListener(NextStep);
        }

        private static Image CreateImage(Transform parent, string objectName, Color color)
        {
            var imageObject = new GameObject(objectName, typeof(RectTransform));
            imageObject.transform.SetParent(parent, false);
            Image image = imageObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static Text CreateText(
            Transform parent,
            string value,
            int fontSize,
            TextAnchor alignment,
            Color color,
            FontStyle style = FontStyle.Normal)
        {
            var textObject = new GameObject(value, typeof(RectTransform));
            textObject.transform.SetParent(parent, false);
            Text text = textObject.AddComponent<Text>();
            text.text = value;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.alignment = alignment;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.raycastTarget = false;
            return text;
        }

        private static Button CreateButton(
            Transform parent,
            string label,
            Vector2 position,
            Vector2 size,
            Vector2 anchor,
            Color color)
        {
            Image image = CreateImage(parent, label, color);
            image.raycastTarget = true;
            SetRect(image.rectTransform, anchor, anchor, position, size);
            image.rectTransform.pivot = anchor;
            Button button = image.gameObject.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.highlightedColor = Color.Lerp(color, Color.white, 0.18f);
            colors.pressedColor = Color.Lerp(color, Color.black, 0.18f);
            button.colors = colors;
            Text text = CreateText(image.transform, label, 14, TextAnchor.MiddleCenter, Color.white, FontStyle.Bold);
            SetRect(text.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            return button;
        }

        private static void SetRect(
            RectTransform rect,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 position,
            Vector2 size)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }
    }
}
