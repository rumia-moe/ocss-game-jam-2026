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

    private void Awake()
    {
        healthUI.DrawHearts((int)EntityCurrentHealth, (int)EntityMaxHealth);
    }

    public void OnMove(InputAction.CallbackContext context)
    {

        rigidbody.linearVelocity = context.ReadValue<Vector2>() * movementSpeed;
    }

    protected virtual void OnCollisionStay2D(Collision2D collision)
    {
        base.OnCollisionStay2D(collision);

        changeHealth(-this.damage * Time.deltaTime);

        Vector2 forceDirection = transform.position - collision.transform.position;
    }

    public override void changeHealth(float health)
    {
        base.changeHealth(health);

        healthUI.DrawHearts((int)EntityCurrentHealth, (int)EntityMaxHealth);
    }

}
