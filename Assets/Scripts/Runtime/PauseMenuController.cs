using ProjetoJogosDigitais.Gameplay;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ProjetoJogosDigitais.Presentation
{
    public sealed class PauseMenuController : MonoBehaviour
    {
        private GameObject overlay;
        private Button pauseButton;
        private Button resumeButton;
        private Button settingsButton;
        private Button tutorialButton;
        private Button saveButton;
        private Button menuButton;

        public bool IsPaused => overlay != null && overlay.activeSelf;

        private void Start()
        {
            BuildInterface();
            overlay.SetActive(false);
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            {
                if (FindFirstObjectByType<IdleManagementPanel>()?.SettingsVisible == true)
                {
                    return;
                }
                TogglePause();
            }
        }

        private void OnDestroy()
        {
            Time.timeScale = 1f;
            pauseButton?.onClick.RemoveListener(OpenPause);
            resumeButton?.onClick.RemoveListener(Resume);
            settingsButton?.onClick.RemoveListener(OpenSettings);
            tutorialButton?.onClick.RemoveListener(OpenTutorial);
            saveButton?.onClick.RemoveListener(SaveNow);
            menuButton?.onClick.RemoveListener(ReturnToMenu);
        }

        public void TogglePause()
        {
            if (IsPaused) Resume(); else OpenPause();
        }

        private void OpenPause()
        {
            overlay.SetActive(true);
            overlay.transform.SetAsLastSibling();
            Time.timeScale = 0f;
            GameAudioFeedback.Play(FeedbackSound.Button);
        }

        private void Resume()
        {
            overlay.SetActive(false);
            Time.timeScale = 1f;
            GameAudioFeedback.Play(FeedbackSound.Button);
        }

        private void OpenSettings()
        {
            FindFirstObjectByType<IdleManagementPanel>()?.ShowSettings();
        }

        private void OpenTutorial()
        {
            Resume();
            FindFirstObjectByType<CareerTutorialController>()?.ShowTutorial();
        }

        private void SaveNow()
        {
            CareerGame.Current?.SaveGame();
            GameAudioFeedback.Play(FeedbackSound.Mission);
        }

        private void ReturnToMenu()
        {
            CareerGame game = CareerGame.Current;
            game?.SaveGame();
            game?.ReleaseSession();
            Time.timeScale = 1f;
            SceneManager.LoadScene(CareerSceneFlow.MainMenuScene, LoadSceneMode.Single);
        }

        private void BuildInterface()
        {
            pauseButton = CreateButton(transform, "PAUSA • ESC", new Vector2(222f, -18f), new Vector2(120f, 32f), new Vector2(0.5f, 1f), new Color(0.18f, 0.31f, 0.46f));
            pauseButton.onClick.AddListener(OpenPause);

            overlay = CreateImage(transform, "Menu de pausa", new Color(0.005f, 0.01f, 0.02f, 0.90f)).gameObject;
            SetRect((RectTransform)overlay.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            overlay.GetComponent<Image>().raycastTarget = true;

            Image panel = CreateImage(overlay.transform, "Painel da pausa", Color.white);
            SetRect(panel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(620f, 560f));
            panel.gameObject.AddComponent<UiRoundedGraphic>().Configure(22);
            panel.gameObject.AddComponent<UiVerticalGradient>().Configure(
                new Color(0.085f, 0.14f, 0.19f, 0.995f),
                new Color(0.025f, 0.05f, 0.085f, 0.995f));
            Outline outline = panel.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.24f, 0.86f, 0.76f, 0.50f);
            outline.effectDistance = new Vector2(2f, -2f);

            Text eyebrow = CreateText(panel.transform, "BYTEFORGE • EXPEDIENTE EM ANDAMENTO", 11, TextAnchor.MiddleCenter, new Color(0.24f, 0.86f, 0.76f), FontStyle.Bold);
            SetRect(eyebrow.rectTransform, new Vector2(0f, 1f), Vector2.one, new Vector2(30f, -42f), new Vector2(-60f, 22f));
            Text title = CreateText(panel.transform, "PAUSA", 32, TextAnchor.MiddleCenter, Color.white, FontStyle.Bold);
            SetRect(title.rectTransform, new Vector2(0f, 1f), Vector2.one, new Vector2(30f, -82f), new Vector2(-60f, 46f));
            Text hint = CreateText(panel.transform, "A produção idle continua enquanto este menu está aberto.", 13, TextAnchor.MiddleCenter, new Color(0.76f, 0.84f, 0.91f));
            SetRect(hint.rectTransform, new Vector2(0f, 1f), Vector2.one, new Vector2(30f, -120f), new Vector2(-60f, 30f));

            resumeButton = CreateMenuButton(panel.transform, "CONTINUAR", -174f, new Color(0.24f, 0.76f, 0.68f));
            settingsButton = CreateMenuButton(panel.transform, "CONFIGURAÇÕES", -234f, new Color(0.22f, 0.40f, 0.58f));
            tutorialButton = CreateMenuButton(panel.transform, "REVER TUTORIAL", -294f, new Color(0.30f, 0.28f, 0.52f));
            saveButton = CreateMenuButton(panel.transform, "SALVAR AGORA", -354f, new Color(0.33f, 0.48f, 0.40f));
            menuButton = CreateMenuButton(panel.transform, "SALVAR E VOLTAR AO MENU", -430f, new Color(0.52f, 0.28f, 0.24f));
            resumeButton.onClick.AddListener(Resume);
            settingsButton.onClick.AddListener(OpenSettings);
            tutorialButton.onClick.AddListener(OpenTutorial);
            saveButton.onClick.AddListener(SaveNow);
            menuButton.onClick.AddListener(ReturnToMenu);
        }

        private static Button CreateMenuButton(Transform parent, string label, float y, Color color)
        {
            return CreateButton(parent, label, new Vector2(70f, y), new Vector2(-140f, 48f), new Vector2(0f, 1f), color, true);
        }

        private static Image CreateImage(Transform parent, string name, Color color)
        {
            var instance = new GameObject(name, typeof(RectTransform));
            instance.transform.SetParent(parent, false);
            Image image = instance.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static Text CreateText(Transform parent, string value, int size, TextAnchor anchor, Color color, FontStyle style = FontStyle.Normal)
        {
            var holder = new GameObject(value, typeof(RectTransform));
            holder.transform.SetParent(parent, false);
            Text text = holder.AddComponent<Text>();
            text.text = value;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size;
            text.fontStyle = style;
            text.alignment = anchor;
            text.color = color;
            text.raycastTarget = false;
            return text;
        }

        private static Button CreateButton(Transform parent, string label, Vector2 position, Vector2 size, Vector2 anchor, Color color, bool stretch = false)
        {
            Image image = CreateImage(parent, label, Color.white);
            Vector2 anchorMax = stretch ? new Vector2(1f, anchor.y) : anchor;
            SetRect(image.rectTransform, anchor, anchorMax, position, size, anchor);
            image.raycastTarget = true;
            image.gameObject.AddComponent<UiRoundedGraphic>().Configure(12);
            image.gameObject.AddComponent<UiVerticalGradient>().Configure(
                Color.Lerp(color, Color.white, 0.10f),
                Color.Lerp(color, Color.black, 0.20f));
            Button button = image.gameObject.AddComponent<Button>();
            image.gameObject.AddComponent<UiButtonMotion>();
            Text text = CreateText(image.transform, label, 13, TextAnchor.MiddleCenter, Color.white, FontStyle.Bold);
            SetRect(text.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            return button;
        }

        private static void SetRect(RectTransform rect, Vector2 min, Vector2 max, Vector2 position, Vector2 size, Vector2? pivot = null)
        {
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.pivot = pivot ?? new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }
    }
}
