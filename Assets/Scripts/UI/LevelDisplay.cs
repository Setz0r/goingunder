using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelDisplay : MonoBehaviour
{
    public static LevelDisplay instance;

    public GameObject LDBackground;
    public GameObject LDOverlay;
    public GameObject DigitOne;
    public GameObject DigitTwo;

    public float ShowDelay = 2f;

    public Animator animator;

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

    public void Show(int level)
    {
        LDOverlay.SetActive(true);
        int[] NumArray = GetIntArray(level);
        DigitOne.GetComponent<Image>().sprite = Zero;
        if (NumArray.Length > 1)
        {
            DigitOne.GetComponent<Image>().sprite = Numbers[NumArray[0]];
            DigitTwo.GetComponent<Image>().sprite = Numbers[NumArray[1]];
        } else
        {
            DigitTwo.GetComponent<Image>().sprite = Numbers[NumArray[0]];
        }
        animator.SetBool("Shown", true);
        StartCoroutine(Hide());
    }

    public IEnumerator Hide()
    {
        yield return new WaitForSeconds(ShowDelay);
        animator.SetBool("Shown", false);
        LDOverlay.SetActive(false);
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
    }

}
