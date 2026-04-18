using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuManager : MonoBehaviour
{
    [Header("Scene Manager")]
    [SerializeField] private int sceneIndexToLoad;
    
    [Header("Canvas")]
    [SerializeField] private Slider widthSlider;
    [SerializeField] private Slider heightSlider;
    [SerializeField] private TMP_Text widthLabel;
    [SerializeField] private TMP_Text heightLabel;
    
    [Header("Board")]
    [SerializeField] private int defaultWidth = 4;
    [SerializeField] private int defaultHeight = 4;

    // Start is called before the first frame update
    private void Start()
    {
        int width = PlayerPrefs.GetInt("width", defaultWidth);
        widthSlider.value = width;
        UpdateWidth();

        int height = PlayerPrefs.GetInt("height", defaultHeight);
        heightSlider.value = height;
        UpdateHeight();
    }

    public void UpdateWidth()
    {
        int width = (int)widthSlider.value;
        PlayerPrefs.SetInt("width", width);
        widthLabel.text = $"Width: {width}";
    }
    
    public void ChangeWidthBy(int deltaWidth)
    {
        widthSlider.value += deltaWidth;
        UpdateWidth();
    }

    public void UpdateHeight()
    {
        int height = (int)heightSlider.value;
        PlayerPrefs.SetInt("height", height);
        heightLabel.text = $"Height: {height}";
    }
    
    public void ChangeHeightBy(int deltaHeight)
    {
        heightSlider.value += deltaHeight;
        UpdateHeight();
    }

    public void Play()
    {
        SceneManager.LoadScene(sceneIndexToLoad);
    }

    public void ResetPlayerPrefs()
    {
        widthSlider.value = defaultWidth;
        heightSlider.value = defaultHeight;
        UpdateWidth();
        UpdateHeight();
    }
    
    public void SetSquareSize(int size)
    {
        widthSlider.value = size;
        heightSlider.value = size;
        UpdateWidth();
        UpdateHeight();
    }
}
