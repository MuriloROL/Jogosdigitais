using System;
using UnityEngine;

namespace ProjetoJogosDigitais.Gameplay
{
    public sealed class GameClock : MonoBehaviour
    {
        [SerializeField] private float minutesSinceMidnight = 8f * 60f;
        [SerializeField] private int day = 1;
        [SerializeField] private float speed = 1f;
        [SerializeField] private float gameMinutesPerRealSecond = 1.5f;

        public int Day => day;
        public float MinutesSinceMidnight => minutesSinceMidnight;
        public float Speed => speed;
        public string DisplayTime => $"Dia {day}  {Mathf.FloorToInt(minutesSinceMidnight / 60f):00}:{Mathf.FloorToInt(minutesSinceMidnight % 60f):00}";
        public event Action Changed;

        private void Awake()
        {
            ApplySpeed();
        }

        private void Update()
        {
            if (speed <= 0f)
            {
                return;
            }

            minutesSinceMidnight += Time.unscaledDeltaTime * gameMinutesPerRealSecond * speed;
            while (minutesSinceMidnight >= 24f * 60f)
            {
                minutesSinceMidnight -= 24f * 60f;
                day++;
            }
            Changed?.Invoke();
        }

        public void SetSpeed(float value)
        {
            speed = Mathf.Clamp(value, 0f, 3f);
            ApplySpeed();
            Changed?.Invoke();
        }

        public void Restore(int restoredDay, float restoredMinutes)
        {
            day = Mathf.Max(1, restoredDay);
            minutesSinceMidnight = Mathf.Repeat(restoredMinutes, 24f * 60f);
            Changed?.Invoke();
        }

        public void ApplySpeed()
        {
            Time.timeScale = speed;
        }
    }
}
