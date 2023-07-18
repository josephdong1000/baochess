using System;
using UnityEngine;

public class CameraManager : MonoBehaviour {
    public Color classicBackgroundColor;
    public Color babaBackgroundColor;

    private ThemeManager.Theme _myTheme;

    private void Start() {
        _myTheme = ThemeManager.Theme.Baba;
    }

    public void LateUpdate() {
        if (ThemeManager.CurrentTheme == ThemeManager.Theme.Classic &&
            _myTheme != ThemeManager.Theme.Classic) {
            Camera.main.backgroundColor = classicBackgroundColor;
            _myTheme = ThemeManager.Theme.Classic;
        } else if (ThemeManager.CurrentTheme == ThemeManager.Theme.Baba &&
                   _myTheme != ThemeManager.Theme.Baba) {
            Camera.main.backgroundColor = babaBackgroundColor;
            _myTheme = ThemeManager.Theme.Baba;
        }
    }
}