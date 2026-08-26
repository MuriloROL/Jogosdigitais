using ProjetoJogosDigitais.Gameplay;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ProjetoJogosDigitais.Presentation
{
    public sealed class MainMenuController : MonoBehaviour
    {
        [SerializeField] private GameObject mainView;
        [SerializeField] private GameObject creditsView;
        [SerializeField] private Button playButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button creditsButton;
        [SerializeField] private Button backButton;

        public bool CreditsVisible => creditsView != null && creditsView.activeSelf;

        public void Configure(
            GameObject menu,
            GameObject credits,
            Button play,
            Button continueCareer,
            Button showCredits,
            Button back)
        {
            mainView = menu;
            creditsView = credits;
            playButton = play;
            continueButton = continueCareer;
            creditsButton = showCredits;
            backButton = back;
        }

        private void Awake()
        {
            playButton.onClick.AddListener(PlayGame);
            continueButton?.onClick.AddListener(ContinueGame);
            creditsButton.onClick.AddListener(ShowCredits);
            backButton.onClick.AddListener(ShowMainMenu);
            ShowMainMenu();
            if (continueButton != null)
            {
                continueButton.interactable = CareerGame.DefaultSaveExists;
            }
        }

        private void OnDestroy()
        {
            playButton?.onClick.RemoveListener(PlayGame);
            continueButton?.onClick.RemoveListener(ContinueGame);
            creditsButton?.onClick.RemoveListener(ShowCredits);
            backButton?.onClick.RemoveListener(ShowMainMenu);
        }

        public void PlayGame()
        {
            CareerGame.Current?.ReleaseSession();
            SceneManager.LoadScene(CareerSceneFlow.InternScene, LoadSceneMode.Single);
        }

        public void ContinueGame()
        {
            CareerGame career = CareerGame.Current;
            if (career == null)
            {
                career = new GameObject("Carreira carregada do menu").AddComponent<CareerGame>();
            }

            if (!career.LoadGame())
            {
                Destroy(career.gameObject);
            }
        }

        public void ShowCredits()
        {
            mainView.SetActive(false);
            creditsView.SetActive(true);
        }

        public void ShowMainMenu()
        {
            creditsView.SetActive(false);
            mainView.SetActive(true);
        }
    }
}
