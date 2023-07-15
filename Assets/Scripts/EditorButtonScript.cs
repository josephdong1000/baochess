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
    private SpriteRenderer _spriteRenderer;
    private SpriteRenderer _childSpriteRenderer;


    // Start is called before the first frame update
    void Start() {
        SelectedPieceType = (PieceScript.PieceType.Empty, PieceScript.Side.None);
        _boardScript = GameObject.FindGameObjectWithTag("Board").GetComponent<BoardScript>();
        AllPieceTypes = _boardScript.TypePieceDict.Keys
            .Skip(1)
            .Select(s => (s, PieceScript.Side.White))
            .Concat(_boardScript.TypePieceDict.Keys.Skip(1).Select(s => (s, PieceScript.Side.Black)))
            .Append((PieceScript.PieceType.Empty, PieceScript.Side.None))
            .ToList();
        // _editorButtons = new();
        
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _childSpriteRenderer = transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>();

        // _childSpriteRenderer.sprite = AllPieceTypes.IndexOf(thisPieceType) % 7

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
            _childSpriteRenderer.sprite = spriteList.Last();
        } else {
            // Debug.Log(AllPieceTypes.IndexOf(thisPieceType));
            // Debug.Log(AllPieceTypes.IndexOf(thisPieceType) % spriteList.Count);

            _childSpriteRenderer.sprite = spriteList[AllPieceTypes.IndexOf(thisPieceType) % (spriteList.Count - 1)];
            _childSpriteRenderer.color = thisPieceType.Item2 == PieceScript.Side.White
                ? _boardScript.whiteColor
                : _boardScript.blackColor;
        }

        if (SelectedPieceType != thisPieceType) {
            _spriteRenderer.color = unhighlightColor;
        } else {
            _spriteRenderer.color = highlightColor;
        }
    }
}