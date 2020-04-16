using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntroAnimation : MonoBehaviour
{
    public Animator cameraAnimator;
    public KeyCode StarterW;
    public KeyCode StarterB;
    public MenuInteraction menuInteraction;
    const string WhitesPlay = "WhitesPlay";
    const string BlacksPlay = "BlacksPlay";
    
    // Start is called before the first frame update
    void Start()
    {
        menuInteraction = GameObject.Find("StartManager").GetComponent<MenuInteraction>();
        if (menuInteraction.colorChoose == 1)
        {
            cameraAnimator.SetTrigger(WhitesPlay);

        }

        if (menuInteraction.colorChoose == 2)
        {
            cameraAnimator.SetTrigger(BlacksPlay);
        }
    }
    


}
