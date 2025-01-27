using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class Mouse : MonoBehaviour
{
    public GameObject mouseItemUI;
    public Image mouseCursor;
    public ItemSlotInfo itemSlot;
    public Image itemImage;
    public TextMeshProUGUI stacksText;
    public ItemPannel sourceItemPannel;
    public int splitSize;
    void Update()
    {
        transform.position = Input.mousePosition;
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            mouseCursor.enabled = false;
            mouseItemUI.SetActive(false);
        }
        else
        {
            mouseCursor.enabled = true;
            if (itemSlot.item != null)
            {
                mouseItemUI.SetActive(true);
            }
            else
            {  
                mouseItemUI.SetActive(false);
            }

        }
        if (itemSlot.item != null)
        {
            if(Input.GetAxis("Mouse ScrollWheel") > 0 && splitSize<itemSlot.stacks)
            {
                splitSize++;
            }
            if (Input.GetAxis("Mouse ScrollWheel") < 0 && splitSize > 1)
            {
                splitSize--;
            }
            stacksText.text = "" + splitSize;
            if (splitSize == itemSlot.stacks)  sourceItemPannel.stacksText.gameObject.SetActive(false);
            else
            {
                sourceItemPannel.stacksText.gameObject.SetActive(true);
                sourceItemPannel.stacksText.text = "" + (itemSlot.stacks - splitSize);
            }
        }
    }
    public void SetUI()
    {
        stacksText.text = "" + splitSize;
        itemImage.sprite = itemSlot.item.GiveItemImage();
    }
    public void EmptySlot()
    {
        itemSlot = new ItemSlotInfo(null , 0);
    }
}
