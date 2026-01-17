using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KingScript : PieceScript {
    public KingScript() : base() {
        MoveFunctions = new List<Func<(int, int), bool>> {
            KingMove
        };
        CaptureFunctions = new List<Func<(int, int), bool>> {
            KingMove
        };
        SelfCaptureFunctions = new() {
            PyrrhicManeuver,
            PyrrhicManeuverNull
        };
        ProtectFunctions = new() {
        };
        RangedFunctions = new List<Func<(int, int), (bool, List<(int, int)>)>> {
        };
        AttackReplaceFunctions = new() {
        };
        PassiveReplaceFunctions = new() {
            CoregencyRevert
        };
    }

    public override PieceType Type => PieceType.King;

    public override Dictionary<string, List<(int, int)>> MoveDict { get; } = new() {
        { "move", BoardScript.GetAdjacentMoves(1, false, true) },
        { "corerev", new List<(int, int)> { (0, 0) } }
    };


    // Start is called before the first frame update
    new void Start() {
        base.Start();

        // SetSpriteSide();
        GetFriendlyPieces();
        CheckInvertDirection();
    }

    [Move]
    public bool KingMove((int, int) move) {
        
        if (!MoveDict["move"].Contains(move)) {
            return false;
        }

        // Can capture any enemy piece or move to empty square
        return BoardScript.IsEmptySquare(Position, move) ||
               BoardScript.IsEnemy(Position, move, PieceSide);
    }

    /// <summary>
    /// Formerly SwordInTheStone
    /// </summary>
    /// <param name="move"></param>
    /// <returns></returns>
    [Move]
    [SpecialMove]
    public bool PyrrhicManeuver((int, int) move) {
        if (!MoveDict["move"].Contains(move)) {
            return false;
        }

        // Can capture any friendly piece except for friendly king with Pyrrhic Maneuver
        return !BoardScript.IsEnemy(Position, move, PieceSide) &&
               BoardScript.GetPieceType(Position, move) != PieceType.King;
    }

    [Move]
    [SpecialMove]
    public bool PyrrhicManeuverNull((int, int) move) {
        return PyrrhicManeuver(move);
    }

    [Move]
    public (bool, Dictionary<(int, int), List<PieceType>>) CoregencyRevert((int, int) move) {
        var falseReturn = (false, new Dictionary<(int, int), List<PieceType>>());

        // Replaces itself
        if (move != MoveDict["corerev"][0]) {
            return falseReturn;
        }

        // This king must be in check
        if (BoardScript.GetPosition(AddMove(move)).GetComponent<PieceScript>().AttackedBy.Count == 0) {
            return falseReturn;
        }

        // Two kings cannot be in check
        // >0 kings must be in check
        int numberAttackedKings = 0;
        if (BoardScript.PieceReferences[(PieceType.King, PieceSide)].Count > 1) {
            foreach (GameObject friendlyKingGO in BoardScript.PieceReferences[(PieceType.King, PieceSide)]) {
                if (friendlyKingGO.GetComponent<PieceScript>().AttackedBy.Count > 0) {
                    numberAttackedKings += 1;
                }
            }
        }

        if (numberAttackedKings > 1 || numberAttackedKings == 0) {
            return falseReturn;
        }

        // Cannot be done if leaves no kings left
        // Should be handled by Checking engine

        // Replace self with replaced queen
        return (true, new Dictionary<(int, int), List<PieceType>> {
            { Position, new List<PieceType> { PieceType.Queen } }
        });
    }
}