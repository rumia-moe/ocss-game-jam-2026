using UnityEngine;

public class ParticleCleanup : MonoBehaviour
{
   void Awake()
    {
        var ps = GetComponent<ParticleSystem>();
        ps.Play();
        Destroy(gameObject, ps.main.duration);
    }
}
