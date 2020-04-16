using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuInteraction : MonoBehaviour
{
    public int colorChoose;
    //white = 1
    //black = 2
    public void loadLevel()
    {

        DontDestroyOnLoad(gameObject);
        SceneManager.LoadScene("Main", LoadSceneMode.Single);
        
    }

    public void selectWhite()
    {
        colorChoose = 1;
    }

    public void selectBlack()
    {
        colorChoose = 2;
    }


}
