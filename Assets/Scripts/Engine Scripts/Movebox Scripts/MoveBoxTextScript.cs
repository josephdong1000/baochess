using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
// using Theme_Scripts;
using UnityEngine;

public class MoveBoxTextScript : MonoBehaviour {
    public Color queenColor;
    public Color footmenColor;
    public Color kingColor;
    public Color bishopColor;
    public Color knightColor;
    public Color pawnColor;
    public Color rookColor;

    public static string QueenHex { get; private set; }
    public static string FootmenHex { get; private set; }
    public static string KingHex { get; private set; }
    public static string BishopHex { get; private set; }
    public static string KnightHex { get; private set; }
    public static string PawnHex { get; private set; }
    public static string RookHex { get; private set; }

    public static readonly Dictionary<string, string> MoveDescriptionsRaw = new() {
        {"WallOfInfantry", "If a pawn is not adjacent to any friendly pawns, pawns can move 1 tile horizontally. Pawns cannot capture with this move."},
        {"PawnForward", ""},
        {"PawnForwardTwo", ""},
        {"PawnCapture", ""},
        {"LiveAnotherDay", "Threatened pawns (able to be captured next turn) can move backwards one or two tiles. Pawns cannot capture with this move."},
        {"PawnPassive", "While adjacent to a friendly pawn, knights cannot be captured by Maginot Line."},
        {"PromotePassive", ""},
        {"PromoteAttack", ""},
        {"BishopMove", ""},
        {"Cannonization", "When the bishop is directly behind a single, lone friendly piece and there is an enemy piece in the same column, row, or diagonal and no more than 6 squares away on the same color square as the bishop is on, you may jump over the allied piece and capture that piece."},
        {"CloisteredMove", "You can move again as long as you either touch another wall in the middle or at the end of your trajectory—you may only capture 1 piece but may not move again after it."},
        {"CloisteredProtect", "While on an edge, bishops cannot be captured by a knight using Serfs Up."},
        {"RookMove", ""},
        {"MaginotLine", "While two friendly rooks are on the same row or within five squares on the same column and are unobstructed, one rook may move twice in one turn but must first move in the direction of the other rook."},
        {"HollowBastionMove", "If there is only one rook, it may move through friendly pawns and kings."},
        {"HollowBastionOverProtect", "Rooks are immune to cannonization and pieces behind and adjacent to a rook cannot be targeted by cannonization while in the same row or column as the attack bishop."},
        {"HollowBastionBetweenProtect", "While there are two rooks on the same row or column, all pieces between them are immune to cannonization."},
        {"FootmenForward", "Same as pawn forward"},
        {"FootmenCapture", "Same as pawn capture"},
        {"TacticalRetreatForward", "If at the last rank of the board, a footman may move backwards one square or capture in a backwards diagonal pattern (same as pawn)."},
        {"TacticalRetreatCapture", "If at the last rank of the board, a footman may move backwards one square or capture in a backwards diagonal pattern (same as pawn)."},
        {"KingMove", "The King is always treated as a promoted pawn. The King may capture friendly pieces (not including friendly Kings) including with Battlefield Command. This capture may ignore checks. Until the end of your opponent’s next turn after the King captures any friendly piece, no piece can use special abilities (including your own). Once per turn, if a King captures a friendly piece you can move the King again."},
        {"PyrrhicManeuver", "The King is always treated as a promoted pawn. The King may capture friendly pieces (not including friendly Kings) including with Battlefield Command. This capture may ignore checks. Until the end of your opponent’s next turn after the King captures any friendly piece, no piece can use special abilities (including your own). Once per turn, if a King captures a friendly piece you can move the King again."},
        {"PyrrhicManeuverNull", "The King is always treated as a promoted pawn. The King may capture friendly pieces (not including friendly Kings) including with Battlefield Command. This capture may ignore checks. Until the end of your opponent’s next turn after the King captures any friendly piece, no piece can use special abilities (including your own). Once per turn, if a King captures a friendly piece you can move the King again."},
        {"CoregencyRevert", "If a player used Coregency and if only one king is checked, you may choose to revert the checked king to a promoted queen which cannot use Queen Mother."},
        {"KnightMove", ""},
        {"SerfsUpMove", "While adjacent to a friendly pawn, Knights can capture the first piece they jump over in their regular move if this move is not to a square adjacent to any of the same friendly pawns it started next to. Knights cannot capture 2 pieces and if they wish to move to an occupied square they must capture that piece and cannot use Serfs Up."},
        {"QueenMove", ""},
        {"QueenMother", "Queen may move, but not capture 1 square in any direction, and then turn into a 3x3 ring of 8 footmen with no footman on the square the queen detonated on. This captures all pieces in a ring adjacent to where the ability activates. Footmen have the same attack and movement pattern as pawns. This ability may only be used once per game per player, and cannot capture Pawns (includes King)."},
        {"CoregencyReplace", "must be done on the first turn of the game, does not use up a turn. Turns your queen into a second king requiring both be captured/checkmated for victory. This queen becomes a king then has the abilities of a king and also a king’s movement. If both kings are checked then one king must be brought out of check, but if only one king is checked you may choose to revert the checked king to a promoted queen which cannot use Queen Mother. This does not take up a turn, but that queen cannot move this turn. A King can never be moved into check even if you have two."},
        {"CoregencyProtect", "Queens are always immune being captured by Battlefield Command"},
        {"TrojanHorse", "If a knight begins a turn adjacent to a pawn, it can move with the pawn (maintains the same relative position to the knight) but one of the pawn or the knight can capture during this turn and moves must have two valid spaces at the start of the move. The pawn cannot capture kings."},
        {"Castle", ""},
        {"BattlefieldCommand", "Any piece within 1 tile of the king can use the king’s movement/attack pattern instead of its own. Under Co-regency, if a piece is simultaneously under the command of 2 Kings or a King under Battlefield command,  it can move twice under the effect of Battlefield Command. Bishops cannot be moved by this ability and and can not be under its influence. Cannot pass on the effects of Battlefield Command or Live Another Day with this ability."},
        {"BattlefieldCommandSelf", "Use Battlefield Command to play Pyrrhic Maneuver"},
        {"BattlefieldCommandSelfNull", "Use Battlefield Command to play Pyrrhic Maneuver"}
    };

