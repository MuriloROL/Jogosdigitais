using UnityEngine;

namespace ProjetoJogosDigitais.Gameplay
{
    public sealed class FurnitureVisualCatalog : MonoBehaviour
    {
        [SerializeField] private Sprite[] fridgeFrames;
        [SerializeField] private Sprite[] bedFrames;
        [SerializeField] private Sprite[] showerFrames;
        [SerializeField] private Sprite[] televisionFrames;

        public static FurnitureVisualCatalog Current { get; private set; }

        public void Configure(Sprite[] fridge, Sprite[] bed, Sprite[] shower, Sprite[] television)
        {
            fridgeFrames = fridge;
            bedFrames = bed;
            showerFrames = shower;
            televisionFrames = television;
        }

        private void Awake()
        {
            Current = this;
        }

        private void OnDestroy()
        {
            if (Current == this)
            {
                Current = null;
            }
        }

        public Sprite[] Get(FurnitureType type)
        {
            return type switch
            {
                FurnitureType.Fridge => fridgeFrames,
                FurnitureType.Bed => bedFrames,
                FurnitureType.Shower => showerFrames,
                FurnitureType.Armchair => televisionFrames,
                _ => null
            };
        }
    }
}
