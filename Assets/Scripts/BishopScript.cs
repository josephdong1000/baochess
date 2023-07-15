using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class BishopScript : PieceScript {
    public BishopScript() : base() {
        MoveFunctions = new List<Func<(int, int), bool>> {
            BishopMove,
            CloisteredMove
        };
        CaptureFunctions = new List<Func<(int, int), bool>> {
            BishopMove,
            Cannonization,
            CloisteredMove
        };
        SelfCaptureFunctions = new() {
        };
        ProtectFunctions = new() {
            CloisteredProtect
        };
        RangedFunctions = new List<Func<(int, int), (bool, List<(int, int)>)>> {
        };
        AttackReplaceFunctions = new();
        PassiveReplaceFunctions = new();
    }

    public override PieceType Type => PieceType.Bishop;

    public override Dictionary<string, List<(int, int)>> MoveDict { get; } = new() {
        {
            "move", GenerateLongMoves(MoveDirection.DownLeft, -1) // Stock bishop move
                .Concat(GenerateLongMoves(MoveDirection.DownRight, -1))
                .Concat(GenerateLongMoves(MoveDirection.UpLeft, -1))
                .Concat(GenerateLongMoves(MoveDirection.UpRight, -1)).ToList()
        }, {
            "cannon", GenerateLongMoves(MoveDirection.DownLeft, 6) // Stock bishop move
                .Concat(GenerateLongMoves(MoveDirection.DownRight, 6))
                .Concat(GenerateLongMoves(MoveDirection.UpLeft, 6))
                .Concat(GenerateLongMoves(MoveDirection.UpRight, 6))
                .Concat(GenerateLongMoves(MoveDirection.Left, 6, spacing: 2, start: 2))
                .Concat(GenerateLongMoves(MoveDirection.Right, 6, spacing: 2, start: 2))
                .Concat(GenerateLongMoves(MoveDirection.Up, 6, spacing: 2, start: 2))
                .Concat(GenerateLongMoves(MoveDirection.Down, 6, spacing: 2, start: 2)).ToList()
        },
        { "cloimove", new List<(int, int)>() },
        { "cloiprot", new List<(int, int)> { (0, 0) } }
    };

    // Start is called before the first frame update
    new void Start() {
        base.Start();

        // SetSpriteSide();
        GetFriendlyPieces();
        CheckInvertDirection();
    }

    [Move]
    public bool BishopMove((int, int) move) {
        // Check that the move is diagonal
        if (!MoveDict["move"].Contains(move)) {
            return false;
        }

        // Check that the move has a clear diagonal
        return BoardScript.IsClearDiagonal(Position, AddMove(move));
    }

    [Move]
    [SpecialMove]
    public bool Cannonization((int, int) move) {
        // Check in a Cannon direction (any cardinal direction, same square color parity, within 6 squares (non-taxicab))
        if (!MoveDict["cannon"].Contains(move)) {
            return false;
        }

        // Check directional adjacent square is occupied and friendly
        (int, int) direction = (Math.Sign(move.Item1), Math.Sign(move.Item2));
        (int, int) adjacentPosition = AddMove(direction);
        if (BoardScript.IsEmptySquare(adjacentPosition) ||
            BoardScript.GetPieceSide(adjacentPosition) != PieceSide) {
            return false;
        }

        // Check that the square after that is empty 
        (int, int) lonePosition = AddMove((direction.Item1 * 2, direction.Item2 * 2));
        if (BoardScript.IsOutsideBoard(lonePosition) ||
            !BoardScript.IsEmptySquare(lonePosition)) {
            return false;
        }


        // Check that target is occupied, not the adjacent square    // and first piece hit
        if (!BoardScript.IsEmptySquare(Position, move) &&
            adjacentPosition != AddMove(move)) {
            // (BoardScript.IsClearDiagonal(adjacentPosition, AddMove(move)) ||
            //  BoardScript.IsClearColumn(adjacentPosition, AddMove(move)) ||
            //  BoardScript.IsClearRow(adjacentPosition, AddMove(move)))) {
            return true;
        }

        return false;
    }

    [Move]
    public bool CloisteredMove((int, int) move) {
        // Check same color square
        if (!BoardScript.IsSameParity(Position, AddMove(move))) {
            return false;
        }

        // Check not a regular bishop move
        if (MoveDict["move"].Contains(move)) {
            return false;
        }

        // Generate possible edge bounce points
        List<(int, int)> bouncePositions = new List<(int, int)>();
        foreach ((int, int) i in MoveDict["move"]) {
            if (BoardScript.IsOnEdge(AddMove(i))) {
                bouncePositions.Add(AddMove(i));
            }
        }
        
        // Check if each possible bounce point leads to two clear diagonals
        foreach ((int, int) bouncePosition in bouncePositions) {
            if (BoardScript.IsClearDiagonal(Position, bouncePosition) &&
                BoardScript.IsEmptySquare(bouncePosition) &&
                BoardScript.IsClearDiagonal(bouncePosition, AddMove(move))) {
                return true;
            }
        }

        return false;
    }

    [Move]
    public (GameObject, string) CloisteredProtect((int, int) move) {
        // If on edge, cannot be SerfsUp'd
        if (MoveDict["cloiprot"][0] == move &&
            BoardScript.IsOnEdge(Position)) {
            return (null, "SerfsUpMove");
        }

        return (null, "");
    }
    
}