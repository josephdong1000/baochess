using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThemeSoundsManager : MonoBehaviour {
    public static ThemeSoundsManager Instance { get; private set; }

    public AudioClip classicMoveSound;
    public AudioClip classicCaptureSound;
    public AudioClip classicCastleSound;
    public AudioClip classicCheckSound;
    public AudioClip classicCheckmateSound;
    public AudioClip classicPromoteSound;
    
    public AudioClip babaMoveSound;
    public AudioClip babaCaptureSound;
    public AudioClip babaCastleSound;
    public AudioClip babaCheckSound;
    public AudioClip babaCheckmateSound;
    public AudioClip babaPromoteSound;
    
    private void Awake() {
        if (Instance != null) {
            Destroy(this);
            return;
        }

        Instance = this;
    }
    
}