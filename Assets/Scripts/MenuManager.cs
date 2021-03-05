using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MenuManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField]
    private Canvas _menuCanvas;
    [SerializeField]
    private Canvas _saveSelectionCanvas, _settingsCanvas, _controlsCanvas;
    [SerializeField]
    private TMP_InputField _inputField;
    [SerializeField]
    private TextMeshProUGUI _errorTMPT;
    [SerializeField]
    private Slider _xSensitivitySlider, _ySensitivitySlider;
    [SerializeField]
    private GameObject _levelUIPrefab, _deleteLevelUIPrefab;
    [SerializeField]
    private float _uiOffset, _errorMessageDisplayTime;

    [Header("Miscelanious")]
    [SerializeField]
    private int _saveMaxCount;

    private List<string> _savedGames;
    private List<GameObject> _saveSlots;
    private List<GameObject> _deleteSlotButton;
    private SaveableSettings _settings;

    // Inicializace, viditelná je úvodní obrazovka
    void Start()
    {
        _menuCanvas.enabled = true;
        _saveSelectionCanvas.enabled = false;
        _settingsCanvas.enabled = false;
        _controlsCanvas.enabled = false;

        _errorTMPT.enabled = false;

        _savedGames = CheckForSavedGames();
        _settings = GameManager.Instance.LoadSettings();
        _xSensitivitySlider.value = _settings.xSensitivity;
        _ySensitivitySlider.value = _settings.ySensitivity;
    }

    // Zavolá metodu v GameManageru, aby zjistil, jestli jsou na zařízení uložené postupy ve hře
    private List<string> CheckForSavedGames()
    {
        List<string> directories = GameManager.Instance.GetSaves();

        return directories;
    }

    // Zavolá metodu v GameManageru, která vytvoří nový uložený postup
    private bool CreateNewSave(string _directory) 
    {
        GameManager.Instance.CreateNewSave(_directory);

        return true;
    }

    // Vykreslí talčítka na vybýraní uložených postupů ve hře
    private void DrawLevelUI()
    {
        if(_saveSlots != null)
        {
            for (int i = 0; i < _saveSlots.Count; i++)
            {
                Destroy(_saveSlots[i]);
                Destroy(_deleteSlotButton[i]);
            }
        }

        _saveSlots = new List<GameObject>();
        _deleteSlotButton = new List<GameObject>();

        for (int i = 0; i < _savedGames.Count; i++)
        {
            _saveSlots.Add(Instantiate(_levelUIPrefab, _saveSelectionCanvas.transform));
            _saveSlots[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(_saveSlots[i].GetComponent<RectTransform>().anchoredPosition.x, _saveSlots[i].GetComponent<RectTransform>().anchoredPosition.y - i * _uiOffset);
            _saveSlots[i].GetComponentInChildren<TextMeshProUGUI>().SetText(_savedGames[i]);
            int x = i;
            _saveSlots[i].GetComponent<Button>().onClick.AddListener(delegate { GetSaveUI(x); });

            _deleteSlotButton.Add(Instantiate(_deleteLevelUIPrefab, _saveSelectionCanvas.transform));
            _deleteSlotButton[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(_deleteSlotButton[i].GetComponent<RectTransform>().anchoredPosition.x, _deleteSlotButton[i].GetComponent<RectTransform>().anchoredPosition.y - i * _uiOffset);
            _deleteSlotButton[i].GetComponent<Button>().onClick.AddListener(delegate { DeleteSaveUI(x); });
        }
    }

    // Zavolá metodu v GameManageru, která načte hru na příslušné cestě
    private void LoadSave(string _path)
    {
        _saveSelectionCanvas.enabled = false;
        GameManager.Instance.LoadGame(_path, _settings);
    }

    #region Button Methods

    // Funkce talčítka, která zobrazí stránku s uloženými postupy
    public void PlayUI()
    {
        _menuCanvas.enabled = false;
        _saveSelectionCanvas.enabled = true;
        DrawLevelUI();
    }

    // Otevře nastavení
    public void SettingsUI()
    {
        _menuCanvas.enabled = false;
        _settingsCanvas.enabled = true;
    }

    // Zobrazí okno s ovládáním
    public void ControlsUI()
    {
        _menuCanvas.enabled = false;
        _controlsCanvas.enabled = true;
    }

    // Vypne aplikaci
    public void ExitUI()
    {
        Application.Quit();
    }

    // Návrat do menu
    public void BackUI()
    {
        _menuCanvas.enabled = true;
        _saveSelectionCanvas.enabled = false;
        _controlsCanvas.enabled = false;
    }

    // Uloží nastavení a zobrazí úvodní obrazovku
    public void SaveAndExitUI()
    {
        GameManager.Instance.SaveSettings(_settings);

        _menuCanvas.enabled = true;
        _settingsCanvas.enabled = false;
    }

    // Změní hodnotu citlivosti na ose x
    public void OnXSensitivityChangedUI()
    {
        _settings.xSensitivity = _xSensitivitySlider.value;
    }

    // Změní hodnotu citlivosti na ose y
    public void OnYSensitivityChangedUI()
    {
        _settings.ySensitivity = _ySensitivitySlider.value;
    }

    // Načte hru na určité pozici
    public void GetSaveUI(int _position)
    {
        LoadSave(_savedGames[_position]);
    }

    // Smaže uložený postup na indexu
    public void DeleteSaveUI(int _position)
    {
        GameManager.Instance.DeleteSave(_savedGames[_position]);
        _savedGames.RemoveAt(_position);
        DrawLevelUI();
    }

    // Vytvoří nový save, když jsou splněné podmínky
    public void CreateSaveUI()
    {
        if(_savedGames.Count < _saveMaxCount)
        {
            string newSaveName = _inputField.text.Trim();

            if (!newSaveName.Equals(""))
            {
                foreach (string saveName in _savedGames)
                {
                    if (saveName == newSaveName)
                    {
                        StartCoroutine(ShowAndHideErrorMessage("Save name already exists"));
                        return;
                    }
                }
                if (CreateNewSave(newSaveName))
                {
                    _savedGames.Add(newSaveName);
                    DrawLevelUI();
                }
            } else
            {
                StartCoroutine(ShowAndHideErrorMessage("Invalid Name"));
            }
        }
        else
        {
            StartCoroutine(ShowAndHideErrorMessage("Slots are full"));
        }
    }

    #endregion

    #region Coroutines

    // Zobrazí Canvas se zprávou pro uživatele a po chvíli ho skryje
    private IEnumerator ShowAndHideErrorMessage(string message)
    {
        _errorTMPT.text = message;
        _errorTMPT.enabled = true;
        yield return new WaitForSecondsRealtime(_errorMessageDisplayTime);
        _errorTMPT.enabled = false;
    }

    #endregion
}
