using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = System.Object;

public class PawnScript : PieceScript {
    public PawnScript() : base() {
        MoveFunctions = new List<Func<(int, int), bool>> {
            PawnForward,
            PawnForwardTwo,
            WallOfInfantry,
            LiveAnotherDay
        };
        CaptureFunctions = new List<Func<(int, int), bool>> {
            PawnCapture
        };
        SelfCaptureFunctions = new() {
        };
        ProtectFunctions = new() {
            PawnPassive
        };
        RangedFunctions = new List<Func<(int, int), (bool, List<(int, int)>)>> {
        };
        AttackReplaceFunctions = new() {
            PromoteAttack
        };
        PassiveReplaceFunctions = new() {
            PromotePassive
        };
    }

    public override PieceType Type => PieceType.Pawn;

    public override Dictionary<string, List<(int, int)>> MoveDict { get; } = new() {
        { "forward", new List<(int, int)> { (1, 0) } }, // Stock pawn move
        { "2forward", new List<(int, int)> { (2, 0) } }, // Stock move forward 2 at beginning of the game
        { "capture", new List<(int, int)> { (1, 1), (1, -1) } }, // Stock pawn capture
        { "wall", new List<(int, int)> { (0, -1), (0, 1) } }, // Wall of infantry
        { "live", new List<(int, int)> { (-1, 0), (-2, 0) } }, // Live another day
        {
            "passive", BoardScript.GetAdjacentMoves(distance: 1, taxicab: false, perimeter: true)
        } // PawnPassive: protect knight against Maginot line
    };

    // Start is called before the first frame
    new void Start() {
        base.Start();

        // SetSpriteSide();
        GetFriendlyPieces();
        CheckInvertDirection();
    }

    /// <summary>
    /// Given any move square, check if Wall Of Infantry allows that move. Cannot capture a piece
    /// </summary>
    /// <param name="move">The coordinates of the move, relative to the piece</param>
    /// <returns>If the move is a Wall of Infantry move</returns>
    [Move]
    [SpecialMove]
    public bool WallOfInfantry((int, int) move) {
        // Check that the move is a Wall Of Infantry move 
        if (!MoveDict["wall"].Contains(move)) {
            return false;
        }

        // Check no adjacent pawns
        List<GameObject> adjacents = BoardScript.GetAdjacentGameObjects(Position, 1, taxicab: false, perimeter: true);
        foreach (GameObject i in adjacents) {
            if (!BoardScript.IsEmptySquare(i) &&
                i.GetComponent<PieceScript>().Type == PieceType.Pawn) {
                return false;
            }
        }

        // Check that no piece is captured
        return BoardScript.IsEmptySquare(Position, move);
    }

    /// <summary>
    /// Given any move square, check if Forward allows that move. Cannot capture a piece
    /// </summary>
    /// <param name="move">The coordinates of the move, relative to the piece</param>
    /// <returns>If the move is a Forward move</returns>
    [Move]
    public bool PawnForward((int, int) move) {
        // Check that the move is forward
        if (MoveDict["forward"][0] != move) {
            return false;
        }

        // Check that no piece is captured
        return BoardScript.IsEmptySquare(Position, move);
    }

    /// <summary>
    /// Given any move square, check if ForwardTwo allows that move. Cannot capture a piece
    /// </summary>
    /// <param name="move"></param>
    /// <returns></returns>
    [Move]
    public bool PawnForwardTwo((int, int) move) {
        // Pawn cannot have moved
        if (MoveCounter > 0) {
            return false;
        }

        // Pawn needs to move forward 2
        if (MoveDict["2forward"][0] != move) {
            return false;
        }

        // Pawn cannot capture a square
        if (!BoardScript.IsEmptySquare(Position, move)) {
            return false;
        }

        // Forward path needs to be clear
        return BoardScript.IsClearColumn(Position, AddMove(move));
    }

    /// <summary>
    /// Given any move square, check if PawnCapture allows that move. Must capture a piece
    /// </summary>
    /// <param name="move">The coordinates of the move, relative to the piece</param>
    /// <returns>If the move is a PawnCapture move</returns>
    [Move]
    public bool PawnCapture((int, int) move) {
        // Check that the move is a Capture move 
        if (!MoveDict["capture"].Contains(move)) {
            return false;
        }

        // Check that a piece is captured
        if (BoardScript.IsEmptySquare(Position, move)) {
            return false;
        }

        return true;

        // Check that the piece is an enemy piece
        // return BoardScript.GetPosition(Position, move).GetComponent<PieceScript>().PieceSide != PieceSide;
    }

