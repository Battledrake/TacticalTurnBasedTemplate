using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[ExecuteInEditMode]
public class CheatWidget : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private string _cheatKey;
    [SerializeField] private Color _baseColor;
    [SerializeField] private Color _selectedColor;
    [SerializeField] private TextMeshProUGUI _cheatText;

    [SerializeField] private Texture2D _baseCursor;
    [SerializeField] private Texture2D _hoverCursor;

    private bool _isSelected = false;

    private void OnValidate()
    {
        _cheatText.text = _cheatKey;
    }

    public void OnButtonPressed()
    {
        _isSelected = !_isSelected;

        if (_isSelected)
        {
            this.GetComponent<Image>().color = _selectedColor;
        }
        else
        {
            this.GetComponent<Image>().color = _baseColor;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Cursor.SetCursor(_baseCursor, new Vector2(0, 0), CursorMode.Auto);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Cursor.SetCursor(_hoverCursor, new Vector2(0, 0), CursorMode.Auto);
    }
}
