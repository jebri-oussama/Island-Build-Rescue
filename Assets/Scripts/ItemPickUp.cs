using UnityEngine;

public class ItemPickUp : MonoBehaviour
{
    public string itemToDrop;
    public int amount = 1;


    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            Inventory playerInventory = other.GetComponentInChildren<Inventory>();
            if(playerInventory != null) PickUpItem(playerInventory);
        }
    }
    public void PickUpItem(Inventory inventory)
    {
        amount = inventory.AddItem(itemToDrop, amount);
        if (amount < 1 ) Destroy(this.transform.root.gameObject);
    }
}
