using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class Mutation : MonoBehaviour
{
    public TextMeshProUGUI descText;
    public TextMeshProUGUI costText;

    public Transform explosion;
    public GameObject parent;


    private int cost;
    public EntityAttribute[] attributes = new EntityAttribute[3];

    void Start()
    {
        cost = Random.Range(50, 101);
        costText.text = "COST " + cost.ToString();

        float multiplier = cost / 50f;

        List<int> availableIndices = new List<int> { 0, 1, 2, 3, 4 };
        descText.text = string.Empty;

        for (int i = 0; i < attributes.Length; i++)
        {
            int randomIndex = Random.Range(0, availableIndices.Count);
            int attributeType = availableIndices[randomIndex];
            availableIndices.RemoveAt(randomIndex);

            float baseRoll = Random.Range(0.5f, 1.5f);
            float finalValue = 0f;

            switch (attributeType)
            {
                case 0:
                    finalValue = baseRoll * multiplier;
                    attributes[i] = new DamageEntityAttribute(finalValue);
                    break;
                case 1:
                    finalValue = baseRoll * multiplier;
                    attributes[i] = new InsightEntityAttribute(finalValue);
                    break;
                case 2:
                    finalValue = baseRoll * multiplier;
                    attributes[i] = new MovementSpeedEntityAttribute(finalValue);
                    break;
                case 3:
                    finalValue = baseRoll / multiplier;
                    attributes[i] = new OvertnessEntityAttribute(finalValue);
                    break;
                case 4:
                    finalValue = baseRoll / multiplier;
                    attributes[i] = new SizeEntityAttribute(finalValue);
                    break;
            }

            descText.text += $"{attributes[i].EntityAttributeName} : {finalValue:F2}\n";
        }
    }

    public void Buy()
    {
        var player = FindAnyObjectByType<Player>();
        if (player.evolutionPoints < cost) return;
        player.evolutionPoints -= cost;
        foreach (var attribute in attributes)
        {
            player.EntityAttributes.Add(attribute);
            attribute.Add(player);
        }
        player.randomizModel();
        Instantiate(explosion, parent.transform.position, Quaternion.identity);
        Destroy(parent);
    }
}