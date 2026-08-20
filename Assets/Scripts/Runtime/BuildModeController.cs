using ProjetoJogosDigitais.Presentation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ProjetoJogosDigitais.Gameplay
{
    public sealed class BuildModeController : MonoBehaviour
    {
        [SerializeField] private Camera worldCamera;
        [SerializeField] private GameSession session;
        [SerializeField] private GameClock clock;
        [SerializeField] private GameObject buildPanel;
        [SerializeField] private Text selectionText;

        private FurnitureItem selected;
        private FurnitureItem activePlacement;
        private Vector3 originalPosition;
        private bool purchasePending;
        private float speedBeforeBuild = 1f;

        public static BuildModeController Current { get; private set; }
        public bool IsOpen => buildPanel != null && buildPanel.activeSelf;

        public void Configure(
            Camera camera,
            GameSession gameSession,
            GameClock gameClock,
            GameObject panel,
            Text selectedLabel)
        {
            worldCamera = camera;
            session = gameSession;
            clock = gameClock;
            buildPanel = panel;
            selectionText = selectedLabel;
            buildPanel.SetActive(false);
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

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.bKey.wasPressedThisFrame)
            {
                Toggle();
            }

            if (!IsOpen)
            {
                return;
            }

            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            {
                if (activePlacement != null)
                {
                    CancelPlacement();
                }
                else
                {
                    Close();
                }
                return;
            }

            if (keyboard != null && keyboard.deleteKey.wasPressedThisFrame)
            {
                SellSelected();
            }

            Mouse mouse = Mouse.current;
            if (mouse == null)
            {
                return;
            }

            Vector3 world = PointerWorld(mouse.position.ReadValue());
            if (activePlacement != null)
            {
                activePlacement.transform.position = Snap(world);
                bool valid = NeighborhoodLayout.IsInsideHouse(activePlacement.transform.position);
                activePlacement.SetHighlighted(valid);

                if (mouse.rightButton.wasPressedThisFrame)
                {
                    CancelPlacement();
                }
                else if (mouse.leftButton.wasPressedThisFrame &&
                         !IsPointerOverUi() &&
                         valid)
                {
                    ConfirmPlacement();
                }
                return;
            }

            if (mouse.leftButton.wasPressedThisFrame && !IsPointerOverUi())
            {
                Collider2D hit = Physics2D.OverlapPoint(world);
                Select(hit != null ? hit.GetComponentInParent<FurnitureItem>() : null);
            }
        }

        public void Toggle()
        {
            if (IsOpen)
            {
                Close();
                return;
            }

            speedBeforeBuild = Mathf.Max(1f, clock.Speed);
            clock.SetSpeed(0f);
            buildPanel.SetActive(true);
            UpdateSelectionLabel("Selecione um móvel ou compre pelo catálogo");
        }

        public void Close()
        {
            CancelPlacement();
            Select(null);
            buildPanel.SetActive(false);
            clock.SetSpeed(speedBeforeBuild);
        }

        public void BeginPurchase(FurnitureType type)
        {
            int price = FurnitureItem.PriceFor(type);
            if (session.Money < price)
            {
                UpdateSelectionLabel($"Dinheiro insuficiente: §{price}");
                return;
            }

            CancelPlacement();
            Vector2 pointer = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
            activePlacement = FurnitureFactory.Create(type, PointerWorld(pointer), true);
            purchasePending = true;
            Select(activePlacement);
            UpdateSelectionLabel($"Posicione {activePlacement.ActionLabel} — custo §{activePlacement.PurchasePrice}");
        }

        public void BeginMoveSelected()
        {
            if (selected == null)
            {
                UpdateSelectionLabel("Selecione primeiro um móvel da casa");
                return;
            }

            activePlacement = selected;
            originalPosition = selected.transform.position;
            purchasePending = false;
            UpdateSelectionLabel("Mova o móvel e clique para confirmar");
        }

        public void SellSelected()
        {
            if (selected == null || activePlacement != null)
            {
                UpdateSelectionLabel("Selecione um móvel para vender");
                return;
            }

            int value = selected.SellPrice;
            session.AddMoney(value);
            Destroy(selected.gameObject);
            selected = null;
            UpdateSelectionLabel($"Móvel vendido por §{value}");
        }

        private void ConfirmPlacement()
        {
            if (purchasePending && !session.TrySpend(activePlacement.PurchasePrice))
            {
                UpdateSelectionLabel("Dinheiro insuficiente");
                return;
            }

            activePlacement.SetHighlighted(false);
            selected = activePlacement;
            activePlacement = null;
            purchasePending = false;
            UpdateSelectionLabel("Móvel posicionado. Selecione outro ou feche o modo.");
        }

        private void CancelPlacement()
        {
            if (activePlacement == null)
            {
                return;
            }

            if (purchasePending)
            {
                Destroy(activePlacement.gameObject);
                selected = null;
            }
            else
            {
                activePlacement.transform.position = originalPosition;
                activePlacement.SetHighlighted(false);
            }

            activePlacement = null;
            purchasePending = false;
            UpdateSelectionLabel("Posicionamento cancelado");
        }

        private void Select(FurnitureItem item)
        {
            if (selected != null && selected != activePlacement)
            {
                selected.SetHighlighted(false);
            }

            selected = item;
            if (selected != null)
            {
                selected.SetHighlighted(true);
                UpdateSelectionLabel($"{selected.Type} — mover ou vender por §{selected.SellPrice}");
            }
        }

        private Vector3 PointerWorld(Vector2 pointer)
        {
            Vector3 world = worldCamera.ScreenToWorldPoint(new Vector3(pointer.x, pointer.y, -worldCamera.transform.position.z));
            world.z = 0f;
            return world;
        }

        private static Vector3 Snap(Vector3 world)
        {
            return new Vector3(
                Mathf.Round(world.x * 2f) / 2f,
                Mathf.Round(world.y * 2f) / 2f,
                0f);
        }

        private static bool IsPointerOverUi()
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }

        private void UpdateSelectionLabel(string value)
        {
            if (selectionText != null)
            {
                selectionText.text = value;
            }
        }
    }
}
