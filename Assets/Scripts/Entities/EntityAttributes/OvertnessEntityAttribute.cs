using UnityEngine;

public class OvertnessEntityAttribute : EntityAttribute
{

    public override string EntityAttributeName { get; } = "Overtness";

    private readonly float overtnessMultiplier;

    public OvertnessEntityAttribute(float overtnessMultiplier)
    {

        this.overtnessMultiplier = overtnessMultiplier;

    }

    public override void Add(Entity entity)
    {

        entity.GetComponent<CircleCollider2D>().radius *= this.overtnessMultiplier;

    }

    public override void Remove(Entity entity)
    {

        entity.GetComponent<CircleCollider2D>().radius /= this.overtnessMultiplier;

    }

}
