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
    public sealed class PrototypeInteractionTests
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
            Assert.That(game.MinutesUsed, Is.EqualTo(60));
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
        public IEnumerator EndDayButtonAdvancesCareerCalendar()
        {
            Time.timeScale = 1f;
            yield return SceneManager.LoadSceneAsync(CareerSceneFlow.InternScene, LoadSceneMode.Single);
            yield return null;

            CareerGame game = Object.FindFirstObjectByType<CareerGame>();
            Button endDay = Object.FindObjectsByType<Button>(FindObjectsSortMode.None)
                .First(button => button.name == "ENCERRAR DIA");

            endDay.onClick.Invoke();
            yield return null;

            Assert.That(game.Day, Is.EqualTo(2));
            Assert.That(game.Delivery, Is.Zero);
            Assert.That(game.LastFeedback, Does.Contain("Meta não atingida"));
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
            yield return new WaitForSeconds(0.2f);
            Assert.That(renderer.sprite.name, Does.Contain("walk-Right"));

            while (!player.IsIdle)
            {
                yield return null;
            }

            player.MoveTo(player.transform.position + Vector3.left * 2f);
            yield return new WaitForSeconds(0.2f);
            Assert.That(renderer.sprite.name, Does.Contain("walk-Left"));
        }

        [UnityTest]
        public IEnumerator PromotionLoadsTheDeveloperOffice()
        {
            Time.timeScale = 1f;
            yield return SceneManager.LoadSceneAsync(CareerSceneFlow.InternScene, LoadSceneMode.Single);
            yield return null;

            CareerGame game = CareerGame.Current;
            for (int day = 0; day < 2; day++)
            {
                int guard = 0;
                while (!game.GoalReached && guard++ < 8)
                {
                    game.PerformAction(OfficeActionType.Code);
                }
                game.EndDay();
                yield return null;
            }

            float timeout = 5f;
            while (SceneManager.GetActiveScene().name != CareerSceneFlow.DeveloperScene)
            {
                timeout -= Time.unscaledDeltaTime;
                Assert.That(timeout, Is.GreaterThan(0f), "A promoção não carregou o escritório de Júnior/Pleno.");
                yield return null;
            }

            Assert.That(game.Rank, Is.EqualTo(CareerRank.JuniorDeveloper));
            Assert.That(Object.FindFirstObjectByType<OfficePlayerController>(), Is.Not.Null);
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
