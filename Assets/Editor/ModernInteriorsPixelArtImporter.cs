using System;
using UnityEditor;
using UnityEngine;

namespace ProjetoJogosDigitais.Editor
{
    /// <summary>
    /// Keeps the curated Modern Interiors artwork crisp and consistently scaled.
    /// Sprite sheets stay in Multiple mode so they can be sliced per animation or tile set.
    /// </summary>
    public sealed class ModernInteriorsPixelArtImporter : AssetPostprocessor
    {
        private const string ArtRoot = "Assets/Art/ModernInteriors16/";

        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(ArtRoot, StringComparison.Ordinal))
            {
                return;
            }

            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = IsCompleteHomePreview(assetPath)
                ? SpriteImportMode.Single
                : SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = 16;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.alphaIsTransparency = true;
        }

        private static bool IsCompleteHomePreview(string path)
        {
            return path.Contains("/HomeDesigns/", StringComparison.Ordinal) &&
                   path.Contains("preview", StringComparison.OrdinalIgnoreCase);
        }
    }
}
