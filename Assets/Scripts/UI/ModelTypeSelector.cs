using TMPro;
using UnityEngine;

public class ModelTypeSelector : MonoBehaviour
{
    [SerializeField] private GameObject[] _modelTypes;
    [SerializeField] private TMP_Text[] _selectedModelTypeTexts;
    [SerializeField] private Color _selectedColor;
    [SerializeField] private Color _defaultColor;

    public void SelectModelType(int index)
    {
        if (index < 0 || index >= _modelTypes.Length)
        {
            Debug.LogError("Invalid model type index selected.");
            return;
        }

        for (int i = 0; i < _modelTypes.Length; i++)
        {
            _modelTypes[i].SetActive(i == index);
        }

        SetTextColor(index);
    }

    private void SetTextColor(int index)
    {
        for (int i = 0; i < _selectedModelTypeTexts.Length; i++)
        {
            if (i == index)
            {
                _selectedModelTypeTexts[i].color = _selectedColor;
            }
            else
            {
                _selectedModelTypeTexts[i].color = _defaultColor;
            }
        }
    }
}
