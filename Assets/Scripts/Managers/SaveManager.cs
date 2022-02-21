using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager instance;

    public int MaxAvailableLevel = 1;
    public int CurrentLevel = 1;
    public int SelectedLevel = 1;

    public Tuple<int,int> LoadData()
    {
        int currentLevel = PlayerPrefs.GetInt("CurrentLevel");
        if (currentLevel == 0)
            currentLevel = 1;

        int maxLevel = PlayerPrefs.GetInt("MaxLevel");
        if (maxLevel == 0)
            maxLevel = 1;

        CurrentLevel = currentLevel;
        MaxAvailableLevel = maxLevel;
        Debug.Log("Game data loaded! : " + currentLevel + " : " + maxLevel);
        return new Tuple<int,int>(currentLevel, maxLevel);
    }

    public void SaveData(int currentLevel, int maxLevel)
    {
        PlayerPrefs.SetInt("CurrentLevel", currentLevel);
        PlayerPrefs.SetInt("MaxLevel", maxLevel);
        PlayerPrefs.Save();
        Debug.Log("Game data saved! : " + currentLevel + " : " + maxLevel);
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
}