using System.Collections.Generic;
using ProjetoJogosDigitais.Gameplay;
using UnityEngine;

namespace ProjetoJogosDigitais.Presentation
{
    public enum FeedbackSound
    {
        Button,
        Work,
        Purchase,
        Promotion,
        Decision,
        Mission,
        Code,
        Meeting,
        Study,
        Coffee,
        Break
    }

    [RequireComponent(typeof(AudioSource))]
    public sealed class GameAudioFeedback : MonoBehaviour
    {
        private const int SampleRate = 22050;
        private readonly Dictionary<FeedbackSound, AudioClip> clips = new();
        private AudioSource source;

        public static GameAudioFeedback Current { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (Current == null)
            {
                new GameObject("Efeitos sonoros • Dev Carrer").AddComponent<GameAudioFeedback>();
            }
        }

        private void Awake()
        {
            if (Current != null && Current != this)
            {
                Destroy(gameObject);
                return;
            }

            Current = this;
            DontDestroyOnLoad(gameObject);
            source = GetComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            RefreshVolume();
            GamePreferences.Changed += RefreshVolume;
        }

        public static void Play(FeedbackSound sound)
        {
            if (Current == null || GamePreferences.EffectsVolume <= 0f)
            {
                return;
            }

            Current.PlayInternal(sound);
        }

        public static void PlayForAction(OfficeActionType action)
        {
            Play(action switch
            {
                OfficeActionType.Code => FeedbackSound.Code,
                OfficeActionType.Meeting => FeedbackSound.Meeting,
                OfficeActionType.Study => FeedbackSound.Study,
                OfficeActionType.Coffee => FeedbackSound.Coffee,
                OfficeActionType.Break => FeedbackSound.Break,
                _ => FeedbackSound.Work
            });
        }

        private void PlayInternal(FeedbackSound sound)
        {
            if (!clips.TryGetValue(sound, out AudioClip clip))
            {
                clip = BuildClip(sound);
                clips.Add(sound, clip);
            }

            source.PlayOneShot(clip);
        }

        private static AudioClip BuildClip(FeedbackSound sound)
        {
            (float startFrequency, float endFrequency, float duration) = sound switch
            {
                FeedbackSound.Button => (520f, 660f, 0.07f),
                FeedbackSound.Work => (260f, 390f, 0.11f),
                FeedbackSound.Purchase => (440f, 760f, 0.16f),
                FeedbackSound.Promotion => (420f, 1040f, 0.32f),
                FeedbackSound.Decision => (360f, 560f, 0.14f),
                FeedbackSound.Mission => (330f, 820f, 0.24f),
                FeedbackSound.Code => (220f, 510f, 0.13f),
                FeedbackSound.Meeting => (380f, 470f, 0.15f),
                FeedbackSound.Study => (520f, 710f, 0.16f),
                FeedbackSound.Coffee => (610f, 360f, 0.18f),
                FeedbackSound.Break => (440f, 280f, 0.22f),
                _ => (440f, 520f, 0.1f)
            };
            int sampleCount = Mathf.CeilToInt(SampleRate * duration);
            var samples = new float[sampleCount];
            for (int index = 0; index < sampleCount; index++)
            {
                float progress = index / (float)sampleCount;
                float frequency = Mathf.Lerp(startFrequency, endFrequency, progress);
                float envelope = Mathf.Sin(progress * Mathf.PI) * (1f - progress * 0.35f);
                samples[index] = Mathf.Sin(2f * Mathf.PI * frequency * index / SampleRate) * envelope * 0.28f;
            }

            AudioClip clip = AudioClip.Create($"feedback-{sound}", sampleCount, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private void OnDestroy()
        {
            GamePreferences.Changed -= RefreshVolume;
            foreach (AudioClip clip in clips.Values)
            {
                if (clip != null)
                {
                    Destroy(clip);
                }
            }

            clips.Clear();
            if (Current == this)
            {
                Current = null;
            }
        }

        private void RefreshVolume()
        {
            if (source != null)
            {
                source.volume = 0.42f * GamePreferences.EffectsVolume;
            }
        }
    }
}
