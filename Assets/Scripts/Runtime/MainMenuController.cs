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
        [SerializeField] private Button creditsButton;
        [SerializeField] private Button backButton;

        public bool CreditsVisible => creditsView != null && creditsView.activeSelf;

        public void Configure(
            GameObject menu,
            GameObject credits,
            Button play,
            Button showCredits,
            Button back)
        {
            mainView = menu;
            creditsView = credits;
            playButton = play;
            creditsButton = showCredits;
            backButton = back;
        }

        private void Awake()
        {
            playButton.onClick.AddListener(PlayGame);
            creditsButton.onClick.AddListener(ShowCredits);
            backButton.onClick.AddListener(ShowMainMenu);
            ShowMainMenu();
        }

        private void OnDestroy()
        {
            playButton?.onClick.RemoveListener(PlayGame);
            creditsButton?.onClick.RemoveListener(ShowCredits);
            backButton?.onClick.RemoveListener(ShowMainMenu);
        }

        public void PlayGame()
        {
            SceneManager.LoadScene(CareerSceneFlow.InternScene, LoadSceneMode.Single);
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
