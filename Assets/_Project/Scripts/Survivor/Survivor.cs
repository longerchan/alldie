using System;
using FrostShelter.SaveSystem;

namespace FrostShelter.Survivor
{
    /// <summary>
    /// 幸存者实体。管理满意度、健康、心情以及职业效率加成。
    /// </summary>
    public class Survivor
    {
        public string Id { get; private set; }
        public string Name { get; set; }
        public string OccupationTag { get; set; }

        public float Satisfaction { get; private set; } = 100f;
        public float Health { get; private set; } = 100f;
        public float Mood { get; private set; } = 100f;

        public bool IsSick { get; set; }
        public bool IsAssigned => !string.IsNullOrEmpty(AssignedBuildingId);
        public string AssignedBuildingId { get; set; }
        public bool IsInExpedition { get; set; }
        public int DaysSurvived { get; set; }

        public bool WantsToEscape => Satisfaction < Core.Constants.SATISFACTION_ESCAPE_THRESHOLD;
        public bool IsDead => Health <= 0f;

        // 职业标签池
        public static readonly string[] OccupationPool =
        {
            "伐木工", "矿工", "猎人", "厨师", "医生",
            "建造师", "研究员", "铁匠", "裁缝", "拾荒者",
        };

        // 随机名称池
        public static readonly string[] NamePool =
        {
            "Jack", "Emily", "Lucas", "Sophia", "Noah",
            "Emma", "Oliver", "Ava", "Ethan", "Isabella",
        };

        public Survivor(string id, string name, string occupationTag)
        {
            Id = id;
            Name = name;
            OccupationTag = occupationTag;
        }

        public float EfficiencyBonus
        {
            get
            {
                float baseBonus = OccupationTag switch
                {
                    "伐木工" => 0.10f,
                    "矿工" => 0.10f,
                    "猎人" => 0.10f,
                    "厨师" => 0.05f,
                    "医生" => 0.05f,
                    "建造师" => 0.08f,
                    "研究员" => 0.08f,
                    "铁匠" => 0.08f,
                    "裁缝" => 0.05f,
                    "拾荒者" => 0.05f,
                    _ => 0.02f,
                };
                // 心情影响效率
                float moodMod = Mood > 80f ? 1.2f : Mood > 50f ? 1.0f : 0.8f;
                return baseBonus * moodMod;
            }
        }

        public float GetEfficiencyForBuilding(BuildingId buildingId)
        {
            bool isMatch = (OccupationTag, buildingId) switch
            {
                ("伐木工", BuildingId.Lumberyard) => true,
                ("矿工", BuildingId.CoalMine) => true,
                ("矿工", BuildingId.IronMine) => true,
                ("猎人", BuildingId.HuntingHut) => true,
                ("医生", BuildingId.MedicalTent) => true,
                ("研究员", BuildingId.ResearchLab) => true,
                ("建造师", BuildingId.Furnace) => true,
                ("建造师", BuildingId.Shelter) => true,
                ("铁匠", BuildingId.WarmWorkshop) => true,
                ("厨师", BuildingId.Tavern) => true,
                _ => false,
            };

            return isMatch ? EfficiencyBonus * 2f : EfficiencyBonus;
        }

        public void ModifySatisfaction(float delta)
        {
            Satisfaction = Math.Max(0f, Math.Min(100f, Satisfaction + delta));
        }

        public void ModifyHealth(float delta)
        {
            Health = Math.Max(0f, Math.Min(100f, Health + delta));
            if (Health <= 0f)
            {
                Health = 0f;
            }
        }

        public void ModifyMood(float delta)
        {
            Mood = Math.Max(0f, Math.Min(100f, Mood + delta));
        }

        /// <summary>每小时tick（由SurvivorManager调用）</summary>
        public void TickHourly(float temperatureDamagePerHour, float sickChance)
        {
            // 满意度衰减
            ModifySatisfaction(-Core.Constants.SATISFACTION_DECAY_PER_HOUR);
            // 心情衰减
            ModifyMood(-Core.Constants.MOOD_DECAY_PER_HOUR);
            // 温度伤害
            if (temperatureDamagePerHour > 0f)
            {
                ModifyHealth(-temperatureDamagePerHour);
            }
            // 疾病判定
            if (!IsSick && sickChance > 0f)
            {
                if (UnityEngine.Random.value < sickChance)
                {
                    IsSick = true;
                }
            }
            if (IsSick)
            {
                ModifyHealth(-0.5f); // 疾病持续伤害
                ModifySatisfaction(-2f);
            }
        }

        public SurvivorData ToSaveData()
        {
            return new SurvivorData
            {
                id = Id,
                name = Name,
                occupationTag = OccupationTag,
                satisfaction = Satisfaction,
                health = Health,
                mood = Mood,
                efficiencyBonus = EfficiencyBonus,
                isSick = IsSick,
                isInExpedition = IsInExpedition,
                assignedBuildingId = AssignedBuildingId,
                daysSurvived = DaysSurvived,
            };
        }

        public static Survivor FromSaveData(SurvivorData data)
        {
            return new Survivor(data.id, data.name, data.occupationTag)
            {
                Satisfaction = data.satisfaction,
                Health = data.health,
                Mood = data.mood,
                IsSick = data.isSick,
                IsInExpedition = data.isInExpedition,
                AssignedBuildingId = data.assignedBuildingId,
                DaysSurvived = data.daysSurvived,
            };
        }
    }
}
