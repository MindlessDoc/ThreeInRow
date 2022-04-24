using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Object = System.Object;
using Timer = System.Threading.Timer;

public class FiledBuilder : MonoBehaviour
{
    [SerializeField] private GameObject _square_H_Prefab;
    [SerializeField] private GameObject _square_a_Prefab;
    [SerializeField] private GameObject _square_0_Prefab;
    [SerializeField] private GameObject _square_V_Prefab;
    [SerializeField] private GameObject _square_2_Prefab;
    [SerializeField] private GameObject _square_1_Prefab;

    [SerializeField] private Canvas _canvas;
    [SerializeField] private EventSystem _eventSystem;

    [SerializeField] private float deltaSize = 50;
    [SerializeField] private float rightX;
    [SerializeField] private float rightY;

    private Dictionary<int, GameObject> _squareVariants;

    private const int FIELD_SIZE = 5;
    private const int COUNT_OF_SQUARES = 6;

    [SerializeField] private List<List<GameObject>> _gameObjectsField;
    private List<List<Square>> _field;

    private float _curTime = 0;
    [SerializeField] private float wrongPause = 0.5f;

    private CurSelectedSquare _selectedSquare;
    private CurSelectedSquare _prevSelectedSquare;

    private bool _checkOnWrong = false;
    private float _timeWrong;

    private bool _checkOnDelete = false;
    private float _timeDelete;
    
    private bool _fillFieldChech = false;
    private float _timeFill;

    [SerializeField] private Text _scoreText;
    private int _score = 0;
    private void Awake()
    {
        _selectedSquare = null;

        _gameObjectsField = new List<List<GameObject>>();
        _field = new List<List<Square>>();
        _squareVariants = new Dictionary<int, GameObject>();
        initSquareVariants();
        
        firstInitField();
        finalInitField();
    }
    
    void Start()
    {
        
    }
    
    void Update()
    {
        _curTime += Time.deltaTime;
        PlayerPrefs.SetFloat("timer", _curTime);

        if (_checkOnWrong && _curTime - _timeWrong > 0.5)
        {
            _checkOnWrong = false;
            DeleteWrongView();
        }
        
        if (_checkOnDelete && _curTime - _timeDelete > 0.3)
        {
            _checkOnDelete = false;
            FallField();
        }
        
        if (_fillFieldChech && _curTime - _timeFill > 0.35)
        {
            _fillFieldChech = false;
            checkOnDelete();
        }
    }

    private void initSquareVariants()
    {
        _squareVariants.Add(0, _square_H_Prefab);
        _squareVariants.Add(1, _square_a_Prefab);
        _squareVariants.Add(2, _square_0_Prefab);
        _squareVariants.Add(3, _square_V_Prefab);
        _squareVariants.Add(4, _square_2_Prefab);
        _squareVariants.Add(5, _square_1_Prefab);
    }

    private void firstInitField()
    {
        for (int i = 0; i < FIELD_SIZE; i++)
        {
            var addGame = new List<GameObject>();
            var addField = new List<Square>();
            for (int j = 0; j < FIELD_SIZE; j++)
            {
                addGame.Add(null );
                addField.Add(null);
            }
            _gameObjectsField.Add(addGame);
            _field.Add(addField);
        }
    }

    private void finalInitField()
    {
        for (int row = 0; row < FIELD_SIZE; row++)
        {
            for (int column = 0; column < FIELD_SIZE; column++)
            {
                List<int> resList = new List<int>();
                for (int index = 0; index < COUNT_OF_SQUARES; index++)
                {
                    if (canCreate(_squareVariants[index].GetComponent<Square>().getType(), row, column))
                    {
                        resList.Add(index);
                    }
                }
                
                _gameObjectsField[row][column] = Instantiate(
                    _squareVariants[resList[Random.Range(0, resList.Count)]],
                    new Vector3(deltaSize * column + rightX, deltaSize * row + rightY, 0),
                    Quaternion.identity
                    );
                _gameObjectsField[row][column].transform.SetParent(_canvas.transform, false);
                _field[row][column] = _gameObjectsField[row][column].GetComponent<Square>();
                _field[row][column].setCoordAndBulder(row, column, this);
            }
        }
    }
    
    private bool canCreate(SquareType type, int row, int column)
    {
        if (row < _field.Count - 1)
        {
            if (_field[row + 1][column] && _field[row + 1][column].getType() == type)
            {
                if (row < _field.Count - 2)
                {
                    if (_field[row + 2][column] && _field[row + 2][column].getType() == type)
                        return false;
                }
            }
        }

        if (row > 0)
        {
            if (_field[row - 1][column] && _field[row - 1][column].getType() == type)
            {
                if (row > 1)
                {
                    if (_field[row - 2][column] && _field[row - 2][column].getType() == type)
                        return false;
                }
            }
        }

        if (column < _field[row].Count - 1)
        {
            if (_field[row][column + 1] && _field[row][column + 1].getType() == type)
            {
                if (column < _field[row].Count - 2)
                {
                    if (_field[row][column + 2] && _field[row][column + 2].getType() == type)
                        return false;
                }
            }
        }

        if (column > 0)
        {
            if (_field[row][column - 1] && _field[row][column - 1].getType() == type)
            {
                if (column > 1)
                {
                    if (_field[row][column - 2] && _field[row][column - 2].getType() == type)
                        return false;
                }
            }
        }
        
        return true;
    }

