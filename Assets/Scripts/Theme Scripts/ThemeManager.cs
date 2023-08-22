using System;
using System.Data;
using System.Runtime.CompilerServices;
using UnityEngine;

// namespace Theme_Scripts {
    // public delegate void ThemePropertyChangedDelegate(ThemeManager.Theme newValue);

    public class ThemeManager : MonoBehaviour {

        private static Theme _currentTheme;
        // public static event ThemePropertyChangedDelegate ThemePropertyChanged;

        // public static Theme CurrentTheme {
        //     get { return _currentTheme;}
        //     private set {
        //         if (value != _currentTheme) {
        //             CurrentTheme = value;
        //             if (ThemePropertyChanged != null) {
        //                 ThemePropertyChanged(value); // Invoke event in setter
        //             }
        //         }
        //     }
        // }
        public static Theme CurrentTheme;
        
        public static ThemeManager Instance;
        private static ThemeColorsManager _themeColorsManager; 
    
        public enum Theme {
            Classic,
            Baba,
            None,
        }
    
        public void Start() {
            CurrentTheme = Theme.Baba;
            Instance = this;
            _themeColorsManager = GetComponent<ThemeColorsManager>();
        }


        public static void SetClassic() {
            CurrentTheme = Theme.Classic;
        }

        public static void SetBaba() {
            CurrentTheme = Theme.Baba;
        }

        /// <summary>
        /// Returns "visual" theme color. Ignores bool useThemePieceColor
        /// </summary>
        /// <returns></returns>
        public (Color, Color) GetThemeColor() {
            if (CurrentTheme == Theme.Baba) {
                return (_themeColorsManager.babaWhiteColor, _themeColorsManager.babaBlackColor);
            }
            if (CurrentTheme == Theme.Classic) {
                return (_themeColorsManager.classicWhiteColor, _themeColorsManager.classicBlackColor);
            }
            return default;
        }

        public Color GetThemeBaseTextColor(Theme theme) {
            if (theme == Theme.Classic) {
                return _themeColorsManager.classicTextColor;
            }
            if (theme == Theme.Baba) {
                return _themeColorsManager.babaTextColor;
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
            if (theme == Theme.Baba) {
                if (_themeColorsManager.useBabaPieceColor) {
                    return (_themeColorsManager.babaWhiteColor, _themeColorsManager.babaBlackColor);
                }
            } else if (theme == Theme.Classic) {
                if (_themeColorsManager.useClassicPieceColor) {
                    return (_themeColorsManager.classicWhiteColor, _themeColorsManager.classicBlackColor);
                }
            }
            return (_themeColorsManager.defaultColor, _themeColorsManager.defaultColor);
        }
    }
// }