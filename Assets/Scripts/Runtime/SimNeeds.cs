using System;
using UnityEngine;

namespace ProjetoJogosDigitais.Gameplay
{
    public sealed class SimNeeds : MonoBehaviour
    {
        [SerializeField] private NeedStat hunger = new(0.78f, 0.0035f);
        [SerializeField] private NeedStat energy = new(0.82f, 0.0024f);
        [SerializeField] private NeedStat hygiene = new(0.76f, 0.0028f);
        [SerializeField] private NeedStat social = new(0.70f, 0.0021f);

        public NeedStat Hunger => hunger;
        public NeedStat Energy => energy;
        public NeedStat Hygiene => hygiene;
        public NeedStat Social => social;
        public event Action NeedsChanged;

        private void Update()
        {
            hunger.Tick(Time.deltaTime);
            energy.Tick(Time.deltaTime);
            hygiene.Tick(Time.deltaTime);
            social.Tick(Time.deltaTime);
            NeedsChanged?.Invoke();
        }

        public NeedStat Get(NeedType type)
        {
            return type switch
            {
                NeedType.Hunger => hunger,
                NeedType.Energy => energy,
                NeedType.Hygiene => hygiene,
                NeedType.Social => social,
                _ => hunger
            };
        }

        public NeedType LowestNeed()
        {
            NeedType lowest = NeedType.Hunger;
            float value = hunger.Current;
            foreach (NeedType type in Enum.GetValues(typeof(NeedType)))
            {
                if (Get(type).Current < value)
                {
                    lowest = type;
                    value = Get(type).Current;
                }
            }

            return lowest;
        }

        public void Restore(NeedType type, float amount)
        {
            Get(type).Restore(amount);
            NeedsChanged?.Invoke();
        }

        public void Eat(float amount)
        {
            Restore(NeedType.Hunger, amount);
        }

        public float[] CaptureValues()
        {
            return new[] { hunger.Current, energy.Current, hygiene.Current, social.Current };
        }

        public void RestoreValues(float[] values)
        {
            if (values == null || values.Length < 4)
            {
                return;
            }

            hunger.SetCurrent(values[0]);
            energy.SetCurrent(values[1]);
            hygiene.SetCurrent(values[2]);
            social.SetCurrent(values[3]);
            NeedsChanged?.Invoke();
        }
    }
}
