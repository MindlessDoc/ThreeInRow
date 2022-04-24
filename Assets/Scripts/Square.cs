using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Square : MonoBehaviour
{
    [SerializeField] private SquareType _type;
    
    [SerializeField] private Sprite _normalSprite;
    [SerializeField] private Sprite _selectSprite;
    [SerializeField] private Sprite _rightSprite;
    [SerializeField] private Sprite _wrongSprite;

    [SerializeField] private FiledBuilder _filedBuilder;

    private int row;
    private int column;

    private void Awake()
    {

    }

    public void OnClick()
    {
        GetComponent<Image>().sprite = _selectSprite;
    }

    public SquareType getType()
    {
        return _type;
    }

    public void setCoord(int row, int column)
    {
        
    }
}