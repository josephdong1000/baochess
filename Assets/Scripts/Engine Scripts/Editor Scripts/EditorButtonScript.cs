using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
// using Theme_Scripts;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

public class EditorButtonScript : MonoBehaviour {
    [HideInInspector] public Color highlightColor;
    [HideInInspector] public Color unhighlightColor;

    /// <summary>
    /// Hardcoded sprite list for piece themes.
    /// Would be nice to just take directly from the Piece scripts themselves
    /// </summary>
    public List<Sprite> classicSpriteList;
    [FormerlySerializedAs("spriteList")] public List<Sprite> babaSpriteList;
    
    [HideInInspector] public (PieceScript.PieceType, PieceScript.Side) ThisPieceType;
    public static (PieceScript.PieceType, PieceScript.Side) SelectedPieceType;
    public static List<(PieceScript.PieceType, PieceScript.Side)> AllPieceTypes { get; private set; }

    private SpriteRenderer _backgroundSpriteRenderer;
    private SpriteRenderer _pieceSpriteRenderer;
    private List<Sprite> _spriteList;
    private ThemeManager.Theme _myTheme;
    private Color _whiteColor;
    private Color _blackColor;


    private void Awake() {
        SelectedPieceType = (PieceScript.PieceType.Empty, PieceScript.Side.None);
        
        _pieceSpriteRenderer = transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>();
        _backgroundSpriteRenderer = transform.GetChild(1).gameObject.GetComponent<SpriteRenderer>();

        _myTheme = ThemeManager.Theme.None;
    }

    // Start is called before the first frame update
    void Start() {
        AllPieceTypes = BoardScript.TypePieceDict.Keys
            .Skip(1)
            .Select(s => (s, PieceScript.Side.White))
            .Append((PieceScript.PieceType.Empty, PieceScript.Side.None))
            .Concat(BoardScript.TypePieceDict.Keys.Skip(1).Select(s => (s, PieceScript.Side.Black)))
            .Append((PieceScript.PieceType.Empty, PieceScript.Side.None))
            .ToList();
        
        // UpdateThemeSpritesColors();
    }

    private void OnMouseDown() {
        SelectedPieceType = ThisPieceType;
    }

    private void Update() {
        if (ThisPieceType == default) {
            return;
        }
        
        UpdateThemeSpritesColors();

        if (ThisPieceType.Item1 == PieceScript.PieceType.Empty) {
            _pieceSpriteRenderer.sprite = _spriteList.Last();
        } else {
            // _pieceSpriteRenderer.sprite = _spriteList[AllPieceTypes.IndexOf(ThisPieceType) % _spriteList.Count];
            _pieceSpriteRenderer.sprite = _spriteList[AllPieceTypes.IndexOf(ThisPieceType)];
            _pieceSpriteRenderer.color = ThisPieceType.Item2 == PieceScript.Side.White
                ? _whiteColor
                : _blackColor;
        }

        if (SelectedPieceType != ThisPieceType) {
            _backgroundSpriteRenderer.color = unhighlightColor;
        } else {
            _backgroundSpriteRenderer.color = highlightColor;
        }
    }

    private void UpdateThemeSpritesColors() {
        // Changes the editor button's default colors and highlight dot sprites based on the current theme

        if (ThemeManager.CurrentTheme != _myTheme) {
            _myTheme = ThemeManager.CurrentTheme;
            (_whiteColor, _blackColor) = ThemeManager.Instance.GetThemePieceColor();
            
            if (ThemeManager.CurrentTheme == ThemeManager.Theme.Classic) {
                highlightColor = ThemeColorsManager.Instance.classicLightSquareColor;
                unhighlightColor = ThemeColorsManager.Instance.classicDarkSquareColor;
                _spriteList = classicSpriteList;
            } else if (ThemeManager.CurrentTheme == ThemeManager.Theme.Baba) {
                
                highlightColor = ThemeColorsManager.Instance.babaLightSquareColor;
                unhighlightColor = ThemeColorsManager.Instance.babaDarkSquareColor;
                _spriteList = babaSpriteList;
            } else {
                throw new Exception("Invalid theme selected");
            }
        }
    }
}