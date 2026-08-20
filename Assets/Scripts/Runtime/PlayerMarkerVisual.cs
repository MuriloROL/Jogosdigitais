using UnityEngine;

namespace ProjetoJogosDigitais.Presentation
{
    public sealed class PlayerMarkerVisual : MonoBehaviour
    {
        private void Awake()
        {
            GameObject marker = PixelShapes.CreateBlock(
                transform,
                "Indicador do jogador",
                new Vector3(0.5f, 2.45f, 0f),
                new Vector2(0.28f, 0.48f),
                new Color(0.25f, 0.95f, 0.58f),
                38);
            marker.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
        }
    }
}
