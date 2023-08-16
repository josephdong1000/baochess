using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
// using Movebox_Scripts;
using TMPro;
using UnityEngine;

public class MoveboxScript : MonoBehaviour {
    public float edgeLength;

    // public float colorFadeSpeed;
    // public float colorFadeDeltaTime;
    public float moveSpeed;
    public float moveDeltaTime;
    public float moveDistance;
    public float hideDistance;
    public float hideXThreshold;
    public int moveboxHeightSize;
    // public MoveboxManager moveboxManager;


    private Vector3 _initialPosition;
    private bool _hovered;
    private bool _hide;
    private bool _delete;
    private bool _update;
    private TextMeshPro _textMeshPro;
    private BoardScript _boardScript;
    private float _maxYPosition;
    private float _minYPosition;

    public int ItemNumber { get; set; }
    public string MoveName { get; set; }
    public float MoveboxY { get; set; }
    public static int HoveredItem;
    public static int SelectedItem;
    private static readonly float _hoverAnimationEpsilon = 0.01f;
    
    // Start is called before the first frame update
    void Start() {
        _hovered = false;
        _hide = false;
        _delete = false;
        _update = true;
        HoveredItem = -1;
        SelectedItem = -1;
        _initialPosition = transform.position;
        transform.position = new Vector3(_initialPosition.x - hideDistance,
                                         _initialPosition.y,
                                         _initialPosition.z);
        _textMeshPro = transform.GetChild(0).gameObject.GetComponent<TextMeshPro>();
        _boardScript = GameObject.FindGameObjectWithTag("Board").GetComponent<BoardScript>();

        _maxYPosition = _boardScript.moveboxInitialPosition.y;
        _minYPosition = Camera.main.ScreenToWorldPoint(new Vector3(0, Screen.height, 0)
                                                       - Camera.main.WorldToScreenPoint(
                                                           _boardScript.moveboxInitialPosition)).y;

        // Debug.Log(_maxYPosition - _minYPosition);
        // Debug.Log(_maxYPosition + " is maxy");
        // Debug.Log(_minYPosition + " is miny");
    }

    // Update is called once per frame
    void LateUpdate() {
        if (_update) {
            StartCoroutine(ShowMoves());
        }

        _update = false;
    }

    private void OnMouseOver() {
        if (SelectedItem == -1) {
            HoveredItem = ItemNumber;
        }

        _hovered = true;
    }

    private void OnMouseExit() {
        if (SelectedItem == -1) {
            HoveredItem = -1;
        }

        _hovered = false;
    }

    private void OnMouseDown() {
        Debug.Log(ItemNumber);
        SelectedItem = ItemNumber;
    }

    IEnumerator ShowMoves() {
        yield return new WaitUntil(() => MoveName is not null &&
                                         MoveBoxTextScript.MoveDescriptionsPretty.ContainsKey(MoveName));

        DisplayMoveName(MoveName);

        float maxDistance = _initialPosition.x + moveDistance / 2;
        float minDistance = _initialPosition.x - moveDistance / 2;
        float hideDistanceAbsolute = _initialPosition.x - hideDistance;

        while (!_hide) {
            if (HoveredItem == -1) {
                EaseXPosition(_initialPosition.x);
            } else if (HoveredItem != ItemNumber) {
                EaseXPosition(minDistance);
            } else {
                EaseXPosition(maxDistance);
            }

            if (_boardScript.MoveboxesHeight * edgeLength > (_maxYPosition - _minYPosition)) {
                EaseYPosition(MoveBoxScrollScript.ScrollPercent);
            }

            yield return new WaitForSeconds(moveDeltaTime);
        }
        
        // while (Mathf.Abs(hideDistanceAbsolute - transform.position.x) > hideDistanceEpsilon) {
        while (transform.position.x > ScreenPositions.Left + hideXThreshold) {
            EaseXPosition(hideDistanceAbsolute);
            yield return new WaitForSeconds(moveDeltaTime);
        }

        // Debug.Log("ready to delete");
        yield return new WaitUntil(() => _delete);

        // Debug.Log("deleting");
        Destroy(gameObject);
    }

    public void FlagEaseOut() {
        _hide = true;
    }

    public void FlagDelete() {
        _delete = true;
    }

    public void EaseXPosition(float xPosition) {
        if (Mathf.Abs(xPosition - transform.position.x) < _hoverAnimationEpsilon) {
            return;
        }

        transform.position = new Vector3(
            Mathf.Lerp(xPosition, transform.position.x, Mathf.Pow(1f - moveSpeed, moveDeltaTime)),
            transform.position.y, _initialPosition.z);
    }

    public void EaseYPosition(float scrollPercent) {
        transform.position = new Vector3(transform.position.x,
                                         _initialPosition.y +
                                         Mathf.Lerp(
                                             0,
                                             _boardScript.MoveboxesHeight * edgeLength -
                                             (_maxYPosition - _minYPosition),
                                             scrollPercent),
                                         _initialPosition.z);
    }

    public void DisplayMoveName(string moveName) {
        string spacedName = string.Join(" ", Regex.Split(moveName, @"(?<!^)(?=[A-Z])"));
        spacedName = spacedName.ToUpperInvariant();

        _textMeshPro.text = $"<align=\"center\"><b>{spacedName}</b></align>\n";
        _textMeshPro.text +=
            $"<size=2px>{MoveBoxTextScript.MoveDescriptionsPretty[moveName]}";

        // Debug.Log(string.Join("\", \"\"},\n {\"", MoveList.AllMoveNames));


    }
    
}