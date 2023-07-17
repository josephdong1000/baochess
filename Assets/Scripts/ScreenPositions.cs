using System;
using UnityEngine;

public class ScreenPositions : MonoBehaviour {
    public static Vector3 CenterPosition;
    public static Vector3 TopRightPosition;
    public static Vector3 TopLeftPosition;
    public static Vector3 BottomRightPosition;
    public static Vector3 BottomLeftPosition;

    public static float Top;
    public static float Bottom;
    public static float Left;
    public static float Right;
    public static float CenterX;
    public static float CenterY;

    public void Awake() {
        CenterPosition = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        TopRightPosition = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));
        TopLeftPosition = Camera.main.ScreenToWorldPoint(new Vector3(0, Screen.height, 0));
        BottomRightPosition = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, 0, 0));
        BottomLeftPosition = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0));

        Top = TopRightPosition.y;
        Right = TopRightPosition.x;
        Bottom = BottomLeftPosition.y;
        Left = BottomLeftPosition.x;

        CenterX = CenterPosition.x;
        CenterY = CenterPosition.y;
        
        // Debug.Log(CenterPosition);
        // Debug.Log(TopRightPosition);
    }
}