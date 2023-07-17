using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

/// <summary>
/// Contains functions that check if a move that involves multiple pieces is legal
/// </summary>
public class MultipleChecker : MonoBehaviour {
    private static GameObject _board;
    private static BoardScript _boardScript;
    private static int _boardSize;

    public static
        Dictionary<Func<(int, int), (int, int), (bool, List<(int, int)>, List<(int, int)>)>,
            (List<PieceScript.PieceType>, List<PieceScript.PieceType>)> PieceIdentity = new() {
            {
                TrojanHorse,
                (new List<PieceScript.PieceType> {
                    PieceScript.PieceType.Knight
                }, new List<PieceScript.PieceType> {
                    PieceScript.PieceType.Pawn
                })
            }, {
                Castle,
                (new List<PieceScript.PieceType> {
                    PieceScript.PieceType.King
                }, new List<PieceScript.PieceType> {
                    PieceScript.PieceType.Rook
                })
            }, {
                BattlefieldCommand,
                (new List<PieceScript.PieceType> {
                    PieceScript.PieceType.Pawn,
                    PieceScript.PieceType.Rook,
                    PieceScript.PieceType.Knight,
                    PieceScript.PieceType.Queen,
                    PieceScript.PieceType.Footmen
                }, new List<PieceScript.PieceType> {
                    PieceScript.PieceType.King
                })
            }, {
                BattlefieldCommandSelf,
                (new List<PieceScript.PieceType> {
                    PieceScript.PieceType.Pawn,
                    PieceScript.PieceType.Rook,
                    PieceScript.PieceType.Knight,
                    PieceScript.PieceType.Queen,
                    PieceScript.PieceType.Footmen
                }, new List<PieceScript.PieceType> {
                    PieceScript.PieceType.King
                })
            }, {
                BattlefieldCommandSelfNull,
                (new List<PieceScript.PieceType> {
                    PieceScript.PieceType.Pawn,
                    PieceScript.PieceType.Rook,
                    PieceScript.PieceType.Knight,
                    PieceScript.PieceType.Queen,
                    PieceScript.PieceType.Footmen
                }, new List<PieceScript.PieceType> {
                    PieceScript.PieceType.King
                })
            }
        };
    
    // Start is called before the first frame update
    void Start() {
        _board = GameObject.FindGameObjectWithTag("Board");
        _boardScript = _board.GetComponent<BoardScript>();
        _boardSize = _boardScript.boardSize;
    }

    public static Dictionary<PieceScript.PieceType, Dictionary<string, List<(int, int)>>> AllMoveDicts() {
        // PieceScript piece;
        Dictionary<PieceScript.PieceType, Dictionary<string, List<(int, int)>>> output =
            new Dictionary<PieceScript.PieceType, Dictionary<string, List<(int, int)>>>();

        // Hardcoded monstrosity
        output.Add(PieceScript.PieceType.Pawn, (new PawnScript()).MoveDict);
        output.Add(PieceScript.PieceType.Knight, (new KnightScript()).MoveDict);
        output.Add(PieceScript.PieceType.Rook, (new RookScript()).MoveDict);
        output.Add(PieceScript.PieceType.Bishop, (new BishopScript()).MoveDict);
        output.Add(PieceScript.PieceType.Footmen, (new FootmenScript()).MoveDict);
        output.Add(PieceScript.PieceType.Queen, (new QueenScript()).MoveDict);
        output.Add(PieceScript.PieceType.King, (new KingScript()).MoveDict);

        return output;
    }

