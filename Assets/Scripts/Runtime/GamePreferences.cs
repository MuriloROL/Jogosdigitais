using System;
using UnityEngine;

namespace ProjetoJogosDigitais.Presentation
{
    public static class GamePreferences
    {
        private const string MusicKey = "devcarrer.settings.music";
        private const string EffectsKey = "devcarrer.settings.effects";
        private const string ReduceMotionKey = "devcarrer.settings.reduce-motion";
        private const string LargeTextKey = "devcarrer.settings.large-text";

        public static bool MusicEnabled
        {
            get => PlayerPrefs.GetInt(MusicKey, 1) == 1;
            set => SetBool(MusicKey, value);
        }

        public static bool EffectsEnabled
        {
            get => PlayerPrefs.GetInt(EffectsKey, 1) == 1;
            set => SetBool(EffectsKey, value);
        }

        public static bool ReduceMotion
        {
            get => PlayerPrefs.GetInt(ReduceMotionKey, 0) == 1;
            set => SetBool(ReduceMotionKey, value);
        }

        public static bool LargeText
        {
            get => PlayerPrefs.GetInt(LargeTextKey, 0) == 1;
            set => SetBool(LargeTextKey, value);
        }

        public static event Action Changed;

        private static void SetBool(string key, bool value)
        {
            PlayerPrefs.SetInt(key, value ? 1 : 0);
            PlayerPrefs.Save();
            Changed?.Invoke();
        }
    }
}
