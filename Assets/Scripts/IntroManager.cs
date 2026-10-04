using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroManager : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene("Preferences");
    }
}