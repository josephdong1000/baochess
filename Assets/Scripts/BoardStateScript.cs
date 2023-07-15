using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class BoardStateScript {
    public static readonly Dictionary<bool[], (PieceScript.PieceType, PieceScript.Side, bool)> BoolsToPiece =
        new Dictionary<bool[], (PieceScript.PieceType, PieceScript.Side, bool)> {
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
            { IntToBools(14), (PieceScript.PieceType.King, PieceScript.Side.None, false) }, // if neither playing
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
    /// </summary>
    public static readonly int NumBools = 64 + 5 * 46 + 4; // ~298

    private static BoardScript _boardScript;

    private static readonly int _boardSize = 8;
    private static readonly int _maxNumPieces = 46;


    public static bool[] BoardToBools(GameObject[,] board) {
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
                            if (ps.whiteSprite) {
                                footmenMoves |= ps.MoveCounter << 2;
                            } else {
                                footmenMoves |= ps.MoveCounter;
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


        return output;
    }

    public static bool[] IntToBools(int num, int numBits = 5) {
        BitArray b = new BitArray(new int[] { num });
        bool[] bits = new bool[b.Count];
        b.CopyTo(bits, 0);
        Array.Reverse(bits);
        bits = bits.Skip(bits.Length - numBits).ToArray();
        return bits;
    }


    // Enumerable.SequenceEqual(target1, target2);
}