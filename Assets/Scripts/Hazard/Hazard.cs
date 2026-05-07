using UnityEngine;

public class Hazard : MonoBehaviour
{

    public int damageValue = 1;
    public float knockBackPowerX = 10;
    public float knockBackPowerY = 10;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            float dirX = 0;
            float dirY = 0;

            if(transform.position.x - collision.transform.position.x <= 0)
            {
                dirX = knockBackPowerX;
            }
            else if (transform.position.x - collision.transform.position.x > 0)
            {
                dirX = -knockBackPowerX;
            }
            if (transform.position.y - collision.transform.position.y <= 0)
            {
                dirY = knockBackPowerY;
            }
            else if (transform.position.y - collision.transform.position.y > 0)
            {
                dirY = -knockBackPowerY;
            }
            collision.gameObject.GetComponent<Stats>().TakeDamage(damageValue, dirX, dirY);
        }
    }
}
