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

    [Header("DeathScreen"), SerializeField]
    private Canvas _deathScreenCanvas;
    [SerializeField, Range(0.1f, 2f)]
    private float _deathScreenRevealTime;
    [SerializeField]
    private float _deathScreenShowTime;

    private InventorySlotContainer _secondaryShopInventoryContainer;

    private string _savePath;
    private int _currentItem;
    private TextMeshProUGUI _equipBuyButtonTMPT;

    // Current equipment
    private InventorySlot _equippedWeaponSlot;

    #endregion

    // Inicializace
    public void Awake()
    {
        _equipBuyButtonTMPT = _equipBuyButton.GetComponentInChildren<TextMeshProUGUI>();
    }

    // Skryje grafické rozhraní a předá hráčově postavě staty
    public void Start()
    {
        HideInventoryUI();
        _bossHealthBar.SetVisibility(false);
        _deathScreenCanvas.enabled = false;
    }

    // Při aktivaci začne odebírat akci On Item Picked Up, která je vyvolána při zvednutí předmětu
    private void OnEnable()
    {
        GroundItem.OnItemPickedUp += AddItem;
    }

    // Při deaktivaci přestane odebírat akci On Item Picked Up
    private void OnDisable()
    {
        GroundItem.OnItemPickedUp -= AddItem;
    }

    #region Displaying

    // Zobrazí inventář a podle toho, jestli je to obchod, má uživatel jiné možnosti interakce s předměty v inventáři
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

    // Zobrazí informace o předmětu
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

        _itemNameTMPT.text = itemObject.itemName;
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
                // Equip or unequip
                if (CheckIfEquipped(id))
                {
                    _equipBuyButtonTMPT.SetText("Unequip");
                    _equipBuyButton.onClick.AddListener(delegate { UnequipItem(id); });
                } else
                {
                    _equipBuyButtonTMPT.SetText("Equip");
                    _equipBuyButton.onClick.AddListener(delegate { EquipItem(id); });
                }
            }

            // Change chanseStatsUI
            _currentStatsTMPT.text = _player.GetStats().StatsToStringColumn(false, false);

            // Change statsDelta
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

    // Zobrazí prázdné okno, protože v inventáři není žádný předmět
    private void DisplayInfoEmpty()
    {
        _itemNameTMPT.text = " ";
        _descriptionTMPT.text = " ";
        _equipBuyButton.onClick.RemoveAllListeners();
        _equipBuyButtonTMPT.SetText(" ");

        // Change chanseStatsUI
        _currentStatsTMPT.text = _player.GetStats().StatsToStringColumn(false, false);

        // Change statsDelta
        CharacterStats selectedObjectStats = new CharacterStats();
        _statsDeltaTMPT.text = selectedObjectStats.StatsToStringColumn(true, true);
    }

    // Zobrazí inventář
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
        } else
        {
            DisplayInfoEmpty();
        }
    }

    // Nastaví popis předmětu ve slotu
    public void SetSlotDescription(GameObject slot, InventorySlot item, int index)
    {
        slot.GetComponent<Image>().sprite = item.ItemObject.uiSprite;
        slot.GetComponent<Button>().onClick.RemoveAllListeners();
        slot.GetComponent<Image>().color = Color.white; // dehighlight

        item.SlotHolderChildPosition = index;
        int id = item.ItemObject.itemID;

        if (!_isShop)
        {
            if (CheckIfEquipped(item))
            {
                slot.GetComponent<Image>().color = Color.green; // highlight
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

    // Ukáže zpráva po smrti hráče
    public void ShowDeathScreen()
    {
        _deathScreenCanvas.enabled = true;
        StartCoroutine(HideDeathScreenAndReset());
    }

    // Skryje zpráva po smrti hráče
    private IEnumerator HideDeathScreenAndReset()
    {
        float timePassed = 0;
        CanvasGroup canvasGroup = _deathScreenCanvas.transform.GetComponent<CanvasGroup>();

        do
        {
            yield return null;
            timePassed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp(timePassed / _deathScreenRevealTime, 0f, 1f);
        } while (timePassed < _deathScreenRevealTime);

        timePassed = 0;

        while (timePassed < _deathScreenShowTime)
        {
            yield return null;
            timePassed += Time.deltaTime;
        }

        _deathScreenCanvas.enabled = false;
        GameManager.Instance.ReturnToHub(false, 0);
    }

    #endregion

    #region Item Usage

    // Vrcí true, když je předmět v slotu vybaven
    private bool CheckIfEquipped(InventorySlot slot)
    {
        if(_equippedWeaponSlot != null) // ADD category
        {
            if (slot.ItemObject.itemID == _equippedWeaponSlot.ItemObject.itemID)
            {
                return true;
            }
        }

        return false;
    }

    // Vrcí true, když je předmět v slotu vybaven
    private bool CheckIfEquipped(int id)
    {
        if (_equippedWeaponSlot != null) // ADD category
        {
            if (id == _equippedWeaponSlot.ItemObject.itemID)
            {
                return true;
            }
        }

        return false;
    }

    // Vybaví předmět podle ID
    public void EquipItem(int itemID)
    {
        InventorySlot inventorySlot = _playerInventoryContainer.GetSlotByItemID(itemID);

        EquipItemInSlot(inventorySlot, true);
        PassStatsToPlayer();
        DisplayInfo(itemID);
    }

    // Vybaví item v určitém slotu
    public void EquipItemInSlot(InventorySlot inventorySlot, bool visualEffect)
    {
        if (inventorySlot.ItemObject.type == ItemType.Weapon) // ADD more equippable categories
        {
            WeaponObject weapon = (WeaponObject)inventorySlot.ItemObject;

            if (visualEffect)
            {
                if (_equippedWeaponSlot != null)
                {
                    _slotHolder.GetChild(_equippedWeaponSlot.SlotHolderChildPosition).GetComponent<Image>().color = Color.white;  // dehighlight
                }
                _slotHolder.GetChild(inventorySlot.SlotHolderChildPosition).GetComponent<Image>().color = Color.green; // highlight
            }

            _equippedWeaponSlot = inventorySlot;
            _player.SwitchAnimationController(weapon.animationType);
            _player.SetWeapons(weapon.model, weapon.positionOffset, weapon.animationType == AnimationType.Twohanded);
        }
    }

    // Odvybaví předmět podle ID
    public void UnequipItem(int itemID)
    {
        InventorySlot inventorySlot = _playerInventoryContainer.GetSlotByItemID(itemID);

        UnequipItemInSlot(inventorySlot, true);
        PassStatsToPlayer();
        DisplayInfo(itemID);
    }

    // Vybaví item v určitém slotu
    public void UnequipItemInSlot(InventorySlot inventorySlot, bool visualEffect)
    {
        if (inventorySlot.ItemObject.type == ItemType.Weapon) // ADD more equippable categories
        {
            WeaponObject weapon = (WeaponObject)inventorySlot.ItemObject;

            if (visualEffect)
            {
                _slotHolder.GetChild(inventorySlot.SlotHolderChildPosition).GetComponent<Image>().color = Color.white;  // dehighlight
            }

            _equippedWeaponSlot = null;
            _player.SwitchAnimationController(AnimationType.Fists);
            _player.RemoveWeapons();
        }
    }

    // Spotřebuje předmět s určitým ID
    public void ConsumeItem(int itemID)
    {
        InventorySlot inventorySlot = _playerInventoryContainer.GetSlotByItemID(itemID);

        ConsumeItemInSlot(inventorySlot);
    }

    // Spotřebuje předmět ve slotu
    private void ConsumeItemInSlot(InventorySlot inventorySlot)
    {
        bool consumed = false;

        if (inventorySlot.ItemObject.type == ItemType.Consumable) // ADD more categories
        {
            ConsumableObject consumable = (ConsumableObject)inventorySlot.ItemObject;

            consumed = _player.RestoreHealth(consumable.healthRegen);
        }

        if (consumed)
        {
            GameObject slot = _slotHolder.transform.GetChild(inventorySlot.SlotHolderChildPosition).gameObject;

            if (_playerInventoryContainer.RemoveItem(inventorySlot.ItemObject.itemID))
            {
                slot.SetActive(false);

                if (_playerInventoryContainer.Slots.Count > 0)
                {
                    DisplayInfo(_playerInventoryContainer.Slots[0].ItemObject.itemID);
                }
            } else
            {
                if (inventorySlot.Amount > 1)
                {
                    slot.GetComponentInChildren<TextMeshProUGUI>().enabled = true;
                    slot.GetComponentInChildren<TextMeshProUGUI>().text = inventorySlot.Amount.ToString();
                }
                else
                {
                    slot.GetComponentInChildren<TextMeshProUGUI>().enabled = false;
                }
            }
        }
    }

    // Odebere předmět z obchodu a hráči peníze a přidá předmět hráčovi do inventáře
    public void BuyItem(int itemID)
    {
        InventorySlot inventorySlot = _secondaryShopInventoryContainer.GetSlotByItemID(itemID);

        if (_playerInventoryContainer.Coins < inventorySlot.ItemObject.price)
        {
            return;
        }

        if(inventorySlot.ItemObject.type == ItemType.Consumable)
        {
            // infinite stock, do nothing
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

    #endregion

    #region Stats

    // Předá hráči data vybavenách předmětů
    public void PassStatsToPlayer()
    {
        _player.SetStats(GetFullEquipmentStats());
    }

    // Vrací součet hodnot vybavených předmětů
    private CharacterStats GetFullEquipmentStats()
    {
        CharacterStats stats = new CharacterStats();

        if (CheckIfEquipped(_equippedWeaponSlot)) // ADD CATEGORY
        {
            stats.AddStats(EquipmentToStats(_equippedWeaponSlot.ItemObject));
        }

        return stats;
    }

    // Převede hodnoty vybaveného předmětu na Character Stats
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

    #endregion

    #region Item List Manipulation

    // Přidá hráčovi peníze
    public void AddCoinsToPlayer(int amount)
    {
        _playerInventoryContainer.Coins += amount;
        _coinBalanceTMPT.text = _playerInventoryContainer.Coins.ToString();
    }

    // Odebere hráčovi peníze
    public void RemoveCoinsFromPlayer(int amount)
    {
        _playerInventoryContainer.Coins -= amount;
        _coinBalanceTMPT.text = _playerInventoryContainer.Coins.ToString();
    }

    // Přidá určité množství předmětů do inventáře
    public void AddItem(ItemObject itemObject, int amount)
    {
        _playerInventoryContainer.AddItem(itemObject, amount);
    }

    // Odstraní předmět z inventáře
    public void RemoveItem(int itemObjectID)
    {
        _playerInventoryContainer.RemoveItem(itemObjectID);
    }

    // Odstaní dočasné předměty
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

    // Odstraní předmět, pokud je u hráče v inventáři; vrací hodnotu, jestli se podařilo předmět odstranit
    public bool RemoveItemIfInInventory(int itemID)
    {
        if (_playerInventoryContainer.HasItem(itemID))
        {
            _playerInventoryContainer.RemoveItem(itemID);

            return true;
        }

        return false;
    }

    #endregion

    #region Saving and retrieving

    // Uloží stav inventářů
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

    // Načte stav inventářů ze souboru
    public void Load(string path)
    {
        _playerInventoryContainer = new InventorySlotContainer(path);

        foreach (InventorySlot slot in _playerInventoryContainer.EquippedItemSlots)
        {
            EquipItemInSlot(slot, false);
        }
        
        if(_equippedWeaponSlot == null)
        {
            _player.SwitchAnimationController(AnimationType.Fists);
        }

        PassStatsToPlayer();
    }

    // Načte stav inventáře obchodu ze souboru
    public void LoadAndOpenShop(InventorySlotContainer container)
    {
        if (_secondaryShopInventoryContainer != null)
        {
            _secondaryShopInventoryContainer.Save();
        }

        _secondaryShopInventoryContainer = container;
        ShowInventory(true);
    }

    // Uloží inventář při vypnutí aplikace
    private void OnApplicationQuit()
    {
        Save();
    }

    #endregion

    #region UI Methods

    // Změní okno v inventáři na okno s inventářem hráče
    public void SwitchToInventoryTabUI()
    {
        _isShop = false;
        DisplayInventory(_playerInventoryContainer);
    }

    //Změní okno v inventáři na okno s inventářem obchodu
    public void SwitchToShopTabUI()
    {
        _isShop = true;
        DisplayInventory(_secondaryShopInventoryContainer);
    }

    // Skryje inventář
    public void HideInventoryUI()
    {
        _inventoryCanvas.enabled = false;
        _inGameCanvas.enabled = true;

        Save();
    }

    // Vrátí hráče do menu
    public void ExitToMenuUI()
    {
        Save();
        GameManager.Instance.ReturnToMenu();
    }

    #endregion
}
