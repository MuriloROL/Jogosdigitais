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
        private Button ambientButton;
        private Button motionButton;
        private Button textButton;
        private Button contrastButton;
        private Button fullscreenButton;
        private Button qualityButton;
        private Button resolutionButton;
        private int observedXp = -1;
        private float productionFlashUntil;

        public Button HireButton => hireButton;
        public Button AutomationButton => automationButton;
        public bool SettingsVisible => settingsOverlay != null && settingsOverlay.activeSelf;

        public void ShowSettings()
        {
            if (settingsOverlay == null)
            {
                return;
            }
            settingsOverlay.SetActive(true);
            settingsOverlay.transform.SetAsLastSibling();
            GameAudioFeedback.Play(FeedbackSound.Button);
        }

        private void Start()
        {
            BuildInterface();
            BindGame();
            GamePreferences.Changed += RefreshPreferences;
            Refresh();
        }

        private void Update()
        {
            if (game == null)
            {
                BindGame();
            }

            Refresh();
            if (managementPanel != null)
            {
                float flash = GamePreferences.ReduceMotion
                    ? 0f
                    : Mathf.Clamp01((productionFlashUntil - Time.unscaledTime) / 0.3f);
                managementPanel.color = Color.Lerp(Color.white, new Color(0.82f, 1f, 0.94f), flash * 0.18f);
            }
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
            string missionHint = null;
            for (int index = 0; index < game.MissionObjectiveCount; index++)
            {
                MissionObjective objective = game.GetMissionObjective(index);
                if (game.IsMissionObjectiveComplete(index))
                {
                    continue;
                }
                if (!CareerMissionCatalog.TryGetAction(objective.Kind, out _))
                {
                    missionHint = $"MISSÃO • {objective.Label.ToUpperInvariant()}  {game.GetMissionObjectiveProgress(index)}/{objective.Required}";
                }
                break;
            }
            teamLabel.text = missionHint ?? (game.HiringUnlocked
                ? $"EQUIPE {game.TeamMembers}/{game.TeamCapacity}  •  {game.ToolingName.ToUpperInvariant()}"
                : $"TRABALHO SOLO  •  {game.ToolingName.ToUpperInvariant()}");
            cycleFill.fillAmount = game.IsLastRank ? 1f : 1f - game.SecondsUntilPassiveTick / 15f;
            if (observedXp >= 0 && game.CareerXp > observedXp)
            {
                productionFlashUntil = Time.unscaledTime + 0.3f;
            }
            observedXp = game.CareerXp;

            hireButton.interactable = game.HiringUnlocked && game.TeamMembers < game.TeamCapacity;
            SetButtonLabel(hireButton, !game.HiringUnlocked
                ? "EQUIPE • LIBERA NO PLENO"
                : game.TeamMembers >= game.TeamCapacity
                    ? "EQUIPE COMPLETA"
                    : $"CONTRATAR COLEGA  R$ {game.NextHireCost}");
            automationButton.interactable = game.ToolingUpgradeUnlocked;
            SetButtonLabel(automationButton, game.AutomationLevel >= 5
                ? "STACK NO MÁXIMO"
                : !game.ToolingUpgradeUnlocked
                    ? $"{game.NextToolingLabel} • {CareerGame.NameFor(game.NextToolingUnlockRank).ToUpperInvariant()}"
                    : $"{game.NextToolingLabel}  R$ {game.NextAutomationCost}");
        }

        private void BuildInterface()
        {
            Vector2 bottomRight = new(1f, 0f);
            Image shadow = CreateImage(transform, "Sombra • Gestão idle", new Color(0f, 0f, 0f, 0.42f));
            SetRect(shadow.rectTransform, bottomRight, bottomRight, new Vector2(-14f, 105f), new Vector2(292f, 282f), bottomRight);
            shadow.gameObject.AddComponent<UiRoundedGraphic>().Configure(18);

            Image panel = CreateImage(transform, "Gestão idle", PanelColor);
            managementPanel = panel;
            SetRect(panel.rectTransform, bottomRight, bottomRight, new Vector2(-18f, 110f), new Vector2(292f, 282f), bottomRight);
            panel.color = Color.white;
            panel.gameObject.AddComponent<UiRoundedGraphic>().Configure(16);
            panel.gameObject.AddComponent<UiVerticalGradient>().Configure(
                new Color(0.075f, 0.125f, 0.17f, 0.99f),
                PanelColor);
            Outline outline = panel.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(Accent.r, Accent.g, Accent.b, 0.32f);
            outline.effectDistance = new Vector2(1f, -1f);

            Image accent = CreateImage(panel.transform, "Linha idle", Accent);
            SetRect(accent.rectTransform, new Vector2(0f, 1f), Vector2.one, new Vector2(14f, -2f), new Vector2(-28f, 3f), new Vector2(0.5f, 1f));
            accent.gameObject.AddComponent<UiRoundedGraphic>().Configure(4);
            Image liveDot = CreateImage(panel.transform, "Produção ativa", Accent);
            SetRect(liveDot.rectTransform, new Vector2(1f, 1f), Vector2.one, new Vector2(-16f, -15f), new Vector2(8f, 8f), Vector2.one);
            liveDot.gameObject.AddComponent<UiRoundedGraphic>().Configure(4);
            Text header = CreateText(panel.transform, "STACK & EQUIPE", 11, TextAnchor.MiddleLeft, Accent, FontStyle.Bold);
            SetRect(header.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(14f, -22f), new Vector2(-42f, 18f));

            productionLabel = CreateText(panel.transform, "+1 ENTREGA / 15s\nPRÓXIMO CICLO 15s", 16, TextAnchor.MiddleLeft, Color.white, FontStyle.Bold);
            productionLabel.lineSpacing = 1.15f;
            SetRect(productionLabel.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(14f, -63f), new Vector2(-28f, 52f));
            teamLabel = CreateText(panel.transform, "EQUIPE 0/1  •  PIPELINE NV.0", 11, TextAnchor.MiddleLeft, new Color(0.72f, 0.82f, 0.92f));
            SetRect(teamLabel.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(14f, -101f), new Vector2(-28f, 18f));

            Image track = CreateImage(panel.transform, "Ciclo automático", new Color(0.01f, 0.02f, 0.035f, 0.96f));
            SetRect(track.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(14f, -120f), new Vector2(-28f, 8f));
            track.gameObject.AddComponent<UiRoundedGraphic>().Configure(4);
            cycleFill = CreateImage(track.transform, "Progresso do ciclo", Accent);
            SetRect(cycleFill.rectTransform, Vector2.zero, Vector2.one, new Vector2(2f, 2f), new Vector2(-4f, -4f));
            cycleFill.type = Image.Type.Filled;
            cycleFill.fillMethod = Image.FillMethod.Horizontal;

            hireButton = CreateButton(panel.transform, "CONTRATAR", new Vector2(14f, -140f), new Vector2(-28f, 40f), Accent);
            hireButton.onClick.AddListener(Hire);
            automationButton = CreateButton(panel.transform, "AUTOMATIZAR", new Vector2(14f, -188f), new Vector2(-28f, 40f), Amber);
            automationButton.onClick.AddListener(UpgradeAutomation);
            Button settings = CreateButton(panel.transform, "ÁUDIO & ACESSIBILIDADE", new Vector2(14f, -236f), new Vector2(-28f, 32f), new Color(0.22f, 0.34f, 0.50f));
            settings.onClick.AddListener(() =>
            {
                ShowSettings();
            });

            BuildSettingsOverlay();
        }

        private void BuildSettingsOverlay()
        {
            settingsOverlay = CreateImage(transform, "Configurações", new Color(0.005f, 0.01f, 0.02f, 0.90f)).gameObject;
            SetRect((RectTransform)settingsOverlay.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            settingsOverlay.GetComponent<Image>().raycastTarget = true;
            Image panel = CreateImage(settingsOverlay.transform, "Painel de configurações", new Color(0.04f, 0.07f, 0.11f, 0.99f));
            SetRect(panel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(880f, 610f));
            panel.color = Color.white;
            panel.gameObject.AddComponent<UiRoundedGraphic>().Configure(20);
            panel.gameObject.AddComponent<UiVerticalGradient>().Configure(
                new Color(0.08f, 0.13f, 0.19f, 0.995f),
                new Color(0.03f, 0.055f, 0.095f, 0.995f));
            Outline outline = panel.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(Accent.r, Accent.g, Accent.b, 0.48f);
            outline.effectDistance = new Vector2(2f, -2f);
            Text title = CreateText(panel.transform, "ÁUDIO & ACESSIBILIDADE", 25, TextAnchor.MiddleCenter, Color.white, FontStyle.Bold);
            SetRect(title.rectTransform, new Vector2(0f, 1f), Vector2.one, new Vector2(28f, -54f), new Vector2(-56f, 42f));

            musicButton = CreateSettingsToggle(panel.transform, -102f, false, () => GamePreferences.MusicVolume = NextVolume(GamePreferences.MusicVolume));
            effectsButton = CreateSettingsToggle(panel.transform, -158f, false, () => GamePreferences.EffectsVolume = NextVolume(GamePreferences.EffectsVolume));
            ambientButton = CreateSettingsToggle(panel.transform, -214f, false, () => GamePreferences.AmbientVolume = NextVolume(GamePreferences.AmbientVolume));
            motionButton = CreateSettingsToggle(panel.transform, -270f, false, () => GamePreferences.ReduceMotion = !GamePreferences.ReduceMotion);
            textButton = CreateSettingsToggle(panel.transform, -326f, false, () => GamePreferences.LargeText = !GamePreferences.LargeText);

            contrastButton = CreateSettingsToggle(panel.transform, -102f, true, () => GamePreferences.HighContrast = !GamePreferences.HighContrast);
            fullscreenButton = CreateSettingsToggle(panel.transform, -158f, true, () => GamePreferences.Fullscreen = !GamePreferences.Fullscreen);
            qualityButton = CreateSettingsToggle(panel.transform, -214f, true, CycleQuality);
            resolutionButton = CreateSettingsToggle(panel.transform, -270f, true, CycleResolution);
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

        private Button CreateSettingsToggle(Transform parent, float y, bool rightColumn, UnityEngine.Events.UnityAction action)
        {
            Button button = CreateToggle(parent, y, action);
            Vector2 anchor = rightColumn ? new Vector2(1f, 1f) : new Vector2(0f, 1f);
            Vector2 pivot = rightColumn ? Vector2.one : new Vector2(0f, 1f);
            SetRect(button.GetComponent<RectTransform>(), anchor, anchor,
                new Vector2(rightColumn ? -34f : 34f, y), new Vector2(390f, 44f), pivot);
            return button;
        }

        private static float NextVolume(float current)
        {
            return current >= 0.99f ? 0f : Mathf.Clamp01(Mathf.Round((current + 0.25f) * 4f) / 4f);
        }

        private static void CycleQuality()
        {
            int count = Mathf.Max(1, QualitySettings.names.Length);
            GamePreferences.QualityLevel = (GamePreferences.QualityLevel + 1) % count;
        }

        private static void CycleResolution()
        {
            Resolution[] resolutions = Screen.resolutions;
            if (resolutions == null || resolutions.Length == 0)
            {
                return;
            }

            int current = 0;
            for (int index = 0; index < resolutions.Length; index++)
            {
                if (resolutions[index].width == Screen.width && resolutions[index].height == Screen.height)
                {
                    current = index;
                }
            }
            Resolution next = resolutions[(current + 1) % resolutions.Length];
            Screen.SetResolution(next.width, next.height, GamePreferences.Fullscreen);
        }

        private void RefreshPreferences()
        {
            if (musicButton == null)
            {
                return;
            }
            SetButtonLabel(musicButton, $"MÚSICA  {Mathf.RoundToInt(GamePreferences.MusicVolume * 100f)}%");
            SetButtonLabel(effectsButton, $"EFEITOS  {Mathf.RoundToInt(GamePreferences.EffectsVolume * 100f)}%");
            SetButtonLabel(ambientButton, $"AMBIENTE  {Mathf.RoundToInt(GamePreferences.AmbientVolume * 100f)}%");
            SetButtonLabel(motionButton, $"MOVIMENTO REDUZIDO: {(GamePreferences.ReduceMotion ? "SIM" : "NÃO")}");
            SetButtonLabel(textButton, $"TEXTO AMPLIADO: {(GamePreferences.LargeText ? "SIM" : "NÃO")}");
            SetButtonLabel(contrastButton, $"ALTO CONTRASTE: {(GamePreferences.HighContrast ? "SIM" : "NÃO")}");
            SetButtonLabel(fullscreenButton, $"TELA CHEIA: {(GamePreferences.Fullscreen ? "SIM" : "NÃO")}");
            string quality = QualitySettings.names.Length == 0 ? "PADRÃO" : QualitySettings.names[GamePreferences.QualityLevel].ToUpperInvariant();
            SetButtonLabel(qualityButton, $"QUALIDADE: {quality}");
            SetButtonLabel(resolutionButton, $"RESOLUÇÃO: {Screen.width} × {Screen.height}");
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
            image.color = Color.white;
            image.gameObject.AddComponent<UiRoundedGraphic>().Configure(11);
            image.gameObject.AddComponent<UiVerticalGradient>().Configure(
                Color.Lerp(color, Color.white, 0.08f),
                Color.Lerp(color, Color.black, 0.22f));
            Outline outline = image.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(color.r, color.g, color.b, 0.30f);
            outline.effectDistance = new Vector2(1f, -1f);
            Button button = image.gameObject.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.highlightedColor = Color.Lerp(color, Color.white, 0.18f);
            colors.pressedColor = Color.Lerp(color, Color.black, 0.18f);
            colors.disabledColor = new Color(0.13f, 0.16f, 0.21f, 0.82f);
            button.colors = colors;
            image.gameObject.AddComponent<UiButtonMotion>();
            Text text = CreateText(image.transform, label, 12, TextAnchor.MiddleCenter, Color.white, FontStyle.Bold);
            SetRect(text.rectTransform, Vector2.zero, Vector2.one, new Vector2(8f, 1f), new Vector2(-16f, -2f));
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
