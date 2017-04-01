using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;


struct DifficultySpec {
    public int width;
    public int height;
    public int mineCount;
    public int totalCells;
    public int emptyCells;

    public DifficultySpec(int width, int height, int mineCount) {
        this.width = width;
        this.height = height;
        this.mineCount = mineCount;
        this.totalCells = width * height;
        this.emptyCells = this.totalCells - mineCount;
    }
}

public class Board : MonoBehaviour {
    public GameManager gameManager;
    [SerializeField]
    Timer timer;
    public static Board Instance;
    public int revealedCells;  // count used for victory check

    // standard difficulty specifications
    static DifficultySpec DIFFICULTY_BEGINNER = new DifficultySpec(8, 8, 10);
    static DifficultySpec DIFFICULTY_INTERMEDIATE = new DifficultySpec(16, 16, 40);
    static DifficultySpec DIFFICULTY_EXPERT = new DifficultySpec(30, 16, 99);
    // enum used for storing difficulty choice in PlayerPrefs as int
    public enum Difficulty { Beginner, Intermediate, Expert };
    Dictionary<Difficulty, DifficultySpec> difficultyMap = new Dictionary<Difficulty, DifficultySpec>() {
        {Difficulty.Beginner, DIFFICULTY_BEGINNER},
        {Difficulty.Intermediate, DIFFICULTY_INTERMEDIATE},
        {Difficulty.Expert, DIFFICULTY_EXPERT},
    };
    DifficultySpec currentDifficulty;

    // game object references
    Cell[,] cells;
    [SerializeField]
    GameObject cellPrefab;
    [SerializeField]
    RectTransform cellContainer;
    [SerializeField]
    Image faceButtonImage;
    [SerializeField]
    Image[] mineCountDigits = new Image[3];

    bool _freshBoard;  // whether the first cell has been clicked
    public bool FreshBoard {
        get { return _freshBoard; }
        set {
            _freshBoard = value;
            if (!_freshBoard) {
                timer.StartTimer();
            }
            else {
                timer.ResetTimer();
                SetFaceImage("face_smile");
            }
        }
    }
    public bool GameStarted {
        get { return !FreshBoard; }
    }

    void Start() {
        Instance = this;
        CreateBoard();
        InitializeBoard();
        UpdateMineCount();
    }

    public void PlaceMines() {
        foreach (Cell cell in cells) {
            cell.ContainsMine = false;
        }

        int numCells = currentDifficulty.width * currentDifficulty.height;
        List<int> selectedCells = Utilities.GetNRandomIndices(numCells, currentDifficulty.mineCount);
        foreach (int flatBoardIndex in selectedCells) {
            int xIndex = flatBoardIndex % currentDifficulty.width;
            int yIndex = flatBoardIndex / currentDifficulty.width;
            cells[yIndex, xIndex].ContainsMine = true;
        }

        foreach (Cell cell in cells) {
            cell.adjacentMineCount = GetAdjacentMineCount(cell);
        }
    }

    void CreateBoard() {
        Difficulty difficultyEnum = (Difficulty)PlayerPrefs.GetInt("DifficultyEnum", (int)Difficulty.Expert);
        currentDifficulty = difficultyMap[difficultyEnum];
        cells = new Cell[currentDifficulty.height, currentDifficulty.width];

        Vector2 cellContainerSize = new Vector2(currentDifficulty.width * 16, currentDifficulty.height * 16);
        cellContainer.sizeDelta = cellContainerSize;
        Vector2 boardSize = new Vector2(32, 64) + cellContainerSize;
        ((RectTransform)transform).sizeDelta = boardSize;
        GridLayoutGroup layoutGroup = cellContainer.GetComponent<GridLayoutGroup>();
        layoutGroup.constraintCount = currentDifficulty.width;

        foreach (Transform child in cellContainer.transform) {
            Destroy(child.gameObject);
        }

        for (int dy = 0; dy < currentDifficulty.height; dy++) {
            for (int dx = 0; dx < currentDifficulty.width; dx++) {
                Cell cell = ((GameObject) Instantiate(cellPrefab, cellContainer)).GetComponent<Cell>();
                cell.board = this;
                cell.transform.localScale = Vector3.one;
                cell.gameObject.name = "cell_" + dx + "_" + dy;
                cell.xPosition = dx;
                cell.yPosition = dy;
                cells[dy, dx] = cell;
            }
        }
    }

    void InitializeBoard() {
        foreach (Cell cell in cells) {
            cell.Reset();
        }
        PlaceMines();
        FreshBoard = true;
        gameManager.gameOver = false;
        revealedCells = 0;
        UpdateMineCount();
    }

    public void RestartGame() {
        timer.StopTimer();
        InitializeBoard();
    }

    void Update() {
        HandleInput();
    }

