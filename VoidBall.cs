using Necro.Data;
using Necro.System;
using System.Collections;
using UnityEngine;
using Zenject;

namespace Necro.Gameplay
{
    public class VoidBall : Ball
    {
        [Inject] private IEffectController _effectController;
        [Inject] private IResourcesService _resourcesService;
        [SerializeField] private StatusData _stunStatusData;
        [SerializeField] private int _voidRadius;
        [SerializeField] private float _voidTime;
        [SerializeField] private float _activationDelay;
        private ProjectilePhysicsController _projectilePhysicsController;
        private PointEffector2D _projectileEffector2D;

        private readonly float _heightOffset = -0.1f;
        private void OnCollisionEnter(Collision collision)
        {
            _audioPlayer.PlayOnceOnAttachedObject(AUDIOEVENT.VOIDHAND_LAND, gameObject);
        }

        protected override IEnumerator ThrowCoroutine(Vector3 direction)
        {
            _audioPlayer.PlayOnceOnAttachedObject(AUDIOEVENT.VOIDHAND_THROW, gameObject);
            _projectilePhysicsController = GetComponent<ProjectilePhysicsController>();
            _projectileEffector2D = GetComponentInChildren<PointEffector2D>();
            _projectilePhysicsController.SetVelocity(direction*_speed);
            yield return new WaitForSeconds(_activationDelay);
            StartCoroutine(EnableVoid());
        }

        private IEnumerator EnableVoid()
        {
            _audioPlayer.PlayOnceOnAttachedObject(AUDIOEVENT.VOIDHAND_EXPLOSION, gameObject);
            _effectController.Play(EffectType.VoidActive, new CommonEffectPosition(transform.position, transform));
            Collider2D[] hit = Physics2D.OverlapCircleAll(transform.position, _voidRadius, LayerMask.GetMask("CollideWithGround"));
           
            foreach (var enemy in hit)
            {

                if (enemy.gameObject.GetComponentInParent<IEnemy>() == null || enemy.gameObject.CompareTag("Player"))
                {
                    continue;
                }              

                var enemyPhysics = enemy.gameObject.GetComponentInParent<EnemyPhysicsController>();

                enemyPhysics.SetPhysicsEnable(true);


                if (transform.position.y - enemy.transform.position.y < _heightOffset)
                {
                    enemy.gameObject.layer = LayerMask.NameToLayer("VoidActive");
                    var statusable = enemy.gameObject.GetComponentInParent<IStatusable>();
                    statusable.AddStatus(_stunStatusData, _unitAttacker);
                }
                else
                {
                    enemy.gameObject.layer = LayerMask.NameToLayer("VoidPassive");
                }              
            }
            _projectileEffector2D.enabled = true;

            yield return new WaitForSeconds(_voidTime);

            foreach (var enemy in hit)
            {
                if (enemy == null || enemy.gameObject.GetComponentInParent<IEnemy>() == null || enemy.gameObject.CompareTag("Player"))
                {
                    continue;
                }

                enemy.gameObject.layer = LayerMask.NameToLayer("CollideWithGround");
                var enemyType = enemy.gameObject.GetComponentInParent<IEnemy>();
                var enemyClass = _resourcesService.GetEnemyClassByType(enemyType.Type);

                if (enemyClass == EnemyClass.Boss || enemyClass == EnemyClass.MiniBoss)
                {
                    continue;
                }

                var statusable = enemy.gameObject.GetComponentInParent<IStatusable>();
                statusable.AddStatus(_stunStatusData, _unitAttacker);

            }    
            DisableVoid();
        }

        private void DisableVoid()
        {
            _projectileEffector2D.enabled = false;
            Dispose();
        }
    }
}