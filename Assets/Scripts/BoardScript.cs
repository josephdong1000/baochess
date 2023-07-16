using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class BoardScript : MonoBehaviour {
    public int boardSize;
    public GameObject pawnPrefab; // these probably should not be broadly GameObject but something specific
    public GameObject rookPrefab;
    public GameObject knightPrefab;
    public GameObject bishopPrefab;
    public GameObject queenPrefab;
    public GameObject kingPrefab;
    public GameObject footmenPrefab;
    public GameObject emptySquare;
    public GameObject boardSquare;
    public GameObject circleHighlightPrefab;
    public GameObject movebox2Prefab;
    public GameObject movebox3Prefab;
    public Vector3 moveboxInitialPosition;
    public GameObject replaceBoxPrefab;
    public int edgeLength;
    public Color whiteColor;
    public Color blackColor;
    public float selectWaitTime;

    public Dictionary<(PieceScript.PieceType, PieceScript.Side), int> PieceCount { get; private set; }
    public Dictionary<(PieceScript.PieceType, PieceScript.Side), List<GameObject>> PieceReferences { get; private set; }
    public Dictionary<List<((int, int), (int, int))>, string> MultiplePiecePositions { get; private set; }
    public PieceScript.Side PlayingSide { get; private set; }

    /// <summary>
    /// Piece position, piece move, piece attack site (usually the same as move).
    /// As a Lookup, maps each position to an IEnumerable
    /// Lookup by piece position (Item1 of tuple)
    /// </summary>
    public ILookup<(int, int), ((int, int), (int, int), (int, int), string)> FinalSingleMoves { get; private set; }

    /// <summary>
    /// 2 piece positions, 2 piece moves
    /// As a lookup, maps each position pair to an IEnumerable
    /// Lookup by piece initial positions (Item1 of tuple)
    /// </summary>
    public ILookup<(int, int), (( (int, int), (int, int) ), ( (int, int), (int, int) ), string)>
        FinalMultipleMoves { get; private set; }

    /// <summary>
    /// Position A is protected against position B's move string
    /// </summary>
    public List<((int, int), (int, int), string)> FinalProtectedPositions { get; private set; }


    public int
        GameTurnCounter { get; private set; } // Only increment when black finishes their turn (so 1 after 2 moves)

    public List<(int, int)> SelectedPositions;

    /// <summary>
    /// HighlightedPositions is robust against quick changes
    /// </summary>
    public List<(int, int)> HighlightedPositions;

    public GameObject[,] HighlightBoard { get; private set; }
    public List<(int, int)> MultiplePairPositions;
    public List<(int, int)> AttackPositions;
    public List<(int, int)> RangedPositions;
    public Dictionary<string, List<string>> MoveNames;
    public ((int, int), (int, int), (int, int), string) selectedSingleMove;
    public (((int, int), (int, int)), ((int, int), (int, int)), string) selectedMultipleMove;

    public List<GameObject> DeleteList { get; private set; }
    public List<GameObject> CircleShieldList { get; private set; }
    public List<GameObject> MoveboxList { get; private set; }
    public List<GameObject> ReplaceBoxList { get; private set; }
    public void AddDeleteList(GameObject go) => DeleteList.Add(go);
    public float MoveboxesHeight { get; private set; }
    public static bool SelectingMove;
    private string _selectedMoveName;
    private Dictionary<PieceScript.Side, int> _extraMoves;
    private Dictionary<(string, PieceScript.Side), int> _bannedMoves;

    private GameObject[,] _board;
    // private List<bool[]> _boardStates;
    private ButtonController.Mode _boardMode = ButtonController.Mode.None;

    private Dictionary<char, GameObject> _charPieceDict;
    private Dictionary<PieceScript.PieceType, char> _typeCharDict;
    public static Dictionary<PieceScript.PieceType, GameObject> TypePieceDict;

    private bool _editingBoard;


    public char[,] BoardTemplateReverse = {
        // Upper case = white, lower case = black
        { 'r', 'n', 'b', 'q', 'k', 'b', 'n', 'r' },
        { 'p', 'p', 'p', 'p', 'p', 'p', 'p', 'p' },
        { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
        { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
        { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
        { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
        { 'P', 'P', 'P', 'P', 'P', 'P', 'P', 'P' },
        { 'R', 'N', 'B', 'Q', 'K', 'B', 'N', 'R' }

        // { ' ', 'p', 'F', 'r', ' ', ' ', 'N', 'B' },
        // { 'R', ' ', ' ', ' ', ' ', ' ', ' ', 'R' },
        // { ' ', 'f', ' ', 'p', ' ', ' ', ' ', ' ' },
        // { ' ', ' ', 'F', ' ', ' ', ' ', ' ', ' ' },
        // { ' ', ' ', ' ', 'r', 'N', 'P', ' ', ' ' },
        // { ' ', ' ', ' ', ' ', ' ', 'B', 'B', ' ' },
        // { ' ', ' ', 'Q', ' ', ' ', 'P', 'B', ' ' },
        // { ' ', ' ', ' ', 'K', 'K', ' ', ' ', ' ' },
        //
        // { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
        // { ' ', ' ', ' ', 'P', ' ', ' ', ' ', ' ' },
        // { ' ', ' ', ' ', ' ', 'P', 'P', ' ', ' ' },
        // { ' ', ' ', ' ', 'Q', 'N', 'p', ' ', ' ' },
        // { ' ', ' ', ' ', 'b', 'p', 'P', 'p', ' ' },
        // { ' ', ' ', ' ', ' ', ' ', 'b', 'P', ' ' },
        // { ' ', 'p', ' ', 'Q', 'K', 'R', ' ', ' ' },
        // { 'R', ' ', ' ', ' ', 'K', ' ', ' ', 'R' },

        // { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
        // { ' ', ' ', ' ', 'q', 'r', ' ', ' ', ' ' },
        // { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
        // { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
        // { ' ', ' ', ' ', ' ', ' ', 'N', ' ', ' ' },
        // { ' ', ' ', ' ', 'K', 'K', 'P', ' ', ' ' },
        // { ' ', ' ', ' ', ' ', ' ', 'P', ' ', ' ' },
        // { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
    };


    private char[,] _boardTemplate;

    private bool _exitUpdate;
    private IEnumerator _updateGameLoop;
    private IEnumerator _checkResetSelection;

    // Hacky config solution
    // public static BoardData BoardData { get; set; }

    private void Awake() {
        // Vertically flip the template board
        UpdateBoardTemplate();

        PieceCount = new Dictionary<(PieceScript.PieceType, PieceScript.Side), int>();
        PieceReferences = new Dictionary<(PieceScript.PieceType, PieceScript.Side), List<GameObject>>();
        MultiplePiecePositions = new();
        PlayingSide = PieceScript.Side.White; // White playing first
        GameTurnCounter = 0;
        DeleteList = new();
        MoveboxList = new();
        ReplaceBoxList = new();
        CircleShieldList = new();
        SelectedPositions = new List<(int, int)>();
        HighlightedPositions = new List<(int, int)>();
        HighlightBoard = new GameObject[boardSize, boardSize];
        MultiplePairPositions = new List<(int, int)>();
        AttackPositions = new List<(int, int)>();
        RangedPositions = new List<(int, int)>();
        FinalProtectedPositions = new();
        MoveNames = new();
        SelectingMove = false;
        _extraMoves = new() {
            { PieceScript.Side.White, 0 },
            { PieceScript.Side.Black, 0 }
        };
        _bannedMoves = MoveList.AllMoveNames
            .ToDictionary(s => (s, PieceScript.Side.White), _ => 0)
            .Concat(MoveList.AllMoveNames.ToDictionary(s => (s, PieceScript.Side.Black), _ => 0))
            .ToDictionary(e => e.Key, e => e.Value);
        _editingBoard = false;
        // _boardStates = new();
        _charPieceDict = new Dictionary<char, GameObject>() {
            { ' ', null },
            { 'p', pawnPrefab },
            { 'r', rookPrefab },
            { 'n', knightPrefab },
            { 'b', bishopPrefab },
            { 'q', queenPrefab },
            { 'k', kingPrefab },
            { 'f', footmenPrefab } // with an E!
        };
        _typeCharDict = new() {
            { PieceScript.PieceType.Empty, ' ' },
            { PieceScript.PieceType.Pawn, 'p' },
            { PieceScript.PieceType.Rook, 'r' },
            { PieceScript.PieceType.Knight, 'n' },
            { PieceScript.PieceType.Bishop, 'b' },
            { PieceScript.PieceType.Queen, 'q' },
            { PieceScript.PieceType.King, 'k' },
            { PieceScript.PieceType.Footmen, 'f' }
        };
        TypePieceDict = new Dictionary<PieceScript.PieceType, GameObject> {
            { PieceScript.PieceType.Empty, emptySquare },
            { PieceScript.PieceType.Pawn, pawnPrefab },
            { PieceScript.PieceType.Rook, rookPrefab },
            { PieceScript.PieceType.Knight, knightPrefab },
            { PieceScript.PieceType.Bishop, bishopPrefab },
            { PieceScript.PieceType.Queen, queenPrefab },
            { PieceScript.PieceType.King, kingPrefab },
            { PieceScript.PieceType.Footmen, footmenPrefab }
        };
    }

    // Start is called before the first frame update
    void Start() {
        _exitUpdate = false;
        StartCoroutine(ToggleButtonModes());
    }

    // Update is called once per frame
    void Update() {
        if (_exitUpdate) {
            return;
        }

        _exitUpdate = true;
        
        _board ??= new GameObject[boardSize, boardSize];
        HighlightBoard ??= new GameObject[boardSize, boardSize];

        ClearBoard();
        PopulateBoard();
        BoardStateScript.StoreBoardState(_board, PlayingSide);
        InstantiateHighlightBoard(); // Cosmetic
        UpdatePieceGameObjectPositions(); // Cosmetic


        _updateGameLoop = UpdateGameLoop();
        StartCoroutine(_updateGameLoop);
        _checkResetSelection = CheckResetSelection(KeyCode.R);
        StartCoroutine(_checkResetSelection);


        // _board = BoardStateScript.BoolsToBoard(BoardStateScript.BoardToBools(_board, PlayingSide)).Item1;
        // PrintBoard();
    }

    IEnumerator UpdateGameLoop() {
        SelectingMove = false;

        FetchMoveNames();

        // PrintBoard();

        Debug.Log("update game loop called");

        bool loss = false;
        while (!loss) {
            UpdateAllSpriteSides();
            UpdatePieceCountReferences();

            // Evaluate all move functions
            ClearAttackProtectLists();
            UpdateAttackLists();
            UpdateMultipleCheckers();
            UpdatePassiveReplaceFunctions();
            UpdateProtectList();
            FinalProtectedPositions.Clear();
            GenerateSingleMoves();
            GenerateMultipleMoves();

            // Await player input
            SelectingMove = true;
            yield return SelectMove();
            SelectingMove = false;

            yield return MovePieces();

            // Final checks
            UpdateBannedTurns();
            CheckExtraMove();
            UpdateAutomaticMoves();
            
            // Update visuals and piece data
            UpdatePiecePositions();
            UpdatePieceProperties(PlayingSide);
            DeletePieces();
            UpdatePieceGameObjectPositions();

            
            if (_extraMoves[PlayingSide] > 0) {
                _extraMoves[PlayingSide] -= 1;
            } else {
                SwitchPlayers();
                BoardStateScript.StoreBoardState(_board, PlayingSide); // Update undo move stack
            }

            // PrintBoard(showAttack: true, showProtect: true);
        }

        Debug.Log("Yippee");
    }

    public void UpdateBoardTemplate() {
        _boardTemplate = new char[boardSize, boardSize];
        for (int i = 0; i < boardSize; i++) {
            for (int j = 0; j < boardSize; j++) {
                _boardTemplate[boardSize - i - 1, j] = BoardTemplateReverse[i, j];
            }
        }
    }


    /// <summary>
    /// Resets both the main board and padded board arrays. Destroys all objects on both boards
    /// </summary>
    public void ClearBoard(bool board = true, bool highlightBoard = true) {
        for (int i = 0; i < boardSize; i++) {
            for (int j = 0; j < boardSize; j++) {
                if (_board[i, j] != null && board) {
                    Destroy(_board[i, j]);
                }

                if (HighlightBoard[i, j] != null && highlightBoard) {
                    Destroy(HighlightBoard[i, j]);
                }
            }
        }

        if (board) {
            _board = new GameObject[boardSize, boardSize];
        }

        if (highlightBoard) {
            HighlightBoard = new GameObject[boardSize, boardSize];
        }
    }

    /// <summary>
    /// Populates the main board according to _boardTemplate
    /// </summary>
    public void PopulateBoard() {
        for (int i = 0; i < boardSize; i++) {
            for (int j = 0; j < boardSize; j++) {
                if (_charPieceDict[char.ToLowerInvariant(_boardTemplate[i, j])] is not null) {
                    // Create piece based on _boardTemplate
                    _board[i, j] = Instantiate<GameObject>(_charPieceDict[char.ToLowerInvariant(_boardTemplate[i, j])],
                                                           PositionToVector3((i, j)),
                                                           quaternion.identity);
                    // Set piece to white/black
                    if (char.IsUpper(_boardTemplate[i, j])) {
                        _board[i, j].GetComponent<PieceScript>().PieceSide = PieceScript.Side.White;
                    } else {
                        _board[i, j].GetComponent<PieceScript>().PieceSide = PieceScript.Side.Black;
                    }

                    // Set piece position
                    _board[i, j].GetComponent<PieceScript>().Position = (i, j);
                } else {
                    _board[i, j] = Instantiate(emptySquare, PositionToVector3((i, j)),
                                               quaternion.identity);

                    _board[i, j].GetComponent<PieceScript>().PieceSide = PieceScript.Side.None;
                    // _board[i, j].SetActive(false); // Hide empty GameObjects, not necessary to show
                }
            }
        }
    }

    public void PopulateBoardEmpty() {
        for (int i = 0; i < boardSize; i++) {
            for (int j = 0; j < boardSize; j++) {
                _board[i, j] = Instantiate(emptySquare, PositionToVector3((i, j)),
                                           quaternion.identity);
                _board[i, j].GetComponent<PieceScript>().PieceSide = PieceScript.Side.None;
                _board[i, j].SetActive(false); // Hide empty GameObjects, not necessary to show
            }
        }
    }

    public void InstantiateHighlightBoard() {
        for (int i = 0; i < boardSize; i++) {
            for (int j = 0; j < boardSize; j++) {
                // Instantiate the board beneath the pieces
                HighlightBoard[i, j] = Instantiate(boardSquare,
                                                   PositionToVector3((i, j)),
                                                   quaternion.identity);
                HighlightBoard[i, j].GetComponent<SelectScript>().Position = (i, j);
            }
        }
    }

    public void UpdatePieceGameObjectPositions() {
        for (int i = 0; i < boardSize; i++) {
            for (int j = 0; j < boardSize; j++) {
                if (!IsEmptySquare((i, j))) {
                    GetPosition((i, j)).transform.position = PositionToVector3((i, j));
                    GetPosition((i, j)).transform.rotation = quaternion.identity;
                } else {
                    GetPosition((i, j)).SetActive(false); // Hide empty GameObjects, not necessary to show    
                }

                HighlightBoard[i, j].transform.position = PositionToVector3((i, j));
                HighlightBoard[i, j].transform.rotation = quaternion.identity;
            }
        }
    }

    public Vector3 PositionToVector3((int, int) position) {
        return new Vector3(position.Item2 * edgeLength, position.Item1 * edgeLength);
    }

    public Dictionary<string, List<string>> FetchMoveNames() {
        MoveNames.Clear();

        List<string> functionCategoryList = new() {
            "MoveFunctions",
            "CaptureFunctions",
            "SelfCaptureFunctions",
            "ProtectFunctions",
            "RangedFunctions",
            "AttackReplaceFunctions",
            "PassiveReplaceFunctions",
            "MultipleFunctions"
        };

        foreach (string s in functionCategoryList) {
            MoveNames[s] = new();
        }

        foreach (GameObject piecePrefab in _charPieceDict.Values) {
            if (piecePrefab is null) {
                continue;
            }

            GameObject transientPiece = Instantiate(piecePrefab);
            PieceScript ps = transientPiece.GetComponent<PieceScript>();
            // ps.Start();
            // PieceScript ps = new piecePrefab().GetComponent<PieceScript>();


            // This is so hardcoded lmao
            foreach (var moveFunction in ps.MoveFunctions) {
                MoveNames["MoveFunctions"].Add(moveFunction.Method.Name);
            }

            foreach (var captureFunctions in ps.CaptureFunctions) {
                MoveNames["CaptureFunctions"].Add(captureFunctions.Method.Name);
            }

            foreach (var captureFunctions in ps.SelfCaptureFunctions) {
                MoveNames["SelfCaptureFunctions"].Add(captureFunctions.Method.Name);
            }

            foreach (var protectFunction in ps.ProtectFunctions) {
                MoveNames["ProtectFunctions"].Add(protectFunction.Method.Name);
            }

            foreach (var rangedFunction in ps.RangedFunctions) {
                MoveNames["RangedFunctions"].Add(rangedFunction.Method.Name);
            }

            foreach (var attackReplaceFunction in ps.AttackReplaceFunctions) {
                MoveNames["AttackReplaceFunctions"].Add(attackReplaceFunction.Method.Name);
            }

            foreach (var passiveReplaceFunction in ps.PassiveReplaceFunctions) {
                MoveNames["PassiveReplaceFunctions"].Add(passiveReplaceFunction.Method.Name);
            }

            Destroy(transientPiece);
        }

        foreach (var multipleFunction in MultipleChecker.PieceIdentity.Keys) {
            MoveNames["MultipleFunctions"].Add(multipleFunction.Method.Name);
        }

        for (int i = 0; i < MoveNames.Count; i++) {
            var item = MoveNames.ElementAt(i);

            MoveNames[item.Key] = item.Value.Distinct().ToList();
        }

        // foreach (var (key, value) in MoveNames) {
        //     Debug.Log(key);
        //     foreach (string movename in value) {
        //         Debug.Log("\t" + movename);
        //     }
        // }

        return MoveNames;
    }

    IEnumerator CheckResetSelection(KeyCode keyCode) {
        while (true) {
            yield return new WaitUntil(() => Input.GetKeyDown(keyCode));
            if (SelectingMove) {
                SelectingMove = false;
                yield return ResetGameLoop();
            }

            yield return new WaitForSeconds(0.05f); // Hardcoded cooldown
        }
    }

    // WIP
    IEnumerator ToggleButtonModes() {
        IEnumerator editBoardLoop = EditBoardLoop();
        
        while (true) {
            
            yield return new WaitUntil(() => ButtonController.ButtonMode != ButtonController.Mode.None);
            _boardMode = ButtonController.ButtonMode;
            ButtonController.ButtonMode = ButtonController.Mode.None;
            
            
            // yield return new WaitUntil(() => Input.GetKeyDown(keyCode));
            _editingBoard = !_editingBoard;
            if (_editingBoard) {
                StopCoroutine(_checkResetSelection);
                StopCoroutine(_updateGameLoop);
                StopCoroutine(nameof(UpdateGameLoop));
                StopCoroutine(nameof(SelectMove));
                StopCoroutine(nameof(MovePieces));
                yield return ClearAllVisualsAndWait();
                
                Debug.Log("wiping board and preparing edit");

                // Wipe board
                ClearBoard(board: false, highlightBoard: true);
                // PopulateBoardEmpty();
                InstantiateHighlightBoard();
                UpdatePieceGameObjectPositions();

                // Instantiate buttons
                GetComponent<EditorButtonManager>().InstantiateEditorButtons();

                Debug.Log("ready to edit");

                StartCoroutine(editBoardLoop);
            } else { // Done editing board

                Debug.Log("exiting board edit");

                StopCoroutine(editBoardLoop);
                editBoardLoop = EditBoardLoop();

                // Erase buttons
                GetComponent<EditorButtonManager>().ClearAllButtons();
                // Consolidate board edits into char dict
                for (int i = 0; i < boardSize; i++) {
                    for (int j = 0; j < boardSize; j++) {
                        char finalPieceChar = _typeCharDict[GetPieceType((i, j))];
                        _boardTemplate[i, j] = GetPieceSide((i, j)) == PieceScript.Side.White
                            ? Char.ToUpperInvariant(finalPieceChar)
                            : finalPieceChar;
                    }
                }

                ClearBoard();
                PopulateBoard();
                InstantiateHighlightBoard();
                UpdatePieceGameObjectPositions();

                // Reset extraMoves and bannedMoves
                _extraMoves = new() {
                    { PieceScript.Side.White, 0 },
                    { PieceScript.Side.Black, 0 }
                };
                for (int i = 0; i < _bannedMoves.Count; i++) {
                    _bannedMoves[_bannedMoves.ElementAt(i).Key] = 0;
                }

                _checkResetSelection = CheckResetSelection(KeyCode.R);
                StartCoroutine(_checkResetSelection);

                Debug.Log("returning to main update loop");

                // Clear visuals and reset game loop
                yield return StartCoroutine(ResetGameLoop());

                Debug.Log("done resetting gameloop");

                yield return new WaitForSeconds(0.05f); // Hardcoded cooldown
            }

            // yield return new WaitUntil(() => Input.GetKeyUp(keyCode));
        }
    }

    IEnumerator EditBoardLoop() {
        while (true) {
            yield return new WaitUntil(() => SelectedPositions.Count > 0);

            Debug.Log("\tprocessing a selected position");

            (int, int) addPosition = SelectedPositions.First();
            SelectedPositions.RemoveAt(0);
            // Debug.Log(SelectedPositions.Count);

            DeleteList.Add(GetPosition(addPosition));
            _board[addPosition.Item1, addPosition.Item2] =
                InstantiatePiece(EditorButtonScript.SelectedPieceType.Item1,
                                 addPosition,
                                 EditorButtonScript.SelectedPieceType.Item2);
            if (GetPieceType(addPosition) == PieceScript.PieceType.Empty) {
                GetPosition(addPosition).SetActive(false);
            }

            DeletePieces();
            UpdateAllSpriteSides();

            // UpdatePieceGameObjectPositions();

            // yield return new WaitForSeconds(0.05f);
        }
    }

    IEnumerator ResetGameLoop() {
        StopCoroutine(_updateGameLoop);
        StopCoroutine(nameof(UpdateGameLoop));

        yield return ClearAllVisualsAndWait();

        // Instantiate new GameLoop
        _updateGameLoop = UpdateGameLoop();

        StartCoroutine(_updateGameLoop);
    }

    /// <summary>
    /// Resets PieceCount and counts up all of the pieces.
    /// </summary>
    private void UpdatePieceCountReferences() {
        ClearPieceCountReferences();
        for (int i = 0; i < boardSize; i++) {
            for (int j = 0; j < boardSize; j++) {
                (int, int) piecePos = (i, j);
                if (!IsEmptySquare(piecePos)) {
                    PieceCount[(GetPieceType(piecePos), GetPieceSide(piecePos))] += 1;
                    PieceReferences[(GetPieceType(piecePos), GetPieceSide(piecePos))].Add(GetPosition(piecePos));
                }
            }
        }
    }

    /// <summary>
    /// Resets PieceCount to all 0s for all pieces
    /// </summary>
    private void ClearPieceCountReferences() {
        PieceCount.Clear();
        PieceReferences.Clear();
        foreach (PieceScript.PieceType i in Enum.GetValues(typeof(PieceScript.PieceType))) {
            foreach (PieceScript.Side j in Enum.GetValues(typeof(PieceScript.Side))) {
                PieceCount.Add((i, j), 0);
                PieceReferences.Add((i, j), new List<GameObject>());
            }
        }
    }

    private void ClearAttackProtectLists() {
        (int, int) position;
        for (int i = 0; i < boardSize; i++) {
            for (int j = 0; j < boardSize; j++) {
                position = (i, j);
                if (!IsEmptySquare(position)) {
                    PieceScript positionPieceScript = GetPosition(position).GetComponent<PieceScript>();
                    positionPieceScript.AttackedBy.Clear();
                    positionPieceScript.ProtectedBy.Clear();
                }
            }
        }
    }

    /// <summary>
    /// <para>Updates all pieces' AttackList, which lists what other GameObjects attack this piece and the attacking move's name.</para>
    /// <para>Must capture a piece.</para>
    /// </summary>
    private void UpdateAttackLists() {
        (int, int) piecePosition;
        (int, int) targetPosition;
        bool capturable;
        (bool, List<(int, int)>) rangedable;
        for (int i = 0; i < boardSize; i++) {
            for (int j = 0; j < boardSize; j++) { // first coordinate

                piecePosition = (i, j);

                // If attacker is not an empty square
                if (!IsEmptySquare(piecePosition) &&
                    GetPosition(piecePosition).GetComponent<PieceScript>().CaptureFunctions !=
                    default(List<Func<(int, int), bool>>)) { // Not uninitialized. REMOVE LATER 

                    // Get all possible capturing and ranged moves
                    GameObject pieceGameObject = GetPosition(piecePosition);
                    List<Func<(int, int), bool>> captureFunctions =
                        pieceGameObject.GetComponent<PieceScript>().CaptureFunctions;
                    // List<Func<(int, int), bool>> selfCaptureFunctions =
                    //     pieceGameObject.GetComponent<PieceScript>().SelfCaptureFunctions;
                    List<Func<(int, int), (bool, List<(int, int)>)>> rangedFunctions =
                        pieceGameObject.GetComponent<PieceScript>().RangedFunctions;
                    List<Func<(int, int), (bool, Dictionary<(int, int), List<PieceScript.PieceType>>)>>
                        replaceFunctions =
                            pieceGameObject.GetComponent<PieceScript>().AttackReplaceFunctions;

                    // Debug.Log($"Evaling attacker {piecePosition}");

                    for (int k = 0; k < boardSize; k++) {
                        for (int l = 0; l < boardSize; l++) { // second coordinate
                            targetPosition = (k, l);

                            // Try Attack functions
                            if (!IsEmptySquare(targetPosition)) { // Target square is not empty
                                // Debug.Log($"\tEvaling attack target {targetPosition}");
                                if (IsEnemy(piecePosition, GetPieceSide(targetPosition))) { // Target square is an enemy
                                    // Iterate over all capturing moves
                                    foreach (Func<(int, int), bool> func in captureFunctions) {
                                        if (InBanList(func.Method.Name)) {
                                            continue;
                                        }

                                        capturable = func(GetMoveFromPositions(piecePosition, targetPosition));
                                        if (capturable) {
                                            GetPosition(targetPosition).GetComponent<PieceScript>()
                                                .AttackListAdd(pieceGameObject, func.Method.Name);
                                        }
                                    }
                                }
                                // } else { // Target not enemy NOT SURE IF THIS COUNTS AS AN ATTACK
                                // foreach (Func<(int, int), bool> func in selfCaptureFunctions) {
                                //     capturable = func(GetMoveFromPositions(piecePosition, targetPosition));
                                //     if (capturable) {
                                //         GetPosition(targetPosition).GetComponent<PieceScript>()
                                //             .AttackListAdd(pieceGameObject, func.Method.Name);
                                //     }
                                // }
                                // }

                                // Then try Ranged functions
                            } else if (IsEmptySquare(targetPosition)) {
                                // Ranged functions do not capture on target square
                                // Iterate over all ranged moves
                                foreach (Func<(int, int), (bool, List<(int, int)>)> func in rangedFunctions) {
                                    if (InBanList(func.Method.Name)) {
                                        continue;
                                    }

                                    rangedable = func(GetMoveFromPositions(piecePosition, targetPosition));
                                    if (rangedable.Item1) {
                                        // Update attackers in all rangedable spots
                                        // Debug.Log($"\tEvaling ranged target {targetPosition}");
                                        foreach ((int, int) rangedablePosition in rangedable.Item2) {
                                            GetPosition(rangedablePosition).GetComponent<PieceScript>()
                                                .AttackListAdd(pieceGameObject, func.Method.Name);
                                        }
                                    }
                                }
                            }

                            // Try Replace functions. Will check all squares, empty/friendly included
                            // Stores only which pieces will attack what
                            foreach (var replaceFunction in replaceFunctions) {
                                if (InBanList(replaceFunction.Method.Name)) {
                                    continue;
                                }

                                var (replaceable, replaceDict) =
                                    replaceFunction(GetMoveFromPositions(piecePosition, targetPosition));
                                if (replaceable) {
                                    // Update attacked pieces with this attacker
                                    // Debug.Log($"\tEvaling replacable target {targetPosition}");
                                    foreach ((int, int) replacePosition in replaceDict.Keys) {
                                        if (!IsEmptySquare(replacePosition) &&
                                            IsEnemy(replacePosition, GetPieceSide(piecePosition))) {
                                            GetPosition(replacePosition).GetComponent<PieceScript>()
                                                .AttackListAdd(pieceGameObject, replaceFunction.Method.Name);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// <para>Updates MultiplePiecePositions, a dictionary containing Lists of 2 position pairs that pieces can move to, mapping to the move name. </para>
    /// <para>May or may not capture pieces.</para>
    /// <para>Checks pairs of non-empty squares that are friendly and match multiplePieceTypes[i]</para>
    /// </summary>
    private void UpdateMultipleCheckers() {
        // Reset MultiplePiecePositions
        MultiplePiecePositions.Clear();

        (int, int) piecePositionA;
        (int, int) piecePositionB;
        var pieceIdentity = MultipleChecker.PieceIdentity;
        foreach (var (multipleFunction,
                     (multiplePieceTypesA,
                     multiplePieceTypesB)) in pieceIdentity) {
            // PieceScript.PieceType multiplePieceTypesA = multiplePieceTypes[0];
            // List<PieceScript.PieceType> multiplePieceTypesB =
            //     multiplePieceTypes.GetRange(1, multiplePieceTypes.Count - 1);

            if (InBanList(multipleFunction.Method.Name)) {
                continue;
            }

            // Iterate over piece A
            for (int i = 0; i < boardSize; i++) {
                for (int j = 0; j < boardSize; j++) {
                    piecePositionA = (i, j);
                    GameObject pieceGameObjectA = GetPosition(piecePositionA);

                    if (!IsEmptySquare(piecePositionA) && // Not empty
                        multiplePieceTypesA.Contains(GetPieceType(piecePositionA)) && // Match piece type 1st
                        GetPosition(piecePositionA).GetComponent<PieceScript>().ProtectFunctions !=
                        default(List<Func<(int, int), (GameObject, string)>>)) { // Not uninitialized. REMOVE LATER

                        // Debug.Log($"Evaling piece A {piecePositionA}");

                        // Iterate over piece B
                        for (int k = 0; k < boardSize; k++) {
                            for (int l = 0; l < boardSize; l++) {
                                piecePositionB = (k, l);
                                GameObject pieceGameObjectB = GetPosition(piecePositionB);

                                if (!IsEmptySquare(piecePositionB) && // Not empty
                                    multiplePieceTypesB.Contains(
                                        GetPieceType(piecePositionB)) && // Match any of piece type 2nd
                                    // GetPieceType(piecePositionB) == multiplePieceTypes[1] && 
                                    !IsEnemy(piecePositionB, GetPieceSide(piecePositionA))) { // Is friendly

                                    // Debug.Log($"\tEvaling piece B {piecePositionB}");

                                    var (isMoveValid, validPositionA, validPositionB) =
                                        multipleFunction(piecePositionA, piecePositionB);

                                    if (isMoveValid) { // # of valid moves > 0
                                        for (int m = 0; m < validPositionA.Count; m++) {
                                            // Iterate over all valid moves (2 pairs of positions)
                                            MultiplePiecePositions.Add(new List<((int, int), (int, int))> {
                                                (piecePositionA, validPositionA[m]), // where piece A goes
                                                (piecePositionB, validPositionB[m]) // where piece B goes
                                            }, multipleFunction.Method.Name);

                                            // Also check if the move captures an enemy piece, in which case update attackList
                                            if (!IsEmptySquare(
                                                    validPositionA[m]) && // if position moving onto is filled
                                                IsEnemy(validPositionA[m], GetPieceSide(piecePositionA))) { // and enemy
                                                GetAttackList(validPositionA[m])
                                                    .Add((pieceGameObjectA, multipleFunction.Method.Name));
                                            }

                                            if (!IsEmptySquare(
                                                    validPositionB[m]) && // if position moving onto is filled
                                                IsEnemy(validPositionB[m], GetPieceSide(piecePositionB))) { // and enemy
                                                GetAttackList(validPositionB[m])
                                                    .Add((pieceGameObjectB, multipleFunction.Method.Name));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private void UpdatePassiveReplaceFunctions() {
        (int, int) piecePosition;
        (int, int) targetPosition;
        for (int i = 0; i < boardSize; i++) {
            for (int j = 0; j < boardSize; j++) { // first coordinate

                piecePosition = (i, j);

                // If piece is not an empty square
                if (!IsEmptySquare(piecePosition) &&
                    GetPosition(piecePosition).GetComponent<PieceScript>().CaptureFunctions !=
                    default(List<Func<(int, int), bool>>)) { // Not uninitialized. REMOVE LATER 

                    // Get all possible capturing and ranged moves
                    GameObject pieceGameObject = GetPosition(piecePosition);
                    List<Func<(int, int), (bool, Dictionary<(int, int), List<PieceScript.PieceType>>)>>
                        replaceFunctions =
                            pieceGameObject.GetComponent<PieceScript>().PassiveReplaceFunctions;

                    // Debug.Log($"Evaling passive replacer {piecePosition}");

                    for (int k = 0; k < boardSize; k++) {
                        for (int l = 0; l < boardSize; l++) { // second coordinate
                            targetPosition = (k, l);

                            // Try Passive Replace functions. Will only log affecting friendly/empty squares
                            foreach (var replaceFunction in replaceFunctions) {
                                if (InBanList(replaceFunction.Method.Name)) {
                                    continue;
                                }

                                var (replaceable, replaceDict) =
                                    replaceFunction(GetMoveFromPositions(piecePosition, targetPosition));
                                if (replaceable) {
                                    // Update attacked pieces with this attacker
                                    // Debug.Log($"\tEvaling replacable target {targetPosition}");
                                    foreach ((int, int) replacePosition in replaceDict.Keys) {
                                        if (IsEmptySquare(replacePosition) ||
                                            !IsEnemy(replacePosition, GetPieceSide(piecePosition))) {
                                            // Update the move options

                                            // NOT IMPLEMENTED YET


                                            // GetPosition(replacePosition).GetComponent<PieceScript>()
                                            //     .AttackListAdd(pieceGameObject, replaceFunction.Method.Name);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private void UpdateProtectList() {
        (int, int) piecePosition;
        (int, int) targetPosition;
        (GameObject, string) protectable;
        for (int i = 0; i < boardSize; i++) {
            for (int j = 0; j < boardSize; j++) { // first coordinate

                piecePosition = (i, j);

                // If defender is not an empty square
                if (!IsEmptySquare(piecePosition) &&
                    GetPosition(piecePosition).GetComponent<PieceScript>().ProtectFunctions !=
                    default(List<Func<(int, int), (GameObject, string)>>)) { // Not uninitialized. REMOVE LATER 

                    // Get all possible protecting moves
                    GameObject pieceGameObject = GetPosition(piecePosition);
                    List<Func<(int, int), (GameObject, string)>> protectFunctions =
                        pieceGameObject.GetComponent<PieceScript>().ProtectFunctions;
                    // Debug.Log($"Evaling defender {piecePosition}");

                    for (int k = 0; k < boardSize; k++) {
                        for (int l = 0; l < boardSize; l++) { // second coordinate
                            targetPosition = (k, l);
                            if (!IsEmptySquare(targetPosition) && // Target square is not empty
                                !IsEnemy(piecePosition, GetPieceSide(targetPosition))) { // Target square is friendly
                                // Debug.Log($"\tEvaling target {targetPosition}");

                                // Iterate over all protective moves
                                foreach (var func in protectFunctions) {
                                    if (InBanList(func.Method.Name)) {
                                        continue;
                                    }

                                    protectable = func(GetMoveFromPositions(piecePosition, targetPosition));
                                    if (!string.IsNullOrEmpty(protectable.Item2)) {
                                        // if (protectable.Item1 is null) {
                                        //     
                                        // }
                                        GetPosition(targetPosition).GetComponent<PieceScript>()
                                            .ProtectedBy.Add((pieceGameObject, protectable.Item1, protectable.Item2));
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void GenerateSingleMoves() {
        // Calculate single moves
        (int, int) piecePosition;
        (int, int) targetPosition;
        bool capturable;
        (bool, List<(int, int)>) rangedable;
        List<((int, int), (int, int), (int, int), string)> singleMoveList = new();
        // List<((int, int), (int, int), (int, int), (int, int), string)> doubleMoveList = new();

        for (int i = 0; i < boardSize; i++) {
            for (int j = 0; j < boardSize; j++) { // first coordinate

                piecePosition = (i, j);

                // If attacker is not an empty square
                if (!IsEmptySquare(piecePosition) &&
                    GetPosition(piecePosition).GetComponent<PieceScript>().CaptureFunctions !=
                    default(List<Func<(int, int), bool>>)) { // Not uninitialized. REMOVE LATER 

                    // Get all possible capturing and ranged moves
                    GameObject pieceGameObject = GetPosition(piecePosition);
                    List<Func<(int, int), bool>> moveFunctions =
                        pieceGameObject.GetComponent<PieceScript>().MoveFunctions
                            .FindAll(f => !InBanList(f.Method.Name));
                    List<Func<(int, int), bool>> captureFunctions =
                        pieceGameObject.GetComponent<PieceScript>().CaptureFunctions
                            .FindAll(f => !InBanList(f.Method.Name));
                    List<Func<(int, int), bool>> selfCaptureFunctions =
                        pieceGameObject.GetComponent<PieceScript>().SelfCaptureFunctions
                            .FindAll(f => !InBanList(f.Method.Name));
                    List<Func<(int, int), (bool, List<(int, int)>)>> rangedFunctions =
                        pieceGameObject.GetComponent<PieceScript>().RangedFunctions
                            .FindAll(f => !InBanList(f.Method.Name));
                    var attackReplaceFunctions = pieceGameObject.GetComponent<PieceScript>().AttackReplaceFunctions
                        .FindAll(f => !InBanList(f.Method.Name));
                    var passiveReplaceFunctions = pieceGameObject.GetComponent<PieceScript>().PassiveReplaceFunctions
                        .FindAll(f => !InBanList(f.Method.Name));

                    for (int k = 0; k < boardSize; k++) {
                        for (int l = 0; l < boardSize; l++) { // second coordinate
                            targetPosition = (k, l);

                            if (!IsEmptySquare(targetPosition)) { // Target occupied

                                // Try Capture functions
                                if (IsEnemy(piecePosition, GetPieceSide(targetPosition))) { // Target square is an enemy

                                    // Iterate over all capturing moves
                                    foreach (Func<(int, int), bool> captureFunction in captureFunctions) {
                                        capturable =
                                            captureFunction(GetMoveFromPositions(piecePosition, targetPosition));
                                        if (capturable) {
                                            var (isProtected, goList) =
                                                IsProtectedAgainst(targetPosition, captureFunction.Method.Name);

                                            if (isProtected &&
                                                InBlockList(goList, GetPosition(piecePosition))) {
                                                FinalProtectedPositions.Add(
                                                    (targetPosition, piecePosition, captureFunction.Method.Name));
                                            } else {
                                                // Attacked piece not protected against this GO
                                                singleMoveList.Add((piecePosition, targetPosition, targetPosition,
                                                                    captureFunction.Method.Name));
                                            }
                                        }
                                    }
                                } else { // Target square is not an enemy
                                    // Iterate over all self-capturing moves
                                    foreach (Func<(int, int), bool> selfCaptureFunction in selfCaptureFunctions) {
                                        capturable =
                                            selfCaptureFunction(GetMoveFromPositions(piecePosition, targetPosition));
                                        if (capturable) {
                                            var (isProtected, goList) =
                                                IsProtectedAgainst(targetPosition, selfCaptureFunction.Method.Name);

                                            // Not sure when a piece would be protected against the same side, but here we are
                                            if (isProtected &&
                                                InBlockList(goList, GetPosition(piecePosition))) {
                                                FinalProtectedPositions.Add(
                                                    (targetPosition, piecePosition, selfCaptureFunction.Method.Name));
                                            } else {
                                                // Attacked piece not protected against this GO
                                                singleMoveList.Add((piecePosition, targetPosition, targetPosition,
                                                                    selfCaptureFunction.Method.Name));
                                            }
                                        }
                                    }
                                }
                            } else { // Target is empty

                                // Try Ranged functions
                                // DONE
                                // Ranged functions do not capture on target square
                                foreach (Func<(int, int), (bool, List<(int, int)>)> rangedFunction in rangedFunctions) {
                                    rangedable = rangedFunction(GetMoveFromPositions(piecePosition, targetPosition));
                                    if (rangedable.Item1) {
                                        foreach ((int, int) rangedablePosition in rangedable.Item2) {
                                            var (isProtected, goList) =
                                                IsProtectedAgainst(rangedablePosition, rangedFunction.Method.Name);

                                            if (isProtected &&
                                                InBlockList(goList, GetPosition(piecePosition))) {
                                                FinalProtectedPositions.Add(
                                                    (rangedablePosition, piecePosition, rangedFunction.Method.Name));
                                            } else {
                                                singleMoveList.Add((piecePosition, targetPosition, rangedablePosition,
                                                                    rangedFunction.Method
                                                                        .Name)); // Attacked piece not protected
                                            }
                                        }
                                    }
                                }

                                // Try Move functions
                                // DONE
                                // Regular moves don't capture anything
                                foreach (Func<(int, int), bool> moveFunction in moveFunctions) {
                                    bool moveable = moveFunction(GetMoveFromPositions(piecePosition, targetPosition));
                                    if (moveable) {
                                        singleMoveList.Add((piecePosition, targetPosition, targetPosition,
                                                            moveFunction.Method.Name));
                                    }
                                }
                            }

                            // Try Attack Replace functions. Will check all squares, empty/friendly included
                            // DONE
                            foreach (var attackReplaceFunction in attackReplaceFunctions) {
                                var (attackReplaceable, attackReplaceDict) =
                                    attackReplaceFunction(GetMoveFromPositions(piecePosition, targetPosition));
                                if (attackReplaceable) {
                                    if (!IsEmptySquare(targetPosition) && // Occupied
                                        IsEnemy(targetPosition, GetPieceSide(piecePosition)) && // Enemy
                                        IsProtectedAgainst(targetPosition, attackReplaceFunction.Method.Name).Item1 &&
                                        InBlockList(
                                            IsProtectedAgainst(targetPosition, attackReplaceFunction.Method.Name).Item2,
                                            GetPosition(piecePosition))) {
                                        // Immune
                                        FinalProtectedPositions.Add((targetPosition, piecePosition,
                                                                     attackReplaceFunction.Method.Name));
                                    } else {
                                        singleMoveList.Add((piecePosition, targetPosition, targetPosition,
                                                            attackReplaceFunction.Method.Name));
                                    }
                                }
                            }

                            // Try Passive Replace functions
                            foreach (var passiveReplaceFunction in passiveReplaceFunctions) {
                                var (passiveReplaceable, passiveReplaceDict) =
                                    passiveReplaceFunction(GetMoveFromPositions(piecePosition, targetPosition));
                                if (passiveReplaceable) {
                                    if (!IsEmptySquare(targetPosition) && // Occupied
                                        IsEnemy(targetPosition, GetPieceSide(piecePosition)) && // Enemy
                                        IsProtectedAgainst(targetPosition, passiveReplaceFunction.Method.Name).Item1 &&
                                        InBlockList(
                                            IsProtectedAgainst(targetPosition, passiveReplaceFunction.Method.Name)
                                                .Item2, GetPosition(piecePosition))) {
                                        // Immune
                                        FinalProtectedPositions.Add((targetPosition, piecePosition,
                                                                     passiveReplaceFunction.Method.Name));
                                    } else {
                                        singleMoveList.Add((piecePosition, targetPosition, targetPosition,
                                                            passiveReplaceFunction.Method.Name));
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        // var singleMoveListDestination = singleMoveList.Select(s => (s.Item2, s.Item3, s.Item4));
        FinalSingleMoves = singleMoveList.ToLookup(o => o.Item1);
    }

    /// <summary>
    /// 
    /// </summary>
    public void GenerateMultipleMoves() {
        // Calculate multiple (double piece) moves
        (int, int) piecePositionA;
        (int, int) piecePositionB;
        List<(((int, int), (int, int)), ((int, int), (int, int)), string)> multipleMoveList = new();

        var pieceIdentity = MultipleChecker.PieceIdentity;
        foreach (var (multipleFunction,
                     (multiplePieceTypesA,
                     multiplePieceTypesB)) in pieceIdentity) {
            // PieceScript.PieceType multiplePieceTypesA = multiplePieceTypes[0];
            // List<PieceScript.PieceType> multiplePieceTypesB =
            //     multiplePieceTypes.GetRange(1, multiplePieceTypes.Count - 1);

            if (InBanList(multipleFunction.Method.Name)) {
                continue;
            }

            // Iterate over piece A
            for (int i = 0; i < boardSize; i++) {
                for (int j = 0; j < boardSize; j++) {
                    piecePositionA = (i, j);
                    GameObject pieceGameObjectA = GetPosition(piecePositionA);

                    if (!IsEmptySquare(piecePositionA) && // Not empty
                        multiplePieceTypesA.Contains(GetPieceType(piecePositionA)) && // Match piece type 1st
                        GetPosition(piecePositionA).GetComponent<PieceScript>().ProtectFunctions !=
                        default(List<Func<(int, int), (GameObject, string)>>)) { // Not uninitialized. REMOVE LATER

                        // Iterate over piece B
                        for (int k = 0; k < boardSize; k++) {
                            for (int l = 0; l < boardSize; l++) {
                                piecePositionB = (k, l);
                                GameObject pieceGameObjectB = GetPosition(piecePositionB);

                                if (!IsEmptySquare(piecePositionB) && // Not empty
                                    multiplePieceTypesB.Contains(
                                        GetPieceType(piecePositionB)) && // Match any of piece type 2nd
                                    !IsEnemy(piecePositionB, GetPieceSide(piecePositionA))) { // Is friendly


                                    var (isMoveValid, validPositionA, validPositionB) =
                                        multipleFunction(piecePositionA, piecePositionB);

                                    if (isMoveValid) { // # of valid moves > 0
                                        for (int m = 0; m < validPositionA.Count; m++) {
                                            // Iterate over all valid moves (2 pairs of positions)
                                            MultiplePiecePositions.Add(new List<((int, int), (int, int))> {
                                                (piecePositionA, validPositionA[m]), // where piece A goes
                                                (piecePositionB, validPositionB[m]) // where piece B goes
                                            }, multipleFunction.Method.Name);


                                            bool isProtectedA = !IsEmptySquare(validPositionA[m]) && // Occupied
                                                                IsEnemy(validPositionA[m],
                                                                        GetPieceSide(piecePositionA)) && // Enemy
                                                                IsProtectedAgainst(validPositionA[m], // Immune
                                                                    multipleFunction.Method.Name).Item1 &&
                                                                InBlockList(
                                                                    IsProtectedAgainst(validPositionA[m], // Immune
                                                                        multipleFunction.Method.Name).Item2,
                                                                    pieceGameObjectA);

                                            bool isProtectedB = !IsEmptySquare(validPositionB[m]) && // Occupied
                                                                IsEnemy(validPositionB[m],
                                                                        GetPieceSide(piecePositionB)) && // Enemy
                                                                IsProtectedAgainst(validPositionB[m], // Immune
                                                                    multipleFunction.Method.Name).Item1 &&
                                                                InBlockList(
                                                                    IsProtectedAgainst(validPositionB[m], // Immune
                                                                        multipleFunction.Method.Name).Item2,
                                                                    pieceGameObjectB);

                                            if (isProtectedA) {
                                                FinalProtectedPositions.Add(
                                                    (validPositionA[m], piecePositionA, multipleFunction.Method.Name));
                                            }

                                            if (isProtectedB) {
                                                FinalProtectedPositions.Add(
                                                    (validPositionB[m], piecePositionB, multipleFunction.Method.Name));
                                            }

                                            if (!isProtectedA && !isProtectedB) {
                                                multipleMoveList.Add(((piecePositionA, piecePositionB),
                                                                      (validPositionA[m], validPositionB[m]),
                                                                      multipleFunction.Method.Name));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        // var singleMoveListDestination = singleMoveList.Select(s => (s.Item2, s.Item3, s.Item4));
        FinalMultipleMoves = multipleMoveList.ToLookup(o => o.Item1.Item1);
    }

    IEnumerator SelectMove() {
        int moveNumberPieces = -1;

        bool validMove = false;

        Debug.Log("Select move called");

        while (!validMove) {
            SelectedPositions.Clear();
            HighlightedPositions.Clear();
            MultiplePairPositions.Clear();
            AttackPositions.Clear();
            RangedPositions.Clear();
            MoveboxList.Clear();
            CircleShieldList.Clear();

            // List<GameObject> mppCircles = new();
            HashSet<string> applicableMoves = new();

            string selectedMoveName;
            (int, int) selectedFinalPosition;

            // AWAIT FIRST VALID SELECTION
            while (SelectedPositions.Count == 0 ||
                   IsEmptySquare(SelectedPositions[0]) ||
                   PlayingSide != GetPieceSide(SelectedPositions[0])) {
                SelectedPositions.Clear();
                yield return new WaitUntil(() => SelectedPositions.Count >= 1);
            }

            // Update Highlighted, MultiplePair, and Attack lists
            foreach (var (attackPosition, finalPosition, targetPosition, moveName)
                     in FinalSingleMoves[SelectedPositions[0]]) {
                HighlightedPositions.Add(finalPosition);
                applicableMoves.Add(moveName);
                if (finalPosition != targetPosition) {
                    RangedPositions.Add(targetPosition);
                }
            }

            foreach (var ((initialPositionA, initialPositionB), (finalPositionA, finalPositionB), moveName)
                     in FinalMultipleMoves[SelectedPositions[0]]) {
                HighlightedPositions.Add(finalPositionA);
                MultiplePairPositions.Add(initialPositionB);
                applicableMoves.Add(moveName);
            }

            foreach (var highlightedPosition in HighlightedPositions) {
                if (!IsEmptySquare(highlightedPosition)) {
                    AttackPositions.Add(highlightedPosition);
                }
            }

            // HighlightedPositions = HighlightedPositions.Distinct().ToList();
            InstantiateCirclesShields();

            // Create Moveboxes
            // Update Highlighted and MultiplePair lists
            MoveboxesHeight = 0;
            foreach (string move in applicableMoves) {
                // Temporary. Have it dynamically fit the size of the text 
                yield return StartCoroutine(InstantiateMovebox(move, 3));
            }

            MoveboxScript.SelectedItem = -1;

            // Initialize Single, Multiple, and MoveName lists
            Debug.Log(SelectedPositions[0]);
            List<((int, int), (int, int), (int, int), string)> narrowedSingleMoves =
                FinalSingleMoves[SelectedPositions[0]].ToList();
            List<(((int, int), (int, int)), ((int, int), (int, int)), string)> narrowedMultipleMoves =
                FinalMultipleMoves[SelectedPositions[0]].ToList();
            List<string> narrowedMoveNames = narrowedSingleMoves.Select(s => s.Item4).ToList();
            narrowedMoveNames.AddRange(narrowedMultipleMoves.Select(s => s.Item3).ToList());
            List<(int, int)> narrowedAssistPositions = narrowedMultipleMoves.Select(s => s.Item1.Item2).ToList();
            List<(int, int)> narrowedRangedPositions =
                narrowedSingleMoves.FindAll(s => s.Item2 != s.Item3).Select(s => s.Item3).ToList();


            bool pickMove = false;
            bool pickTarget = false;
            bool pickPath = false;
            bool pickAssist = false;
            bool[] deltaPick;
            int choicesPicked = 0;

            while (narrowedSingleMoves.Count + narrowedMultipleMoves.Count > 1 ||
                   CountTrue(pickTarget, pickPath, pickAssist, pickMove) == 0) {
                deltaPick = new[] { false, false, false, false };

                // AWAIT SECOND/THIRD VALID SELECTION: move or 2nd square
                while (choicesPicked >= CountTrue(pickMove, pickTarget, pickPath, pickAssist)) {
                    yield return new WaitUntil(
                        () => (CountTrue(pickTarget, pickPath, pickAssist) < SelectedPositions.Count - 1) ||
                              (!pickMove && MoveboxScript.SelectedItem != -1));

                    if (CountTrue(pickTarget, pickPath, pickAssist) < SelectedPositions.Count - 1) {
                        if (!pickTarget && HighlightedPositions.Contains(SelectedPositions.Last())) {
                            pickTarget = true;
                            deltaPick[1] = true;
                            break;
                        } else if (!pickAssist && MultiplePairPositions.Contains(SelectedPositions.Last())) {
                            pickAssist = true;
                            deltaPick[3] = true;
                            break;
                        } else if (!pickPath && RangedPositions.Contains(SelectedPositions.Last())) {
                            pickPath = true;
                            deltaPick[2] = true;
                            break;
                        }

                        SelectedPositions.RemoveAt(SelectedPositions.Count - 1);

                        SelectingMove = false;
                        yield return StartCoroutine(ResetGameLoop());
                        
                        // if (_selectingMove) {
                        //     yield return StartCoroutine(ResetGameLoop());
                        // }
                        // ResetGameLoop();
                        // StartCoroutine(ResetGameLoop()); // Click invalid square in board -> reset selection
                    } else if (!pickMove && MoveboxScript.SelectedItem != -1) {
                        pickMove = true;
                        deltaPick[0] = true;
                        Debug.Log("bingbong");
                        break;
                    } else {
                        throw new NotImplementedException("what did you do lol");
                    }
                }

                choicesPicked++;

                // ResetGameLoop();
                // yield break;

                yield return StartCoroutine(DeleteCircleSpritesAndList());

                if (deltaPick[0]) { // MOVE
                    // Update Highlighted and MultiplePair lists
                    selectedMoveName = MoveboxList[MoveboxScript.SelectedItem].GetComponent<MoveboxScript>().MoveName;

                    narrowedSingleMoves = narrowedSingleMoves.FindAll(s => s.Item4 == selectedMoveName);
                    narrowedMultipleMoves = narrowedMultipleMoves.FindAll(s => s.Item3 == selectedMoveName);
                    // Update MoveNames list
                    narrowedMoveNames = new() { selectedMoveName };
                    // Updated Assist list
                    narrowedAssistPositions = narrowedMultipleMoves.Select(s => s.Item1.Item2).ToList();
                    // Update Ranged list
                    narrowedRangedPositions = narrowedSingleMoves.FindAll(s => s.Item2 != s.Item3)
                        .Select(s => s.Item3)
                        .ToList();
                } else if (deltaPick[1]) { // TARGET
                    // Update Highlighted and MultiplePair lists
                    selectedFinalPosition = SelectedPositions.Last();
                    narrowedSingleMoves = narrowedSingleMoves.FindAll(s => s.Item2 == selectedFinalPosition);
                    narrowedMultipleMoves = narrowedMultipleMoves.FindAll(s => s.Item2.Item1 == selectedFinalPosition);
                    // Update MoveNames list
                    var tempMoveNames = narrowedSingleMoves.Select(s => s.Item4).ToList();
                    tempMoveNames.AddRange(narrowedMultipleMoves.Select(s => s.Item3).ToList());
                    narrowedMoveNames = narrowedMoveNames.Intersect(tempMoveNames).ToList();
                    // Update Assist list
                    narrowedAssistPositions = narrowedMultipleMoves.Select(s => s.Item1.Item2).ToList();
                    // Update Ranged list
                    narrowedRangedPositions = narrowedSingleMoves.FindAll(s => s.Item2 != s.Item3)
                        .Select(s => s.Item3)
                        .ToList();
                } else if (deltaPick[2]) { // PATH
                    // Update Ranged list
                    narrowedRangedPositions = new List<(int, int)> { SelectedPositions.Last() };
                    // Update Highlighted and MultiplePair lists
                    narrowedSingleMoves =
                        narrowedSingleMoves.FindAll(s => s.Item2 != s.Item3 && s.Item3 == narrowedRangedPositions[0]);
                    narrowedMultipleMoves.Clear();
                    // Update MoveNames list
                    narrowedMoveNames = narrowedMoveNames.Intersect(narrowedSingleMoves.Select(s => s.Item4).ToList())
                        .ToList();
                    // Update Assist list
                    narrowedAssistPositions.Clear();
                } else { // ASSIST (multiplemove)
                    // Update Assist list
                    narrowedAssistPositions = new List<(int, int)> { SelectedPositions.Last() };
                    // Update Highlighted and MultiplePair lists
                    narrowedSingleMoves.Clear();
                    narrowedMultipleMoves =
                        narrowedMultipleMoves.FindAll(s => s.Item1.Item2 == narrowedAssistPositions[0]);
                    // Update MoveNames list
                    narrowedMoveNames =
                        narrowedMoveNames.Intersect(
                                narrowedMultipleMoves.Select(s => s.Item3).ToList())
                            .ToList();
                    // Update Ranged list
                    narrowedRangedPositions.Clear();
                }


                // Ease out moveboxes
                foreach (GameObject movebox in MoveboxList) {
                    if (movebox != null &&
                        !narrowedMoveNames.Contains(movebox.GetComponent<MoveboxScript>().MoveName)) {
                        movebox.GetComponent<MoveboxScript>().FlagEaseOut();
                    }
                }

                // GameObject[] moveBoxesToDelete = GameObject.FindGameObjectsWithTag("Movebox");
                // for (int i = 0; i < moveBoxesToDelete.Length; i++) {
                //     
                // }

                ClearCirclePositionsLists();


                // Update Highlighted, MPP, and Attack positions
                foreach (var (attackPosition, finalPosition, targetPosition, moveName)
                         in narrowedSingleMoves) {
                    HighlightedPositions.Add(finalPosition);
                    if (!IsEmptySquare(targetPosition)) {
                        if (targetPosition == finalPosition) {
                            AttackPositions.Add(targetPosition);
                        } else {
                            RangedPositions.Add(targetPosition);
                        }
                    }
                }

                foreach (var ((initialPositionA, initialPositionB), (finalPositionA, finalPositionB), moveName)
                         in narrowedMultipleMoves) {
                    HighlightedPositions.Add(finalPositionA);
                    MultiplePairPositions.Add(initialPositionB);
                    if (!IsEmptySquare(finalPositionA)) {
                        AttackPositions.Add(finalPositionA);
                    }
                    // if (!IsEmptySquare(finalPositionB)) {
                    //     AttackPositions.Add(finalPositionB);
                    // }
                }

                // InstantiateCirclesShields(moveName: new List<string> { selectedMoveName });
                yield return StartCoroutine(DeleteCircleSpritesAndList());
                InstantiateCirclesShields(moveName: narrowedMoveNames);
            }

            if (narrowedSingleMoves.Count + narrowedMultipleMoves.Count == 0) { // A move is not valid for the square
                yield return StartCoroutine(DeleteCircleSpritesAndList());
                continue;
                // throw new InvalidDataException("Somehow you selected NO MOVE??");
            }

            yield return new WaitForSeconds(selectWaitTime);
            
            // Deactivate ability to cancel move
            SelectingMove = false;

            yield return ClearAllVisualsAndWait();

            // Save the final selected move
            selectedSingleMove = narrowedSingleMoves.Count > 0 ? narrowedSingleMoves[0] : new();
            selectedMultipleMove = narrowedMultipleMoves.Count > 0 ? narrowedMultipleMoves[0] : new();

            _selectedMoveName = narrowedSingleMoves.Count > 0
                ? narrowedSingleMoves[0].Item4
                : narrowedMultipleMoves[0].Item3;

            validMove = true;
        }


        // return (moveNumberPieces, (0, 0));
    }

    /// <summary>
    /// Adds to CircleShieldList based on a MoveName
    /// </summary>
    /// <param name="moveName"></param>
    private void InstantiateCirclesShields(List<string> moveName = null) {
        // Spawn select reticles (piece + target)
        foreach ((int, int) selectedPosition in SelectedPositions) {
            CircleShieldList.Add(InstantiateOneReticle(selectedPosition, CircleHighlightScript.SpriteType.Select));
        }

        // Spawn Attack reticles
        foreach (var attackPosition in AttackPositions) {
            CircleShieldList.Add(InstantiateOneReticle(attackPosition, CircleHighlightScript.SpriteType.Attack));
        }

        // Spawn Ranged reticles
        foreach (var rangedPosition in RangedPositions) {
            CircleShieldList.Add(InstantiateOneReticle(rangedPosition, CircleHighlightScript.SpriteType.Ranged));
        }

        // Spawn in MultiplePair circles
        var multiplePairPositionsDistinct = MultiplePairPositions.Distinct().ToList();
        foreach ((int, int) multiplePairPosition in multiplePairPositionsDistinct) {
            CircleShieldList.Add(InstantiateOneReticle(multiplePairPosition, CircleHighlightScript.SpriteType.Cirle));
        }

        // Spawn in Shield circles
        var attackDefendPairs = FinalProtectedPositions.Select(t => (t.Item1, t.Item2));
        IEnumerable<((int, int), (int, int), string)> attackDefendThisMove = null;
        if (moveName is not null) {
            attackDefendThisMove = FinalProtectedPositions.FindAll(s => moveName.Contains(s.Item3));
        }

        for (int i = 0; i < boardSize; i++) {
            for (int j = 0; j < boardSize; j++) {
                if (attackDefendPairs.Contains(((i, j), SelectedPositions[0])) &&
                    !(moveName is not null &&
                      !attackDefendThisMove.Select(s => (s.Item1, s.Item2)).Contains(((i, j), SelectedPositions[0]))))
                    // the defending piece is not protected against the attacking piece by the move name
                    // movename is not present in the spawned selectedpositions
                {
                    CircleShieldList.Add(InstantiateOneReticle((i, j), CircleHighlightScript.SpriteType.Shield));
                }
            }
        }

        // IsProtectedAgainst()
    }

    /// <summary>
    /// Helper function for InstantiateCirclesShields
    /// </summary>
    /// <param name="position"></param>
    /// <param name="spriteType"></param>
    /// <returns></returns>
    private GameObject InstantiateOneReticle((int, int) position, CircleHighlightScript.SpriteType spriteType) {
        GameObject reticle =
            Instantiate(circleHighlightPrefab,
                        PositionToVector3(position),
                        quaternion.identity);
        CircleHighlightScript circleHighlightComponent = reticle.GetComponent<CircleHighlightScript>();
        circleHighlightComponent.Position = position;
        circleHighlightComponent.thisSpriteType = spriteType;
        // CircleShieldList.Add(circleHighlight);
        return reticle;
    }

    private GameObject InstantiatePiece(PieceScript.PieceType pieceType, (int, int) position,
        PieceScript.Side pieceSide = PieceScript.Side.None) {
        GameObject pieceGO = Instantiate(TypePieceDict[pieceType],
                                         PositionToVector3(position),
                                         quaternion.identity);
        if (pieceType == PieceScript.PieceType.Empty) {
            pieceGO.GetComponent<PieceScript>().PieceSide = PieceScript.Side.None;
            pieceGO.SetActive(false);
        } else {
            if (pieceSide == PieceScript.Side.None) {
                throw new NotImplementedException("Can't take in None piecetypes for board pieces");
            }

            pieceGO.GetComponent<PieceScript>().PieceSide = pieceSide;
        }

        return pieceGO;
    }

    IEnumerator InstantiateMovebox(string moveName, int moveboxHeight) {
        float moveboxY = 0;
        for (int i = 0; i < MoveboxList.Count; i++) {
            // if (i == 0) {
            //     // moveboxY += MoveboxList[i].GetComponent<MoveboxScript>().moveboxHeightSize / 2.0f;
            //     moveboxY += MoveboxList[i].GetComponent<MoveboxScript>().moveboxHeightSize;
            // } else {
            //     moveboxY += MoveboxList[i].GetComponent<MoveboxScript>().moveboxHeightSize;
            // }
            moveboxY += MoveboxList[i].GetComponent<MoveboxScript>().moveboxHeightSize;
        }

        GameObject prefab;
        if (moveboxHeight == 2) {
            prefab = movebox2Prefab;
        } else if (moveboxHeight == 3) {
            prefab = movebox3Prefab;
        } else {
            throw new NotImplementedException("Can't take moveboxes that are not 2 or 3 yet");
        }

        // if (MoveboxList.Count == 0) {
        //     moveboxY = 0;
        // } else {
        moveboxY += prefab.GetComponent<MoveboxScript>().moveboxHeightSize / 2.0f;
        // }

        float scale = prefab.GetComponent<MoveboxScript>().edgeLength;
        GameObject movebox = Instantiate(prefab,
                                         moveboxInitialPosition + new Vector3(0, -moveboxY * scale, 0),
                                         Quaternion.identity);
        movebox.GetComponent<MoveboxScript>().ItemNumber = MoveboxList.Count;
        movebox.GetComponent<MoveboxScript>().MoveName = moveName;
        movebox.GetComponent<MoveboxScript>().MoveboxY = moveboxY;
        MoveboxList.Add(movebox);

        MoveboxesHeight = Math.Max(moveboxHeight,
                                   moveboxY + prefab.GetComponent<MoveboxScript>().moveboxHeightSize / 2.0f);

        yield break;
    }

    public void EaseOutAllMoveboxes() {
        // foreach (GameObject movebox in MoveboxList) {
        //     if (movebox != null) {
        //         movebox.GetComponent<MoveboxScript>().FlagEaseOut();
        //     }
        // }
        //
        // for (int i = 0; i < MoveboxList.Count; i++) {
        //     if(MoveboxList.)
        // }

        for (int i = 0; i < MoveboxList.Count; i++) {
            if (MoveboxList[i] != null) {
                MoveboxList[i].GetComponent<MoveboxScript>().FlagEaseOut();
                MoveboxList[i].GetComponent<MoveboxScript>().FlagDelete();
            }
        }
    }

    IEnumerator EaseOutAllMoveboxesAndWait() {
        EaseOutAllMoveboxes();

        yield return new WaitUntil(() => MoveboxList.All(g => g == null));

        Debug.Log("moveboxes easing out");
    }

    IEnumerator DeleteCircleSpritesAndList() {
        for (int i = 0; i < CircleShieldList.Count; i++) {
            if (CircleShieldList[i] != null) {
                yield return StartCoroutine(CircleShieldList[i].GetComponent<CircleHighlightScript>().Destroy());
            }
        }

        CircleShieldList.Clear();

        yield break;
    }

    public void DestroyAllCircles() {
        GameObject[] garbageList = GameObject.FindGameObjectsWithTag("Circle");
        for (int i = 0; i < garbageList.Length; i++) {
            Destroy(garbageList[i]);
        }
    }

    IEnumerator ClearAllVisualsAndWait() {
        // Clear out highlighted positions
        HighlightedPositions.Clear();

        // Start to clear out all remaining circle/shield items
        List<Coroutine> fadeoutList = new();
        for (int i = 0; i < CircleShieldList.Count; i++) {
            if (CircleShieldList[i] != null &&
                CircleShieldList[i].TryGetComponent(out CircleHighlightScript chs)) {
                fadeoutList.Add(StartCoroutine(chs.FadeOutSequence()));
            }
        }

        // Wait for all moveboxes to clear out
        yield return EaseOutAllMoveboxesAndWait();

        // Wait for all circle/shield items to clear out
        for (int i = 0; i < fadeoutList.Count; i++) {
            yield return fadeoutList[i];
        }

        DestroyAllCircles();

    }

    private void ClearCirclePositionsLists() {
        HighlightedPositions.Clear();
        MultiplePairPositions.Clear();
        AttackPositions.Clear();
        RangedPositions.Clear();
    }

    IEnumerator MovePieces() {
        if (selectedSingleMove != default(((int, int), (int, int), (int, int), string))) {
            var (piecePosition, finalPosition, targetPosition, moveName) = selectedSingleMove;

            if (MoveNames["AttackReplaceFunctions"].Contains(moveName) ||
                MoveNames["PassiveReplaceFunctions"].Contains(moveName)) {
                Type replacedType = GetPosition(piecePosition).GetComponent<PieceScript>().GetType();
                var replaceMethod = replacedType.GetMethod(moveName);

                (int, int) replaceMove = GetMoveFromPositions(piecePosition, finalPosition);

                (bool, Dictionary<(int, int), List<PieceScript.PieceType>>) replaceOutput =
                    ((bool, Dictionary<(int, int), List<PieceScript.PieceType>>))replaceMethod.Invoke(
                        GetPosition(piecePosition).GetComponent<PieceScript>(),
                        new object[] { replaceMove });

                // Debug.Log(replaceOutput.Item1);
                // Debug.Log(string.Join(",", replaceOutput.Item2));

                PieceScript.PieceType selectedPieceType;

                DeleteList.Add(GetPosition(piecePosition));
                _board[piecePosition.Item1, piecePosition.Item2] =
                    InstantiatePiece(PieceScript.PieceType.Empty, piecePosition);

                foreach (var (newPosition, newPieceTypes) in replaceOutput.Item2) {
                    if (newPieceTypes.Count == 1) {
                        selectedPieceType = newPieceTypes[0];
                    } else {
                        yield return SelectReplacements(newPieceTypes, newPosition);
                        selectedPieceType = newPieceTypes[ReplaceboxScript.SelectedReplace];
                    }

                    DeleteList.Add(GetPosition(newPosition));
                    _board[newPosition.Item1, newPosition.Item2] =
                        InstantiatePiece(selectedPieceType, newPosition, PlayingSide);
                    GetPosition(newPosition).GetComponent<PieceScript>().SetSpriteSide();

                    // Hardcoded check for Coregency.
                    // All replace functions cause the new pieces to be treated as promoted pieces, EXCEPT for kings.
                    // Maybe you can define it so that Kings don't have any invalid promote moves? And just set this to be true for all
                    if (moveName != "CoregencyReplace") {
                        GetPosition(newPosition).GetComponent<PieceScript>().Promoted = true;
                    }
                }
            } else if (MoveNames["MoveFunctions"].Contains(moveName) ||
                       MoveNames["CaptureFunctions"].Contains(moveName) ||
                       MoveNames["SelfCaptureFunctions"].Contains(moveName)) {
                DeleteList.Add(GetPosition(finalPosition));
                _board[finalPosition.Item1, finalPosition.Item2] = _board[piecePosition.Item1, piecePosition.Item2];
                _board[piecePosition.Item1, piecePosition.Item2] =
                    InstantiatePiece(PieceScript.PieceType.Empty, piecePosition);
            } else if (MoveNames["RangedFunctions"].Contains(moveName)) {
                DeleteList.Add(GetPosition(finalPosition));
                _board[finalPosition.Item1, finalPosition.Item2] = _board[piecePosition.Item1, piecePosition.Item2];
                _board[piecePosition.Item1, piecePosition.Item2] =
                    InstantiatePiece(PieceScript.PieceType.Empty, piecePosition);
                DeleteList.Add(GetPosition(targetPosition));
                _board[targetPosition.Item1, targetPosition.Item2] =
                    InstantiatePiece(PieceScript.PieceType.Empty, piecePosition);
            } else {
                throw new NotImplementedException("Somehow called a protect function");
            }

            // Increment moves for moved pieces
            GetPosition(targetPosition).GetComponent<PieceScript>().IncrementMoveCounter();
        } else {
            var ((piecePositionA, piecePositionB), (finalPositionA, finalPositionB), moveName) = selectedMultipleMove;
            if (finalPositionA != piecePositionA) {
                DeleteList.Add(GetPosition(finalPositionA));
                _board[finalPositionA.Item1, finalPositionA.Item2] = _board[piecePositionA.Item1, piecePositionA.Item2];
                _board[piecePositionA.Item1, piecePositionA.Item2] =
                    InstantiatePiece(PieceScript.PieceType.Empty, piecePositionA);
            }

            if (finalPositionB != piecePositionB) {
                DeleteList.Add(GetPosition(finalPositionB));
                _board[finalPositionB.Item1, finalPositionB.Item2] = _board[piecePositionB.Item1, piecePositionB.Item2];
                _board[piecePositionB.Item1, piecePositionB.Item2] =
                    InstantiatePiece(PieceScript.PieceType.Empty, piecePositionB);
            }

            // Increment moves for moved pieces
            GetPosition(finalPositionA).GetComponent<PieceScript>().IncrementMoveCounter();
            GetPosition(finalPositionB).GetComponent<PieceScript>().IncrementMoveCounter();
        }

        yield break;
    }

    IEnumerator SelectReplacements(List<PieceScript.PieceType> validPieceTypes, (int, int) centerPosition) {
        ReplaceboxScript.SelectedReplace = -1;
        ReplaceBoxList.Clear();
        foreach (PieceScript.PieceType pieceType in validPieceTypes) {
            ReplaceBoxList.Add(Instantiate(replaceBoxPrefab,
                                           PositionToVector3(centerPosition),
                                           Quaternion.identity));

            ReplaceBoxList.Last().GetComponent<ReplaceboxScript>().PieceType = pieceType;
        }

        yield return new WaitUntil(() => ReplaceboxScript.SelectedReplace != -1);

        DeleteReplaceBoxes();
    }

    public void UpdateAutomaticMoves() {
        // piecePieceScript.AutomaticMove(); // Check automatic moves, like for Footmen self capture
        for (int i = 0; i < boardSize; i++) {
            for (int j = 0; j < boardSize; j++) {
                GetPosition((i, j)).GetComponent<PieceScript>().AutomaticMove();
                if (DeleteList.Contains(GetPosition((i, j)))) {
                    _board[i, j] =
                        InstantiatePiece(PieceScript.PieceType.Empty, (i, j));
                    GetPosition((i, j)).GetComponent<PieceScript>().SetSpriteSide();
                }
            }
        }
    }

    public void UpdateBannedTurns() {
        // Decrement all banned turn counters
        for (int i = 0; i < _bannedMoves.Count; i++) {
            var (bannedMove, bannedTurns) = _bannedMoves.ElementAt(i);
            if (bannedMove.Item2 == PlayingSide) {
                _bannedMoves[bannedMove] = Math.Max(0, bannedTurns - 1);
            }
        }

        // foreach (var (bannedMove, bannedTurns) in _bannedMoves) {
        //     _bannedMoves[bannedMove] = Math.Max(0, bannedTurns - 1);
        // }

        // Add to ban list
        if (RepeatMovesScript.BanningMovesDict.Keys.Contains(_selectedMoveName)) {
            Dictionary<(string, bool), int> bannedDict = RepeatMovesScript.BanningMovesDict[_selectedMoveName];
            foreach (var (bannedMove, bannedTurns) in bannedDict) {
                _bannedMoves[(bannedMove.Item1,
                              bannedMove.Item2
                                  ? InvertSide(PlayingSide)
                                  : PlayingSide)]
                    += bannedTurns;
            }
        }
    }

    public bool InBanList(string moveName, PieceScript.Side playingSide = PieceScript.Side.None) {
        // if (playingSide == PieceScript.Side.None) {
        //     return _bannedMoves[(moveName, PlayingSide)] > 0;
        // }

        return _bannedMoves[(moveName, playingSide == PieceScript.Side.None
                                 ? PlayingSide
                                 : playingSide)] > 0;
    }

    public void CheckExtraMove() {
        if (RepeatMovesScript.ExtraMovesDict.Keys.Contains(_selectedMoveName)) {
            _extraMoves[PlayingSide] += RepeatMovesScript.ExtraMovesDict[_selectedMoveName];
        }
    }

    /// <summary>
    /// Refreshes every piece's positions
    /// </summary>
    public void UpdatePiecePositions() {
        for (int i = 0; i < boardSize; i++) {
            for (int j = 0; j < boardSize; j++) {
                GetPosition((i, j)).GetComponent<PieceScript>().Position = (i, j);
            }
        }
    }


    /// <summary>
    /// Update general piece properties: NumberTurnsOnBoard, MoveCounter
    /// </summary>
    public void UpdatePieceProperties(PieceScript.Side playingSide) {
        (int, int) piecePosition;
        for (int i = 0; i < boardSize; i++) {
            for (int j = 0; j < boardSize; j++) { // piece position

                piecePosition = (i, j);

                if (!IsEmptySquare(piecePosition) &&
                    GetPieceSide(piecePosition) == playingSide) {
                    PieceScript piecePieceScript = GetPosition(piecePosition).GetComponent<PieceScript>();
                    piecePieceScript.IncrementTurnsOnBoardCounter(); // Increment number of turns on board
                    // Move counter is incremented in MovePieces
                }
            }
        }
    }

    public void UpdateAllSpriteSides() {
        for (int i = 0; i < boardSize; i++) {
            for (int j = 0; j < boardSize; j++) {
                if (!IsEmptySquare((i, j))) {
                    GetPosition((i, j)).GetComponent<PieceScript>().SetSpriteSide();
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void DeletePieces() {
        foreach (GameObject pieceGO in DeleteList) {
            pieceGO.GetComponent<PieceScript>().DeleteAnimation();
            Destroy(pieceGO);
        }

        DeleteList.Clear();
    }

    public void DeleteReplaceBoxes() {
        foreach (GameObject replaceBox in ReplaceBoxList) {
            Destroy(replaceBox);
        }

        ReplaceBoxList.Clear();
    }

    /// <summary>
    /// <para>Switch which player can make a move.</para>
    /// <para>Also increments GameTurnCounter after 2 moves. After black's first move, 1. After black's second move, 2, etc.</para>
    /// </summary>
    public void SwitchPlayers() {
        if (PlayingSide == PieceScript.Side.Black) {
            GameTurnCounter++;
        }

        PlayingSide = InvertSide(PlayingSide);

        // if (PlayingSide == PieceScript.Side.White) {
        //     PlayingSide = PieceScript.Side.Black;
        // } else { // Black finished their move
        //     PlayingSide = PieceScript.Side.White;
        // }
    }

    public static int CountTrue(params bool[] args) {
        return args.Count(t => t);
    }


    public List<(int, int)> GetAdjacentPositions((int, int) position, int distance = 1, bool taxicab = true,
        bool perimeter = false, bool connected = false) {
        HashSet<(int, int)> currentAdjacent = new HashSet<(int, int)>();
        HashSet<(int, int)> nextAdjacent = new HashSet<(int, int)>();

        currentAdjacent.Add(position);
        for (int n = 0; n < distance; n++) {
            foreach ((int, int) i in currentAdjacent) {
                int currrow = i.Item1;
                int currcol = i.Item2;
                nextAdjacent.Add((currrow + 1, currcol));
                nextAdjacent.Add((currrow - 1, currcol));
                nextAdjacent.Add((currrow, currcol + 1));
                nextAdjacent.Add((currrow, currcol - 1));
                if (!taxicab) {
                    nextAdjacent.Add((currrow + 1, currcol + 1));
                    nextAdjacent.Add((currrow + 1, currcol - 1));
                    nextAdjacent.Add((currrow - 1, currcol + 1));
                    nextAdjacent.Add((currrow - 1, currcol - 1));
                }
            }

            foreach ((int, int) i in nextAdjacent) {
                if (!IsOutsideBoard(i.Item1, i.Item2)) { // Cannot be outside the board
                    if (!(connected &&
                          n < distance - 1 &&
                          !IsEmptySquare(i))) {
                        currentAdjacent.Add(i);
                    }
                }
            }

            nextAdjacent.Clear();
        }

        currentAdjacent.Remove(position); // Remove the center hole

        if (perimeter) {
            List<(int, int)> output = new List<(int, int)>();
            foreach ((int, int) i in currentAdjacent) {
                if (Distance(i, position, taxicab: taxicab) == distance) {
                    output.Add(i);
                }
            }

            return output;
        } else {
            return currentAdjacent.ToList();
        }
    }

    public List<(int, int)> GetAdjacentMoves(int distance = 1, bool taxicab = true, bool perimeter = false) {
        HashSet<(int, int)> currentAdjacent = new HashSet<(int, int)>();
        HashSet<(int, int)> nextAdjacent = new HashSet<(int, int)>();

        currentAdjacent.Add((0, 0));
        for (int n = 0; n < distance; n++) {
            foreach ((int, int) i in currentAdjacent) {
                int currrow = i.Item1;
                int currcol = i.Item2;
                nextAdjacent.Add((currrow + 1, currcol));
                nextAdjacent.Add((currrow - 1, currcol));
                nextAdjacent.Add((currrow, currcol + 1));
                nextAdjacent.Add((currrow, currcol - 1));
                if (!taxicab) {
                    nextAdjacent.Add((currrow + 1, currcol + 1));
                    nextAdjacent.Add((currrow + 1, currcol - 1));
                    nextAdjacent.Add((currrow - 1, currcol + 1));
                    nextAdjacent.Add((currrow - 1, currcol - 1));
                }
            }

            currentAdjacent = new HashSet<(int, int)>(nextAdjacent);

            // foreach ((int, int) i in nextAdjacent) {
            //     if (!IsOutsideBoard(i.Item1, i.Item2)) {
            //         currentAdjacent.Add(i);
            //     }
            // }

            nextAdjacent.Clear();
        }

        currentAdjacent.Remove((0, 0)); // Remove the center hole

        if (perimeter) {
            List<(int, int)> output = new List<(int, int)>();
            foreach ((int, int) i in currentAdjacent) {
                if (Distance(i, (0, 0), taxicab: taxicab) == distance) {
                    output.Add(i);
                }
            }

            return output;
        } else {
            return currentAdjacent.ToList();
        }
    }

    public List<GameObject> GetAdjacentGameObjects((int, int) piecePosition, int distance = 1, bool taxicab = true,
        bool perimeter = false, bool connected = false, bool includeEmpty = false) {
        List<GameObject> output = new List<GameObject>();
        List<(int, int)> adjacentMoves = GetAdjacentPositions(piecePosition, distance, taxicab, perimeter, connected);
        foreach ((int, int) i in adjacentMoves) {
            output.Add(GetPosition(i));
        }

        if (!includeEmpty) {
            output.RemoveAll(s => IsEmptySquare(s.GetComponent<PieceScript>().Position));
            // output.RemoveAll(s => Object.ReferenceEquals(s, emptySquare));
        }

        return output;
    }

    public GameObject GetPosition((int, int) position) {
        return _board[position.Item1, position.Item2];
    }

    public GameObject GetPosition((int, int) position, (int, int) move) {
        return _board[position.Item1 + move.Item1, position.Item2 + move.Item2];
    }

    public (int, int) GetMoveFromPositions((int, int) positionStart, (int, int) positionEnd) {
        return (positionEnd.Item1 - positionStart.Item1, positionEnd.Item2 - positionStart.Item2);
    }

    /// <summary>
    /// Returns if two positions on a board have an unobstructed row, i.e. empty cells between the two positions. False if on different rows.
    /// </summary>
    /// <param name="a">1st position</param>
    /// <param name="b">2nd position</param>
    /// <returns>If the positions are connected by an unobstructed row</returns>
    public bool IsClearRow((int, int) a, (int, int) b,
        List<(PieceScript.PieceType, PieceScript.Side)> whitelist = null) {
        whitelist ??= new List<(PieceScript.PieceType, PieceScript.Side)>();

        // Check same row
        if (a.Item1 != b.Item1) {
            return false;
        }

        // Check if row is clear
        int mincol = Math.Min(a.Item2, b.Item2);
        int maxcol = Math.Max(a.Item2, b.Item2);
        if (whitelist.Count == 0) { // no whitelist items
            for (int i = mincol + 1; i < maxcol; i++) {
                if (!IsEmptySquare((a.Item1, i))) {
                    return false;
                }
                // if (!Object.ReferenceEquals(_board[a.Item1, i], emptySquare)) {
                //     return false;
                // }
            }
        } else { // whitelist pieces
            for (int i = mincol + 1; i < maxcol; i++) {
                if (!IsEmptySquare((a.Item1, i)) &&
                    !whitelist.Contains((GetPieceType((a.Item1, i)), GetPieceSide((a.Item1, i))))) {
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Returns if two positions on a board have an unobstructed column, i.e. empty cells between the two positions. False if on different columns.
    /// </summary>
    /// <param name="a">1st position</param>
    /// <param name="b">2nd position</param>
    /// <returns>If the positions are connected by an unobstructed column</returns>
    public bool IsClearColumn((int, int) a, (int, int) b,
        List<(PieceScript.PieceType, PieceScript.Side)> whitelist = null) {
        whitelist ??= new List<(PieceScript.PieceType, PieceScript.Side)>();

        // Check same column
        if (a.Item2 != b.Item2) {
            return false;
        }

        // Check if column is clear
        int minrow = Math.Min(a.Item1, b.Item1);
        int maxrow = Math.Max(a.Item1, b.Item1);
        if (whitelist.Count == 0) { // no whitelist items
            for (int i = minrow + 1; i < maxrow; i++) {
                if (!IsEmptySquare((i, a.Item2))) {
                    return false;
                }
            }
        } else {
            for (int i = minrow + 1; i < maxrow; i++) {
                if (!IsEmptySquare((i, a.Item2)) &&
                    !whitelist.Contains((GetPieceType((i, a.Item2)), GetPieceSide((i, a.Item2))))) {
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Returns if two positions on a board have an unobstructed diagonal, i.e. empty cells between the two positions. False if not on a diagonal.
    /// </summary>
    /// <param name="a">1st position</param>
    /// <param name="b">2nd position</param>
    /// <param name="whitelist">If the positions are connected by an unobstructed column</param>
    /// <returns></returns>
    public bool IsClearDiagonal((int, int) a, (int, int) b,
        List<(PieceScript.PieceType, PieceScript.Side)> whitelist = null) {
        whitelist ??= new List<(PieceScript.PieceType, PieceScript.Side)>();

        // Check on a diagonal
        if (Math.Abs(a.Item1 - b.Item1) != Math.Abs(a.Item2 - b.Item2)) {
            return false;
        }

        (int, int) direction = (Math.Sign(b.Item1 - a.Item1), Math.Sign(b.Item2 - a.Item2));
        int numdiag = Math.Abs(a.Item1 - b.Item1); // number of diagonal steps

        for (int i = 1; i < numdiag; i++) {
            (int, int) middlePosition = (a.Item1 + direction.Item1 * i, a.Item2 + direction.Item2 * i);

            if (whitelist.Count == 0) { // No whitelist
                if (!IsEmptySquare(middlePosition)) {
                    return false;
                }
            } else { // Whitelist
                if (!IsEmptySquare(middlePosition) &&
                    !whitelist.Contains((GetPieceType(middlePosition), GetPieceSide(middlePosition)))) {
                    return false;
                }
            }
        }

        return true;
    }


    /// <summary>
    /// Check if a square in a move direction is empty
    /// </summary>
    /// <param name="position">The initial coordinates of the piece</param>
    /// <param name="move">The coordinates of the square, relative to the piece's position</param>
    /// <returns>If the square is empty</returns>
    public bool IsEmptySquare((int, int) position, (int, int) move) {
        return GetPosition(position, move).GetComponent<PieceScript>().PieceSide == PieceScript.Side.None;
        // return Object.ReferenceEquals(GetPosition(position, move), emptySquare);
    }

    /// <summary>
    /// Check if a square at a position is empty.
    /// </summary>
    /// <param name="position">The coordinates of the square</param>
    /// <returns>If the square is empty</returns>
    public bool IsEmptySquare((int, int) position) {
        return GetPosition(position).GetComponent<PieceScript>().PieceSide == PieceScript.Side.None;
        // return Object.ReferenceEquals(GetPosition(position), emptySquare);
    }

    public bool IsEmptySquare(GameObject pieceGameObject) {
        return pieceGameObject.GetComponent<PieceScript>().PieceSide == PieceScript.Side.None;
    }


    public bool IsOutsideBoard(int row, int col) {
        return row >= boardSize || row < 0 || col >= boardSize || col < 0;
    }

    public bool IsOutsideBoard((int, int) position) {
        return position.Item1 >= boardSize || position.Item1 < 0 ||
               position.Item2 >= boardSize || position.Item2 < 0;
    }

    public bool IsOnEdge((int, int) position) {
        return !IsOutsideBoard(position) &&
               (position.Item1 == 0 || position.Item2 == 0 ||
                position.Item1 == boardSize - 1 || position.Item2 == boardSize - 1);
    }

    public bool IsOnFarEdge((int, int) position, PieceScript.Side pieceSide) {
        return (pieceSide == PieceScript.Side.White && position.Item1 == boardSize - 1) ||
               (pieceSide == PieceScript.Side.Black && position.Item1 == 0);
    }

    public bool IsSameParity((int, int) positionA, (int, int) positionB) {
        return (Math.Abs(positionA.Item1 - positionB.Item1) + Math.Abs(positionA.Item2 + positionB.Item2)) % 2 == 0;
    }

    /// <summary>
    /// Given a position and a move, return if that piece is on a different side than the side provided
    /// </summary>
    /// <param name="position">Initial position of piece</param>
    /// <param name="move">Change in piece position to get to other piece</param>
    /// <param name="side">This piece's side</param>
    /// <returns>If piece at position+move is an enemy of side</returns>
    public bool IsEnemy((int, int) position, (int, int) move, PieceScript.Side side) {
        if (_board[position.Item1 + move.Item1, position.Item2 + move.Item2].GetComponent<PieceScript>().PieceSide ==
            PieceScript.Side.None) {
            throw new NotImplementedException("IsEnemy cannot take None piecetype");
        }

        return _board[position.Item1 + move.Item1, position.Item2 + move.Item2].GetComponent<PieceScript>().PieceSide !=
               side;
    }

    /// <summary>
    /// Given a position, return if that piece is on a different side than the side provided
    /// </summary>
    /// <param name="position">Position of piece in question</param>
    /// <param name="side">This piece's side</param>
    /// <returns>If piece at position is an enemy of side</returns>
    public bool IsEnemy((int, int) position, PieceScript.Side side) {
        if (_board[position.Item1, position.Item2].GetComponent<PieceScript>().PieceSide == PieceScript.Side.None) {
            throw new NotImplementedException("IsEnemy cannot take None piecetype");
        }

        return _board[position.Item1, position.Item2].GetComponent<PieceScript>().PieceSide != side;
    }

    /// <summary>
    /// Checks if a piece is protected against a particular piece with a particular move. If yes, also provide which GameObject(s) are being blocked.
    /// </summary>
    /// <param name="defendingPosition">The defender's position</param>
    /// <param name="moveName">The move evaluated</param>
    /// <returns>If the piece is protected against that move, and a list of GameObjects that are blocked</returns>
    public (bool, List<GameObject>) IsProtectedAgainst((int, int) defendingPosition, string moveName) {
        bool isProtected =
            GetPosition(defendingPosition).GetComponent<PieceScript>().ProtectedBy.Select(s => s.Item3)
                .Contains(moveName);
        if (isProtected) {
            return (true, GetPosition(defendingPosition).GetComponent<PieceScript>().ProtectedBy
                        .FindAll(s => s.Item3 == moveName)
                        .Select(s => s.Item2)
                        .ToList());
        }

        return (false, new List<GameObject>());

        // if (isProtected && save) {
        // FinalProtectedPositions.Add((position, moveName));
        // }
        // return isProtected;
    }


    public bool InBlockList(List<GameObject> blockList, GameObject attackingPiece) {
        if (blockList.All(s => s is null)) {
            return true;
        }

        return blockList.Contains(attackingPiece);
    }


    public PieceScript.PieceType GetPieceType((int, int) position, (int, int) move) {
        return GetPosition(position, move).GetComponent<PieceScript>().Type;
    }

    public PieceScript.PieceType GetPieceType((int, int) position) {
        return GetPosition(position).GetComponent<PieceScript>().Type;
    }

    // public PieceScript.Side GetPieceSide((int, int) position, (int, int) move) {
    //     return GetPosition(position, move).GetComponent<PieceScript>().PieceSide;
    // }

    public PieceScript.Side GetPieceSide((int, int) position) {
        PieceScript.Side side = GetPosition(position).GetComponent<PieceScript>().PieceSide;

        // // Potentially not needed
        // if (side == PieceScript.Side.None) {
        //     throw new NotImplementedException("Something called an empty square");
        // }

        return side;
    }

    public PieceScript.Side InvertSide(PieceScript.Side side) {
        if (side == PieceScript.Side.None) {
            throw new NotImplementedException("Can't invert none side");
        }

        return side == PieceScript.Side.White
            ? PieceScript.Side.Black
            : PieceScript.Side.White;
    }

    /// <summary>
    /// Get AttackList of target square
    /// </summary>
    public List<(GameObject, string)> GetAttackList((int, int) position, (int, int) move) {
        return GetPosition(position, move).GetComponent<PieceScript>().AttackedBy;
    }

    /// <summary>
    /// Get AttackList of target square
    /// </summary>
    public List<(GameObject, string)> GetAttackList((int, int) position) {
        return GetPosition(position).GetComponent<PieceScript>().AttackedBy;
    }

    /// <summary>
    /// Add position and move and return the sum
    /// </summary>
    /// <param name="position"></param>
    /// <param name="move"></param>
    /// <returns>Position + move</returns>
    public (int, int) AddMovePositions((int, int) position, (int, int) move) {
        return (position.Item1 + move.Item1, position.Item2 + move.Item2);
    }

    /// <summary>
    /// Get list of GameObjects in AttackList of target square
    /// </summary>
    public List<GameObject> GetAttackers((int, int) position, (int, int) move) {
        return GetAttackList(position, move).Select(_ => _.Item1).ToList();
    }

    /// <summary>
    /// Get list of GameObjects in AttackList of target square
    /// </summary>
    public List<GameObject> GetAttackers((int, int) position) {
        return GetAttackList(position).Select(_ => _.Item1).ToList();
    }

    /// <summary>
    /// Get list of strings of attacking moves in AttackList of target square
    /// </summary>
    public List<string> GetAttackingMoves((int, int) position, (int, int) move) {
        return GetAttackList(position, move).Select(_ => _.Item2).ToList().Distinct().ToList();
    }

    /// <summary>
    /// Get list of strings of attacking moves in AttackList of target square
    /// </summary>
    public List<string> GetAttackingMoves((int, int) position) {
        return GetAttackList(position).Select(_ => _.Item2).ToList().Distinct().ToList();
    }


    public int GetNumberPerformedMoves((int, int) position, (int, int) move) {
        return GetPosition(position, move).GetComponent<PieceScript>().MoveCounter;
    }

    public int GetNumberPerformedMoves((int, int) position) {
        return GetPosition(position).GetComponent<PieceScript>().MoveCounter;
    }

    public int GetNumberTurnsOnBoard((int, int) position, (int, int) move) {
        return GetPosition(position, move).GetComponent<PieceScript>().TurnsOnBoard;
    }

    public int GetNumberTurnsOnBoard((int, int) position) {
        return GetPosition(position).GetComponent<PieceScript>().TurnsOnBoard;
    }

    /// <summary>
    /// Returns shortest distance between 2 tuples, either in taxicab mode or diagonals-included mode.
    /// </summary>
    /// <param name="a">1st coordinates</param>
    /// <param name="b">2nd coordinates</param>
    /// <param name="taxicab">Whether to use taxicab distance</param>
    /// <returns></returns>
    public int Distance((int, int) a, (int, int) b, bool taxicab = true) {
        int xdelta = Math.Abs(a.Item1 - b.Item1);
        int ydelta = Math.Abs(a.Item2 - b.Item2);

        if (taxicab) {
            return xdelta + ydelta;
        } else {
            return Math.Min(xdelta, ydelta) + Math.Abs(xdelta - ydelta);
        }
    }


    /// <summary>
    /// Prints the board state into the Debug.Log console
    /// </summary>
    private void PrintBoard(bool showAttack = false, bool showProtect = false) {
        for (int i = boardSize - 1; i >= 0; i--) { // show rows backwards
            string line = "";
            for (int j = 0; j < boardSize; j++) {
                if (!IsEmptySquare((i, j))) {
                    line += $"[ {GetPieceType((i, j))}";
                    if (showAttack) {
                        line +=
                            $" ({String.Join(",", _board[i, j].GetComponent<PieceScript>().AttackedBy.Select(a => a.Item1.GetComponent<PieceScript>().Type + ":" + a.Item2))})";
                    }

                    if (showProtect) {
                        line +=
                            $" <{String.Join(",", _board[i, j].GetComponent<PieceScript>().ProtectedBy.Select(a => a.Item1.GetComponent<PieceScript>().Type + ":" + a.Item2))}>";
                    }

                    line += " ]\t";
                } else {
                    line += "[  NULL  ]\t";
                }
            }

            Debug.Log(line);
        }

        Debug.Log("------");
    }

    // public void DebugLogEnumerable<T>([NotNull] IEnumerable<T> enumerable) {
    //     if (enumerable == null) throw new ArgumentNullException(nameof(enumerable));
    //     Debug.Log(String.Join(",", enumerable));
    // }
}