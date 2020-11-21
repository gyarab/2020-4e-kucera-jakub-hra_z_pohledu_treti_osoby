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
    public Canvas menuCanvas;
    public Canvas saveSelectionCanvas;
    public TMP_InputField inputField;
    public GameObject levelUIPrefab;
    public GameObject deleteLevelUIPrefab;
    public float uiOffset;

    [Header("Miscelanious")]
    [SerializeField]
    private int _saveMaxCount;

    private List<string> _savedGames;
    private List<GameObject> _saveSlots;
    private List<GameObject> _deleteSlotButton;

    // Start is called before the first frame update
    void Start()
    {
        menuCanvas.enabled = true;
        saveSelectionCanvas.enabled = false;

        _savedGames = CheckForSavedGames();
    }

    private List<string> CheckForSavedGames()
    {
        List<string> directories = GameManager.Instance.GetSaves();

        return directories;
    }

    private bool CreateNewSave(string _directory) // TODO move to gui?
    {
        GameManager.Instance.CreateNewSave(_directory);

        return true;
    }

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
            _saveSlots.Add(Instantiate(levelUIPrefab, saveSelectionCanvas.transform));
            _saveSlots[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(_saveSlots[i].GetComponent<RectTransform>().anchoredPosition.x, _saveSlots[i].GetComponent<RectTransform>().anchoredPosition.y - i * uiOffset);
            _saveSlots[i].GetComponentInChildren<TextMeshProUGUI>().SetText(_savedGames[i]);
            int x = i;
            _saveSlots[i].GetComponent<Button>().onClick.AddListener(delegate { GetSaveUI(x); });

            _deleteSlotButton.Add(Instantiate(deleteLevelUIPrefab, saveSelectionCanvas.transform));
            _deleteSlotButton[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(_deleteSlotButton[i].GetComponent<RectTransform>().anchoredPosition.x, _deleteSlotButton[i].GetComponent<RectTransform>().anchoredPosition.y - i * uiOffset);
            _deleteSlotButton[i].GetComponent<Button>().onClick.AddListener(delegate { DeleteSaveUI(x); });
        }
    }

    // TODO
    private void LoadSave(string _path) // TODO loading bar?
    {
        saveSelectionCanvas.enabled = false;
        GameManager.Instance.LoadGame(_path);
    }

    #region Button Methods

    public void PlayUI()
    {
        menuCanvas.enabled = false;
        saveSelectionCanvas.enabled = true;
        DrawLevelUI();
    }

    // TODO
    public void AboutUI()
    {
        Debug.Log("About");
    }

    public void ExitUI()
    {
        Application.Quit();
    }

    public void BackUI()
    {
        menuCanvas.enabled = true;
        saveSelectionCanvas.enabled = false;
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
            string newSaveName = inputField.text.Trim();

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
