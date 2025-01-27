using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;

public class Inventory : MonoBehaviour
{
    [SerializeReference] public List<ItemSlotInfo> items = new List<ItemSlotInfo>();

    [Space]
    [Header("Inventory Menu Component")]
    public GameObject inventoryMenu;
    public GameObject itemPannel;
    public GameObject itemPannelGrid;
    public Camera cam;
    public Mouse mouse;

    private List<ItemPannel> existingPanels = new List<ItemPannel>();
    Dictionary<string ,Item> allItemsDictionary = new Dictionary<string ,Item>();
    [Space]
    public int inventorySize = 15;

    void Start()
    {
        for (int i = 0; i < inventorySize; i++)
        {
            items.Add(new ItemSlotInfo(null, 0));
        }


        List<Item> allItems = GetAllItems().ToList();
        string itemsInDictionary = "items in dictionary: ";
        foreach (Item i in allItems)
        {
            if(!allItemsDictionary.ContainsKey(i.GiveName()))
            {
                allItemsDictionary.Add(i.GiveName(), i);
                itemsInDictionary += ", " + i.GiveName();
            }
            else
            {
                Debug.Log("" + "already exists with name " + allItemsDictionary[i.GiveName()]);
            }
            
        }
        itemsInDictionary += ".";
        Debug.Log(itemsInDictionary);




        AddItem("Wood", 35);
        AddItem("Stone", 25);


    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (inventoryMenu.activeSelf)
            {
                inventoryMenu.SetActive(false);
                mouse.EmptySlot();
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                inventoryMenu.SetActive(true);
                Cursor.lockState = CursorLockMode.Confined;
                RefreshInventory();
            }
        }
        if (Input.GetKeyDown(KeyCode.Mouse1) && mouse.itemSlot.item != null)
        {
           RefreshInventory();
        }
        if(Input.GetKeyDown(KeyCode.Mouse0) && mouse.itemSlot.item != null && !EventSystem.current.IsPointerOverGameObject())
        {
            DropItem(mouse.itemSlot.item.GiveName());
        }
    }


    public void RefreshInventory()
    {
        existingPanels = itemPannelGrid.GetComponentsInChildren<ItemPannel>().ToList();
        // Creating new slots if needed 
        if (existingPanels.Count == inventorySize)
        {
            int amountToCreate = inventorySize - existingPanels.Count;
            for (int i = 0; i < amountToCreate; i++)
            {
                GameObject newPanel = Instantiate(itemPannel, itemPannelGrid.transform);
                existingPanels.Add(newPanel.GetComponent<ItemPannel>());
            }
        }
        int index = 0;
        foreach (ItemSlotInfo i in items)
        { // naming the items in the list 
            i.name = "" + (index + 1);
            if (i.item != null) i.name += ": " + i.item.GiveName();
            else i.name += ": -";


            //updating the slots

            ItemPannel panel = existingPanels[index];

            if (panel != null)
            {
                panel.name = i.name + "Panel";
                panel.inventory = this;
                panel.itemSlot = i;
                if (i.item != null)
                {
                    panel.itemImage.gameObject.SetActive(true);
                    panel.itemImage.sprite = i.item.GiveItemImage();
                    panel.itemImage.CrossFadeAlpha(1,0.05f,true);
                    panel.stacksText.gameObject.SetActive(true);
                    panel.stacksText.text = "" + i.stacks;
                }
                else
                {
                    panel.itemImage.gameObject.SetActive(false);
                    panel.stacksText.gameObject.SetActive(false);
                }

            }
            index++;

        }
        mouse.EmptySlot();

    }


    public int AddItem(string itemName, int amount)
    {
        // find item to add
        Item item = null;
        allItemsDictionary.TryGetValue(itemName, out item);
        //exit item if found
        if(item == null)
        {
            Debug.Log("Couldnt find item");
            return amount;
        }
        // check if u can add to an existing slot
        foreach (ItemSlotInfo i in items)
        {
            if (i.item != null)
            {
                if (i.item.GiveName() == item.GiveName())
                {
                    if (amount > i.item.MaxStacks() - i.stacks)
                    {
                        amount -= i.item.MaxStacks() - i.stacks;
                        i.stacks = i.item.MaxStacks();
                    }
                    else
                    {
                        i.stacks += amount;
                        if(inventoryMenu.activeSelf) RefreshInventory();
                        return 0;
                    }
                }
            }
        }
        // adding the rest of the items in a new slot 
        foreach (ItemSlotInfo i in items)
        { 
            if (i.item == null)
            {
                if(amount > item.MaxStacks())
                {
                    i.item = item;
                    i.stacks = item.MaxStacks();
                    amount -= item.MaxStacks();
                }
                else
                {
                    i.item = item;
                    i.stacks += amount;
                    if (inventoryMenu.activeSelf) RefreshInventory();
                    return 0;
                }
            }
        }

        // no space in inventory reminder

        Debug.Log("No Space for : " + item.GiveName());
        if(inventoryMenu.activeSelf) RefreshInventory();
        return amount;
    }

    
    public void DropItem(string itemName)
    {
        Item item = null;
        allItemsDictionary.TryGetValue(itemName, out item);
        if (item == null)
        {
            Debug.Log("no items found to drop");
            return;
        }
        Transform camTransform = cam.transform;
        GameObject droppedItem = Instantiate(item.DropObject(), cam.transform.position + camTransform.forward * 5f + new Vector3(0, 0.3f, 0), Quaternion.Euler(Vector3.zero) );
        Rigidbody rb = droppedItem.GetComponent<Rigidbody>();
        if (rb != null) rb.linearVelocity = camTransform.forward * 25;
        ItemPickUp ip = droppedItem.GetComponentInChildren<ItemPickUp>();
        if (ip != null)
        {
            ip.itemToDrop = itemName;
            ip.amount = mouse.splitSize;
            mouse.itemSlot.stacks -= mouse.splitSize;
        }

        if (mouse.itemSlot.stacks < 1) ClearSlot(mouse.itemSlot);
        mouse.EmptySlot();
        RefreshInventory();
    }


    public void ClearSlot(ItemSlotInfo slot)
    {
        slot.item = null;
        slot.stacks = 0;
    }


    IEnumerable<Item> GetAllItems()
    {
        return System.AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes()).Where(type=>type.IsSubclassOf(typeof(Item)))
            .Select(type => System.Activator.CreateInstance(type) as Item);
    }
}

