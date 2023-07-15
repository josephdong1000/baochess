using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EditorButtonManager : MonoBehaviour
{
    public GameObject editorButtonPrefab;
    public Vector3 topLeftPosition;
    public float buttonSpacing;
    
    // private BoardScript _boardScript;
    private static List<GameObject> _editorButtons;
    
    
    // Start is called before the first frame update
    void Start() {
        // _boardScript = GameObject.FindGameObjectWithTag("Board").GetComponent<BoardScript>();
        _editorButtons = new();
    }

    public void InstantiateEditorButtons() {

        int nPieces = BoardScript.TypePieceDict.Count - 1; // Accounts for the empty piece

        for (int i = 0; i < EditorButtonScript.AllPieceTypes.Count; i++) {

            Vector3 position = topLeftPosition +
                               new Vector3((int)(i / nPieces) * buttonSpacing, 
                                           i % nPieces * buttonSpacing * -1, 
                                           0);
            
            _editorButtons.Add(Instantiate(editorButtonPrefab,
                                           position, 
                                           Quaternion.identity));
            _editorButtons.Last().GetComponent<EditorButtonScript>().thisPieceType = EditorButtonScript.AllPieceTypes[i];
        }
    }

    public void ClearAllButtons() {
        for (int i = 0; i < _editorButtons.Count; i++) {
            Destroy(_editorButtons[i]);
        }
        _editorButtons.Clear();
    }
    
    
}
