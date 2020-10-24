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
    public int Coins { get; set; } // TODO loading and saving

    [Header("General")]
    [SerializeField]
    private Canvas _inventoryCanvas;
    [SerializeField]
    private Canvas _inGameCanvas;
    [SerializeField]
    private InventorySlotContainer _inventoryContainer;
    [SerializeField]
    private bool _isShop;

    [Header("ItemsUI")]
    [SerializeField]
    private GameObject _itemUI;
    [SerializeField]
    private Transform _slotHolder;
    [SerializeField]
    private GameObject _slotPrefab;

    [Header("DescriptionUI")]
    [SerializeField]
    private GameObject _descriptionUI;
    [SerializeField]
    private TextMeshProUGUI _itemNameTMPT;
    [SerializeField]
    private TextMeshProUGUI _descriptionTMPT;
    [SerializeField]
    private Button _equipBuyButton;
    
    [Header("StatsUI")]
    [SerializeField]
    private GameObject _statsUI;

    private string _savePath;
    private int _currentItem;
    private TextMeshProUGUI _equipBuyButtonTMPT;

    public void Start()
    {
        _equipBuyButtonTMPT = _equipBuyButton.GetComponentInChildren<TextMeshProUGUI>();
        HideInventory();
    }

    public void ShowInventory()
    {
        DisplayInventory();
        _inventoryCanvas.enabled = true;
        _inGameCanvas.enabled = false;
    }

    public void HideInventory()
    {
        _inventoryCanvas.enabled = false;
        _inGameCanvas.enabled = true;
    }

    public void DisplayInfo(int _slotPosition)
    {
        _itemNameTMPT.text = _inventoryContainer.Slots[_slotPosition].itemObject.name;
        _descriptionTMPT.text = _inventoryContainer.Slots[_slotPosition].itemObject.description;
        _equipBuyButton.onClick.RemoveAllListeners();

        if (_isShop)
        {
            _equipBuyButtonTMPT.SetText("Buy");
            int a = _slotPosition;
            _equipBuyButton.onClick.AddListener(delegate { BuyItem(a); });
        } else
        {
            _equipBuyButtonTMPT.SetText("Equip");
            int a = _slotPosition;
            _equipBuyButton.onClick.AddListener(delegate { EquipItem(a); });
        }
    }

    public void DisplayInventory()
    {
        int counter = 0;

        foreach(Transform child in _slotHolder)
        {
            if(counter < _inventoryContainer.Slots.Count)
            {
                child.gameObject.SetActive(true);
                child.GetComponent<Image>().sprite = _inventoryContainer.Slots[counter].itemObject.uiSprite;
                child.GetComponent<Button>().onClick.RemoveAllListeners();
                int a = counter;
                child.GetComponent<Button>().onClick.AddListener(delegate { DisplayInfo(a); });

                if (_isShop)
                {
                    child.GetComponentInChildren<TextMeshProUGUI>().enabled = true;
                    child.GetComponentInChildren<TextMeshProUGUI>().text = _inventoryContainer.Slots[counter].itemObject.price.ToString();
                }
                else
                {
                    if (_inventoryContainer.Slots[counter].amount > 1)
                    {
                        child.GetComponentInChildren<TextMeshProUGUI>().enabled = true;
                        child.GetComponentInChildren<TextMeshProUGUI>().text = _inventoryContainer.Slots[counter].amount.ToString();
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

        for (; counter < _inventoryContainer.Slots.Count; counter++)
        {
            GameObject newGO = Instantiate(_slotPrefab, _slotHolder);
            newGO.GetComponent<Image>().sprite = _inventoryContainer.Slots[counter].itemObject.uiSprite;
            newGO.GetComponent<Button>().onClick.RemoveAllListeners();
            int a = counter;
            newGO.GetComponent<Button>().onClick.AddListener(delegate { DisplayInfo(a); });

            if (_inventoryContainer.Slots[counter].amount > 1)
            {
                newGO.GetComponentInChildren<TextMeshProUGUI>().enabled = true;
                newGO.GetComponentInChildren<TextMeshProUGUI>().text = _inventoryContainer.Slots[counter].amount.ToString();
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

    public void SetShop(bool isShop)
    {
        _isShop = isShop;
    }

    #region Item List Manipulation

    public void AddItem(ItemObject _itemObject, int _amount)
    {
        for (int i = 0; i < _inventoryContainer.Slots.Count; i++)
        {
            if (_inventoryContainer.Slots[i].itemObject.id == _itemObject.id)
            {
                _inventoryContainer.Slots[i].amount += _amount;
                return;
            }
        }

        _inventoryContainer.Slots.Add(new InventoryItem(_itemObject, _amount));
    }

    public void RemoveItem(int _itemObjectID)
    {
        for (int i = 0; i < _inventoryContainer.Slots.Count; i++)
        {
            if (_inventoryContainer.Slots[i].itemObject.id == _itemObjectID)
            {
                _inventoryContainer.Slots[i].amount--;

                if (_inventoryContainer.Slots[i].amount <= 0)
                {
                    _inventoryContainer.Slots.Remove(_inventoryContainer.Slots[i]);
                }
            }
        }
    }

    public void Save()
    {
        LoadManager.SaveFile(_savePath, new SaveableInventory(_inventoryContainer.Slots));
    }

    public void Load(string path)
    {
        _inventoryContainer.Load(path);

        // TODO else add starting item?
    }

    public void SwitchContainer(InventorySlotContainer container)
    {
        _inventoryContainer.Save();
        _inventoryContainer = container;
    }

    public void Clear()
    {
        _inventoryContainer.Slots = new List<InventoryItem>();
    }

    private void OnApplicationQuit()
    {
        _inventoryContainer.Save();
    }

    #endregion
}
