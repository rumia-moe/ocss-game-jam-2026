using UnityEngine;
using UnityEngine.InputSystem;

public class Player : Entity
{

    public void OnMove(InputAction.CallbackContext context)
    {

        rigidbody.linearVelocity = context.ReadValue<Vector2>();

    }

}
