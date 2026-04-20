using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Loads Scenes
/// </summary>
public class SceneLoader : MonoBehaviour
{
    public void LoadScene(int index) => SceneManager.LoadScene(index);
    public void LoadScene(string name) => SceneManager.LoadScene(name);
}