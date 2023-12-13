using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class CheatWidget : MonoBehaviour
{
    [SerializeField] private string _cheatKey;
    [SerializeField] private Color _baseColor;
    [SerializeField] private Color _selectedColor;
    [SerializeField] private TextMeshProUGUI _cheatText;

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
}
