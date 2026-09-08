using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace ProjetoJogosDigitais.Tests
{
    public sealed class CharacterAssetTests
    {
        [Test]
        public void BuildScenesOnlyDependOnTheNewCharacterPack()
        {
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes.Where(scene => scene.enabled))
            {
                string[] dependencies = AssetDatabase.GetDependencies(scene.path, true);
                Assert.That(dependencies.Any(path => path.Contains("ModernInteriors")), Is.False, scene.path);
                if (!scene.path.EndsWith("MainMenu.unity"))
                {
                    Assert.That(dependencies, Does.Contain("Assets/Art/ELVCharacters/Char_003.png"), scene.path);
                    Assert.That(dependencies, Does.Contain("Assets/Art/ELVCharacters/Char_003_Idle.png"), scene.path);
                }
            }
        }

        [Test]
        public void AllSixCharactersHaveCompleteWalkAndIdleSheets()
        {
            for (int character = 1; character <= 6; character++)
            {
                foreach (string suffix in new[] { "", "_Idle" })
                {
                    string stem = $"Char_{character:000}{suffix}";
                    string path = $"Assets/Art/ELVCharacters/{stem}.png";
                    Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();
                    Assert.That(sprites, Has.Length.EqualTo(16), path);
                    for (int row = 0; row < 4; row++)
                    {
                        for (int frame = 0; frame < 4; frame++)
                        {
                            Sprite sprite = sprites.Single(item => item.name == $"{stem}_{row}_{frame}");
                            Assert.That(sprite.rect.size, Is.EqualTo(new Vector2(24f, 24f)));
                            Assert.That(sprite.texture.width, Is.EqualTo(96));
                            Assert.That(sprite.texture.height, Is.EqualTo(96));
                        }
                    }
                }
            }
        }
    }
}
