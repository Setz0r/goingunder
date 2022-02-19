using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

public class Fader : MonoBehaviour
{    
    public static Fader instance;
    public Animator animator;

    [Serializable]
    public class FaderFadedEvent : UnityEvent { }

    [FormerlySerializedAs("onFade")]
    [SerializeField]
    private FaderFadedEvent m_OnFaded = new FaderFadedEvent();

    public FaderFadedEvent onFaded
    {
        get { return m_OnFaded; }
        set { m_OnFaded = value; }
    }
    private void Awake()
    {
        instance = this;
    }

    public void FadeOut()
    {
        animator.Play("FadeOut");
    }

    public void FadeIn()
    {
        animator.Play("FadeIn");
    }

    public void FadedOut()
    {
        m_OnFaded.Invoke();
    }

    private void OnEnable()
    {
        FadeIn();
    }

    private void Start()
    {
    }
}
