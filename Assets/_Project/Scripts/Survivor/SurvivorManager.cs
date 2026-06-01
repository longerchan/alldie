using FrostShelter.SaveSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using FrostShelter.Core;
using FrostShelter.Temperature;

namespace FrostShelter.Survivor
{
    /// <summary>
    /// 幸存者管理器。管理所有幸存者的生命周期、分配、满意度。
    /// </summary>
    public class SurvivorManager : IService
    {
        private readonly List<Survivor> _survivors = new();

        private EventDispatcher _events;
        private TemperatureManager _temperatureManager;

        public IReadOnlyList<Survivor> AllSurvivors => _survivors;
        public int TotalSurvivorCount => _survivors.Count;
        public int MaxSurvivorCapacity { get; set; } = 4;
        public int AvailableSurvivorCount =>
            _survivors.Count(s => !s.IsAssigned && !s.IsInExpedition);

        public event Action<Survivor> OnSurvivorArrived;
        public event Action<Survivor> OnSurvivorEscaped;
        public event Action<Survivor> OnSurvivorDied;

        public void Initialize() { }

        public void Shutdown()
        {
            _survivors.Clear();
        }

        public void SetDependencies(EventDispatcher events, TemperatureManager temperatureManager)
        {
            _events = events;
            _temperatureManager = temperatureManager;
        }

        public Survivor AddSurvivor(string name, string occupationTag)
        {
            if (TotalSurvivorCount >= MaxSurvivorCapacity) return null;

            var survivor = new Survivor(Guid.NewGuid().ToString(), name, occupationTag);
            _survivors.Add(survivor);

            OnSurvivorArrived?.Invoke(survivor);
            _events?.Dispatch(GameEventType.SurvivorArrived);
            return survivor;
        }

        public Survivor AddRandomSurvivor()
        {
            string name = Survivor.NamePool[UnityEngine.Random.Range(0, Survivor.NamePool.Length)];
            string occupation = Survivor.OccupationPool[
                UnityEngine.Random.Range(0, Survivor.OccupationPool.Length)];
            return AddSurvivor(name, occupation);
        }

        public void RemoveSurvivor(string survivorId)
        {
            var survivor = GetSurvivor(survivorId);
            if (survivor != null)
            {
                _survivors.Remove(survivor);
            }
        }

        public Survivor GetSurvivor(string id)
        {
            return _survivors.Find(s => s.Id == id);
        }

        public List<Survivor> GetSurvivorsByOccupation(string occupationTag)
        {
            return _survivors.Where(s => s.OccupationTag == occupationTag).ToList();
        }

        public List<Survivor> GetAvailableSurvivors()
        {
            return _survivors.Where(s => !s.IsAssigned && !s.IsInExpedition).ToList();
        }

        public void AssignToBuilding(string survivorId, BuildingId buildingId)
        {
            var survivor = GetSurvivor(survivorId);
            if (survivor != null)
            {
                survivor.AssignedBuildingId = buildingId.ToString();
            }
        }

        public void UnassignFromBuilding(string survivorId)
        {
            var survivor = GetSurvivor(survivorId);
            if (survivor != null)
            {
                survivor.AssignedBuildingId = null;
            }
        }

        /// <summary>每小时tick</summary>
        public void OnMinuteTick()
        {
            float tempDamage = _temperatureManager?.SurvivorDamagePerHour ?? 0f;
            float sickChance = _temperatureManager?.SickChancePerHour ?? 0f;

            var toRemove = new List<Survivor>();

            foreach (var survivor in _survivors)
            {
                survivor.DaysSurvived++;
                survivor.TickHourly(tempDamage, sickChance);

                // 逃跑判定
                if (survivor.WantsToEscape)
                {
                    if (UnityEngine.Random.value < 0.1f) // 每小时10%逃跑概率
                    {
                        toRemove.Add(survivor);
                        OnSurvivorEscaped?.Invoke(survivor);
                        _events?.Dispatch(GameEventType.SurvivorEscaped);
                        continue;
                    }
                }

                // 死亡判定
                if (survivor.IsDead)
                {
                    toRemove.Add(survivor);
                    OnSurvivorDied?.Invoke(survivor);
                    _events?.Dispatch(GameEventType.SurvivorDied);
                }
            }

            foreach (var dead in toRemove)
            {
                _survivors.Remove(dead);
            }
        }

        public void HealSurvivor(string survivorId, float amount)
        {
            GetSurvivor(survivorId)?.ModifyHealth(amount);
        }

        public void BoostMorale(float amount)
        {
            foreach (var s in _survivors)
                s.ModifyMood(amount);
        }

        public void LoadFromSaveData(List<SaveSystem.SurvivorData> dataList)
        {
            _survivors.Clear();
            foreach (var data in dataList)
            {
                _survivors.Add(Survivor.FromSaveData(data));
            }
        }

        public List<SaveSystem.SurvivorData> ToSaveData()
        {
            return _survivors.Select(s => s.ToSaveData()).ToList();
        }
    }
}
