using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class MoveList : MonoBehaviour {
    public static List<string> AllSpecialMoveNames;
    public static List<string> AllMoveNames;

    private static BoardScript _boardScript;

    private List<Type> _pieceTypes = new List<Type> {
        typeof(PawnScript),
        typeof(BishopScript),
        typeof(RookScript),
        typeof(FootmenScript),
        typeof(KingScript),
        typeof(KnightScript),
        typeof(QueenScript),
        typeof(MultipleChecker),
    };

    private void Awake() {
        _boardScript = GameObject.FindGameObjectWithTag("Board").GetComponent<BoardScript>();

        AllMoveNames = new();
        AllSpecialMoveNames = new();
        
        foreach (Type pieceType in _pieceTypes) {
            AllMoveNames = AllMoveNames.Concat(GetAllMoves(pieceType)).ToList();
            AllSpecialMoveNames = AllSpecialMoveNames.Concat(GetSpecialMoves(pieceType)).ToList();
        }
    }
    
    /// <summary>
    /// Inverts the moves selected. Useful for pulling all moves except for one
    /// </summary>
    /// <param name="moveList"></param>
    /// <returns></returns>
    public static List<string> InvertMoveSelection(List<string> moveList) {
        // Flatten all move names
        List<string> output = new();

        foreach (string moveName in AllMoveNames) {
            if (!moveList.Contains(moveName)) {
                output.Add(moveName);
            }
        }

        return output;
    }

    public static List<string> GetAllMoves(Type pieceClass) {
        return GetMethodInfos(pieceClass)
            // .Where(s => !s.IsSpecialName)
            .Where(m => m.IsDefined(typeof(MoveAttribute)))
            .Select(s => s.Name)
            .ToList();
    }

    public static List<string> GetSpecialMoves(Type pieceClass) {
        return GetMethodInfos(pieceClass)
            .Where(m => m.IsDefined(typeof(SpecialMoveAttribute)))
            .Select(s => s.Name).ToList();
    }

    public static List<MethodInfo> GetMethodInfos(Type pieceClass) {
        return pieceClass.GetMethods(BindingFlags.Instance |
                                     BindingFlags.Static |
                                     BindingFlags.Public |
                                     BindingFlags.DeclaredOnly).ToList();
    }
}