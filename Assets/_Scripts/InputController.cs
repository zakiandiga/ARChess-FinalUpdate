using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 *  This class handles player's input, such as selection of pieces and squares and moving the pieces
 */
public class InputController : MonoBehaviour
{
    string playerColor = "White";
    Transform selectedPiece; // reference to the selected piece, initially empty
    Transform selectedSquare; // reference to the selected square, initially empty;

    void selectSquare(Transform square)
    {
        deselectSquare(); //  make sure that previously selected square is deselected
        selectedSquare = square; // remember the selected square
        highlightValid(); // highlight newly selected square
    }

    void deselectSquare()
    {
        if (selectedSquare) // deselecting is only possible if a square was first selected
        {
            selectedSquare.GetComponent<MeshRenderer>().material.color = new Color(0, 0, 0, 0.0f); //change color to transparent (alpha = 0)
            selectedSquare = null; // forget the previosly selected square
        }
    }

    void highlightValid()
    {
        selectedSquare.GetComponent<MeshRenderer>().material.color = new Color(0, 1, 0, 0.3f); // highlight with green color
    }

    void highlightInvalid()
    {
        selectedSquare.GetComponent<MeshRenderer>().material.color = new Color(1, 0, 0, 0.3f); // highlight with red color
    }

    void selectPiece(Transform piece)
    {
        deselectPiece(); // first deselect the previosly selected piece, if any
        selectedPiece = piece; // remember selected piece

        Renderer[] renderers = selectedPiece.GetComponentsInChildren<Renderer>(); // get renderers of all visual elements of the selected piece
        foreach (Renderer r in renderers) // iterate over all renderers collected in the previous step
        {
            r.material.EnableKeyword("_EMISSION"); // enable light emission in the shader
            r.material.SetColor("_EmissionColor", new Color(0,0.5f,0,0.2f)); // set light emission to light, semi-transparent green
        }
    }

    void deselectPiece()
    {
        if (selectedPiece) // make sure that there is a piece to be deselected
        {
            Renderer[] renderers = selectedPiece.GetComponentsInChildren<Renderer>(); // get renderers of all visual elements of the selected piece
            foreach (Renderer r in renderers) // iterate over all renderers in collected in the previous step
            {
                r.material.DisableKeyword("_EMISSION"); // disable light emission in the shader
                r.material.SetColor("_EmissionColor", Color.black); // set light emission color to black
            }
        }
        selectedPiece = null; // forget the piece
    }

    // Update is called once per frame
    void Update()
    {

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); // create an infinite ray from camera, thorough screen, into the game word

        RaycastHit hit; // this object will be filled up with whatever the ray hits
        List<string> layers = new List<string>(); // create a list of layers we want to hit
        layers.Add("Square"); // add square layer 
        layers.Add(playerColor); // add player layer (either White or Black, depending who is the player)

        if (GameState.playersTurn && Physics.Raycast(ray, out hit, 100, LayerMask.GetMask(layers.ToArray()))) // perform the ray casting test. If anything was hit, it will be remembered in "hit" object
        {
            if (hit.transform.tag == "square" && selectedPiece) // check if the tag of the object we hit is "square"
            {
                selectSquare(hit.transform); // select the object that was hit (since we now know it is "square"). For now, we assume the square is a valid move
                if (GameState.isValidMove(selectedPiece, selectedSquare))
                {
                    if (Input.GetMouseButtonDown(0)) // check if the mouse button was pressed and whether the move is valid 
                    {
                        GameState.movePiece(selectedPiece, selectedSquare); // move the piece
                        deselectPiece(); // deselected the piece after it was moved
                        GameState.playersTurn = false;
                        GameState.aiTurn = true;

                    }
                }
                else
                {
                    highlightInvalid(); // in selectSquare, we assumed the the move is valid, but now we know it is not, so we highlight the square as invalid
                }
            }

            if (Input.GetMouseButtonDown(0) && hit.transform.tag == "piece" && hit.transform.name.Contains(playerColor)) // if we clicked on our piece 
            {
                selectPiece(hit.transform); // then we select that piece
                deselectSquare();
            }
        }


    }

}
