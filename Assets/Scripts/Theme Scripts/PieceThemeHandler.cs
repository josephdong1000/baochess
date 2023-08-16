using System;
using UnityEngine;

public class PieceThemeHandler : MonoBehaviour {
    public Sprite classicWhiteSprite;
    public Sprite classicBlackSprite;

    [HideInInspector] public Color classicWhiteColor;
    [HideInInspector] public Color classicBlackColor;

    public Sprite babaWhiteSprite;
    public Sprite babaBlackSprite;

    [HideInInspector] public Color babaWhiteColor;
    [HideInInspector] public Color babaBlackColor;


    [HideInInspector] public Sprite whiteSprite;
    [HideInInspector] public Sprite blackSprite;
    [HideInInspector] public Color whiteColor;
    [HideInInspector] public Color blackColor;


    private ThemeColorsManager _themeColorsManager;
    private ThemeManager.Theme _myTheme;


    private void Awake() {
        _themeColorsManager = GameObject.FindGameObjectWithTag("Theme Manager").GetComponent<ThemeColorsManager>();
        classicWhiteColor = _themeColorsManager.classicWhiteColor;
        classicBlackColor = _themeColorsManager.classicBlackColor;
        babaWhiteColor = _themeColorsManager.babaWhiteColor;
        babaBlackColor = _themeColorsManager.babaBlackColor;
        _myTheme = ThemeManager.Theme.None;
        UpdateSpriteAndColor();
    }
    
    private void Update() {
        UpdateSpriteAndColor();
    }
    
    private void UpdateSpriteAndColor() {
        if (ThemeManager.CurrentTheme == ThemeManager.Theme.Classic &&
            _myTheme != ThemeManager.Theme.Classic) {
            whiteSprite = classicWhiteSprite;
            blackSprite = classicBlackSprite;
            whiteColor = classicWhiteColor;
            blackColor = classicBlackColor;
            _myTheme = ThemeManager.Theme.Classic;
        } else if (ThemeManager.CurrentTheme == ThemeManager.Theme.Baba &&
                   _myTheme != ThemeManager.Theme.Baba) {
            whiteSprite = babaWhiteSprite;
            blackSprite = babaBlackSprite;
            whiteColor = babaWhiteColor;
            blackColor = babaBlackColor;
            _myTheme = ThemeManager.Theme.Baba;
        }
    }
}