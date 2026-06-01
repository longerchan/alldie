using FrostShelter.Core;
using FrostShelter.SaveSystem;
using System;
using System.Collections.Generic;

namespace FrostShelter.Building
{
    /// <summary>
    /// 温暖工坊。制造特殊道具（暖炉、工具、火晶等），应对暴风雪/探险。
    /// </summary>
    public class WarmWorkshop : BuildingBase
    {
        public int CraftQueueSize => CurrentLevelConfig?.CraftQueueSize ?? 1 + Level;

        public List<CraftJob> CraftQueue { get; private set; } = new();
        public List<WorkshopRecipe> UnlockedRecipes { get; private set; } = new();

        public override void Initialize(BuildingId id, BuildingConfigSO config, int initialLevel = 0)
        {
            base.Initialize(id, config, initialLevel);
            InitDefaultRecipes();
        }

        private void InitDefaultRecipes()
        {
            UnlockedRecipes.Add(new WorkshopRecipe
            {
                RecipeId = "portable_heater",
                RecipeName = "便携暖炉",
                OutputItemId = "heater_item",
                OutputCount = 1,
                CraftTimeSeconds = 30f,
                Description = "探险中使用，恢复暖炉值+30",
                RequiredWorkshopLevel = 1,
                CostWood = 5,
                CostCoal = 3,
            });
            UnlockedRecipes.Add(new WorkshopRecipe
            {
                RecipeId = "fire_crystal",
                RecipeName = "火晶合成",
                OutputItemId = "fire_crystal",
                OutputResourceType = SaveSystem.ResourceType.FireCrystal,
                OutputCount = 1,
                CraftTimeSeconds = 120f,
                Description = "终极燃料，提升熔炉功率",
                RequiredWorkshopLevel = 3,
                CostCoal = 20,
                CostIronOre = 5,
            });
            UnlockedRecipes.Add(new WorkshopRecipe
            {
                RecipeId = "repair_kit",
                RecipeName = "修理工具包",
                OutputItemId = "repair_kit",
                OutputCount = 1,
                CraftTimeSeconds = 45f,
                Description = "探险中修复装备",
                RequiredWorkshopLevel = 2,
                CostWood = 8,
                CostIronOre = 3,
            });
        }

        public bool CanCraft => Level >= 0 && CraftQueue.Count < CraftQueueSize;

        public bool StartCraft(string recipeId, Resource.ResourceManager resourceManager)
        {
            if (!CanCraft) return false;
            var recipe = UnlockedRecipes.Find(r => r.RecipeId == recipeId);
            if (recipe == null || Level < recipe.RequiredWorkshopLevel) return false;

            var cost = new Resource.ResourceCost();
            if (recipe.CostWood > 0) cost.Add(SaveSystem.ResourceType.Wood, recipe.CostWood);
            if (recipe.CostCoal > 0) cost.Add(SaveSystem.ResourceType.Coal, recipe.CostCoal);
            if (recipe.CostIronOre > 0) cost.Add(SaveSystem.ResourceType.IronOre, recipe.CostIronOre);

            if (!resourceManager.CanAfford(cost)) return false;
            resourceManager.Spend(cost, Core.ResourceChangeReason.Upgrade);

            CraftQueue.Add(new CraftJob
            {
                RecipeId = recipeId,
                TotalTimeSeconds = recipe.CraftTimeSeconds,
                RemainingTimeSeconds = recipe.CraftTimeSeconds,
            });
            return true;
        }

        public List<WorkshopRecipe> TickCrafting(float deltaSeconds)
        {
            var completed = new List<WorkshopRecipe>();
            if (CraftQueue.Count == 0) return completed;

            var efficiency = TotalEfficiencyMultiplier();
            var first = CraftQueue[0];
            first.RemainingTimeSeconds -= deltaSeconds * efficiency;
            if (first.RemainingTimeSeconds <= 0f)
            {
                var recipe = UnlockedRecipes.Find(r => r.RecipeId == first.RecipeId);
                if (recipe != null) completed.Add(recipe);
                CraftQueue.RemoveAt(0);
            }
            return completed;
        }
    }

    [Serializable]
    public class CraftJob
    {
        public string RecipeId;
        public float TotalTimeSeconds;
        public float RemainingTimeSeconds;
    }

    [Serializable]
    public class WorkshopRecipe
    {
        public string RecipeId;
        public string RecipeName;
        public string OutputItemId;
        public SaveSystem.ResourceType OutputResourceType;
        public int OutputCount;
        public float CraftTimeSeconds;
        public string Description;
        public int RequiredWorkshopLevel;
        public float CostWood;
        public float CostCoal;
        public float CostIronOre;
    }
}
