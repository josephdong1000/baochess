using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BoardCoordinateScript : MonoBehaviour {

    public float centerDistance;

    private (int, int) _boardSquarePosition;
    
    public void InitToBoardSquare(GameObject boardSquare, bool fillRow) {
        transform.position = boardSquare.transform.position;
        _boardSquarePosition = boardSquare.GetComponent<SelectScript>().Position;
        if (fillRow) {
            // transform.position += new Vector3(centerDistance, -centerDistance, 0);
            GetComponent<TextMeshPro>().alignment = TextAlignmentOptions.BottomRight;
            // GetComponent<TextMeshPro>().alignment = TextAlignmentOptions.BaselineRight;
            GetComponent<TextMeshPro>().text = ((char)(97 + _boardSquarePosition.Item2)).ToString();
        } else {
            // transform.position += new Vector3(-centerDistance, centerDistance, 0);
            GetComponent<TextMeshPro>().alignment = TextAlignmentOptions.TopLeft;
            GetComponent<TextMeshPro>().text = (_boardSquarePosition.Item1 + 1) + "";
        }
        
    }
    
}
