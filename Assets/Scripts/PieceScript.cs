using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class PieceScript : MonoBehaviour {
    public virtual PieceType Type { get; }
    public Side PieceSide { get; set; }
    protected List<(PieceType, Side)> FriendlyPieces;
    public int MoveCounter { get; protected set; }
    public void IncrementMoveCounter() => MoveCounter++; // USE THIS FUNCTION
    public int TurnsOnBoard { get; protected set; }
    public void IncrementTurnsOnBoardCounter() => TurnsOnBoard++;
    public bool Promoted { get; set; }

    public Sprite whiteSprite;
    public Sprite blackSprite;
    public Sprite babaSprite;
    public static Color WhiteColor;
    public static Color BlackColor;

    private bool debugMode = false;

    /// <summary>
    /// Runs at the end of the player's turn. Includes things like piece self-deletion (Footmen)
    /// </summary>
    public virtual void AutomaticMove() {
    }

    /// <summary>
    /// Animation that plays before the piece is deleted from the board
    /// </summary>
    public void DeleteAnimation() {
    }

    public (int, int) Position { get; set; }
    public virtual Dictionary<string, List<(int, int)>> MoveDict { get; }

    /// <summary>
    /// List of functions that take in a move and return if that move is legal. Will only be checked for moving to empty square.
    /// </summary>
    public List<Func<(int, int), bool>> MoveFunctions { get; protected set; }

    /// <summary>
    /// List of functions that take in a move and return if that move can legally capture. Will only be checked for moving onto enemy square.
    /// </summary>
    public List<Func<(int, int), bool>> CaptureFunctions { get; protected set; }
    
    /// <summary>
    /// List of functions that take in a move and return if that move can legally capture. Will only be checked for moving onto friendly square.
    /// </summary>
    public List<Func<(int, int), bool>> SelfCaptureFunctions { get; protected set; }

    /// <summary>
    /// <para>List of functions that take in a move and return if the piece at the move is protected by this piece.</para>
    /// <para>If yes, return what move this piece protects against, and the piece that is getting nullified (if not null)</para>
    /// </summary>
    public List<Func<(int, int), (GameObject, string)>> ProtectFunctions { get; protected set; }

    /// <summary>
    /// <para>List of functions that take in a move and return if this piece can move to that move. If yes, return a dictionary corresponding to new piece spawn positions.</para>
    /// <para>Dictionary has keys of final positions, and values of Lists with all possible PieceTypes that can spawn there.</para>
    /// <para>All positions in Dictionary (its keys) must be performed together. Will be checked against enemy pieces.</para>
    /// </summary>
    public List<Func<(int, int), (bool, Dictionary<(int, int), List<PieceType>>)>> AttackReplaceFunctions {
        get;
        protected set;
    }

    /// <summary>
    /// <para>List of functions that take in a move and return if this piece can move to that move. If yes, return a dictionary corresponding to new piece spawn positions.</para>
    /// <para>Dictionary has keys of final positions, and values of Lists with all possible PieceTypes that can spawn there.</para>
    /// <para>All positions in Dictionary (its keys) must be performed together. Will be checked against empty/friendly pieces.</para>
    /// </summary>
    public List<Func<(int, int), (bool, Dictionary<(int, int), List<PieceType>>)>> PassiveReplaceFunctions {
        get;
        protected set;
    }


    /// <summary>
    /// <para>List of functions that that in a move and return if this piece can capture elsewhere by doing that move. Will only check for moving onto blank square</para>
    /// <para>If yes, return true and a list of possible capturable target positions</para>
    /// </summary>
    public List<Func<(int, int), (bool, List<(int, int)>)>>
        RangedFunctions { get; protected set; } // Moves that move somewhere but attack elsewhere

    public static List<string> SpecialFunctions;
    
    
    protected static GameObject Board;
    protected static BoardScript BoardScript; // some way to make this not set a million times?
    protected static int BoardSize;


    // Instance variables
    /// <summary>
    /// List of GameObjects that attack this piece, using string move
    /// </summary>
    public List<(GameObject, string)> AttackedBy { get; protected set; }
    /// <summary>
    /// List of GameObjects that defend this piece against GameObjects (null if any) using string move
    /// </summary>
    public List<(GameObject, GameObject, string)> ProtectedBy { get; protected set; }

    public enum Side {
        None, // MIGHT BREAK STUFF
        White,
        Black,
    }

    public enum PieceType {
        Pawn,
        Footmen,
        Rook,
        Knight,
        Bishop,
        Queen,
        King,
        Empty
    }

    protected enum MoveDirection {
        Up,
        Down,
        Left,
        Right,
        UpRight,
        DownRight,
        UpLeft,
        DownLeft
    }

    // Define instance variables
    public PieceScript() {
        AttackedBy = new List<(GameObject, string)>();
        ProtectedBy = new List<(GameObject, GameObject, string)>();
    }


    // Start is called before the first frame update
    public void Start() {
        Board = GameObject.FindGameObjectWithTag("Board");
        BoardScript = Board.GetComponent<BoardScript>();
        BoardSize = BoardScript.boardSize;
        MoveCounter = 0;
        TurnsOnBoard = 0;
        Promoted = false; // Override if a promoted pawn
        // WhiteColor = BoardScript.whiteColor;
        // BlackColor = BoardScript.blackColor;
        
        // AllFunctionLists = new List<dynamic> {
        //     MoveFunctions,
        //     CaptureFunctions,
        //     ProtectFunctions,
        //     RangedFunctions,
        //     AttackReplaceFunctions,
        //     PassiveReplaceFunctions
        // };
        // SetSpriteSide();
    }

    public void SetSpriteSide() {
        SpriteRenderer sp = GetComponent<SpriteRenderer>();
        WhiteColor = BoardScript.whiteColor;
        BlackColor = BoardScript.blackColor;

        if (PieceSide != Side.None) {
            sp.sprite = babaSprite;

            if (PieceSide == Side.White) {
                sp.sprite = debugMode ? whiteSprite : babaSprite;
                sp.color = WhiteColor;
            } else {
                sp.sprite = debugMode ? blackSprite : babaSprite;
                sp.color = BlackColor;
            }
        }
    }


    /// <summary>
    /// Generate a list of moves for long moves, those only specified by direction
    /// </summary>
    /// <param name="direction">The direction for the long move</param>
    /// <param name="distance">The distance to generate moves. If -1, defaults to boardSize</param>
    /// <param name="spacing">Spacing between squares. Default 1 (no spaces)</param>
    /// <param name="start">Starting point of moves. Default 1 (1 square away from 0,0)</param>
    /// <returns>n-1 long list of move tuples, n = board edge length</returns>
    /// <exception cref="InvalidOperationException">wtf did you do</exception>
    protected static List<(int, int)> GenerateLongMoves(MoveDirection direction, int distance, int spacing = 1,
        int start = 1) {
        List<(int, int)> output = new List<(int, int)>();
        if (distance == -1) {
            distance = BoardSize - 1;
        }

        for (int i = start; i <= distance; i += spacing) {
            if (direction == MoveDirection.Up) {
                output.Add((i, 0));
            } else if (direction == MoveDirection.Down) {
                output.Add((-i, 0));
            } else if (direction == MoveDirection.Left) {
                output.Add((0, -i));
            } else if (direction == MoveDirection.Right) {
                output.Add((0, i));
            } else if (direction == MoveDirection.UpRight) {
                output.Add((i, i));
            } else if (direction == MoveDirection.DownRight) {
                output.Add((-i, i));
            } else if (direction == MoveDirection.UpLeft) {
                output.Add((i, -i));
            } else if (direction == MoveDirection.DownLeft) {
                output.Add((-i, -i));
            } else {
                throw new InvalidOperationException("wtf did you do");
            }
        }

        return output;
    }

    public void AttackListAdd(GameObject enemyPiece, string moveName) {
        AttackedBy.Add((enemyPiece, moveName));
    }

    // public void AttackListRemove(GameObject enemyPiece, string moveName) {
    //     AttackedBy.Remove((enemyPiece, moveName));
    // }

    // public void AttackListClear() {
    //     AttackedBy.Clear();
    // }

    protected void CheckInvertDirection() {
        if (PieceSide == Side.Black) { // Reverse movement direction for black
            foreach (KeyValuePair<string, List<(int, int)>> keyValuePair in MoveDict) {
                for (int i = 0; i < MoveDict[keyValuePair.Key].Count; i++) {
                    MoveDict[keyValuePair.Key][i] = (
                        MoveDict[keyValuePair.Key][i].Item1 * -1, MoveDict[keyValuePair.Key][i].Item2);
                }
            }
        }
    }

    /// <summary>
    /// Adds move to the current position and returns the final position
    /// </summary>
    /// <param name="move">Move to perform</param>
    /// <returns>Final position</returns>
    public (int, int) AddMove((int, int) move) {
        return (Position.Item1 + move.Item1, Position.Item2 + move.Item2);
    }

    // /// <summary>
    // /// Add position and move and return the sum
    // /// </summary>
    // /// <param name="position"></param>
    // /// <param name="move"></param>
    // /// <returns>Position + move</returns>
    // public (int, int) AddMovePositions((int, int) position, (int, int) move) {
    //     return (position.Item1 + move.Item1, position.Item2 + move.Item2);
    // }

    protected void GetFriendlyPieces() {
        FriendlyPieces ??= new List<(PieceType, Side)>();
        FriendlyPieces.Clear();
        foreach (PieceType i in Enum.GetValues(typeof(PieceType))) {
            if (i != PieceType.Empty) {
                FriendlyPieces.Add((i, PieceSide));
            }
        }
    }
    
}