    public void SquareIsSelect(Square square, int row, int column)
    {
        if (_selectedSquare == null)
        {
            _selectedSquare = new CurSelectedSquare(square, row, column);
            square.setSelectedView();
        }
        else if (_selectedSquare.getSquare() == square)
        {
            square.setNormalView();
            _selectedSquare = null;
        }
        else
        {
            _prevSelectedSquare = _selectedSquare;
            _selectedSquare = new CurSelectedSquare(square, row, column);

            if (Math.Abs(_prevSelectedSquare.getRow() - _selectedSquare.getRow()) 
                + Math.Abs(_prevSelectedSquare.getColumn() - _selectedSquare.getColumn()) == 1)
            {
                int prevRow = _prevSelectedSquare.getRow();
                int prevColumn = _prevSelectedSquare.getColumn();
                int curRow = _selectedSquare.getRow();
                int curColumn = _selectedSquare.getColumn();

                SwapSquares(prevRow, prevColumn, curRow, curColumn);

                checkOnDelete();
            }
            else
            {
                SelectedWrong();
                _checkOnWrong = true;
                _timeWrong = _curTime;
            }
        }
    }

    private void SwapSquares(int prevRow, int prevColumn, int curRow, int curColumn)
    {
        if (_gameObjectsField[prevRow][prevColumn] == null && _gameObjectsField[curRow][curColumn] == null)
        {
            return;;
        }
        if (_gameObjectsField[prevRow][prevColumn] == null)
        {
            _gameObjectsField[curRow][curColumn].GetComponent<Square>().setCoordAndBulder(prevRow, curColumn, this);
            initNewSquare(prevRow, prevColumn, _gameObjectsField[curRow][curColumn]);
            _field[prevRow][prevColumn] = _gameObjectsField[prevRow][prevColumn].GetComponent<Square>();

            _gameObjectsField[curRow][curColumn] = null;
            _field[curRow][curColumn] = null;
            
        }
        else if (_gameObjectsField[curRow][curColumn] == null)
        {
            _gameObjectsField[prevRow][prevColumn].GetComponent<Square>().setCoordAndBulder(curRow, curColumn, this);
            initNewSquare(curRow, curColumn, _gameObjectsField[prevRow][prevColumn]);
            _field[curRow][curColumn] = _gameObjectsField[curRow][curColumn].GetComponent<Square>();
            
            _gameObjectsField[prevRow][prevColumn] = null;
            _field[prevRow][prevColumn] = null;
        }
        else
        {
            GameObject firstObject = createCopy(_gameObjectsField[prevRow][prevColumn]);
            GameObject secondObject = createCopy(_gameObjectsField[curRow][curColumn]);
            firstObject.GetComponent<Square>().setCoordAndBulder(curRow, curColumn, this);
            secondObject.GetComponent<Square>().setCoordAndBulder(prevRow, prevColumn, this);

            initNewSquare(curRow, curColumn, firstObject);
            initNewSquare(prevRow, prevColumn, secondObject);

        }

        if (_field[curRow][curColumn])
        {
            _field[curRow][curColumn].setNormalView();
        }
        
        if (_field[prevRow][prevColumn])
        {
            _field[prevRow][prevColumn].setNormalView();
        }

        _prevSelectedSquare = null;
        _selectedSquare = null;
    }

    private GameObject createCopy(GameObject gameObject)
    {
        Square square = gameObject.GetComponent<Square>();
        GameObject res = Instantiate(
            gameObject,
            new Vector3(deltaSize * square.getColumn() + rightX, deltaSize * square.getRow() + rightY, 0),
            Quaternion.identity
        );
        res.transform.SetParent(_canvas.transform, false);

        return res;
    }

