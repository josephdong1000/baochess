using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SelectScript : MonoBehaviour {
    public Color defaultColor;
    public Color hoveredColor;
    public Color highlightedColor;
    public Color selectedFriendlyColor;
    public Color selectedEnemyColor;

    public float colorFadeSpeed;
    public float colorFadeDeltaTime;

    public Sprite whiteSprite;
    public Sprite blackSprite;
    public Sprite whiteHighlightSprite;
    public Sprite blackHighlightSprite;

    public (int, int) Position { get; set; }
    private bool _hovered;
    private bool _update;
    private SpriteRenderer _spriteRenderer;

    private float _proportionHovered;

    // private List<(int, int)> _boardSelectedList;
    private List<(int, int)> _boardHighlightedList;
    private BoardScript _boardScript;
    private PieceScript.Side _playingSide;
    private PieceScript.Side _thisPieceSide;

    private void Awake() {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        GameObject board = GameObject.FindGameObjectWithTag("Board");
        _boardScript = board.GetComponent<BoardScript>();
        _boardHighlightedList = _boardScript.HighlightedPositions;

        _hovered = false;
        _update = true;
        _proportionHovered = 0;
    }

    private void Start() {
        // _spriteRenderer = GetComponent<SpriteRenderer>();
        // GameObject board = GameObject.FindGameObjectWithTag("Board");
        // _boardScript = board.GetComponent<BoardScript>();
        // _boardHighlightedList = _boardScript.HighlightedPositions;
        //
        // _hovered = false;
        // _update = true;
        // _proportionHovered = 0;

        
        UpdateBaseColor();
    }

    // Update is called once per frame
    void Update() {
        if (_update) {
            FetchSides();
            StartCoroutine(UpdateHoveringColor());
            _update = false;
        }
        UpdateBaseColor();
    }

    private void OnMouseOver() {
        _hovered = true;
    }

    private void OnMouseExit() {
        _hovered = false;
    }

    IEnumerator UpdateHoveringColor() {
        
        while (true) {

            // UpdateBaseColor();

            if (_hovered) {
                // _proportionHovered = Math.Min(_proportionHovered + Time.deltaTime * colorFadeSpeed, 1);
                _proportionHovered = Math.Min(_proportionHovered + colorFadeDeltaTime * colorFadeSpeed, 1);
            } else {
                // _proportionHovered = Math.Max(_proportionHovered - Time.deltaTime * colorFadeSpeed, 0);
                _proportionHovered = Math.Max(_proportionHovered - colorFadeDeltaTime * colorFadeSpeed, 0);
            }

            // Lerp between default and hovered colors
            _spriteRenderer.color =
                Color.Lerp((_boardHighlightedList.Contains(Position)) ? highlightedColor : defaultColor,
                           hoveredColor,
                           _proportionHovered);
            
            yield return new WaitForSeconds(colorFadeDeltaTime);
        }
    }

    private void FetchSides() {
        _playingSide = _boardScript.PlayingSide;
        if (_boardScript.IsEmptySquare(Position)) {
            _thisPieceSide = PieceScript.Side.None;
        } else {
            _thisPieceSide = _boardScript.GetPieceSide(Position);
        }
    }

    private void UpdateBaseColor() {
        if (_boardScript.HighlightedPositions.Contains(Position) &&
            !_boardScript.SelectedPositions.Contains(Position) &&
            !_boardScript.AttackPositions.Contains(Position)) {
            _spriteRenderer.sprite = _boardScript.IsSameParity((0, 1), Position)
                ? whiteHighlightSprite
                : blackHighlightSprite;
        } else {
            _spriteRenderer.sprite = _boardScript.IsSameParity((0, 1), Position) ? whiteSprite : blackSprite;
        }
    }


    private void OnMouseDown() {
        if (BoardScript.SelectingMove) {
            _boardScript.SelectedPositions.Add(Position);    
        }
        

        // if (!Input.GetKey(KeyCode.Space)) {
        // }

        // Hacked together piece changer DELETE LATER

        // if (Input.GetKey(KeyCode.Space)) {
        //     List<char> thing = new List<char> {
        //         ' ', 'r', 'n', 'b', 'q', 'k', 'p', 'f',
        //         'R', 'N', 'B', 'Q', 'K', 'P', 'F'
        //     };
        //     int index = thing.IndexOf(_boardScript.BoardTemplateReverse[7 - Position.Item1, Position.Item2]);
        //     if (thing[index] != 'F') {
        //         _boardScript.BoardTemplateReverse[7 - Position.Item1, Position.Item2] = thing[index + 1];
        //         // for (int i = 0; i < 8; i++) {
        //         //     for (int j = 0; j < 8; j++) {
        //         //         Debug.Log(_boardScript.BoardTemplateReverse[7 - i, j] + " is " + (i, j));
        //         //     }
        //         // }
        //     } else {
        //         _boardScript.BoardTemplateReverse[7 - Position.Item1, Position.Item2] = ' ';
        //     }
        //     
        //     _boardScript.UpdateBoardTemplate();
        //     
        //     
        //     
        //     _boardScript.DeleteList.Add(_boardScript.GetPosition(Position));
        //     _boardScript.DeletePieces();
        //     // _boardScript.UpdatePiecePositions();
        //     // _boardScript.UpdatePieceGameObjectPositions();
        //     _boardScript.ClearBoard();
        //     _boardScript.PopulateBoard();
        //     _boardScript.ResetGameLoop();
        // }
    }

    // private void UpdatePosition() {
    //     Position = gameObject.GetComponent<PieceScript>().Position;
    // }

    // private void UpdateBoardSelectedList() {
    //     // _boardSelectedList = boar
    // }
}