using ProjetoJogosDigitais.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace ProjetoJogosDigitais.Presentation
{
    public sealed class MvpHud : MonoBehaviour
    {
        [SerializeField] private SimController player;
        [SerializeField] private GameSession session;
        [SerializeField] private GameClock clock;
        [SerializeField] private Image[] needFills;
        [SerializeField] private Text[] needLabels;
        [SerializeField] private Text statusLabel;
        [SerializeField] private Text queueLabel;
        [SerializeField] private Text clockLabel;
        [SerializeField] private Text moneyLabel;
        [SerializeField] private Text relationshipLabel;

        public void Configure(
            SimController playerSim,
            GameSession gameSession,
            GameClock gameClock,
            Image[] fills,
            Text[] labels,
            Text status,
            Text queue,
            Text clockText,
            Text moneyText,
            Text relationshipText)
        {
            player = playerSim;
            session = gameSession;
            clock = gameClock;
            needFills = fills;
            needLabels = labels;
            statusLabel = status;
            queueLabel = queue;
            clockLabel = clockText;
            moneyLabel = moneyText;
            relationshipLabel = relationshipText;
        }

        private void Update()
        {
            if (player == null)
            {
                return;
            }

            for (int index = 0; index < 4; index++)
            {
                NeedType type = (NeedType)index;
                float value = player.Needs.Get(type).Current;
                needFills[index].fillAmount = value;
                needFills[index].color = Color.Lerp(
                    new Color(0.88f, 0.25f, 0.20f),
                    new Color(0.35f, 0.82f, 0.42f),
                    value);
                needLabels[index].text = $"{NeedName(type)}  {Mathf.RoundToInt(value * 100f)}%";
            }

            statusLabel.text = player.Status;
            queueLabel.text = "Fila: " + player.QueueSummary();
            clockLabel.text = clock.DisplayTime;
            moneyLabel.text = $"§ {session.Money}";
            relationshipLabel.text = session.RelationshipSummary();
        }

        private static string NeedName(NeedType type)
        {
            return type switch
            {
                NeedType.Hunger => "Fome",
                NeedType.Energy => "Energia",
                NeedType.Hygiene => "Higiene",
                NeedType.Social => "Social",
                _ => type.ToString()
            };
        }
    }
}
