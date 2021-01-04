using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MenuManager : MonoBehaviour
{
    [Header("UI")] // TODO change to serialized
    [SerializeField]
    private Canvas _menuCanvas;
    [SerializeField]
    private Canvas _saveSelectionCanvas;
    [SerializeField]
    private Canvas _settingsCanvas;
    [SerializeField]
    private TMP_InputField _inputField;
    [SerializeField]
    private Slider _xSensitivitySlider;
    [SerializeField]
    private Slider _ySensitivitySlider;
    [SerializeField]
    private GameObject levelUIPrefab;
    [SerializeField]
    private GameObject deleteLevelUIPrefab;
    [SerializeField]
    private float uiOffset;

    [Header("Miscelanious")]
    [SerializeField]
    private int _saveMaxCount;

    private List<string> _savedGames;
    private List<GameObject> _saveSlots;
    private List<GameObject> _deleteSlotButton;
    private SaveableSettings _settings;

    // Start is called before the first frame update
    void Start()
    {
        _menuCanvas.enabled = true;
        _saveSelectionCanvas.enabled = false;
        _settingsCanvas.enabled = false;

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
    private bool CreateNewSave(string _directory) // TODO move to gui?
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
            _saveSlots.Add(Instantiate(levelUIPrefab, _saveSelectionCanvas.transform));
            _saveSlots[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(_saveSlots[i].GetComponent<RectTransform>().anchoredPosition.x, _saveSlots[i].GetComponent<RectTransform>().anchoredPosition.y - i * uiOffset);
            _saveSlots[i].GetComponentInChildren<TextMeshProUGUI>().SetText(_savedGames[i]);
            int x = i;
            _saveSlots[i].GetComponent<Button>().onClick.AddListener(delegate { GetSaveUI(x); });

            _deleteSlotButton.Add(Instantiate(deleteLevelUIPrefab, _saveSelectionCanvas.transform));
            _deleteSlotButton[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(_deleteSlotButton[i].GetComponent<RectTransform>().anchoredPosition.x, _deleteSlotButton[i].GetComponent<RectTransform>().anchoredPosition.y - i * uiOffset);
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

    // TODO rework to settings
    public void SettingsUI()
    {
        _menuCanvas.enabled = false;
        _settingsCanvas.enabled = true;
    }

    public void ExitUI()
    {
        Application.Quit();
    }

    public void BackUI()
    {
        _menuCanvas.enabled = true;
        _saveSelectionCanvas.enabled = false;
    }

    public void SaveAndExitUI()
    {
        GameManager.Instance.SaveSettings(_settings);

        _menuCanvas.enabled = true;
        _settingsCanvas.enabled = false;
    }

    public void OnXSensitivityChangedUI()
    {
        _settings.xSensitivity = _xSensitivitySlider.value;
    }

    public void OnYSensitivityChangedUI()
    {
        _settings.ySensitivity = _ySensitivitySlider.value;
    }

    public void GetSaveUI(int _position)
    {
        LoadSave(_savedGames[_position]);
    }

    public void DeleteSaveUI(int _position)
    {
        GameManager.Instance.DeleteSave(_savedGames[_position]);
        _savedGames.RemoveAt(_position);
        DrawLevelUI();
    }

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
                        Debug.Log("Save name already exists"); // Tell user TODO
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
                Debug.Log("Invalid Name"); // TODO
            }
        }
        else
        {
            Debug.Log("Slots are full"); // TODO
        }
    }

    #endregion
}
