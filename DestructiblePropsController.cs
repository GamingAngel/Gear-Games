using Necro.Data;
using Necro.Level;
using System;
using System.Collections;
using UnityEngine;
using Zenject;

namespace Necro.Gameplay
{
    public class DestructiblePropsController: IDestructiblePropsController
    {
        [Inject] private ILocationController _locationController;
        [Inject] private IDropFromPropCreatable _dropfromDestructible;

        public event Action<DestructibleType, Transform, Transform> OnDeathProp;

        public IEnumerator LoadProps()
        {
            var biomData = _locationController.GetCurrentBiomData();

            if (biomData == null)
                yield break;

            int destructibleCount = 0;

            foreach (var props in biomData.DestructibleDatas)
            {
                var destructible = props.SpawnedGameObject.GetComponent<Destructible>();
                if (destructible.DestructibleType != DestructibleType.Candle)
                {
                    SubscribeToPropDeath(destructible);
                    destructibleCount++;
                }             
            }
            _dropfromDestructible.Init(_locationController.GetCurrentBiomData().BiomType, destructibleCount);
            yield return null;
        }

        private void SubscribeToPropDeath(Destructible propDamageable)
        {
            propDamageable.OnDeathProp += PropsDeathHandler;
        }

        private void PropsDeathHandler(Destructible destructible,DestructibleType destructibleType, Transform transformProp, Transform enemyTransform)
        {
            destructible.OnDeathProp -= PropsDeathHandler;
            OnDeathProp.Invoke(destructibleType, transformProp, enemyTransform);
        }
    }
}
