using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class SoundManagerScript : MonoBehaviour {

    public static float VolumeLevel { get; private set; }
    private static AudioSource _audioSource;
    private static AudioClip _audioClip;

    private void Awake() {
        _audioSource = GetComponent<AudioSource>();
        SetVolumeLevel(0.5f);
    }

    public void SetVolumeLevel(float volume) {
        VolumeLevel = volume;
    }

    public static void PlaySound(SoundEffect soundEffect) {
        
        switch (ThemeManager.CurrentTheme) {
            case ThemeManager.Theme.Classic:
                switch (soundEffect) {
                    case SoundEffect.None:
                        break;
                    case SoundEffect.Move:
                        _audioClip = ThemeSoundsManager.Instance.classicMoveSound;
                        break;
                    case SoundEffect.Capture:
                        _audioClip = ThemeSoundsManager.Instance.classicCaptureSound;
                        break;
                    case SoundEffect.Castle:
                        _audioClip = ThemeSoundsManager.Instance.classicCastleSound;
                        break;
                    case SoundEffect.Check:
                        _audioClip = ThemeSoundsManager.Instance.classicCheckSound;
                        break;
                    case SoundEffect.Checkmate:
                        _audioClip = ThemeSoundsManager.Instance.classicCheckmateSound;
                        break;
                    case SoundEffect.Promote:
                        _audioClip = ThemeSoundsManager.Instance.classicPromoteSound;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(soundEffect), soundEffect, null);
                }

                break;
            case ThemeManager.Theme.Baba:
                switch (soundEffect) {
                    case SoundEffect.None:
                        break;
                    case SoundEffect.Move:
                        _audioClip = ThemeSoundsManager.Instance.babaMoveSound;
                        break;
                    case SoundEffect.Capture:
                        _audioClip = ThemeSoundsManager.Instance.babaCaptureSound;
                        break;
                    case SoundEffect.Castle:
                        _audioClip = ThemeSoundsManager.Instance.babaCastleSound;
                        break;
                    case SoundEffect.Check:
                        _audioClip = ThemeSoundsManager.Instance.babaCheckSound;
                        break;
                    case SoundEffect.Checkmate:
                        _audioClip = ThemeSoundsManager.Instance.babaCheckmateSound;
                        break;
                    case SoundEffect.Promote:
                        _audioClip = ThemeSoundsManager.Instance.babaPromoteSound;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(soundEffect), soundEffect, null);
                }
                
                break;
            default:
                Debug.LogError($"No sound effect found {ThemeManager.CurrentTheme} in SoundManagerScript");
                break;
        }
        
        _audioSource.PlayOneShot(_audioClip, VolumeLevel);
    }
    

    public enum SoundEffect {
        None = 0,
        Move,
        Capture,
        Castle,
        Check,
        Checkmate,
        Promote
    }
}
