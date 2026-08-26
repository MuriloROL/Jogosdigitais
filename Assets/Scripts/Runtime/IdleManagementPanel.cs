using ProjetoJogosDigitais.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace ProjetoJogosDigitais.Presentation
{
    public sealed class IdleManagementPanel : MonoBehaviour
    {
        private static readonly Color PanelColor = new(0.035f, 0.06f, 0.095f, 0.98f);
        private static readonly Color Accent = new(0.24f, 0.82f, 0.76f);
        private static readonly Color Amber = new(0.96f, 0.63f, 0.20f);

        private CareerGame game;
        private Image managementPanel;
        private Text productionLabel;
        private Text teamLabel;
        private Image cycleFill;
        private Button hireButton;
        private Button automationButton;
        private GameObject settingsOverlay;
        private Button musicButton;
        private Button effectsButton;
        private Button motionButton;
        private Button textButton;
        private int observedXp = -1;
        private float productionFlashUntil;

        public Button HireButton => hireButton;
        public Button AutomationButton => automationButton;
        public bool SettingsVisible => settingsOverlay != null && settingsOverlay.activeSelf;

        private void Start()
        {
            BuildInterface();
            BindGame();
            GamePreferences.Changed += RefreshPreferences;
            Refresh();
            if (managementPanel != null)
            {
                float flash = GamePreferences.ReduceMotion
                    ? 0f
                    : Mathf.Clamp01((productionFlashUntil - Time.unscaledTime) / 0.3f);
                managementPanel.color = Color.Lerp(PanelColor, new Color(0.07f, 0.18f, 0.19f, 0.98f), flash);
            }
        }

        private void Update()
        {
            if (game == null)
            {
                BindGame();
            }

            Refresh();
        }

        private void OnDestroy()
        {
            if (game != null)
            {
                game.Changed -= Refresh;
                game.Promoted -= OnPromoted;
            }
            GamePreferences.Changed -= RefreshPreferences;
        }

        private void BindGame()
        {
            if (game == CareerGame.Current)
            {
                return;
            }
            if (game != null)
            {
                game.Changed -= Refresh;
                game.Promoted -= OnPromoted;
            }
            game = CareerGame.Current;
            if (game != null)
            {
                game.Changed += Refresh;
                game.Promoted += OnPromoted;
            }
        }

        private void OnPromoted(CareerRank promotedRank)
        {
            GameAudioFeedback.Play(FeedbackSound.Promotion);
            Refresh();
        }

        private void Hire()
        {
            if (game != null && game.HireTeammate())
            {
                GameAudioFeedback.Play(FeedbackSound.Purchase);
            }
            else
            {
                GameAudioFeedback.Play(FeedbackSound.Button);
            }
            Refresh();
        }

        private void UpgradeAutomation()
        {
            if (game != null && game.UpgradeAutomation())
            {
                GameAudioFeedback.Play(FeedbackSound.Purchase);
            }
            else
            {
                GameAudioFeedback.Play(FeedbackSound.Button);
            }
            Refresh();
        }

        private void Refresh()
        {
            if (game == null || productionLabel == null)
            {
                return;
            }

            productionLabel.text = game.IsLastRank
                ? "CARREIRA CONCLUÍDA"
                : $"+{game.PassiveDeliveryPerTick} ENTREGA / 15s\nPRÓXIMO CICLO {Mathf.CeilToInt(game.SecondsUntilPassiveTick)}s";
            teamLabel.text = $"EQUIPE {game.TeamMembers}/{game.TeamCapacity}  •  PIPELINE NV.{game.AutomationLevel}";
            cycleFill.fillAmount = game.IsLastRank ? 1f : 1f - game.SecondsUntilPassiveTick / 15f;
            if (observedXp >= 0 && game.CareerXp > observedXp)
            {
                productionFlashUntil = Time.unscaledTime + 0.3f;
            }
            observedXp = game.CareerXp;

            hireButton.interactable = !game.IsLastRank && game.TeamMembers < game.TeamCapacity;
            SetButtonLabel(hireButton, game.TeamMembers >= game.TeamCapacity
                ? "EQUIPE COMPLETA"
                : $"CONTRATAR  R$ {game.NextHireCost}");
            automationButton.interactable = !game.IsLastRank && game.AutomationLevel < 5;
            SetButtonLabel(automationButton, game.AutomationLevel >= 5
                ? "PIPELINE MÁXIMO"
                : $"AUTOMATIZAR  R$ {game.NextAutomationCost}");
        }

        private void BuildInterface()
        {
            Image panel = CreateImage(transform, "Gestão idle", PanelColor);
            managementPanel = panel;
            Vector2 bottomRight = new(1f, 0f);
            SetRect(panel.rectTransform, bottomRight, bottomRight, new Vector2(-18f, 110f), new Vector2(260f, 278f), bottomRight);
            Outline outline = panel.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(Accent.r, Accent.g, Accent.b, 0.65f);
            outline.effectDistance = new Vector2(1f, -1f);

            Image accent = CreateImage(panel.transform, "Linha idle", Accent);
            SetRect(accent.rectTransform, new Vector2(0f, 1f), Vector2.one, Vector2.zero, new Vector2(0f, 3f), new Vector2(0.5f, 1f));
            Text header = CreateText(panel.transform, "BYTEFORGE IDLE", 11, TextAnchor.MiddleLeft, Accent, FontStyle.Bold);
            SetRect(header.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(14f, -20f), new Vector2(-28f, 18f));

            productionLabel = CreateText(panel.transform, "+1 ENTREGA / 15s\nPRÓXIMO CICLO 15s", 16, TextAnchor.MiddleLeft, Color.white, FontStyle.Bold);
            productionLabel.lineSpacing = 1.15f;
            SetRect(productionLabel.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(14f, -62f), new Vector2(-28f, 52f));
            teamLabel = CreateText(panel.transform, "EQUIPE 0/1  •  PIPELINE NV.0", 11, TextAnchor.MiddleLeft, new Color(0.72f, 0.82f, 0.92f));
            SetRect(teamLabel.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(14f, -101f), new Vector2(-28f, 18f));

            Image track = CreateImage(panel.transform, "Ciclo automático", new Color(0.01f, 0.02f, 0.035f, 0.96f));
            SetRect(track.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(14f, -120f), new Vector2(-28f, 8f));
            cycleFill = CreateImage(track.transform, "Progresso do ciclo", Accent);
            SetRect(cycleFill.rectTransform, Vector2.zero, Vector2.one, new Vector2(2f, 2f), new Vector2(-4f, -4f));
            cycleFill.type = Image.Type.Filled;
            cycleFill.fillMethod = Image.FillMethod.Horizontal;

            hireButton = CreateButton(panel.transform, "CONTRATAR", new Vector2(14f, -140f), new Vector2(-28f, 40f), Accent);
            hireButton.onClick.AddListener(Hire);
            automationButton = CreateButton(panel.transform, "AUTOMATIZAR", new Vector2(14f, -188f), new Vector2(-28f, 40f), Amber);
            automationButton.onClick.AddListener(UpgradeAutomation);
            Button settings = CreateButton(panel.transform, "ÁUDIO & ACESSIBILIDADE", new Vector2(14f, -236f), new Vector2(-28f, 28f), new Color(0.22f, 0.34f, 0.50f));
            settings.onClick.AddListener(() =>
            {
                settingsOverlay.SetActive(true);
                settingsOverlay.transform.SetAsLastSibling();
                GameAudioFeedback.Play(FeedbackSound.Button);
            });

            BuildSettingsOverlay();
        }

        private void BuildSettingsOverlay()
        {
            settingsOverlay = CreateImage(transform, "Configurações", new Color(0.005f, 0.01f, 0.02f, 0.90f)).gameObject;
            SetRect((RectTransform)settingsOverlay.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            settingsOverlay.GetComponent<Image>().raycastTarget = true;
            Image panel = CreateImage(settingsOverlay.transform, "Painel de configurações", new Color(0.04f, 0.07f, 0.11f, 0.99f));
            SetRect(panel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(520f, 390f));
            panel.gameObject.AddComponent<Outline>().effectColor = Accent;
            Text title = CreateText(panel.transform, "ÁUDIO & ACESSIBILIDADE", 25, TextAnchor.MiddleCenter, Color.white, FontStyle.Bold);
            SetRect(title.rectTransform, new Vector2(0f, 1f), Vector2.one, new Vector2(28f, -54f), new Vector2(-56f, 42f));

            musicButton = CreateToggle(panel.transform, -112f, () => GamePreferences.MusicEnabled = !GamePreferences.MusicEnabled);
            effectsButton = CreateToggle(panel.transform, -166f, () => GamePreferences.EffectsEnabled = !GamePreferences.EffectsEnabled);
            motionButton = CreateToggle(panel.transform, -220f, () => GamePreferences.ReduceMotion = !GamePreferences.ReduceMotion);
            textButton = CreateToggle(panel.transform, -274f, () => GamePreferences.LargeText = !GamePreferences.LargeText);
            Button close = CreateButton(panel.transform, "VOLTAR AO JOGO", new Vector2(34f, 30f), new Vector2(-68f, 44f), Accent, false);
            close.onClick.AddListener(() =>
            {
                settingsOverlay.SetActive(false);
                GameAudioFeedback.Play(FeedbackSound.Button);
            });
            settingsOverlay.SetActive(false);
            RefreshPreferences();
        }

        private Button CreateToggle(Transform parent, float y, UnityEngine.Events.UnityAction action)
        {
            Button button = CreateButton(parent, string.Empty, new Vector2(34f, y), new Vector2(-68f, 42f), new Color(0.18f, 0.29f, 0.43f));
            button.onClick.AddListener(action);
            button.onClick.AddListener(() => GameAudioFeedback.Play(FeedbackSound.Button));
            return button;
        }

        private void RefreshPreferences()
        {
            if (musicButton == null)
            {
                return;
            }
            SetButtonLabel(musicButton, $"MÚSICA: {(GamePreferences.MusicEnabled ? "LIGADA" : "DESLIGADA")}");
            SetButtonLabel(effectsButton, $"EFEITOS: {(GamePreferences.EffectsEnabled ? "LIGADOS" : "DESLIGADOS")}");
            SetButtonLabel(motionButton, $"MOVIMENTO REDUZIDO: {(GamePreferences.ReduceMotion ? "SIM" : "NÃO")}");
            SetButtonLabel(textButton, $"TEXTO AMPLIADO: {(GamePreferences.LargeText ? "SIM" : "NÃO")}");
            CanvasScaler scaler = GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.referenceResolution = GamePreferences.LargeText
                    ? new Vector2(1152f, 648f)
                    : new Vector2(1280f, 720f);
            }
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
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }

        private static Button CreateButton(Transform parent, string label, Vector2 position, Vector2 size, Color color, bool topAnchored = true)
        {
            Image image = CreateImage(parent, label, color);
            Vector2 anchor = topAnchored ? new Vector2(0f, 1f) : Vector2.zero;
            Vector2 anchorMax = topAnchored ? Vector2.one : new Vector2(1f, 0f);
            SetRect(image.rectTransform, anchor, anchorMax, position, size, anchor);
            image.raycastTarget = true;
            Button button = image.gameObject.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.highlightedColor = Color.Lerp(color, Color.white, 0.18f);
            colors.pressedColor = Color.Lerp(color, Color.black, 0.18f);
            colors.disabledColor = new Color(0.13f, 0.16f, 0.21f, 0.82f);
            button.colors = colors;
            Text text = CreateText(image.transform, label, 12, TextAnchor.MiddleCenter, Color.white, FontStyle.Bold);
            SetRect(text.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            return button;
        }

        private static void SetButtonLabel(Button button, string value)
        {
            Text label = button.GetComponentInChildren<Text>();
            if (label != null)
            {
                label.text = value;
            }
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
