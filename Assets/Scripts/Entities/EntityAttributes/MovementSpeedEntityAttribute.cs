using UnityEngine;

public class MovementSpeedEntityAttribute : EntityAttribute
{

    public float movementSpeed = 1f;

    public override void Add(Entity entity)
    {

        entity.movementSpeed *= movementSpeed;

    }

    public override void Remove(Entity entity)
    {

        entity.movementSpeed /= movementSpeed;

    }

}
