using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class EditorButtonManager : MonoBehaviour {
    public GameObject editorButtonPrefab;

    // public Vector3 topLeftPosition;
    public float verticalPadding;

    [FormerlySerializedAs("buttonSpacing")]
    public float horizontalPadding;


    // private BoardScript _boardScript;
    private static List<GameObject> _editorButtons;
    // private static Vector3 bottomLeftPosition;


    // Start is called before the first frame update
    void Start() {
        // _boardScript = GameObject.FindGameObjectWithTag("Board").GetComponent<BoardScript>();
        transform.position = ScreenPositions.CenterPosition;
        _editorButtons = new();
    }

    public void InstantiateEditorButtons() {
        int nPieces = BoardScript.TypePieceDict.Count; // 8, including the clear buttons

        for (int i = 0; i < EditorButtonScript.AllPieceTypes.Count; i++) {
            // Vector3 position = topLeftPosition +
            //                    new Vector3((int)(i / nPieces) * horizontalPadding, 
            //                                i % nPieces * horizontalPadding * -1, 
            //                                0);

            // Vector3 position = Vector3.zero;
            // if (i == EditorButtonScript.AllPieceTypes.Count - 1) {
            //     Vector3 = new Vector3(ScreenPositions.CenterX + , ScreenPositions.CenterY)
            // } else {
            //     
            // }

            Vector3 position = new Vector3(
                (float)(ScreenPositions.CenterX + (i % nPieces / (float)(nPieces - 1) - 0.5) * nPieces * horizontalPadding),
                (i / nPieces == 0) ? ScreenPositions.Bottom + verticalPadding : ScreenPositions.Top - verticalPadding,
                0);

            // Vector3 position = ScreenPositions.CenterPosition;

            // Vector3 position = topLeftPosition +
            //                    new Vector3((int)(i / nPieces) * horizontalPadding, 
            //                                i % nPieces * horizontalPadding * -1, 
            //                                0);

            _editorButtons.Add(Instantiate(editorButtonPrefab,
                                           position,
                                           Quaternion.identity));
            _editorButtons.Last().GetComponent<EditorButtonScript>().ThisPieceType =
                EditorButtonScript.AllPieceTypes[i];
        }
    }

    public void ClearAllButtons() {
        for (int i = 0; i < _editorButtons.Count; i++) {
            Destroy(_editorButtons[i]);
        }

        _editorButtons.Clear();
    }
}