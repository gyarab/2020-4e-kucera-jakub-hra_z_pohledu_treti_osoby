using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class InventoryMonoBehaviour : MonoBehaviour
{
    public string savePath; // change to private TODO
    public Canvas inventoryUI;
    public List<InventoryItem> slots;
    public bool isShop;
    public Canvas inGameCanvas;

    [Header("ItemsUI")]
    public GameObject itemUI;
    public Transform slotHolder;
    public GameObject slotPrefab;
    [Header("DescriptionUI")]
    public GameObject descriptionUI;
    public TextMeshProUGUI itemNameTMPT;
    public TextMeshProUGUI descriptionTMPT;
    public Button equipBuyButton;

    private int currentItem;
    private TextMeshProUGUI equipBuyButtonTMPT;

    [Header("StatsUI")]
    public GameObject statsUI;

    public void Start()
    {
        equipBuyButtonTMPT = equipBuyButton.GetComponentInChildren<TextMeshProUGUI>();
        HideInventory();
    }

    public void ShowInventory()
    {
        DisplayInventory();
        inventoryUI.enabled = true;
        inGameCanvas.enabled = false;
    }

    public void HideInventory()
    {
        inventoryUI.enabled = false;
        inGameCanvas.enabled = true;
    }

    public void DisplayInfo(int _slotPosition)
    {
        itemNameTMPT.text = slots[_slotPosition].itemObject.name;
        descriptionTMPT.text = slots[_slotPosition].itemObject.description;
        equipBuyButton.onClick.RemoveAllListeners();

        if (isShop)
        {
            equipBuyButtonTMPT.SetText("Buy");
            int a = _slotPosition;
            equipBuyButton.onClick.AddListener(delegate { BuyItem(a); });
        } else
        {
            equipBuyButtonTMPT.SetText("Equip");
            int a = _slotPosition;
            equipBuyButton.onClick.AddListener(delegate { EquipItem(a); });
        }
    }

    public void DisplayInventory()
    {
        int counter = 0;

        foreach(Transform child in slotHolder)
        {
            if(counter < slots.Count)
            {
                child.gameObject.SetActive(true);
                child.GetComponent<Image>().sprite = slots[counter].itemObject.uiSprite;
                child.GetComponent<Button>().onClick.RemoveAllListeners();
                int a = counter;
                child.GetComponent<Button>().onClick.AddListener(delegate { DisplayInfo(a); });

                if (isShop)
                {
                    child.GetComponentInChildren<TextMeshProUGUI>().enabled = true;
                    child.GetComponentInChildren<TextMeshProUGUI>().text = slots[counter].itemObject.price.ToString();
                }
                else
                {
                    if (slots[counter].amount > 1)
                    {
                        child.GetComponentInChildren<TextMeshProUGUI>().enabled = true;
                        child.GetComponentInChildren<TextMeshProUGUI>().text = slots[counter].amount.ToString();
                    }
                    else
                    {
                        child.GetComponentInChildren<TextMeshProUGUI>().enabled = false;
                    }
                }
            } else
            {
                child.gameObject.SetActive(false);
            }
            counter++;
        }

        for (; counter < slots.Count; counter++)
        {
            GameObject newGO = Instantiate(slotPrefab, slotHolder);
            newGO.GetComponent<Image>().sprite = slots[counter].itemObject.uiSprite;
            newGO.GetComponent<Button>().onClick.RemoveAllListeners();
            int a = counter;
            newGO.GetComponent<Button>().onClick.AddListener(delegate { DisplayInfo(a); });

            if (slots[counter].amount > 1)
            {
                newGO.GetComponentInChildren<TextMeshProUGUI>().enabled = true;
                newGO.GetComponentInChildren<TextMeshProUGUI>().text = slots[counter].amount.ToString();
            }
            else
            {
                newGO.GetComponentInChildren<TextMeshProUGUI>().enabled = false;
            }
        }
    }

    // TODO
    public void EquipItem(int _slotPosition)
    {
        Debug.Log("EQUIP _slotPos " + _slotPosition);
    }

    // TODO
    public void BuyItem(int _slotPosition)
    {
        Debug.Log("BUY _slotPos " + _slotPosition);
    }

    #region Item List Manipulation

    public void AddItem(ItemObject _itemObject, int _amount)
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].itemObject.id == _itemObject.id)
            {
                slots[i].amount += _amount;
                return;
            }
        }

        slots.Add(new InventoryItem(_itemObject, _amount));
    }

    public void RemoveItem(int _itemObjectID)
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].itemObject.id == _itemObjectID)
            {
                slots[i].amount--;

                if (slots[i].amount <= 0)
                {
                    slots.Remove(slots[i]);
                }
            }
        }
    }

    public void Save()
    {
        SaveableInventory inv = new SaveableInventory();
        foreach(InventoryItem item in slots)
        {
            inv.items.Add(new SaveableInventorySlot(item.itemObject.id, item.amount));
        }

        string saveData = JsonUtility.ToJson(inv);
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(string.Concat(Application.persistentDataPath, savePath));
        bf.Serialize(file, saveData);
        file.Close();
    }

    public void Load()
    {
        if(File.Exists(string.Concat(Application.persistentDataPath, savePath)))
        {
            SaveableInventory inv = new SaveableInventory();
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(string.Concat(Application.persistentDataPath, savePath), FileMode.Open);
            JsonUtility.FromJsonOverwrite(bf.Deserialize(file).ToString(), inv);
            file.Close();

            slots = new List<InventoryItem>();
            foreach(SaveableInventorySlot slot in inv.items)
            {
                slots.Add(new InventoryItem(GameManager.Instance.GetItemObjectByID(slot.id), slot.amount));
            }
        }
    }

    public void Clear()
    {
        slots = new List<InventoryItem>();
    }

    #endregion
}
