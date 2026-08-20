using System;
using System.Collections;
using ProjetoJogosDigitais.Presentation;
using UnityEngine;

namespace ProjetoJogosDigitais.Gameplay
{
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class FurnitureItem : MonoBehaviour, IWorldInteractable
    {
        [SerializeField] private string saveId;
        [SerializeField] private FurnitureType furnitureType;
        [SerializeField] private bool purchasedByPlayer;
        [SerializeField] private Vector2 interactionOffset = new(-0.8f, -0.2f);
        [SerializeField] private Sprite[] visualFrames;

        private bool inUse;
        private GameObject generatedVisual;
        private SpriteRenderer visualRenderer;

        public string SaveId => saveId;
        public FurnitureType Type => furnitureType;
        public bool PurchasedByPlayer => purchasedByPlayer;
        public bool IsAvailable => !inUse;
        public Vector3 InteractionPosition => transform.position + (Vector3)interactionOffset;
        public NeedType SatisfiesNeed => furnitureType switch
        {
            FurnitureType.Fridge => NeedType.Hunger,
            FurnitureType.Bed => NeedType.Energy,
            FurnitureType.Shower => NeedType.Hygiene,
            FurnitureType.Armchair => NeedType.Energy,
            _ => NeedType.Energy
        };
        public string ActionLabel => furnitureType switch
        {
            FurnitureType.Fridge => "Preparar uma refeição",
            FurnitureType.Bed => "Dormir",
            FurnitureType.Shower => "Tomar banho",
            FurnitureType.Armchair => "Assistir TV",
            _ => "Usar móvel"
        };
        public int PurchasePrice => PriceFor(furnitureType);
        public int SellPrice => Mathf.RoundToInt(PurchasePrice * 0.55f);

        public static int PriceFor(FurnitureType type) => type switch
        {
            FurnitureType.Fridge => 650,
            FurnitureType.Bed => 480,
            FurnitureType.Shower => 420,
            FurnitureType.Armchair => 220,
            _ => 100
        };

        private void Awake()
        {
            BuildAppearance();
        }

        public void Configure(FurnitureType type, string id, bool purchased, Sprite[] frames = null)
        {
            furnitureType = type;
            saveId = string.IsNullOrWhiteSpace(id) ? Guid.NewGuid().ToString("N") : id;
            purchasedByPlayer = purchased;
            visualFrames = frames;
            interactionOffset = type == FurnitureType.Bed
                ? new Vector2(-1.25f, -0.1f)
                : new Vector2(-0.8f, -0.15f);

            BoxCollider2D collider = GetComponent<BoxCollider2D>();
            Vector2 size = GetVisualSize(type, visualFrames);
            collider.size = size;
            collider.offset = visualFrames != null && visualFrames.Length > 0 && visualFrames[0] != null
                ? visualFrames[0].bounds.center
                : Vector2.up * size.y * 0.5f;
        }

        public void BeginInteraction(SimController actor, Action completed)
        {
            if (!inUse)
            {
                StartCoroutine(InteractionRoutine(actor, completed));
            }
            else
            {
                completed?.Invoke();
            }
        }

        public void SetHighlighted(bool highlighted)
        {
            foreach (SpriteRenderer renderer in GetComponentsInChildren<SpriteRenderer>())
            {
                renderer.color = highlighted
                    ? new Color(1f, 0.88f, 0.35f, 0.8f)
                    : visualFrames != null && visualFrames.Length > 0
                        ? Color.white
                        : GetColor(furnitureType);
            }
        }

        private IEnumerator InteractionRoutine(SimController actor, Action completed)
        {
            inUse = true;
            float startedAt = Time.realtimeSinceStartup;
            float duration = furnitureType switch
            {
                FurnitureType.Bed => 2.8f,
                FurnitureType.Shower => 2.2f,
                _ => 1.5f
            };

            while (Time.realtimeSinceStartup - startedAt < duration)
            {
                float elapsed = Time.realtimeSinceStartup - startedAt;
                if (visualRenderer != null && visualFrames != null && visualFrames.Length > 1)
                {
                    int frame = Mathf.FloorToInt(elapsed * 7f) % visualFrames.Length;
                    visualRenderer.sprite = visualFrames[frame];
                }
                else
                {
                    float pulse = 0.94f + Mathf.Sin(Time.realtimeSinceStartup * 8f) * 0.04f;
                    generatedVisual.transform.localScale = Vector3.one * pulse;
                }
                yield return null;
            }

            generatedVisual.transform.localScale = Vector3.one;
            if (visualRenderer != null && visualFrames != null && visualFrames.Length > 0)
            {
                visualRenderer.sprite = visualFrames[0];
            }
            float amount = furnitureType switch
            {
                FurnitureType.Fridge => 0.48f,
                FurnitureType.Bed => 0.58f,
                FurnitureType.Shower => 0.62f,
                FurnitureType.Armchair => 0.24f,
                _ => 0.25f
            };
            actor.Needs.Restore(SatisfiesNeed, amount);
            inUse = false;
            completed?.Invoke();
        }

        private void BuildAppearance()
        {
            if (generatedVisual != null)
            {
                Destroy(generatedVisual);
            }

            if (visualFrames != null && visualFrames.Length > 0 && visualFrames[0] != null)
            {
                generatedVisual = new GameObject($"Sprite {furnitureType}");
                generatedVisual.transform.SetParent(transform, false);
                visualRenderer = generatedVisual.AddComponent<SpriteRenderer>();
                visualRenderer.sprite = visualFrames[0];
                visualRenderer.sortingOrder = 12;
                return;
            }

            Vector2 size = GetSize(furnitureType);
            generatedVisual = PixelShapes.CreateBlock(
                transform,
                $"Visual {furnitureType}",
                new Vector3(size.x * 0.5f, size.y * 0.5f, 0f),
                size,
                GetColor(furnitureType),
                10);

            Color detailColor = Color.Lerp(GetColor(furnitureType), Color.white, 0.35f);
            PixelShapes.CreateBlock(
                generatedVisual.transform,
                "Detalhe",
                new Vector3(0f, 0.24f, 0f),
                new Vector2(0.72f, 0.13f),
                detailColor,
                11);
        }

        private static Vector2 GetSize(FurnitureType type)
        {
            return type switch
            {
                FurnitureType.Fridge => new Vector2(1.05f, 1.8f),
                FurnitureType.Bed => new Vector2(2.1f, 1.15f),
                FurnitureType.Shower => new Vector2(1.15f, 1.75f),
                FurnitureType.Armchair => new Vector2(1.25f, 1.05f),
                _ => Vector2.one
            };
        }

        private static Vector2 GetVisualSize(FurnitureType type, Sprite[] frames)
        {
            return frames != null && frames.Length > 0 && frames[0] != null
                ? frames[0].bounds.size
                : GetSize(type);
        }

        private static Color GetColor(FurnitureType type)
        {
            return type switch
            {
                FurnitureType.Fridge => new Color(0.72f, 0.80f, 0.91f),
                FurnitureType.Bed => new Color(0.48f, 0.62f, 0.82f),
                FurnitureType.Shower => new Color(0.42f, 0.78f, 0.82f),
                FurnitureType.Armchair => new Color(0.77f, 0.47f, 0.36f),
                _ => Color.gray
            };
        }
    }

    public static class FurnitureFactory
    {
        public static FurnitureItem Create(
            FurnitureType type,
            Vector3 position,
            bool purchasedByPlayer,
            string saveId = null,
            Sprite[] visualFrames = null)
        {
            var furnitureObject = new GameObject(type.ToString());
            furnitureObject.SetActive(false);
            furnitureObject.transform.position = position;
            furnitureObject.AddComponent<BoxCollider2D>();
            FurnitureItem item = furnitureObject.AddComponent<FurnitureItem>();
            Sprite[] frames = visualFrames ?? FurnitureVisualCatalog.Current?.Get(type);
            item.Configure(type, saveId, purchasedByPlayer, frames);
            furnitureObject.SetActive(true);
            return item;
        }
    }
}
