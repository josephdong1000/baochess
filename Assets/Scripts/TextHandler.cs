using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class TextHandler : MonoBehaviour {

    [HideInInspector] public TMP_FontAsset babaFont;
    [HideInInspector] public float babaFontScale; // Multiplier off base font size
    [HideInInspector] public TMP_FontAsset classicFont;
    [HideInInspector] public float classicFontScale;

    private float _initFontSize;
    private ThemeManager.Theme _myTheme;
    private TextMeshPro _textMeshPro;


    // Start is called before the first frame update
    void Start() {
        _textMeshPro = GetComponent<TextMeshPro>();
        _initFontSize = _textMeshPro.fontSize;

        babaFont = Resources.Load<TMP_FontAsset>("Assets/Fonts/MedievalSharp-Regular SDF.asset");
        babaFontScale = 1;
        classicFont = Resources.Load<TMP_FontAsset>("Assets/Fonts/EBGaramond-VariableFont_wght SDF.asset");
        classicFontScale = 1;
    }

    private void LateUpdate() {
        if (ThemeManager.CurrentTheme == ThemeManager.Theme.Classic &&
            _myTheme != ThemeManager.Theme.Classic) {
            _textMeshPro.font = classicFont;
            _textMeshPro.fontSize = _initFontSize * classicFontScale;
            _myTheme = ThemeManager.CurrentTheme;
        } else if (ThemeManager.CurrentTheme == ThemeManager.Theme.Baba &&
                   _myTheme != ThemeManager.Theme.Baba) {
            _textMeshPro.font = babaFont;
            _textMeshPro.fontSize = _initFontSize * babaFontScale;
            _myTheme = ThemeManager.CurrentTheme;
        }
    }
}