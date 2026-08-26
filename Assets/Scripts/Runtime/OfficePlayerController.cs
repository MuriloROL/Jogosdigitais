using System;
using ProjetoJogosDigitais.Presentation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace ProjetoJogosDigitais.Gameplay
{
    [RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
    public sealed class OfficePlayerController : MonoBehaviour
    {
        [SerializeField] private Camera worldCamera;
        [SerializeField, Min(0.1f)] private float movementSpeed = 4.2f;
        [SerializeField] private Vector2 worldMin = new(-10f, -5.5f);
        [SerializeField] private Vector2 worldMax = new(10f, 5.5f);

        private CharacterWalkAnimator walkAnimator;
        private OfficeStation targetStation;
        private Vector3 destination;
        private bool hasDestination;
        private bool interacting;
        private string status = "Passe o mouse e clique em um botão de atividade.";
        private GameObject destinationMarker;
        private OfficeStation hoveredStation;

        public string Status => status;
        public bool IsIdle => !hasDestination && !interacting;
        public bool IsInteracting => interacting;
        public OfficeStation HoveredStation => hoveredStation;
        public event Action<string> StatusChanged;

        public void Configure(Camera camera, Vector2 minimum, Vector2 maximum)
        {
            worldCamera = camera;
            worldMin = minimum;
            worldMax = maximum;
        }

        private void Awake()
        {
            worldCamera ??= Camera.main;
            walkAnimator = GetComponent<CharacterWalkAnimator>();
            CreateDestinationMarker();
        }

        private void Update()
        {
            ReadPointer();
            TickMovement();
        }

        private void OnDisable()
        {
            SetHoveredStation(null);
        }

        public void SelectStation(OfficeStation station)
        {
            if (station == null || interacting)
            {
                return;
            }

            targetStation = station;
            destination = ClampToOffice(station.InteractionPosition);
            hasDestination = true;
            SetHoveredStation(null);
            destinationMarker.transform.position = destination + Vector3.down * 0.2f;
            destinationMarker.SetActive(true);
            SetStatus($"Destino confirmado • {station.ActionLabel}");
        }

        public void MoveTo(Vector3 position)
        {
            if (interacting)
            {
                return;
            }

            targetStation = null;
            destination = ClampToOffice(position);
            hasDestination = true;
            destinationMarker.transform.position = destination + Vector3.down * 0.2f;
            destinationMarker.SetActive(true);
            SetStatus("Caminhando pelo escritório");
        }

        public void BeginInteraction(OfficeStation station)
        {
            interacting = true;
            SetStatus(station.ActionLabel);
        }

        public void CompleteInteraction(string result)
        {
            interacting = false;
            targetStation = null;
            SetStatus(result);
        }

        public void ShowStatus(string value)
        {
            SetStatus(value);
        }

        private void ReadPointer()
        {
            Mouse mouse = Mouse.current;
            if (mouse == null)
            {
                return;
            }

            if (interacting || EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                SetHoveredStation(null);
                return;
            }

            Vector2 pointer = mouse.position.ReadValue();
            Vector3 world = worldCamera.ScreenToWorldPoint(new Vector3(pointer.x, pointer.y, -worldCamera.transform.position.z));
            world.z = 0f;

            OfficeStation station = FindStationAt(world);
            SetHoveredStation(station);
            if (!mouse.leftButton.wasPressedThisFrame)
            {
                return;
            }

            if (station != null)
            {
                SelectStation(station);
            }
            else
            {
                MoveTo(world);
            }
        }

        private static OfficeStation FindStationAt(Vector2 world)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(world, 0.22f);
            OfficeStation nearest = null;
            float nearestDistance = float.PositiveInfinity;
            for (int index = 0; index < hits.Length; index++)
            {
                OfficeStation station = hits[index].GetComponentInParent<OfficeStation>();
                if (station == null)
                {
                    continue;
                }

                float distance = Vector2.SqrMagnitude((Vector2)station.InteractionPosition - world);
                if (distance < nearestDistance)
                {
                    nearest = station;
                    nearestDistance = distance;
                }
            }

            return nearest;
        }

        private void SetHoveredStation(OfficeStation station)
        {
            if (hoveredStation == station)
            {
                return;
            }

            hoveredStation?.SetPointerFocus(false);
            hoveredStation = station;
            hoveredStation?.SetPointerFocus(true);
            if (!hasDestination && !interacting)
            {
                SetStatus(station == null
                    ? "Clique em um botão de atividade no escritório."
                    : $"Clique para confirmar • {station.ActionLabel}");
            }

        }

        private void TickMovement()
        {
            if (!hasDestination)
            {
                walkAnimator?.SetMotion(Vector2.zero);
                return;
            }

            Vector2 before = transform.position;
            Vector2 remaining = (Vector2)destination - before;
            if (remaining.magnitude > 0.06f)
            {
                Vector2 movement = Vector2.ClampMagnitude(remaining, movementSpeed * Time.deltaTime);
                transform.position += (Vector3)movement;
                walkAnimator?.SetMotion(movement);
                return;
            }

            transform.position = destination;
            walkAnimator?.SetMotion(Vector2.zero);
            hasDestination = false;
            destinationMarker.SetActive(false);

            OfficeStation arrivedStation = targetStation;
            if (arrivedStation != null)
            {
                arrivedStation.Activate(this);
            }
            else
            {
                SetStatus("Escolha uma estação de trabalho.");
            }
        }

        private Vector3 ClampToOffice(Vector3 value)
        {
            return new Vector3(
                Mathf.Clamp(value.x, worldMin.x, worldMax.x),
                Mathf.Clamp(value.y, worldMin.y, worldMax.y),
                0f);
        }

        private void CreateDestinationMarker()
        {
            destinationMarker = new GameObject("Destino confirmado");
            GameObject halo = PixelShapes.CreateBlock(
                destinationMarker.transform,
                "Alvo do destino",
                Vector3.zero,
                new Vector2(0.72f, 0.72f),
                new Color(0.28f, 0.92f, 0.85f, 0.42f),
                30);
            halo.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
            GameObject core = PixelShapes.CreateBlock(
                destinationMarker.transform,
                "Centro do destino",
                Vector3.zero,
                new Vector2(0.24f, 0.24f),
                new Color(0.78f, 1f, 0.96f, 0.98f),
                31);
            core.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
            destinationMarker.SetActive(false);
        }

        private void SetStatus(string value)
        {
            if (status == value)
            {
                return;
            }
            status = value;
            StatusChanged?.Invoke(value);
        }
    }
}
