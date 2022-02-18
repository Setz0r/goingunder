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
    Undo,
    Wall,
    DeathSound,
    Turn
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public AudioMixer mainMixer;

    // Music
    public bool musicMuted;
    public float musicVolume;
    public AudioSource backgroundMusic;
    public AudioClip menuMusic;
    public AudioClip gameMusic;

    // SFX
    public bool sfxMuted;
    public float sfxVolume;
    public AudioSource growSound;
    public AudioSource winSound;
    public AudioSource gameOverSound;
    public AudioSource waterSound;
    public AudioSource poisonSound;
    public AudioSource undoSound;
    public AudioSource wallSound;
    public AudioSource deathSound;
    public AudioSource turnSound;
    public AudioClip[] turnSounds;

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
                if (!growSound.isPlaying)
                    growSound.Play();
                break;
            case SFXType.Win:
                if (!winSound.isPlaying)
                    winSound.Play();
                break;
            case SFXType.GameOver:
                if (!gameOverSound.isPlaying)
                    gameOverSound.Play();
                break;
            case SFXType.Water:
                waterSound.Play();
                break;
            case SFXType.Poison:
                break;
            case SFXType.DeathSound:
                if (!deathSound.isPlaying)
                    deathSound.Play();
                break;
            case SFXType.Undo:
                break;
            case SFXType.Wall:
                wallSound.Play();
                break;
            case SFXType.Turn:
                turnSound.clip = turnSounds[UnityEngine.Random.Range(0, turnSounds.Length)];
                turnSound.Play();
                break;
        }
    }

    public void StopSound(SFXType type)
    {
        switch (type)
        {
            case SFXType.Grow:
                if (growSound.isPlaying)
                    growSound.Stop();
                break;
            case SFXType.Win:
                if (winSound.isPlaying)
                    winSound.Stop();
                break;
            case SFXType.GameOver:
                if (gameOverSound.isPlaying)
                    gameOverSound.Stop();
                break;
            case SFXType.Water:
                if (waterSound.isPlaying)
                    waterSound.Stop();
                break;
            case SFXType.Poison:
                break;
            case SFXType.DeathSound:
                if (deathSound.isPlaying)
                    deathSound.Stop();
                break;
            case SFXType.Undo:
                break;
            case SFXType.Wall:
                if (wallSound.isPlaying)
                    wallSound.Stop();
                break;
            case SFXType.Turn:
                if (turnSound.isPlaying)
                    turnSound.Stop();
                break;
        }

    }

    public static IEnumerator StartFade(AudioMixer audioMixer, string exposedParam, float duration, float targetVolume)
    {
        float currentTime = 0;
        float currentVol;
        audioMixer.GetFloat(exposedParam, out currentVol);
        currentVol = Mathf.Pow(10, currentVol / 20);
        float targetValue = Mathf.Clamp(targetVolume, 0.0001f, 1);
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            float newVol = Mathf.Lerp(currentVol, targetValue, currentTime / duration);
            audioMixer.SetFloat(exposedParam, Mathf.Log10(newVol) * 20);
            yield return null;
        }
        yield break;
    }

    public void FadeOutMusic(float speed)
    {
        StartCoroutine(StartFade(mainMixer, "MusicVolume", speed, 0));
    }

    public void FadeInMusic(float speed)
    {
        StartCoroutine(StartFade(mainMixer, "MusicVolume", speed, musicVolume));
    }

    public void ToggleMusic(bool muted)
    {
        backgroundMusic.mute = muted;
        musicMuted = muted;
    }

    public void ToggleSFX(bool muted)
    {
        mainMixer.SetFloat("SFXVolume", muted ? -80 : sfxVolume);
        sfxMuted = muted;
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);    
        } else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        mainMixer.SetFloat("MusicVolume", musicVolume);
        mainMixer.SetFloat("SFXVolume", sfxVolume);
    }

    private void Update()
    {

    }
}
