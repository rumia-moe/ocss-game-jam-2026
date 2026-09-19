using UnityEngine;

public class InsightEntityAttribute : EntityAttribute
{

    public override string EntityAttributeName { get; set; } = "Insight";

    private float insightMultiplier;

    public InsightEntityAttribute(float insightMultiplier)
    {

        this.insightMultiplier = insightMultiplier;

    }

    public override void Add(Entity entity)
    {

        entity.GetComponent<Collider2D>();

    }

    public override void Remove(Entity entity)
    {
        


    }

}
