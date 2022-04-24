using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.EventSystems;
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

                (_gameObjectsField[prevRow][prevColumn], _gameObjectsField[curRow][curColumn]) = 
                    (_gameObjectsField[curRow][curColumn], _gameObjectsField[prevRow][prevColumn]);
                
                initNewSquare(curRow, curColumn);
                initNewSquare(prevRow, prevColumn);

                _field[curRow][curColumn] = _gameObjectsField[curRow][curColumn].GetComponent<Square>();
                _field[prevRow][prevColumn] = _gameObjectsField[prevRow][prevColumn].GetComponent<Square>();
                
                _field[curRow][curColumn].setCoordAndBulder(curRow, curColumn, this);
                _field[prevRow][prevColumn].setCoordAndBulder(prevRow, prevColumn, this);
                
                _field[curRow][curColumn].setNormalView();
                _field[prevRow][prevColumn].setNormalView();

                _prevSelectedSquare = null;
                _selectedSquare = null;
            }
            else
            {
                SelectedWrong();
                _checkOnWrong = true;
                _timeWrong = _curTime;
            }
        }
    }

    private void initNewSquare(int row, int column)
    {
        GameObject prev = _gameObjectsField[row][column];
        
        _gameObjectsField[row][column] = Instantiate(
            _gameObjectsField[row][column],
            new Vector3(deltaSize * column + rightX, deltaSize * row + rightY, 0),
            Quaternion.identity
        );
        _gameObjectsField[row][column].transform.SetParent(_canvas.transform, false);
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
