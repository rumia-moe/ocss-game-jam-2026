using UnityEngine;

public class IntelectEntityAttribute : EntityAttribute
{

    public override string EntityAttributeName { get; } = "Intelect";

    private readonly float intelectMultiplier;

    public IntelectEntityAttribute(float intelectMultiplier)
    {

        this.intelectMultiplier = intelectMultiplier;

    }

    public override void Add(Entity entity)
    {

        entity.intelect *= this.intelectMultiplier;

    }

    public override void Remove(Entity entity)
    {

        entity.transform.localScale /= this.intelectMultiplier;

    }

}
