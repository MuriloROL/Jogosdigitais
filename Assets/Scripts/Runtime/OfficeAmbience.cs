using UnityEngine;

namespace ProjetoJogosDigitais.Presentation
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class OfficeAmbience : MonoBehaviour
    {
        private const int SampleRate = 11025;
        private const float Duration = 8f;
        private AudioSource source;
        private AudioClip generatedClip;

        private void Awake()
        {
            source = GetComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = true;
            source.spatialBlend = 0f;
            source.ignoreListenerPause = true;
            generatedClip = BuildOfficeLoop();
            source.clip = generatedClip;
            RefreshVolume();
            GamePreferences.Changed += RefreshVolume;
            source.Play();
        }

        private void OnDestroy()
        {
            GamePreferences.Changed -= RefreshVolume;
            if (generatedClip != null)
            {
                Destroy(generatedClip);
            }
        }

        private void RefreshVolume()
        {
            if (source != null)
            {
                source.volume = 0.09f * GamePreferences.AmbientVolume;
            }
        }

        private static AudioClip BuildOfficeLoop()
        {
            int count = Mathf.RoundToInt(SampleRate * Duration);
            var samples = new float[count];
            uint seed = 0x5A17u;
            for (int index = 0; index < count; index++)
            {
                float time = index / (float)SampleRate;
                seed = seed * 1664525u + 1013904223u;
                float noise = ((seed >> 8) / 16777215f - 0.5f) * 0.028f;
                float hum = Mathf.Sin(time * Mathf.PI * 2f * 58f) * 0.012f;
                float typingPhase = (time % 0.73f) / 0.73f;
                float typing = typingPhase < 0.018f
                    ? Mathf.Sin(time * Mathf.PI * 2f * 920f) * (1f - typingPhase / 0.018f) * 0.08f
                    : 0f;
                samples[index] = noise + hum + typing;
            }

            AudioClip clip = AudioClip.Create("office-ambience-generated", count, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
