using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmptyScript : PieceScript
{
    public override PieceType Type => PieceType.Empty;
    
    // Start is called before the first frame update
    new void Start() {
        base.Start();
    }

}
