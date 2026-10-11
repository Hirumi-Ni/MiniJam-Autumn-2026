using UnityEngine;
using UnityEngine.SceneManagement;

public class EndingUI : MonoBehaviour
{
    public void RestartButton()
    {
        SceneManager.LoadScene("MainScene");
    }
}
