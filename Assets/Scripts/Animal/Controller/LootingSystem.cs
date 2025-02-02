using UnityEngine;

public class LootingSystem : MonoBehaviour
{
    public GameObject[] lootPrefabs;
    public Transform[] lootPoints;

    private AnimalController animalController;

    private void Start()
    {
        animalController = GetComponent<AnimalController>();
    }

    private void Update()
    {
        if (animalController.canLoot && Input.GetKeyDown(KeyCode.E) && animalController.FSM.currentState is DeathState && animalController.IsPlayerFacingAnimal())
        {
            SpawnLoot();
            Destroy(gameObject);
        }
    }

    private void SpawnLoot()
    {
        foreach (var point in lootPoints)
        {
            try
            {
                var randomIndex = Random.Range(0, lootPrefabs.Length);
                var selectedLootPrefab = lootPrefabs[randomIndex];

                Instantiate(selectedLootPrefab, point.position + Vector3.up * 0.6f, Quaternion.identity);
            }
            catch
            {
                Debug.Log("Warning suppressed for loot instantiation.");
            }
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            animalController.canLoot = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            animalController.canLoot = false;
        }
    }
}
