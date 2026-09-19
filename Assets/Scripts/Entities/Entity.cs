using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(Collider2D))]
public abstract class Entity : MonoBehaviour
{

    protected Rigidbody2D rigidbody;

    public string entityName = "Entity";
    public float entityHealth = 10f;

    // Attributes
    [HideInInspector]
    public float movementSpeed = 1f;
    [HideInInspector]
    public float insight = 1f;

    protected virtual EntityAttribute[] EntityAttributes { get; set; } = { };
    protected virtual EntitySkill[] EntitySkills { get; set; } = { };

    protected virtual void Start()
    {

        rigidbody = GetComponent<Rigidbody2D>();
        
        foreach (var entityAttribute in EntityAttributes)
        {

            entityAttribute.Add(this);

        }

    }

    protected virtual void Update() { }

}
