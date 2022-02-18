using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Fader : MonoBehaviour
{    
    public static Fader instance;
    public Animator animator;

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
}