    void HandleInput() {
        if (!gameManager.gameOver && Input.GetMouseButtonUp(0)) {
            SetFaceImage("face_smile");
        }

        // restart with R key
        if (Input.GetKeyUp(KeyCode.R)) {
            Debug.Log("restarting game due to \"R\" press");
            RestartGame();
        }

        // exit to main menu with M key
        if (Input.GetKeyUp(KeyCode.M)) {
            gameManager.GoToMenu();
        }

        if (Config.ENABLE_DEBUG_COMMANDS) {
            if (Input.GetKeyUp(KeyCode.P)) {
                if (FreshBoard) {
                    Debug.Log("re-placing mines due to \"P\" press");
                    PlaceMines();
                }
                else {
                    Debug.Log("cannot re-place mines after game has started");
                }
            }
        }
    }

    public void CheckIfGameWon() {
        if (revealedCells == currentDifficulty.emptyCells) {
            DoGameVictory();
        }
    }

    public void DoGameOver() {
        gameManager.gameOver = true;
        timer.StopTimer();
        foreach (Cell cell in cells) {
            cell.DoGameOverReveal();
        }
        SetFaceImage("face_dead");
    }

    void DoGameVictory() {
        Debug.Log(string.Format("You win! Time: {0:0.00} seconds.", timer.GameTime));
        gameManager.gameOver = true;
        timer.StopTimer();
        foreach (Cell cell in cells) {
            cell.DoGameOverReveal();
        }
        SetFaceImage("face_cool");
    }

    public void UpdateMineCount() {
        int mineCount = currentDifficulty.mineCount - GetFlagCount();
        mineCount = Mathf.Max(0, mineCount);
        for (int i = 0; i < mineCountDigits.Length; i++) {
            int digitValue = (mineCount / (int)Mathf.Pow(10, i)) % 10;
            int digitIndex = mineCountDigits.Length - (i + 1);
            mineCountDigits[digitIndex].sprite = Sprites.spritesDict["clock_" + digitValue];
        }
    }

    List<Cell> GetAdjacentCells(Cell centerCell, bool includeFlagged = true) {
        int cx = centerCell.xPosition;
        int cy = centerCell.yPosition;

        // TODO // could probably be optimized
        List<Cell> adjacentCells = new List<Cell>();
        for (int y = cy - 1; y <= cy + 1; y++) {
            for (int x = cx - 1; x <= cx + 1; x++) {
                // make sure cell at coordinates exists and isn't the input cell
                bool cellIsSelf = (x == cx && y == cy);
                bool cellInBounds = (x >= 0 && x < currentDifficulty.width &&
                                     y >= 0 && y < currentDifficulty.height);
                if (cellInBounds && !cellIsSelf) {
                    Cell adjacentCell = cells[y, x];
                    if (adjacentCell.Flagged && !includeFlagged) {
                        continue;
                    }
                    adjacentCells.Add(adjacentCell);
                }
            }
        }

        return adjacentCells;
    }

    int GetAdjacentMineCount(Cell cell) {
        int numAdjacentMines = 0;
        List<Cell> adjacentCells = GetAdjacentCells(cell);
        foreach (Cell adjacentCell in adjacentCells) {
            if (adjacentCell.ContainsMine) {
                numAdjacentMines++;
            }
        }
        return numAdjacentMines;
    }

    int GetAdjacentFlagCount(Cell cell) {
        int numAdjacentFlags = 0;
        List<Cell> adjacentCells = GetAdjacentCells(cell);
        foreach (Cell adjacentCell in adjacentCells) {
            if (adjacentCell.Flagged) {
                numAdjacentFlags++;
            }
        }
        return numAdjacentFlags;
    }

    public void RevealAdjacentUnflaggedCells(Cell cell) {
        if (cell.adjacentMineCount == GetAdjacentFlagCount(cell)) {
            List<Cell> adjacentUnflaggedCells = GetAdjacentCells(cell, includeFlagged: false);
            // special pass that will only detonate mines
            // prevents revealing safe cells before detonation
            foreach (Cell adjacentCell in adjacentUnflaggedCells) {
                if (adjacentCell.ContainsMine) {
                    adjacentCell.Reveal();
                    return;
                }
            }
            foreach (Cell adjacentCell in adjacentUnflaggedCells) {
                if (adjacentCell.Clickable) {
                    adjacentCell.Reveal();
                }
            }
        }
    }

    int GetFlagCount() {
        int count = 0;
        foreach (Cell cell in cells) {
            if (cell.Flagged) {
                count += 1;
            }
        }
        return count;
    }

    public void SetFaceImage(string name) {
        if (Sprites.spritesDict.ContainsKey(name)) {
            faceButtonImage.sprite = Sprites.spritesDict[name];
        }
    }
}
