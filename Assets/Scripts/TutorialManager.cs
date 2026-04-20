using UnityEngine;
using TMPro;

/// <summary>
/// Manages the Tutorial
/// </summary>
public class TutorialManager : MonoBehaviour
{
    [SerializeField] private GameObject[] pages;
    [SerializeField] private TMP_Text pageLabel;

    private int currPage;

    /// <summary>
    /// Start is called on the frame when a script is enabled just before any of the Update methods are called.
    /// </summary>
    private void Start()
    {
        for (int i = 0; i < pages.Length; i++) 
            pages[i].SetActive(false);
        
        pages[currPage].SetActive(true);
        UpdatePageLabel();
    }
    
    private void UpdatePageLabel()
    {
        pageLabel.text = $"{currPage+1}/{pages.Length}";
    }
    
    public void NextPage()
    {
        pages[currPage].SetActive(false);
        currPage = (currPage + 1) % pages.Length;
        pages[currPage].SetActive(true);
        UpdatePageLabel();
    }
    
    public void PreviousPage()
    {
        pages[currPage].SetActive(false);
        currPage = (currPage - 1 + pages.Length) % pages.Length; // -1 % 2 = -1 for some reason
        pages[currPage].SetActive(true);
        UpdatePageLabel();
    }
}