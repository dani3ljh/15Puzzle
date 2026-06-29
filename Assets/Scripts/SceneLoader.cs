using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Loads Scenes
/// </summary>
public class SceneLoader : MonoBehaviour
{
    // yes I know the audio should be wrapped in another class but i dont wanna
    [SerializeField] private AudioManager audioManager;

    public void LoadScene(int index) {
        float length = audioManager.PlaySound("click");

        StartCoroutine(LoadSceneAfterTime(length, index));
    }
    public void LoadScene(string name) {
        float length = audioManager.PlaySound("click");

        StartCoroutine(LoadSceneAfterTime(length, name));
    }
    
    private IEnumerator LoadSceneAfterTime(float timeSeconds, int index) {
        yield return new WaitForSeconds(timeSeconds);
        
        SceneManager.LoadScene(index);
    }
    private IEnumerator LoadSceneAfterTime(float timeSeconds, string name) {
        yield return new WaitForSeconds(timeSeconds);
        
        SceneManager.LoadScene(name);
    }
}