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

    private int _row;
    private int _column;

    private void Awake()
    {
        
    }

    public void OnClick()
    {
        _filedBuilder.SquareIsSelect(this, _row, _column);
    }

    public SquareType getType()
    {
        return _type;
    }

    public void setCoordAndBulder(int row, int column, FiledBuilder filedBuilder)
    {
        _row = row;
        _column = column;
        _filedBuilder = filedBuilder;
    }

    public void setSelectedView()
    {
        GetComponent<Image>().sprite = _selectSprite;
    }

    public void setNormalView()
    {
        GetComponent<Image>().sprite = _normalSprite;
    }

    public void setWrongView()
    {
        GetComponent<Image>().sprite = _wrongSprite;
    }
}
