using UnityEngine;

public class IdleBehavior : StateMachineBehaviour
{
    [SerializeField]
    private float _minTimeIdleChange = 12f; // Tiempo mínimo antes del cambio
    [SerializeField]
    private float _maxTimeIdleChange = 50f; // Tiempo máximo antes del cambio
    [SerializeField]
    private int _numberofIdleAnimations;

    private float _idleTime;
    private float _timeIdleChange; // Nuevo tiempo aleatorio para cambiar de animación

    private bool _isInIdleSpecial;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        ResetIdle(animator);
        _timeIdleChange = Random.Range(_minTimeIdleChange, _maxTimeIdleChange); // Asignar un tiempo aleatorio
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!_isInIdleSpecial)
        {
            _idleTime += Time.deltaTime;
            if (_idleTime > _timeIdleChange)
            {
                _isInIdleSpecial = true;
                animator.SetFloat("IdleBlend", 1f);
            }
        }
        else if (stateInfo.normalizedTime % 1 >= 0.95f)
        {
            ResetIdle(animator);
            _timeIdleChange = Random.Range(_minTimeIdleChange, _maxTimeIdleChange);
        }
    }

    private void ResetIdle(Animator animator)
    {
        _isInIdleSpecial = false;
        _idleTime = 0;
        animator.SetFloat("IdleBlend", 0f);

    }
}

