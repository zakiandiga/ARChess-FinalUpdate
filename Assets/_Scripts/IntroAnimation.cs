using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntroAnimation : MonoBehaviour
{
    public Animator cameraAnimator;
    public KeyCode StarterW;
    public KeyCode StarterB;
    const string WhitesPlay = "WhitesPlay";
    const string BlacksPlay = "BlacksPlay";
    
    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(StarterW))
        {
            cameraAnimator.SetTrigger(WhitesPlay);
        }
        if (Input.GetKeyDown(StarterB))
        {
            cameraAnimator.SetTrigger(BlacksPlay);
        }
    }
}
