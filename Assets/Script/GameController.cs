using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public static GameController instance;
    public Transform gameOverPanel;
    Block[,] grid;
    public Block[] blocks;
    public Transform blockPrefab;
    public Transform board;
    public BlockInformation[] currentBlockChoose = new BlockInformation[2];
    public LineRenderer lineRenderer;  // Kéo thả LineRenderer vào đây
    public TextMeshProUGUI lifeText;
    public TextMeshProUGUI levelText;
    public Slider timerSlider;
    public AudioSource source;
    public AudioClip click;
    public AudioClip takenBlock;
    public AudioClip rightAnswer;
    public AudioClip wrongAnswer;
    public AudioClip nextLevel;
    public AudioClip gameover;
    public int gridSizeX = 18;
    public int gridSizeY = 10;
    public bool allowMoveBlockAfterPathRemove;
    public bool allowMoveBlockAfterRemove;
    public bool MoveBlockVertical;
    public bool MoveBlockHorizontal;
    public bool MoveBlockPoint;
    public static float timer;
    public static int life;


    [Min(1)]public int level;
    private void Awake()
    {
        grid = new Block[gridSizeX, gridSizeY];
        currentBlockChoose = new BlockInformation[2];
        timer = 600f;
        life = 10;
        UpdateLife(life);UpdateTimer(timer);
        if(instance == null)
        {
            instance = this;
        }
    }
    private void Update()
    {
        Timer();
    }
    private void LevelText(int level)
    {
        levelText.text = level+"";
    }
    void Timer()
    {
        if(timer > 0)
        {
            timer -= Time.deltaTime;
            UpdateTimer(timer);
        } else
        {
            GameOver();
        }
    }
    public void UpdateTimer(float timer) {
        timerSlider.value = timer;
    }
    public void UpdateLife(int life)
    {
        lifeText.text = life + "";
        if(life != 0 ) lifeText.color = Color.white;
        else lifeText.color = Color.red;
    }
    // Start is called before the first frame update
    void Start()
    {
        CreateBoardLogic();
        CreateBoardUI();
        ChooseLevel(level);
    }

    void CreateBoardUI()
    {
        for (int i = 1; i < gridSizeX - 1; i++)
        {
            for (int j = 1; j < gridSizeY - 1; j++)
            {
                Transform block = Instantiate(blockPrefab, board);
                block.position = new Vector3(i * 0.4f, j * 0.48f);
                UpdateBlockInfomation(block, i, j, grid[i, j]);
            }
        }
    }
    void UpdateBlockInfomation(Transform blockTransform, int x, int y, Block block)
    {
        blockTransform.GetComponent<BlockInformation>().x = x;
        blockTransform.GetComponent<BlockInformation>().y = y;
        blockTransform.GetComponent<BlockInformation>().block = block;
        blockTransform.GetComponent<SpriteRenderer>().sprite = block.sprite;

    }

    Block RandomBlock()
    {
        int randomIndex = Random.Range(0, blocks.Count());
        return blocks[randomIndex];
    }

    bool IsSameBlock(Block block1, Block block2)
    {
        return block1.type == block2.type;
    }
    bool IsSamePosition(int x1, int y1, int x2, int y2)
    {
        return x1 == x2 && y1 == y2;
    }
    bool IsPositionNull(int x, int y)
    {
        if (x < 0 || x >= gridSizeX || y < 0 || y >= gridSizeY) return false;
        return grid[x, y] == null;
    }


    public void CheckPairBlockChoosen()
    {
        if (IsSameBlock(currentBlockChoose[0].block, currentBlockChoose[1].block) && CanConnect(currentBlockChoose[0].x, currentBlockChoose[0].y, currentBlockChoose[1].x, currentBlockChoose[1].y))
        {
            Debug.Log($"Remove pair Blocks at pos[{currentBlockChoose[0].x},{currentBlockChoose[0].y}] and [{currentBlockChoose[1].x},{currentBlockChoose[1].y}]");
            RemoveBlock(currentBlockChoose[0]);
            RemoveBlock(currentBlockChoose[1]);
            PlayAudio(rightAnswer);
        }else
        {
            currentBlockChoose[0].PaintColor(Color.white);
            currentBlockChoose[1].PaintColor(Color.white);
            PlayAudio(wrongAnswer);
        }
        ResetCurrentBlockChoosen();
    }
    void ResetCurrentBlockChoosen()
    {
        currentBlockChoose[0] = null;
        currentBlockChoose[1] = null;

    }
    void RemoveBlock(BlockInformation block)
    {
        Destroy(block.gameObject);
        grid[block.x, block.y] = null;
    }
    // Kiểm tra đường thẳng dọc
    bool CheckLineVertical(int x, int y1, int y2)
    {
        int minY = Mathf.Min(y1, y2);
        int maxY = Mathf.Max(y1, y2);
        if (minY == maxY) return false;

        for (int i = minY + 1; i < maxY; i++)
        {
            if (!IsPositionNull(x, i)) return false;
        }
        return true;
    }
    // Kiểm tra đường thẳng ngang
    bool CheckLineHorizontal(int x1, int x2, int y)
    {
        int minX = Mathf.Min(x1, x2);
        int maxX = Mathf.Max(x1, x2);
        if (minX == maxX) return false;

        for (int i = minX + 1; i < maxX; i++)
        {
            if (!IsPositionNull(i, y)) return false;
        }
        return true;
    }
    // Kiểm tra đường gấp khúc 1 góc (L-shape)
    bool CheckLShape(int x1, int y1, int x2, int y2)
    {
        // Điểm đổi hướng 1: (x1, y2)
        if (IsPositionNull(x1, y2) &&
            CheckLineHorizontal(x1, x2, y2) &&  // Chỉ 3 tham số
            CheckLineVertical(x1, y1, y2))      // Sửa lại thứ tự tham số cho đúng
        {
            return true;
        }

        // Điểm đổi hướng 2: (x2, y1)
        if (IsPositionNull(x2, y1) &&
            CheckLineHorizontal(x1, x2, y1) &&  // Chỉ 3 tham số
            CheckLineVertical(x2, y1, y2))      // Sửa lại thứ tự tham số cho đúng
        {
            return true;
        }

        return false;
    }
    
    bool CheckUShape(int x1, int y1, int x2, int y2)
    {
        int minX = Mathf.Min(x1, x2);
        int maxX = Mathf.Max(x1, x2);

        // 🟢 Duyệt từ trái qua phải (x tăng dần)
        for (int x = minX; x >= 0; x--)  // 🔄 Sửa: minX - 1 để tránh lặp lại điểm gốc
        {
            if (IsPositionNull(x, y1) && IsPositionNull(x, y2) &&
                CheckLineVertical(x, y1, y2) &&
                CheckLineHorizontal(x1, x, y1) &&
                CheckLineHorizontal(x2, x, y2))
            {
                return true;
            }
        }

        // 🟢 Duyệt từ phải qua trái (x giảm dần)
        for (int x = minX; x < gridSizeX; x++)  // 🔄 Sửa: maxX + 1 để tránh lặp lại điểm gốc
        {
            if (IsPositionNull(x, y1) && IsPositionNull(x, y2) &&
                CheckLineVertical(x, y1, y2) &&
                CheckLineHorizontal(x1, x, y1) &&
                CheckLineHorizontal(x2, x, y2))
            {
                return true;
            }
        }

        int minY = Mathf.Min(y1, y2);
        int maxY = Mathf.Max(y1, y2);

        // 🟢 Duyệt từ trên xuống dưới (y giảm dần)
        for (int y = minY; y >= 0; y--)  // 🔄 Sửa: minY - 1 để tránh lặp lại điểm gốc
        {
            if (IsPositionNull(x1, y) && IsPositionNull(x2, y) &&
                CheckLineHorizontal(x1, x2, y) &&
                CheckLineVertical(x1, y1, y) &&
                CheckLineVertical(x2, y2, y))
            {
                return true;
            }
        }

        // 🟢 Duyệt từ dưới lên trên (y tăng dần)
        for (int y = minY; y < gridSizeY; y++)  // 🔄 Sửa: maxY + 1 để tránh lặp lại điểm gốc
        {
            if (IsPositionNull(x1, y) && IsPositionNull(x2, y) &&
                CheckLineHorizontal(x1, x2, y) &&
                CheckLineVertical(x1, y1, y) &&
                CheckLineVertical(x2, y2, y))
            {
                return true;
            }
        }

        return false;
    }


    // Kiểm tra toàn bộ các trường hợp
    IEnumerator DrawPath(List<Vector3> pathPoints)
    {
        lineRenderer.enabled = true;
        allowMoveBlockAfterPathRemove = false;
        lineRenderer.positionCount = pathPoints.Count;
        for (int i = 0; i < pathPoints.Count; i++)
        {
            lineRenderer.SetPosition(i, pathPoints[i]);
        }
        yield return new WaitForSeconds(0.2f);
        lineRenderer.enabled = false;
        allowMoveBlockAfterPathRemove = true;
        GetRewardOnDeleteBlock();
        CheckWin();
    }
    bool CanConnect(int x1, int y1, int x2, int y2)
    {
        if (x1 == x2 && y1 == y2) return false;

        List<Vector3> pathPoints = new List<Vector3>();

        // 🟢 Kiểm tra cùng cột
        if (x1 == x2 && CheckLineVertical(x1, y1, y2))
        {
            pathPoints.Add(GetWorldPosition(x1, y1));
            pathPoints.Add(GetWorldPosition(x2, y2));
            StartCoroutine(DrawPath(pathPoints));
            return true;
        }

        // 🟢 Kiểm tra cùng hàng
        if (y1 == y2 && CheckLineHorizontal(x1, x2, y1))
        {
            pathPoints.Add(GetWorldPosition(x1, y1));
            pathPoints.Add(GetWorldPosition(x2, y2));
            StartCoroutine(DrawPath(pathPoints));
            return true;
        }

        // 🟢 Đường L-shape
        if (CheckLShape(x1, y1, x2, y2))
        {
            pathPoints.Add(GetWorldPosition(x1, y1));

            if (IsPositionNull(x1, y2) && CheckLineVertical(x1, y1, y2) && CheckLineHorizontal(x1, x2, y2))
            {
                pathPoints.Add(GetWorldPosition(x1, y2));  // Điểm đổi hướng 1
            }
            else if (IsPositionNull(x2, y1) && CheckLineHorizontal(x1, x2, y1) && CheckLineVertical(x2, y1, y2))
            {
                pathPoints.Add(GetWorldPosition(x2, y1));  // Điểm đổi hướng 2
            }

            pathPoints.Add(GetWorldPosition(x2, y2));
            StartCoroutine(DrawPath(pathPoints));
            return true;
        }

        // 🟢 Đường U-shape
        if (CheckUShape(x1, y1, x2, y2))
        {
            pathPoints.Add(GetWorldPosition(x1, y1));

            bool pathFound = false;

            // 🟢 Duyệt từ trái sang phải
            for (int x = x1 + 1; x < gridSizeX && !pathFound; x++)
            {
                if (IsPositionNull(x, y1) && IsPositionNull(x, y2) &&
                    CheckLineVertical(x, y1, y2) &&
                    CheckLineHorizontal(x1, x, y1) &&
                    CheckLineHorizontal(x2, x, y2))
                {
                    pathPoints.Add(GetWorldPosition(x, y1));  // Điểm đổi hướng 1
                    pathPoints.Add(GetWorldPosition(x, y2));  // Điểm đổi hướng 2
                    pathFound = true;
                    break;
                }
            }

            // 🟢 Duyệt từ phải sang trái
            for (int x = x1 - 1; x >= 0 && !pathFound; x--)
            {
                if (IsPositionNull(x, y1) && IsPositionNull(x, y2) &&
                    CheckLineVertical(x, y1, y2) &&
                    CheckLineHorizontal(x1, x, y1) &&
                    CheckLineHorizontal(x2, x, y2))
                {
                    pathPoints.Add(GetWorldPosition(x, y1));
                    pathPoints.Add(GetWorldPosition(x, y2));
                    pathFound = true;
                    break;
                }
            }

            // 🟢 Duyệt từ trên xuống dưới
            for (int y = y1 + 1; y < gridSizeY && !pathFound; y++)
            {
                if (IsPositionNull(x1, y) && IsPositionNull(x2, y) &&
                    CheckLineHorizontal(x1, x2, y) &&
                    CheckLineVertical(x1, y1, y) &&
                    CheckLineVertical(x2, y2, y))
                {
                    pathPoints.Add(GetWorldPosition(x1, y));  // Điểm đổi hướng 1
                    pathPoints.Add(GetWorldPosition(x2, y));  // Điểm đổi hướng 2
                    pathFound = true;
                    break;
                }
            }

            // 🟢 Duyệt từ dưới lên trên
            for (int y = y1 - 1; y >= 0 && !pathFound; y--)
            {
                if (IsPositionNull(x1, y) && IsPositionNull(x2, y) &&
                    CheckLineHorizontal(x1, x2, y) &&
                    CheckLineVertical(x1, y1, y) &&
                    CheckLineVertical(x2, y2, y))
                {
                    pathPoints.Add(GetWorldPosition(x1, y));  // Điểm đổi hướng 1
                    pathPoints.Add(GetWorldPosition(x2, y));  // Điểm đổi hướng 2
                    pathFound = true;
                    break;
                }
            }

            pathPoints.Add(GetWorldPosition(x2, y2));

            if (pathFound)
            {
                StartCoroutine(DrawPath(pathPoints));
                return true;
            }
        }

        return false;
    }



    private float cellWidth = 0.4f;
    private float cellHeight = 0.48f;

    Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(x * cellWidth, y * cellHeight);
    }
    [ContextMenu("MoveBlockByVerticalLine")]
    public void MoveBlockByVerticalLine()
    {
        MoveBlockByVerticalLine(0);
    }
    void MoveBlockByVerticalLine(float colIndex)
    {
        for (int y = 1; y < gridSizeY - 1; y++)
        {
            for (int x = 1; x < gridSizeX - 1; x++)
            {
                if (x > colIndex+0.5f) MoveBlock(x, y, x - 1, y);
                if (x < colIndex-0.5f) MoveBlock(x, y, x + 1, y);
            }
        }
    }
    void MoveBlockByHorizontalLine(float colIndex)
    {
        for (int y = 1; y < gridSizeY - 1; y++)
        {
            for (int x = 1; x < gridSizeX - 1; x++)
            {
                if (y > colIndex) MoveBlock(x, y, x, y-1);
                if (y < colIndex) MoveBlock(x, y, x, y+1);
            }
        }
    }

    void MoveBlockByPointLine(Vector2 point)
    {
        for (int y = 1; y < gridSizeY - 1; y++)
        {
            for (int x = 1; x < gridSizeX - 1; x++)
            {
                if (x > point.x + 0.5f) MoveBlock(x, y, x - 1, y);
                if (x < point.x - 0.5f) MoveBlock(x, y, x + 1, y);
                if (y > point.y + 0.5f) MoveBlock(x, y, x, y - 1);
                if (y < point.y - 0.5f) MoveBlock(x, y, x, y + 1);
            }
        }
    }

    void MoveBlock(int x, int y,int xTarget,int yTarget)
    {
        try
        {
            if (grid[x, y] == null) return;
            if (xTarget == 0 || yTarget == 0 || xTarget == gridSizeX - 1 || yTarget == gridSizeY - 1) return;
            if (grid[xTarget, yTarget] != null) return;
            grid[xTarget, yTarget] = grid[x, y];
            grid[x, y] = null;
            var blockInfomations = GameObject.FindObjectsByType<BlockInformation>(FindObjectsSortMode.None);
            var blockInfomation = blockInfomations.FirstOrDefault(b => b.x == x && b.y == y);
            if (blockInfomation == null) return;
            blockInfomation.transform.position = GetWorldPosition(xTarget, yTarget);
            blockInfomation.x = xTarget;
            blockInfomation.y = yTarget;
        }
        catch
        {
            Debug.LogError("Error at [{x},{y}] , [{xTarget},{yTarget}]");
            Debug.LogError(grid[x, y]);
            Debug.LogError(grid[xTarget, yTarget]);
        }
        
    }
    public float sliceVerticalIndex;
    public float sliceHorizontalIndex;
    public Vector2 slicePointIndex;
    private void FixedUpdate()
    {
        LevelText(level);
        if (allowMoveBlockAfterPathRemove && allowMoveBlockAfterRemove) {

            if(MoveBlockVertical) MoveBlockByVerticalLine(sliceVerticalIndex);
            if (MoveBlockHorizontal) MoveBlockByHorizontalLine(sliceHorizontalIndex);
            if (MoveBlockPoint) MoveBlockByPointLine(slicePointIndex);
        }
    }

    void ChooseLevel(int level)
    {
        ResetMoveBlock();
        switch (level)
        {
            case 1 : break;
            case 2 : { allowMoveBlockAfterRemove = true; MoveBlockHorizontal = true; sliceHorizontalIndex = 1; break; }
            case 3 : { allowMoveBlockAfterRemove = true; MoveBlockHorizontal = true; sliceHorizontalIndex = 8; break; }
            case 4 : { allowMoveBlockAfterRemove = true; MoveBlockHorizontal = true; sliceHorizontalIndex = 4f; break; }
            case 5: { allowMoveBlockAfterRemove = true; MoveBlockVertical = true; sliceVerticalIndex = 1; break; }
            case 6: { allowMoveBlockAfterRemove = true; MoveBlockVertical = true; sliceVerticalIndex = 17; break; }
            case 7: { allowMoveBlockAfterRemove = true; MoveBlockVertical = true; sliceVerticalIndex = 8f; break; }
            case 8: { allowMoveBlockAfterRemove = true; MoveBlockPoint = true; slicePointIndex = new Vector2(8f,4f); break; }
            default : { ChooseRandomMode(); break; }
        }
    }
    void ChooseRandomMode()
    {
        ResetMoveBlock(); 
        allowMoveBlockAfterRemove = true;

        int randomMode = Random.Range(1, 4);
        switch (randomMode)
        {
            case 1: { MoveBlockHorizontal = true; sliceHorizontalIndex = Random.Range(1, 17); break; }
            case 2: { MoveBlockVertical = true; sliceVerticalIndex = Random.Range(1, 17); break; }
            case 3: { MoveBlockPoint = true; slicePointIndex = new Vector2(Random.Range(1, 17), Random.Range(1, 9)); break; }
        }
    }
    void ResetMoveBlock()
    {
        allowMoveBlockAfterRemove = false;
        MoveBlockVertical = false;
        MoveBlockHorizontal = false;
        MoveBlockPoint = false;
    }
    void CreateBoardLogic()
    {
        // Tạo danh sách tất cả vị trí hợp lệ trong grid (trừ viền ngoài)
        List<Vector2Int> emptyPositions = new List<Vector2Int>();
        for (int i = 1; i < gridSizeX - 1; i++)
        {
            for (int j = 1; j < gridSizeY - 1; j++)
            {
                emptyPositions.Add(new Vector2Int(i, j));
            }
        }

        // Trộn ngẫu nhiên danh sách vị trí trống
        emptyPositions = emptyPositions.OrderBy(pos => Random.value).ToList();

        // Lần lượt lấy từng cặp vị trí để đặt Block
        for (int i = 0; i < emptyPositions.Count; i += 2)
        {
            if (i + 1 >= emptyPositions.Count) break;  // Nếu số ô trống lẻ, bỏ ô cuối

            var randomBlock = RandomBlock();
            var pos1 = emptyPositions[i];
            var pos2 = emptyPositions[i + 1];

            grid[pos1.x, pos1.y] = randomBlock;
            grid[pos2.x, pos2.y] = randomBlock;
        }
    }
    [ContextMenu("Change Living Blocks")]
    public void ChangeLivingBlocks()
    {
        PlayAudio(nextLevel);
        ClearBoard();
        List<Vector2Int> shufferedPositions = new List<Vector2Int>();
        for (int i = 1; i < gridSizeX-1; i++)
        {
            for (int j = 1; j < gridSizeY-1; j++)
            {
                if (grid[i, j] == null) continue;
                shufferedPositions.Add(new Vector2Int(i, j));
            }
        }
        shufferedPositions = shufferedPositions.OrderBy(pos => Random.value).ToList();
        for (int i = 0;i < shufferedPositions.Count; i += 2)
        {
            var randomBLock = RandomBlock();
            grid[shufferedPositions[i].x,shufferedPositions[i].y] = randomBLock;
            grid[shufferedPositions[i+1].x, shufferedPositions[i+1].y] = randomBLock;
        }
        var blockInfomations = board.GetComponentsInChildren<BlockInformation>();
        for (int i = 1; i < gridSizeX-1; i++)
        {
            for (int j = 1; j < gridSizeY-1; j++)
            {
                if (grid[i, j] == null) continue;
                foreach(var block in blockInfomations)
                {
                    if (IsBlockInformationNull(block))
                    {
                        block.x = i; block.y = j;
                        block.block = grid[i, j];
                        UpdateBlockInfomation(block.transform, block.x, block.y, block.block);
                        block.transform.position = GetWorldPosition(block.x, block.y);
                        break;
                    }
                }
            }
        }
    }
    void GameOver()
    {
        Debug.Log("Game Over");
        PlayAudio(gameover); 
        gameOverPanel.gameObject.SetActive(true);
    }
    public void ClearBoard()
    {
        BlockInformation[] blocksInformation = board.GetComponentsInChildren<BlockInformation>();
        foreach(var blockInformation in blocksInformation)
        {
            blockInformation.x = -1;
            blockInformation.y = -1;
            blockInformation.block = null;
            blockInformation.GetComponent<SpriteRenderer>().sprite = null;
        }
    }
    public void CheckWin()
    {
        if (IsBoardEmpty())
        {
            PlayAudio(nextLevel);
            NextLevel();
            GetRewardOnWin();
            Debug.Log("You win !! Next level");
        }
    }
    public void RestartGame()
    {
        PlayAudio(nextLevel);
        ResetTimer();
        level = 1;
        ClearBoardGameObject();
        ResetBoardLogic();
        CreateBoardLogic();
        CreateBoardUI();
        ChooseLevel(level);
    }
    void NextLevel()
    {
        ResetTimer();
        level++;
        ClearBoardGameObject();
        ResetBoardLogic();
        CreateBoardLogic();
        CreateBoardUI();
        ChooseLevel(level);
    }
    void ResetTimer()
    {
        timer = 600f;
        UpdateTimer(timer);
    }
    void GetRewardOnDeleteBlock()
    {
        timer += 2f;
    }
    void GetRewardOnWin()
    {
        life++;
        UpdateLife(life);
    }
    public void ClearBoardGameObject()
    {
        for (int i = board.childCount - 1; i >= 0; i--)
        {
            Destroy(board.GetChild(i).gameObject);
        }
    }
    public bool IsBoardEmpty()
    {
        if (board.childCount > 0) return false;
        /*for(int i = 1; i < gridSizeX - 2; i++)
        {
            for (int j = 1; j < gridSizeY - 2; j++)
            {
                if (grid[i, j] != null) return false;
            }
        }*/
        return true;
    }
    void ResetBoardLogic()
    {
        for (int j = 0; j < gridSizeY; j++)
        {
            for (int i = 0; i < gridSizeX; i++)
            {
                if (grid[i, j] == null) continue;
                grid[i, j] = null;
            }
        }
        
    }
    public bool IsBlockInformationNull(BlockInformation block)
    {
        return block.block == null;
    }
    public void PlayAudio(AudioClip clip)
    {
        source.clip = clip;
        source.Play();
    }
}


