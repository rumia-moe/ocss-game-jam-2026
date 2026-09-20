using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;

[RequireComponent(typeof(PlayerInput))]
public class Player : Entity
{

    [HideInInspector]
    public override string EntityName { get; set; } = "Player";

    public override List<EntityAttribute> EntityAttributes { get; set; } = new List<EntityAttribute> { new IntelectEntityAttribute(10f) };

    public override EntitySkill PrimarySkill { get; set; } = new AttackEntitySkill();
    public override EntitySkill SecondarySkill { get; set; } = new DashEntitySkill();

    public GameObject hud;
    public GameObject GameOver;

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

        movement = context.ReadValue<Vector2>() * this.movementSpeed * 500f;

    }

    public override void changeHealth(float health, Entity source)
    {
        base.changeHealth(health, source);

        healthUI.DrawHearts((int)EntityCurrentHealth, (int)EntityMaxHealth);
    }

    protected override void Update()
    {
        base.Update();
        evolutionText.text = this.evolutionPoints + "EP";
    }

    public override void entityDeath(Entity source)
    {
        alive = false;
        movement = Vector2.zero;
        rigidbody.gravityScale = 1f;
        hud.gameObject.SetActive(false);
        GameOver.gameObject.SetActive(true);
    }


    protected override void FixedUpdate() {
        base.FixedUpdate();
        if (movement != Vector2.zero) {
            rigidbody.AddForce(movement * Time.deltaTime);
        }
    }

}
