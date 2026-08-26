using System;
using System.Linq;
using ProjetoJogosDigitais.Gameplay;
using ProjetoJogosDigitais.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ProjetoJogosDigitais.Editor
{
    public static class OfficeMvpSceneBuilder
    {
        private const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";
        private const string InternScenePath = "Assets/Scenes/OfficeInternMVP.unity";
        private const string DeveloperScenePath = "Assets/Scenes/OfficeDeveloperMVP.unity";
        private const string LeadershipScenePath = "Assets/Scenes/OfficeLeadershipMVP.unity";
        private const string ExecutiveScenePath = "Assets/Scenes/OfficeCtoMVP.unity";
        private const string FinaleScenePath = "Assets/Scenes/OwnerFinale.unity";
        private const string CharacterPath = "Assets/Art/ModernInteriors16/Characters/Premade/Premade_Character_01.png";
        private const string OfficeArtRoot = "Assets/Art/GeneratedOffices/";
        private static readonly Color Panel = new(0.055f, 0.075f, 0.11f, 0.95f);
        private static readonly Color Accent = new(0.24f, 0.82f, 0.76f);
        private static readonly Color Muted = new(0.60f, 0.68f, 0.78f);
        private static readonly Color Amber = new(0.96f, 0.66f, 0.24f);

        [InitializeOnLoadMethod]
        private static void OpenMvpWhenEditorStartsBlank()
        {
            EditorApplication.delayCall += TryOpenMvpWhenBlank;
        }

        private static void TryOpenMvpWhenBlank()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += TryOpenMvpWhenBlank;
                return;
            }

            Scene activeScene = SceneManager.GetActiveScene();
            if (!string.IsNullOrEmpty(activeScene.path))
            {
                return;
            }

            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorApplication.isPlaying = false;
                EditorApplication.delayCall += TryOpenMvpWhenBlank;
                return;
            }

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(MainMenuScenePath) != null)
            {
                EditorSceneManager.OpenScene(MainMenuScenePath, OpenSceneMode.Single);
                Debug.Log($"[DEV CAREER MVP] Cena principal aberta automaticamente: {MainMenuScenePath}");
            }
        }

        [MenuItem("Projeto/Construir MVP carreira dev")]
        public static void Build()
        {
            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
            }

            BuildMainMenuScene();
            BuildOfficeScene(CareerSceneFlow.InternScene, InternScenePath, OfficeTier.Intern);
            BuildOfficeScene(CareerSceneFlow.DeveloperScene, DeveloperScenePath, OfficeTier.Developer);
            BuildOfficeScene(CareerSceneFlow.LeadershipScene, LeadershipScenePath, OfficeTier.Leadership);
            BuildOfficeScene(CareerSceneFlow.ExecutiveScene, ExecutiveScenePath, OfficeTier.Executive);
            BuildFinaleScene();

            ConfigureBuildScenes();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorSceneManager.OpenScene(MainMenuScenePath, OpenSceneMode.Single);
            Debug.Log("[DEV CAREER MVP] Tela inicial, quatro escritórios e cena final foram criados.");
        }

        [MenuItem("Projeto/Construir tela inicial")]
        public static void BuildMainMenu()
        {
            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
            }

            BuildMainMenuScene();
            ConfigureBuildScenes();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorSceneManager.OpenScene(MainMenuScenePath, OpenSceneMode.Single);
            Debug.Log("[DEV CAREER MVP] Tela inicial criada e definida como primeira cena do build.");
        }

        private static void ConfigureBuildScenes()
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(MainMenuScenePath, true),
                new EditorBuildSettingsScene(InternScenePath, true),
                new EditorBuildSettingsScene(DeveloperScenePath, true),
                new EditorBuildSettingsScene(LeadershipScenePath, true),
                new EditorBuildSettingsScene(ExecutiveScenePath, true),
                new EditorBuildSettingsScene(FinaleScenePath, true)
            };
        }

        private static void BuildMainMenuScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = CareerSceneFlow.MainMenuScene;
            CreateCamera(6.2f);

            var canvasObject = new GameObject("Interface da tela inicial");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = true;
            canvasObject.AddComponent<GraphicRaycaster>();
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = 0.5f;

            Image background = CreateImage(canvasObject.transform, "Fundo ByteForge", new Color(0.018f, 0.028f, 0.052f));
            SetRect(background.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            Image skyline = CreateImage(background.transform, "Horizonte da cidade", new Color(0.035f, 0.09f, 0.14f, 0.92f));
            SetRect(skyline.rectTransform, Vector2.zero, new Vector2(1f, 0.34f), Vector2.zero, Vector2.zero);
            for (int index = 0; index < 13; index++)
            {
                float width = 72f + index % 3 * 18f;
                float height = 95f + index % 5 * 27f;
                Image building = CreateImage(skyline.transform, $"Prédio {index + 1}", new Color(0.025f, 0.052f + index % 2 * 0.012f, 0.085f));
                SetRect(building.rectTransform, Vector2.zero, Vector2.zero, new Vector2(index * 104f - 24f, 0f), new Vector2(width, height));
                building.rectTransform.pivot = Vector2.zero;

                Image window = CreateImage(building.transform, "Janela acesa", index % 3 == 0 ? Amber : Accent);
                SetRect(window.rectTransform, new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.65f), Vector2.zero, new Vector2(10f, 10f));
            }

            for (int index = 0; index < 8; index++)
            {
                Image circuit = CreateImage(background.transform, $"Linha tecnológica {index + 1}", new Color(Accent.r, Accent.g, Accent.b, 0.055f));
                SetRect(circuit.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -70f - index * 76f), new Vector2(0f, 1f));
            }

            var mainView = new GameObject("Menu principal", typeof(RectTransform));
            mainView.transform.SetParent(canvasObject.transform, false);
            SetRect((RectTransform)mainView.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            Image brand = CreatePanel(mainView.transform, "Marca Dev Carrer", new Vector2(0f, 54f), new Vector2(610f, 390f), new Vector2(0.5f, 0.5f), Accent);
            Text eyebrow = CreateText(brand.transform, "// BYTEFORGE IDLE SIMULATION", 14, TextAnchor.MiddleCenter, Accent, FontStyle.Bold);
            SetRect(eyebrow.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(30f, -52f), new Vector2(-60f, 24f));
            Text title = CreateText(brand.transform, "DEV CARRER", 54, TextAnchor.MiddleCenter, Color.white, FontStyle.Bold);
            SetRect(title.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(30f, -118f), new Vector2(-60f, 70f));
            Text subtitle = CreateText(brand.transform, "IDLE GAME • DE ESTAGIÁRIO A DONO", 20, TextAnchor.MiddleCenter, Amber, FontStyle.Bold);
            SetRect(subtitle.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(30f, -169f), new Vector2(-60f, 32f));
            Text description = CreateText(brand.transform, "Trabalhe, automatize e lidere.\nUma carreira completa em um único dia.", 16, TextAnchor.MiddleCenter, Muted);
            description.lineSpacing = 1.2f;
            SetRect(description.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(48f, -224f), new Vector2(-96f, 54f));

            Button play = CreateButton(brand.transform, "JOGAR", new Vector2(0f, 118f), new Vector2(300f, 52f), new Vector2(0.5f, 0f), Accent);
            play.GetComponentInChildren<Text>().fontSize = 18;
            Button continueCareer = CreateButton(brand.transform, "CONTINUAR", new Vector2(0f, 66f), new Vector2(300f, 44f), new Vector2(0.5f, 0f), new Color(0.19f, 0.48f, 0.47f));
            continueCareer.GetComponentInChildren<Text>().fontSize = 14;
            Button credits = CreateButton(brand.transform, "CRÉDITOS", new Vector2(0f, 18f), new Vector2(300f, 40f), new Vector2(0.5f, 0f), new Color(0.18f, 0.31f, 0.46f));
            credits.GetComponentInChildren<Text>().fontSize = 14;

            Text footer = CreateText(mainView.transform, "DEV CARRER IDLE GAME  •  VERSÃO 0.2.0", 11, TextAnchor.MiddleCenter, new Color(Muted.r, Muted.g, Muted.b, 0.72f));
            SetRect(footer.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 18f), new Vector2(0f, 24f));

            var creditsRoot = new GameObject("Créditos", typeof(RectTransform));
            creditsRoot.transform.SetParent(canvasObject.transform, false);
            SetRect((RectTransform)creditsRoot.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Image creditsView = CreatePanel(creditsRoot.transform, "Painel de créditos", Vector2.zero, new Vector2(620f, 500f), new Vector2(0.5f, 0.5f), Amber);
            Text creditsTitle = CreateText(creditsView.transform, "CRÉDITOS", 36, TextAnchor.MiddleCenter, Amber, FontStyle.Bold);
            SetRect(creditsTitle.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(30f, -70f), new Vector2(-60f, 52f));
            Text creditsText = CreateText(
                creditsView.transform,
                "ESTÚDIO\nDev Murilo Lima\n\nCONCEPÇÃO E DESENVOLVIMENTO\nMurilo Lima\n\nARTE PIXEL\nModern Interiors • LimeZu\nlimezu.itch.io\n\nMÚSICA\nLevel Music Loop - Tigrun • qubodup\nCC0 1.0 via OpenGameArt\n\nTECNOLOGIA\nDesenvolvido com Unity\n\nObrigado por jogar!",
                15,
                TextAnchor.UpperCenter,
                Color.white);
            creditsText.lineSpacing = 1.15f;
            SetRect(creditsText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(54f, -140f), new Vector2(-108f, 330f));
            Button back = CreateButton(creditsView.transform, "VOLTAR", new Vector2(0f, 28f), new Vector2(220f, 48f), new Vector2(0.5f, 0f), Amber);
            back.GetComponentInChildren<Text>().fontSize = 14;
            creditsRoot.SetActive(false);

            MainMenuController controller = canvasObject.AddComponent<MainMenuController>();
            controller.Configure(mainView, creditsRoot, play, continueCareer, credits, back);
            CreateEventSystem();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, MainMenuScenePath);
        }

        private static void BuildOfficeScene(string sceneName, string scenePath, OfficeTier tier)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = sceneName;

            float cameraSize = tier switch
            {
                OfficeTier.Intern => 6.2f,
                OfficeTier.Developer => 7.6f,
                OfficeTier.Leadership => 8.25f,
                OfficeTier.Executive => 7.3f,
                _ => 7.6f
            };
            Camera camera = CreateCamera(cameraSize);
            OfficeEnvironment environment = new GameObject($"Escritório ByteForge - {tier}").AddComponent<OfficeEnvironment>();
            environment.Configure(tier, LoadOfficeBackground(tier));
            CareerGame career = new GameObject("Carreira do jogador").AddComponent<CareerGame>();

            Vector2 min = tier switch
            {
                OfficeTier.Intern => new Vector2(-6.3f, -4.1f),
                OfficeTier.Developer => new Vector2(-9.6f, -5.7f),
                OfficeTier.Leadership => new Vector2(-10.6f, -6.3f),
                OfficeTier.Executive => new Vector2(-9f, -5.4f),
                _ => new Vector2(-9.6f, -5.7f)
            };
            Vector2 max = -min;
            Vector2 spawn = Vector2.zero;
            OfficePlayerController player = CreatePlayer(camera, spawn, min, max);

            CreateOfficeLayout(tier);
            CreateHud(career, player);
            CreateEventSystem();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, scenePath);
        }

        private static void CreateOfficeLayout(OfficeTier tier)
        {
            switch (tier)
            {
                case OfficeTier.Intern:
                    CreateStation("Mesa compartilhada", OfficeActionType.Code, new Vector2(-3.55f, 2.25f), new Vector2(4.5f, 2.4f), new Vector2(0f, -1.65f));
                    CreateStation("Mesa do supervisor", OfficeActionType.Meeting, new Vector2(4.05f, 2.15f), new Vector2(4.1f, 2.9f), new Vector2(0f, -1.75f));
                    CreateStation("Manuais antigos", OfficeActionType.Study, new Vector2(-4.65f, -2.7f), new Vector2(3.3f, 2.4f), new Vector2(0f, 1.35f));
                    CreateStation("Cafeteira da copa", OfficeActionType.Coffee, new Vector2(0f, -3.25f), new Vector2(3.2f, 2.1f), new Vector2(0f, 1.35f));
                    CreateStation("Sofá do corredor", OfficeActionType.Break, new Vector2(4.55f, -2.85f), new Vector2(4.1f, 2.6f), new Vector2(0f, 1.45f));
                    CreateCoworker("Marta • Supervisora", "Premade_Character_05.png", new Vector2(4.7f, 0.15f));
                    break;
                case OfficeTier.Developer:
                    CreateStation("Estação de desenvolvimento", OfficeActionType.Code, new Vector2(-6.3f, 3.55f), new Vector2(6.6f, 3.2f), new Vector2(0f, -1.9f));
                    CreateStation("Sala de reunião", OfficeActionType.Meeting, new Vector2(6.35f, 3.45f), new Vector2(5.6f, 3.8f), new Vector2(0f, -2.2f));
                    CreateStation("Biblioteca técnica", OfficeActionType.Study, new Vector2(-7.1f, -4.2f), new Vector2(4.3f, 3.1f), new Vector2(0f, 1.55f));
                    CreateStation("Copa e café", OfficeActionType.Coffee, new Vector2(0f, -4.45f), new Vector2(4f, 2.7f), new Vector2(0f, 1.55f));
                    CreateStation("Lounge da equipe", OfficeActionType.Break, new Vector2(6.2f, -4.05f), new Vector2(5.5f, 3.5f), new Vector2(0f, 1.75f));
                    CreateCoworker("Lia • Dev Pleno", "Premade_Character_04.png", new Vector2(-8.2f, 0.1f));
                    CreateCoworker("Caio • Dev Júnior", "Premade_Character_02.png", new Vector2(-2.2f, 2.1f));
                    CreateCoworker("Ravi • Tech Lead", "Premade_Character_03.png", new Vector2(8.4f, 0.1f));
                    break;
                case OfficeTier.Leadership:
                    CreateStation("Mesa de arquitetura", OfficeActionType.Code, new Vector2(-6.7f, 4.7f), new Vector2(6.4f, 3.2f), new Vector2(0f, -2f));
                    CreateStation("War room", OfficeActionType.Meeting, new Vector2(6.1f, 4.25f), new Vector2(6.3f, 4.2f), new Vector2(0f, -2.35f));
                    CreateStation("Centro de conhecimento", OfficeActionType.Study, new Vector2(-7.75f, -4.75f), new Vector2(4.8f, 3.3f), new Vector2(0f, 1.65f));
                    CreateStation("Café da liderança", OfficeActionType.Coffee, new Vector2(0f, -4.95f), new Vector2(4.3f, 3f), new Vector2(0f, 1.65f));
                    CreateStation("Lounge dos líderes", OfficeActionType.Break, new Vector2(6.65f, -4.7f), new Vector2(5.8f, 3.5f), new Vector2(0f, 1.8f));
                    CreateCoworker("Nina • Dev Sênior", "Premade_Character_04.png", new Vector2(-9.4f, 0.4f));
                    CreateCoworker("Theo • Tech Lead", "Premade_Character_03.png", new Vector2(8.8f, 0.3f));
                    CreateCoworker("Maya • Product Manager", "Premade_Character_05.png", new Vector2(0.5f, 2.4f));
                    break;
                case OfficeTier.Executive:
                    CreateStation("Mesa executiva", OfficeActionType.Code, new Vector2(-4.85f, 3.75f), new Vector2(5.8f, 3.1f), new Vector2(0f, -1.95f));
                    CreateStation("Mesa estratégica", OfficeActionType.Meeting, new Vector2(5.1f, 3.7f), new Vector2(5.8f, 4f), new Vector2(0f, -2.2f));
                    CreateStation("Biblioteca do CTO", OfficeActionType.Study, new Vector2(-6.05f, -4.15f), new Vector2(4.5f, 3.2f), new Vector2(0f, 1.55f));
                    CreateStation("Café privativo", OfficeActionType.Coffee, new Vector2(0f, -4.15f), new Vector2(4f, 2.8f), new Vector2(0f, 1.55f));
                    CreateStation("Lounge executivo", OfficeActionType.Break, new Vector2(5.45f, -4.05f), new Vector2(5.3f, 3.4f), new Vector2(0f, 1.7f));
                    CreateCoworker("Clara • Chief of Staff", "Premade_Character_05.png", new Vector2(7.6f, 0.25f));
                    break;
            }
        }

        private static Sprite LoadOfficeBackground(OfficeTier tier)
        {
            string fileName = tier switch
            {
                OfficeTier.Intern => "office-intern.png",
                OfficeTier.Developer => "office-developer.png",
                OfficeTier.Leadership => "office-leadership.png",
                OfficeTier.Executive => "office-cto.png",
                _ => "office-developer.png"
            };
            string path = OfficeArtRoot + fileName;
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
            {
                throw new InvalidOperationException($"Arte de escritório não encontrada ou ainda não importada: {path}");
            }

            return sprite;
        }

        private static void BuildFinaleScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = CareerSceneFlow.FinaleScene;
            CreateCamera(7.1f);
            new GameObject("Carreira concluída").AddComponent<CareerGame>();

            Texture2D sheet = AssetDatabase.LoadAssetAtPath<Texture2D>(CharacterPath);
            Sprite sprite = AssetDatabase.LoadAllAssetsAtPath(CharacterPath).OfType<Sprite>().FirstOrDefault(item => item.name == "Premade_Character_01_0");
            var hero = new GameObject("Alex - Dono da ByteForge");
            hero.transform.position = new Vector3(-9f, -0.65f, 0f);
            SpriteRenderer renderer = hero.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = 30;
            CharacterWalkAnimator animator = hero.AddComponent<CharacterWalkAnimator>();
            animator.Configure(sheet);

            var canvasObject = new GameObject("Interface do final");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = 0.5f;
            Image titlePanel = CreatePanel(canvasObject.transform, "Conclusão", new Vector2(0f, -28f), new Vector2(720f, 116f), new Vector2(0.5f, 1f), Amber);
            Text title = CreateText(titlePanel.transform, "O ÚLTIMO DIA COMO FUNCIONÁRIO", 30, TextAnchor.MiddleCenter, new Color(0.98f, 0.76f, 0.28f));
            SetRect(title.rectTransform, new Vector2(0f, 0.45f), Vector2.one, new Vector2(18f, -4f), new Vector2(-36f, -8f));
            Text subtitle = CreateText(titlePanel.transform, "Uma jornada que começou com um estágio...", 18, TextAnchor.MiddleCenter, Color.white);
            SetRect(subtitle.rectTransform, Vector2.zero, new Vector2(1f, 0.48f), new Vector2(18f, 2f), new Vector2(-36f, -4f));
            Text legacy = CreateText(canvasObject.transform, "7 CARGOS  •  1 DIA DE CARREIRA  •  1 LEGADO", 17, TextAnchor.MiddleCenter, new Color(0.78f, 0.84f, 0.94f));
            SetRect(legacy.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 28f), new Vector2(600f, 42f));

            OwnerFinaleController finale = new GameObject("Sequência final").AddComponent<OwnerFinaleController>();
            finale.Configure(hero.transform, animator, title, subtitle);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, FinaleScenePath);
        }

        private static Camera CreateCamera(float orthographicSize)
        {
            var cameraObject = new GameObject("Main Camera") { tag = "MainCamera" };
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = orthographicSize;
            camera.backgroundColor = new Color(0.025f, 0.035f, 0.055f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            cameraObject.AddComponent<AudioListener>();
            return camera;
        }

        private static OfficePlayerController CreatePlayer(Camera camera, Vector2 spawn, Vector2 minimum, Vector2 maximum)
        {
            Texture2D sheet = AssetDatabase.LoadAssetAtPath<Texture2D>(CharacterPath);
            Sprite sprite = AssetDatabase.LoadAllAssetsAtPath(CharacterPath)
                .OfType<Sprite>()
                .FirstOrDefault(item => item.name == "Premade_Character_01_0");
            if (sheet == null || sprite == null)
            {
                throw new InvalidOperationException($"Personagem não encontrado em {CharacterPath}");
            }

            var playerObject = new GameObject("Desenvolvedor - Alex");
            playerObject.transform.position = spawn;
            SpriteRenderer renderer = playerObject.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = 25;
            CharacterWalkAnimator animator = playerObject.AddComponent<CharacterWalkAnimator>();
            animator.Configure(sheet);
            CapsuleCollider2D collider = playerObject.AddComponent<CapsuleCollider2D>();
            collider.size = new Vector2(0.6f, 0.72f);
            collider.offset = new Vector2(0.5f, 0.36f);
            OfficePlayerController player = playerObject.AddComponent<OfficePlayerController>();
            player.Configure(camera, minimum, maximum);
            TextMesh badgeText = CreateWorldLabel(playerObject.transform, "ALEX • ESTAGIÁRIO", new Vector3(0.5f, 2.05f), Accent, 40, 0.095f);
            playerObject.AddComponent<CareerNameBadge>().Configure(badgeText);
            return player;
        }

        private static void CreateCoworker(string label, string spriteFile, Vector2 position)
        {
            string path = CharacterPath.Replace("Premade_Character_01.png", spriteFile);
            Sprite sprite = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().FirstOrDefault();
            if (sprite == null)
            {
                return;
            }

            var coworker = new GameObject(label);
            coworker.transform.position = position;
            SpriteRenderer renderer = coworker.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = 20;
            CreateWorldLabel(coworker.transform, label, new Vector3(0.5f, 1.9f), new Color(0.82f, 0.86f, 0.94f), 38, 0.078f);
        }

        private static void CreateStation(string name, OfficeActionType type, Vector2 position, Vector2 colliderSize, Vector2 interactionOffset)
        {
            var stationObject = new GameObject(name);
            stationObject.transform.position = position;
            BoxCollider2D collider = stationObject.AddComponent<BoxCollider2D>();
            collider.size = colliderSize;
            OfficeStation station = stationObject.AddComponent<OfficeStation>();
            station.Configure(type, interactionOffset);
        }

        private static void CreateHud(CareerGame game, OfficePlayerController player)
        {
            var canvasObject = new GameObject("Interface da carreira");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = true;
            canvasObject.AddComponent<GraphicRaycaster>();
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = 0.5f;

            Image careerPanel = CreatePanel(canvasObject.transform, "Carreira", new Vector2(18f, -18f), new Vector2(280f, 108f), new Vector2(0f, 1f), Accent);
            CreateSectionHeader(careerPanel.transform, "CARREIRA // DEV CARRER", Accent);
            Text rank = CreateText(careerPanel.transform, "ESTAGIÁRIO", 23, TextAnchor.MiddleLeft, Accent, FontStyle.Bold);
            SetRect(rank.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(16f, -50f), new Vector2(-32f, 30f));
            Text promotion = CreateText(careerPanel.transform, "XP 0/250 PARA PROMOÇÃO", 12, TextAnchor.MiddleLeft, Muted);
            SetRect(promotion.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(16f, -77f), new Vector2(-32f, 20f));
            Image promotionFill = CreateBar(careerPanel.transform, new Vector2(16f, -98f), new Vector2(248f, 7f), Accent);

            Image topPanel = CreatePanel(canvasObject.transform, "Expediente", new Vector2(0f, -18f), new Vector2(390f, 92f), new Vector2(0.5f, 1f), new Color(0.30f, 0.66f, 0.92f));
            CreateSectionHeader(topPanel.transform, "EXPEDIENTE", new Color(0.30f, 0.66f, 0.92f), true);
            Text clock = CreateText(topPanel.transform, "DIA 1  //  09:00", 22, TextAnchor.MiddleCenter, Color.white, FontStyle.Bold);
            SetRect(clock.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(16f, -49f), new Vector2(-32f, 28f));
            Text salary = CreateText(topPanel.transform, "SALÁRIO R$ 1.800   •   CAIXA R$ 120", 12, TextAnchor.MiddleCenter, new Color(0.56f, 0.90f, 0.62f));
            SetRect(salary.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(16f, -71f), new Vector2(-32f, 18f));
            Image workdayFill = CreateBar(topPanel.transform, new Vector2(20f, -84f), new Vector2(350f, 6f), new Color(0.30f, 0.66f, 0.92f));

            Image statsPanel = CreatePanel(canvasObject.transform, "Desempenho", new Vector2(18f, -140f), new Vector2(280f, 250f), new Vector2(0f, 1f), new Color(0.35f, 0.72f, 0.95f));
            CreateSectionHeader(statsPanel.transform, "DESEMPENHO DO DIA", new Color(0.35f, 0.72f, 0.95f));
            Text delivery = CreateText(statsPanel.transform, "ENTREGA  0/100", 14, TextAnchor.MiddleLeft, Color.white, FontStyle.Bold);
            SetRect(delivery.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(16f, -48f), new Vector2(-32f, 22f));
            Image deliveryFill = CreateBar(statsPanel.transform, new Vector2(16f, -70f), new Vector2(248f, 10f), new Color(0.26f, 0.72f, 0.92f));
            Text energy = CreateText(statsPanel.transform, "ENERGIA  100%", 13, TextAnchor.MiddleLeft, Color.white, FontStyle.Bold);
            SetRect(energy.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(16f, -98f), new Vector2(-32f, 20f));
            Image energyFill = CreateBar(statsPanel.transform, new Vector2(16f, -117f), new Vector2(248f, 8f), new Color(0.35f, 0.86f, 0.48f));
            Text stress = CreateText(statsPanel.transform, "ESTRESSE  8%", 13, TextAnchor.MiddleLeft, Color.white, FontStyle.Bold);
            SetRect(stress.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(16f, -145f), new Vector2(-32f, 20f));
            Image stressFill = CreateBar(statsPanel.transform, new Vector2(16f, -164f), new Vector2(248f, 8f), new Color(0.40f, 0.75f, 0.95f));
            Text knowledge = CreateStatChip(statsPanel.transform, "CONHECIMENTO\n0", new Vector2(16f, 16f), new Color(0.68f, 0.58f, 0.96f));
            Text reputation = CreateStatChip(statsPanel.transform, "REPUTAÇÃO\n0", new Vector2(144f, 16f), Amber);

            Image objectivePanel = CreatePanel(canvasObject.transform, "Sprint do dia", new Vector2(-18f, -18f), new Vector2(260f, 132f), Vector2.one, Amber);
            CreateSectionHeader(objectivePanel.transform, "SPRINT DO DIA", Amber);
            Text objective = CreateText(objectivePanel.transform, "100 PONTOS RESTANTES\n8h00 disponíveis", 15, TextAnchor.MiddleLeft, Color.white, FontStyle.Bold);
            objective.lineSpacing = 1.25f;
            SetRect(objective.rectTransform, Vector2.zero, Vector2.one, new Vector2(16f, 12f), new Vector2(-32f, -48f));

            Image helpPanel = CreatePanel(canvasObject.transform, "Atalhos", new Vector2(-18f, -164f), new Vector2(260f, 154f), Vector2.one, new Color(0.55f, 0.46f, 0.90f));
            CreateSectionHeader(helpPanel.transform, "AÇÕES DO ESCRITÓRIO", new Color(0.55f, 0.46f, 0.90f));
            Text help = CreateText(helpPanel.transform, "PROGRAMAR  entrega\nREUNIÃO  reputação\nESTUDAR  conhecimento\nCAFÉ  energia\nPAUSA  reduz estresse", 13, TextAnchor.UpperLeft, new Color(0.84f, 0.88f, 0.95f));
            help.lineSpacing = 1.25f;
            SetRect(help.rectTransform, Vector2.zero, Vector2.one, new Vector2(16f, 14f), new Vector2(-32f, -51f));

            Image bottomPanel = CreatePanel(canvasObject.transform, "Central de ações", new Vector2(0f, 18f), new Vector2(760f, 80f), new Vector2(0.5f, 0f), Accent);
            Text statusTitle = CreateText(bottomPanel.transform, "STATUS ATUAL", 10, TextAnchor.MiddleLeft, Accent, FontStyle.Bold);
            SetRect(statusTitle.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(18f, -19f), new Vector2(390f, 16f));
            statusTitle.rectTransform.pivot = new Vector2(0f, 0.5f);
            Text status = CreateText(bottomPanel.transform, "Escolha onde começar o expediente.", 14, TextAnchor.MiddleLeft, new Color(0.82f, 0.92f, 0.96f), FontStyle.Bold);
            SetRect(status.rectTransform, Vector2.zero, Vector2.zero, new Vector2(18f, 9f), new Vector2(410f, 40f));
            status.rectTransform.pivot = Vector2.zero;
            Button endDay = CreateButton(bottomPanel.transform, "RESGATAR META", new Vector2(-174f, 16f), new Vector2(138f, 46f), new Vector2(1f, 0f), Accent);
            Button save = CreateButton(bottomPanel.transform, "SALVAR", new Vector2(-100f, 16f), new Vector2(68f, 46f), new Vector2(1f, 0f), new Color(0.22f, 0.36f, 0.52f));
            Button load = CreateButton(bottomPanel.transform, "CARREGAR", new Vector2(-16f, 16f), new Vector2(78f, 46f), new Vector2(1f, 0f), new Color(0.22f, 0.36f, 0.52f));

            CareerHud hud = canvasObject.AddComponent<CareerHud>();
            hud.Configure(game, player, rank, promotion, promotionFill, clock, salary, workdayFill, delivery, deliveryFill, energy, energyFill, stress, stressFill, knowledge, reputation, status, objective, endDay, save, load);
        }

        private static Image CreateBar(Transform parent, Vector2 position, Vector2 size, Color fillColor)
        {
            Image background = CreateImage(parent, "Trilho", new Color(0.015f, 0.025f, 0.045f, 0.95f));
            SetRect(background.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), position, size);
            background.rectTransform.pivot = new Vector2(0f, 0.5f);
            Image fill = CreateImage(background.transform, "Preenchimento", fillColor);
            SetRect(fill.rectTransform, Vector2.zero, Vector2.one, new Vector2(2f, 2f), new Vector2(-4f, -4f));
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillAmount = 1f;
            return fill;
        }

        private static Image CreatePanel(Transform parent, string name, Vector2 position, Vector2 size, Vector2 anchor, Color accent)
        {
            Image shadow = CreateImage(parent, $"Sombra • {name}", new Color(0f, 0f, 0f, 0.46f));
            RectTransform shadowRect = shadow.rectTransform;
            shadowRect.anchorMin = anchor;
            shadowRect.anchorMax = anchor;
            shadowRect.pivot = anchor;
            shadowRect.anchoredPosition = position + new Vector2(4f, -5f);
            shadowRect.sizeDelta = size;

            Image panel = CreateImage(parent, name, Panel);
            RectTransform rect = panel.rectTransform;
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            Outline outline = panel.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(accent.r, accent.g, accent.b, 0.46f);
            outline.effectDistance = new Vector2(1f, -1f);
            outline.useGraphicAlpha = false;

            Image topLine = CreateImage(panel.transform, "Linha de destaque", accent);
            SetRect(topLine.rectTransform, new Vector2(0f, 1f), Vector2.one, Vector2.zero, new Vector2(0f, 3f));
            topLine.rectTransform.pivot = new Vector2(0.5f, 1f);
            return panel;
        }

        private static void CreateSectionHeader(Transform parent, string value, Color color, bool centered = false)
        {
            Text header = CreateText(parent, value, 10, centered ? TextAnchor.MiddleCenter : TextAnchor.MiddleLeft, color, FontStyle.Bold);
            SetRect(header.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(14f, -20f), new Vector2(-28f, 18f));
        }

        private static Text CreateStatChip(Transform parent, string value, Vector2 position, Color color)
        {
            Image chip = CreateImage(parent, value.Split('\n')[0], new Color(color.r * 0.16f, color.g * 0.16f, color.b * 0.16f, 0.94f));
            SetRect(chip.rectTransform, Vector2.zero, Vector2.zero, position, new Vector2(120f, 54f));
            chip.rectTransform.pivot = Vector2.zero;
            Outline outline = chip.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(color.r, color.g, color.b, 0.35f);
            outline.effectDistance = Vector2.one;
            Text text = CreateText(chip.transform, value, 11, TextAnchor.MiddleCenter, color, FontStyle.Bold);
            text.lineSpacing = 1.1f;
            SetRect(text.rectTransform, Vector2.zero, Vector2.one, new Vector2(4f, 3f), new Vector2(-8f, -6f));
            return text;
        }

        private static Image CreateImage(Transform parent, string name, Color color)
        {
            var imageObject = new GameObject(name, typeof(RectTransform));
            imageObject.transform.SetParent(parent, false);
            Image image = imageObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static Text CreateText(Transform parent, string value, int fontSize, TextAnchor alignment, Color color, FontStyle style = FontStyle.Normal)
        {
            var textObject = new GameObject(value.Split('\n')[0], typeof(RectTransform));
            textObject.transform.SetParent(parent, false);
            Text text = textObject.AddComponent<Text>();
            text.text = value;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.alignment = alignment;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.raycastTarget = false;
            Shadow shadow = textObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.72f);
            shadow.effectDistance = new Vector2(1f, -1f);
            return text;
        }

        private static Button CreateButton(Transform parent, string label, Vector2 position, Vector2 size, Vector2 anchor, Color color)
        {
            Image image = CreateImage(parent, label, color);
            RectTransform rect = image.rectTransform;
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            image.raycastTarget = true;
            Outline outline = image.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(color.r, color.g, color.b, 0.72f);
            outline.effectDistance = new Vector2(1f, -1f);
            Button button = image.gameObject.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.highlightedColor = Color.Lerp(color, Color.white, 0.18f);
            colors.pressedColor = Color.Lerp(color, Color.black, 0.18f);
            button.colors = colors;
            colors.disabledColor = new Color(0.16f, 0.19f, 0.24f, 0.72f);
            button.colors = colors;
            Text text = CreateText(image.transform, label, 12, TextAnchor.MiddleCenter, Color.white, FontStyle.Bold);
            SetRect(text.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            return button;
        }

        private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private static TextMesh CreateWorldLabel(Transform parent, string value, Vector3 position, Color color, int order, float characterSize)
        {
            var labelObject = new GameObject(value);
            labelObject.transform.SetParent(parent, false);
            labelObject.transform.localPosition = position;
            TextMesh text = labelObject.AddComponent<TextMesh>();
            text.text = value;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 44;
            text.characterSize = characterSize;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.color = color;
            MeshRenderer renderer = text.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = text.font.material;
            renderer.sortingOrder = order;
            return text;
        }

        private static void CreateEventSystem()
        {
            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<InputSystemUIInputModule>();
        }
    }
}
