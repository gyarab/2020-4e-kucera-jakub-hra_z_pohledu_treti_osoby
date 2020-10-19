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
    public Canvas menuCanvas;
    public Canvas saveSelectionCanvas;
    public Canvas loadingCanvas;
    public TMP_InputField inputField;
    public GameObject levelUIPrefab;
    public GameObject deleteLevelUIPrefab;
    public float uiOffset;

    [Header("Miscelanious")]
    public int saveMaxCount;
    private List<string> savedGames;

    private List<GameObject> saveSlots;
    private List<GameObject> deleteSlotButton;

    // Start is called before the first frame update
    void Start()
    {
        menuCanvas.enabled = true;
        saveSelectionCanvas.enabled = false;
        loadingCanvas.enabled = false;

        savedGames = CheckForSavedGames();
    }


    private List<string> CheckForSavedGames()
    {
        List<string> directories = GameManager.Instance.GetSaves();

        return directories;
    }

    private bool CreateNewSave(string _directory) // TODO move to gui?
    {
        GameManager.Instance.CreateNewSave(_directory);

        // Save all Inventories and map Chunks to the folder TODO

        return true;
    }

    private void DrawLevelUI()
    {
        if(saveSlots != null)
        {
            for (int i = 0; i < saveSlots.Count; i++)
            {
                Destroy(saveSlots[i]);
                Destroy(deleteSlotButton[i]);
            }
        }

        saveSlots = new List<GameObject>();
        deleteSlotButton = new List<GameObject>();

        for (int i = 0; i < savedGames.Count; i++)
        {
            saveSlots.Add(Instantiate(levelUIPrefab, saveSelectionCanvas.transform));
            saveSlots[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(saveSlots[i].GetComponent<RectTransform>().anchoredPosition.x, saveSlots[i].GetComponent<RectTransform>().anchoredPosition.y - i * uiOffset);
            saveSlots[i].GetComponentInChildren<TextMeshProUGUI>().SetText(savedGames[i]);
            int x = i;
            saveSlots[i].GetComponent<Button>().onClick.AddListener(delegate { GetSaveUI(x); });

            deleteSlotButton.Add(Instantiate(deleteLevelUIPrefab, saveSelectionCanvas.transform));
            deleteSlotButton[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(deleteSlotButton[i].GetComponent<RectTransform>().anchoredPosition.x, deleteSlotButton[i].GetComponent<RectTransform>().anchoredPosition.y - i * uiOffset);
            deleteSlotButton[i].GetComponent<Button>().onClick.AddListener(delegate { DeleteSaveUI(x); });
        }
    }

    // TODO
    private void LoadSave(string _path) // TODO loading bar?
    {
        // Read current Player's location and load that scene TODO
        // Create Game Manager and pass bool to instantiate player TODO
        //SceneManager.LoadScene("Game");

        loadingCanvas.enabled = true;
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
        LoadSave(savedGames[_position]);
    }

    public void DeleteSaveUI(int _position)
    {
        GameManager.Instance.DeleteSave(savedGames[_position]);
        savedGames.RemoveAt(_position);
        DrawLevelUI();
    }

    public void CreateSaveUI()
    {
        if(savedGames.Count < saveMaxCount)
        {
            string newSaveName = inputField.text.Trim();

            if (!newSaveName.Equals(""))
            {
                foreach (string saveName in savedGames)
                {
                    if (saveName == newSaveName)
                    {
                        Debug.Log("Save name already exists"); // Tell user TODO
                        return;
                    }
                }
                if (CreateNewSave(newSaveName))
                {
                    savedGames.Add(newSaveName);
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
