using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveBoxScrollScript : MonoBehaviour {
    public float yMargin;
    // public float xThreshold;
    public float moveSpeed;
    public float moveDeltaTime;
    public float resetPositionCooldown;

    public static float ScrollPercent;
    private static float _lastMousePercent;
    private static float _timeOfLastHover;
    private float _xThresholdScreenProportion;

    // Start is called before the first frame update
    void Start() {
        ScrollPercent = 0;
        // _xThreshold = transform.position.x;
        _xThresholdScreenProportion = Camera.main.WorldToScreenPoint(transform.position).x / Screen.width;
        StartCoroutine(UpdateScrollPercent());
        // StartCoroutine(CheckMouseInRegion());
    }

    // Update is called once per frame
    void Update() {
        if (Input.mousePosition.x / Screen.width < _xThresholdScreenProportion) {
            float lastMouseY = Mathf.Clamp(Input.mousePosition.y, yMargin, Screen.height - yMargin);
            _lastMousePercent = 1 - (lastMouseY - yMargin) / (Screen.height - 2 * yMargin);
            _timeOfLastHover = Time.time;
        } else if (_timeOfLastHover < Time.time - resetPositionCooldown) {
            _lastMousePercent = 0;
        }
    }

    IEnumerator UpdateScrollPercent() {
        while (true) {
            ScrollPercent = Mathf.Lerp(_lastMousePercent, ScrollPercent, Mathf.Pow( 1f - moveSpeed, moveDeltaTime));
            yield return new WaitForSeconds(moveDeltaTime);
        }
        yield break;
    }

    // IEnumerator CheckMouseInRegion() {
    //     while (true) {
    //         
    //     }
    // }

    // IEnumerator OnMouseOver() {
    //     // float lastMouseY = Mathf.Clamp(Input.mousePosition.y, yMargin, Screen.height - yMargin);
    //     // _lastMousePercent = 1 - (lastMouseY - yMargin) / (Screen.height - 2 * yMargin);
    //     // Debug.Log("mouseOver percent" + _lastMousePercent);
    //     // _timeOfLastHover = Time.time;
    //     yield return new WaitForSeconds(moveDeltaTime);
    // }
}