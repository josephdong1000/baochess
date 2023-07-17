using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class EditorButtonScript : MonoBehaviour {
    public Color highlightColor;
    public Color unhighlightColor;
    public List<Sprite> spriteList;


    public (PieceScript.PieceType, PieceScript.Side) thisPieceType { get; set; }

    public static (PieceScript.PieceType, PieceScript.Side) SelectedPieceType;
    public static List<(PieceScript.PieceType, PieceScript.Side)> AllPieceTypes { get; private set; }

    private BoardScript _boardScript;
    private SpriteRenderer _backgroundSpriteRenderer;
    private SpriteRenderer _pieceSpriteRenderer;


    // Start is called before the first frame update
    void Start() {
        SelectedPieceType = (PieceScript.PieceType.Empty, PieceScript.Side.None);
        _boardScript = GameObject.FindGameObjectWithTag("Board").GetComponent<BoardScript>();
        AllPieceTypes = BoardScript.TypePieceDict.Keys
            .Skip(1)
            .Select(s => (s, PieceScript.Side.White))
            .Append((PieceScript.PieceType.Empty, PieceScript.Side.None))
            .Concat(BoardScript.TypePieceDict.Keys.Skip(1).Select(s => (s, PieceScript.Side.Black)))
            .Append((PieceScript.PieceType.Empty, PieceScript.Side.None))
            .ToList();
        // _editorButtons = new();
        
        // Debug.Log(string.Join(",", AllPieceTypes));

        // _backgroundSpriteRenderer = GetComponent<SpriteRenderer>();
        _pieceSpriteRenderer = transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>();
        _backgroundSpriteRenderer = transform.GetChild(1).gameObject.GetComponent<SpriteRenderer>();

        // _pieceSpriteRenderer.sprite = AllPieceTypes.IndexOf(thisPieceType) % 7

        // Debug.Log(string.Join(",", AllPieceTypes));
    }

    private void OnMouseDown() {
        SelectedPieceType = thisPieceType;
    }

    private void Update() {
        // if (AllPieceTypes.IndexOf(thisPieceType) == -1) {
        if (thisPieceType == default) {
            // Debug.Log(thisPieceType);
            return;
        }

        
        

        if (thisPieceType.Item1 == PieceScript.PieceType.Empty) {
            _pieceSpriteRenderer.sprite = spriteList.Last();
        } else {
            _pieceSpriteRenderer.sprite = spriteList[AllPieceTypes.IndexOf(thisPieceType) % spriteList.Count];
            // _pieceSpriteRenderer.sprite = spriteList[AllPieceTypes.IndexOf(thisPieceType) % (spriteList.Count - 1)];
            _pieceSpriteRenderer.color = thisPieceType.Item2 == PieceScript.Side.White
                ? _boardScript.whiteColor
                : _boardScript.blackColor;
        }

        if (SelectedPieceType != thisPieceType) {
            _backgroundSpriteRenderer.color = unhighlightColor;
        } else {
            _backgroundSpriteRenderer.color = highlightColor;
        }
    }
}