using UnityEngine;
using UnityEngine.UI;

public class HeartsHud : MonoBehaviour
{

    public GameObject fullHeart;
    public GameObject depletedHeart;

    public void DrawHearts(int currentHealth, int maxHealth)
    {
        foreach(Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < maxHealth; i++)
        {
            if (i < currentHealth)
            {
                Instantiate(fullHeart, transform);
            }
            else
            {
                Instantiate(depletedHeart, transform);
            }
        }
    }
}
