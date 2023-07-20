using System;
using UnityEngine;

public class ThemeManager : MonoBehaviour {

    public static Theme CurrentTheme { get; set; }

    public enum Theme {
        Classic,
        Baba,
        None,
    }

    public void Start() {
        CurrentTheme = Theme.Baba;
    }


    public static void SetClassic() {
        CurrentTheme = Theme.Classic;
    }

    public static void SetBaba() {
        CurrentTheme = Theme.Baba;
    }
        
}