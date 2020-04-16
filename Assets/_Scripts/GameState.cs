using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
/*
 *  This class handles game state: it remembers current game state and allows others to check whether a move is valid
 */
public class GameState : MonoBehaviour
{
    string playerColor = "White";
    public MenuInteraction menuInteraction;
    public static bool playersTurn = true; // player, not AI, gets the first move, regardless of color
    public static bool aiTurn = false; // 
    public static Transform[,] chessboard = new Transform[8,8]; // game state two-dimensiona array
    private static List<Transform> activeWhitePieces = new List<Transform>();
    private static List<Transform> activeBlackPieces = new List<Transform>();
    private static List<Transform> inactiveWhitePieces = new List<Transform>();
    private static List<Transform> inactiveBlackPieces = new List<Transform>();

    
    public static Dictionary<string, List<Transform>> activePieces = new Dictionary<string,List<Transform>>();
    public static Dictionary<string, List<Transform>> inactivePieces = new Dictionary<string, List<Transform>>();
    public GameObject originalSquare; // reference to square object we use to build the chessboard
    public Transform chessboardParent; // reference to container of our chessboard
    public Transform chessPieces; // reference to parent of all pieces
    const int SQUARE_SIZE = 2; // this constant allow us to scale our chessboard

    static bool isPathEmpty(Transform piece, Transform square, float maxDistance)
    {
        RaycastHit hit;
        // draw a line in scene view, so we see it for raycasting. First I get a vector of direction, then normalize it (make it lenght to be 1 unit long)
        // and then multiply it by our max Distance.
        Debug.DrawRay(piece.position + Vector3.up,  (square.position-piece.position).normalized * maxDistance, Color.red);

        // create a list of layers that will be used for raycasting. Every game object is by default on "Default" layer, but like with the tags
        // you can create custom layers. I created "White" layer and "Black" layer. Each piece is on white or black layer
        // by specifying the layers for raycasting, I make sure that hitting other objects is ignored: raycasting only returns true if the
        // ray hits an object on one of the specified layers.
        List<string> layers = new List<string>(); // create a list of layers we want to hit
        layers.Add("White"); // add whites layer 
        layers.Add("Black"); // add blacks layer 

        if (Physics.Raycast(piece.position + Vector3.up, (square.position-piece.position), out hit, maxDistance, LayerMask.GetMask(layers.ToArray()))) {
            Debug.Log("GameState: Hey, dude! The path toward the destination is occupied! You can't move over other pieces unless you are a knight!");
            return false;        
        }
        return true;
    }

    public static void movePiece(Transform piece, Transform square)
    {
        piece.GetComponent<Collider>().enabled = false;
        piece.parent.GetComponent<Square>().piece = null;
        piece.GetComponent<NavMeshAgent>().SetDestination(square.position); // move the piece. Later, we will animate this step, but for now the move is immediate
        piece.GetComponent<PieceBehaviour>().enemy = square.GetComponent<Square>().piece; // set enemy in the moving piece to enemy piece, if there is any
        square.GetComponent<Square>().piece = piece; // let square remember what piece is sitting on it
        piece.parent = square; // let piece remember was piece it is sitting on, by setting it as parent
        piece.GetComponent<Collider>().enabled = true;
    }

