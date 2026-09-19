using UnityEngine;

public class FishSpawner : MonoBehaviour
{
    public Transform target; // drag your target object in here

    void Awake()
    {
        var chain = GetComponent<IKChain>();
        ///chain.profile = FishSpeciesLibrary.Clownfish(); // or .Shark() .Eel() .Minnow()
        chain.target = target;
        chain.Init();
    }

    void Update()
    {
       
    }

    void SwapTo(FishProfile p)
    {
        var chain = GetComponent<IKChain>();
        chain.profile = p;
        chain.Init();
    }
}