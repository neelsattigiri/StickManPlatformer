using UnityEngine;

public class Stats : MonoBehaviour
{

    public GameObject player;
    public int maxHP = 10;

    public int currentHP;



    private void Awake()
    {
        currentHP = maxHP;
    }

    public void TakeDamage(int damage, float knockBackX, float knockBackY)
    {
        player.GetComponent<Player>().knockBackDirectionX = knockBackX;
        player.GetComponent<Player>().knockBackDirectionY = knockBackY;
        player.GetComponent<Player>().TakeHit();
        currentHP -= damage;

    }

    private void Death()
    {
        Debug.Log("Death");
        player.GetComponent<Player>().PlayerDeath();
    }
}
