using ProjetoJogosDigitais.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace ProjetoJogosDigitais.Presentation
{
    public sealed class CareerHud : MonoBehaviour
    {
        [SerializeField] private CareerGame game;
        [SerializeField] private OfficePlayerController player;
        [SerializeField] private Text rankLabel;
        [SerializeField] private Text promotionLabel;
        [SerializeField] private Image promotionFill;
        [SerializeField] private Text clockLabel;
        [SerializeField] private Text salaryLabel;
        [SerializeField] private Image workdayFill;
        [SerializeField] private Text deliveryLabel;
        [SerializeField] private Image deliveryFill;
        [SerializeField] private Text energyLabel;
        [SerializeField] private Image energyFill;
        [SerializeField] private Text stressLabel;
        [SerializeField] private Image stressFill;
        [SerializeField] private Text knowledgeLabel;
        [SerializeField] private Text reputationLabel;
        [SerializeField] private Text statusLabel;
        [SerializeField] private Text objectiveLabel;
        [SerializeField] private Button endDayButton;
        [SerializeField] private Button saveButton;
        [SerializeField] private Button loadButton;
        private OfficeStation[] stationMarkers;

        public void Configure(
            CareerGame career,
            OfficePlayerController officePlayer,
            Text rank,
            Text promotion,
            Image promotionBar,
            Text clock,
            Text salary,
            Image workdayBar,
            Text delivery,
            Image deliveryBar,
            Text energy,
            Image energyBar,
            Text stress,
            Image stressBar,
            Text knowledge,
            Text reputation,
            Text status,
            Text objective,
            Button endDay,
            Button save,
            Button load)
        {
            game = career;
            player = officePlayer;
            rankLabel = rank;
            promotionLabel = promotion;
            promotionFill = promotionBar;
            clockLabel = clock;
            salaryLabel = salary;
            workdayFill = workdayBar;
            deliveryLabel = delivery;
            deliveryFill = deliveryBar;
            energyLabel = energy;
            energyFill = energyBar;
            stressLabel = stress;
            stressFill = stressBar;
            knowledgeLabel = knowledge;
            reputationLabel = reputation;
            statusLabel = status;
            objectiveLabel = objective;
            endDayButton = endDay;
            saveButton = save;
            loadButton = load;
        }

        private void Awake()
        {
            endDayButton?.onClick.AddListener(EndDay);
            saveButton?.onClick.AddListener(SaveGame);
            loadButton?.onClick.AddListener(LoadGame);
            if (GetComponent<CareerTutorialController>() == null)
            {
                gameObject.AddComponent<CareerTutorialController>();
            }
            if (GetComponent<IdleManagementPanel>() == null)
            {
                gameObject.AddComponent<IdleManagementPanel>();
            }
            if (GetComponent<CareerDecisionController>() == null)
            {
                gameObject.AddComponent<CareerDecisionController>();
            }
            if (GetComponent<MissionBriefingController>() == null)
            {
                gameObject.AddComponent<MissionBriefingController>();
            }
            if (GetComponent<OfficeAmbience>() == null)
            {
                gameObject.AddComponent<OfficeAmbience>();
            }
            if (GetComponent<PauseMenuController>() == null)
            {
                gameObject.AddComponent<PauseMenuController>();
            }
            if (GetComponent<AccessibilityVisualController>() == null)
            {
                gameObject.AddComponent<AccessibilityVisualController>();
            }
        }

        private void OnDestroy()
        {
            endDayButton?.onClick.RemoveListener(EndDay);
            saveButton?.onClick.RemoveListener(SaveGame);
            loadButton?.onClick.RemoveListener(LoadGame);
        }

        private void Update()
        {
            if (game == null)
            {
                game = CareerGame.Current;
            }
            if (game == null)
            {
                return;
            }

            rankLabel.text = game.RankName.ToUpperInvariant();
            promotionLabel.text = game.IsLastRank
                ? $"XP {game.CareerXp:N0} • CARREIRA CONCLUÍDA"
                : $"XP {game.CareerXp:N0}/{game.XpRequiredForNextRank:N0} PARA PROMOÇÃO";
            promotionFill.fillAmount = game.RankXpProgress;
            clockLabel.text = game.DisplayTime.ToUpperInvariant().Replace("  ", "  //  ");
            salaryLabel.text = $"SALÁRIO R$ {game.MonthlySalary:N0}   •   CAIXA R$ {game.Money:N0}";
            workdayFill.fillAmount = Mathf.Clamp01(game.MinutesUsed / (float)CareerGame.WorkdayMinutes);
            int checklistGoal = Mathf.Max(1, game.MissionObjectiveGoal);
            deliveryLabel.text = game.IsLastRank
                ? $"ENTREGA TOTAL  {game.Delivery:N0}"
                : $"CHECKLIST  {game.MissionObjectiveProgress}/{checklistGoal}  •  ENTREGA {game.Delivery:N0}";
            deliveryFill.fillAmount = game.IsLastRank
                ? 1f
                : Mathf.Clamp01(game.MissionObjectiveProgress / (float)checklistGoal);
            deliveryFill.color = game.MissionReadyToClaim
                ? new Color(0.32f, 0.88f, 0.52f)
                : new Color(0.26f, 0.72f, 0.92f);

            energyLabel.text = $"ENERGIA  {game.Energy}%";
            energyFill.fillAmount = game.Energy / 100f;
            energyFill.color = Color.Lerp(new Color(0.92f, 0.28f, 0.24f), new Color(0.35f, 0.86f, 0.48f), game.Energy / 100f);
            stressLabel.text = $"ESTRESSE  {game.Stress}%";
            stressFill.fillAmount = game.Stress / 100f;
            stressFill.color = Color.Lerp(new Color(0.40f, 0.75f, 0.95f), new Color(0.95f, 0.30f, 0.25f), game.Stress / 100f);
            knowledgeLabel.text = $"CONHECIMENTO\n{game.Knowledge}";
            reputationLabel.text = $"REPUTAÇÃO\n{game.Reputation}";
            statusLabel.text = player == null || player.IsIdle ? game.LastFeedback : player.Status;
            objectiveLabel.text = game.IsLastRank
                ? "JORNADA CONCLUÍDA\nA ByteForge agora faz parte do seu legado."
                : $"MISSÃO {game.MissionNumber}/{game.MissionsPerRank}  •  {game.MissionTitle}\n" +
                  $"{game.MissionChecklistText()}\n" +
                  $"RECOMPENSA  R$ {game.MissionRewardMoney}  •  +{game.MissionRewardXp} XP";
            bool controlsAvailable = player == null || !player.IsInteracting;
            endDayButton.interactable = controlsAvailable && game.MissionReadyToClaim;
            Text endDayLabel = endDayButton.GetComponentInChildren<Text>();
            if (endDayLabel != null)
            {
                endDayLabel.text = game.IsLastRank ? "CARREIRA CONCLUÍDA" : "CONCLUIR MISSÃO";
            }
            saveButton.interactable = controlsAvailable;
            loadButton.interactable = controlsAvailable;
            RefreshMissionHighlight();
        }

        private void EndDay()
        {
            game?.ClaimMission();
            GameAudioFeedback.Play(FeedbackSound.Mission);
            player?.ShowStatus(game.LastFeedback);
        }

        private void RefreshMissionHighlight()
        {
            if (stationMarkers == null || stationMarkers.Length == 0)
            {
                stationMarkers = FindObjectsByType<OfficeStation>(FindObjectsSortMode.None);
            }

            bool hasAction = game.TryGetNextMissionAction(out OfficeActionType nextAction);
            foreach (OfficeStation station in stationMarkers)
            {
                if (station != null)
                {
                    station.SetMissionFocus(hasAction && station.ActionType == nextAction);
                }
            }
        }

        private void SaveGame()
        {
            game?.SaveGame();
            GameAudioFeedback.Play(FeedbackSound.Button);
            player?.ShowStatus(game.LastFeedback);
        }

        private void LoadGame()
        {
            game?.LoadGame();
            GameAudioFeedback.Play(FeedbackSound.Button);
            player?.ShowStatus(game.LastFeedback);
        }
    }
}
