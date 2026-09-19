using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class Player : Entity
{

    [HideInInspector]
    public override string EntityName { get; set; } = "Player";

    protected override EntityAttribute[] EntityAttributes { get; set; } = { new MovementSpeedEntityAttribute(5f) };

    protected float evolutionPoints = 0f;

    public HeartsHud healthUI;

    private Vector2 movement = Vector2.zero;

    private void Awake()
    {
        healthUI.DrawHearts((int)EntityCurrentHealth, (int)EntityMaxHealth);
    }

    public void OnMove(InputAction.CallbackContext context)
    {

        movement = context.ReadValue<Vector2>() * movementSpeed;

    }

    public override void changeHealth(float health)
    {
        base.changeHealth(health);

        healthUI.DrawHearts((int)EntityCurrentHealth, (int)EntityMaxHealth);
    }

    protected override void FixedUpdate() {
        base.FixedUpdate();
        if (movement != Vector2.zero) {
            rigidbody.linearVelocity = movement;
        }
    }

}
