using UnityEngine;

public class EatState : IState
{
    private AnimalController animalController;
    private Animator animator;
    private float eatingDuration = 6f;
    private float eatingTimer = 0f;
    private GameObject nearbyFood = null;
    private float minEatingDistance = 0.8f;
    private bool isEating = false;

    public EatState(AnimalController controller)
    {
        animalController = controller;
        animator = controller.Animator;
    }

    public void Enter()
    {
        if (isEating) return; // Prevent double entry
        isEating = true;

        Debug.Log("Entering Eat State");
        animalController.PausePatrol();

        nearbyFood = animalController.GetNearbyFood();
        if (nearbyFood != null)
        {
            animator.SetTrigger("Eat"); // Ensure animation is triggered only once
            eatingTimer = 0f;
        }
        else
        {
            animalController.FSM.ChangeState(new PatrolState(animalController));
        }
    }

    public void Exit()
    {
        Debug.Log("Exiting Eat State");
        animalController.ResumePatrol();
        isEating = false;
    }

    public void Update()
    {
        if (nearbyFood != null)
        {
            HandleMovingToFood();
        }

        eatingTimer += Time.deltaTime;

        if (eatingTimer >= eatingDuration * 0.8f) // Destroy food midway
        {
            DestroyFood();
        }

        if (eatingTimer >= eatingDuration)
        {
            EndEating();
        }
    }

    private void HandleMovingToFood()
    {
        if (nearbyFood == null) return;

        Vector3 directionToFood = nearbyFood.transform.position - animalController.transform.position;
        if (directionToFood.magnitude > minEatingDistance)
        {
            animalController.MoveToTarget(nearbyFood.transform.position, animalController.PatrolSpeed);
        }
    }

    private void DestroyFood()
    {
        if (nearbyFood != null)
        {
            GameObject.Destroy(nearbyFood);
            nearbyFood = null;
        }
    }

    private void EndEating()
    {
        animalController.Health = Mathf.Min(animalController.Health + 10f, 100f);
        animalController.FSM.ChangeState(new PatrolState(animalController));
    }
}
