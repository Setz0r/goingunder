using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CutSceneManager : MonoBehaviour
{
    public static CutSceneManager instance;
    
    public GameObject cutscenePanel;
    public GameObject displayImage;
    public Sprite[] cutsceneImages;
    public Animator animator;

    public void PlayCutsceneSound(int cs)
    {
        switch(cs)
        {
            case 1:
                AudioManager.instance.PlaySound(SFXType.CS1);
                break;
            case 2:
                AudioManager.instance.PlaySound(SFXType.CS2);
                break;
            case 3:
                AudioManager.instance.PlaySound(SFXType.CS3);
                break;
            case 4:
                AudioManager.instance.PlaySound(SFXType.CS4);
                break;
        }
    }

    public void ShowCutscene(int number)
    {
        displayImage.GetComponent<Image>().sprite = cutsceneImages[number];
        animator.SetTrigger("ShowCutscene");
        PlayCutsceneSound(number + 1);
    }

    private void Awake()
    {
        instance = this;
    }

}