    public static void eliminatePiece(Transform piece)
    {
        piece.GetComponent<Collider>().enabled = false; // disable piece collider
        piece.parent.GetComponent<Square>().piece = null; // remove current square's reference to the piece
        piece.parent = null; // remove pieces reference to its square
        piece.gameObject.AddComponent<Rigidbody>(); // make it respond to physics
        piece.GetComponent<Rigidbody>().useGravity = true; // make it respond to gravity

        Vector3 direction = new Vector3(0, 10, Random.Range(40, 60)); // prepare push direction
        if (piece.name.Contains("White"))
        {
            direction = new Vector3(0, 10, Random.Range(-60, -40)); // if it is white, the direction is negative on z axxis
            activeWhitePieces.Remove(piece); // remove piece from the list of live white pieces
        }
        else
        {
            activeBlackPieces.Remove(piece); // remove piece from the list of live black pieces
        }
        
        piece.GetComponent<Rigidbody>().AddForce(direction, ForceMode.Impulse); // push the piece out of the chessboard in a direction dependent on the piece color
        //piece.GetComponent<Collider>().enabled = true; // re-enable pieces collider, so when it hits the wall it will not go through it
        
    }
    // the function below is "static" which means it can be called through class name, like that: GameState.isValidMove
    // without a need to have a reference to this class. Using "static" is OK only for classes that for sure have just one object
    // Since there is only one game state, it is ok to have "static" functions. Otherwise, it would be a bad idea.
    public static bool isValidMove(Transform piece, Transform square) // given piece and square, the function checks if piece can move to the square
    {
        if (!piece || !square) return false; // if there is no piece or no square, move is impossible. This should never happen, but better save then sorry!

        Square origin = piece.parent.GetComponent<Square>(); // get the square component from the square on which our selected piece sits on
        Square destination = square.GetComponent<Square>(); // get the square component from the square of the potential destination

        // max distance for raycasting is a square right in front of the destination square
        // (remember the bug from class? We used "1" instead of SQUARE_SIZE, so the ray was too short! (it would work if squares were of size 1)
        // the caclualtion may look complex but this is a simple Pitagoras theorem a^2+b^2=c^2
        float maxDistance = Vector3.Distance(square.position, piece.position) - SQUARE_SIZE;

        if (piece.name.Contains("Pawn"))  // validate moves for whites
        {
            if (piece.name.Contains("White"))
            {
                if (((origin.j + 1 == destination.j && origin.i == destination.i) && !destination.piece) ||
                    ((origin.j + 2 == destination.j && origin.i == destination.i) && !destination.piece && origin.j == 1 && isPathEmpty(piece, square, maxDistance)) ||
                    ((origin.j + 1 == destination.j && origin.i + 1 == destination.i) && destination.piece && destination.piece.name.Contains("Black")) ||
                    ((origin.j + 1 == destination.j && origin.i - 1 == destination.i) && destination.piece && (destination.piece.name.Contains("Black"))))
                {
                    return true;
                }
                else return false;
            }
            if (piece.name.Contains("Black"))
            {

                if (((origin.j - 1 == destination.j && origin.i == destination.i) && !destination.piece) ||
                    ((origin.j - 2 == destination.j && origin.i == destination.i) && !destination.piece && origin.j == 6 && isPathEmpty(piece, square, maxDistance)) ||
                    ((origin.j - 1 == destination.j && origin.i + 1 == destination.i) && destination.piece && destination.piece.name.Contains("White")) ||
                    ((origin.j - 1 == destination.j && origin.i - 1 == destination.i) && destination.piece && (destination.piece.name.Contains("White"))))
                {
                    return true;
                }
                else return false;
            }
        }

        if (piece.name.Contains("King"))
        {
            if (((origin.i + 1 == destination.i && origin.j == destination.j) ||
                 (origin.i - 1 == destination.i && origin.j == destination.j) ||
                 (origin.i + 1 == destination.i && origin.j + 1 == destination.j) ||
                 (origin.i - 1 == destination.i && origin.j - 1 == destination.j) ||
                 (origin.i == destination.i && origin.j + 1 == destination.j) ||
                 (origin.i == destination.i && origin.j - 1 == destination.j) ||
                 (origin.i - 1 == destination.i && origin.j + 1 == destination.j) ||
                 (origin.i + 1 == destination.i && origin.j - 1 == destination.j))
                &&
                 (!destination.piece || destination.piece.gameObject.layer != piece.gameObject.layer))
            // replace that with conditions 
            {
                return true;
            }
            else return false;
        }

        if (piece.name.Contains("Queen"))
        {

            if (((origin.i == destination.i && origin.j != destination.j) ||
                 (origin.j == destination.j && origin.i != destination.i) ||
                 (destination.j - origin.j == destination.i - origin.i) ||
                 (origin.j - destination.j == destination.i - origin.i))
               && isPathEmpty(piece, square, maxDistance)
               &&
                 (!destination.piece || destination.piece.gameObject.layer != piece.gameObject.layer))

            //replace that with conditions 
            {
                return true;
            }
            else return false;
        }

        if (piece.name.Contains("Bishop"))
        {

            if (((destination.j - origin.j == destination.i - origin.i) ||
                (origin.j - destination.j == destination.i - origin.i))
               && isPathEmpty(piece, square, maxDistance)
               &&
               (!destination.piece || destination.piece.gameObject.layer != piece.gameObject.layer))  // replace that with conditions 
            {
                return true;
            }
            else return false;
        }

        if (piece.name.Contains("Rook"))
        {
            if (((origin.i == destination.i && origin.j != destination.j) ||
                 (origin.j == destination.j && origin.i != destination.i))
               && isPathEmpty(piece, square, maxDistance)
               &&
               (!destination.piece || destination.piece.gameObject.layer != piece.gameObject.layer))// replace that with conditions 
            {
                return true;
            }
            else return false;
        }

        if (piece.name.Contains("Horse"))
        {
            if (((origin.i - 1 == destination.i && origin.j + 2 == destination.j) ||
                (origin.i + 1 == destination.i && origin.j + 2 == destination.j) ||
                (origin.i - 1 == destination.i && origin.j - 2 == destination.j) ||
                (origin.i + 1 == destination.i && origin.j - 2 == destination.j) ||
                (origin.i - 2 == destination.i && origin.j - 1 == destination.j) ||
                (origin.i - 2 == destination.i && origin.j + 1 == destination.j) ||
                (origin.i + 2 == destination.i && origin.j - 1 == destination.j) ||
                (origin.i + 2 == destination.i && origin.j + 1 == destination.j))
                && (!destination.piece || destination.piece.gameObject.layer != piece.gameObject.layer))
            {
                return true;
            }
            else return false;
        }

        Debug.Log("GameState: Nope! That's not a valid move!");
        return false;
    }

