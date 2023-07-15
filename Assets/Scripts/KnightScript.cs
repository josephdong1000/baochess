using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = System.Object;

public class KnightScript : PieceScript {
    public KnightScript() : base() {
        MoveFunctions = new List<Func<(int, int), bool>> {
            KnightMove
        };
        CaptureFunctions = new List<Func<(int, int), bool>> {
            KnightMove
        };
        SelfCaptureFunctions = new() {
        };
        ProtectFunctions = new() {
            // SerfsUpProtect, already implemented in Pawn PawnPassive
        };
        RangedFunctions = new List<Func<(int, int), (bool, List<(int, int)>)>> {
            SerfsUpMove
        };
        AttackReplaceFunctions = new();
        PassiveReplaceFunctions = new();
    }

    public override PieceType Type => PieceType.Knight;

    public override Dictionary<string, List<(int, int)>> MoveDict { get; } = new() {
        {
            "move", new List<(int, int)> { // Stock knight move
                (1, 2), (2, 1), (2, -1), (1, -2),
                (-1, 2), (-2, 1), (-2, -1), (-1, -2)
            }
        },
        { "serfmove", new List<(int, int)> { } },
        { "serfprot", new List<(int, int)> { } }
    };

    // Start is called before the first frame update
    new void Start() {
        base.Start();

        // SetSpriteSide();
        GetFriendlyPieces();
        CheckInvertDirection();
    }

    [Move]
    public bool KnightMove((int, int) move) {
        return MoveDict["move"].Contains(move);
    }

    /// <summary>
    /// Given any move, check if it is a Serfs Up move. Must capture a piece, but not on the same spot.
    /// </summary>
    /// <param name="move"></param>
    /// <returns>If that move is a legal Serfs Up move, and if true, which positions are under attack</returns>
    [Move]
    [SpecialMove]
    public (bool, List<(int, int)>) SerfsUpMove((int, int) move) {
        // Move must end at an empty square, at a knight position
        if (!BoardScript.IsEmptySquare(Position, move) ||
            !MoveDict["move"].Contains(move)) {
            return (false, new List<(int, int)>());
        }

        // Check that the knight is next to a friendly pawn, and does not remain adjacent to the same pawn after move
        List<GameObject> adjacentGameObjects =
            BoardScript.GetAdjacentGameObjects(Position, distance: 1, taxicab: false, perimeter: true);
        adjacentGameObjects.RemoveAll(s => BoardScript.IsEmptySquare(s));

        List<GameObject> newAdjacentGameObjects =
            BoardScript.GetAdjacentGameObjects(AddMove(move), distance: 1, taxicab: false, perimeter: true);
        newAdjacentGameObjects.RemoveAll(s => BoardScript.IsEmptySquare(s));

        bool nextToPawn = false;
        foreach (GameObject i in adjacentGameObjects) {
            if (i.GetComponent<PieceScript>().Type == PieceType.Pawn &&
                i.GetComponent<PieceScript>().PieceSide == PieceSide) { // Knight next to friendly pawn
                nextToPawn = true;
                if (newAdjacentGameObjects.Contains(i)) { // If that pawn overlaps in new position
                    return (false, new List<(int, int)>());
                }
            }
        }

        if (!nextToPawn) {
            return (false, new List<(int, int)>());
        }


        // Give a 1-2 long list of enemy pieces to capture
        (int, int) moveDirection = (Math.Sign(move.Item1), Math.Sign(move.Item2)); // A diagonal direction
        List<(int, int)> finalPositions = new List<(int, int)>();

        // First check route 1
        List<(int, int)> capturableMoves = new List<(int, int)> {
            (moveDirection.Item1, 0),
            (move.Item1, move.Item2 - moveDirection.Item2)
        };
        foreach ((int, int) capturableMove in capturableMoves) {
            if (!BoardScript.IsEmptySquare(AddMove(capturableMove)) &&
                BoardScript.IsEnemy(AddMove(capturableMove), PieceSide)) {
                finalPositions.Add(AddMove(capturableMove));
                break;
            }
        }

        // Then check route 2
        capturableMoves = new List<(int, int)> {
            (0, moveDirection.Item2),
            (move.Item1 - moveDirection.Item1, move.Item2)
        };
        foreach ((int, int) capturableMove in capturableMoves) {
            if (!BoardScript.IsEmptySquare(AddMove(capturableMove)) &&
                BoardScript.IsEnemy(AddMove(capturableMove), PieceSide)) {
                finalPositions.Add(AddMove(capturableMove));
                break;
            }
        }

        if (finalPositions.Count != 0) {
            return (true, finalPositions);
        }

        return (false, new List<(int, int)>());
    }
    
    // Already implemented in PawnScript.cs
    // public string SerfsUpProtect((int, int) move) {
    //     // Can only protect itself against Maginot Line
    //     if (move != (0,0)) {
    //         return "";
    //     }
    //     
    //     // Check adjacency to friendly pawn
    //     List< (int, int)> adjacentGameObjects =
    //         BoardScript.GetAdjacentPositions(Position, distance: 1, taxicab: true, perimeter: true);
    //     foreach ((int, int) i in adjacentGameObjects) {
    //         if (!BoardScript.IsEnemy(i, PieceSide) && // Friendly
    //             BoardScript.GetPieceType(i) == PieceType.Pawn) { // Pawn
    //             return "MaginotLine";
    //         }
    //     }
    //
    //     return "";
    // }
}