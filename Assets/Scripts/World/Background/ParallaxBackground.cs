using UnityEngine;

public class ParallaxBackgrounds : MonoBehaviour
{
    private float length;

    private float[] startPos;

    public GameObject[] components;


    public GameObject camera;
    public float parralaxFactor;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startPos = new float[components.Length];
        for (int i = 0; i < startPos.Length; i++)
        {
            startPos[i] = components[i].transform.position.x;
        }
        length = components[0].GetComponent<SpriteRenderer>().bounds.size.x;
    }

    // Update is called once per frame
    void Update()
    {
        float temp = camera.transform.position.x * (1 - parralaxFactor);
        float distance = camera.transform.position.x * parralaxFactor;

        for (int i = 0; i < startPos.Length; i++)
        {
            transform.position = new Vector3(startPos[i] + distance, transform.position.y, transform.position.z);
        }

        for (int i = 0; i < startPos.Length; i++)
        {
            if (temp > startPos[i] + length)
            {
                startPos[i] += length;
            }
            else if (temp < startPos[i] - length)
            {
                startPos[i] -= length;
            }
        }
    }
}
