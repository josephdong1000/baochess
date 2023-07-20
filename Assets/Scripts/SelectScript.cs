using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SelectScript : MonoBehaviour {
    // public Color highlightedColor;
    // public Color selectedFriendlyColor;
    // public Color selectedEnemyColor;

    public float colorFadeSpeed;
    public float colorFadeDeltaTime;

    public Sprite blankSprite;
    public Sprite classicHighlightDotSprite;
    public Sprite babaHighlightDotSprite;

    // Deprecated
    // public Sprite whiteSprite;
    // public Sprite blackSprite;
    // public Sprite whiteHighlightSprite;
    // public Sprite blackHighlightSprite;


    [HideInInspector] public (int, int) Position;
    private bool _hovered;
    private bool _update;
    private SpriteRenderer _spriteRenderer;
    private SpriteRenderer _childSpriteRenderer;

    private Color _defaultColor;
    private Color _hoveredColor;
    private float _proportionHovered;

    private BoardScript _boardScript;
    private ThemeColorsManager _themeColorsManager;
    private ThemeManager.Theme _myTheme;
    

    private void Awake() {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _spriteRenderer.sprite = blankSprite;
        _childSpriteRenderer = transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>();
        _childSpriteRenderer.enabled = false;

        GameObject board = GameObject.FindGameObjectWithTag("Board");
        _boardScript = board.GetComponent<BoardScript>();

        _hovered = false;
        _update = true;
        _proportionHovered = 0;
        _themeColorsManager = GameObject.FindGameObjectWithTag("Theme Manager").GetComponent<ThemeColorsManager>();
        _myTheme = ThemeManager.Theme.None;
    }

    private void Start() {
        UpdateThemeSpritesColors();
    }

    // Update is called once per frame
    void Update() {
        UpdateThemeSpritesColors();
        if (_update) {
            // FetchSides();
            StartCoroutine(UpdateHoveringColor());
            _update = false;
        }
    }

    private void LateUpdate() {
        UpdateHighlightDot();
    }

    private void OnMouseOver() {
        _hovered = true;
    }

    private void OnMouseExit() {
        _hovered = false;
    }

    IEnumerator UpdateHoveringColor() {
        while (true) {

            if (_hovered) {
                _proportionHovered = Math.Min(_proportionHovered + colorFadeDeltaTime * colorFadeSpeed, 1);
            } else {
                _proportionHovered = Math.Max(_proportionHovered - colorFadeDeltaTime * colorFadeSpeed, 0);
            }

            // Lerp between default and hovered colors
            _spriteRenderer.color =
                Color.Lerp(_defaultColor,
                           (_defaultColor + _hoveredColor) * 0.5f, // Average between the colors
                           _proportionHovered);

            yield return new WaitForSeconds(colorFadeDeltaTime);
        }
    }

    private void UpdateThemeSpritesColors() {
        // Changes the board square's default colors and highlight dot sprites based on the current theme
        
        if (ThemeManager.CurrentTheme == ThemeManager.Theme.Classic &&
            _myTheme != ThemeManager.Theme.Classic) {
            
            _defaultColor = _boardScript.IsSameParity((0, 1), Position)
                ? _themeColorsManager.classicLightSquareColor
                : _themeColorsManager.classicDarkSquareColor;
            _hoveredColor = _themeColorsManager.classicHoveredColor;
            _childSpriteRenderer.sprite = classicHighlightDotSprite;
            
            _myTheme = ThemeManager.Theme.Classic;
        } else if (ThemeManager.CurrentTheme == ThemeManager.Theme.Baba &&
                   _myTheme != ThemeManager.Theme.Baba) {
            
            _defaultColor = _boardScript.IsSameParity((0, 1), Position)
                ? _themeColorsManager.babaLightSquareColor
                : _themeColorsManager.babaDarkSquareColor;
            _hoveredColor = _themeColorsManager.babaHoveredColor;
            _childSpriteRenderer.sprite = babaHighlightDotSprite;
            
            _myTheme = ThemeManager.Theme.Baba;
        }
    }

    /// <summary>
    /// If applicable, show the highlight dot
    /// </summary>
    private void UpdateHighlightDot() {
        

        if (_boardScript.HighlightedPositions.Contains(Position) &&
            !_boardScript.SelectedPositions.Contains(Position) &&
            !_boardScript.AttackPositions.Contains(Position)) {
            // If the sprite is highlightable
            _childSpriteRenderer.enabled = true;
        } else {
            _childSpriteRenderer.enabled = false;
        }
    }


    private void OnMouseDown() {
        if (BoardScript.SelectingMove) {
            _boardScript.SelectedPositions.Add(Position);
        }
    }
}