using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class Player : Entity
{

    [HideInInspector]
    public override string EntityName { get; set; } = "Player";

    public override List<EntityAttribute> EntityAttributes { get; set; } = new List<EntityAttribute> { new MovementSpeedEntityAttribute(5f * 100f) };

    public float evolutionPoints = 0f;

    public Canvas hud;
    public Canvas GameoverScreen;

    public HeartsHud healthUI;

    private Vector2 movement = Vector2.zero;

    public TextMeshProUGUI evolutionText;

    protected override void Awake()
    {
        base.Awake();
        healthUI.DrawHearts((int)EntityCurrentHealth, (int)EntityMaxHealth);
    }

    public void OnMove(InputAction.CallbackContext context)
    {

        movement = context.ReadValue<Vector2>() * this.movementSpeed;

    }

    public override void changeHealth(float health)
    {
        base.changeHealth(health);

        healthUI.DrawHearts((int)EntityCurrentHealth, (int)EntityMaxHealth);
    }

    protected override void Update()
    {
        base.Update();
        evolutionText.text = this.evolutionPoints + "EP";
    }

    public override void entityDeath()
    {
        Destroy(this);
        hud.gameObject.SetActive(false);
        GameoverScreen.gameObject.SetActive(true);
    }

    protected override void FixedUpdate() {
        base.FixedUpdate();
        if (movement != Vector2.zero) {
            rigidbody.AddForce(movement * Time.deltaTime);
        }
    }

}
