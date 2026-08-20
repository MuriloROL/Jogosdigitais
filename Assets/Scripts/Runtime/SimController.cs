using System;
using System.Collections.Generic;
using System.Linq;
using ProjetoJogosDigitais.Presentation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace ProjetoJogosDigitais.Gameplay
{
    [RequireComponent(typeof(SimNeeds), typeof(SpriteRenderer), typeof(Collider2D))]
    public sealed class SimController : MonoBehaviour
    {
        private sealed class QueuedAction
        {
            public Vector3 Position;
            public IWorldInteractable Target;
            public string Label;
            public bool InteractionStarted;
        }

        [SerializeField] private string simName = "Alex";
        [SerializeField] private bool playerControlled;
        [SerializeField] private Camera worldCamera;
        [SerializeField, Min(0.1f)] private float movementSpeed = 3.4f;
        [SerializeField] private Vector2 worldMin = new(-17f, -12f);
        [SerializeField] private Vector2 worldMax = new(17f, 12f);

        private readonly Queue<QueuedAction> queuedActions = new();
        private SimNeeds needs;
        private SpriteRenderer spriteRenderer;
        private CharacterWalkAnimator walkAnimator;
        private Collider2D movementCollider;
        private QueuedAction currentAction;
        private GameObject destinationMarker;
        private string status = "Aguardando uma ação";
        private float blockedTime;

        private const float CollisionSkin = 0.025f;
        private const float BlockedActionTimeout = 1.25f;

        public string SimName => simName;
        public bool IsPlayer => playerControlled;
        public SimNeeds Needs => needs;
        public string Status => status;
        public bool IsMoving => currentAction != null &&
                                !currentAction.InteractionStarted &&
                                Vector2.Distance(transform.position, CurrentDestination()) > 0.05f;
        public bool IsIdle => currentAction == null && queuedActions.Count == 0;
        public int QueueCount => queuedActions.Count + (currentAction != null ? 1 : 0);

        public event Action<string> StatusChanged;

        public void Configure(
            string displayName,
            bool isPlayer,
            Camera camera,
            Vector2 minimum,
            Vector2 maximum)
        {
            simName = displayName;
            playerControlled = isPlayer;
            worldCamera = camera;
            worldMin = minimum;
            worldMax = maximum;
        }

        private void Awake()
        {
            needs = GetComponent<SimNeeds>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            walkAnimator = GetComponent<CharacterWalkAnimator>();
            movementCollider = GetComponent<Collider2D>();
            worldCamera ??= Camera.main;
            CreateDestinationMarker();
        }

        private void Update()
        {
            if (playerControlled)
            {
                ReadPointer();
            }

            TickCurrentAction();
        }

        public void QueueInteraction(IWorldInteractable target, bool append = false)
        {
            if (target == null || ReferenceEquals(target, GetComponent<SocialInteractable>()))
            {
                return;
            }

            if (!append)
            {
                ClearQueue();
            }

            queuedActions.Enqueue(new QueuedAction
            {
                Target = target,
                Label = target.ActionLabel
            });
            StartNextActionIfNeeded();
        }

        public void MoveTo(Vector3 worldPosition, bool append = false)
        {
            if (!append)
            {
                ClearQueue();
            }

            queuedActions.Enqueue(new QueuedAction
            {
                Position = ClampToWorld(worldPosition),
                Label = "Caminhar"
            });
            StartNextActionIfNeeded();
        }

        public void ClearQueue()
        {
            queuedActions.Clear();
            if (currentAction == null || !currentAction.InteractionStarted)
            {
                currentAction = null;
            }

            if (currentAction == null)
            {
                destinationMarker.SetActive(false);
                SetStatus("Aguardando uma ação");
            }
        }

        public string QueueSummary()
        {
            IEnumerable<string> labels = currentAction != null
                ? new[] { currentAction.Label }.Concat(queuedActions.Select(action => action.Label))
                : queuedActions.Select(action => action.Label);
            string summary = string.Join("  →  ", labels.Take(4));
            return string.IsNullOrEmpty(summary) ? "Fila vazia" : summary;
        }

        public void SetPositionInstant(Vector3 position)
        {
            transform.position = ClampToWorld(position);
            Physics2D.SyncTransforms();
            ClearQueue();
        }

        private void ReadPointer()
        {
            if (BuildModeController.Current != null && BuildModeController.Current.IsOpen)
            {
                return;
            }

            Mouse mouse = Mouse.current;
            if (mouse == null || !mouse.leftButton.wasPressedThisFrame)
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
            bool append = Keyboard.current != null &&
                          (Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed);

            Collider2D hit = Physics2D.OverlapPoint(world);
            IWorldInteractable interactable = hit == null
                ? null
                : hit.GetComponentsInParent<MonoBehaviour>().OfType<IWorldInteractable>().FirstOrDefault();

            if (interactable != null)
            {
                QueueInteraction(interactable, append);
            }
            else
            {
                MoveTo(world, append);
            }
        }

        private void TickCurrentAction()
        {
            StartNextActionIfNeeded();
            if (currentAction == null || currentAction.InteractionStarted)
            {
                walkAnimator?.SetMotion(Vector2.zero);
                return;
            }

            Vector3 destination = CurrentDestination();
            if (Vector2.Distance(transform.position, destination) > 0.05f)
            {
                Vector2 before = transform.position;
                Vector2 desiredDelta = ((Vector2)destination - before).normalized * movementSpeed * Time.deltaTime;
                if (desiredDelta.magnitude > Vector2.Distance(before, destination))
                {
                    desiredDelta = (Vector2)destination - before;
                }

                Vector2 actualDelta = MoveWithWallCollisions(desiredDelta);
                walkAnimator?.SetMotion(actualDelta);
                blockedTime = actualDelta.sqrMagnitude < 0.000001f ? blockedTime + Time.deltaTime : 0f;
                if (blockedTime >= BlockedActionTimeout)
                {
                    destinationMarker.SetActive(false);
                    SetStatus("Caminho bloqueado por uma parede");
                    CompleteCurrentAction();
                }

                return;
            }

            blockedTime = 0f;
            walkAnimator?.SetMotion(Vector2.zero);
            destinationMarker.SetActive(false);
            if (currentAction.Target == null)
            {
                CompleteCurrentAction();
                return;
            }

            if (!currentAction.Target.IsAvailable)
            {
                SetStatus("Objeto ocupado; ação cancelada");
                CompleteCurrentAction();
                return;
            }

            QueuedAction startedAction = currentAction;
            startedAction.InteractionStarted = true;
            SetStatus(startedAction.Label);
            startedAction.Target.BeginInteraction(this, () =>
            {
                if (ReferenceEquals(currentAction, startedAction))
                {
                    CompleteCurrentAction();
                }
            });
        }

        private void StartNextActionIfNeeded()
        {
            if (currentAction != null || queuedActions.Count == 0)
            {
                return;
            }

            currentAction = queuedActions.Dequeue();
            blockedTime = 0f;
            Vector3 destination = CurrentDestination();
            destinationMarker.transform.position = new Vector3(destination.x, destination.y - 0.3f, 0f);
            destinationMarker.SetActive(true);
            SetStatus(currentAction.Target == null ? "Caminhando" : $"Indo: {currentAction.Label}");
        }

        private void CompleteCurrentAction()
        {
            currentAction = null;
            blockedTime = 0f;
            SetStatus(queuedActions.Count > 0 ? "Preparando próxima ação" : "Aguardando uma ação");
            StartNextActionIfNeeded();
        }

        private Vector3 CurrentDestination()
        {
            if (currentAction == null)
            {
                return transform.position;
            }

            return ClampToWorld(currentAction.Target?.InteractionPosition ?? currentAction.Position);
        }

        private Vector3 ClampToWorld(Vector3 value)
        {
            return new Vector3(
                Mathf.Clamp(value.x, worldMin.x, worldMax.x),
                Mathf.Clamp(value.y, worldMin.y, worldMax.y),
                0f);
        }

        private Vector2 MoveWithWallCollisions(Vector2 delta)
        {
            Vector2 moved = Vector2.zero;
            moved += MoveAxis(new Vector2(delta.x, 0f));
            moved += MoveAxis(new Vector2(0f, delta.y));
            return moved;
        }

        private Vector2 MoveAxis(Vector2 delta)
        {
            float distance = delta.magnitude;
            if (distance <= 0.000001f)
            {
                return Vector2.zero;
            }

            Vector2 direction = delta / distance;
            Bounds bounds = movementCollider.bounds;
            RaycastHit2D[] hits = Physics2D.CapsuleCastAll(
                bounds.center,
                bounds.size,
                CapsuleDirection2D.Vertical,
                0f,
                direction,
                distance + CollisionSkin);
            float allowedDistance = distance;
            for (int index = 0; index < hits.Length; index++)
            {
                RaycastHit2D hit = hits[index];
                if (hit.collider == null || hit.collider.GetComponentInParent<WallObstacle>() == null)
                {
                    continue;
                }

                allowedDistance = Mathf.Min(allowedDistance, Mathf.Max(0f, hit.distance - CollisionSkin));
            }

            Vector2 movement = direction * allowedDistance;
            transform.position += (Vector3)movement;
            Physics2D.SyncTransforms();
            return movement;
        }

        private void CreateDestinationMarker()
        {
            destinationMarker = PixelShapes.CreateBlock(
                null,
                $"Destino de {simName}",
                Vector3.zero,
                new Vector2(0.32f, 0.12f),
                playerControlled
                    ? new Color(0.35f, 0.95f, 0.84f, 0.85f)
                    : new Color(0.95f, 0.70f, 0.35f, 0.5f),
                5);
            destinationMarker.SetActive(false);
        }

        private void SetStatus(string value)
        {
            status = value;
            StatusChanged?.Invoke(status);
        }
    }
}
