using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChampionAnimation : MonoBehaviour
{
    private GameObject characterModel;
    private Animator animator;
    private ChampionController championController;

    private Vector3 lastFramePosition;
    // Start is called before the first frame update
    void Start()
    {
        characterModel = this.transform.Find("character").gameObject;
        animator = characterModel.GetComponent<Animator>();
        championController = this.GetComponent<ChampionController>();
    }

    // Update is called once per frame
    void Update()
    {
        float movementSpeed = (this.transform.position - lastFramePosition).magnitude / Time.deltaTime;

        animator.SetFloat("movementSpeed", movementSpeed);
        lastFramePosition = this.transform.position;
    }
    public void IsAnimated(bool b)
    {
        animator.enabled = b;
    }
    public void DoAttack(bool b)
    {
        animator.SetBool("isAttacking", b);
    }
    public void OnAttackAnimationFinished()
    {
        animator.SetBool("isAttacking", false);

        championController.OnAttackAnimationFinished();

    }
}
