using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private int sceneIndexToLoad;
    [SerializeField] private TMP_InputField widthInput;
    [SerializeField] private TMP_InputField heightInput;
    [SerializeField] private int defaultWidth = 4;
    [SerializeField] private int defaultHeight = 4;

    // Start is called before the first frame update
    private void Start()
    {
        int width = PlayerPrefs.GetInt("width", -1);
        if (width == -1)
            PlayerPrefs.SetInt("width", defaultWidth);
        else
            widthInput.text = width.ToString();

        int height = PlayerPrefs.GetInt("height", -1);
        if (height == -1)
            PlayerPrefs.SetInt("height", defaultHeight);
        else
            heightInput.text = height.ToString();
    }

    public void UpdateWidth(string text)
    {
        PlayerPrefs.SetInt("width",
            text == ""
            ? defaultWidth
            : int.Parse(text)
        );
    }

    public void UpdateHeight(string text)
    {
        PlayerPrefs.SetInt("height",
            text == ""
            ? defaultHeight
            : int.Parse(text)
        );
    }

    public void Play()
    {
        SceneManager.LoadScene(sceneIndexToLoad);
    }

    public void ResetPlayerPrefs()
    {
        PlayerPrefs.DeleteKey("width");
        PlayerPrefs.DeleteKey("height");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
