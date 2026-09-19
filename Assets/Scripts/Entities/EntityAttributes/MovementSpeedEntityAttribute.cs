
public class MovementSpeedEntityAttribute : EntityAttribute
{

    private float movementSpeed;

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
