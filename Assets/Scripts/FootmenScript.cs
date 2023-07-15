using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FootmenScript : PieceScript {
    public FootmenScript() : base() {
        MoveFunctions = new List<Func<(int, int), bool>> {
            FootmenForward,
            TacticalRetreatForward
        };
        CaptureFunctions = new List<Func<(int, int), bool>> {
            FootmenCapture,
            TacticalRetreatCapture
        };
        SelfCaptureFunctions = new() {
        };
        ProtectFunctions = new() {
        };
        RangedFunctions = new List<Func<(int, int), (bool, List<(int, int)>)>> {
        };
        AttackReplaceFunctions = new() {
        };
        PassiveReplaceFunctions = new();
    }

    public override PieceType Type => PieceType.Footmen;

    public override Dictionary<string, List<(int, int)>> MoveDict { get; } = new() {
        { "forward", new List<(int, int)> { (1, 0) } }, // Stock footmen move
        { "capture", new List<(int, int)> { (1, 1), (1, -1) } }, // Stock footmen capture
        { "tactmove", new List<(int, int)> { (-1, 0) } }, // Backwards tactical retreat move
        { "tactcap", new List<(int, int)> { (-1, 1), (-1, -1) } }, // Backwards tactical retreat capture
    };

    // Start is called before the first frame update
    new void Start() {
        base.Start();

        // SetSpriteSide();
        GetFriendlyPieces();
        CheckInvertDirection();
    }


    /// <summary>
    /// Given any move square, check if FootmenForward allows that move. Cannot capture a piece
    /// </summary>
    /// <param name="move">The coordinates of the move, relative to the piece</param>
    /// <returns>If the move is a FootmenForward move</returns>
    [Move]
    public bool FootmenForward((int, int) move) {
        // Check that the move is forward
        if (MoveDict["forward"][0] != move) {
            return false;
        }

        // Check that no piece is captured
        return BoardScript.IsEmptySquare(Position, move);
    }

    /// <summary>
    /// Given any move square, check if FootmenCapture allows that move. Must capture a piece
    /// </summary>
    /// <param name="move">The coordinates of the move, relative to the piece</param>
    /// <returns>If the move is a FootmenCapture move</returns>
    [Move]
    public bool FootmenCapture((int, int) move) {
        // Check that the move is a Capture move 
        if (!MoveDict["capture"].Contains(move)) {
            return false;
        }

        // Check that a piece is captured
        return !BoardScript.IsEmptySquare(Position, move);
    }

    [Move]
    public bool TacticalRetreatForward((int, int) move) {
        // Check that the move is backwards from edge of board
        if (MoveDict["tactmove"][0] != move ||
            !BoardScript.IsOnFarEdge(Position, PieceSide)) {
            return false;
        }

        // Check that no piece is captured
        return BoardScript.IsEmptySquare(Position, move);
    }

    [Move]
    public bool TacticalRetreatCapture((int, int) move) {
        // Check that the move is a Capture move from edge of board
        if (!MoveDict["tactcap"].Contains(move) ||
            !BoardScript.IsOnFarEdge(Position, PieceSide)) {
            return false;
        }

        // Check that a piece is captured
        if (BoardScript.IsEmptySquare(Position, move)) {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Self-delete Footmen after existing for 3 turns, except next to king
    /// </summary>
    public override void AutomaticMove() {
        if (TurnsOnBoard > 1 &&
            BoardScript.GetAdjacentGameObjects(Position, 1, false, true)
                .FindAll(g => g.GetComponent<PieceScript>().Type == PieceType.King)
                .FindAll(g => g.GetComponent<PieceScript>().PieceSide == PieceSide)
                .Count == 0) {
            // Add self on delete list
            BoardScript.AddDeleteList(gameObject);
            // _board[newPosition.Item1, newPosition.Item2] =
            //     InstantiatePiece(selectedPieceType, newPosition, PlayingSide);
            // GetPosition(newPosition).GetComponent<PieceScript>().SetSpriteSide();
        }
    }
}