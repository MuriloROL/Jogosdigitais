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
            public CareerRank rank;
            public int day;
            public int minutesUsed;
            public int money;
            public int delivery;
            public int energy;
            public int stress;
            public int knowledge;
            public int reputation;
            public int successfulDaysAtRank;
            public int totalSuccessfulDays;
            public string lastFeedback;
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
        private static readonly int[] GoodDaysForPromotion = { 2, 3, 4, 5, 5, 6, 0 };

        public const int WorkdayMinutes = 8 * 60;
        public const int StartHour = 9;

        [SerializeField] private CareerRank rank = CareerRank.Intern;
        [SerializeField, Min(1)] private int day = 1;
        [SerializeField] private int minutesUsed;
        [SerializeField] private int money = 120;
        [SerializeField] private int delivery;
        [SerializeField, Range(0, 100)] private int energy = 100;
        [SerializeField, Range(0, 100)] private int stress = 8;
        [SerializeField] private int knowledge;
        [SerializeField] private int reputation;
        [SerializeField] private int successfulDaysAtRank;
        [SerializeField] private int totalSuccessfulDays;
        [SerializeField] private string lastFeedback = "Primeiro dia: entregue valor e construa sua carreira.";

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
        public int SuccessfulDaysAtRank => successfulDaysAtRank;
        public int TotalSuccessfulDays => totalSuccessfulDays;
        public int GoodDaysRequired => GoodDaysForPromotion[(int)rank];
        public int MonthlySalary => MonthlySalaries[(int)rank];
        public int DailyPay => Mathf.RoundToInt(MonthlySalary / 22f);
        public bool GoalReached => delivery >= DailyGoal;
        public bool IsLastRank => rank == CareerRank.Owner;
        public string LastFeedback => lastFeedback;
        public string SavePath => string.IsNullOrEmpty(savePathOverride)
            ? Path.Combine(Application.persistentDataPath, "dev-career-mvp.json")
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
            int duration = ActionDuration(type);
            if (minutesUsed + duration > WorkdayMinutes)
            {
                lastFeedback = "O expediente terminou. Encerre o dia para continuar.";
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

            minutesUsed += duration;
            energy = Mathf.Clamp(energy, 0, 100);
            stress = Mathf.Clamp(stress, 0, 100);
            knowledge = Mathf.Max(0, knowledge);
            reputation = Mathf.Max(0, reputation);

            int gained = delivery - deliveryBefore;
            if (gained > 0)
            {
                lastFeedback += $"  +{gained} de entrega.";
            }
            if (GoalReached)
            {
                lastFeedback += "  Meta do dia concluída!";
            }

            Changed?.Invoke();
            return new OfficeActionResult(true, lastFeedback, gained);
        }

        public bool EndDay()
        {
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
                lastFeedback = $"Dia produtivo! Salário + bônus: R$ {payment + bonus}.";
            }
            else
            {
                reputation = Mathf.Max(0, reputation - 2);
                lastFeedback = $"Meta não atingida. Você recebeu R$ {payment}, mas o dia não contou para a promoção.";
            }

            bool promoted = TryPromote();
            day++;
            minutesUsed = 0;
            delivery = 0;
            energy = 100;
            stress = Mathf.Max(5, stress - 45);
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
                rank = rank,
                day = day,
                minutesUsed = minutesUsed,
                money = money,
                delivery = delivery,
                energy = energy,
                stress = stress,
                knowledge = knowledge,
                reputation = reputation,
                successfulDaysAtRank = successfulDaysAtRank,
                totalSuccessfulDays = totalSuccessfulDays,
                lastFeedback = lastFeedback
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
            day = Mathf.Max(1, data.day);
            minutesUsed = Mathf.Clamp(data.minutesUsed, 0, WorkdayMinutes);
            money = Mathf.Max(0, data.money);
            delivery = Mathf.Max(0, data.delivery);
            energy = Mathf.Clamp(data.energy, 0, 100);
            stress = Mathf.Clamp(data.stress, 0, 100);
            knowledge = Mathf.Max(0, data.knowledge);
            reputation = Mathf.Max(0, data.reputation);
            successfulDaysAtRank = Mathf.Max(0, data.successfulDaysAtRank);
            totalSuccessfulDays = Mathf.Max(0, data.totalSuccessfulDays);
            lastFeedback = string.IsNullOrEmpty(data.lastFeedback) ? "Carreira carregada." : data.lastFeedback;
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

        private bool TryPromote()
        {
            if (IsLastRank || successfulDaysAtRank < GoodDaysRequired)
            {
                return false;
            }

            rank++;
            successfulDaysAtRank = 0;
            reputation += 10;
            knowledge += 8;
            lastFeedback += $"  PROMOÇÃO: agora você é {RankName}!";
            Promoted?.Invoke(rank);
            return true;
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
