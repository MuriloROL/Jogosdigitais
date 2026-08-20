using ProjetoJogosDigitais.Gameplay;
using UnityEngine;

namespace ProjetoJogosDigitais.Presentation
{
    public sealed class CareerNameBadge : MonoBehaviour
    {
        [SerializeField] private TextMesh label;
        private CareerRank displayedRank = (CareerRank)(-1);

        public void Configure(TextMesh text)
        {
            label = text;
        }

        private void Update()
        {
            CareerGame game = CareerGame.Current;
            if (label == null || game == null || displayedRank == game.Rank)
            {
                return;
            }

            displayedRank = game.Rank;
            label.text = $"ALEX • {game.RankName.ToUpperInvariant()}";
        }
    }
}
