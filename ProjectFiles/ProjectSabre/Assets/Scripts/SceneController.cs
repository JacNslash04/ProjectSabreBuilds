using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void toCharacterSelect()
    {
        SceneManager.LoadScene("CharacterSelectScreen");
    }

    public void returnToMenu()
    {
        SceneManager.LoadScene("TitleScreen");
    }

    public void toGameplay()
    {
        SceneManager.LoadScene("SampleScene");
        Cursor.visible = false;
    }

    public void toCredits()
    {
        SceneManager.LoadScene("CreditsScreen");
    }
    public void toWinScreen()
    {
        SceneManager.LoadScene("YouWinScreen");
        Cursor.visible = true;
    }
    public void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
