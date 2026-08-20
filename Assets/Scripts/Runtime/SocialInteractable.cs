using System;
using System.Collections;
using UnityEngine;

namespace ProjetoJogosDigitais.Gameplay
{
    [RequireComponent(typeof(SimController))]
    public sealed class SocialInteractable : MonoBehaviour, IWorldInteractable
    {
        private SimController target;
        private bool inUse;

        public string ActionLabel => target == null ? "Conversar" : $"Conversar com {target.SimName}";
        public Vector3 InteractionPosition => transform.position + Vector3.left * 0.8f;
        public NeedType SatisfiesNeed => NeedType.Social;
        public bool IsAvailable => !inUse;

        private void Awake()
        {
            target = GetComponent<SimController>();
        }

        public void BeginInteraction(SimController actor, Action completed)
        {
            if (actor == target || inUse)
            {
                completed?.Invoke();
                return;
            }

            StartCoroutine(TalkRoutine(actor, completed));
        }

        private IEnumerator TalkRoutine(SimController actor, Action completed)
        {
            inUse = true;
            float startedAt = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - startedAt < 2f)
            {
                yield return null;
            }

            actor.Needs.Restore(NeedType.Social, 0.38f);
            target.Needs.Restore(NeedType.Social, 0.16f);
            GameSession.Current?.RegisterConversation(actor, target);
            inUse = false;
            completed?.Invoke();
        }
    }
}
