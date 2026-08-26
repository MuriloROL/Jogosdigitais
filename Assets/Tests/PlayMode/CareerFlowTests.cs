using System.Collections;
using System.Linq;
using NUnit.Framework;
using ProjetoJogosDigitais.Gameplay;
using ProjetoJogosDigitais.Presentation;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace ProjetoJogosDigitais.Tests
{
    public sealed class CareerFlowTests
    {
        [UnitySetUp]
        public IEnumerator RemovePersistentCareerBetweenTests()
        {
            if (CareerGame.Current != null)
            {
                Object.Destroy(CareerGame.Current.gameObject);
                yield return null;
            }
        }

        [UnityTest]
        public IEnumerator DeveloperWalksToDeskAndProducesDelivery()
        {
            Time.timeScale = 1f;
            yield return SceneManager.LoadSceneAsync(CareerSceneFlow.InternScene, LoadSceneMode.Single);
            yield return null;

            CareerGame game = Object.FindFirstObjectByType<CareerGame>();
            OfficePlayerController player = Object.FindFirstObjectByType<OfficePlayerController>();
            OfficeStation desk = Object.FindObjectsByType<OfficeStation>(FindObjectsSortMode.None)
                .First(station => station.ActionType == OfficeActionType.Code);
            int deliveryBefore = game.Delivery;

            player.SelectStation(desk);
            float timeout = 8f;
            while (!player.IsIdle)
            {
                timeout -= Time.unscaledDeltaTime;
                Assert.That(timeout, Is.GreaterThan(0f), "Alex não concluiu a tarefa na estação de desenvolvimento.");
                yield return null;
            }

            Assert.That(game.Delivery, Is.GreaterThan(deliveryBefore));
            Assert.That(game.MinutesUsed, Is.Zero, "Ações manuais não devem acelerar o relógio real do idle.");
            Assert.That(player.Status, Does.Contain("entreg"));
        }

        [UnityTest]
        public IEnumerator CoffeeRestoresEnergyAfterCoding()
        {
            Time.timeScale = 1f;
            yield return SceneManager.LoadSceneAsync(CareerSceneFlow.InternScene, LoadSceneMode.Single);
            yield return null;

            CareerGame game = Object.FindFirstObjectByType<CareerGame>();
            game.PerformAction(OfficeActionType.Code);
            game.PerformAction(OfficeActionType.Code);
            int tiredEnergy = game.Energy;

            OfficeActionResult coffee = game.PerformAction(OfficeActionType.Coffee);

            Assert.That(coffee.Succeeded, Is.True);
            Assert.That(game.Energy, Is.GreaterThan(tiredEnergy));
            Assert.That(game.Stress, Is.LessThan(34));
        }

        [UnityTest]
        public IEnumerator SprintButtonClaimsTheCurrentMilestone()
        {
            Time.timeScale = 1f;
            yield return SceneManager.LoadSceneAsync(CareerSceneFlow.InternScene, LoadSceneMode.Single);
            yield return null;

            CareerGame game = Object.FindFirstObjectByType<CareerGame>();
            while (!game.GoalReached)
            {
                game.PerformAction(OfficeActionType.Code);
            }
            Button endDay = Object.FindObjectsByType<Button>(FindObjectsSortMode.None)
                .First(button => button.name == "RESGATAR META" || button.name == "ENCERRAR DIA");

            endDay.onClick.Invoke();
            yield return null;

            Assert.That(game.Day, Is.EqualTo(1));
            Assert.That(game.SprintBonusClaimed, Is.True);
            Assert.That(game.LastFeedback, Does.Contain("Meta resgatada"));
        }

        [UnityTest]
        public IEnumerator HorizontalWalkingUsesTheCorrectDirectionFrames()
        {
            Time.timeScale = 1f;
            yield return SceneManager.LoadSceneAsync(CareerSceneFlow.InternScene, LoadSceneMode.Single);
            yield return null;

            OfficePlayerController player = Object.FindFirstObjectByType<OfficePlayerController>();
            SpriteRenderer renderer = player.GetComponent<SpriteRenderer>();
            Vector3 start = player.transform.position;

            player.MoveTo(start + Vector3.right * 2f);
            yield return null;
            Assert.That(renderer.sprite.name, Does.Contain("walk-Right"));

            while (!player.IsIdle)
            {
                yield return null;
            }

            player.MoveTo(player.transform.position + Vector3.left * 2f);
            yield return null;
            Assert.That(renderer.sprite.name, Does.Contain("walk-Left"));
        }

        [UnityTest]
        public IEnumerator PromotionLoadsTheDeveloperOffice()
        {
            Time.timeScale = 1f;
            yield return SceneManager.LoadSceneAsync(CareerSceneFlow.InternScene, LoadSceneMode.Single);
            yield return null;

            CareerGame game = CareerGame.Current;
            game.AdvanceOpenTime(350f * 15f);
            yield return null;

            float timeout = 5f;
            while (SceneManager.GetActiveScene().name != CareerSceneFlow.DeveloperScene)
            {
                timeout -= Time.unscaledDeltaTime;
                Assert.That(timeout, Is.GreaterThan(0f), "A promoção não carregou o escritório de Júnior/Pleno.");
                yield return null;
            }

            Assert.That(game.Rank, Is.EqualTo(CareerRank.JuniorDeveloper));
            Assert.That(Object.FindFirstObjectByType<OfficePlayerController>(), Is.Not.Null);
            CareerDecisionController decision = Object.FindFirstObjectByType<CareerDecisionController>();
            yield return null;
            Assert.That(decision, Is.Not.Null);
            Assert.That(decision.IsVisible, Is.True);
        }

        [UnityTest]
        public IEnumerator MainMenuShowsCreditsAndStartsTheGame()
        {
            Time.timeScale = 1f;
            yield return SceneManager.LoadSceneAsync(CareerSceneFlow.MainMenuScene, LoadSceneMode.Single);
            yield return null;

            MainMenuController menu = Object.FindFirstObjectByType<MainMenuController>();
            BackgroundMusic music = Object.FindFirstObjectByType<BackgroundMusic>();
            GameAudioFeedback effects = Object.FindFirstObjectByType<GameAudioFeedback>();
            Assert.That(menu, Is.Not.Null);
            Assert.That(music, Is.Not.Null);
            Assert.That(effects, Is.Not.Null);
            Assert.That(music.ClipName, Is.EqualTo("levelmusicloop-tigrun"));
            Assert.That(music.IsLooping, Is.True);
            Assert.That(menu.CreditsVisible, Is.False);
            Assert.That(Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Any(button => button.name == "CONTINUAR"), Is.True);

            Button credits = Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .First(button => button.name == "CRÉDITOS");
            credits.onClick.Invoke();
            yield return null;
            Assert.That(menu.CreditsVisible, Is.True);

            Button back = Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .First(button => button.name == "VOLTAR");
            back.onClick.Invoke();
            yield return null;
            Assert.That(menu.CreditsVisible, Is.False);

            Button play = Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .First(button => button.name == "JOGAR");
            play.onClick.Invoke();

            float timeout = 5f;
            while (SceneManager.GetActiveScene().name != CareerSceneFlow.InternScene)
            {
                timeout -= Time.unscaledDeltaTime;
                Assert.That(timeout, Is.GreaterThan(0f), "O botão Jogar não abriu o escritório inicial.");
                yield return null;
            }

            Assert.That(Object.FindFirstObjectByType<OfficePlayerController>(), Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator TutorialExplainsTheFirstCareerDay()
        {
            PlayerPrefs.DeleteKey(CareerTutorialController.SeenPlayerPrefsKey);
            Time.timeScale = 1f;
            yield return SceneManager.LoadSceneAsync(CareerSceneFlow.InternScene, LoadSceneMode.Single);
            yield return null;

            CareerTutorialController tutorial = Object.FindFirstObjectByType<CareerTutorialController>();
            Assert.That(tutorial, Is.Not.Null);
            Assert.That(tutorial.IsVisible, Is.True);
            Assert.That(tutorial.StepCount, Is.EqualTo(5));

            for (int step = 0; step < tutorial.StepCount; step++)
            {
                tutorial.NextStep();
            }

            Assert.That(tutorial.IsVisible, Is.False);
            Assert.That(PlayerPrefs.GetInt(CareerTutorialController.SeenPlayerPrefsKey), Is.EqualTo(1));
            PlayerPrefs.DeleteKey(CareerTutorialController.SeenPlayerPrefsKey);
        }

        [UnityTest]
        public IEnumerator IdlePanelHiresATeammateAndShowsSettings()
        {
            Time.timeScale = 1f;
            yield return SceneManager.LoadSceneAsync(CareerSceneFlow.InternScene, LoadSceneMode.Single);
            yield return null;
            yield return null;

            CareerGame game = CareerGame.Current;
            IdleManagementPanel panel = Object.FindFirstObjectByType<IdleManagementPanel>();
            Assert.That(panel, Is.Not.Null);
            RectTransform managementRect = GameObject.Find("Gestão idle").GetComponent<RectTransform>();
            Assert.That(managementRect.anchorMin, Is.EqualTo(new Vector2(1f, 0f)));
            Assert.That(managementRect.anchoredPosition.y, Is.EqualTo(110f));
            game.AdvanceOpenTime(15f * 60f);
            panel.HireButton.onClick.Invoke();
            yield return null;

            Assert.That(game.TeamMembers, Is.EqualTo(1));
            Assert.That(game.PassiveDeliveryPerTick, Is.EqualTo(2));

            Button settings = Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .First(button => button.name == "ÁUDIO & ACESSIBILIDADE");
            settings.onClick.Invoke();
            Assert.That(panel.SettingsVisible, Is.True);
        }

        [UnityTest]
        public IEnumerator OwnerFinaleFinishesWithoutPlayerControls()
        {
            Time.timeScale = 1f;
            yield return SceneManager.LoadSceneAsync(CareerSceneFlow.FinaleScene, LoadSceneMode.Single);
            yield return null;

            OwnerFinaleController finale = Object.FindFirstObjectByType<OwnerFinaleController>();
            Assert.That(Object.FindFirstObjectByType<OfficePlayerController>(), Is.Null);

            float timeout = 10f;
            while (!finale.HasFinished)
            {
                timeout -= Time.unscaledDeltaTime;
                Assert.That(timeout, Is.GreaterThan(0f), "A animação final não terminou.");
                yield return null;
            }
        }

        [UnityTearDown]
        public IEnumerator ResetTimeScale()
        {
            Time.timeScale = 1f;
            yield return null;
        }
    }
}
