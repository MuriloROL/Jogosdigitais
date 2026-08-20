using UnityEngine;

namespace ProjetoJogosDigitais.Presentation
{
    public static class NeighborhoodLayout
    {
        public static readonly Vector2[] HouseCenters =
        {
            new(-14f, 10f),
            new(14f, 10f),
            new(-14f, -10f),
            new(14f, -10f)
        };

        public static readonly Vector2[] HouseExtents =
        {
            new(6.4f, 5.1f),
            new(8.2f, 5.1f),
            new(5.8f, 4.4f),
            new(5.8f, 2.5f)
        };

        public static bool IsInsideHouse(Vector2 position)
        {
            for (int index = 0; index < HouseCenters.Length; index++)
            {
                Vector2 center = HouseCenters[index];
                Vector2 extents = HouseExtents[index];
                if (Mathf.Abs(position.x - center.x) <= extents.x &&
                    Mathf.Abs(position.y - center.y) <= extents.y)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public sealed class NeighborhoodVisuals : MonoBehaviour
    {
        [SerializeField] private Sprite[] houseMaps;

        private static readonly Color Grass = new(0.25f, 0.45f, 0.31f);
        private static readonly Color Road = new(0.20f, 0.22f, 0.27f);
        private static readonly Color Sidewalk = new(0.58f, 0.61f, 0.61f);

        public void Configure(Sprite[] maps)
        {
            houseMaps = maps;
        }

        private void Awake()
        {
            BuildNeighborhood();
        }

        private void BuildNeighborhood()
        {
            PixelShapes.CreateBlock(transform, "Gramado", Vector3.zero, new Vector2(52f, 38f), Grass, -40);
            PixelShapes.CreateBlock(transform, "Rua vertical", Vector3.zero, new Vector2(5f, 38f), Road, -30);
            PixelShapes.CreateBlock(transform, "Rua horizontal", Vector3.zero, new Vector2(52f, 4.5f), Road, -30);
            PixelShapes.CreateBlock(transform, "Calçada oeste", new Vector3(-3f, 0f), new Vector2(0.75f, 38f), Sidewalk, -29);
            PixelShapes.CreateBlock(transform, "Calçada leste", new Vector3(3f, 0f), new Vector2(0.75f, 38f), Sidewalk, -29);
            PixelShapes.CreateBlock(transform, "Calçada norte", new Vector3(0f, 2.75f), new Vector2(52f, 0.75f), Sidewalk, -29);
            PixelShapes.CreateBlock(transform, "Calçada sul", new Vector3(0f, -2.75f), new Vector2(52f, 0.75f), Sidewalk, -29);

            for (int index = 0; index < NeighborhoodLayout.HouseCenters.Length; index++)
            {
                BuildHouse(index, NeighborhoodLayout.HouseCenters[index]);
            }
        }

        private void BuildHouse(int index, Vector2 center)
        {
            if (houseMaps == null || index >= houseMaps.Length || houseMaps[index] == null)
            {
                return;
            }

            Sprite map = houseMaps[index];
            PixelShapes.CreateBlock(
                transform,
                $"Casa {index + 1} - sombra",
                center + Vector2.down * 0.25f,
                (Vector2)map.bounds.size + Vector2.one * 0.5f,
                new Color(0.08f, 0.09f, 0.12f, 0.45f),
                -21);
            var houseObject = new GameObject($"Casa {index + 1} - Modern Interiors");
            houseObject.transform.SetParent(transform, false);
            houseObject.transform.localPosition = center;
            SpriteRenderer renderer = houseObject.AddComponent<SpriteRenderer>();
            renderer.sprite = map;
            renderer.sortingOrder = -20;
            BuildWallColliders(houseObject.transform, index);
        }

        private static void BuildWallColliders(Transform house, int index)
        {
            switch (index)
            {
                case 0:
                    AddOuterWalls(house, 7f, 6.65f, 1.3f);
                    AddWall(house, "Parede interna esquerda", new Vector2(-4.05f, -1.25f), new Vector2(5.9f, 0.22f));
                    AddWall(house, "Parede interna direita", new Vector2(4.05f, -1.25f), new Vector2(5.9f, 0.22f));
                    break;
                case 1:
                    AddOuterWalls(house, 9.45f, 6.65f, 2.3f);
                    break;
                case 2:
                    AddOuterWalls(house, 6.9f, 5.4f, 1.35f);
                    break;
                case 3:
                    AddWall(house, "Parede superior", new Vector2(0f, 2.9f), new Vector2(13.8f, 0.24f));
                    AddWall(house, "Parede esquerda", new Vector2(-6.9f, 0f), new Vector2(0.24f, 5.8f));
                    AddWall(house, "Parede direita", new Vector2(6.9f, 0f), new Vector2(0.24f, 5.8f));
                    AddWall(house, "Parede inferior esquerda", new Vector2(-5.7f, -2.9f), new Vector2(2.4f, 0.24f));
                    AddWall(house, "Parede inferior central", new Vector2(0f, -2.9f), new Vector2(6.0f, 0.24f));
                    AddWall(house, "Parede inferior direita", new Vector2(5.7f, -2.9f), new Vector2(2.4f, 0.24f));
                    break;
            }
        }

        private static void AddOuterWalls(Transform house, float halfWidth, float halfHeight, float bottomDoorHalfWidth)
        {
            AddWall(house, "Parede superior", new Vector2(0f, halfHeight), new Vector2(halfWidth * 2f, 0.24f));
            AddWall(house, "Parede esquerda", new Vector2(-halfWidth, 0f), new Vector2(0.24f, halfHeight * 2f));
            AddWall(house, "Parede direita", new Vector2(halfWidth, 0f), new Vector2(0.24f, halfHeight * 2f));

            float segmentWidth = halfWidth - bottomDoorHalfWidth;
            float center = (halfWidth + bottomDoorHalfWidth) * 0.5f;
            AddWall(house, "Parede inferior esquerda", new Vector2(-center, -halfHeight), new Vector2(segmentWidth, 0.24f));
            AddWall(house, "Parede inferior direita", new Vector2(center, -halfHeight), new Vector2(segmentWidth, 0.24f));
        }

        private static void AddWall(Transform house, string wallName, Vector2 localPosition, Vector2 size)
        {
            var wall = new GameObject(wallName);
            wall.transform.SetParent(house, false);
            wall.transform.localPosition = localPosition;
            wall.AddComponent<WallObstacle>();
            BoxCollider2D collider = wall.AddComponent<BoxCollider2D>();
            collider.size = size;
        }
    }
}
