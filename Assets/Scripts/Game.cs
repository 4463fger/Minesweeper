using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class Game : MonoBehaviour
{
    public int width = 16;
    public int height = 16;
    public int mineCount = 16; //地雷数

    private Board _board;   //用于更新游戏板的状态

    private Cell[,] _state; //存储单元格信息
    private bool _gamevoer;

    private void OnValidate()
    {
        mineCount = Mathf.Clamp(mineCount, 0, width * height);
    }

    private void Awake()
    {
        _board = GetComponentInChildren<Board>();
    }

    private void Start()
    {
        NewGame();
    }

    private void NewGame()
    {
        _state = new Cell[width, height];
        _gamevoer = false;

        GenerateCells();
        GenerateMines();
        GenerateNumbers();

        //调整相机位置
        if (Camera.main != null) Camera.main.transform.position = new Vector3(width / 2f, height / 2f, -10f);
        _board.Draw(_state);
    }

    private void GenerateCells()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = new Cell();
                cell.position = new Vector3Int(x, y, 0);
                cell.type = Cell.Type.Empty;
                _state[x, y] = cell;
            }
        }
    }
    private void GenerateMines()
    {
        for (int i = 0; i < mineCount; i++)
        {
            int x = Random.Range(0,width);
            int y = Random.Range(0,height);

            if (_state[x, y].type == Cell.Type.Mine)
                i--;

            _state[x, y].type = Cell.Type.Mine;
        }
    }

    private void GenerateNumbers()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = _state[x, y];

                if(cell.type ==Cell.Type.Mine)
                    continue;

                cell.number = CountMines(x, y);

                if (cell.number > 0)
                    cell.type = Cell.Type.Number;

                _state[x, y] = cell;
            }
        }
    }
    //计算数字
    private int CountMines(int cellX,int cellY)
    {
        int count = 0;

        for (int adjacentX = -1; adjacentX <= 1; adjacentX++)
        {
            for (int adjacentY = -1; adjacentY <=1; adjacentY++)
            {
                if(adjacentX == 0 && adjacentY == 0)
                    continue;

                int x = cellX + adjacentX;
                int y = cellY + adjacentY;

                if (GetCell(x,y).type == Cell.Type.Mine)
                    count++;
            }
        }

        return count;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            NewGame();
        }
        else if (!_gamevoer)
        {
            //右键标记
            if (Input.GetMouseButtonDown(1))
                Flag();
            else if (Input.GetMouseButtonDown(0))
                Reveal();
        }
    }

    private void Flag()
    {
        Vector3 worldPosition   = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = _board.Tilemap.WorldToCell(worldPosition);
        Cell cell = GetCell(cellPosition.x,cellPosition.y);

        if (cell.type == Cell.Type.Invalid)
            return;

        cell.flagged = !cell.flagged;
        _state[cellPosition.x, cellPosition.y] = cell;
        _board.Draw(_state);
    }
    private void Reveal()
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = _board.Tilemap.WorldToCell(worldPosition);
        Cell cell = GetCell(cellPosition.x, cellPosition.y);

        if (cell.type ==Cell.Type.Invalid || cell.revealed || cell.flagged)
            return;

        switch (cell.type)
        {
            case Cell.Type.Mine:
                Explode(cell);
                break;
            case Cell.Type.Empty:
                Flood(cell);
                CheckWinCondition();
                break;
            default:
                cell.revealed = true;
                _state[cellPosition.x, cellPosition.y] = cell;
                CheckWinCondition();
                break;
        }

        _board.Draw(_state);
    }

    private void CheckWinCondition()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = _state[x, y];
                if (cell.type !=Cell.Type.Mine && !cell.revealed)
                {
                    return;
                }
            }
        }
        _gamevoer = true;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = _state[x, y];
                if (cell.type == Cell.Type.Mine)
                {
                    cell.flagged = true;
                    _state[x, y] = cell;
                }
            }
        }
    }
    private void Flood(Cell cell)
    {
        if (cell.revealed) return;
        if(cell.type == Cell.Type.Mine || cell.type == Cell.Type.Invalid) return;

        cell.revealed = true;
        _state[cell.position.x, cell.position.y] = cell;

        if (cell.type == Cell.Type.Empty)
        {
            Flood(GetCell(cell.position.x - 1, cell.position.y));
            Flood(GetCell(cell.position.x + 1, cell.position.y));
            Flood(GetCell(cell.position.x   ,  cell.position.y-1 ));
            Flood(GetCell(cell.position.x    , cell.position.y+1));
        }
    }
    private void Explode(Cell cell)
    {
        _gamevoer = true;

        cell.revealed = true;
        cell.exploded = true;
        _state[cell.position.x, cell.position.y] = cell;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                cell = _state[x, y];
                if (cell.type == Cell.Type.Mine)
                {
                    cell.revealed = true;
                    _state[x, y] = cell;
                }
            }
        }
    }
    private Cell GetCell(int x,int y)
    {
        if (IsValid(x,y))
            return _state[x, y];
        else
            return new Cell();
    }
    //判断是否有效
    private bool IsValid(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }
}