    /// <summary>
    /// Given any move square, check if Live Another Day allows that move. Cannot capture a piece
    /// </summary>
    /// <param name="move">The coordinates of the move, relative to the piece</param>
    /// <returns>If the move is a Live Another Day move</returns>
    [Move]
    public bool LiveAnotherDay((int, int) move) {
        // Check that the move is a Live Another Day move
        if (!MoveDict["live"].Contains(move)) {
            return false;
        }

        // // Cannot capture a piece // Already evaluated as a Move function not a Capture function
        // if (!BoardScript.IsEmptySquare(AddMove(move))) {
        //     return false;
        // }

        // Check that the squares between in the column are clear
        if (!BoardScript.IsClearColumn(Position, AddMove(move))) {
            return false;
        }

        // Check that the pawn is under attack
        return AttackedBy.Count != 0;
    }

    /// <summary>
    /// Given any move square, check if that square is protected against MaginotLine
    /// </summary>
    /// <param name="move">The coordinates of the move, relative to the piece</param>
    /// <returns>"MaginotLine" if protected by pawn passive against MaginotLine</returns>
    [Move]
    public (GameObject, string) PawnPassive((int, int) move) {
        // If adjacent to pawn
        if (!MoveDict["passive"].Contains(move)) {
            return (null, "");
        }

        // and is a Knight of the same color
        if (!BoardScript.IsEmptySquare(Position, move) &&
            BoardScript.GetPieceType(Position, move) == PieceType.Knight &&
            !BoardScript.IsEnemy(Position, move, PieceSide)) {
            return (null, "MaginotLine"); // Pawn protects knight against maginot line
        }

        return (null, "");
    }

    [Move]
    public (bool, Dictionary<(int, int), List<PieceType>>) PromotePassive((int, int) move) {
        var falseReturn = (false, new Dictionary<(int, int), List<PieceType>>());
        // Check at second to last row
        if ((PieceSide == Side.White && Position.Item1 != BoardSize - 2) ||
            (PieceSide == Side.Black && Position.Item1 != 1)) {
            return falseReturn;
        }

        List<PieceType> promoteList = new() {
            PieceType.Knight,
            PieceType.Bishop,
            PieceType.Rook,
            PieceType.Queen
        };

        (int, int) finalPosition;
        Dictionary<(int, int), List<PieceType>> outputDict = new();

        if (PawnForward(move) &&
            BoardScript.IsEmptySquare(Position, move)) {
            // move == MoveDict["forward"][0] &&
            // BoardScript.IsEmptySquare(Position, move)) {
            // Piece can move forward
            finalPosition = AddMove(move);
            outputDict.Add(finalPosition, promoteList);
        }

        return (outputDict.Count > 0, outputDict);
    }

    [Move]
    public (bool, Dictionary<(int, int), List<PieceType>>) PromoteAttack((int, int) move) {
        var falseReturn = (false, new Dictionary<(int, int), List<PieceType>>());
        // Check at second to last row
        if ((PieceSide == Side.White && Position.Item1 != BoardSize - 2) ||
            (PieceSide == Side.Black && Position.Item1 != 1)) {
            return falseReturn;
        }

        List<PieceType> promoteList = new() {
            PieceType.Knight,
            PieceType.Bishop,
            PieceType.Rook,
            PieceType.Queen
        };

        (int, int) finalPosition;
        Dictionary<(int, int), List<PieceType>> outputDict = new();

        if (PawnCapture(move) &&
            !BoardScript.IsEmptySquare(Position, move) &&
            BoardScript.IsEnemy(Position, move, PieceSide)) {
            // MoveDict["capture"].Contains(move) &&
            //        !BoardScript.IsEmptySquare(Position, move) &&
            //        BoardScript.IsEnemy(Position, move, PieceSide)) {
            // Piece can capture an enemy at a forward diagonal
            finalPosition = AddMove(move);
            outputDict.Add(finalPosition, promoteList);
        }

        return (outputDict.Count > 0, outputDict);
    }
}