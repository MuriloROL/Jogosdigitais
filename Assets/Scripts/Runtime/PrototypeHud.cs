using ProjetoJogosDigitais.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace ProjetoJogosDigitais.Presentation
{
    public sealed class PrototypeHud : MonoBehaviour
    {
        [SerializeField] private SimNeeds needs;
        [SerializeField] private SimController controller;
        [SerializeField] private Image hungerFill;
        [SerializeField] private Text hungerValue;
        [SerializeField] private Text statusValue;

        public void Configure(
            SimNeeds simNeeds,
            SimController simController,
            Image fill,
            Text hungerLabel,
            Text statusLabel)
        {
            needs = simNeeds;
            controller = simController;
            hungerFill = fill;
            hungerValue = hungerLabel;
            statusValue = statusLabel;
        }

        private void Update()
        {
            if (needs == null)
            {
                return;
            }

            float hunger = needs.Hunger.Current;
            hungerFill.fillAmount = hunger;
            hungerFill.color = Color.Lerp(new Color(0.88f, 0.25f, 0.20f), new Color(0.35f, 0.82f, 0.42f), hunger);
            hungerValue.text = $"Fome  {Mathf.RoundToInt(hunger * 100f)}%";
            statusValue.text = controller != null ? controller.Status : string.Empty;
        }
    }
}