    public static Dictionary<string, string> MoveDescriptionsPretty = new();

    public void Awake() {
        QueenHex = ColorUtility.ToHtmlStringRGBA(queenColor);
        FootmenHex = ColorUtility.ToHtmlStringRGBA(footmenColor);
        KingHex = ColorUtility.ToHtmlStringRGBA(kingColor);
        BishopHex = ColorUtility.ToHtmlStringRGBA(bishopColor);
        KnightHex = ColorUtility.ToHtmlStringRGBA(knightColor);
        PawnHex = ColorUtility.ToHtmlStringRGBA(pawnColor);
        RookHex = ColorUtility.ToHtmlStringRGBA(rookColor);

        // MoveDescriptionsPretty = new();
        foreach (var (moveName, moveDescription) in MoveDescriptionsRaw) {
            MoveDescriptionsPretty.Add(moveName, ColorMoveRichText(moveDescription));
        }
    }
    
    // public static string GenerateMoveDescription(string moveName) {
    //     return ColorMoveRichText(MoveDescriptionsRaw[moveName]);
    // }
    
    public static string ColorMoveRichText(string moveDesc) {
        moveDesc = Regex.Replace( moveDesc, "queen\\b", $"<color=#{QueenHex}>Queen</color>", RegexOptions.IgnoreCase);
        moveDesc = Regex.Replace( moveDesc, "queens", $"<color=#{QueenHex}>Queens</color>", RegexOptions.IgnoreCase);
        
        moveDesc = Regex.Replace( moveDesc, "footmen", $"<color=#{FootmenHex}>Footmen</color>", RegexOptions.IgnoreCase);
        moveDesc = Regex.Replace( moveDesc, "footman", $"<color=#{FootmenHex}>Footman</color>", RegexOptions.IgnoreCase);
        
        moveDesc = Regex.Replace( moveDesc, "king\\b", $"<color=#{KingHex}>King</color>", RegexOptions.IgnoreCase);
        moveDesc = Regex.Replace( moveDesc, "kings", $"<color=#{KingHex}>Kings</color>", RegexOptions.IgnoreCase);
        
        moveDesc = Regex.Replace( moveDesc, "bishop\\b", $"<color=#{BishopHex}>Bishop</color>", RegexOptions.IgnoreCase);
        moveDesc = Regex.Replace( moveDesc, "bishops", $"<color=#{BishopHex}>Bishops</color>", RegexOptions.IgnoreCase);
        
        moveDesc = Regex.Replace( moveDesc, "knight\\b", $"<color=#{KnightHex}>Knight</color>", RegexOptions.IgnoreCase);
        moveDesc = Regex.Replace( moveDesc, "knights", $"<color=#{KnightHex}>Knights</color>", RegexOptions.IgnoreCase);
        
        moveDesc = Regex.Replace( moveDesc, "pawn\\b", $"<color=#{PawnHex}>Pawn</color>", RegexOptions.IgnoreCase);
        moveDesc = Regex.Replace( moveDesc, "pawns", $"<color=#{PawnHex}>Pawns</color>", RegexOptions.IgnoreCase);
        
        moveDesc = Regex.Replace( moveDesc, "rook\\b", $"<color=#{RookHex}>Rook</color>", RegexOptions.IgnoreCase);
        moveDesc = Regex.Replace( moveDesc, "rooks", $"<color=#{RookHex}>Rooks</color>", RegexOptions.IgnoreCase);
        
        return moveDesc;
    }
    
}
