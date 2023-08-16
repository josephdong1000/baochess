using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class RookScript : PieceScript {
    public RookScript() : base() {
        MoveFunctions = new List<Func<(int, int), bool>> {
            RookMove,
            MaginotLine,
            HollowBastionMove
        };
        CaptureFunctions = new List<Func<(int, int), bool>> {
            RookMove,
            MaginotLine,
            HollowBastionMove
        };
        SelfCaptureFunctions = new() {
        };
        ProtectFunctions = new() {
            HollowBastionOverProtect,
            HollowBastionBetweenProtect
        };
        RangedFunctions = new List<Func<(int, int), (bool, List<(int, int)>)>> {
        };
        AttackReplaceFunctions = new();
        PassiveReplaceFunctions = new();
    }

    public override PieceType Type => PieceType.Rook;

    public override Dictionary<string, List<(int, int)>> MoveDict { get; } = new() {
        {
            "move", GenerateLongMoves(MoveDirection.Left, -1) // Stock rook move
                .Concat(GenerateLongMoves(MoveDirection.Right, -1))
                .Concat(GenerateLongMoves(MoveDirection.Up, -1))
                .Concat(GenerateLongMoves(MoveDirection.Down, -1)).ToList()
        }, {
            "maginot", GenerateLongMoves(MoveDirection.Left, -1) // Maginot line distance to other rook
                .Concat(GenerateLongMoves(MoveDirection.Right, -1))
                .Concat(GenerateLongMoves(MoveDirection.Up, 5))
                .Concat(GenerateLongMoves(MoveDirection.Down, 5)).ToList()
        }, {
            "hollowmove", GenerateLongMoves(MoveDirection.Left, -1) // Moving through pawn hollow bastion
                .Concat(GenerateLongMoves(MoveDirection.Right, -1))
                .Concat(GenerateLongMoves(MoveDirection.Up, -1))
                .Concat(GenerateLongMoves(MoveDirection.Down, -1)).ToList()
        },
        { "hollowprot", new List<(int, int)>() } // Protective hollow bastion
    };


    // Start is called before the first frame update
    new void Start() {
        base.Start();

        // SetSpriteSide();
        GetFriendlyPieces();
        CheckInvertDirection();
    }

    /// <summary>
    /// Given any move, check if it is a vanilla RookMove. May capture a piece
    /// </summary>
    /// <param name="move">The coordinates of the move, relative to piece</param>
    /// <returns>If the move is a vanilla RookMove</returns>
    [Move]
    public bool RookMove((int, int) move) {
        // Check that the move is up/down/left/right
        if (!MoveDict["move"].Contains(move)) {
            return false;
        }

        // Check that the two moves have a clear column or row
        if (move.Item1 == 0) {
            // row move
            if (!BoardScript.IsClearRow(Position, AddMove(move))) {
                return false;
            }
        } else {
            // column move
            if (!BoardScript.IsClearColumn(Position, AddMove(move))) {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Given any move, check if it is a Maginot Line move. May capture a piece
    /// </summary>
    /// <param name="move">The coordinates of the move, relative to piece</param>
    /// <returns>If the move is a Maginot Line move</returns>
    [Move]
    [SpecialMove]
    public bool MaginotLine((int, int) move) {
        // Check other rook is on same row or column and same color
        bool aligned = false;
        (int, int) otherRookPosition = (-1, -1);
        // (int, int) middleMove;
        foreach ((int, int) i in MoveDict["maginot"]) {
            // Debug.Log(AddMove(i));
            // Debug.Log(BoardScript.IsOutsideBoard((4,5)));
            // middleMove = AddMove(i);

            otherRookPosition = AddMove(i);
            if (!BoardScript.IsOutsideBoard(otherRookPosition) &&
                !BoardScript.IsEmptySquare(Position, i) &&
                BoardScript.GetPosition(otherRookPosition).GetComponent<PieceScript>().Type == PieceType.Rook &&
                BoardScript.GetPosition(otherRookPosition).GetComponent<PieceScript>().PieceSide == PieceSide) {
                aligned = true;
                // otherRookPosition = ;
                break;
            }
        }

        if (!aligned) {
            return false;
        }

        if (otherRookPosition == (-1, -1)) {
            Debug.Log("wtf did you do");
        }

        // Check for open row or col between rooks
        if (!(BoardScript.IsClearRow(Position, otherRookPosition)
              || BoardScript.IsClearColumn(Position, otherRookPosition))) {
            return false;
        }

        // Check that move first goes in direction of other rook, then goes in clear row
        var finalPosition = AddMove(move);
        if (otherRookPosition.Item1 == Position.Item1) {
            // same row
            return finalPosition.Item2 > Math.Min(otherRookPosition.Item2, Position.Item2) &&
                   finalPosition.Item2 < Math.Max(otherRookPosition.Item2, Position.Item2) && // between rooks
                   finalPosition.Item1 != Position.Item1 && // Different rows
                   BoardScript.IsClearColumn(finalPosition, (Position.Item1, finalPosition.Item2));
        } else {
            // same column
            return finalPosition.Item1 > Math.Min(otherRookPosition.Item1, Position.Item1) &&
                   finalPosition.Item1 < Math.Max(otherRookPosition.Item1, Position.Item1) &&
                   finalPosition.Item2 != Position.Item2 && // Different columns
                   BoardScript.IsClearRow(finalPosition, (finalPosition.Item1, Position.Item2));
        }
    }

    /// <summary>
    /// Given any move, check if it is a Hollow Bastion move. May capture a piece
    /// </summary>
    /// <param name="move">The coordinates of the move, relative to piece</param>
    /// <returns>If the move is a Maginot Line move</returns>
    [Move]
    public bool HollowBastionMove((int, int) move) {
        // Check that the move is up/down/left/right
        if (!MoveDict["hollowmove"].Contains(move)) {
            return false;
        }

        // Check there is only 1 same-color rook on the board
        if (BoardScript.PieceCount[(PieceType.Rook, PieceSide)] != 1) {
            return false;
        }

        // Check that the two moves have a clear column or row, excluding friendly pawns
        List<(PieceType, Side)> whitelist = new List<(PieceType, Side)> {
            (PieceType.Pawn, PieceSide),
            (PieceType.King, PieceSide)
        };
        if (move.Item1 == 0) {
            // row move
            if (!BoardScript.IsClearRow(Position, AddMove(move), whitelist: whitelist)) {
                return false;
            }
            // Not a valid move already
            if (BoardScript.IsClearRow(Position, AddMove(move))) {
                return false;
            }
        } else {
            // column move
            if (!BoardScript.IsClearColumn(Position, AddMove(move), whitelist: whitelist)) {
                return false;
            }
            // Not a valid move already
            if (BoardScript.IsClearColumn(Position, AddMove(move))) {
                return false;
            }
        }

        return true;
    }

    [Move]
    public (GameObject, string) HollowBastionOverProtect((int, int) move) {
        // Protect self against Cannonization
        if (move == (0, 0)) {
            return (null, "Cannonization");
        }

        // Protect all adjacents against Cannonization
        if (BoardScript.Distance((0, 0), move, taxicab: false) == 1) {
            return (null, "Cannonization");
        }

        if (move.Item1 == 0) {
            // Check row for bishop in line
            for (int col = 0; col < BoardSize; col++) { // check row at all cols 
                (int, int) evalPosition = (Position.Item1, col);
                if (!BoardScript.IsEmptySquare(evalPosition) &&
                    BoardScript.GetPieceType(evalPosition) == PieceType.Bishop &&
                    BoardScript.IsEnemy(evalPosition, PieceSide)) {
                    (int, int) evalJumpPosition = (Position.Item1, col + Math.Sign(Position.Item2 - col));
                    (int, int) evalBlankPosition = (Position.Item1, col + Math.Sign(Position.Item2 - col) * 2);

                    if (!BoardScript.IsEmptySquare(evalJumpPosition) &&
                        BoardScript.IsEnemy(evalJumpPosition, PieceSide) &&
                        BoardScript.IsEmptySquare(evalBlankPosition)) {
                        if (move.Item2 * Math.Sign(col - Position.Item2) < 0) {
                            return (BoardScript.GetPosition(evalPosition), "Cannonization");
                        }
                    }
                }
            }
        } else if (move.Item2 == 0) {
            // Check col for bishop in line
            for (int row = 0; row < BoardSize; row++) {
                (int, int) evalPosition = (row, Position.Item2);
                if (!BoardScript.IsEmptySquare(evalPosition) &&
                    BoardScript.GetPieceType(evalPosition) == PieceType.Bishop &&
                    BoardScript.IsEnemy(evalPosition, PieceSide)) {
                    (int, int) evalJumpPosition = (row + Math.Sign(Position.Item1 - row), Position.Item2);
                    (int, int) evalBlankPosition = (row + Math.Sign(Position.Item1 - row) * 2, Position.Item2);

                    if (!BoardScript.IsEmptySquare(evalJumpPosition) &&
                        BoardScript.IsEnemy(evalJumpPosition, PieceSide) &&
                        BoardScript.IsEmptySquare(evalBlankPosition)) {
                        if (move.Item1 * Math.Sign(row - Position.Item1) < 0) {
                            return (BoardScript.GetPosition(evalPosition), "Cannonization");
                        }
                    }
                }
            }
        }


        // // Check if a cannonized (attacking) bishop is in the same row/column as rook
        // List<(GameObject, string)> alist = BoardScript.GetAttackList(Position); // Get pieces attacking (this) rook
        // bool hollowBishop = false;
        // (int, int) bishopPosition = (-1, -1);
        //
        // for (int i = 0; i < alist.Count; i++) {
        //     if (alist[i].Item2 == "Cannonization" && // if has a cannonized attacker
        //         alist[i].Item1.GetComponent<PieceScript>().Type == PieceType.Bishop) // and is a bishop (redundant?)
        //     {
        //         bishopPosition = alist[i].Item1.GetComponent<PieceScript>().Position;
        //         // If bishop lined up
        //         if (bishopPosition.Item1 == Position.Item1 || bishopPosition.Item2 == Position.Item2) {
        //             hollowBishop = true;
        //             break;
        //         }
        //     }
        // }
        //
        // // If cannonized bishop on same row/col as (this) rook
        // if (hollowBishop) {
        //     List<(int, int)> adjacents = BoardScript.GetAdjacentPositions(Position, 1, taxicab: false, perimeter: true);
        //
        //     // Adjacent pieces immune to cannonization
        //     if (adjacents.Contains(AddMove(move))) {
        //         return "Cannonization";
        //     }
        //
        //     // Pieces opposite of bishop direction immune to cannonization
        //     if (bishopPosition.Item1 == Position.Item1) { // bishop and rook on same row
        //         if (AddMove(move).Item1 == Position.Item1 && // aligned
        //             (bishopPosition.Item2 - Position.Item2) * (AddMove(move).Item2 - Position.Item2) < 0) { // between
        //             return "Cannonization";
        //         }
        //     } else { // bishop and rook on same column
        //         if (AddMove(move).Item2 == Position.Item2 && // aligned
        //             (bishopPosition.Item1 - Position.Item1) * (AddMove(move).Item1 - Position.Item1) < 0) { // between
        //             return "Cannonization";
        //         }
        //     }
        // }

        return (null, "");
    }

    [Move]
    public (GameObject, string) HollowBastionBetweenProtect((int, int) move) {
        // check 2 rooks on same row/col for more cannonization protection
        var otherRooks = BoardScript.PieceReferences[(PieceType.Rook, PieceSide)];
        foreach (GameObject i in otherRooks) {
            (int, int) otherRookPosition = i.GetComponent<PieceScript>().Position;
            if (otherRookPosition.Item1 == Position.Item1) { // Same row
                if (AddMove(move).Item1 == Position.Item1 &&
                    (Position.Item2 - AddMove(move).Item2) * (otherRookPosition.Item2 - AddMove(move).Item2) < 0) {
                    // In between rooks
                    return (null, "Cannonization");
                }
            } else if (otherRookPosition.Item2 == Position.Item2) { // Same column
                if (AddMove(move).Item2 == Position.Item2 &&
                    (Position.Item1 - AddMove(move).Item1) * (otherRookPosition.Item1 - AddMove(move).Item1) < 0) {
                    // In between rooks
                    return (null, "Cannonization");
                }
            }
        }

        return (null, "");
    }
}