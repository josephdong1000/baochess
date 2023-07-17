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
    public static Dictionary<string, Dictionary<(string, bool), bool>> BanningMovesDict;

    /// <summary>
    /// Given a move (key) that grants extra moves, return how many moves it grants (value)
    /// </summary>
    public static Dictionary<string, int> ExtraMovesDict;

    private static BoardScript _boardScript;
    private static List<string> _allMoveNames;
    private static Dictionary<(string, bool), bool> _swordInTheStoneDictSelfEnemy;
    private static Dictionary<(string, bool), bool> _swordInTheStoneDictEnemy;

    // Start is called before the first frame update
    void Awake() {
        // _boardScript = GameObject.FindGameObjectWithTag("Board").GetComponent<BoardScript>();

        ExtraMovesDict = new() {
            { "CoregencyReplace", 1 },
            { "CoregencyRevert", 1 },
            { "SwordInTheStone", 1 },
            { "BattlefieldCommandSelf", 1 },
        };
        
        _swordInTheStoneDictEnemy = MoveList.AllSpecialMoveNames
            .ToDictionary(s => (s, true), _ => true)
            .ToDictionary(e => e.Key, e => e.Value);

        
        _swordInTheStoneDictSelfEnemy = MoveList.InvertMoveSelection(MoveList.GetAllMoves(typeof(KingScript)))
            .Append("SwordInTheStone")
            .Append("SwordInTheStoneNull")
            // .Append("BattlefieldCommandSelf") // Already included
            .ToDictionary(s => (s, false), _ => true)
            .Concat(MoveList.AllSpecialMoveNames.ToDictionary(s => (s, true), _ => true))
            .ToDictionary(e => e.Key, e => e.Value);

        BanningMovesDict = new() {
            {
                "CoregencyRevert", MoveList.GetAllMoves(typeof(QueenScript))
                    .ToDictionary(s => (s, false), _ => true)
            }, {
                "SwordInTheStone", _swordInTheStoneDictSelfEnemy
            }, {
                "BattlefieldCommandSelf", _swordInTheStoneDictSelfEnemy
            }, {
                "SwordInTheStoneNull", _swordInTheStoneDictEnemy
            }, {
                "BattlefieldCommandSelfNull", _swordInTheStoneDictEnemy
            }
        };
    }
}