using UnityEngine;
using UnityEngine.InputSystem;

public class Player : Entity
{

    protected override EntityAttribute[] EntityAttributes { get; set; } = { new MovementSpeedEntityAttribute(5f) };

    public void OnMove(InputAction.CallbackContext context)
    {

        rigidbody.linearVelocity = context.ReadValue<Vector2>() * movementSpeed;

    }

}
