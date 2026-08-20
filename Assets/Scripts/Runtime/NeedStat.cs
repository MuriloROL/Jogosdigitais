using System;
using UnityEngine;

namespace ProjetoJogosDigitais.Gameplay
{
    [Serializable]
    public sealed class NeedStat
    {
        [SerializeField, Range(0f, 1f)] private float current = 0.72f;
        [SerializeField, Min(0f)] private float decayPerSecond = 0.012f;

        public float Current => current;
        public float DecayPerSecond => decayPerSecond;

        public NeedStat()
        {
        }

        public NeedStat(float initialValue, float decayRate)
        {
            current = Mathf.Clamp01(initialValue);
            decayPerSecond = Mathf.Max(0f, decayRate);
        }

        public void Tick(float deltaTime)
        {
            current = Mathf.Clamp01(current - decayPerSecond * Mathf.Max(0f, deltaTime));
        }

        public void Restore(float amount)
        {
            current = Mathf.Clamp01(current + Mathf.Max(0f, amount));
        }

        public void SetCurrent(float value)
        {
            current = Mathf.Clamp01(value);
        }
    }
}
