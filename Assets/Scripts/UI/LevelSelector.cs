using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelector : MonoBehaviour
{
    public static LevelSelector instance;

    public GameObject DigitOne;
    public GameObject DigitTwo;

    public Sprite Zero;
    public Sprite One;
    public Sprite Two;
    public Sprite Three;
    public Sprite Four;
    public Sprite Five;
    public Sprite Six;
    public Sprite Seven;
    public Sprite Eight;
    public Sprite Nine;

    public Sprite[] Numbers;

    public int MaxLevel = 16;
    public int MaxAvailableLevel = 1;
    public int SelectedLevel = 1;

    int[] GetIntArray(int num)
    {
        List<int> listOfInts = new List<int>();
        while (num > 0)
        {
            listOfInts.Add(num % 10);
            num = num / 10;
        }
        listOfInts.Reverse();
        return listOfInts.ToArray();
    }

    public void UpdateLevel(int level)
    {        
        int[] NumArray = GetIntArray(level);
        DigitOne.GetComponent<Image>().sprite = Zero;
        if (NumArray.Length > 1)
        {
            DigitOne.GetComponent<Image>().sprite = Numbers[NumArray[0]];
            DigitTwo.GetComponent<Image>().sprite = Numbers[NumArray[1]];
        }
        else
        {
            DigitTwo.GetComponent<Image>().sprite = Numbers[NumArray[0]];
        }
        SaveManager.instance.SelectedLevel = level;
    }

    public int HighestLevel
    {
        get
        {
            return Mathf.Min(MaxAvailableLevel, MaxLevel);
        }
    }

    public void RightPress()
    {
        AudioManager.instance.PlaySound(SFXType.Click);
        if (SelectedLevel + 1 > HighestLevel)
            SelectedLevel = HighestLevel;
        else
            SelectedLevel++;
        UpdateLevel(SelectedLevel);
    }

    public void LeftPress()
    {
        AudioManager.instance.PlaySound(SFXType.Click);
        if (SelectedLevel - 1 < 1)
            SelectedLevel = 1;
        else
            SelectedLevel--;
        UpdateLevel(SelectedLevel);
    }

    private void Awake()
    {
        instance = this;
        Numbers = new Sprite[10]
        {
            Zero,One,Two,Three,Four,Five,Six,Seven,Eight,Nine
        };
    }

    private void Start()
    {
        Tuple<int,int> loadedData = SaveManager.instance.LoadData();

        SelectedLevel = SaveManager.instance.SelectedLevel > 1 ? SaveManager.instance.SelectedLevel : loadedData.Item1;
        MaxAvailableLevel = loadedData.Item2;

        UpdateLevel(SelectedLevel);
    }
}
