using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjetoJogosDigitais.Gameplay
{
    public sealed class CareerGame : MonoBehaviour
    {
        [Serializable]
        private sealed class SaveData
        {
            public int saveVersion;
            public CareerRank rank;
            public int day;
            public int minutesUsed;
            public int money;
            public int delivery;
            public int energy;
            public int stress;
            public int knowledge;
            public int reputation;
            public int careerXp;
            public int dailyTaskProgress;
            public bool dailyTaskCompleted;
            public string lastCareerEvent;
            public int successfulDaysAtRank;
            public int totalSuccessfulDays;
            public string lastFeedback;
            public float elapsedWorkSeconds;
            public float idleTickAccumulator;
            public int teamMembers;
            public int automationLevel;
            public bool sprintBonusClaimed;
            public int pendingDecisionRank;
            public int missionIndexAtRank;
            public int[] missionObjectiveProgress;
            public int totalMissionsCompleted;
            public bool missionBriefingPending;
        }

        private static readonly string[] RankNames =
        {
            "Estagiário",
            "Dev Júnior",
            "Dev Pleno",
            "Dev Sênior",
            "Tech Lead",
            "CTO",
            "Dono"
        };

        private static readonly int[] MonthlySalaries = { 1800, 3500, 6500, 10000, 14000, 22000, 50000 };
        private static readonly int[] GoodDaysForPromotion = { 3, 3, 3, 3, 3, 3, 0 };
        private static readonly int[] TotalXpForNextRank = { 350, 1100, 2200, 3800, 6000, 9000, 9000 };
        private static readonly int[] ToolUpgradeCosts = { 120, 250, 550, 1000, 1700 };
        private static readonly int[] ToolPassiveOutput = { 1, 2, 3, 3, 4, 5 };
        private static readonly CareerRank[] ToolUnlockRanks =
        {
            CareerRank.Intern,
            CareerRank.JuniorDeveloper,
            CareerRank.MidLevelDeveloper,
            CareerRank.SeniorDeveloper,
            CareerRank.TechLead
        };
        private static readonly string[] ToolStageNames =
        {
            "Fluxo manual",
            "Codex Assistente",
            "Codex Multiagentes",
            "Testes inteligentes",
            "Pipeline orquestrado",
            "Plataforma autônoma"
        };
        private static readonly string[] ToolPurchaseLabels =
        {
            "COMPRAR CODEX",
            "ATIVAR MULTIAGENTES",
            "AUTOMATIZAR TESTES",
            "ORQUESTRAR PIPELINE",
            "ESCALAR PLATAFORMA"
        };
        private const int CurrentSaveVersion = 7;
        private const float IdleTickSeconds = 15f;
        private const int MaxAutomationLevel = 5;
        public const int StartingMoney = 120;
        public const int WorkdayMinutes = 8 * 60;
        public const int StartHour = 9;
        public const float WorkdayRealSeconds = 4f * 60f * 60f;

        [SerializeField] private CareerRank rank = CareerRank.Intern;
        [SerializeField, Min(1)] private int day = 1;
        [SerializeField] private int minutesUsed;
        [SerializeField] private int money = StartingMoney;
        [SerializeField] private int delivery;
        [SerializeField, Range(0, 100)] private int energy = 100;
        [SerializeField, Range(0, 100)] private int stress = 8;
        [SerializeField] private int knowledge;
        [SerializeField] private int reputation;
        [SerializeField] private int careerXp;
        [SerializeField] private int dailyTaskProgress;
        [SerializeField] private bool dailyTaskCompleted;
        [SerializeField] private string lastCareerEvent = "Nenhum evento de carreira hoje.";
        [SerializeField] private int successfulDaysAtRank;
        [SerializeField] private int totalSuccessfulDays;
        [SerializeField] private string lastFeedback = "Primeiro dia: entregue valor e construa sua carreira.";
        [SerializeField] private float elapsedWorkSeconds;
        [SerializeField] private float idleTickAccumulator;
        [SerializeField] private int teamMembers;
        [SerializeField] private int automationLevel;
        [SerializeField] private bool sprintBonusClaimed;
        [SerializeField] private int pendingDecisionRank = -1;
        [SerializeField] private int missionIndexAtRank;
        [SerializeField] private int[] missionObjectiveProgress = new int[3];
        [SerializeField] private int totalMissionsCompleted;
        [SerializeField] private bool missionBriefingPending = true;

        private string savePathOverride;
        private Coroutine pendingSceneChange;

        public static CareerGame Current { get; private set; }
        public CareerRank Rank => rank;
        public string RankName => NameFor(rank);
        public int Day => day;
        public int MinutesUsed => minutesUsed;
        public int MinutesRemaining => Mathf.Max(0, WorkdayMinutes - minutesUsed);
        public int Money => money;
        public int Delivery => delivery;
        public int DailyGoal => MissionObjectiveGoal;
        public int Energy => energy;
        public int Stress => stress;
        public int Knowledge => knowledge;
        public int Reputation => reputation;
        public int CareerXp => careerXp;
        public int XpRequiredForNextRank => TotalXpForNextRank[(int)rank];
        public int XpAtCurrentRankStart => rank == CareerRank.Intern ? 0 : TotalXpForNextRank[(int)rank - 1];
        public float RankXpProgress => IsLastRank
            ? 1f
            : Mathf.InverseLerp(XpAtCurrentRankStart, XpRequiredForNextRank, careerXp);
        public int SuccessfulDaysAtRank => successfulDaysAtRank;
        public int TotalSuccessfulDays => totalSuccessfulDays;
        public float ElapsedWorkSeconds => elapsedWorkSeconds;
        public int TeamMembers => teamMembers;
        public int AutomationLevel => automationLevel;
        public int TeamCapacity => TeamCapacityFor(rank);
        public int PassiveDeliveryPerTick => ToolPassiveOutput[automationLevel] + teamMembers;
        public float PassiveDeliveryPerMinute => PassiveDeliveryPerTick * (60f / IdleTickSeconds);
        public float SecondsUntilPassiveTick => Mathf.Max(0f, IdleTickSeconds - idleTickAccumulator);
        public int NextHireCost => 300 * (teamMembers + 1);
        public int NextAutomationCost => automationLevel >= MaxAutomationLevel
            ? 0
            : ToolUpgradeCosts[automationLevel];
        public string ToolingName => ToolStageNames[automationLevel];
        public string NextToolingLabel => automationLevel >= MaxAutomationLevel
            ? "STACK NO MÁXIMO"
            : ToolPurchaseLabels[automationLevel];
        public CareerRank NextToolingUnlockRank => automationLevel >= MaxAutomationLevel
            ? CareerRank.Owner
            : ToolUnlockRanks[automationLevel];
        public bool HiringUnlocked => rank >= CareerRank.MidLevelDeveloper && !IsLastRank;
        public bool ToolingUpgradeUnlocked => !IsLastRank &&
            automationLevel < MaxAutomationLevel &&
            rank >= NextToolingUnlockRank;
        public bool CanHire => HiringUnlocked && teamMembers < TeamCapacity && money >= NextHireCost;
        public bool CanUpgradeAutomation => ToolingUpgradeUnlocked && money >= NextAutomationCost;
        public bool SprintBonusClaimed => sprintBonusClaimed;
        public bool HasPendingCareerDecision => pendingDecisionRank >= 0 && pendingDecisionRank < (int)CareerRank.Owner;
        public CareerRank PendingDecisionRank => HasPendingCareerDecision ? (CareerRank)pendingDecisionRank : rank;
        public CareerMissionDefinition CurrentMission => CareerMissionCatalog.Get(rank, missionIndexAtRank);
        public int MissionIndexAtRank => missionIndexAtRank;
        public int MissionNumber => Mathf.Min(CareerMissionCatalog.MissionsPerRank, missionIndexAtRank + 1);
        public int MissionsPerRank => CareerMissionCatalog.MissionsPerRank;
        public int TotalMissionsCompleted => totalMissionsCompleted;
        public bool MissionBriefingPending => !IsLastRank && missionBriefingPending;
        public string MissionTitle => IsLastRank ? "CARREIRA CONCLUÍDA" : CurrentMission.Title;
        public string MissionBriefing => IsLastRank ? "A ByteForge agora faz parte do seu legado." : CurrentMission.Briefing;
        public int MissionRewardXp => IsLastRank ? 0 : CurrentMission.RewardXp;
        public int MissionRewardMoney => IsLastRank ? 0 : CurrentMission.RewardMoney;
        public int MissionObjectiveCount => IsLastRank ? 0 : CurrentMission.Objectives.Length;
        public int MissionObjectiveProgress => CalculateMissionObjectiveProgress();
        public int MissionObjectiveGoal => CalculateMissionObjectiveGoal();
        public bool MissionReadyToClaim => !IsLastRank && AreMissionObjectivesComplete();
        // Compatibilidade temporária com HUD/tests das versões anteriores.
        public string DailyTaskTitle => MissionTitle;
        public string DailyTaskDescription => MissionBriefing;
        public int DailyTaskProgress => MissionObjectiveProgress;
        public int DailyTaskGoal => MissionObjectiveGoal;
        public int DailyTaskRewardXp => MissionRewardXp;
        public bool DailyTaskCompleted => MissionReadyToClaim;
        public string LastCareerEvent => lastCareerEvent;
        public int GoodDaysRequired => GoodDaysForPromotion[(int)rank];
        public int MonthlySalary => MonthlySalaries[(int)rank];
        public int DailyPay => Mathf.RoundToInt(MonthlySalary / 22f);
        public bool GoalReached => MissionReadyToClaim;
        public bool IsLastRank => rank == CareerRank.Owner;
        public string LastFeedback => lastFeedback;
        public static string DefaultSavePath => Path.Combine(Application.persistentDataPath, "dev-career-mvp.json");
        public static bool DefaultSaveExists => File.Exists(DefaultSavePath);
        public string SavePath => string.IsNullOrEmpty(savePathOverride)
            ? DefaultSavePath
            : savePathOverride;
        public string DisplayTime
        {
            get
            {
                int absoluteMinutes = StartHour * 60 + minutesUsed;
                return $"Dia {day}  {absoluteMinutes / 60:00}:{absoluteMinutes % 60:00}";
            }
        }

        public event Action Changed;
        public event Action<CareerRank> Promoted;

        private void Awake()
        {
            if (Current != null && Current != this)
            {
                Destroy(gameObject);
                return;
            }

            Current = this;
            EnsureMissionProgress();
            if (Application.isPlaying)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (Current == this)
            {
                Current = null;
            }
        }

        private void Update()
        {
            if (Application.isPlaying && !IsLastRank)
            {
                AdvanceOpenTime(Time.unscaledDeltaTime);
            }
        }

        public static string NameFor(CareerRank value)
        {
            return RankNames[(int)value];
        }

        public static int TeamCapacityFor(CareerRank value)
        {
            return value switch
            {
                CareerRank.Intern => 0,
                CareerRank.JuniorDeveloper => 0,
                CareerRank.MidLevelDeveloper => 1,
                CareerRank.SeniorDeveloper => 2,
                CareerRank.TechLead => 4,
                CareerRank.Cto => 6,
                CareerRank.Owner => 6,
                _ => 0
            };
        }

        public static string ActionName(OfficeActionType type)
        {
            return type switch
            {
                OfficeActionType.Code => "Programar",
                OfficeActionType.Meeting => "Participar da reunião",
                OfficeActionType.Study => "Estudar tecnologia",
                OfficeActionType.Coffee => "Tomar café",
                OfficeActionType.Break => "Fazer uma pausa",
                _ => type.ToString()
            };
        }

        public static int ActionDuration(OfficeActionType type)
        {
            return type switch
            {
                OfficeActionType.Code => 60,
                OfficeActionType.Meeting => 45,
                OfficeActionType.Study => 60,
                OfficeActionType.Coffee => 30,
                OfficeActionType.Break => 30,
                _ => 60
            };
        }

        public MissionObjective GetMissionObjective(int index)
        {
            if (IsLastRank || index < 0 || index >= CurrentMission.Objectives.Length)
            {
                return default;
            }
            return CurrentMission.Objectives[index];
        }

        public int GetMissionObjectiveProgress(int index)
        {
            MissionObjective objective = GetMissionObjective(index);
            return objective.Kind switch
            {
                MissionObjectiveKind.ToolLevel => Mathf.Min(objective.Required, automationLevel),
                MissionObjectiveKind.TeamSize => Mathf.Min(objective.Required, teamMembers),
                _ => index >= 0 && index < missionObjectiveProgress.Length
                    ? Mathf.Min(objective.Required, missionObjectiveProgress[index])
                    : 0
            };
        }

        public bool IsMissionObjectiveComplete(int index)
        {
            MissionObjective objective = GetMissionObjective(index);
            return objective.Required > 0 && GetMissionObjectiveProgress(index) >= objective.Required;
        }

        public string MissionChecklistText()
        {
            if (IsLastRank)
            {
                return "✓ Jornada concluída";
            }

            var builder = new StringBuilder();
            for (int index = 0; index < MissionObjectiveCount; index++)
            {
                MissionObjective objective = GetMissionObjective(index);
                int progress = GetMissionObjectiveProgress(index);
                if (index > 0)
                {
                    builder.Append('\n');
                }
                builder.Append(progress >= objective.Required ? "✓ " : "□ ");
                builder.Append(objective.Label);
                builder.Append("  ");
                builder.Append(progress);
                builder.Append('/');
                builder.Append(objective.Required);
            }
            return builder.ToString();
        }

        public bool TryGetNextMissionAction(out OfficeActionType action)
        {
            for (int index = 0; index < MissionObjectiveCount; index++)
            {
                MissionObjective objective = GetMissionObjective(index);
                if (IsMissionObjectiveComplete(index))
                {
                    continue;
                }

                return CareerMissionCatalog.TryGetAction(objective.Kind, out action);
            }

            action = default;
            return false;
        }

        public bool TryGetNextMissionObjective(out MissionObjective objective)
        {
            for (int index = 0; index < MissionObjectiveCount; index++)
            {
                if (!IsMissionObjectiveComplete(index))
                {
                    objective = GetMissionObjective(index);
                    return true;
                }
            }

            objective = default;
            return false;
        }

        public void AcknowledgeMissionBriefing()
        {
            if (!missionBriefingPending)
            {
                return;
            }

            missionBriefingPending = false;
            lastFeedback = $"MISSÃO INICIADA: {MissionTitle}.";
            AutoSave();
            Changed?.Invoke();
        }

        public OfficeActionResult PerformAction(OfficeActionType type)
        {
            int deliveryBefore = delivery;
            switch (type)
            {
                case OfficeActionType.Code:
                    PerformCoding();
                    break;
                case OfficeActionType.Meeting:
                    delivery += 14;
                    energy -= 10;
                    stress += 9;
                    reputation += 7;
                    lastFeedback = "Você alinhou o time e ganhou reputação.";
                    break;
                case OfficeActionType.Study:
                    delivery += 5;
                    energy -= 14;
                    stress += 5;
                    knowledge += 10;
                    reputation += 1;
                    lastFeedback = "Novo conhecimento adquirido para os próximos desafios.";
                    break;
                case OfficeActionType.Coffee:
                    energy += 30;
                    stress -= 7;
                    lastFeedback = "Café tomado. Energia recuperada.";
                    break;
                case OfficeActionType.Break:
                    energy += 14;
                    stress -= 26;
                    lastFeedback = "Uma pausa curta colocou as ideias no lugar.";
                    break;
            }

            energy = Mathf.Clamp(energy, 0, 100);
            stress = Mathf.Clamp(stress, 0, 100);
            knowledge = Mathf.Max(0, knowledge);
            reputation = Mathf.Max(0, reputation);

            int gained = delivery - deliveryBefore;
            if (gained > 0)
            {
                careerXp += gained;
                lastFeedback += $"  +{gained} de entrega e +{gained} XP.";
            }
            TrackMissionAction(type);
            if (MissionReadyToClaim)
            {
                lastFeedback += "  CHECKLIST CONCLUÍDO: finalize a missão no painel.";
            }

            AutoSave();
            Changed?.Invoke();
            return new OfficeActionResult(true, lastFeedback, gained);
        }

        public void AdvanceOpenTime(float seconds)
        {
            if (seconds <= 0f || IsLastRank)
            {
                return;
            }

            int minuteBefore = minutesUsed;
            elapsedWorkSeconds += seconds;
            minutesUsed = Mathf.Clamp(
                Mathf.FloorToInt(elapsedWorkSeconds / WorkdayRealSeconds * WorkdayMinutes),
                0,
                WorkdayMinutes);
            idleTickAccumulator += seconds;

            bool produced = false;
            while (idleTickAccumulator >= IdleTickSeconds && !IsLastRank)
            {
                idleTickAccumulator -= IdleTickSeconds;
                ProducePassiveDelivery();
                produced = true;
            }

            if (produced || minutesUsed != minuteBefore)
            {
                Changed?.Invoke();
            }
        }

        public bool HireTeammate()
        {
            if (!HiringUnlocked)
            {
                lastFeedback = rank == CareerRank.Intern
                    ? "Estagiários ainda não contratam. Comece adquirindo o Codex."
                    : "Como Júnior, evolua o Codex com multiagentes. A equipe é liberada no cargo de Dev Pleno.";
                Changed?.Invoke();
                return false;
            }

            if (teamMembers >= TeamCapacity)
            {
                lastFeedback = "A equipe está completa para o seu cargo atual.";
                Changed?.Invoke();
                return false;
            }

            int cost = NextHireCost;
            if (money < cost)
            {
                lastFeedback = $"Faltam R$ {cost - money} para contratar o próximo colega.";
                Changed?.Invoke();
                return false;
            }

            money -= cost;
            teamMembers++;
            reputation += 2;
            lastFeedback = $"Novo colega contratado! A equipe agora gera {PassiveDeliveryPerTick} pontos a cada {IdleTickSeconds:0}s.";
            AutoSave();
            Changed?.Invoke();
            return true;
        }

        public bool UpgradeAutomation()
        {
            if (automationLevel >= MaxAutomationLevel)
            {
                lastFeedback = "A automação da ByteForge já está no nível máximo.";
                Changed?.Invoke();
                return false;
            }

            if (!ToolingUpgradeUnlocked)
            {
                lastFeedback = $"{NextToolingLabel} será liberado no cargo de {NameFor(NextToolingUnlockRank)}.";
                Changed?.Invoke();
                return false;
            }

            int cost = NextAutomationCost;
            if (money < cost)
            {
                lastFeedback = $"Faltam R$ {cost - money} para {NextToolingLabel.ToLowerInvariant()}.";
                Changed?.Invoke();
                return false;
            }

            money -= cost;
            automationLevel++;
            knowledge += 4;
            lastFeedback = $"{ToolingName} ativado! Produção passiva: {PassiveDeliveryPerTick} pontos por ciclo.";
            AutoSave();
            Changed?.Invoke();
            return true;
        }

        public bool ClaimSprintBonus()
        {
            return ClaimMission();
        }

        public bool ClaimMission()
        {
            if (!MissionReadyToClaim)
            {
                lastFeedback = "Conclua todos os itens do checklist antes de finalizar a missão.";
                Changed?.Invoke();
                return false;
            }

            CareerMissionDefinition completedMission = CurrentMission;
            money += completedMission.RewardMoney;
            careerXp += completedMission.RewardXp;
            reputation += 2;
            successfulDaysAtRank++;
            totalSuccessfulDays++;
            totalMissionsCompleted++;
            missionIndexAtRank++;
            ResetMissionProgress();
            missionBriefingPending = true;
            lastCareerEvent = completedMission.Completion;
            lastFeedback = $"MISSÃO CONCLUÍDA: {completedMission.Title}! +R$ {completedMission.RewardMoney} e +{completedMission.RewardXp} XP. {completedMission.Completion}";

            if (missionIndexAtRank >= CareerMissionCatalog.MissionsPerRank)
            {
                careerXp = Mathf.Max(careerXp, XpRequiredForNextRank);
            }
            PromoteWhileEligible();
            AutoSave();
            Changed?.Invoke();
            return true;
        }

        public void ResolveCareerDecision(
            string summary,
            int moneyDelta,
            int knowledgeDelta,
            int reputationDelta,
            int stressDelta,
            int xpDelta)
        {
            if (!HasPendingCareerDecision)
            {
                return;
            }

            money = Mathf.Max(0, money + moneyDelta);
            knowledge = Mathf.Max(0, knowledge + knowledgeDelta);
            reputation = Mathf.Max(0, reputation + reputationDelta);
            stress = Mathf.Clamp(stress + stressDelta, 0, 100);
            careerXp = Mathf.Max(0, careerXp + xpDelta);
            pendingDecisionRank = -1;
            lastCareerEvent = summary;
            lastFeedback = summary;
            PromoteWhileEligible();
            AutoSave();
            Changed?.Invoke();
        }

        public bool EndDay()
        {
            return ClaimMission();
        }

        public void SaveGame()
        {
            WriteSave(true);
        }

        public bool LoadGame()
        {
            if (!File.Exists(SavePath))
            {
                lastFeedback = "Ainda não existe uma carreira salva.";
                Changed?.Invoke();
                return false;
            }

            SaveData data = JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));
            rank = data.rank;
            // A versão idle acontece em um único expediente. Saves antigos mantêm
            // toda a progressão, mas são trazidos para o Dia 1 da nova jornada.
            day = 1;
            minutesUsed = Mathf.Clamp(data.minutesUsed, 0, WorkdayMinutes);
            money = Mathf.Max(0, data.money);
            delivery = Mathf.Max(0, data.delivery);
            energy = Mathf.Clamp(data.energy, 0, 100);
            stress = Mathf.Clamp(data.stress, 0, 100);
            knowledge = Mathf.Max(0, data.knowledge);
            reputation = Mathf.Max(0, data.reputation);
            careerXp = data.saveVersion >= 2
                ? Mathf.Max(0, data.careerXp)
                : EstimateXpFromLegacySave(data.rank, data.successfulDaysAtRank, data.delivery);
            dailyTaskProgress = 0;
            dailyTaskCompleted = false;
            lastCareerEvent = string.IsNullOrEmpty(data.lastCareerEvent)
                ? "Nenhum evento de carreira hoje."
                : data.lastCareerEvent;
            successfulDaysAtRank = Mathf.Max(0, data.successfulDaysAtRank);
            totalSuccessfulDays = Mathf.Max(0, data.totalSuccessfulDays);
            lastFeedback = string.IsNullOrEmpty(data.lastFeedback) ? "Carreira carregada." : data.lastFeedback;
            elapsedWorkSeconds = data.saveVersion >= 5
                ? Mathf.Max(0f, data.elapsedWorkSeconds)
                : minutesUsed * 60f;
            idleTickAccumulator = data.saveVersion >= 5
                ? Mathf.Clamp(data.idleTickAccumulator, 0f, IdleTickSeconds)
                : 0f;
            teamMembers = data.saveVersion >= 5
                ? Mathf.Clamp(data.teamMembers, 0, TeamCapacityFor(rank))
                : 0;
            automationLevel = data.saveVersion >= 5
                ? Mathf.Clamp(data.automationLevel, 0, MaxToolingLevelFor(rank))
                : 0;
            sprintBonusClaimed = data.saveVersion >= 5 && data.sprintBonusClaimed;
            pendingDecisionRank = data.saveVersion >= 5
                ? Mathf.Clamp(data.pendingDecisionRank, -1, (int)CareerRank.Cto)
                : -1;
            missionIndexAtRank = data.saveVersion >= 7
                ? Mathf.Clamp(data.missionIndexAtRank, 0, CareerMissionCatalog.MissionsPerRank - 1)
                : 0;
            totalMissionsCompleted = data.saveVersion >= 7
                ? Mathf.Max(0, data.totalMissionsCompleted)
                : Mathf.Max(0, (int)rank * CareerMissionCatalog.MissionsPerRank);
            missionObjectiveProgress = data.saveVersion >= 7 && data.missionObjectiveProgress != null
                ? data.missionObjectiveProgress
                : new int[3];
            missionBriefingPending = data.saveVersion >= 7 ? data.missionBriefingPending : true;
            EnsureMissionProgress();
            Changed?.Invoke();
            RequestSceneForCurrentRank();
            return true;
        }

        public void SetSavePathOverride(string path)
        {
            savePathOverride = path;
        }

        public void ReleaseSession()
        {
            if (Current == this)
            {
                Current = null;
            }
            Destroy(gameObject);
        }

        private int CalculateMissionObjectiveProgress()
        {
            int total = 0;
            for (int index = 0; index < MissionObjectiveCount; index++)
            {
                total += GetMissionObjectiveProgress(index);
            }
            return total;
        }

        private int CalculateMissionObjectiveGoal()
        {
            int total = 0;
            for (int index = 0; index < MissionObjectiveCount; index++)
            {
                total += GetMissionObjective(index).Required;
            }
            return total;
        }

        private bool AreMissionObjectivesComplete()
        {
            if (MissionObjectiveCount == 0)
            {
                return false;
            }

            for (int index = 0; index < MissionObjectiveCount; index++)
            {
                if (!IsMissionObjectiveComplete(index))
                {
                    return false;
                }
            }
            return true;
        }

        private void EnsureMissionProgress()
        {
            int requiredLength = Mathf.Max(3, MissionObjectiveCount);
            if (missionObjectiveProgress != null && missionObjectiveProgress.Length >= requiredLength)
            {
                return;
            }

            int[] previous = missionObjectiveProgress;
            missionObjectiveProgress = new int[requiredLength];
            if (previous != null)
            {
                Array.Copy(previous, missionObjectiveProgress, Mathf.Min(previous.Length, missionObjectiveProgress.Length));
            }
        }

        private void ResetMissionProgress()
        {
            missionObjectiveProgress = new int[3];
            dailyTaskProgress = 0;
            dailyTaskCompleted = false;
        }

        private SaveData CreateSaveData()
        {
            return new SaveData
            {
                saveVersion = CurrentSaveVersion,
                rank = rank,
                day = day,
                minutesUsed = minutesUsed,
                money = money,
                delivery = delivery,
                energy = energy,
                stress = stress,
                knowledge = knowledge,
                reputation = reputation,
                careerXp = careerXp,
                dailyTaskProgress = MissionObjectiveProgress,
                dailyTaskCompleted = MissionReadyToClaim,
                lastCareerEvent = lastCareerEvent,
                successfulDaysAtRank = successfulDaysAtRank,
                totalSuccessfulDays = totalSuccessfulDays,
                lastFeedback = lastFeedback,
                elapsedWorkSeconds = elapsedWorkSeconds,
                idleTickAccumulator = idleTickAccumulator,
                teamMembers = teamMembers,
                automationLevel = automationLevel,
                sprintBonusClaimed = false,
                pendingDecisionRank = pendingDecisionRank,
                missionIndexAtRank = missionIndexAtRank,
                missionObjectiveProgress = missionObjectiveProgress,
                totalMissionsCompleted = totalMissionsCompleted,
                missionBriefingPending = missionBriefingPending
            };
        }

        private void AutoSave()
        {
            if (Application.isPlaying)
            {
                WriteSave(false);
            }
        }

        private void WriteSave(bool showFeedback)
        {
            File.WriteAllText(SavePath, JsonUtility.ToJson(CreateSaveData(), true));
            if (!showFeedback)
            {
                return;
            }

            lastFeedback = "Carreira salva.";
            Changed?.Invoke();
        }

        private void PerformCoding()
        {
            float energyFactor = Mathf.Lerp(0.55f, 1f, energy / 100f);
            float stressFactor = Mathf.Lerp(1f, 0.68f, stress / 100f);
            float knowledgeBonus = Mathf.Min(10f, knowledge * 0.08f);
            int output = Mathf.RoundToInt(32f * energyFactor * stressFactor + knowledgeBonus);
            delivery += Mathf.Max(10, output);
            energy -= 18;
            stress += 13;
            knowledge += 2;
            reputation += 1;
            lastFeedback = energy < 25
                ? "Você programou, mas o cansaço reduziu seu ritmo."
                : "Código entregue com qualidade.";
        }

        private void TrackMissionAction(OfficeActionType performedAction)
        {
            bool progressed = false;
            for (int index = 0; index < MissionObjectiveCount; index++)
            {
                MissionObjective objective = GetMissionObjective(index);
                if (!CareerMissionCatalog.TryGetAction(objective.Kind, out OfficeActionType requiredAction) ||
                    requiredAction != performedAction || IsMissionObjectiveComplete(index))
                {
                    continue;
                }

                missionObjectiveProgress[index] = Mathf.Min(objective.Required, missionObjectiveProgress[index] + 1);
                progressed = true;
            }

            if (progressed)
            {
                lastFeedback += $"  Checklist: {MissionObjectiveProgress}/{MissionObjectiveGoal}.";
            }
        }

        private void ProducePassiveDelivery()
        {
            int output = PassiveDeliveryPerTick;
            delivery += output;
            careerXp += output;
            money += Mathf.Max(1, output / 2);
            energy = Mathf.Min(100, energy + 1);
            stress = Mathf.Max(0, stress - 1);
            lastFeedback = $"{ToolingName} entregou +{output} automaticamente.";
            if (MissionReadyToClaim)
            {
                lastFeedback += "  Checklist concluído: finalize a missão.";
            }
        }

        private void PromoteWhileEligible()
        {
            bool promoted = false;
            while (!IsLastRank &&
                missionIndexAtRank >= CareerMissionCatalog.MissionsPerRank &&
                careerXp >= XpRequiredForNextRank)
            {
                promoted |= TryPromote();
            }

            if (promoted)
            {
                RequestSceneForCurrentRank();
            }
        }

        private void ApplyCareerEvent()
        {
            lastCareerEvent = "Nenhum evento de carreira hoje.";
            if ((day + (int)rank) % 3 != 0)
            {
                return;
            }

            int selector = (day * 7 + (int)rank * 3) % 4;
            switch (selector)
            {
                case 0:
                    knowledge += 6;
                    lastCareerEvent = "EVENTO: workshop surpresa — +6 conhecimento.";
                    break;
                case 1:
                    stress = Mathf.Clamp(stress + 14, 0, 100);
                    reputation += 6;
                    lastCareerEvent = "EVENTO: incidente em produção — +14 estresse, +6 reputação.";
                    break;
                case 2:
                    reputation += 8;
                    lastCareerEvent = "EVENTO: elogio de um cliente — +8 reputação.";
                    break;
                default:
                    int projectBonus = Mathf.Max(1, Mathf.RoundToInt(DailyPay * 0.3f));
                    money += projectBonus;
                    lastCareerEvent = $"EVENTO: bônus de projeto — +R$ {projectBonus}.";
                    break;
            }

            lastFeedback += $"  {lastCareerEvent}";
        }

        private bool TryPromote()
        {
            if (IsLastRank ||
                missionIndexAtRank < CareerMissionCatalog.MissionsPerRank ||
                careerXp < XpRequiredForNextRank)
            {
                return false;
            }

            rank++;
            successfulDaysAtRank = 0;
            delivery = 0;
            missionIndexAtRank = 0;
            ResetMissionProgress();
            sprintBonusClaimed = false;
            money += Mathf.Max(50, DailyPay / 2);
            pendingDecisionRank = IsLastRank ? -1 : (int)rank;
            reputation += 10;
            knowledge += 8;
            lastFeedback += $"  PROMOÇÃO: agora você é {RankName}!";
            Promoted?.Invoke(rank);
            AutoSave();
            return true;
        }

        private static int EstimateXpFromLegacySave(
            CareerRank savedRank,
            int productiveDaysAtRank,
            int currentDayDelivery)
        {
            if (savedRank == CareerRank.Owner)
            {
                return TotalXpForNextRank[(int)CareerRank.Cto];
            }

            int rankIndex = (int)savedRank;
            int rankStart = rankIndex == 0 ? 0 : TotalXpForNextRank[rankIndex - 1];
            int rankEnd = TotalXpForNextRank[rankIndex];
            int daysRequired = GoodDaysForPromotion[rankIndex];
            float progress = daysRequired <= 0
                ? 0f
                : Mathf.Clamp01(productiveDaysAtRank / (float)daysRequired);
            int migratedXp = rankStart + Mathf.RoundToInt((rankEnd - rankStart) * progress);
            return Mathf.Min(rankEnd, migratedXp + Mathf.Max(0, currentDayDelivery));
        }

        private static int MaxToolingLevelFor(CareerRank value)
        {
            int level = 0;
            while (level < ToolUnlockRanks.Length && value >= ToolUnlockRanks[level])
            {
                level++;
            }
            return level;
        }

        private void RequestSceneForCurrentRank()
        {
            if (!Application.isPlaying || CareerSceneFlow.IsCurrentSceneCorrect(rank))
            {
                return;
            }

            if (pendingSceneChange != null)
            {
                StopCoroutine(pendingSceneChange);
            }
            pendingSceneChange = StartCoroutine(LoadCareerSceneNextFrame());
        }

        private IEnumerator LoadCareerSceneNextFrame()
        {
            yield return null;
            pendingSceneChange = null;
            string sceneName = CareerSceneFlow.SceneFor(rank);
            if (Application.CanStreamedLevelBeLoaded(sceneName))
            {
                SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
            }
            else
            {
                Debug.LogError($"Cena da carreira não encontrada no build: {sceneName}");
            }
        }
    }
}