    /// <summary>
    /// Given the positions of a knight and pawn, return all possible final positions for the knight and pawn.
    /// </summary>
    /// <param name="knightPosition"></param>
    /// <param name="pawnPosition"></param>
    /// <returns>3-long tuple. If there are/are not moves, possible knight final positions, possible pawn final positions</returns>
    [Move]
    public static (bool, List<(int, int)>, List<(int, int)>) TrojanHorse((int, int) knightPosition,
        (int, int) pawnPosition) {
        // Checking that Knight and Pawn are in positions is responsibility of BoardScript
        // Also checking that Knight and Pawn are the same color
        // Also checking that they are Knight and Pawn

        var falseReturn = (false, new List<(int, int)>(), new List<(int, int)>());

        // Knight and pawn are adjacent
        if (_boardScript.Distance(knightPosition, pawnPosition, taxicab: false) > 1) {
            return falseReturn;
        }


        List<(int, int)> finalKnightPositions = new List<(int, int)>();
        List<(int, int)> finalPawnPositions = new List<(int, int)>();
        // Check all possible moves that the knight can take
        foreach (var possibleKnightMove in AllMoveDicts()[PieceScript.PieceType.Knight]["move"]) {
            (int, int) finalKnightPosition = _boardScript.AddMovePositions(knightPosition, possibleKnightMove);
            (int, int) finalPawnPosition = _boardScript.AddMovePositions(pawnPosition, possibleKnightMove);

            // Final positions must be inside board
            if (_boardScript.IsOutsideBoard(finalKnightPosition) || _boardScript.IsOutsideBoard(finalPawnPosition)) {
                continue;
            }

            PieceScript.Side thisPieceSide = _boardScript.GetPieceSide(knightPosition);

            bool knightOccupied = !_boardScript.IsEmptySquare(finalKnightPosition);
            bool pawnOccupied = !_boardScript.IsEmptySquare(finalPawnPosition);


            // If no squares occupied, legal
            if (!knightOccupied && !pawnOccupied) {
                finalKnightPositions.Add(finalKnightPosition);
                finalPawnPositions.Add(finalPawnPosition);
                continue;
            }

            // If both square occupied, illegal
            if (knightOccupied && pawnOccupied) {
                continue;
            }

            // If 1 square occupied, check it is an enemy and not a king
            if (knightOccupied) { // Knight final position occupied
                if (_boardScript.IsEnemy(finalKnightPosition, thisPieceSide)) { // capturing an enemy
                    finalKnightPositions.Add(finalKnightPosition);
                    finalPawnPositions.Add(finalPawnPosition);
                    continue;
                }
            } else { // Pawn final position occupied
                if (_boardScript.IsEnemy(finalPawnPosition, thisPieceSide) && // capturing an enemy
                    _boardScript.GetPieceType(finalPawnPosition) != PieceScript.PieceType.King) { // that is not a king
                    finalKnightPositions.Add(finalKnightPosition);
                    finalPawnPositions.Add(finalPawnPosition);
                    continue;
                }
            }
        }

        return (finalKnightPositions.Count != 0, finalKnightPositions, finalPawnPositions);
    }


    /// <summary>
    /// Given the positions of a king and rook, return if the king and rook can castle and to which positions. King cannot castle in check (this function checks for this) 
    /// </summary>
    /// <param name="kingPosition"></param>
    /// <param name="rookPosition"></param>
    /// <returns></returns>
    [Move]
    public static (bool, List<(int, int)>, List<(int, int)>) Castle((int, int) kingPosition, (int, int) rookPosition) {
        var falseReturn = (false, new List<(int, int)>(), new List<(int, int)>());

        // Ensure king and rook have not moved
        if (_boardScript.GetNumberPerformedMoves(kingPosition) != 0 ||
            _boardScript.GetNumberPerformedMoves(rookPosition) != 0) {
            return falseReturn;
        }

        // Ensure clear path between king and rook
        if (!_boardScript.IsClearRow(kingPosition, rookPosition)) {
            return falseReturn;
        }

        // Ensure King is not under attack
        if (_boardScript.GetAttackers(kingPosition).Count > 0) {
            return falseReturn;
        }

        // Return castle positions (long or short)
        int direction = Math.Sign(rookPosition.Item2 - kingPosition.Item2); // +1 = right
        return (true,
                new List<(int, int)> { (kingPosition.Item1, kingPosition.Item2 + 2 * direction) },
                new List<(int, int)> { (kingPosition.Item1, kingPosition.Item2 + direction) });
    }


