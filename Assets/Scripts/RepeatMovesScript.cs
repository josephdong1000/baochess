using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;

public class RepeatMovesScript : MonoBehaviour {
    /// <summary>
    /// <para>Given a move, return a Dictionary of subsequent moves that it bans.</para>
    /// <para>Banned dictionary says how long the banned move & if enemy (string, bool) is banned for (value, # turns)</para>
    /// </summary>
    public static Dictionary<string, Dictionary<(string, bool), int>> BanningMovesDict;

    /// <summary>
    /// Given a move (key) that grants extra moves, return how many moves it grants (value)
    /// </summary>
    public static Dictionary<string, int> ExtraMovesDict;

    private static BoardScript _boardScript;
    private static List<string> _allMoveNames;


    // Start is called before the first frame update
    void Start() {
        // _boardScript = GameObject.FindGameObjectWithTag("Board").GetComponent<BoardScript>();

        ExtraMovesDict = new() {
            { "CoregencyReplace", 1 },
            { "CoregencyRevert", 1 },
            { "SwordInTheStone", 1 },
            { "BattlefieldCommandSelf", 1 },
        };

        BanningMovesDict = new() {
            {
                "CoregencyRevert", MoveList.GetAllMoves(typeof(QueenScript))
                    .ToDictionary(s => (s, false), _ => 1)
            }, {
                "SwordInTheStone",
                MoveList.InvertMoveSelection(MoveList.GetAllMoves(typeof(KingScript)))
                    .Append("SwordInTheStone")
                    // .Append("BattlefieldCommandSelf") // Already included
                    .ToDictionary(s => (s, false), _ => 1)
                    .Concat(MoveList.AllSpecialMoveNames.ToDictionary(s => (s, true), _ => 1))
                    .ToDictionary(e => e.Key, e => e.Value)
            }, {
                "BattlefieldCommandSelf",
                MoveList.InvertMoveSelection(MoveList.GetAllMoves(typeof(KingScript)))
                    .Append("SwordInTheStone")
                    // .Append("BattlefieldCommandSelf")
                    .ToDictionary(s => (s, false), _ => 1)
                    .Concat(MoveList.AllSpecialMoveNames.ToDictionary(s => (s, true), _ => 1))
                    .ToDictionary(e => e.Key, e => e.Value)
            }
        };

    }
}