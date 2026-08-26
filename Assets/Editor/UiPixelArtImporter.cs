using System;
using UnityEditor;
using UnityEngine;

namespace ProjetoJogosDigitais.Editor
{
    /// <summary>Keeps the generated UI illustrations sharp and preserves their transparent edges.</summary>
    public sealed class UiPixelArtImporter : AssetPostprocessor
    {
        private const string ArtRoot = "Assets/Art/UI/Pixel/";

        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(ArtRoot, StringComparison.Ordinal))
            {
                return;
            }

            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100f;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.alphaIsTransparency = true;
        }
    }
}
