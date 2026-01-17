using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VersionInfoScript : MonoBehaviour {
    private TMP_Text _tmpText;
    
    private void Awake() {
        _tmpText = GetComponent<TMP_Text>();
        ThemeManager.OnThemeChange += ChangeTextColor;
    }

    private void ChangeTextColor() {
        if (ThemeManager.CurrentTheme == ThemeManager.Theme.Classic) {
            _tmpText.color = ThemeColorsManager.Instance.classicGreyTextColor;
        } else if (ThemeManager.CurrentTheme == ThemeManager.Theme.Baba) {
            _tmpText.color = ThemeColorsManager.Instance.babaGreyTextColor;
        } else {
            Debug.Log($"Theme not supported {ThemeManager.CurrentTheme}", this);
        }
    }
    
}
