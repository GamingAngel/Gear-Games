using Necro.Gameplay.Unit.Player;
using Spine.Unity;
using System.Collections;
using UnityEngine;
using Zenject;

public class Grass : MonoBehaviour
{
    [Inject] private IPlayerController _playerController;

    [SerializeField] private SkeletonAnimation _skeletonAnimation;
    [SerializeField] private string[] _animations;

    private readonly float _delay = 2f;
    private bool _isPlayerInside;

    void Start()
    {
        SetAnimation(_animations[0], true);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            _isPlayerInside = true;
            float playerDirection = _playerController.GetPlayerPosition().x - transform.position.x;

            if (playerDirection > 0)
            {
                StartCoroutine(PlayAnimation(_animations[1], false));
            }
            else
            {
                StartCoroutine(PlayAnimation(_animations[2], false));
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            _isPlayerInside = false;
            _skeletonAnimation.AnimationState.AddAnimation(0, _animations[0], true, 0);
        }
    }

    private IEnumerator PlayAnimation(string animationName, bool value)
    {
        SetAnimation(animationName, value);
        yield return new WaitForSeconds(_delay);

        if(_isPlayerInside)
        {
            SetAnimation(_animations[3], true);
        }
        else
        {
            SetAnimation(_animations[0], true);
        }
    }

    private void SetAnimation(string animationName, bool value)
    {
        _skeletonAnimation.AnimationState.SetAnimation(0, animationName, value);
    }
}
