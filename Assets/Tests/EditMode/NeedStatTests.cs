using NUnit.Framework;
using ProjetoJogosDigitais.Gameplay;
using ProjetoJogosDigitais.Presentation;
using UnityEngine;

namespace ProjetoJogosDigitais.Tests
{
    public sealed class NeedStatTests
    {
        [Test]
        public void Tick_DecreasesNeedAccordingToElapsedTime()
        {
            var hunger = new NeedStat(0.8f, 0.1f);

            hunger.Tick(2f);

            Assert.That(hunger.Current, Is.EqualTo(0.6f).Within(0.0001f));
        }

        [Test]
        public void Restore_ClampsNeedAtOne()
        {
            var hunger = new NeedStat(0.8f, 0.1f);

            hunger.Restore(0.5f);

            Assert.That(hunger.Current, Is.EqualTo(1f));
        }

        [Test]
        public void Tick_NeverMakesNeedNegative()
        {
            var hunger = new NeedStat(0.05f, 1f);

            hunger.Tick(10f);

            Assert.That(hunger.Current, Is.EqualTo(0f));
        }

        [Test]
        public void FurnitureCatalog_HasPositivePricesAndSellValues()
        {
            foreach (FurnitureType type in System.Enum.GetValues(typeof(FurnitureType)))
            {
                int price = FurnitureItem.PriceFor(type);
                Assert.That(price, Is.GreaterThan(0));
                Assert.That(Mathf.RoundToInt(price * 0.55f), Is.LessThan(price));
            }
        }

        [Test]
        public void NeighborhoodLayout_AcceptsHouseInteriorAndRejectsRoad()
        {
            Assert.That(NeighborhoodLayout.IsInsideHouse(NeighborhoodLayout.HouseCenters[0]), Is.True);
            Assert.That(NeighborhoodLayout.IsInsideHouse(Vector2.zero), Is.False);
        }
    }
}
