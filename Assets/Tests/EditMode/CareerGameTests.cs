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
        public void ManualCodingAcceleratesDeliveryWithoutAdvancingTheRealClock()
        {
            OfficeActionResult result = game.PerformAction(OfficeActionType.Code);
            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.DeliveryGained, Is.GreaterThan(0));
            Assert.That(game.Delivery, Is.EqualTo(result.DeliveryGained));
            Assert.That(game.CareerXp, Is.EqualTo(result.DeliveryGained));
            Assert.That(game.Energy, Is.LessThan(100));
            Assert.That(game.MinutesUsed, Is.Zero);
        }

        [Test]
        public void PassiveProductionRunsEveryFifteenOpenSeconds()
        {
            game.AdvanceOpenTime(14f);
            Assert.That(game.Delivery, Is.Zero);
            game.AdvanceOpenTime(1f);
            Assert.That(game.Delivery, Is.EqualTo(1));
            Assert.That(game.CareerXp, Is.EqualTo(1));
            Assert.That(game.Money, Is.EqualTo(121));
            Assert.That(game.SecondsUntilPassiveTick, Is.EqualTo(15f).Within(0.01f));
        }

        [Test]
        public void ClockOnlyCountsTimeExplicitlySpentWithTheGameOpen()
        {
            game.AdvanceOpenTime(60f * 73f);
            Assert.That(game.Day, Is.EqualTo(1));
            Assert.That(game.MinutesUsed, Is.EqualTo(73));
            Assert.That(game.DisplayTime, Does.Contain("10:13"));
        }

        [Test]
        public void HiringAColleagueDoublesInitialPassiveOutput()
        {
            game.AdvanceOpenTime(15f * 60f);
            Assert.That(game.Money, Is.EqualTo(game.NextHireCost));
            Assert.That(game.HireTeammate(), Is.True);
            Assert.That(game.TeamMembers, Is.EqualTo(1));
            Assert.That(game.PassiveDeliveryPerTick, Is.EqualTo(2));
            Assert.That(game.Money, Is.Zero);
            Assert.That(game.HireTeammate(), Is.False, "Estagiário deve ter apenas uma vaga de equipe.");
        }

        [Test]
        public void AutomationUpgradeIncreasesProductionAndConsumesCash()
        {
            int safety = 1000;
            while (game.Money < game.NextAutomationCost && safety-- > 0)
            {
                game.AdvanceOpenTime(15f);
            }
            int outputBefore = game.PassiveDeliveryPerTick;
            int cost = game.NextAutomationCost;
            int moneyBefore = game.Money;
            Assert.That(game.UpgradeAutomation(), Is.True);
            Assert.That(game.AutomationLevel, Is.EqualTo(1));
            Assert.That(game.PassiveDeliveryPerTick, Is.GreaterThanOrEqualTo(outputBefore));
            Assert.That(game.Money, Is.EqualTo(moneyBefore - cost));
        }

        [Test]
        public void DailyTaskRewardsXpOnlyOnce()
        {
            game.PerformAction(OfficeActionType.Code);
            Assert.That(game.DailyTaskProgress, Is.EqualTo(1));
            int xpBeforeCompletion = game.CareerXp;
            OfficeActionResult second = game.PerformAction(OfficeActionType.Code);
            Assert.That(game.DailyTaskCompleted, Is.True);
            Assert.That(game.CareerXp - xpBeforeCompletion, Is.EqualTo(second.DeliveryGained + game.DailyTaskRewardXp));
            int xpAfterCompletion = game.CareerXp;
            OfficeActionResult third = game.PerformAction(OfficeActionType.Code);
            Assert.That(game.CareerXp - xpAfterCompletion, Is.EqualTo(third.DeliveryGained));
        }

        [Test]
        public void SprintBonusCanOnlyBeClaimedOnce()
        {
            while (!game.GoalReached)
            {
                game.PerformAction(OfficeActionType.Code);
            }
            int moneyBefore = game.Money;
            Assert.That(game.ClaimSprintBonus(), Is.True);
            Assert.That(game.Money, Is.GreaterThan(moneyBefore));
            Assert.That(game.SprintBonusClaimed, Is.True);
            Assert.That(game.ClaimSprintBonus(), Is.False);
        }

        [Test]
        public void HiringAvailableTeamReachesOwnerInAboutOneWorkday()
        {
            int ticks = 0;
            while (!game.IsLastRank && ticks++ < 2000)
            {
                game.AdvanceOpenTime(15f);
                if (game.CanHire) game.HireTeammate();
            }
            float hours = game.ElapsedWorkSeconds / 3600f;
            Assert.That(game.Rank, Is.EqualTo(CareerRank.Owner));
            Assert.That(hours, Is.InRange(7f, 8f));
            Assert.That(game.Day, Is.EqualTo(1));
        }

        [Test]
        public void PromotionCreatesAPendingCareerDecision()
        {
            game.AdvanceOpenTime(350f * 15f);
            Assert.That(game.Rank, Is.EqualTo(CareerRank.JuniorDeveloper));
            Assert.That(game.HasPendingCareerDecision, Is.True);
            game.ResolveCareerDecision("Escolha concluída.", 10, 2, 3, -2, 5);
            Assert.That(game.HasPendingCareerDecision, Is.False);
            Assert.That(game.LastCareerEvent, Is.EqualTo("Escolha concluída."));
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
        public void SaveAndLoadRestoresIdleCareerStateWithoutAddingOfflineProgress()
        {
            string path = Path.Combine(Application.temporaryCachePath, "dev-carrer-idle-test.json");
            game.SetSavePathOverride(path);
            game.AdvanceOpenTime(25f * 60f);
            game.HireTeammate();
            game.AdvanceOpenTime(33f);
            float expectedElapsed = game.ElapsedWorkSeconds;
            int expectedXp = game.CareerXp;
            int expectedTeam = game.TeamMembers;
            game.SaveGame();
            game.AdvanceOpenTime(900f);
            Assert.That(game.LoadGame(), Is.True);
            Assert.That(game.ElapsedWorkSeconds, Is.EqualTo(expectedElapsed).Within(0.01f));
            Assert.That(game.CareerXp, Is.EqualTo(expectedXp));
            Assert.That(game.TeamMembers, Is.EqualTo(expectedTeam));
            if (File.Exists(path)) File.Delete(path);
        }

        [Test]
        public void VersionThreeSaveMigratesToIdleWithoutOfflineProduction()
        {
            string path = Path.Combine(Application.temporaryCachePath, "dev-carrer-v3-test.json");
            game.SetSavePathOverride(path);
            File.WriteAllText(path, "{\"saveVersion\":3,\"rank\":1,\"day\":3,\"minutesUsed\":60,\"money\":300,\"careerXp\":500,\"energy\":80}");
            Assert.That(game.LoadGame(), Is.True);
            Assert.That(game.Rank, Is.EqualTo(CareerRank.JuniorDeveloper));
            Assert.That(game.CareerXp, Is.EqualTo(500));
            Assert.That(game.ElapsedWorkSeconds, Is.EqualTo(3600f));
            Assert.That(game.TeamMembers, Is.Zero);
            if (File.Exists(path)) File.Delete(path);
        }
    }
}
