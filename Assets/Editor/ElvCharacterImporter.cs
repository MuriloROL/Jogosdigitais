using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ProjetoJogosDigitais.Presentation;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace ProjetoJogosDigitais.Editor
{
    public sealed class ElvCharacterImporter : AssetPostprocessor
    {
        private const string ArtRoot = "Assets/Art/ELVCharacters/";

        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(ArtRoot, StringComparison.Ordinal))
            {
                return;
            }

            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = CharacterWalkAnimator.PixelsPerUnit;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.alphaIsTransparency = true;

            var factories = new SpriteDataProviderFactories();
            factories.Init();
            var provider = factories.GetSpriteEditorDataProviderFromObject(importer);
            provider.InitSpriteEditorDataProvider();
            var existing = provider.GetSpriteRects().ToDictionary(rect => rect.name);
            var rects = new List<SpriteRect>();
            string stem = Path.GetFileNameWithoutExtension(assetPath);
            bool idle = stem.EndsWith("_Idle", StringComparison.Ordinal);
            int size = CharacterWalkAnimator.CellSize;
            for (int row = 0; row < 4; row++)
            {
                for (int column = 0; column < CharacterWalkAnimator.FramesPerDirection; column++)
                {
                    string name = $"{stem}_{row}_{column}";
                    rects.Add(new SpriteRect
                    {
                        name = name,
                        rect = new Rect(column * size, (3 - row) * size, size, size),
                        alignment = SpriteAlignment.Custom,
                        pivot = CharacterWalkAnimator.FramePivot(idle),
                        spriteID = existing.TryGetValue(name, out var previous) ? previous.spriteID : GUID.Generate()
                    });
                }
            }
            provider.SetSpriteRects(rects.ToArray());
            provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(
                rects.Select(rect => new SpriteNameFileIdPair(rect.name, rect.spriteID)));
            provider.Apply();
        }
    }
}
