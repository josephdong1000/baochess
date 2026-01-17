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

    public static int BanningMovesDictLength = 5;

    /// <summary>
    /// Given a move (key) that grants extra moves, return how many moves it grants (value)
    /// </summary>
    public static Dictionary<string, int> ExtraMovesDict;

    private static BoardScript _boardScript;
    private static List<string> _allMoveNames;
    private static Dictionary<(string, bool), bool> _pyrrhicManeuverDictSelfEnemy; // Banned moves for yourself
    private static Dictionary<(string, bool), bool> _pyrrhicManeuverDictEnemy; // Banned moves for opponent

    // Start is called before the first frame update
    private void Awake() {
        // _boardScript = GameObject.FindGameObjectWithTag("Board").GetComponent<BoardScript>();

        ExtraMovesDict = new() {
            // 06/19/2024 PATCH: CoregencyReplace (0th turn) consumes a turn (it was too broken)
            // { "CoregencyReplace", 1 },
            { "CoregencyRevert", 1 },
            { "PyrrhicManeuver", 1 },
            { "BattlefieldCommandSelf", 1 },
        };
    }

    private void Start() {
        _pyrrhicManeuverDictEnemy = MoveList.AllSpecialMoveNames
            .ToDictionary(s => (s, true), _ => true)
            .ToDictionary(e => e.Key, e => e.Value);

        // 06/19/2024 PATCH: PyrrhicManeuver enables you to move any piece one more time 
        // _pyrrhicManeuverDictSelfEnemy = MoveList.InvertMoveSelection(MoveList.GetAllMoves(typeof(KingScript)))...
        _pyrrhicManeuverDictSelfEnemy = new List<string> { "PyrrhicManeuver", "PyrrhicManeuverNull" }
            .ToDictionary(s => (s, false), _ => true)
            .Concat(MoveList.AllSpecialMoveNames.ToDictionary(s => (s, true), _ => true))
            .ToDictionary(e => e.Key, e => e.Value);

        // Remember to update BanningMovesDictLength when adding/removing to this list
        BanningMovesDict = new() {
            {
                "CoregencyRevert", MoveList.GetAllMoves(typeof(QueenScript))
                    .ToDictionary(s => (s, false), _ => true)
            }, {
                "PyrrhicManeuver", _pyrrhicManeuverDictSelfEnemy
            }, {
                "BattlefieldCommandSelf", _pyrrhicManeuverDictSelfEnemy
            }, {
                "PyrrhicManeuverNull", _pyrrhicManeuverDictEnemy
            }, {
                "BattlefieldCommandSelfNull", _pyrrhicManeuverDictEnemy
            }
        };
    }
}