using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackBehaviour : StateMachineBehaviour
{
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Debug.Log("attack anim finished");

        animator.gameObject.transform.parent.GetComponent<ChampionAnimation>().OnAttackAnimationFinished();
    }
}
