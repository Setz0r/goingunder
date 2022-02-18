using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public GameObject musicToggle;
    public GameObject sfxToggle;

    public void ToggleMusic()
    {
        AudioManager.instance.ToggleMusic(!musicToggle.GetComponent<Toggle>().isOn);
    }

    public void ToggleSFX()
    {
        AudioManager.instance.ToggleSFX(!sfxToggle.GetComponent<Toggle>().isOn);
    }

    public void StartGame()
    {
        GameSceneManager.instance.LoadGameplayScene();
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private void Start()
    {
        sfxToggle.GetComponent<Toggle>().SetIsOnWithoutNotify(!AudioManager.instance.sfxMuted);
        musicToggle.GetComponent<Toggle>().SetIsOnWithoutNotify(!AudioManager.instance.musicMuted);

        if (AudioManager.instance.backgroundMusic.isPlaying)
            return;

        AudioManager.instance.PlayMusic(MusicType.Gameplay);
    }
}
