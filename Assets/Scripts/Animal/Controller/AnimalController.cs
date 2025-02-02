using UnityEngine;
using UnityEngine.UI;

public class AnimalController : MonoBehaviour
{
    public AnimalFSM FSM;

    private float obstacleCheckDistance = 2f;

    public float GlowRange = 5f;
    public Color glowColor;
    public SkinnedMeshRenderer animalRenderer;
    public bool canLoot = false;
    public Animator Animator;
    public AnimalType animalType;
    public float Health = 100f;
    public Slider healthBarSlider;

    public GameObject[] players;

    public float PlayerDetectionRange = 10f;
    public float EatRange = 4f;
    public float PatrolSpeed = 1f;
    public float EscapeSpeed = 2f;
    public float ChaseSpeed = 2f;

    private float RotationTime = 2f;
    private Vector3 patrolCenter;
    public float PatrolZoneRadius = 10f;
    private Vector3 targetPatrolPosition;

    private bool isMovementStopped = false;

    private float savedPatrolSpeed;
    private float savedEscapeSpeed;
    private float savedChaseSpeed;

    private void Awake()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
        FSM = GetComponent<AnimalFSM>();
        Animator = GetComponent<Animator>();
        FSM.ChangeState(new PatrolState(this));
    }
    private void Start()
    {
        patrolCenter = transform.position;
        SetNewPatrolPoint();
    }

    private void Update()
    {
        FSM.Update();
        UpdateHealthBar();

        healthBarSlider.gameObject.SetActive(IsPlayerNearby() && !(FSM.currentState is DeathState));

        if (Health <= 0f && !(FSM.currentState is DeathState))
        {
            FSM.ChangeState(new DeathState(this));
            Debug.Log("Animal ready for looting. Press 'E' to loot.");
        }

        foreach (GameObject player in players)
        {
            bool isGlowing = Vector3.Distance(transform.position, player.transform.position) < GlowRange
                             && (FSM.currentState is DeathState);
            EnableGlow(isGlowing);
            canLoot = isGlowing;
        }
    }

    private void EnableGlow(bool isGlowing)
    {
        if (animalRenderer != null && animalRenderer.materials.Length > 0)
        {
            foreach (var material in animalRenderer.materials)
            {
                if (material.HasProperty("_EmissionColor"))
                {
                    material.SetColor("_EmissionColor", isGlowing ? glowColor : Color.black);
                    if (isGlowing)
                        material.EnableKeyword("_EMISSION");
                    else
                        material.DisableKeyword("_EMISSION");
                }
            }
        }
    }

    public bool IsPlayerNearby()
    {
        foreach (GameObject player in players)
        {
            if (Vector3.Distance(transform.position, player.transform.position) < 20)
                return true;
        }
        return false;
    }

    public bool DetectPlayerAndPredator()
    {
        return DetectObjectInRange("Player") || DetectObjectInRange("Predator");
    }

    public bool DetectPlayerAndPrey()
    {
        return DetectObjectInRange("Player") || DetectObjectInRange("Prey");
    }

    private bool DetectObjectInRange(string tag)
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag(tag);
        foreach (GameObject obj in objects)
        {
            if (IsRayBlockedByWall(obj))
                continue;

            if (Vector3.Distance(transform.position, obj.transform.position) < PlayerDetectionRange)
                return true;
        }
        return false;
    }

    public bool IsRayBlockedByWall(GameObject target)
    {
        RaycastHit hit;
        Vector3 directionToTarget = (target.transform.position - transform.position).normalized;
        Debug.DrawRay(transform.position, directionToTarget * PlayerDetectionRange, Color.red);

        int layerMask = LayerMask.GetMask("Wall");

        return Physics.Raycast(transform.position, directionToTarget, out hit, PlayerDetectionRange, layerMask)
               && hit.collider.CompareTag("Wall");
    }

    public GameObject GetNearbyFood()
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag("AnimalFood");
        foreach (GameObject obj in objects)
        {
            if (Vector3.Distance(transform.position, obj.transform.position) < EatRange)
                return obj;
        }
        return null;
    }


    public bool IsPlayerFacingAnimal()
    {
        foreach (GameObject player in players)
        {
            Vector3 directionToAnimal = transform.position - player.transform.position;
            directionToAnimal.y = 0;

            if (Vector3.Dot(player.transform.forward, directionToAnimal.normalized) > 0.1f)
                return true;
        }
        return false;
    }

    private void UpdateHealthBar()
    {
        if (healthBarSlider != null)
        {
            healthBarSlider.value = Health / 100f;
        }
    }

    public void TakeDamage(float damage)
    {
        Health -= damage;
        if (Health <= 0f)
        {
            Health = 0f;
            FSM.ChangeState(new DeathState(this));
        }
        UpdateHealthBar();
    }

    public void Patrol()
    {
        // Check if the animal is close to the patrol point on the X and Z axes
        if (Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(targetPatrolPosition.x, 0, targetPatrolPosition.z)) < 1f)
        {
            SetNewPatrolPoint();  // Set a new random patrol point
        }

        MoveToTarget(targetPatrolPosition, PatrolSpeed);
    }


    public void SetNewPatrolPoint()
    {
        Vector3 randomDirection;
        bool isValidPatrolPoint = false;

        while (!isValidPatrolPoint)
        {
            // Set a random point within the patrol zone
            randomDirection = Random.insideUnitSphere * PatrolZoneRadius;
            randomDirection.y = 0; // Ensure the patrol stays at the same height (y = 0)
            targetPatrolPosition = patrolCenter + randomDirection;

            // Optional: Ensure the new patrol point isn't too close to the animal's current position
            if (Vector3.Distance(transform.position, targetPatrolPosition) < 2f)
                continue;

            // Check if the new patrol point is blocked by a wall (raycast to the patrol point)
            if (!IsRayBlockedByWall(targetPatrolPosition))
            {
                isValidPatrolPoint = true;
            }
        }
    }


    // Modify the method to check for wall in the direction of patrol point
    public bool IsRayBlockedByWall(Vector3 targetPosition)
    {
        RaycastHit hit;
        Vector3 directionToTarget = (targetPosition - transform.position).normalized; // Direction to patrol point
        Debug.DrawRay(transform.position, directionToTarget * PlayerDetectionRange, Color.red);

        int layerMask = LayerMask.GetMask("Wall");

        // Raycast to the target patrol point to check for obstacles
        return Physics.Raycast(transform.position, directionToTarget, out hit, Vector3.Distance(transform.position, targetPosition), layerMask)
               && hit.collider.CompareTag("Wall");
    }



    private bool IsObstacleAhead(Vector3 direction)
    {
        RaycastHit hit;
        int layerMask = LayerMask.GetMask("Wall");

        return Physics.Raycast(transform.position, direction, out hit, obstacleCheckDistance, layerMask);
    }



    private bool IsPathClear(Vector3 direction)
    {
        RaycastHit hit;
        int layerMask = LayerMask.GetMask("Wall");

        return !Physics.Raycast(transform.position, direction, out hit, obstacleCheckDistance, layerMask);
    }
    private Vector3 GetAdjustedDirection(Vector3 originalDirection)
    {
        // Keep Y direction constant, only adjust X and Z.
        Vector3 flatDirection = new Vector3(originalDirection.x, 0, originalDirection.z).normalized;

        Vector3 leftDirection = Quaternion.Euler(0, -45, 0) * flatDirection;
        Vector3 rightDirection = Quaternion.Euler(0, 45, 0) * flatDirection;

        return !IsObstacleAhead(leftDirection) ? leftDirection : !IsObstacleAhead(rightDirection) ? rightDirection : Vector3.zero;
    }

    public void MoveToTarget(Vector3 targetPosition, float speed)
    {
        if (isMovementStopped) return;

        // The rigidbody will automatically adjust to terrain, no need to modify Y position manually
        Vector3 direction = (targetPosition - transform.position).normalized;

        if (direction == Vector3.zero) return;

        // Keep the Y position unchanged, only move on X and Z axes
        direction.y = 0;

        // Ensure the animal only moves on the X and Z axis
        targetPosition.y = transform.position.y;

        if (IsPathClear(direction))
        {
            // Move the animal towards the target position, without changing the Y position
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

            // Smoothly rotate the animal to face the target direction
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * RotationTime);
        }
        else
        {
            // Adjust direction based on obstacles, keeping the Y axis intact
            Vector3 adjustedDirection = GetAdjustedDirection(direction);
            if (adjustedDirection != Vector3.zero)
            {
                transform.position += adjustedDirection * (speed * Time.deltaTime);
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(adjustedDirection), Time.deltaTime * RotationTime);
            }
        }
    }


    public void StopMovement()
    {
        isMovementStopped = true;
    }

    public void ResumeMovement()
    {
        isMovementStopped = false;
    }

    public void SaveCurrentSpeeds()
    {
        savedPatrolSpeed = PatrolSpeed;
        savedEscapeSpeed = EscapeSpeed;
        savedChaseSpeed = ChaseSpeed;
    }

    public void RestoreSavedSpeeds()
    {
        PatrolSpeed = savedPatrolSpeed;
        EscapeSpeed = savedEscapeSpeed;
        ChaseSpeed = savedChaseSpeed;
    }

    //public void StopPatrol() => currentPatrolIndex = 0;

    public void PausePatrol()
    {
        isMovementStopped = true;
    }

    public void ResumePatrol()
    {
        isMovementStopped = false;
    }
}
