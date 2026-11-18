using UnityEngine;

public sealed class ExerciseHud : MonoBehaviour
{
    public TimedFallPlatform Platform { get; set; }

    private void OnGUI()
    {
        GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.fontSize = 22;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.normal.textColor = Color.white;

        GUIStyle bodyStyle = new GUIStyle(GUI.skin.label);
        bodyStyle.fontSize = 16;
        bodyStyle.normal.textColor = Color.white;

        GUILayout.BeginArea(new Rect(20f, 20f, Screen.width - 40f, 115f));
        GUILayout.Label("Unity 2D Exercises - Quizzes 2 and 8", titleStyle);
        GUILayout.Label("Move: A/D or arrows    Jump: Space    Restart: R", bodyStyle);
        GUILayout.Label("Touch the orange platform. It moves for 5 seconds, then stops and falls.", bodyStyle);

        if (Platform != null)
        {
            string phase = Platform.Phase == TimedFallPlatform.PlatformPhase.Moving
                ? $"Moving: {Platform.SecondsRemaining:0.0}s left"
                : Platform.Phase.ToString();
            GUILayout.Label($"Platform: {phase}", bodyStyle);
        }

        GUILayout.EndArea();
    }
}
