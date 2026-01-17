using System;
// using Theme_Scripts;
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


    // private ThemeColorsManager _themeColorsManager;


    private void Awake() {
        (classicWhiteColor, classicBlackColor) = ThemeManager.Instance.GetThemePieceColor(ThemeManager.Theme.Classic); 
        (babaWhiteColor, babaBlackColor) = ThemeManager.Instance.GetThemePieceColor(ThemeManager.Theme.Baba); 
        
        // Using a publisher-subscriber model to efficiently change theme colors
        ThemeManager.OnThemeChange += UpdateSpriteAndColor;
    }

    private void Start() {
        UpdateSpriteAndColor();
    }
    
    private void UpdateSpriteAndColor() {
        if (ThemeManager.CurrentTheme == ThemeManager.Theme.Classic) {
            whiteSprite = classicWhiteSprite;
            blackSprite = classicBlackSprite;
            whiteColor = classicWhiteColor;
            blackColor = classicBlackColor;
        } else if (ThemeManager.CurrentTheme == ThemeManager.Theme.Baba) {
            whiteSprite = babaWhiteSprite;
            blackSprite = babaBlackSprite;
            whiteColor = babaWhiteColor;
            blackColor = babaBlackColor;
        } else {
            Debug.LogError($"Not a valid Theme: {ThemeManager.CurrentTheme}", this);
        }
    }
}