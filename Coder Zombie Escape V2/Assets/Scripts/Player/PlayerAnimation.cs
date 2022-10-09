using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    private Animator playerAnimator;
    private string actualAnimation; 


    void Awake()
    {
        playerAnimator = GetComponent<Animator>();
    }

    private void changeAnimation(string newAnimation)
    {
        //chequeo si la animación actual es la animación que quiero reproducir
        if(actualAnimation == newAnimation)
        {
            return;
        }

        //si no lo es ejecuto el método
        playerAnimator.Play(newAnimation);
        actualAnimation = newAnimation;
    }

    //creo un método para cada animación
    public void showAnimationIdle()
    {
        changeAnimation("IDLE");
    }

    public void showAnimationRun()
    {
        changeAnimation("Run");
    }

    public void showAnimationSlide()
    {
        changeAnimation("Slide");
    }

    public void showAnimationJump()
    {
        changeAnimation("Jump");
    }

    public void showAnimationHit()
    {
        changeAnimation("Hit");
    }
}
