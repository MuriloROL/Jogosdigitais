using System.IO;
using NUnit.Framework;
using ProjetoJogosDigitais.Gameplay;
using UnityEngine;

namespace ProjetoJogosDigitais.Tests
{
    public sealed class CareerGameTests
    {
        private GameObject gameObject;
        private CareerGame game;

        [SetUp]
        public void SetUp()
        {
            gameObject = new GameObject("Career game test");
            game = gameObject.AddComponent<CareerGame>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(gameObject);
        }

        [Test]
        public void CodingBuildsDeliveryAndConsumesEnergyAndTime()
        {
            OfficeActionResult result = game.PerformAction(OfficeActionType.Code);

            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.DeliveryGained, Is.GreaterThan(0));
            Assert.That(game.Delivery, Is.EqualTo(result.DeliveryGained));
            Assert.That(game.Energy, Is.LessThan(100));
            Assert.That(game.MinutesUsed, Is.EqualTo(60));
        }

        [Test]
        public void InternIsPromotedAfterTwoProductiveDays()
        {
            CompleteProductiveDay(game);
            Assert.That(game.Rank, Is.EqualTo(CareerRank.Intern));

            bool promoted = CompleteProductiveDay(game);

            Assert.That(promoted, Is.True);
            Assert.That(game.Rank, Is.EqualTo(CareerRank.JuniorDeveloper));
            Assert.That(game.SuccessfulDaysAtRank, Is.Zero);
        }

        [Test]
        public void PlayerCanProgressThroughEveryCareerRank()
        {
            int safety = 40;
            while (!game.IsLastRank && safety-- > 0)
            {
                CompleteProductiveDay(game);
            }

            Assert.That(game.Rank, Is.EqualTo(CareerRank.Owner));
            Assert.That(game.TotalSuccessfulDays, Is.EqualTo(25));
        }

        [Test]
        public void UnproductiveDayDoesNotCountForPromotion()
        {
            game.EndDay();

            Assert.That(game.Rank, Is.EqualTo(CareerRank.Intern));
            Assert.That(game.SuccessfulDaysAtRank, Is.Zero);
            Assert.That(game.Day, Is.EqualTo(2));
        }

        [TestCase(CareerRank.Intern, CareerSceneFlow.InternScene)]
        [TestCase(CareerRank.JuniorDeveloper, CareerSceneFlow.DeveloperScene)]
        [TestCase(CareerRank.MidLevelDeveloper, CareerSceneFlow.DeveloperScene)]
        [TestCase(CareerRank.SeniorDeveloper, CareerSceneFlow.LeadershipScene)]
        [TestCase(CareerRank.TechLead, CareerSceneFlow.LeadershipScene)]
        [TestCase(CareerRank.Cto, CareerSceneFlow.ExecutiveScene)]
        [TestCase(CareerRank.Owner, CareerSceneFlow.FinaleScene)]
        public void CareerRankMapsToExpectedEnvironment(CareerRank rank, string expectedScene)
        {
            Assert.That(CareerSceneFlow.SceneFor(rank), Is.EqualTo(expectedScene));
        }

        [Test]
        public void SaveAndLoadRestoresCareerState()
        {
            string path = Path.Combine(Application.temporaryCachePath, "dev-career-test.json");
            game.SetSavePathOverride(path);
            game.PerformAction(OfficeActionType.Code);
            game.PerformAction(OfficeActionType.Study);
            int expectedDelivery = game.Delivery;
            int expectedKnowledge = game.Knowledge;
            game.SaveGame();

            game.PerformAction(OfficeActionType.Coffee);
            game.EndDay();
            Assert.That(game.LoadGame(), Is.True);

            Assert.That(game.Day, Is.EqualTo(1));
            Assert.That(game.Delivery, Is.EqualTo(expectedDelivery));
            Assert.That(game.Knowledge, Is.EqualTo(expectedKnowledge));

            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        private static bool CompleteProductiveDay(CareerGame career)
        {
            int actions = 0;
            while (!career.GoalReached && actions++ < 8)
            {
                OfficeActionResult result = career.PerformAction(OfficeActionType.Code);
                Assert.That(result.Succeeded, Is.True, "O expediente acabou antes da meta ser alcançada.");
            }

            Assert.That(career.GoalReached, Is.True, $"Meta {career.DailyGoal} não foi alcançada no dia {career.Day}.");
            return career.EndDay();
        }
    }
}
