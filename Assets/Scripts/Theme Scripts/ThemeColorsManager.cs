using System;
using UnityEngine;

/// <summary>
/// Holds all color information for themes
/// This class can probably be a singleton...
/// </summary>
public class ThemeColorsManager : MonoBehaviour {
    public static ThemeColorsManager Instance { get; private set; }

    public Color defaultColor;

    // Classic colors
    public Color classicWhiteColor;
    public Color classicBlackColor;
    public bool useClassicPieceColor; // False since sprites are correct color

    public Color classicLightSquareColor;
    public Color classicDarkSquareColor;
    public Color classicHoveredColor;
    public Color classicTextColor;
    public Color classicGreyTextColor;

    // Baba colors
    public Color babaWhiteColor;
    public Color babaBlackColor;
    public bool useBabaPieceColor; // True since sprites are all white, and must be colored

    public Color babaLightSquareColor;
    public Color babaDarkSquareColor;
    public Color babaHoveredColor;
    public Color babaTextColor;
    public Color babaGreyTextColor;

    private void Awake() {
        if (Instance != null) {
            Destroy(this);
            return;
        }

        Instance = this;
    }
}