using System;
using System.Collections;
using System.Collections.Generic;
using Theme_Scripts;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class TextHandler : MonoBehaviour {

    [HideInInspector] public TMP_FontAsset babaFont;
    [HideInInspector] public float babaFontScale; // Multiplier off base font size
    [HideInInspector] public float babaLineSpacingOffset;
    
    [HideInInspector] public TMP_FontAsset classicFont;
    [HideInInspector] public float classicFontScale;
    [HideInInspector] public float classicLineSpacingOffset;

    private float _initFontSize;
    private ThemeManager.Theme _myTheme;
    private TMP_Text _tmpText;


    // Start is called before the first frame update
    void Start() {
        _tmpText = GetComponent<TMP_Text>();
        _initFontSize = _tmpText.fontSize;
        _myTheme = ThemeManager.Theme.Baba;

        babaFont = Resources.Load<TMP_FontAsset>("Fonts/MedievalSharp-Regular SDF");
        babaFontScale = 1;
        babaLineSpacingOffset = 0;
        classicFont = Resources.Load<TMP_FontAsset>("Fonts/EBGaramond-VariableFont_wght SDF");
        classicFontScale = 1;
        classicLineSpacingOffset = -15;


        // Debug.Log(classicFont);
        // Debug.Log(babaFont);

    }

    private void LateUpdate() {

        // Debug.Log(_tmpText);
        if (ThemeManager.CurrentTheme == ThemeManager.Theme.Classic &&
            _myTheme != ThemeManager.Theme.Classic) {
            _tmpText.font = classicFont;
            _tmpText.fontSize = _initFontSize * classicFontScale;
            _tmpText.lineSpacing = classicLineSpacingOffset;
            
            _myTheme = ThemeManager.CurrentTheme;
        } else if (ThemeManager.CurrentTheme == ThemeManager.Theme.Baba &&
                   _myTheme != ThemeManager.Theme.Baba) {
            _tmpText.font = babaFont;
            _tmpText.fontSize = _initFontSize * babaFontScale;
            _tmpText.lineSpacing = babaLineSpacingOffset;
            
            _myTheme = ThemeManager.CurrentTheme;
        }
    }
}