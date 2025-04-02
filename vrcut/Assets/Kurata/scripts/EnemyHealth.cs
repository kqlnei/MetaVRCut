using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public void TakeDamage()
    {
        Destroy(gameObject);
    }
}