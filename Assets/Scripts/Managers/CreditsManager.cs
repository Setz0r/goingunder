using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreditsManager : MonoBehaviour
{
    public GameObject CreditsImage;
    bool goingToMainMenu;

    void Start()
    {
        AudioManager.instance.PlayMusic(MusicType.Credits);        
        AudioManager.instance.FadeInMusic(1);
        CreditsImage.GetComponent<Animator>().SetBool("ShowCredit", true);
    }

    IEnumerator GotoMainMenu()
    {
        AudioManager.instance.FadeOutMusic(1);
        yield return new WaitForSeconds(1);
        GameSceneManager.instance.LoadMainMenuScene();
    }

    private void Update()
    {
        if (!goingToMainMenu && Input.GetKeyDown(KeyCode.Escape))
        {
            goingToMainMenu = true;
            StartCoroutine(GotoMainMenu());
        }
    }
}
