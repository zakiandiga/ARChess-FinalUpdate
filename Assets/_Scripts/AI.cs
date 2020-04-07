using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI : MonoBehaviour
{
    string ai_player = "Black"; // name of the player, "Black" by default, but we can change it at a start of the game
    Transform selectedPiece = null; // current valid piece to move
    Transform selectedSquare = null; // curent valid destination for our piece
    

    // Start is called before the first frame update
    void Start()
    {

    }

    // this fuction tries to find a chessboard square where the current figure could be placed. As we painfully learned in class, it is not always possible
    // to find a valid destination square for a piece
    bool selectSquare(Transform potentiallySelectedPiece) { // 

        const int MAX_ATTEMPTS = 300; // we will try to find a destination maximum try 300 times
        int attemptCount = 0; // counter for how many times we tried already

        // Random square approach
        while (attemptCount < MAX_ATTEMPTS) { // as long as we didn't find a valid destination square, and we did not exhaust all attempts...
            attemptCount ++; // increase attempts counter
            int i = Random.Range(0,8); // get random column
            int j = Random.Range(0,8); // get random row
            Debug.Log("SKIPPY: How about I try to move " + potentiallySelectedPiece.name+ " to the square at row "+j+ " and column "+i);
            if (GameState.isValidMove(potentiallySelectedPiece, GameState.chessboard[i,j])) { // check if the randmly selected destination square works for our potentially selected piece
                selectedSquare = GameState.chessboard[i,j]; // hurray! we found a valid destination for our piece!
                Debug.Log("SKIPPY: Finally! For " + potentiallySelectedPiece.name+ " the square at row "+j+ " and column "+i+" is a valid move!");
                return true; // since we are done, let's exit the function and return true, that is, a confirmation that a piece was successfully selected
            }
            Debug.Log("SKIPPY: Ok, ok, I get it, that was not a good idea!");
        }

        Debug.Log ("SKIPPY: Ugh! All my random attempts failed... Let's try to be more systematic.");
        // we did not exit the function yet? well, then let's try a systematic approach
        // that is, trying each and every square in the chessboard until we find (or not, if there is none) a valid destination piece

        for (int i=0; i < 8; ++i)  // for every column of the chessboard
        {
            for (int j=0; j < 8; ++j) // and every row of the chessboard
            {
                Debug.Log("SKIPPY: How about I try to move " + potentiallySelectedPiece.name+ " to the square at row "+j+ " and column "+i);
                if (GameState.isValidMove(potentiallySelectedPiece, GameState.chessboard[i, j]))
                { //check if that square is a valid destination for our piece
                    selectedSquare = GameState.chessboard[i, j]; // hurray! we found a destination square that works for or piece!
                    Debug.Log("Selected square: " + selectedSquare.name);
                    Debug.Log("SKIPPY: Awesome! For " + potentiallySelectedPiece.name + " the square at row " + j + " and column " + i + " is a valid move! Being systematic paid off!");
                    return true; // since we are done, ;et's exit the while loop
                }
            }
            Debug.Log("SKIPPY: Well, row "+i+" did not work... Let's try the next one");
        }
     
        Debug.Log("SKIPPY: Well, clearly, there is no valid move for this piece of s*** chess");
        return false; // sadly, there is no valid destination square for our potentiallySelectedPiece
    }

    bool selectPiece()
    {

        int attemptCount = 0;
        const int MAX_ATTEMPTS = 20; // we will try to select piece 100 times. It is very unlikely that we will not succeed. But 100 is a lot of times, since 
                                    // for every potential piece we are doing, at worst, up to 300 random tries to find a square, and up to 64 systematic trials to 
                                    // find a square. If we were really not luckly, we may be running that function 20*(300+64) times! Piece of cake for a modern PC...
        
        Debug.Log("SKIPPY: My turn b***. I will crush you! Let's first try a random piece");
        // let's first try a random approach: select random piece and see if it has a correct destination.
        while (attemptCount < MAX_ATTEMPTS) // as long as we didn't find a piece that can move, and we did not exhaust all attempts...
        {  
            attemptCount++; // increase attempts count
            int pieceIndex = Random.Range(0, GameState.activePieces[ai_player].Count); // select random index of a piece. list.Count gives you a number of elements in a list (which may change over time)
            Transform candidatePiece = GameState.activePieces[ai_player][pieceIndex];  // this is our candidate piece to move
            Debug.Log("SKIPPY: How about moving " +candidatePiece.name);
            if (selectSquare(candidatePiece)) // let's try to find a destination square for our candiate piece
            {
                selectedPiece = candidatePiece; // hurray! we found a piece and a destination for it!
                Debug.Log("SKIPPY: Finally!!!! Gocha! HAHAHAHA!");
                return true; // since we are done, let's exit and announce the success by returning true
            }
            Debug.Log("SKIPPY: Bad luck this time, good luck next time... At least I hope. Let's try again.");
        }
        // still here? if we did not exit above, it means we were really unucky and despite the fact that we tried 20 times, we did not find a piece to move
        // of course, that means we were accidently selecting the same piece multiple times, which is not optimal, but easier to implement
        // so let's try a systematic approach: go through all pieces, one by one, and try selecting a destination for it
        
        Debug.Log("SKIPPY: Hmm, for some reason my random approach was not successful. So unlikely! How did I end up in such an unlucky branch of the multiverse? Well, let's try to be systematic then...");
        foreach (Transform candidatePiece in GameState.activePieces[ai_player]) // for all pieces in our list of pieces
        {   
            Debug.Log("SKIPPY: Ok, so how about " + candidatePiece.name);
            
            if (selectSquare(candidatePiece))  // try to select a square destination for the piece
            {
                Debug.Log("SKIPPY: Finally!!!! Gocha! HAHAHAHA!");
                selectedPiece = candidatePiece; // hurray! we finally found the piece (and, inside the selectSquare, its destination)
                return true; // since we are done, let's exit and announe that we succeeded!
            }
            Debug.Log("SKIPPY: Really?! Fine! Let's try again!");
        }
        
        return false;// despite of all the attempts, we did not find a piece that has any move. Since we did the systematic approach, 
        // that means there is no valid move and the game is over

    }

    void makeRandomMove()
    {
        
        if (selectPiece()) // check if there is piece to move with a valid destination
        {
            GameState.movePiece(selectedPiece, selectedSquare);
            GameState.playersTurn = true;
            GameState.aiTurn = false;
        } else {
            Debug.Log ("GameState: There is no valid move for any piece! You've lost, SKIPPY!");// since there was no way to select a piece, AI lost the game!
            //TODO: where and how should be specify that the game is lost? 
        }
    }

    // Update is called once per frame
    void Update()
    {
        // every frame, we check if it AI turn.
        if (GameState.aiTurn) // if it is AI turn (that is, if it is not player's turn)
        {
            Invoke("makeRandomMove",4); // try to find an AI piece and move it
            GameState.aiTurn = false;
        }
    }
}
