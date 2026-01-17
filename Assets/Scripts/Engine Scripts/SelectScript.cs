using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
// using Theme_Scripts;
using UnityEngine;

public class SelectScript : MonoBehaviour {
    
    public float colorFadeSpeed;
    public float colorFadeDeltaTime;

    public Sprite blankSprite;
    public Sprite classicHighlightDotSprite;
    public Sprite babaHighlightDotSprite;
    
    [HideInInspector] public (int, int) Position;
    private bool _hovered;
    private bool _update;
    private SpriteRenderer _spriteRenderer;
    private SpriteRenderer _childSpriteRenderer;

    private Color _defaultColor;
    private Color _hoveredColor;
    private float _proportionHovered;

    private BoardScript _boardScript;
    // private ThemeColorsManager _themeColorsManager;
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
        // _themeColorsManager = GameObject.FindGameObjectWithTag("Theme Manager").GetComponent<ThemeColorsManager>();
        // _myTheme = ThemeManager.Theme.None;

        // Efficiently change themes
        ThemeManager.OnThemeChange += UpdateThemeSpritesColors;
    }

    private void Start() {
        UpdateThemeSpritesColors();
    }

    void Update() {
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

    private void OnMouseDown() {
        if (BoardScript.SelectingMove) {
            _boardScript.SelectedPositions.Add(Position);
        }
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
        if (ThemeManager.CurrentTheme == ThemeManager.Theme.Classic) {
            _defaultColor = _boardScript.IsSameParity((0, 1), Position)
                ? ThemeColorsManager.Instance.classicLightSquareColor
                : ThemeColorsManager.Instance.classicDarkSquareColor;
            _hoveredColor = ThemeColorsManager.Instance.classicHoveredColor;
            if (_childSpriteRenderer != null) {
                _childSpriteRenderer.sprite = classicHighlightDotSprite;
            } 
        } else if (ThemeManager.CurrentTheme == ThemeManager.Theme.Baba) {
            _defaultColor = _boardScript.IsSameParity((0, 1), Position)
                ? ThemeColorsManager.Instance.babaLightSquareColor
                : ThemeColorsManager.Instance.babaDarkSquareColor;
            _hoveredColor = ThemeColorsManager.Instance.babaHoveredColor;
            if (_childSpriteRenderer != null){
                _childSpriteRenderer.sprite = babaHighlightDotSprite;
            }
        } else {
            Debug.LogError($"Not a valid Theme: {ThemeManager.CurrentTheme}", this);
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
}