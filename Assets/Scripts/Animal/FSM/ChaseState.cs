using UnityEngine;

public class ChaseState : IState
{
    private AnimalController animalController;
    private Animator animator;
    private GameObject target;

    public ChaseState(AnimalController controller)
    {
        animalController = controller;
        animator = controller.Animator;
    }

    public void Enter()
    {
        animator.SetTrigger("Chase");
        target = FindNearestTarget();
    }

    public void Exit()
    {
        animator.ResetTrigger("Chase");
    }

    public void Update()
    {
        if (target == null)
        {
            target = FindNearestTarget();
        }

        if (target != null)
        {
            if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Chase"))
            {
                animator.SetTrigger("Chase");
            }

            if (Vector3.Distance(animalController.transform.position, target.transform.position) <= 3f)
            {
                animalController.FSM.ChangeState(new AttackState(animalController, target));
                return;
            }

            animalController.MoveToTarget(target.transform.position, animalController.ChaseSpeed);
        }

        if (target == null || !animalController.DetectPlayerAndPrey())
        {
            animalController.FSM.ChangeState(new PatrolState(animalController));
        }
    }


    private GameObject FindNearestTarget()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject[] preyAnimals = GameObject.FindGameObjectsWithTag("Prey");

        float closestDistance = Mathf.Infinity;
        GameObject nearestTarget = null;

        foreach (GameObject player in players)
        {
            float distance = Vector3.Distance(animalController.transform.position, player.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                nearestTarget = player;
            }
        }

        foreach (GameObject prey in preyAnimals)
        {
            float distance = Vector3.Distance(animalController.transform.position, prey.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                nearestTarget = prey;
            }
        }

        return nearestTarget;
    }
}
