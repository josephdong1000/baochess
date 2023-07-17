using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoardStateScript : MonoBehaviour {
    public static readonly Dictionary<bool[], (PieceScript.PieceType, PieceScript.Side, bool)> BoolsToPiece =
        new Dictionary<bool[], (PieceScript.PieceType, PieceScript.Side, bool)>(BoolArrayComparator.Default) {
            { IntToBools(0), (PieceScript.PieceType.Pawn, PieceScript.Side.White, false) },
            { IntToBools(1), (PieceScript.PieceType.Pawn, PieceScript.Side.Black, false) },
            { IntToBools(2), (PieceScript.PieceType.Knight, PieceScript.Side.White, false) },
            { IntToBools(3), (PieceScript.PieceType.Knight, PieceScript.Side.Black, false) },
            { IntToBools(4), (PieceScript.PieceType.Bishop, PieceScript.Side.White, false) },
            { IntToBools(5), (PieceScript.PieceType.Bishop, PieceScript.Side.Black, false) },
            { IntToBools(6), (PieceScript.PieceType.Rook, PieceScript.Side.White, false) },
            { IntToBools(7), (PieceScript.PieceType.Rook, PieceScript.Side.Black, false) },
            { IntToBools(8), (PieceScript.PieceType.Queen, PieceScript.Side.White, false) },
            { IntToBools(9), (PieceScript.PieceType.Queen, PieceScript.Side.Black, false) },
            { IntToBools(10), (PieceScript.PieceType.Footmen, PieceScript.Side.White, false) },
            { IntToBools(11), (PieceScript.PieceType.Footmen, PieceScript.Side.Black, false) },
            { IntToBools(12), (PieceScript.PieceType.King, PieceScript.Side.White, false) }, // if white playing
            { IntToBools(13), (PieceScript.PieceType.King, PieceScript.Side.Black, false) }, // if black playing
            { IntToBools(14), default }, // deprecated
            // { IntToBools(14), (PieceScript.PieceType.King, PieceScript.Side.None, false) }, // the not playing one
            { IntToBools(15), default },
            { IntToBools(16), (PieceScript.PieceType.Pawn, PieceScript.Side.White, true) }, // moved pawn
            { IntToBools(17), (PieceScript.PieceType.Pawn, PieceScript.Side.Black, true) },
            { IntToBools(18), default },
            { IntToBools(19), default },
            { IntToBools(20), default },
            { IntToBools(21), default },
            { IntToBools(22), (PieceScript.PieceType.Rook, PieceScript.Side.White, true) }, // moved rook
            { IntToBools(23), (PieceScript.PieceType.Rook, PieceScript.Side.Black, true) },
            { IntToBools(24), default },
            { IntToBools(25), default },
            { IntToBools(26), default },
            { IntToBools(27), default },
            { IntToBools(28), default },
            { IntToBools(29), default },
            { IntToBools(30), default },
            { IntToBools(31), default },
        };

    public static readonly Dictionary<(PieceScript.PieceType, PieceScript.Side, bool), bool[]> PieceToBools =
        BoolsToPiece.Where(x => x.Value != default)
            .ToDictionary(x => x.Value, x => x.Key);

    /// <summary>
    /// <para>64 bools to flag as occupied or not</para>
    /// <para>Next 5 * 46 sequentially indicate piece type, side, and moved or not per square</para>
    /// <para>Remaining 4 bools indicate how long footmen have been on the board</para>
    /// <para>1 bool for playing player, 1 bool for banned special moves for current player</para>
    /// </summary>
    public static readonly int NumBools = 64 + 5 * 46 + 4 + 1 + RepeatMovesScript.BanningMovesDict.Count; // ~300

    public static List<bool[]> BoardStates = new List<bool[]>();

    // private static BoardScript _boardScript;

    private static readonly int _boardSize = 8;
    private static readonly int _maxNumPieces = 46;


    public static bool[] BoardToBools(GameObject[,] board, PieceScript.Side playingSide, bool[] currentBannedFlags) {
        bool[] output = new bool[NumBools];

        int typeIndex = 0;
        int footmenMoves = 0;

        PieceScript ps;
        for (int i = 0; i < _boardSize; i++) {
            for (int j = 0; j < _boardSize; j++) {
                ps = board[i, j].GetComponent<PieceScript>();
                if (ps.Type != PieceScript.PieceType.Empty) {
                    output[i * _boardSize + j] = true;
                    if (ps.Type == PieceScript.PieceType.Rook ||
                        ps.Type == PieceScript.PieceType.Pawn) {
                        Array.Copy(PieceToBools[(ps.Type, ps.PieceSide, ps.MoveCounter > 0)],
                                   0,
                                   output,
                                   _boardSize * _boardSize + typeIndex * 5,
                                   5);
                    } else {
                        Array.Copy(PieceToBools[(ps.Type, ps.PieceSide, false)],
                                   0,
                                   output,
                                   _boardSize * _boardSize + typeIndex * 5,
                                   5);
                        if (ps.Type == PieceScript.PieceType.Footmen) {
                            if (ps.PieceSide == PieceScript.Side.White) {
                                footmenMoves |= Math.Min(3, ps.TurnsOnBoard) << 2;
                            } else {
                                footmenMoves |= Math.Min(3, ps.TurnsOnBoard);
                                Debug.Log(ps.TurnsOnBoard);
                            }
                        }
                    }

                    typeIndex += 1;
                }
            }
        }

        Array.Copy(IntToBools(footmenMoves, numBits: 4),
                   0,
                   output,
                   _boardSize * _boardSize + _maxNumPieces * 5,
                   4);
        
        output[_boardSize * _boardSize + _maxNumPieces * 5 + 4] = playingSide == PieceScript.Side.White;
        
        Array.Copy(currentBannedFlags,
            0,
            output,
            _boardSize * _boardSize + _maxNumPieces * 5 + 4 + 1,
            RepeatMovesScript.BanningMovesDict.Count);
        
        return output;
    }

    public static (GameObject[,], PieceScript.Side, bool[]) BoolsToBoard(bool[] bools) {
        GameObject[,] output = new GameObject[_boardSize, _boardSize];
        int typeIndex = _boardSize * _boardSize;
        for (int i = 0; i < _boardSize; i++) {
            for (int j = 0; j < _boardSize; j++) {
                if (bools[i * _boardSize + j]) { // Occupied square

                    var (pieceType, side, hasMoved) = BoolsToPiece[bools.Skip(typeIndex).Take(5).ToArray()];
                    output[i, j] = Instantiate(BoardScript.TypePieceDict[pieceType]);
                    output[i, j].GetComponent<PieceScript>().PieceSide = side;

                    if (pieceType == PieceScript.PieceType.Footmen) {
                        if (side == PieceScript.Side.White) {
                            output[i, j].GetComponent<PieceScript>()
                                .SetTurnsOnBoard(
                                    BoolsToInt(bools.Skip(_boardSize * _boardSize + _maxNumPieces * 5)
                                                   .Take(2)
                                                   .ToArray()));
                        } else {
                            output[i, j].GetComponent<PieceScript>()
                                .SetTurnsOnBoard(
                                    BoolsToInt(bools.Skip(_boardSize * _boardSize + _maxNumPieces * 5 + 2)
                                                   .Take(2)
                                                   .ToArray()));
                            
                            Debug.Log(BoolsToInt(bools.Skip(_boardSize * _boardSize + _maxNumPieces * 5 + 2)
                                                     .Take(2)
                                                     .ToArray()));
                        }
                    } else {
                        output[i, j].GetComponent<PieceScript>().SetTurnsOnBoard(0);
                    }

                    output[i, j].GetComponent<PieceScript>().MoveCounter = hasMoved ? 1 : 0;
                    

                    // if (hasMoved) {
                    //     output[i, j].GetComponent<PieceScript>().IncrementMoveCounter();
                    // } else {
                    //     output[i, j].GetComponent<PieceScript>().MoveCounter = 0;
                    // }

                    typeIndex += 5;
                } else {
                    output[i, j] = Instantiate(BoardScript.TypePieceDict[PieceScript.PieceType.Empty]);
                }

                output[i, j].GetComponent<PieceScript>().Position = (i, j);
            }
        }
        
        PieceScript.Side playingSide = bools[_boardSize * _boardSize + _maxNumPieces * 5 + 4] ? PieceScript.Side.White : PieceScript.Side.Black;

        bool[] banSpecialNext = bools
            .Skip(_boardSize * _boardSize + _maxNumPieces * 5 + 4 + 1)
            .Take(RepeatMovesScript.BanningMovesDict.Count)
            .ToArray();
        
        return (output, playingSide, banSpecialNext);
    }

    public static void StoreBoardState(GameObject[,] board, PieceScript.Side playingSide, bool[] banSpecialNext) {
        BoardStates.Add(BoardToBools(board, playingSide, banSpecialNext));
    }

    public static (GameObject[,], PieceScript.Side, bool[]) GetBoardState(int index) {
        return BoolsToBoard(BoardStates[index]);
    }
    
    public static (GameObject[,], PieceScript.Side, bool[]) PopBoardState() {
        var value = BoolsToBoard(BoardStates.Last()); 
        BoardStates.RemoveAt(BoardStates.Count - 1);
        return value;
    }
    
    public static bool[] IntToBools(int num, int numBits = 5) {
        BitArray b = new BitArray(new int[] { num });
        // b.Length = numBits;

        bool[] bits = new bool[b.Count];
        b.CopyTo(bits, 0);
        Array.Reverse(bits);
        bits = bits.Skip(bits.Length - numBits).ToArray();
        return bits;

        // return b;
    }

    public static int BoolsToInt(bool[] bools) {
        BitArray bitField = new BitArray(bools.Reverse().ToArray());
        int[] output = new int[1];
        bitField.CopyTo(output, 0);
        return output[0];
    }
}