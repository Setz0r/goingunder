using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[Serializable]
public enum MusicType : int
{
    None,
    MainMenu,
    Gameplay
}

[Serializable]
public enum SFXType : int
{
    None,
    Grow,
    Win,
    GameOver,
    Water,
    Poison,
    Undo
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public AudioMixer mainMixer;
    
    // Music
    public AudioSource backgroundMusic;
    public AudioClip menuMusic;
    public AudioClip gameMusic;

    // SFX
    public AudioSource growSound;
    public AudioSource winSound;
    public AudioSource gameOverSound;
    public AudioSource waterSound;
    public AudioSource poisonSound;
    public AudioSource undoSound;

    public void PlayMusic(MusicType type)
    {
        backgroundMusic.Stop();
        if (type == MusicType.MainMenu)
            backgroundMusic.clip = menuMusic;
        else if (type == MusicType.Gameplay)
            backgroundMusic.clip = gameMusic;
        backgroundMusic.Play();
    }

    public void StopMusic()
    {
        backgroundMusic.Stop();
    }

    public void PlaySound(SFXType type)
    {
        switch (type)
        {
            case SFXType.Grow:
                break;
            case SFXType.Win:
                break;
            case SFXType.GameOver:
                break;
            case SFXType.Water:
                break;
            case SFXType.Poison:
                break;
            case SFXType.Undo:
                break;
        }
    }

    public void StopSound(SFXType type)
    {

    }

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        DontDestroyOnLoad(this);
    }
}
