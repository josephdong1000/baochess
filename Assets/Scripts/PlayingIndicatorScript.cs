using System.Collections;
using System.Collections.Generic;
using Theme_Scripts;
using UnityEngine;

public class PlayingIndicatorScript : MonoBehaviour {
    public float sidePadding;

    private BoardScript _boardScript;
    private SpriteRenderer _spriteRenderer;

    // Start is called before the first frame update
    void Start() {
        _boardScript = GameObject.FindGameObjectWithTag("Board").GetComponent<BoardScript>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        transform.position = new Vector3(ScreenPositions.Right - sidePadding, ScreenPositions.CenterY, 0);
    }

    // Update is called once per frame
    void Update() {
        if (_boardScript.BoardMode == ButtonController.Mode.Edit) {
            _spriteRenderer.color = Color.white;
        } else {
            _spriteRenderer.color = _boardScript.PlayingSide == PieceScript.Side.White
                ? ThemeManager.Instance.GetThemeColor().Item1
                : ThemeManager.Instance.GetThemeColor().Item2;
        }
    }
}