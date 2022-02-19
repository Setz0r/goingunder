using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public GameObject musicToggle;
    public GameObject sfxToggle;
    public GameObject quitButton;

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
        Fader.instance.FadeOut();
    }

    public void Quit()
    {
#if (UNITY_EDITOR || DEVELOPMENT_BUILD)
        Debug.Log(this.name + " : " + this.GetType() + " : " + System.Reflection.MethodBase.GetCurrentMethod().Name);
#endif
#if (UNITY_EDITOR)
        UnityEditor.EditorApplication.isPlaying = false;
#elif (UNITY_STANDALONE)
    Application.Quit();
#elif (UNITY_WEBGL)
    Application.OpenURL("about:blank");
#endif
    }

    public void QuitGame()
    {
        Quit();
    }

    private void Start()
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            quitButton.SetActive(false);
        }

        sfxToggle.GetComponent<Toggle>().SetIsOnWithoutNotify(!AudioManager.instance.sfxMuted);
        musicToggle.GetComponent<Toggle>().SetIsOnWithoutNotify(!AudioManager.instance.musicMuted);

        if (AudioManager.instance.backgroundMusic.isPlaying)
            return;

        AudioManager.instance.PlayMusic(MusicType.Gameplay);
    }
}
