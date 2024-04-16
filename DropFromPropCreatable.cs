using Necro.Data;
using Necro.Gameplay.EnemySystem;
using Necro.Level;
using Necro.System;
using UnityEngine;
using Zenject;
using static Necro.Data.DropFromDestructibleData;

namespace Necro.Gameplay
{
    public class DropFromPropCreatable : IDropFromPropCreatable
    {
        [Inject] private IEnemyController _enemyController;
        [Inject(Id = "CurrencyDropCreatable")] private IDropCreatable _currencyDropCreatable;
        [Inject(Id = "EquipmentDropCreatable")] private IDropCreatable _equipmentDropCreatable;
        [Inject] private ILootSystem _lootSystem;
        [Inject] private IResourcesService _resourcesService;
        [Inject] private IStatSystem _statSystem;

        private DropFromDestructibleData _dropFromDestructibleData;

        private struct DropPercentage
        {
            public float nothingPercentage;
            public float enemyPercentage;
            public float bodyPercentage;
        }

        private DropPercentage _dropPercentage;
        private BiomType _biomType;

        private int _totalCurrency;
        private int _totalDestructible;

        public void Init(BiomType biomType, int totalDestructible)
        {
            _dropFromDestructibleData = _resourcesService.GetDropFromDestructibleData();
            _biomType = biomType;
            _totalCurrency = _dropFromDestructibleData.GetTotalCurrency(_biomType);
            _totalDestructible = totalDestructible;
        }

        public void DropItems(DestructibleType _destructibleType, Transform transformItem, Transform transformEnemy)
        {
            var destructibleStats = _dropFromDestructibleData.GetDropInfo(_biomType, _destructibleType);

            SpawnCurrency(transformItem);

            CalculatePercentage(ref destructibleStats);

            float randomValue = Random.value;
        
            if (randomValue < _dropPercentage.nothingPercentage)
            {
                return;
            }
           
            if (randomValue < _dropPercentage.nothingPercentage + _dropPercentage.enemyPercentage && destructibleStats.Enemy.Length > 0)
            {
                SpawnEnemy(ref destructibleStats, transformEnemy);
                return;
            }

            if (destructibleStats.BodyParts.Length > 0)
            {
                SpawnBodyPart(ref destructibleStats, transformItem);
            }
        }

        private void SpawnCurrency(Transform transform)
        {
            var currencyData = _lootSystem.GetCurrencyData(BiomEnemyType.Zombie, CurrencyType.Memory);
            var args = currencyData.LootData.GetLootArgs();
            args.Count = 1;
            var dropAmount = GetDropAmount();
            
            for (var i = 0; i < dropAmount; i++)
            {
                _currencyDropCreatable.CreateItem<CurrencyLootItem>(_resourcesService.GetCurrencyLootItemPrefab(), args, transform.position);
            }
            
            return;

            int GetDropAmount()
            {
                var lAdditionalShards = _statSystem.GetStatModifiedValue(StatType.AdditionalShards);
                var lTotalCurrency = _totalCurrency + (_totalCurrency * lAdditionalShards / 100);
                return Mathf.RoundToInt(lTotalCurrency / _totalDestructible);
            }
        }

        private void SpawnEnemy(ref ItemDropSettings destructibleStats, Transform transform)
        {
            var spawnData = new EnemySpawnPointArgs
            {
                EnemyType = destructibleStats.Enemy[Random.Range(0, destructibleStats.Enemy.Length)],
                TargetTransform = transform
            };
            _enemyController.AsyncCreateEnemy(spawnData);
        }

        private void SpawnBodyPart(ref ItemDropSettings destructibleStats, Transform transform)
        {
            _equipmentDropCreatable.Drop(destructibleStats.BodyParts[Random.Range(0, destructibleStats.BodyParts.Length)], transform);
        }

        private void CalculatePercentage(ref ItemDropSettings destructibleStats)
        {
            float total = destructibleStats.SpawnNothingChance + destructibleStats.SpawnChanceEnemy + destructibleStats.SpawnChanceBody;
            _dropPercentage.nothingPercentage = destructibleStats.SpawnNothingChance / total;
            _dropPercentage.enemyPercentage = destructibleStats.SpawnChanceEnemy / total;
            _dropPercentage.bodyPercentage = destructibleStats.SpawnChanceBody / total;
        }
    }
}