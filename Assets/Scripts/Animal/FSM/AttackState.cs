using UnityEngine;

public class AttackState : IState
{
    private AnimalController animalController;
    private Animator animator;
    private GameObject target;
    private float damage = 10f;
    private bool hasDamaged = false;


    public AttackState(AnimalController controller, GameObject attackTarget)
    {
        animalController = controller;
        animator = controller.Animator;
        target = attackTarget;
    }

    public void Enter()
    {
        animator.SetTrigger("Attack");
        animator.SetBool("isMoving", false);
        hasDamaged = false;
        animalController.StopMovement();
        //Debug.Log("Entering Attack State");
    }

    public void Exit()
    {
        animator.ResetTrigger("Attack");
        animator.SetBool("isMoving", true);
        animalController.ResumeMovement();
        //Debug.Log("Exiting Attack State");
    }



    public void Update()
    {
        if (target == null)
        {
            animalController.FSM.ChangeState(new PatrolState(animalController));
            return;
        }

        var targetAnimalController = target.GetComponent<AnimalController>();
        var targetPlayerController = target.GetComponent<PlayerController>();

        if (targetAnimalController == null && targetPlayerController == null)
        {
            //Debug.Log("Invalid target. Returning to Patrol State.");
            animalController.FSM.ChangeState(new PatrolState(animalController));
            return;
        }

        if (targetAnimalController != null && targetAnimalController.Health <= 0f)
        {
            Debug.Log("Target animal is dead. Returning to Chase State.");
            target.tag = "Dead";
            animalController.FSM.ChangeState(new ChaseState(animalController));
            return;
        }

        if (targetPlayerController != null && targetPlayerController.Health <= 0f)
        {
            Debug.Log("Target player is dead. Returning to Chase State.");
            target.tag = "Dead";
            animalController.FSM.ChangeState(new ChaseState(animalController));
            return;
        }

        if (animator.GetBool("isMoving") == false)
        {
            animalController.MoveToTarget(new Vector3(target.transform.position.x, animalController.transform.position.y, target.transform.position.z), 0f);
        }

        if (Vector3.Distance(animalController.transform.position, target.transform.position) <= 3f)
        {
            if (!hasDamaged)
            {
                if (targetAnimalController != null)
                {
                    targetAnimalController.TakeDamage(damage);
                    Debug.Log($"Target animal damaged by {damage}. Remaining Health: {targetAnimalController.Health}");
                }
                else if (targetPlayerController != null)
                {
                    targetPlayerController.TakeDamage(damage);
                    Debug.Log($"Target player damaged by {damage}. Remaining Health: {targetPlayerController.Health}");
                }

                damage = Mathf.Max(damage - 10f, 0f);

                //Debug.Log($"Damage reduced to: {damage}");
                hasDamaged = true;
            }
        }
        else
        {
            //Debug.Log("Target moved out of range. Returning to Chase State.");
            animalController.FSM.ChangeState(new ChaseState(animalController));
        }
    }

}
