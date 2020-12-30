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
    public HealthBar BossHealthBar => _bossHealthBar;

    #region Variables

    [Header("General")]
    [SerializeField]
    private Canvas _inventoryCanvas;
    [SerializeField]
    private Canvas _inGameCanvas;
    [SerializeField]
    private InventorySlotContainer _playerInventoryContainer;
    [SerializeField]
    private bool _isShop;
    [SerializeField]
    private GameObject _shopTabButton;
    [SerializeField]
    private PlayerController _player;

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
    [SerializeField]
    private TextMeshProUGUI _currentStatsTMPT;
    [SerializeField]
    private TextMeshProUGUI _statsDeltaTMPT;
    [SerializeField]
    private TextMeshProUGUI _coinBalanceTMPT;

    [Header("BossHealthBar")]
    [SerializeField]
    private HealthBar _bossHealthBar;

    private InventorySlotContainer _secondaryShopInventoryContainer;

    private string _savePath;
    private int _currentItem;
    private TextMeshProUGUI _equipBuyButtonTMPT;

    // Current equipment
    private InventorySlot _equippedWeaponSlot;

    #endregion

    public void Awake()
    {
        _equipBuyButtonTMPT = _equipBuyButton.GetComponentInChildren<TextMeshProUGUI>();
    }

    public void Start()
    {
        HideInventoryUI();
        PassStatsToPlayer();
        _bossHealthBar.SetVisibility(false);
    }

    private void OnEnable()
    {
        GroundItem.OnItemPickedUp += AddItem;
    }

    private void OnDisable()
    {
        GroundItem.OnItemPickedUp -= AddItem;
    }

    #region Displaying

    public void ShowInventory(bool shop)
    {
        _isShop = shop;

        if (_isShop)
        {
            _shopTabButton.SetActive(true);
            DisplayInventory(_secondaryShopInventoryContainer);
        } else
        {
            _shopTabButton.SetActive(false);
            DisplayInventory(_playerInventoryContainer);
        }

        _inventoryCanvas.enabled = true;
        _inGameCanvas.enabled = false;
    }

    public void DisplayInfo(int itemID)
    {
        InventorySlotContainer inventoryContainer;

        if (_isShop)
        {
            inventoryContainer = _secondaryShopInventoryContainer;
        } else
        {
            inventoryContainer = _playerInventoryContainer;
        }

        ItemObject itemObject = inventoryContainer.GetSlotByItemID(itemID).ItemObject;

        _itemNameTMPT.text = itemObject.name;
        _descriptionTMPT.text = itemObject.description;
        _equipBuyButton.onClick.RemoveAllListeners();

        int id = itemObject.itemID;
        if (itemObject is ConsumableObject)
        {
            if (_isShop)
            {
                _equipBuyButtonTMPT.SetText("Buy");
                _equipBuyButton.onClick.AddListener(delegate { BuyItem(id); });
            }
            else
            {
                _equipBuyButtonTMPT.SetText("Consume");
                _equipBuyButton.onClick.AddListener(delegate { ConsumeItem(id); });
            }
        } else
        {
            if (_isShop)
            {
                _equipBuyButtonTMPT.SetText("Buy");
                _equipBuyButton.onClick.AddListener(delegate { BuyItem(id); });
            }
            else
            {
                // TODO if equiped -> unequip
                _equipBuyButtonTMPT.SetText("Equip");
                _equipBuyButton.onClick.AddListener(delegate { EquipItem(id); });
            }

            // TODO change chanseStatsUI
            _currentStatsTMPT.text = _player.GetStats().StatsToStringColumn(false, false);
            // TODO change statsDelta
            CharacterStats selectedObjectStats = EquipmentToStats(itemObject);
            CharacterStats equippedObjectStats = new CharacterStats(); ;
            switch (itemObject.type)
            {
                case ItemType.Weapon:
                    if(_equippedWeaponSlot != null)
                    {
                        equippedObjectStats = EquipmentToStats(_equippedWeaponSlot.ItemObject);
                    }
                    break;
            }

            selectedObjectStats.SubtractStats(equippedObjectStats);
            _statsDeltaTMPT.text = selectedObjectStats.StatsToStringColumn(true, true);
        }
    }

    public void DisplayInventory(InventorySlotContainer inventory)
    {
        _coinBalanceTMPT.text = _playerInventoryContainer.Coins.ToString();

        int counter = 0;

        foreach (Transform child in _slotHolder)
        {
            if (counter < inventory.Slots.Count)
            {
                child.gameObject.SetActive(true);

                SetSlotDescription(child.gameObject, inventory.Slots[counter], counter);
            } else
            {
                child.gameObject.SetActive(false);
            }
            counter++;
        }

        for (; counter < inventory.Slots.Count; counter++)
        {
            GameObject newGO = Instantiate(_slotPrefab, _slotHolder);

            SetSlotDescription(newGO, inventory.Slots[counter], counter);
        }

        if (inventory.Slots.Count > 0)
        {
            DisplayInfo(inventory.Slots[0].ItemObject.itemID);
        }
    }

    public void SetSlotDescription(GameObject slot, InventorySlot item, int index)
    {
        slot.GetComponent<Image>().sprite = item.ItemObject.uiSprite;
        slot.GetComponent<Button>().onClick.RemoveAllListeners();
        slot.GetComponent<Image>().color = Color.white; // TODO dehighlight

        item.SlotHolderChildPosition = index;
        int id = item.ItemObject.itemID;

        if (!_isShop)
        {
            if (CheckIfEquipped(item))
            {
                slot.GetComponent<Image>().color = Color.green; // TODO highlight
            }
        }

        slot.GetComponent<Button>().onClick.AddListener(delegate { DisplayInfo(id); });

        if (_isShop)
        {
            slot.GetComponentInChildren<TextMeshProUGUI>().enabled = true;
            slot.GetComponentInChildren<TextMeshProUGUI>().text = item.ItemObject.price.ToString();
        }
        else
        {
            if (item.Amount > 1)
            {
                slot.GetComponentInChildren<TextMeshProUGUI>().enabled = true;
                slot.GetComponentInChildren<TextMeshProUGUI>().text = item.Amount.ToString();
            }
            else
            {
                slot.GetComponentInChildren<TextMeshProUGUI>().enabled = false;
            }
        }
    }

    #endregion

    private bool CheckIfEquipped(InventorySlot slot)
    {
        if(_equippedWeaponSlot != null)
        {
            if (slot.ItemObject.itemID == _equippedWeaponSlot.ItemObject.itemID)
            {
                return true;
            }
        }

        return false;
    }

    public void EquipItem(int itemID)
    {
        InventorySlot inventorySlot = _playerInventoryContainer.GetSlotByItemID(itemID);

        EquipItemInSlot(inventorySlot, true);
        PassStatsToPlayer();
        DisplayInfo(itemID);
    }

    public void EquipItemInSlot(InventorySlot inventorySlot, bool visualEffect)
    {
        if (inventorySlot.ItemObject.type == ItemType.Weapon) // ADD more equippable categories
        {
            WeaponObject weapon = (WeaponObject)inventorySlot.ItemObject;

            if (visualEffect)
            {
                if (_equippedWeaponSlot != null)
                {
                    _slotHolder.GetChild(_equippedWeaponSlot.SlotHolderChildPosition).GetComponent<Image>().color = Color.white;  // TODO dehighlight
                }
                _slotHolder.GetChild(inventorySlot.SlotHolderChildPosition).GetComponent<Image>().color = Color.green; // TODO highlight
            }

            _equippedWeaponSlot = inventorySlot;
            _player.SwitchAnimationController(weapon.animationType);
            _player.SetWeapons(weapon.model, weapon.positionOffset, weapon.animationType == AnimationType.Twohanded);
        }
    }

    public void ConsumeItem(int itemID)
    {
        InventorySlot inventorySlot = _playerInventoryContainer.GetSlotByItemID(itemID);

        ConsumeItemInSlot(inventorySlot);
    }

    private void ConsumeItemInSlot(InventorySlot inventorySlot)
    {
        bool consumed = false;

        if (inventorySlot.ItemObject.type == ItemType.Consumable) // ADD more equippable categories
        {
            ConsumableObject consumable = (ConsumableObject)inventorySlot.ItemObject;

            consumed = _player.RestoreHealth(consumable.healthRegen);
        }

        if (consumed)
        {
            if (_playerInventoryContainer.RemoveItem(inventorySlot.ItemObject.itemID))
            {
                _slotHolder.transform.GetChild(inventorySlot.SlotHolderChildPosition).gameObject.SetActive(false);

                if (_playerInventoryContainer.Slots.Count > 0)
                {
                    DisplayInfo(_playerInventoryContainer.Slots[0].ItemObject.itemID);
                }
            }
        }
    }

    public void BuyItem(int itemID)
    {
        InventorySlot inventorySlot = _secondaryShopInventoryContainer.GetSlotByItemID(itemID);

        if (_playerInventoryContainer.Coins < inventorySlot.ItemObject.price)
        {
            return;
        }

        if(inventorySlot.ItemObject.type == ItemType.Consumable)
        {
            // infinite stock do nothing
        }
        else if (_secondaryShopInventoryContainer.RemoveItem(inventorySlot.ItemObject.itemID)) {
            _slotHolder.transform.GetChild(inventorySlot.SlotHolderChildPosition).gameObject.SetActive(false);

            if (_secondaryShopInventoryContainer.Slots.Count > 0)
            {
                DisplayInfo(_secondaryShopInventoryContainer.Slots[0].ItemObject.itemID);
            }
        }

        _playerInventoryContainer.AddItem(inventorySlot.ItemObject, 1);

        RemoveCoinsFromPlayer(inventorySlot.ItemObject.price);
    }

    public bool RemoveItemIfInInventory(int itemID)
    {
        if (_playerInventoryContainer.HasItem(itemID))
        {
            _playerInventoryContainer.RemoveItem(itemID);

            return true;
        }

        return false;
    }

    public void PassStatsToPlayer()
    {
        _player.SetStats(GetFullEquipmentStats());
    }

    private CharacterStats GetFullEquipmentStats()
    {
        CharacterStats stats = new CharacterStats();

        if (CheckIfEquipped(_equippedWeaponSlot)) // ADD CATEGORY
        {
            stats.AddStats(EquipmentToStats(_equippedWeaponSlot.ItemObject));
        }

        return stats;
    }

    private CharacterStats EquipmentToStats(ItemObject equipment)
    {
        CharacterStats stats = new CharacterStats();

        if (equipment == null)
        {
            return stats;
        }

        if (equipment.type == ItemType.Weapon) // ADD CATEGORY
        {
            WeaponObject weapon = (WeaponObject)equipment;
            stats.Damage += weapon.damage;
            stats.Health += weapon.healthBonus;
            stats.ArmourPenetration += weapon.armourPenetration;
        }

        return stats;
    }

    #region Item List Manipulation // TODO move some?

    public void AddCoinsToPlayer(int amount)
    {
        _playerInventoryContainer.Coins += amount;
        _coinBalanceTMPT.text = _playerInventoryContainer.Coins.ToString();
    }

    public void RemoveCoinsFromPlayer(int amount)
    {
        _playerInventoryContainer.Coins -= amount;
        _coinBalanceTMPT.text = _playerInventoryContainer.Coins.ToString();
    }

    public void AddItem(ItemObject itemObject, int amount)
    {
        _playerInventoryContainer.AddItem(itemObject, amount);
    }

    public void RemoveItem(int itemObjectID)
    {
        _playerInventoryContainer.RemoveItem(itemObjectID);
    }

    public void RemoveTemporaryItems()
    {
        for (int i = 0; i < _playerInventoryContainer.Slots.Count; i++)
        {
            if (_playerInventoryContainer.Slots[i].ItemObject.type == ItemType.Quest)
            {
                _playerInventoryContainer.RemoveItemAtIndex(i);
            }
        }
    }

    public void Save()
    {
        if (_playerInventoryContainer != null)
        {
            _playerInventoryContainer.EquippedItemSlots = new List<InventorySlot>(); // ADD CATEGORY

            if(_equippedWeaponSlot != null)
            {
                _playerInventoryContainer.EquippedItemSlots.Add(_equippedWeaponSlot);
            }

            _playerInventoryContainer.Save();
        }

        if (_secondaryShopInventoryContainer != null)
        {
            _secondaryShopInventoryContainer.Save();
        }
    }

    public void Load(string path)
    {
        _playerInventoryContainer = new InventorySlotContainer(path);

        foreach (InventorySlot slot in _playerInventoryContainer.EquippedItemSlots)
        {
            EquipItemInSlot(slot, false);
        }

        PassStatsToPlayer();
    }

    public void LoadAndOpenShop(InventorySlotContainer container)
    {
        if (_secondaryShopInventoryContainer != null)
        {
            _secondaryShopInventoryContainer.Save();
        }

        _secondaryShopInventoryContainer = container;
        ShowInventory(true);
    }

    private void OnApplicationQuit()
    {
        Save();
    }

    #endregion

    #region UI Methods

    public void SwitchToInventoryTabUI()
    {
        Debug.Log("Player inventory");
        _isShop = false;
        DisplayInventory(_playerInventoryContainer);
    }

    public void SwitchToShopTabUI()
    {
        Debug.Log("Shop inventory");
        _isShop = true;
        DisplayInventory(_secondaryShopInventoryContainer);
    }

    public void HideInventoryUI()
    {
        _inventoryCanvas.enabled = false;
        _inGameCanvas.enabled = true;

        Save();
    }

    #endregion
}
