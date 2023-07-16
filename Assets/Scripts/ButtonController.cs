using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonController : MonoBehaviour {

    public static Mode ButtonMode = Mode.None; 

    public enum Mode {
        Play,
        Edit,
        Undo,
        None
    }

    public void SetPlay() {
        ButtonMode = Mode.Play;
    }
    
    public void SetEdit() {
        ButtonMode = Mode.Edit;
    }

    public void SetUndo() {
        ButtonMode = Mode.Undo;
    }
    
    
}
