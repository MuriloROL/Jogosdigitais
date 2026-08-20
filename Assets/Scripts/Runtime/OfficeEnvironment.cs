using ProjetoJogosDigitais.Gameplay;
using UnityEngine;

namespace ProjetoJogosDigitais.Presentation
{
    /// <summary>
    /// Presents the hand-authored office artwork while keeping its world scale
    /// aligned with the movement and interaction layout created by the scene builder.
    /// </summary>
    public sealed class OfficeEnvironment : MonoBehaviour
    {
        [SerializeField] private OfficeTier tier;
        [SerializeField] private Sprite background;

        public void Configure(OfficeTier officeTier, Sprite artwork)
        {
            tier = officeTier;
            background = artwork;
        }

        private void Awake()
        {
            BuildOffice();
        }

        private void BuildOffice()
        {
            if (background == null)
            {
                Debug.LogError($"Arte do escritório {tier} não foi configurada.", this);
                return;
            }

            var artworkObject = new GameObject("Arte completa do escritório");
            artworkObject.transform.SetParent(transform, false);

            SpriteRenderer renderer = artworkObject.AddComponent<SpriteRenderer>();
            renderer.sprite = background;
            renderer.sortingOrder = -30;
            renderer.color = Color.white;

            float targetWidth = tier switch
            {
                OfficeTier.Intern => 15f,
                OfficeTier.Developer => 22f,
                OfficeTier.Leadership => 24f,
                OfficeTier.Executive => 21f,
                _ => 22f
            };
            float scale = targetWidth / background.bounds.size.x;
            artworkObject.transform.localScale = new Vector3(scale, scale, 1f);
        }
    }
}
