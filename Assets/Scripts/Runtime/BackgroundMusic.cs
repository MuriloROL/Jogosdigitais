using UnityEngine;

namespace ProjetoJogosDigitais.Presentation
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class BackgroundMusic : MonoBehaviour
    {
        private const string MusicResourcePath = "Audio/levelmusicloop-tigrun";
        private const float DefaultVolume = 0.22f;

        private AudioSource source;

        public static BackgroundMusic Current { get; private set; }
        public string ClipName => source != null && source.clip != null ? source.clip.name : string.Empty;
        public bool IsLooping => source != null && source.loop;
        public float Volume => source == null ? 0f : source.volume;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (Current != null)
            {
                return;
            }

            new GameObject("Música ambiente • Level Music Loop - Tigrun")
                .AddComponent<BackgroundMusic>();
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
            source.clip = Resources.Load<AudioClip>(MusicResourcePath);
            source.loop = true;
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            RefreshVolume();
            source.ignoreListenerPause = true;
            GamePreferences.Changed += RefreshVolume;

            if (source.clip == null)
            {
                Debug.LogError($"Música não encontrada em Resources/{MusicResourcePath}.");
                return;
            }

            source.Play();
        }

        private void OnDestroy()
        {
            GamePreferences.Changed -= RefreshVolume;
            if (Current == this)
            {
                Current = null;
            }
        }

        private void RefreshVolume()
        {
            if (source != null)
            {
                source.volume = DefaultVolume * GamePreferences.MusicVolume;
            }
        }
    }
}
