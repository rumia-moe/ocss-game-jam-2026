using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class Player : Entity
{

    protected override EntityAttribute[] EntityAttributes { get; set; } = { new MovementSpeedEntityAttribute(5f) };

    protected float evolutionPoints = 0f;

    public void OnMove(InputAction.CallbackContext context)
    {

        rigidbody.linearVelocity = context.ReadValue<Vector2>() * movementSpeed;

    }

}
