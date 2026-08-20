using System.Linq;
using UnityEngine;

namespace ProjetoJogosDigitais.Gameplay
{
    [RequireComponent(typeof(SimController))]
    public sealed class NpcBrain : MonoBehaviour
    {
        [SerializeField] private float decisionInterval = 1.5f;
        [SerializeField] private Vector2 homeCenter;
        [SerializeField] private Vector2 homeExtents = new(6f, 4f);

        private SimController controller;
        private float nextDecisionAt;

        public void Configure(Vector2 residenceCenter, Vector2 residenceExtents)
        {
            homeCenter = residenceCenter;
            homeExtents = residenceExtents;
        }

        private void Awake()
        {
            controller = GetComponent<SimController>();
        }

        private void Update()
        {
            if (!controller.IsIdle || Time.unscaledTime < nextDecisionAt)
            {
                return;
            }

            nextDecisionAt = Time.unscaledTime + decisionInterval + Random.Range(0f, 1.2f);
            NeedType lowest = controller.Needs.LowestNeed();
            if (controller.Needs.Get(lowest).Current < 0.55f)
            {
                IWorldInteractable best = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
                    .OfType<IWorldInteractable>()
                    .Where(item => item.SatisfiesNeed == lowest && item.IsAvailable)
                    .Where(item => item is not SocialInteractable social || social.gameObject != gameObject)
                    .Where(item => IsAtHome(item.InteractionPosition))
                    .OrderBy(item => Vector2.Distance(transform.position, item.InteractionPosition))
                    .FirstOrDefault();
                if (best != null)
                {
                    controller.QueueInteraction(best);
                    return;
                }
            }

            Vector2 wander = homeCenter + new Vector2(
                Random.Range(-homeExtents.x * 0.62f, homeExtents.x * 0.62f),
                Random.Range(-homeExtents.y * 0.62f, homeExtents.y * 0.62f));
            controller.MoveTo(wander);
        }

        private bool IsAtHome(Vector2 position)
        {
            Vector2 offset = position - homeCenter;
            return Mathf.Abs(offset.x) <= homeExtents.x && Mathf.Abs(offset.y) <= homeExtents.y;
        }
    }
}
