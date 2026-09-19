
public class MovementSpeedEntityAttribute : EntityAttribute
{

    public override string EntityAttributeName { get; set; } = "Movement Speed";

    private readonly float movementSpeed;

    public MovementSpeedEntityAttribute(float movementSpeed)
    {

        this.movementSpeed = movementSpeed;

    }

    public override void Add(Entity entity)
    {

        entity.movementSpeed *= this.movementSpeed;

    }

    public override void Remove(Entity entity)
    {

        entity.movementSpeed /= this.movementSpeed;

    }

}
