using UnityEngine;
using TMPro;
using UnityEngine.UI;

[RequireComponent(typeof(SceneLoader))]
public class SettingsManager : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown colorSetting;
    [SerializeField] private Toggle sfxSetting;
    
    private SceneLoader sceneLoader;

    private void Start()
    {
        sceneLoader = GetComponent<SceneLoader>();
        colorSetting.value = PlayerPrefs.GetInt("colors");
        sfxSetting.isOn = PlayerPrefs.GetInt("sfx") == 1;
    }

    public void ColorValueChanged()
    {
        PlayerPrefs.SetInt("colors", colorSetting.value);
    }
    
    public void SFXValueChanged()
    {
        PlayerPrefs.SetInt("sfx", sfxSetting.isOn ? 1 : 0);
        sceneLoader.sfxSetting = sfxSetting.isOn;
    }
}
