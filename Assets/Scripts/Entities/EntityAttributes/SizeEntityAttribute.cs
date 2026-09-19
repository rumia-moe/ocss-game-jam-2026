using UnityEngine;

public class SizeEntityAttribute : EntityAttribute
{

    public override string EntityAttributeName { get; set; } = "Size";

    private readonly float size;

    public SizeEntityAttribute(float size)
    {

        this.size = size;

    }

    public override void Add(Entity entity)
    {

        entity.transform.localScale *= this.size;

    }

    public override void Remove(Entity entity)
    {

        entity.transform.localScale /= this.size;

    }

}
