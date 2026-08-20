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
        private string status = "Escolha onde começar o expediente.";
        private GameObject destinationMarker;

        public string Status => status;
        public bool IsIdle => !hasDestination && !interacting;
        public bool IsInteracting => interacting;
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

        public void SelectStation(OfficeStation station)
        {
            if (station == null || interacting)
            {
                return;
            }

            targetStation = station;
            destination = ClampToOffice(station.InteractionPosition);
            hasDestination = true;
            destinationMarker.transform.position = destination + Vector3.down * 0.2f;
            destinationMarker.SetActive(true);
            SetStatus($"Indo: {station.ActionLabel}");
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
            if (interacting || mouse == null || !mouse.leftButton.wasPressedThisFrame)
            {
                return;
            }

            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            Vector2 pointer = mouse.position.ReadValue();
            Vector3 world = worldCamera.ScreenToWorldPoint(new Vector3(pointer.x, pointer.y, -worldCamera.transform.position.z));
            world.z = 0f;

            Collider2D[] hits = Physics2D.OverlapPointAll(world);
            for (int index = 0; index < hits.Length; index++)
            {
                OfficeStation station = hits[index].GetComponentInParent<OfficeStation>();
                if (station != null)
                {
                    SelectStation(station);
                    return;
                }
            }

            MoveTo(world);
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
            destinationMarker = PixelShapes.CreateBlock(
                null,
                "Destino no escritório",
                Vector3.zero,
                new Vector2(0.42f, 0.12f),
                new Color(0.28f, 0.92f, 0.85f, 0.9f),
                30);
            destinationMarker.SetActive(false);
        }

        private void SetStatus(string value)
        {
            status = value;
            StatusChanged?.Invoke(value);
        }
    }
}
