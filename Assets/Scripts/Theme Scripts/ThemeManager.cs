using System;
using System.Data;
using System.Runtime.CompilerServices;
using UnityEngine;

public class ThemeManager : MonoBehaviour {
    public static Theme CurrentTheme { get; private set; }
    public static ThemeManager Instance { get; private set; }

    public delegate void ThemeChangeEvent();

    public static event ThemeChangeEvent OnThemeChange;


    // private static ThemeColorsManager _themeColorsManager;

    public enum Theme {
        Classic,
        Baba,
        None,
    }

    private void Awake() {
        if (Instance != null) {
            Destroy(this);
            return;
        }

        Instance = this;
        CurrentTheme = Theme.Baba;
    }

    private void Start() {
        OnThemeChanged();
    }


    public static void SetClassic() {
        CurrentTheme = Theme.Classic;
        OnThemeChanged();
    }

    public static void SetBaba() {
        CurrentTheme = Theme.Baba;
        OnThemeChanged();
    }

    /// <summary>
    /// Publishes to ThemeManager.ThemeChange event subscribers
    /// For efficient theme changing (i.e. not every Update() cycle)
    /// </summary>
    private static void OnThemeChanged() {
        OnThemeChange?.Invoke();
    }

    /// <summary>
    /// Returns "visual" theme color. Ignores bool useThemePieceColor
    /// </summary>
    /// <returns></returns>
    public (Color, Color) GetThemeColor() {
        if (CurrentTheme == Theme.Baba) {
            return (ThemeColorsManager.Instance.babaWhiteColor,
                    ThemeColorsManager.Instance.babaBlackColor);
        }

        if (CurrentTheme == Theme.Classic) {
            return (ThemeColorsManager.Instance.classicWhiteColor,
                    ThemeColorsManager.Instance.classicBlackColor);
        }

        return default;
    }

    public Color GetThemeBaseTextColor() {
        return GetThemeBaseTextColor(CurrentTheme);
    }

    public Color GetThemeBaseTextColor(Theme theme) {
        if (theme == Theme.Classic) {
            return ThemeColorsManager.Instance.classicTextColor;
        }

        if (theme == Theme.Baba) {
            return ThemeColorsManager.Instance.babaTextColor;
        }

        return default;
    }

    /// <summary>
    /// Returns "true" theme color based on CurrentTheme. Considers bool useThemePieceColor
    /// </summary>
    /// <returns></returns>
    public (Color, Color) GetThemePieceColor() {
        return GetThemePieceColor(CurrentTheme);
    }

    /// <summary>
    /// Returns "true" theme color. Considers bool useThemePieceColor
    /// </summary>
    /// <returns></returns>
    public (Color, Color) GetThemePieceColor(Theme theme) {
        if (theme == Theme.Baba && ThemeColorsManager.Instance.useBabaPieceColor) {
            return (ThemeColorsManager.Instance.babaWhiteColor,
                    ThemeColorsManager.Instance.babaBlackColor);
        }

        if (theme == Theme.Classic && ThemeColorsManager.Instance.useClassicPieceColor) {
            return (ThemeColorsManager.Instance.classicWhiteColor,
                    ThemeColorsManager.Instance.classicBlackColor);
        }

        return (ThemeColorsManager.Instance.defaultColor,
                ThemeColorsManager.Instance.defaultColor);
    }
}