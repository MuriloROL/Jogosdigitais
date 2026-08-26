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
        public IEnumerator EveryStationHasALargeClickableActivityMarker()
        {
            Time.timeScale = 1f;
            yield return SceneManager.LoadSceneAsync(CareerSceneFlow.InternScene, LoadSceneMode.Single);
            yield return null;

            OfficeStation[] stations = Object.FindObjectsByType<OfficeStation>(FindObjectsSortMode.None);
            Assert.That(stations, Has.Length.EqualTo(5));
            foreach (OfficeStation station in stations)
            {
                Collider2D target = station.InteractionCollider;
                Assert.That(target, Is.Not.Null, $"{station.name} não criou o alvo clicável.");
                Assert.That(target.isTrigger, Is.True);
                Assert.That(target.OverlapPoint(station.InteractionPosition), Is.True,
                    $"O losango de {station.name} precisa estar dentro da área clicável.");
                Assert.That(target.bounds.size.x, Is.GreaterThan(2f));

                station.SetPointerFocus(true);
                Assert.That(station.IsPointerFocused, Is.True);
                station.SetPointerFocus(false);
            }
        }

        [UnityTest]
        public IEnumerator ChecklistButtonClaimsTheCurrentMission()
        {
            Time.timeScale = 1f;
            yield return SceneManager.LoadSceneAsync(CareerSceneFlow.InternScene, LoadSceneMode.Single);
            yield return null;

            CareerGame game = Object.FindFirstObjectByType<CareerGame>();
            game.PerformAction(OfficeActionType.Study);
            game.PerformAction(OfficeActionType.Meeting);
            game.PerformAction(OfficeActionType.Code);
            game.PerformAction(OfficeActionType.Code);
            Assert.That(game.MissionReadyToClaim, Is.True);
            Button endDay = Object.FindObjectsByType<Button>(FindObjectsSortMode.None)
                .First(button => button.GetComponentInChildren<Text>(true)?.text == "CONCLUIR MISSÃO");

            endDay.onClick.Invoke();
            yield return null;

            Assert.That(game.MissionIndexAtRank, Is.EqualTo(1));
            Assert.That(game.TotalMissionsCompleted, Is.EqualTo(1));
            Assert.That(game.LastFeedback, Does.Contain("MISSÃO CONCLUÍDA"));
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
            CompleteCurrentMission(game);
            CompleteCurrentMission(game);
            CompleteCurrentMission(game);
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
                .First(button => button.name == "VOLTAR AO MENU");
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
        public IEnumerator IdlePanelFollowsCodexAgentsAndTeamProgression()
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
            Assert.That(managementRect.anchoredPosition.y, Is.EqualTo(110f).Within(3f));
            Assert.That(panel.HireButton.interactable, Is.False);
            panel.HireButton.onClick.Invoke();
            Assert.That(game.TeamMembers, Is.Zero);

            panel.AutomationButton.onClick.Invoke();
            Assert.That(game.ToolingName, Is.EqualTo("Codex Assistente"));
            Assert.That(game.PassiveDeliveryPerTick, Is.EqualTo(2));

            CompleteCurrentMission(game);
            CompleteCurrentMission(game);
            CompleteCurrentMission(game);
            float sceneTimeout = 5f;
            while (SceneManager.GetActiveScene().name != CareerSceneFlow.DeveloperScene)
            {
                sceneTimeout -= Time.unscaledDeltaTime;
                Assert.That(sceneTimeout, Is.GreaterThan(0f));
                yield return null;
            }
            yield return null;
            panel = Object.FindFirstObjectByType<IdleManagementPanel>();
            game.ResolveCareerDecision("Promoção positiva.", 0, 0, 0, 0, 0);
            panel.AutomationButton.onClick.Invoke();
            Assert.That(game.Rank, Is.EqualTo(CareerRank.JuniorDeveloper));
            Assert.That(game.ToolingName, Is.EqualTo("Codex Multiagentes"));
            Assert.That(game.HiringUnlocked, Is.False);

            CompleteCurrentMission(game);
            CompleteCurrentMission(game);
            CompleteCurrentMission(game);
            game.ResolveCareerDecision("Promoção positiva.", 0, 0, 0, 0, 0);
            panel.HireButton.onClick.Invoke();
            Assert.That(game.Rank, Is.EqualTo(CareerRank.MidLevelDeveloper));
            Assert.That(game.TeamMembers, Is.EqualTo(1));
            Assert.That(game.PassiveDeliveryPerTick, Is.EqualTo(4));

            Button settings = Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .First(button => button.name == "ÁUDIO & ACESSIBILIDADE");
            settings.onClick.Invoke();
            Assert.That(panel.SettingsVisible, Is.True);
            yield return null;
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

        [UnityTest]
        public IEnumerator PauseKeepsIdleProductionRunning()
        {
            Time.timeScale = 1f;
            yield return SceneManager.LoadSceneAsync(CareerSceneFlow.InternScene, LoadSceneMode.Single);
            yield return null;

            CareerGame game = CareerGame.Current;
            PauseMenuController pause = Object.FindFirstObjectByType<PauseMenuController>();
            int deliveryBefore = game.Delivery;
            pause.TogglePause();
            Assert.That(Time.timeScale, Is.Zero);

            float timeout = 16f;
            while (timeout > 0f)
            {
                timeout -= Time.unscaledDeltaTime;
                yield return null;
            }

            Assert.That(game.Delivery, Is.GreaterThan(deliveryBefore));
            pause.TogglePause();
            Assert.That(Time.timeScale, Is.EqualTo(1f));
        }

        private static void CompleteCurrentMission(CareerGame game)
        {
            game.AcknowledgeMissionBriefing();
            for (int index = 0; index < game.MissionObjectiveCount; index++)
            {
                MissionObjective objective = game.GetMissionObjective(index);
                switch (objective.Kind)
                {
                    case MissionObjectiveKind.ToolLevel:
                        while (game.AutomationLevel < objective.Required)
                        {
                            while (game.Money < game.NextAutomationCost) game.AdvanceOpenTime(15f);
                            Assert.That(game.UpgradeAutomation(), Is.True);
                        }
                        break;
                    case MissionObjectiveKind.TeamSize:
                        while (game.TeamMembers < objective.Required)
                        {
                            while (game.Money < game.NextHireCost) game.AdvanceOpenTime(15f);
                            Assert.That(game.HireTeammate(), Is.True);
                        }
                        break;
                    default:
                        Assert.That(CareerMissionCatalog.TryGetAction(objective.Kind, out OfficeActionType action),
                            Is.True);
                        while (!game.IsMissionObjectiveComplete(index)) game.PerformAction(action);
                        break;
                }
            }

            Assert.That(game.MissionReadyToClaim, Is.True, game.MissionChecklistText());
            Assert.That(game.ClaimMission(), Is.True);
        }

        [UnityTearDown]
        public IEnumerator ResetTimeScale()
        {
            Time.timeScale = 1f;
            yield return null;
        }
    }
}
