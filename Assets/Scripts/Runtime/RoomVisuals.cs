using UnityEngine;

namespace ProjetoJogosDigitais.Presentation
{
    public sealed class RoomVisuals : MonoBehaviour
    {
        private static readonly Color FloorA = new(0.78f, 0.68f, 0.55f);
        private static readonly Color FloorB = new(0.73f, 0.61f, 0.48f);
        private static readonly Color Wall = new(0.93f, 0.85f, 0.70f);
        private static readonly Color WallTrim = new(0.39f, 0.24f, 0.22f);

        private void Awake()
        {
            BuildRoom();
        }

        private void BuildRoom()
        {
            for (int x = -7; x <= 7; x++)
            {
                for (int y = -4; y <= 3; y++)
                {
                    Color color = (x + y) % 2 == 0 ? FloorA : FloorB;
                    PixelShapes.CreateBlock(
                        transform,
                        $"Floor {x},{y}",
                        new Vector3(x, y, 0f),
                        Vector2.one,
                        color,
                        -20);
                }
            }

            PixelShapes.CreateBlock(transform, "Top wall", new Vector3(0f, 3.8f), new Vector2(15f, 1.4f), Wall, -10);
            PixelShapes.CreateBlock(transform, "Top trim", new Vector3(0f, 3.08f), new Vector2(15f, 0.18f), WallTrim, -9);
            PixelShapes.CreateBlock(transform, "Left wall", new Vector3(-7.42f, -0.4f), new Vector2(0.18f, 7f), WallTrim, -9);
            PixelShapes.CreateBlock(transform, "Right wall", new Vector3(7.42f, -0.4f), new Vector2(0.18f, 7f), WallTrim, -9);
            PixelShapes.CreateBlock(transform, "Bottom wall", new Vector3(0f, -4.02f), new Vector2(15f, 0.18f), WallTrim, -9);

            PixelShapes.CreateBlock(transform, "Kitchen counter", new Vector3(4.9f, 2.55f), new Vector2(3.8f, 0.75f), new Color(0.53f, 0.35f, 0.27f), -5);
            PixelShapes.CreateBlock(transform, "Kitchen top", new Vector3(4.9f, 2.9f), new Vector2(3.8f, 0.12f), new Color(0.88f, 0.78f, 0.62f), -4);
            PixelShapes.CreateBlock(transform, "Rug", new Vector3(-1.2f, -0.7f), new Vector2(4.2f, 2.4f), new Color(0.35f, 0.57f, 0.56f), -8);
            PixelShapes.CreateBlock(transform, "Rug inset", new Vector3(-1.2f, -0.7f), new Vector2(3.7f, 1.9f), new Color(0.76f, 0.72f, 0.56f), -7);
        }
    }
}
