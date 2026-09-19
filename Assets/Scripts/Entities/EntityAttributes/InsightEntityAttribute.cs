using UnityEngine;

public class InsightEntityAttribute : EntityAttribute
{

    public override string EntityAttributeName { get; } = "Insight";

    private float insightMultiplier;

    public InsightEntityAttribute(float insightMultiplier)
    {

        this.insightMultiplier = insightMultiplier;

    }

    public override void Add(Entity entity)
    {

        entity.insight *= this.insightMultiplier;

    }

    public override void Remove(Entity entity)
    {

        entity.insight /= this.insightMultiplier;

    }

}
