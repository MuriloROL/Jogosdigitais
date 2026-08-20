using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ProjetoJogosDigitais.Gameplay
{
    public sealed class GameSession : MonoBehaviour
    {
        [Serializable]
        private sealed class SaveData
        {
            public int money;
            public int day;
            public float minutes;
            public List<SimData> sims = new();
            public List<FurnitureData> furniture = new();
            public List<RelationshipData> relationships = new();
        }

        [Serializable]
        private sealed class SimData
        {
            public string simName;
            public Vector3 position;
            public float[] needs;
        }

        [Serializable]
        private sealed class FurnitureData
        {
            public string saveId;
            public FurnitureType type;
            public Vector3 position;
            public bool purchased;
        }

        [Serializable]
        private sealed class RelationshipData
        {
            public string npcName;
            public int friendship;
        }

        [SerializeField] private int money = 2500;
        [SerializeField] private SimController player;
        [SerializeField] private GameClock clock;
        [SerializeField] private List<string> relationshipNames = new();
        [SerializeField] private List<int> relationshipValues = new();

        private string savePathOverride;

        public static GameSession Current { get; private set; }
        public int Money => money;
        public string SavePath => string.IsNullOrEmpty(savePathOverride)
            ? Path.Combine(Application.persistentDataPath, "life-sim-mvp.json")
            : savePathOverride;
        public event Action Changed;

        public void Configure(SimController playerSim, GameClock gameClock)
        {
            player = playerSim;
            clock = gameClock;
        }

        public void SetSavePathOverride(string path)
        {
            savePathOverride = path;
        }

        private void Awake()
        {
            Current = this;
        }

        private void OnDestroy()
        {
            if (Current == this)
            {
                Current = null;
            }
        }

        public bool TrySpend(int amount)
        {
            if (amount < 0 || money < amount)
            {
                return false;
            }

            money -= amount;
            Changed?.Invoke();
            return true;
        }

        public void AddMoney(int amount)
        {
            money = Mathf.Max(0, money + amount);
            Changed?.Invoke();
        }

        public void RegisterConversation(SimController first, SimController second)
        {
            SimController npc = first.IsPlayer ? second : second.IsPlayer ? first : null;
            if (npc == null)
            {
                return;
            }

            int index = relationshipNames.IndexOf(npc.SimName);
            if (index < 0)
            {
                relationshipNames.Add(npc.SimName);
                relationshipValues.Add(5);
            }
            else
            {
                relationshipValues[index] = Mathf.Clamp(relationshipValues[index] + 5, -100, 100);
            }
            Changed?.Invoke();
        }

        public string RelationshipSummary()
        {
            if (relationshipNames.Count == 0)
            {
                return "Amizades: conheça os vizinhos";
            }

            return "Amizades: " + string.Join(
                "  •  ",
                relationshipNames.Select((name, index) => $"{name} {relationshipValues[index]}").Take(3));
        }

        public void SaveGame()
        {
            var data = new SaveData
            {
                money = money,
                day = clock.Day,
                minutes = clock.MinutesSinceMidnight,
                relationships = relationshipNames.Select((name, index) => new RelationshipData
                {
                    npcName = name,
                    friendship = relationshipValues[index]
                }).ToList()
            };

            foreach (SimController sim in FindObjectsByType<SimController>(FindObjectsSortMode.None))
            {
                data.sims.Add(new SimData
                {
                    simName = sim.SimName,
                    position = sim.transform.position,
                    needs = sim.Needs.CaptureValues()
                });
            }

            foreach (FurnitureItem item in FindObjectsByType<FurnitureItem>(FindObjectsSortMode.None))
            {
                data.furniture.Add(new FurnitureData
                {
                    saveId = item.SaveId,
                    type = item.Type,
                    position = item.transform.position,
                    purchased = item.PurchasedByPlayer
                });
            }

            File.WriteAllText(SavePath, JsonUtility.ToJson(data, true));
            Changed?.Invoke();
            Debug.Log($"[MVP] Jogo salvo em {SavePath}");
        }

        public void LoadGame()
        {
            if (!File.Exists(SavePath))
            {
                Debug.LogWarning("[MVP] Ainda não existe um jogo salvo.");
                return;
            }

            SaveData data = JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));
            money = data.money;
            clock.Restore(data.day, data.minutes);

            foreach (SimController sim in FindObjectsByType<SimController>(FindObjectsSortMode.None))
            {
                SimData saved = data.sims.FirstOrDefault(candidate => candidate.simName == sim.SimName);
                if (saved != null)
                {
                    sim.SetPositionInstant(saved.position);
                    sim.Needs.RestoreValues(saved.needs);
                }
            }

            foreach (FurnitureItem item in FindObjectsByType<FurnitureItem>(FindObjectsSortMode.None))
            {
                Destroy(item.gameObject);
            }
            foreach (FurnitureData item in data.furniture)
            {
                FurnitureFactory.Create(item.type, item.position, item.purchased, item.saveId);
            }

            relationshipNames = data.relationships.Select(item => item.npcName).ToList();
            relationshipValues = data.relationships.Select(item => item.friendship).ToList();
            Changed?.Invoke();
            Debug.Log("[MVP] Jogo carregado.");
        }
    }
}
