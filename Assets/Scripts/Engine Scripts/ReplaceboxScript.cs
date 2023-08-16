using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ReplaceboxScript : MonoBehaviour {

    public Color outlineColor;
    public Color fillColor;
    public Sprite pawnSprite;
    public Sprite rookSprite;
    public Sprite knightSprite;
    public Sprite bishopSprite;
    public Sprite queenSprite;
    public Sprite kingSprite;
    public Sprite footmenSprite;
    

    public Vector3 CenterPosition { get; private set; }
    public PieceScript.PieceType PieceType { get; set; }

    public static int SelectedReplace;

    private BoardScript _boardScript;
    private int _edgeLength;
    private int _replaceIndex;

    private List<GameObject> _replaceBoxList;
    private SpriteRenderer _outlineSpriteRenderer;
    private SpriteRenderer _fillSpriteRenderer;
    private SpriteRenderer _pieceSpriteRenderer;
    private Dictionary<PieceScript.PieceType, Sprite> _pieceTypeToSprite;

    // Start is called before the first frame update
    void Start() {
        _boardScript = GameObject.FindGameObjectWithTag("Board").GetComponent<BoardScript>();
        _edgeLength = _boardScript.edgeLength;

        SelectedReplace = -1;
        _replaceIndex = _boardScript.ReplaceBoxList.IndexOf(gameObject);
        // _pieceType = _boardScript.ReplaceBoxList[_replaceIndex].GetComponent<PieceScript>().Type;
        
        CenterPosition = transform.position;
        transform.rotation = Quaternion.Euler(0,
                                              0, 
                                              90 * _replaceIndex);
        transform.position = CenterPosition - transform.up * _edgeLength;

        _outlineSpriteRenderer = transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>();
        _fillSpriteRenderer = transform.GetChild(1).gameObject.GetComponent<SpriteRenderer>();
        _pieceSpriteRenderer = transform.GetChild(2).gameObject.GetComponent<SpriteRenderer>();
        transform.GetChild(2).gameObject.transform.rotation = Quaternion.identity;

        _outlineSpriteRenderer.color = outlineColor;
        _fillSpriteRenderer.color = fillColor;
        _pieceSpriteRenderer.color = _boardScript.PlayingSide == PieceScript.Side.White
            ? _boardScript.whiteColor
            : _boardScript.blackColor;

        _pieceTypeToSprite = new() {
            { PieceScript.PieceType.Pawn, pawnSprite },
            { PieceScript.PieceType.Rook, rookSprite },
            { PieceScript.PieceType.Knight, knightSprite },
            { PieceScript.PieceType.Bishop, bishopSprite },
            { PieceScript.PieceType.Queen, queenSprite },
            { PieceScript.PieceType.King, kingSprite },
            { PieceScript.PieceType.Footmen, footmenSprite }
        };

        UpdatePieceSprite(PieceType);
    }

    private void UpdatePieceSprite(PieceScript.PieceType pieceType) {
        _pieceSpriteRenderer.sprite = _pieceTypeToSprite[pieceType];
    }

    private void OnMouseDown() {
        SelectedReplace = _replaceIndex;
    }
}
