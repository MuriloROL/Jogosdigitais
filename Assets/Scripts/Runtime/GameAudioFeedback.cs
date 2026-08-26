using System.Collections.Generic;
using UnityEngine;

namespace ProjetoJogosDigitais.Presentation
{
    public enum FeedbackSound
    {
        Button,
        Work,
        Purchase,
        Promotion,
        Decision
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
            source.volume = 0.34f;
        }

        public static void Play(FeedbackSound sound)
        {
            if (Current == null || !GamePreferences.EffectsEnabled)
            {
                return;
            }

            Current.PlayInternal(sound);
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
    }
}
