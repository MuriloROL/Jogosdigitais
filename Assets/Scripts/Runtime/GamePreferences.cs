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
        private const string HighContrastKey = "devcarrer.settings.high-contrast";
        private const string MusicVolumeKey = "devcarrer.settings.music-volume";
        private const string EffectsVolumeKey = "devcarrer.settings.effects-volume";
        private const string AmbientVolumeKey = "devcarrer.settings.ambient-volume";
        private const string FullscreenKey = "devcarrer.settings.fullscreen";
        private const string QualityKey = "devcarrer.settings.quality";

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

        public static bool HighContrast
        {
            get => PlayerPrefs.GetInt(HighContrastKey, 0) == 1;
            set => SetBool(HighContrastKey, value);
        }

        public static float MusicVolume
        {
            get => PlayerPrefs.GetFloat(MusicVolumeKey, 0.75f);
            set => SetFloat(MusicVolumeKey, value);
        }

        public static float EffectsVolume
        {
            get => PlayerPrefs.GetFloat(EffectsVolumeKey, 0.75f);
            set => SetFloat(EffectsVolumeKey, value);
        }

        public static float AmbientVolume
        {
            get => PlayerPrefs.GetFloat(AmbientVolumeKey, 0.45f);
            set => SetFloat(AmbientVolumeKey, value);
        }

        public static bool Fullscreen
        {
            get => PlayerPrefs.GetInt(FullscreenKey, 1) == 1;
            set
            {
                PlayerPrefs.SetInt(FullscreenKey, value ? 1 : 0);
                PlayerPrefs.Save();
                Screen.fullScreen = value;
                Changed?.Invoke();
            }
        }

        public static int QualityLevel
        {
            get => Mathf.Clamp(PlayerPrefs.GetInt(QualityKey, QualitySettings.GetQualityLevel()), 0, Mathf.Max(0, QualitySettings.names.Length - 1));
            set
            {
                int level = Mathf.Clamp(value, 0, Mathf.Max(0, QualitySettings.names.Length - 1));
                PlayerPrefs.SetInt(QualityKey, level);
                PlayerPrefs.Save();
                QualitySettings.SetQualityLevel(level, true);
                Changed?.Invoke();
            }
        }

        public static event Action Changed;

        private static void SetBool(string key, bool value)
        {
            PlayerPrefs.SetInt(key, value ? 1 : 0);
            PlayerPrefs.Save();
            Changed?.Invoke();
        }

        private static void SetFloat(string key, float value)
        {
            PlayerPrefs.SetFloat(key, Mathf.Clamp01(value));
            PlayerPrefs.Save();
            Changed?.Invoke();
        }
    }
}
