using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FiledBuilder : MonoBehaviour
{
    [SerializeField] private GameObject _square_H_Prefab;
    [SerializeField] private GameObject _square_a_Prefab;
    [SerializeField] private GameObject _square_0_Prefab;
    [SerializeField] private GameObject _square_V_Prefab;
    [SerializeField] private GameObject _square_2_Prefab;
    [SerializeField] private GameObject _square_1_Prefab;

    [SerializeField] private Canvas _canvas;

    [SerializeField] private float deltaSize = 50;
    [SerializeField] private float rightX;
    [SerializeField] private float rightY;


    private Transform _transform;

    private Dictionary<int, GameObject> _squareVariants;

    private const int FIELD_SIZE = 5;
    private const int COUNT_OF_SQUARES = 6;

    [SerializeField] private List<List<GameObject>> _gameObjectsField;
    private List<List<Square>> _field;
 

    private void Awake()
    {
        _transform = _canvas.transform;
        
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
                
                Debug.Log(_transform.position);
                _gameObjectsField[row][column] = Instantiate(
                    _squareVariants[resList[Random.Range(0, resList.Count - 1)]],
                    new Vector3(deltaSize * column + rightX, deltaSize * row + rightY, 0),
                    Quaternion.identity
                    );
                _gameObjectsField[row][column].transform.SetParent(_canvas.transform, false);
                _field[row][column] = _gameObjectsField[row][column].GetComponent<Square>();
            }
        }
    }
    
    private bool canCreate(SquareType type, int row, int column)
    {
        if (row < _field.Count - 1)
        {
            if (_field[row + 1][column] && _field[row + 1][column].getType() == type)
                return false;
        }

        if (row < _field.Count - 2)
        {
            if (_field[row + 2][column] && _field[row + 2][column].getType() == type)
                return false;
        }
        
        if (row > 0)
        {
            if (_field[row - 1][column] && _field[row - 1][column].getType() == type)
                return false;
        }
        
        if (row > 1)
        {
            if (_field[row - 2][column] && _field[row - 2][column].getType() == type)
                return false;
        }
        
        if (column < _field[row].Count - 1)
        {
            if (_field[row][column + 1] && _field[row][column + 1].getType() == type)
                return false;
        }

        if (column < _field[row].Count - 2)
        {
            if (_field[row][column + 2] && _field[row][column + 2].getType() == type)
                return false;
        }
        
        if (column > 0)
        {
            if (_field[row][column - 1] && _field[row][column - 1].getType() == type)
                return false;
        }
        
        if (column > 1)
        {
            if (_field[row][column - 2] && _field[row][column - 2].getType() == type)
                return false;
        }

        return true;
    }
}