    private void checkOnDelete()
    {
        bool wasDeleted = false;
        for (int row = 0; row < FIELD_SIZE; row++)
        {
            for (int column = 0; column < FIELD_SIZE; column++)
            {
                if (_field[row][column] == null)
                {
                    continue;
                }
                SquareType curType = _field[row][column].getType();
                List<Square> toDelete = new List<Square>();
                List<Square> additional = new List<Square>();
                for (int curRow = row + 1; curRow < FIELD_SIZE && _field[curRow][column] && _field[curRow][column].getType() == curType; curRow++)
                {
                    additional.Add(_field[curRow][column]);
                }
                checkOnMerge(toDelete, additional);
                
                for (int curRow = row - 1; curRow >= 0 && _field[curRow][column] && _field[curRow][column].getType() == curType; curRow--)
                {
                    additional.Add(_field[curRow][column]);
                }
                checkOnMerge(toDelete, additional);
                additional = new List<Square>();
                
                for (int curColumn = column + 1; curColumn < FIELD_SIZE && _field[row][curColumn] && _field[row][curColumn].getType() == curType; curColumn++)
                {
                    additional.Add(_field[row][curColumn]);
                }
                checkOnMerge(toDelete, additional);
                
                for (int curColumn = column - 1; curColumn >= 0 && _field[row][curColumn] && _field[row][curColumn].getType() == curType; curColumn--)
                {
                    additional.Add(_field[row][curColumn]);
                }
                checkOnMerge(toDelete, additional);

                if (toDelete.Count >= 2)
                {
                    toDelete.Add(_field[row][column]);
                    deleteSquares(toDelete);
                    wasDeleted = true;
                    _score += toDelete.Count;
                    _scoreText.text = _score.ToString();
                }
            }
        }

        if (wasDeleted)
        {
            _timeDelete = _curTime;
            _checkOnDelete = true;
            //FallField();
        }
    }

    private void checkOnMerge(List<Square> toDelete, List<Square> additional)
    {
        if (additional.Count >= 2)
        {
            for (int i = 0; i < additional.Count && !toDelete.Contains(additional[i]); i++)
            {
                toDelete.Add(additional[i]);
            }
        }
    }

    private void deleteSquares(List<Square> toDelete)
    {
        for (int i = 0; i < toDelete.Count; i++)
        {
            Destroy(_gameObjectsField[toDelete[i].getRow()][toDelete[i].getColumn()]);
            _gameObjectsField[toDelete[i].getRow()][toDelete[i].getColumn()] = null;
            _field[toDelete[i].getRow()][toDelete[i].getColumn()] = null;
        }
    }

    private void FallField()
    {
        bool wasFalles = true;
        for (int i = 0; i < 4; i++)
        {
            for (int row = FIELD_SIZE - 1; row >= 0; row--)
            {
                for (int column = FIELD_SIZE - 1; column >= 0; column--)
                {
                    if (row >= 1 && _field[row - 1][column] == null)
                    {
                        SwapSquares(row, column, row - 1, column);
                    }
                }
            }
        }

        initField();
        _timeFill = _curTime;
        _fillFieldChech = true;
        //checkOnDelete();
    }

    private void initField()
    {
        for (int row = FIELD_SIZE - 1; row >= 0; row--)
        {
            for (int column = FIELD_SIZE - 1; column >= 0; column--)
            {
                if (_gameObjectsField[row][column] == null)
                {
                    _gameObjectsField[row][column] = Instantiate(
                        _squareVariants[Random.Range(0, 5)],
                        new Vector3(deltaSize * column + rightX, deltaSize * row + rightY, 0),
                        Quaternion.identity
                    );
                    _gameObjectsField[row][column].transform.SetParent(_canvas.transform, false);
                    _field[row][column] = _gameObjectsField[row][column].GetComponent<Square>();
                    _field[row][column].setCoordAndBulder(row, column, this);
                }
            }
        }
    }

    private void initNewSquare(int row, int column, GameObject gameObject)
    {
        GameObject prev = gameObject;

        Destroy(_gameObjectsField[row][column]);
        
        _gameObjectsField[row][column] = Instantiate(
            gameObject,
            new Vector3(deltaSize * column + rightX, deltaSize * row + rightY, 0),
            Quaternion.identity
        );
        _gameObjectsField[row][column].GetComponent<Square>().setCoordAndBulder(row, column, this);
        _gameObjectsField[row][column].transform.SetParent(_canvas.transform, false);
        _field[row][column] = _gameObjectsField[row][column].GetComponent<Square>();
        Destroy(prev);
    }

    private void SelectedWrong()
    {
        _prevSelectedSquare.getSquare().setWrongView();
        _selectedSquare.getSquare().setWrongView();
        _eventSystem.enabled = false;
    }

    private void DeleteWrongView()
    {
        _prevSelectedSquare.getSquare().setNormalView();
        _selectedSquare.getSquare().setNormalView();
        
        _prevSelectedSquare = null;
        _selectedSquare = null;
        _eventSystem.enabled = true;
    }

    public class CurSelectedSquare
    {
        private int _row;
        private int _column;
        private Square _square;
        public CurSelectedSquare(Square square, int row, int column)
        {
            _square = square;
            _row = row;
            _column = column;
        }

        public int getRow()
        {
            return _row;
        }
        
        public int getColumn()
        {
            return _column;
        }

        public Square getSquare()
        {
            return _square;
        }
    }
}
