using System;
using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using ProjetoJogosDigitais.Gameplay;
using ProjetoJogosDigitais.Presentation;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace ProjetoJogosDigitais.Tests
{
    public sealed class OfficeMovementTests
    {
        private OfficePlayerController player;
        private CareerGame game;
        private string savePath;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            if (CareerGame.Current != null)
            {
                Object.Destroy(CareerGame.Current.gameObject);
                yield return null;
            }

            Time.timeScale = 1f;
            yield return SceneManager.LoadSceneAsync(CareerSceneFlow.InternScene);
            game = CareerGame.Current;
            savePath = Path.Combine(Application.temporaryCachePath, $"movement-test-{Guid.NewGuid():N}.json");
            game.SetSavePathOverride(savePath);
            game.enabled = false;
            player = Object.FindFirstObjectByType<OfficePlayerController>();
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (game != null)
            {
                Object.Destroy(game.gameObject);
            }
            yield return null;
            if (!string.IsNullOrEmpty(savePath) && File.Exists(savePath))
            {
                File.Delete(savePath);
            }
        }

        [UnityTest]
        public IEnumerator WalkPoseDependsOnDistanceInsteadOfElapsedFrames()
        {
            player.enabled = false;
            CharacterWalkAnimator animator = player.GetComponent<CharacterWalkAnimator>();
            SpriteRenderer renderer = player.GetComponent<SpriteRenderer>();
            animator.SetMotion(Vector2.zero);
            animator.SetMotion(Vector2.right * 0.62f);
            Sprite singleUpdatePose = renderer.sprite;

            animator.SetMotion(Vector2.zero);
            Sprite idlePose = renderer.sprite;
            for (int frame = 0; frame < 31; frame++)
            {
                animator.SetMotion(Vector2.right * 0.02f);
                yield return null;
            }

            Assert.That(renderer.sprite, Is.SameAs(singleUpdatePose),
                "O mesmo percurso deve produzir a mesma passada, independentemente do número de frames.");
            animator.SetMotion(Vector2.zero);
            Assert.That(renderer.sprite, Is.SameAs(idlePose));
            yield return null;
            Assert.That(renderer.sprite, Is.SameAs(idlePose), "Parado, Alex não deve continuar marchando.");
        }

        [UnityTest]
        public IEnumerator ShortWalkAndMidWalkRedirectStopExactlyAtTheTarget()
        {
            Vector3 start = player.transform.position;
            Vector3 shortTarget = start + Vector3.right * 0.04f;
            player.MoveTo(shortTarget);
            yield return WaitForArrival();
            Assert.That(Vector3.Distance(player.transform.position, shortTarget), Is.LessThan(0.0001f));

            player.MoveTo(start + Vector3.right * 2f);
            yield return new WaitForSeconds(0.2f);
            Assert.That(player.IsIdle, Is.False);
            Vector3 redirectedTarget = start + new Vector3(-0.7f, 0.7f, 0f);
            player.MoveTo(redirectedTarget);
            yield return WaitForArrival();
            Assert.That(Vector3.Distance(player.transform.position, redirectedTarget), Is.LessThan(0.0001f));
            Vector3 stoppedAt = player.transform.position;
            yield return new WaitForSeconds(0.2f);
            Assert.That(player.transform.position, Is.EqualTo(stoppedAt), "Não deve haver deriva depois da chegada.");
        }

        [UnityTest]
        public IEnumerator WalkingToDeskStillCompletesTheActivity()
        {
            OfficeStation desk = Object.FindObjectsByType<OfficeStation>(FindObjectsSortMode.None)
                .First(station => station.ActionType == OfficeActionType.Code);
            int delivery = game.Delivery;
            player.SelectStation(desk);
            yield return WaitForArrival();
            Assert.That(game.Delivery, Is.GreaterThan(delivery));
            Assert.That(player.GetComponent<SpriteRenderer>().sprite.name, Does.Contain("idle"));
        }

        private IEnumerator WaitForArrival()
        {
            float deadline = Time.realtimeSinceStartup + 10f;
            while (!player.IsIdle)
            {
                Assert.That(Time.realtimeSinceStartup, Is.LessThan(deadline), "Alex não chegou ao destino.");
                yield return null;
            }
        }
    }
}