    // at the beginning of the game, we procedurally construct the chessboard
    void Start()
    {
        /*menuInteraction = GameObject.Find("StartManager").GetComponent<MenuInteraction>();
        if (menuInteraction.colorChoose == 1)
        {
            playerColor = "White";

        }

        if (menuInteraction.colorChoose == 2)
        {
            playerColor = "Black";
        }
        */
        for (int i = 0; i < 8; ++i) // in every column
        {
            for (int j = 0; j < 8; ++j) // and every row of the chessboard
            {
                GameObject square = Instantiate(originalSquare, chessboardParent); // create square
                square.GetComponent<Square>().i = i; // let square remember it's row
                square.GetComponent<Square>().j = j; // let square remember it's column
                square.transform.position = new Vector3(i * SQUARE_SIZE - SQUARE_SIZE * 3.5f, 1.8f, j * SQUARE_SIZE - SQUARE_SIZE * 3.5f); // position the square

                for (int k = 0; k < chessPieces.childCount; ++k) // look through all pieces
                {
                    if (Vector3.Distance(chessPieces.GetChild(k).position, square.transform.position) < SQUARE_SIZE/2) // and if piece is closer than half the size of square
                    {
                        square.GetComponent<Square>().piece = chessPieces.GetChild(k); // let square remember what piece is sitting on it
                        chessPieces.GetChild(k).parent = square.transform; // let piece remember what square it is sitting on (by using parent)
                    }
                }
                square.GetComponent<MeshRenderer>().material.color = new Color(0, 0, 0, 0.0f); // reset color to transparent

                chessboard[i, j] = square.transform; // put the square reference into our game state memory at a proper position
            }
        }

        Destroy(originalSquare); // once we created all squares, we don't need our initial red square


        List<string> whitePiecesNames = new List<string>();
        List<string> blackPiecesNames = new List<string>();
        foreach (GameObject t in GameObject.FindGameObjectsWithTag("piece"))
        { // find all objects tagged as "piece" and iterate over them
            if (t.layer == LayerMask.NameToLayer("White"))
            {
                activeWhitePieces.Add(t.transform);
                whitePiecesNames.Add(t.name);
            }
            else
            {
                activeBlackPieces.Add(t.transform);
                blackPiecesNames.Add(t.name);
            }
        }

        activePieces["White"] = activeWhitePieces;
        activePieces["Black"] = activeBlackPieces;

        /*foreach (Transform t in activeWhitePieces)
        {
            int i = Random.Range(0, whitePiecesNames.Count);
            t.name = whitePiecesNames[i];
            whitePiecesNames.RemoveAt(i);

        }*/

         foreach (Transform t in activeBlackPieces)
        {
            int i = Random.Range(0, blackPiecesNames.Count);
            t.name = blackPiecesNames[i];
            blackPiecesNames.RemoveAt(i);
        }


    }
}