    [Move]
    public static (bool, List<(int, int)>, List<(int, int)>) BattlefieldCommand((int, int) anyPosition,
        (int, int) kingPosition) {
        var falseReturn = (false, new List<(int, int)>(), new List<(int, int)>());
        int numMoves = 1;

        // Check if king is adjacent to another same-color king
        var adjacentGameObjects = _boardScript.GetAdjacentGameObjects(kingPosition, 1, false, true, false);
        foreach (GameObject adjacentPieceGameObject in adjacentGameObjects) {
            if (adjacentPieceGameObject.GetComponent<PieceScript>().Type == PieceScript.PieceType.King &&
                adjacentPieceGameObject.GetComponent<PieceScript>().PieceSide ==
                _boardScript.GetPieceSide(kingPosition)) {
                numMoves = 2;
                break;
            }
        }

        // And this king has to be adjacent to the anyPiece
        if (_boardScript.Distance(kingPosition, anyPosition, taxicab: false) > 1) {
            return falseReturn;
        }

        // anyPiece moves 1-2 squares away
        var finalPiecePositions = _boardScript.GetAdjacentPositions(anyPosition, numMoves, false, false, true);

        // anyPiece cannot capture a friendly piece
        finalPiecePositions.RemoveAll(s => !_boardScript.IsEmptySquare(s) &&
                                           !_boardScript.IsEnemy(
                                               s,
                                               _boardScript.GetPosition(anyPosition).GetComponent<PieceScript>()
                                                   .PieceSide));

        var finalKingPositions =
            Enumerable.Repeat(kingPosition, finalPiecePositions.Count).ToList(); // King remains stationary


        return (finalPiecePositions.Count > 0, finalPiecePositions, finalKingPositions);
    }


    /// <summary>
    /// Same as BattlefieldCommand, except for self-pieces --> enables Sword in the Stone
    /// </summary>
    /// <param name="anyPosition"></param>
    /// <param name="kingPosition"></param>
    /// <returns></returns>
    [Move]
    [SpecialMove]
    public static (bool, List<(int, int)>, List<(int, int)>) BattlefieldCommandSelf((int, int) anyPosition,
        (int, int) kingPosition) {
        var falseReturn = (false, new List<(int, int)>(), new List<(int, int)>());
        int numMoves = 1;

        // Check if king is adjacent to another same-color king
        var adjacentGameObjects = _boardScript.GetAdjacentGameObjects(kingPosition, 1, false, true, false);
        foreach (GameObject adjacentPieceGameObject in adjacentGameObjects) {
            if (adjacentPieceGameObject.GetComponent<PieceScript>().Type == PieceScript.PieceType.King &&
                adjacentPieceGameObject.GetComponent<PieceScript>().PieceSide ==
                _boardScript.GetPieceSide(kingPosition)) {
                numMoves = 2;
                break;
            }
        }

        // And this king has to be adjacent to the anyPiece
        if (_boardScript.Distance(kingPosition, anyPosition, taxicab: false) > 1) {
            return falseReturn;
        }

        // anyPiece moves 1-2 squares away
        var finalPiecePositions = _boardScript.GetAdjacentPositions(anyPosition, numMoves, false, false, true);

        // Debug.Log(string.Join(",", finalPiecePositions.Select(t => string.Format("[ '{0}', '{1}']", t.Item1, t.Item2))));
        // Debug.Log(finalPiecePositions.Count);

        // anyPiece must capture a piece
        finalPiecePositions.RemoveAll(s => _boardScript.IsEmptySquare(s));

        // anyPiece cannot capture an enemy piece
        finalPiecePositions.RemoveAll(s => _boardScript.IsEnemy(
                                          s,
                                          _boardScript.GetPosition(anyPosition).GetComponent<PieceScript>().PieceSide));

        // anyPiece cannot capture a friendly king
        finalPiecePositions.RemoveAll(s =>
                                          !_boardScript.IsEnemy( // Is friendly
                                              s,
                                              _boardScript.GetPosition(anyPosition).GetComponent<PieceScript>()
                                                  .PieceSide) &&
                                          _boardScript.GetPieceType(s) == PieceScript.PieceType.King); // Is a king 

        var finalKingPositions =
            Enumerable.Repeat(kingPosition, finalPiecePositions.Count).ToList(); // King remains stationary

        return (finalPiecePositions.Count > 0, finalPiecePositions, finalKingPositions);
    }

    [Move]
    [SpecialMove]
    public static (bool, List<(int, int)>, List<(int, int)>) BattlefieldCommandSelfNull((int, int) anyPosition,
        (int, int) kingPosition) {
        return BattlefieldCommandSelf(anyPosition, kingPosition);
    }
}