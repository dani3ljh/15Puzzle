using UnityEngine;
using TMPro;

public class SettingsManager : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown colorSetting;

    private void Start()
    {
        colorSetting.value = PlayerPrefs.GetInt("colors", 0);
    }

    public void ValueChanged(int value)
    {
        PlayerPrefs.SetInt("colors", value);
    }
}
