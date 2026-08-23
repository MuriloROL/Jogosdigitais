using UnityEngine.SceneManagement;

namespace ProjetoJogosDigitais.Gameplay
{
    public static class CareerSceneFlow
    {
        public const string MainMenuScene = "MainMenu";
        public const string InternScene = "OfficeInternMVP";
        public const string DeveloperScene = "OfficeDeveloperMVP";
        public const string LeadershipScene = "OfficeLeadershipMVP";
        public const string ExecutiveScene = "OfficeCtoMVP";
        public const string FinaleScene = "OwnerFinale";

        public static string SceneFor(CareerRank rank)
        {
            return rank switch
            {
                CareerRank.Intern => InternScene,
                CareerRank.JuniorDeveloper => DeveloperScene,
                CareerRank.MidLevelDeveloper => DeveloperScene,
                CareerRank.SeniorDeveloper => LeadershipScene,
                CareerRank.TechLead => LeadershipScene,
                CareerRank.Cto => ExecutiveScene,
                CareerRank.Owner => FinaleScene,
                _ => InternScene
            };
        }

        public static bool IsCurrentSceneCorrect(CareerRank rank)
        {
            return SceneManager.GetActiveScene().name == SceneFor(rank);
        }
    }
}
