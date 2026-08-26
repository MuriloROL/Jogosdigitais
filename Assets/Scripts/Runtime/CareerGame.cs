using System;
using System.Collections;
using System.IO;
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
        }

        private readonly struct DailyTaskDefinition
        {
            public DailyTaskDefinition(
                string title,
                string description,
                OfficeActionType requiredAction,
                int requiredCount,
                int rewardXp)
            {
                Title = title;
                Description = description;
                RequiredAction = requiredAction;
                RequiredCount = requiredCount;
                RewardXp = rewardXp;
            }

            public string Title { get; }
            public string Description { get; }
            public OfficeActionType RequiredAction { get; }
            public int RequiredCount { get; }
            public int RewardXp { get; }
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
        private static readonly int[] GoodDaysForPromotion = { 1, 1, 1, 1, 1, 1, 0 };
        private static readonly int[] TotalXpForNextRank = { 350, 900, 1700, 2800, 4200, 7500, 7500 };
        private static readonly DailyTaskDefinition[] DailyTasks =
        {
            new("CAÇA AOS BUGS", "Programe duas vezes para eliminar erros críticos.", OfficeActionType.Code, 2, 35),
            new("ALINHAMENTO DE SPRINT", "Participe de duas reuniões com a equipe.", OfficeActionType.Meeting, 2, 30),
            new("UPSKILL", "Complete duas sessões de estudo.", OfficeActionType.Study, 2, 30),
            new("FOCO PROFUNDO", "Conclua três sessões de programação.", OfficeActionType.Code, 3, 45),
            new("RITMO SUSTENTÁVEL", "Faça uma pausa antes que o estresse domine.", OfficeActionType.Break, 1, 20),
            new("CAFÉ COM O TIME", "Visite a cafeteira e recupere o ritmo.", OfficeActionType.Coffee, 1, 15),
            new("PONTE COM O TIME", "Participe de uma reunião importante.", OfficeActionType.Meeting, 1, 20)
        };

        private const int CurrentSaveVersion = 5;
        private const int ProductiveDayXpBonus = 25;
        private const float IdleTickSeconds = 15f;
        private const int MaxAutomationLevel = 5;
        public const int WorkdayMinutes = 8 * 60;
        public const int StartHour = 9;
        public const float WorkdayRealSeconds = 8f * 60f * 60f;

        [SerializeField] private CareerRank rank = CareerRank.Intern;
        [SerializeField, Min(1)] private int day = 1;
        [SerializeField] private int minutesUsed;
        [SerializeField] private int money = 120;
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
        public int DailyGoal => 100 + (int)rank * 18;
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
        public int TeamCapacity => IsLastRank ? 6 : Mathf.Min(6, (int)rank + 1);
        public int PassiveDeliveryPerTick => Mathf.Max(1, Mathf.RoundToInt((1 + teamMembers) * (1f + automationLevel * 0.25f)));
        public float PassiveDeliveryPerMinute => PassiveDeliveryPerTick * (60f / IdleTickSeconds);
        public float SecondsUntilPassiveTick => Mathf.Max(0f, IdleTickSeconds - idleTickAccumulator);
        public int NextHireCost => 180 * (teamMembers + 1);
        public int NextAutomationCost => 450 * (automationLevel + 1);
        public bool CanHire => !IsLastRank && teamMembers < TeamCapacity && money >= NextHireCost;
        public bool CanUpgradeAutomation => !IsLastRank && automationLevel < MaxAutomationLevel && money >= NextAutomationCost;
        public bool SprintBonusClaimed => sprintBonusClaimed;
        public bool HasPendingCareerDecision => pendingDecisionRank >= 0 && pendingDecisionRank < (int)CareerRank.Owner;
        public CareerRank PendingDecisionRank => HasPendingCareerDecision ? (CareerRank)pendingDecisionRank : rank;
        public string DailyTaskTitle => CurrentDailyTask.Title;
        public string DailyTaskDescription => CurrentDailyTask.Description;
        public int DailyTaskProgress => dailyTaskProgress;
        public int DailyTaskGoal => CurrentDailyTask.RequiredCount;
        public int DailyTaskRewardXp => CurrentDailyTask.RewardXp;
        public bool DailyTaskCompleted => dailyTaskCompleted;
        public string LastCareerEvent => lastCareerEvent;
        public int GoodDaysRequired => GoodDaysForPromotion[(int)rank];
        public int MonthlySalary => MonthlySalaries[(int)rank];
        public int DailyPay => Mathf.RoundToInt(MonthlySalary / 22f);
        public bool GoalReached => delivery >= DailyGoal;
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

        private DailyTaskDefinition CurrentDailyTask
        {
            get
            {
                int index = (day - 1 + (int)rank * 3) % DailyTasks.Length;
                return DailyTasks[index];
            }
        }

        private void Awake()
        {
            if (Current != null && Current != this)
            {
                Destroy(gameObject);
                return;
            }

            Current = this;
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

        public OfficeActionResult PerformAction(OfficeActionType type)
        {
            if (elapsedWorkSeconds >= WorkdayRealSeconds)
            {
                lastFeedback = "O expediente terminou. Sua carreira de hoje foi concluída.";
                Changed?.Invoke();
                return new OfficeActionResult(false, lastFeedback, 0);
            }

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
            TrackDailyTask(type);
            if (GoalReached)
            {
                lastFeedback += "  Meta da sprint disponível para resgate!";
            }

            PromoteWhileEligible();
            Changed?.Invoke();
            return new OfficeActionResult(true, lastFeedback, gained);
        }

        public void AdvanceOpenTime(float seconds)
        {
            if (seconds <= 0f || IsLastRank)
            {
                return;
            }

            float allowed = Mathf.Min(seconds, WorkdayRealSeconds - elapsedWorkSeconds);
            if (allowed <= 0f)
            {
                return;
            }

            int minuteBefore = minutesUsed;
            elapsedWorkSeconds += allowed;
            minutesUsed = Mathf.Clamp(Mathf.FloorToInt(elapsedWorkSeconds / 60f), 0, WorkdayMinutes);
            idleTickAccumulator += allowed;

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

            int cost = NextAutomationCost;
            if (money < cost)
            {
                lastFeedback = $"Faltam R$ {cost - money} para melhorar a automação.";
                Changed?.Invoke();
                return false;
            }

            money -= cost;
            automationLevel++;
            knowledge += 4;
            lastFeedback = $"Pipeline automatizado no nível {automationLevel}! Produção passiva: {PassiveDeliveryPerTick} pontos por ciclo.";
            Changed?.Invoke();
            return true;
        }

        public bool ClaimSprintBonus()
        {
            if (!GoalReached)
            {
                lastFeedback = $"Ainda faltam {DailyGoal - delivery} pontos para resgatar a meta da sprint.";
                Changed?.Invoke();
                return false;
            }

            if (sprintBonusClaimed)
            {
                lastFeedback = "O bônus desta sprint já foi resgatado.";
                Changed?.Invoke();
                return false;
            }

            sprintBonusClaimed = true;
            int bonus = Mathf.Max(25, Mathf.RoundToInt(DailyPay * 0.15f));
            money += bonus;
            careerXp += ProductiveDayXpBonus;
            reputation += 3;
            successfulDaysAtRank++;
            totalSuccessfulDays++;
            lastFeedback = $"Meta resgatada! +R$ {bonus} e +{ProductiveDayXpBonus} XP.";
            PromoteWhileEligible();
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
            Changed?.Invoke();
        }

        public bool EndDay()
        {
            if (day == 1 && !IsLastRank)
            {
                return ClaimSprintBonus();
            }

            bool successful = GoalReached;
            int payment = DailyPay;
            money += payment;

            if (successful)
            {
                int bonus = Mathf.RoundToInt(payment * 0.2f);
                money += bonus;
                successfulDaysAtRank++;
                totalSuccessfulDays++;
                reputation += 3;
                careerXp += ProductiveDayXpBonus;
                lastFeedback = $"Dia produtivo! R$ {payment + bonus} e +{ProductiveDayXpBonus} XP de bônus.";
            }
            else
            {
                reputation = Mathf.Max(0, reputation - 2);
                lastFeedback = $"Meta não atingida. Você recebeu R$ {payment}; todo XP conquistado foi mantido.";
            }

            bool promoted = TryPromote();
            day++;
            minutesUsed = 0;
            delivery = 0;
            energy = 100;
            stress = Mathf.Max(5, stress - 45);
            dailyTaskProgress = 0;
            dailyTaskCompleted = false;
            ApplyCareerEvent();
            Changed?.Invoke();
            if (promoted)
            {
                RequestSceneForCurrentRank();
            }
            return promoted;
        }

        public void SaveGame()
        {
            var data = new SaveData
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
                dailyTaskProgress = dailyTaskProgress,
                dailyTaskCompleted = dailyTaskCompleted,
                lastCareerEvent = lastCareerEvent,
                successfulDaysAtRank = successfulDaysAtRank,
                totalSuccessfulDays = totalSuccessfulDays,
                lastFeedback = lastFeedback,
                elapsedWorkSeconds = elapsedWorkSeconds,
                idleTickAccumulator = idleTickAccumulator,
                teamMembers = teamMembers,
                automationLevel = automationLevel,
                sprintBonusClaimed = sprintBonusClaimed,
                pendingDecisionRank = pendingDecisionRank
            };
            File.WriteAllText(SavePath, JsonUtility.ToJson(data, true));
            lastFeedback = "Carreira salva.";
            Changed?.Invoke();
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
            dailyTaskProgress = data.saveVersion >= 3
                ? Mathf.Clamp(data.dailyTaskProgress, 0, CurrentDailyTask.RequiredCount)
                : 0;
            dailyTaskCompleted = data.saveVersion >= 3 && data.dailyTaskCompleted;
            lastCareerEvent = string.IsNullOrEmpty(data.lastCareerEvent)
                ? "Nenhum evento de carreira hoje."
                : data.lastCareerEvent;
            successfulDaysAtRank = Mathf.Max(0, data.successfulDaysAtRank);
            totalSuccessfulDays = Mathf.Max(0, data.totalSuccessfulDays);
            lastFeedback = string.IsNullOrEmpty(data.lastFeedback) ? "Carreira carregada." : data.lastFeedback;
            elapsedWorkSeconds = data.saveVersion >= CurrentSaveVersion
                ? Mathf.Clamp(data.elapsedWorkSeconds, 0f, WorkdayRealSeconds)
                : minutesUsed * 60f;
            idleTickAccumulator = data.saveVersion >= CurrentSaveVersion
                ? Mathf.Clamp(data.idleTickAccumulator, 0f, IdleTickSeconds)
                : 0f;
            teamMembers = data.saveVersion >= CurrentSaveVersion ? Mathf.Clamp(data.teamMembers, 0, 6) : 0;
            automationLevel = data.saveVersion >= CurrentSaveVersion
                ? Mathf.Clamp(data.automationLevel, 0, MaxAutomationLevel)
                : 0;
            sprintBonusClaimed = data.saveVersion >= CurrentSaveVersion && data.sprintBonusClaimed;
            pendingDecisionRank = data.saveVersion >= CurrentSaveVersion
                ? Mathf.Clamp(data.pendingDecisionRank, -1, (int)CareerRank.Cto)
                : -1;
            Changed?.Invoke();
            RequestSceneForCurrentRank();
            return true;
        }

        public void SetSavePathOverride(string path)
        {
            savePathOverride = path;
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

        private void TrackDailyTask(OfficeActionType performedAction)
        {
            DailyTaskDefinition task = CurrentDailyTask;
            if (dailyTaskCompleted || performedAction != task.RequiredAction)
            {
                return;
            }

            dailyTaskProgress = Mathf.Min(task.RequiredCount, dailyTaskProgress + 1);
            if (dailyTaskProgress < task.RequiredCount)
            {
                lastFeedback += $"  Tarefa do dia: {dailyTaskProgress}/{task.RequiredCount}.";
                return;
            }

            dailyTaskCompleted = true;
            careerXp += task.RewardXp;
            lastFeedback += $"  TAREFA CONCLUÍDA: {task.Title}! +{task.RewardXp} XP.";
        }

        private void ProducePassiveDelivery()
        {
            int output = PassiveDeliveryPerTick;
            delivery += output;
            careerXp += output;
            money += Mathf.Max(1, output / 2);
            energy = Mathf.Min(100, energy + 1);
            stress = Mathf.Max(0, stress - 1);
            lastFeedback = $"Equipe entregou +{output} automaticamente.";
            if (GoalReached && !sprintBonusClaimed)
            {
                lastFeedback += "  Meta disponível para resgate!";
            }

            PromoteWhileEligible();
        }

        private void PromoteWhileEligible()
        {
            bool promoted = false;
            while (!IsLastRank && careerXp >= XpRequiredForNextRank)
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
            if (IsLastRank || careerXp < XpRequiredForNextRank)
            {
                return false;
            }

            rank++;
            successfulDaysAtRank = 0;
            delivery = 0;
            dailyTaskProgress = 0;
            dailyTaskCompleted = false;
            sprintBonusClaimed = false;
            money += Mathf.Max(50, DailyPay / 2);
            pendingDecisionRank = IsLastRank ? -1 : (int)rank;
            reputation += 10;
            knowledge += 8;
            lastFeedback += $"  PROMOÇÃO: agora você é {RankName}!";
            Promoted?.Invoke(rank);
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
