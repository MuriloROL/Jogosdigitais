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
        public void ManualActionAdvancesChecklistWithoutAdvancingCareerClock()
        {
            OfficeActionResult result = game.PerformAction(OfficeActionType.Study);

            Assert.That(result.Succeeded, Is.True);
            Assert.That(game.GetMissionObjectiveProgress(0), Is.EqualTo(1));
            Assert.That(game.IsMissionObjectiveComplete(0), Is.True);
            Assert.That(game.MinutesUsed, Is.Zero);
            Assert.That(game.MissionChecklistText(), Does.Contain("✓ Estudar a documentação"));
        }

        [Test]
        public void PassiveProductionRunsEveryFifteenOpenSeconds()
        {
            game.AdvanceOpenTime(14f);
            Assert.That(game.Delivery, Is.Zero);
            game.AdvanceOpenTime(1f);
            Assert.That(game.Delivery, Is.EqualTo(1));
            Assert.That(game.CareerXp, Is.EqualTo(1));
            Assert.That(game.Money, Is.EqualTo(CareerGame.StartingMoney + 1));
            Assert.That(game.SecondsUntilPassiveTick, Is.EqualTo(15f).Within(0.01f));
        }

        [Test]
        public void StartingBudgetBuysCodexAndInternCannotHire()
        {
            Assert.That(game.Money, Is.EqualTo(CareerGame.StartingMoney));
            Assert.That(game.NextAutomationCost, Is.EqualTo(CareerGame.StartingMoney));
            Assert.That(game.HiringUnlocked, Is.False);
            Assert.That(game.HireTeammate(), Is.False);
            Assert.That(game.TeamMembers, Is.Zero);

            Assert.That(game.UpgradeAutomation(), Is.True);
            Assert.That(game.AutomationLevel, Is.EqualTo(1));
            Assert.That(game.ToolingName, Is.EqualTo("Codex Assistente"));
            Assert.That(game.UpgradeAutomation(), Is.False,
                "Multiagentes precisam aguardar a promoção para Júnior.");
        }

        [Test]
        public void InternHasThreeChecklistMissionsAndNoMandatoryWaiting()
        {
            Assert.That(game.MissionsPerRank, Is.EqualTo(3));
            Assert.That(game.MissionObjectiveCount, Is.InRange(2, 3));

            CompleteCurrentRank();

            Assert.That(game.Rank, Is.EqualTo(CareerRank.JuniorDeveloper));
            Assert.That(game.TotalMissionsCompleted, Is.EqualTo(3));
            Assert.That(game.ElapsedWorkSeconds, Is.Zero,
                "A promoção não deve depender de esperar ciclos do Codex.");
            Assert.That(game.AutomationLevel, Is.EqualTo(1));
            Assert.That(game.TeamMembers, Is.Zero);
            Assert.That(game.HasPendingCareerDecision, Is.True);
        }

        [Test]
        public void JuniorGetsMultiAgentsAndHiringOnlyUnlocksAtMidLevel()
        {
            CompleteCurrentRank();
            game.ResolveCareerDecision("Promoção aceita.", 0, 0, 0, 0, 0);

            Assert.That(game.Rank, Is.EqualTo(CareerRank.JuniorDeveloper));
            Assert.That(game.HiringUnlocked, Is.False);
            Assert.That(game.UpgradeAutomation(), Is.True);
            Assert.That(game.ToolingName, Is.EqualTo("Codex Multiagentes"));
            Assert.That(game.HireTeammate(), Is.False);

            CompleteCurrentRank();

            Assert.That(game.Rank, Is.EqualTo(CareerRank.MidLevelDeveloper));
            Assert.That(game.HiringUnlocked, Is.True);
            Assert.That(game.TeamCapacity, Is.EqualTo(1));
        }

        [Test]
        public void FullCareerHasEighteenMissionsWithoutMandatoryWaiting()
        {
            int guard = 0;
            while (!game.IsLastRank && guard++ < 18)
            {
                CompleteCurrentMission();
                if (game.HasPendingCareerDecision)
                {
                    game.ResolveCareerDecision("Decisão positiva.", 0, 0, 0, 0, 0);
                }
            }

            Assert.That(game.Rank, Is.EqualTo(CareerRank.Owner));
            Assert.That(game.TotalMissionsCompleted, Is.EqualTo(18));
            Assert.That(game.ElapsedWorkSeconds, Is.Zero,
                "Nenhum upgrade, contratação ou missão deve obrigar espera passiva.");
            Assert.That(game.AutomationLevel, Is.EqualTo(5));
            Assert.That(game.TeamMembers, Is.EqualTo(4));
        }

        [Test]
        public void MissionCatalogUsesOnlyImmediateActionsPurchasesAndHiring()
        {
            for (CareerRank rank = CareerRank.Intern; rank <= CareerRank.Cto; rank++)
            {
                for (int mission = 0; mission < CareerMissionCatalog.MissionsPerRank; mission++)
                {
                    foreach (MissionObjective objective in CareerMissionCatalog.Get(rank, mission).Objectives)
                    {
                        bool immediate = CareerMissionCatalog.TryGetAction(objective.Kind, out _) ||
                            objective.Kind == MissionObjectiveKind.ToolLevel ||
                            objective.Kind == MissionObjectiveKind.TeamSize;
                        Assert.That(immediate, Is.True,
                            $"{rank}, missão {mission + 1}, ainda contém uma espera obrigatória.");
                    }
                }
            }
        }

        [Test]
        public void CareerNeverFailsOrBlocksActionsAfterFourHours()
        {
            string path = Path.Combine(Application.temporaryCachePath, "dev-career-overtime-test.json");
            game.SetSavePathOverride(path);
            game.AdvanceOpenTime(CareerGame.WorkdayRealSeconds + 3600f);

            OfficeActionResult result = game.PerformAction(OfficeActionType.Code);
            float elapsedBeforeSave = game.ElapsedWorkSeconds;
            game.SaveGame();
            Assert.That(game.LoadGame(), Is.True);

            Assert.That(result.Succeeded, Is.True);
            Assert.That(game.Day, Is.EqualTo(1));
            Assert.That(game.ElapsedWorkSeconds, Is.GreaterThan(CareerGame.WorkdayRealSeconds));
            Assert.That(game.ElapsedWorkSeconds, Is.EqualTo(elapsedBeforeSave).Within(0.01f));
            if (File.Exists(path)) File.Delete(path);
        }

        [Test]
        public void SaveAndLoadRestoresMissionChecklistWithoutOfflineProgress()
        {
            string path = Path.Combine(Application.temporaryCachePath, "dev-career-mission-v7-test.json");
            game.SetSavePathOverride(path);
            game.PerformAction(OfficeActionType.Study);
            game.AdvanceOpenTime(33f);
            float expectedElapsed = game.ElapsedWorkSeconds;
            game.SaveGame();

            game.PerformAction(OfficeActionType.Meeting);
            game.AdvanceOpenTime(900f);
            Assert.That(game.LoadGame(), Is.True);

            Assert.That(game.ElapsedWorkSeconds, Is.EqualTo(expectedElapsed).Within(0.01f));
            Assert.That(game.GetMissionObjectiveProgress(0), Is.EqualTo(1));
            Assert.That(game.GetMissionObjectiveProgress(1), Is.Zero);
            Assert.That(game.MissionIndexAtRank, Is.Zero);
            if (File.Exists(path)) File.Delete(path);
        }

        [Test]
        public void LegacySaveRemovesIllegalInternTeamAndClampsTooling()
        {
            string path = Path.Combine(Application.temporaryCachePath, "dev-career-v5-progression-test.json");
            game.SetSavePathOverride(path);
            File.WriteAllText(path,
                "{\"saveVersion\":5,\"rank\":0,\"money\":300,\"careerXp\":100," +
                "\"teamMembers\":1,\"automationLevel\":5}");

            Assert.That(game.LoadGame(), Is.True);
            Assert.That(game.Rank, Is.EqualTo(CareerRank.Intern));
            Assert.That(game.TeamMembers, Is.Zero);
            Assert.That(game.AutomationLevel, Is.EqualTo(1));
            Assert.That(game.MissionIndexAtRank, Is.Zero);
            if (File.Exists(path)) File.Delete(path);
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

        private void CompleteCurrentRank()
        {
            CareerRank startingRank = game.Rank;
            int guard = 0;
            while (game.Rank == startingRank && guard++ < CareerMissionCatalog.MissionsPerRank)
            {
                CompleteCurrentMission();
            }
        }

        private void CompleteCurrentMission()
        {
            Assert.That(game.IsLastRank, Is.False);
            game.AcknowledgeMissionBriefing();

            for (int index = 0; index < game.MissionObjectiveCount; index++)
            {
                MissionObjective objective = game.GetMissionObjective(index);
                switch (objective.Kind)
                {
                    case MissionObjectiveKind.ToolLevel:
                        while (game.AutomationLevel < objective.Required)
                        {
                            WaitUntil(game.NextAutomationCost);
                            Assert.That(game.UpgradeAutomation(), Is.True,
                                $"Não foi possível comprar {game.NextToolingLabel} no cargo {game.RankName}.");
                        }
                        break;
                    case MissionObjectiveKind.TeamSize:
                        while (game.TeamMembers < objective.Required)
                        {
                            WaitUntil(game.NextHireCost);
                            Assert.That(game.HireTeammate(), Is.True,
                                $"Não foi possível contratar no cargo {game.RankName}.");
                        }
                        break;
                    default:
                        Assert.That(CareerMissionCatalog.TryGetAction(objective.Kind, out OfficeActionType action),
                            Is.True, $"Objetivo {objective.Kind} não pode ser executado.");
                        while (!game.IsMissionObjectiveComplete(index))
                        {
                            Assert.That(game.PerformAction(action).Succeeded, Is.True);
                        }
                        break;
                }
            }

            Assert.That(game.MissionReadyToClaim, Is.True, game.MissionChecklistText());
            Assert.That(game.ClaimMission(), Is.True);
        }

        private void WaitUntil(int requiredMoney)
        {
            int guard = 0;
            while (game.Money < requiredMoney && guard++ < 2000)
            {
                game.AdvanceOpenTime(15f);
            }
            Assert.That(game.Money, Is.GreaterThanOrEqualTo(requiredMoney));
        }
    }
}
