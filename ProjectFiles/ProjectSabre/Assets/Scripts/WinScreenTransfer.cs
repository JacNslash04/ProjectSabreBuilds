using UnityEngine;
using UnityEngine.SceneManagement;
public class WinScreenTransfer : MonoBehaviour
{

    public PlayerController player;
    public Rigidbody2D rb;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D playerCollider)
    {
        if (player.enemyDefeatedCount >= 33)
        {
            if (rb != null)
            {
                SceneManager.LoadScene("YouWinScreen");
                Cursor.visible = true;
            }
        }
    }
}
