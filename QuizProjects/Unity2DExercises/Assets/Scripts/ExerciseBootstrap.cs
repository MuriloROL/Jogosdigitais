using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ExerciseBootstrap
{
    private const string ExerciseSceneName = "QuizExercises";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void RegisterSceneBuilder()
    {
        SceneManager.sceneLoaded -= BuildScene;
        SceneManager.sceneLoaded += BuildScene;
    }

    private static void BuildScene(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != ExerciseSceneName)
        {
            return;
        }

        Sprite shapeSprite = CreateShapeSprite();
        CreateCamera();
        CreateGround(shapeSprite);
        CreatePlayer(shapeSprite);
        TimedFallPlatform platform = CreateTimedPlatform(shapeSprite);

        GameObject interfaceObject = new GameObject("Exercise Instructions");
        ExerciseHud hud = interfaceObject.AddComponent<ExerciseHud>();
        hud.Platform = platform;
    }

    private static Sprite CreateShapeSprite()
    {
        Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }

    private static void CreateCamera()
    {
        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);

        Camera camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 5.5f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.07f, 0.10f, 0.18f);
    }

    private static void CreateGround(Sprite shapeSprite)
    {
        GameObject ground = new GameObject("Ground");
        ground.transform.position = new Vector3(0f, -4.3f, 0f);
        ground.transform.localScale = new Vector3(20f, 0.5f, 1f);

        SpriteRenderer renderer = ground.AddComponent<SpriteRenderer>();
        renderer.sprite = shapeSprite;
        renderer.color = new Color(0.22f, 0.55f, 0.56f);
        ground.AddComponent<BoxCollider2D>();
    }

    private static void CreatePlayer(Sprite shapeSprite)
    {
        GameObject player = new GameObject("Player");
        player.tag = "Player";
        player.transform.position = new Vector3(-4.2f, -3.35f, 0f);

        Rigidbody2D body = player.AddComponent<Rigidbody2D>();
        body.gravityScale = 2.5f;
        body.constraints = RigidbodyConstraints2D.FreezeRotation;

        BoxCollider2D collider = player.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(0.8f, 1.25f);

        GameObject visual = new GameObject("ELV Character");
        visual.transform.SetParent(player.transform, false);
        visual.transform.localScale = Vector3.one * 0.65f;
        SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>();
        renderer.sprite = FindCharacterSprite() ?? shapeSprite;
        renderer.color = renderer.sprite == shapeSprite ? new Color(0.36f, 0.76f, 1f) : Color.white;
        renderer.sortingOrder = 2;

        player.AddComponent<PlayerMovement2D>();
    }

    private static Sprite FindCharacterSprite()
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>("ELV/Char_003_Idle");
        return Array.Find(sprites, sprite => sprite.name == "Char_003_Idle_0")
               ?? (sprites.Length > 0 ? sprites[0] : null);
    }

    private static TimedFallPlatform CreateTimedPlatform(Sprite shapeSprite)
    {
        GameObject platform = new GameObject("Five Second Platform");
        platform.transform.position = new Vector3(-0.8f, -2.5f, 0f);
        platform.transform.localScale = new Vector3(3f, 0.4f, 1f);

        SpriteRenderer renderer = platform.AddComponent<SpriteRenderer>();
        renderer.sprite = shapeSprite;
        renderer.color = new Color(1f, 0.65f, 0.23f);
        renderer.sortingOrder = 1;

        platform.AddComponent<BoxCollider2D>();
        platform.AddComponent<Rigidbody2D>();
        return platform.AddComponent<TimedFallPlatform>();
    }
}
