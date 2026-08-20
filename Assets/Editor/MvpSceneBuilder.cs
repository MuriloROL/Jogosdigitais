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
    public static class MvpSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/MVP.unity";
        private const string CharacterRoot = "Assets/Art/ModernInteriors16/Characters/Premade/";
        private static readonly Vector2 WorldMin = new(-25f, -18f);
        private static readonly Vector2 WorldMax = new(25f, 18f);
        private const string HomeRoot = "Assets/Art/ModernInteriors16/HomeDesigns/";
        private const string AnimatedRoot = "Assets/Art/ModernInteriors16/AnimatedObjects/";

        [MenuItem("Projeto/Construir MVP do bairro")]
        public static void Build()
        {
            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
            }

            PrepareArtImports();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "MVP";

            Camera camera = CreateCamera();
            CreateLighting();

            var neighborhood = new GameObject("Bairro Vila Serena");
            NeighborhoodVisuals neighborhoodVisuals = neighborhood.AddComponent<NeighborhoodVisuals>();
            neighborhoodVisuals.Configure(LoadHouseMaps());

            var systems = new GameObject("Sistemas do jogo");
            GameClock clock = systems.AddComponent<GameClock>();
            GameSession session = systems.AddComponent<GameSession>();
            BuildModeController buildMode = systems.AddComponent<BuildModeController>();
            FurnitureVisualCatalog visualCatalog = systems.AddComponent<FurnitureVisualCatalog>();
            visualCatalog.Configure(
                LoadSprites(AnimatedRoot + "animated_fridge.png"),
                new[] { LoadSprite("Assets/Art/ModernInteriors16/Interiors/4_Bedroom_16x16.png", "4_Bedroom_16x16_4") },
                LoadSprites(AnimatedRoot + "animated_bathtub.png"),
                LoadSprites(AnimatedRoot + "animated_old_tv.png"));

            SimController player = CreateSim(
                "Alex",
                true,
                camera,
                "Premade_Character_01.png",
                NeighborhoodLayout.HouseCenters[0] + new Vector2(-0.8f, -0.4f));

            string[] npcNames = { "Nina", "Theo", "Maya", "Davi" };
            string[] npcSprites =
            {
                "Premade_Character_02.png",
                "Premade_Character_03.png",
                "Premade_Character_04.png",
                "Premade_Character_05.png"
            };
            Vector2[] npcSpawnOffsets =
            {
                new(2.2f, -0.5f),
                new(0f, -0.5f),
                new(0f, -0.4f),
                new(0f, -0.2f)
            };
            for (int index = 0; index < npcNames.Length; index++)
            {
                Vector2 house = NeighborhoodLayout.HouseCenters[index % NeighborhoodLayout.HouseCenters.Length];
                SimController npc = CreateSim(
                    npcNames[index],
                    false,
                    camera,
                    npcSprites[index],
                    house + npcSpawnOffsets[index]);
                npc.gameObject.AddComponent<NpcBrain>().Configure(house, NeighborhoodLayout.HouseExtents[index]);
            }

            camera.GetComponent<CameraFollow2D>().Configure(player.transform, new Vector2(-19f, -12f), new Vector2(19f, 12f));
            session.Configure(player, clock);
            CreateInitialFurniture(visualCatalog);
            CreateHud(camera, player, session, clock, buildMode);
            CreateEventSystem();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeGameObject = player.gameObject;
            Debug.Log($"[MVP] Bairro jogável criado em {ScenePath}");
        }

        private static Camera CreateCamera()
        {
            var cameraObject = new GameObject("Main Camera") { tag = "MainCamera" };
            cameraObject.transform.position = new Vector3(-14f, 10f, -10f);
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5.8f;
            camera.backgroundColor = new Color(0.11f, 0.12f, 0.16f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            cameraObject.AddComponent<AudioListener>();
            cameraObject.AddComponent<CameraFollow2D>();
            return camera;
        }

        private static void CreateLighting()
        {
            var lightObject = new GameObject("Global Light 2D");
            Type lightType = Type.GetType("UnityEngine.Rendering.Universal.Light2D, Unity.RenderPipelines.Universal.Runtime");
            if (lightType == null)
            {
                return;
            }

            Component light = lightObject.AddComponent(lightType);
            var serializedLight = new SerializedObject(light);
            SerializedProperty lightKind = serializedLight.FindProperty("m_LightType");
            if (lightKind != null)
            {
                lightKind.intValue = 4;
            }
            serializedLight.ApplyModifiedPropertiesWithoutUndo();
        }

        private static SimController CreateSim(
            string displayName,
            bool player,
            Camera camera,
            string spriteFile,
            Vector2 position)
        {
            string path = CharacterRoot + spriteFile;
            string baseName = spriteFile.Replace(".png", string.Empty);
            Sprite sprite = AssetDatabase.LoadAllAssetsAtPath(path)
                .OfType<Sprite>()
                .FirstOrDefault(candidate => candidate.name == baseName + "_0");
            if (sprite == null)
            {
                throw new InvalidOperationException($"Personagem não encontrado: {path}");
            }

            var simObject = new GameObject(player ? "Jogador - Alex" : $"NPC - {displayName}");
            simObject.transform.position = position;
            SpriteRenderer renderer = simObject.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = 25;
            CharacterWalkAnimator walkAnimator = simObject.AddComponent<CharacterWalkAnimator>();
            walkAnimator.Configure(AssetDatabase.LoadAssetAtPath<Texture2D>(path));

            var collider = simObject.AddComponent<CapsuleCollider2D>();
            collider.size = new Vector2(0.58f, 0.68f);
            collider.offset = new Vector2(0.5f, 0.35f);

            simObject.AddComponent<SimNeeds>();
            SimController controller = simObject.AddComponent<SimController>();
            controller.Configure(displayName, player, camera, WorldMin, WorldMax);
            simObject.AddComponent<SocialInteractable>();
            CreateNameTag(simObject.transform, displayName, player);

            if (player)
            {
                simObject.AddComponent<PlayerMarkerVisual>();
            }

            return controller;
        }

        private static void CreateNameTag(Transform parent, string displayName, bool player)
        {
            CreateWorldText(parent, displayName + " sombra", displayName, new Vector3(0.53f, 1.92f, 0f), Color.black, 39);
            CreateWorldText(
                parent,
                displayName + " nome",
                displayName,
                new Vector3(0.5f, 1.96f, 0f),
                player ? new Color(0.45f, 1f, 0.68f) : Color.white,
                40);
        }

        private static void CreateWorldText(
            Transform parent,
            string objectName,
            string value,
            Vector3 position,
            Color color,
            int sortingOrder)
        {
            var textObject = new GameObject(objectName);
            textObject.transform.SetParent(parent, false);
            textObject.transform.localPosition = position;
            TextMesh text = textObject.AddComponent<TextMesh>();
            text.text = value;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 48;
            text.characterSize = 0.12f;
            text.anchor = TextAnchor.LowerCenter;
            text.alignment = TextAlignment.Center;
            text.color = color;
            MeshRenderer renderer = textObject.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = text.font.material;
            renderer.sortingOrder = sortingOrder;
        }

        private static void CreateInitialFurniture(FurnitureVisualCatalog catalog)
        {
            Vector2[][] furnitureOffsets =
            {
                new[] { new Vector2(4.2f, 1.5f), new Vector2(-4.2f, 1.5f), new Vector2(4.1f, -2.6f), new Vector2(-3.4f, -2.5f) },
                new[] { new Vector2(4.2f, 1.5f), new Vector2(-4.2f, 1.5f), new Vector2(4.1f, -2.6f), new Vector2(-3.4f, -2.5f) },
                new[] { new Vector2(4.2f, 1.5f), new Vector2(-4.2f, 1.5f), new Vector2(4.1f, -2.6f), new Vector2(-3.4f, -2.5f) },
                new[] { new Vector2(4.2f, 0.9f), new Vector2(-4.2f, 0.9f), new Vector2(4.1f, -0.8f), new Vector2(-3.4f, -1.2f) }
            };

            for (int index = 0; index < NeighborhoodLayout.HouseCenters.Length; index++)
            {
                Vector2 center = NeighborhoodLayout.HouseCenters[index];
                Vector2[] offsets = furnitureOffsets[index];
                FurnitureFactory.Create(FurnitureType.Fridge, center + offsets[0], false, $"house-{index}-fridge", catalog.Get(FurnitureType.Fridge));
                FurnitureFactory.Create(FurnitureType.Bed, center + offsets[1], false, $"house-{index}-bed", catalog.Get(FurnitureType.Bed));
                FurnitureFactory.Create(FurnitureType.Shower, center + offsets[2], false, $"house-{index}-shower", catalog.Get(FurnitureType.Shower));
                FurnitureFactory.Create(FurnitureType.Armchair, center + offsets[3], false, $"house-{index}-tv", catalog.Get(FurnitureType.Armchair));
            }
        }

        private static void CreateHud(
            Camera camera,
            SimController player,
            GameSession session,
            GameClock clock,
            BuildModeController buildMode)
        {
            var canvasObject = new GameObject("HUD do MVP");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<GraphicRaycaster>();
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = 0.5f;

            Image needsPanel = CreatePanel(canvasObject.transform, "Necessidades", new Vector2(18f, -18f), new Vector2(330f, 270f), new Vector2(0f, 1f));
            Text title = CreateText(needsPanel.transform, "ALEX", 24, TextAnchor.MiddleLeft, new Color(0.96f, 0.88f, 0.66f));
            SetRect(title.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(16f, -38f), new Vector2(-32f, 32f));

            var fills = new Image[4];
            var labels = new Text[4];
            string[] needNames = { "Fome", "Energia", "Higiene", "Social" };
            for (int index = 0; index < 4; index++)
            {
                float y = -73f - index * 43f;
                labels[index] = CreateText(needsPanel.transform, needNames[index], 16, TextAnchor.MiddleLeft, Color.white);
                SetRect(labels[index].rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, y), new Vector2(112f, 24f));
                labels[index].rectTransform.pivot = new Vector2(0f, 0.5f);

                Image background = CreateImage(needsPanel.transform, $"Barra {needNames[index]}", new Color(0.04f, 0.05f, 0.07f));
                SetRect(background.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(126f, y), new Vector2(184f, 16f));
                background.rectTransform.pivot = new Vector2(0f, 0.5f);
                fills[index] = CreateImage(background.transform, "Preenchimento", Color.green);
                SetRect(fills[index].rectTransform, Vector2.zero, Vector2.one, new Vector2(2f, 2f), new Vector2(-4f, -4f));
                fills[index].type = Image.Type.Filled;
                fills[index].fillMethod = Image.FillMethod.Horizontal;
                fills[index].fillAmount = 0.75f;
            }

            Text relationship = CreateText(needsPanel.transform, "Amizades: conheça os vizinhos", 15, TextAnchor.MiddleLeft, new Color(0.66f, 0.88f, 0.94f));
            SetRect(relationship.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(16f, 8f), new Vector2(-32f, 32f));
            relationship.rectTransform.pivot = new Vector2(0f, 0f);

            Image topPanel = CreatePanel(canvasObject.transform, "Tempo e dinheiro", new Vector2(0f, -18f), new Vector2(430f, 76f), new Vector2(0.5f, 1f));
            Text clockText = CreateText(topPanel.transform, "Dia 1  08:00", 20, TextAnchor.MiddleLeft, Color.white);
            SetRect(clockText.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(14f, 0f), new Vector2(160f, 0f));
            clockText.rectTransform.pivot = new Vector2(0f, 0.5f);
            Text moneyText = CreateText(topPanel.transform, "§ 2500", 20, TextAnchor.MiddleRight, new Color(0.55f, 0.95f, 0.58f));
            SetRect(moneyText.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(-14f, 0f), new Vector2(100f, 0f));
            moneyText.rectTransform.pivot = new Vector2(1f, 0.5f);

            CreateButton(topPanel.transform, "❚❚", new Vector2(180f, 20f), new Vector2(42f, 36f), () => clock.SetSpeed(0f));
            CreateButton(topPanel.transform, "▶", new Vector2(226f, 20f), new Vector2(42f, 36f), () => clock.SetSpeed(1f));
            CreateButton(topPanel.transform, "2×", new Vector2(272f, 20f), new Vector2(42f, 36f), () => clock.SetSpeed(2f));

            Image bottomPanel = CreatePanel(canvasObject.transform, "Ações", new Vector2(18f, 18f), new Vector2(720f, 104f), Vector2.zero);
            Text status = CreateText(bottomPanel.transform, "Aguardando uma ação", 18, TextAnchor.MiddleLeft, new Color(0.65f, 0.90f, 0.96f));
            SetRect(status.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(16f, -32f), new Vector2(-32f, 28f));
            Text queue = CreateText(bottomPanel.transform, "Fila: vazia", 15, TextAnchor.MiddleLeft, Color.white);
            SetRect(queue.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(16f, 10f), new Vector2(-32f, 30f));
            queue.rectTransform.pivot = new Vector2(0f, 0f);

            Button furnitureButton = CreateButton(canvasObject.transform, "MÓVEIS  [B]", new Vector2(-184f, 20f), new Vector2(168f, 52f), buildMode.Toggle, new Vector2(1f, 0f));
            Button saveButton = CreateButton(canvasObject.transform, "SALVAR", new Vector2(-184f, 78f), new Vector2(80f, 42f), session.SaveGame, new Vector2(1f, 0f));
            Button loadButton = CreateButton(canvasObject.transform, "CARREGAR", new Vector2(-98f, 78f), new Vector2(82f, 42f), session.LoadGame, new Vector2(1f, 0f));

            Image helpPanel = CreatePanel(canvasObject.transform, "Ajuda", new Vector2(-18f, -104f), new Vector2(410f, 86f), Vector2.one);
            Text help = CreateText(helpPanel.transform, "Clique: andar/interagir\nShift + clique: adicionar à fila\nConverse com os vizinhos", 15, TextAnchor.MiddleLeft, Color.white);
            SetRect(help.rectTransform, Vector2.zero, Vector2.one, new Vector2(14f, 8f), new Vector2(-28f, -16f));

            Image buildPanel = CreatePanel(canvasObject.transform, "Catálogo de móveis", new Vector2(-18f, -18f), new Vector2(350f, 420f), Vector2.one);
            Text buildTitle = CreateText(buildPanel.transform, "CATÁLOGO DE MÓVEIS", 22, TextAnchor.MiddleCenter, new Color(0.96f, 0.88f, 0.66f));
            SetRect(buildTitle.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(12f, -52f), new Vector2(-24f, 40f));
            Text selection = CreateText(buildPanel.transform, "Selecione um móvel", 15, TextAnchor.MiddleCenter, Color.white);
            SetRect(selection.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(12f, 64f), new Vector2(-24f, 54f));

            CreateCatalogButton(buildPanel.transform, "Geladeira  §650", 0, () => buildMode.BeginPurchase(FurnitureType.Fridge));
            CreateCatalogButton(buildPanel.transform, "Cama  §480", 1, () => buildMode.BeginPurchase(FurnitureType.Bed));
            CreateCatalogButton(buildPanel.transform, "Chuveiro  §420", 2, () => buildMode.BeginPurchase(FurnitureType.Shower));
            CreateCatalogButton(buildPanel.transform, "TV  §220", 3, () => buildMode.BeginPurchase(FurnitureType.Armchair));
            CreateButton(buildPanel.transform, "MOVER", new Vector2(18f, 90f), new Vector2(98f, 42f), buildMode.BeginMoveSelected);
            CreateButton(buildPanel.transform, "VENDER", new Vector2(126f, 90f), new Vector2(98f, 42f), buildMode.SellSelected);
            CreateButton(buildPanel.transform, "FECHAR", new Vector2(234f, 90f), new Vector2(98f, 42f), buildMode.Close);

            buildMode.Configure(camera, session, clock, buildPanel.gameObject, selection);
            MvpHud hud = canvasObject.AddComponent<MvpHud>();
            hud.Configure(player, session, clock, fills, labels, status, queue, clockText, moneyText, relationship);
        }

        private static void CreateCatalogButton(Transform parent, string label, int row, UnityEngine.Events.UnityAction action)
        {
            CreateButton(parent, label, new Vector2(24f, 304f - row * 55f), new Vector2(302f, 46f), action);
        }

        private static void CreateEventSystem()
        {
            var eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<InputSystemUIInputModule>();
        }

        private static Image CreatePanel(Transform parent, string name, Vector2 position, Vector2 size, Vector2 anchor)
        {
            Image panel = CreateImage(parent, name, new Color(0.09f, 0.11f, 0.16f, 0.94f));
            SetRect(panel.rectTransform, anchor, anchor, position, size);
            panel.rectTransform.pivot = anchor;
            return panel;
        }

        private static Image CreateImage(Transform parent, string name, Color color)
        {
            var imageObject = new GameObject(name, typeof(RectTransform));
            imageObject.transform.SetParent(parent, false);
            Image image = imageObject.AddComponent<Image>();
            image.color = color;
            return image;
        }

        private static Text CreateText(Transform parent, string value, int size, TextAnchor alignment, Color color)
        {
            var textObject = new GameObject(value, typeof(RectTransform));
            textObject.transform.SetParent(parent, false);
            Text text = textObject.AddComponent<Text>();
            text.text = value;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size;
            text.alignment = alignment;
            text.color = color;
            text.raycastTarget = false;
            return text;
        }

        private static Button CreateButton(
            Transform parent,
            string label,
            Vector2 position,
            Vector2 size,
            UnityEngine.Events.UnityAction action,
            Vector2? anchor = null)
        {
            Vector2 usedAnchor = anchor ?? Vector2.zero;
            Image background = CreateImage(parent, label, new Color(0.22f, 0.31f, 0.39f, 0.98f));
            SetRect(background.rectTransform, usedAnchor, usedAnchor, position, size);
            background.rectTransform.pivot = usedAnchor;
            Button button = background.gameObject.AddComponent<Button>();
            button.targetGraphic = background;
            button.onClick.AddListener(action);
            Text text = CreateText(background.transform, label, 15, TextAnchor.MiddleCenter, Color.white);
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

        private static Sprite[] LoadHouseMaps()
        {
            return HouseMapPaths()
                .Select(AssetDatabase.LoadAssetAtPath<Sprite>)
                .ToArray();
        }

        private static void PrepareArtImports()
        {
            foreach (string path in HouseMapPaths())
            {
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }
        }

        private static string[] HouseMapPaths()
        {
            return new[]
            {
                HomeRoot + "Generic_Home_Designs/Generic_Home_1_preview_16x16.png",
                HomeRoot + "Japanese_Interiors_Home_Designs/Japanese_Home_1_preview_16x16.png",
                HomeRoot + "Condominium_Designs/Condominium_Design_preview.png",
                HomeRoot + "Condominium_Designs/Condominium_Design_2_preview.png"
            };
        }

        private static Sprite LoadSprite(string path, string name)
        {
            Sprite sprite = AssetDatabase.LoadAllAssetsAtPath(path)
                .OfType<Sprite>()
                .FirstOrDefault(candidate => candidate.name == name);
            if (sprite == null)
            {
                throw new InvalidOperationException($"Sprite não encontrado: {name} em {path}");
            }
            return sprite;
        }

        private static Sprite[] LoadSprites(string path)
        {
            Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(path)
                .OfType<Sprite>()
                .OrderBy(sprite => ParseSuffix(sprite.name))
                .ToArray();
            if (sprites.Length == 0)
            {
                throw new InvalidOperationException($"Sprites não encontrados em {path}");
            }
            return sprites;
        }

        private static int ParseSuffix(string value)
        {
            int separator = value.LastIndexOf('_');
            return separator >= 0 && int.TryParse(value[(separator + 1)..], out int number)
                ? number
                : int.MaxValue;
        }
    }
}
