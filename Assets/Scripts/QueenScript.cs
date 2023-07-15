using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QueenScript : PieceScript {
    public QueenScript() : base() {
        MoveFunctions = new List<Func<(int, int), bool>> {
            QueenMove
        };
        CaptureFunctions = new List<Func<(int, int), bool>> {
            QueenMove
        };
        SelfCaptureFunctions = new() {
        };
        ProtectFunctions = new() {
            CoregencyProtect
        };
        RangedFunctions = new List<Func<(int, int), (bool, List<(int, int)>)>> {
        };
        AttackReplaceFunctions = new() {
            QueenMother
        };
        PassiveReplaceFunctions = new() {
            CoregencyReplace
        };
    }

    public override PieceType Type => PieceType.Queen;

    public override Dictionary<string, List<(int, int)>> MoveDict { get; } = new() {
        {
            "move", GenerateLongMoves(MoveDirection.DownLeft, -1) // Stock queen move
                .Concat(GenerateLongMoves(MoveDirection.DownRight, -1))
                .Concat(GenerateLongMoves(MoveDirection.UpLeft, -1))
                .Concat(GenerateLongMoves(MoveDirection.UpRight, -1))
                .Concat(GenerateLongMoves(MoveDirection.Left, -1))
                .Concat(GenerateLongMoves(MoveDirection.Right, -1))
                .Concat(GenerateLongMoves(MoveDirection.Up, -1))
                .Concat(GenerateLongMoves(MoveDirection.Down, -1)).ToList()
        },
        { "mother", BoardScript.GetAdjacentMoves(1, taxicab: false, perimeter: true) }, // Queen mother
        { "coreprot", new List<(int, int)> { (0, 0) } },
        { "corerepl", new List<(int, int)> { (0, 0) } }
    };

    // Start is called before the first frame update
    new void Start() {
        base.Start();
        
        // SetSpriteSide();
        GetFriendlyPieces();
        CheckInvertDirection();
    }

    [Move]
    public bool QueenMove((int, int) move) {
        if (!MoveDict["move"].Contains(move)) {
            return false;
        }

        if (BoardScript.IsClearColumn(Position, AddMove(move)) ||
            BoardScript.IsClearRow(Position, AddMove(move)) ||
            BoardScript.IsClearDiagonal(Position, AddMove(move))) {
            return true;
        }

        return false;
    }

    [Move]
    [SpecialMove]
    public (bool, Dictionary<(int, int), List<PieceType>>) QueenMother((int, int) move) {
        var falseReturn = (false, new Dictionary<(int, int), List<PieceType>>());
        // Move is adjacent
        if (!MoveDict["mother"].Contains(move)) {
            return falseReturn;
        }

        // Move cannot capture a piece
        if (!BoardScript.IsEmptySquare(AddMove(move))) {
            return falseReturn;
        }

        (int, int) queenPostPosition = AddMove(move); // Center position of queenbomb
        List<PieceType> footmenList = new List<PieceType> { PieceType.Footmen }; // Returned Footmen list
        Dictionary<(int, int), List<PieceType>> endingPiecePositions = new();
        // Check if all 8 positions around the mother can spawn a footman
        foreach ((int, int) explodeMove in MoveDict["mother"]) {
            // Check each position
            (int, int) explodeMoveTargetPosition = BoardScript.AddMovePositions(queenPostPosition, explodeMove);

            // Cannot go spawn outside the board
            if (BoardScript.IsOutsideBoard(explodeMoveTargetPosition)) {
                continue;
            }

            if (BoardScript.IsEmptySquare(explodeMoveTargetPosition)) {
                // Spawning onto empty square is legal
                endingPiecePositions.Add(explodeMoveTargetPosition, footmenList);
            } else if (BoardScript.GetPieceType(explodeMoveTargetPosition) != PieceType.Pawn &&
                       BoardScript.GetPieceType(explodeMoveTargetPosition) != PieceType.King) {
                // Spawning onto square without pawn or king also legal
                endingPiecePositions.Add(explodeMoveTargetPosition, footmenList);
            }
        }

        return (endingPiecePositions.Count > 0, endingPiecePositions);
    }

    /// <summary>
    /// Check if a replace move is a valid Coregency move. Must occur at the beginning of the game and does not count as a move.
    /// </summary>
    /// <param name="move"></param>
    /// <returns></returns>
    [Move]
    public (bool, Dictionary<(int, int), List<PieceType>>) CoregencyReplace((int, int) move) {
        var falseReturn = (false, new Dictionary<(int, int), List<PieceType>>());

        // Must occur at 0th (first) turn of game
        if (BoardScript.GameTurnCounter > 0) {
            return falseReturn;
        }

        // Replaces itself
        if (move != MoveDict["corerepl"][0]) {
            return falseReturn;
        }

        // Replace self with king
        return (true, new Dictionary<(int, int), List<PieceType>> {
            { Position, new List<PieceType> { PieceType.King } }
        });
    }
    
    [Move]
    public (GameObject, string) CoregencyProtect((int, int) move) {
        // Queen protects self against battlefield command
        if (move != MoveDict["coreprot"][0]) {
            return (null, "");
        }

        return (null, "BattlefieldCommand");
    }
    
}