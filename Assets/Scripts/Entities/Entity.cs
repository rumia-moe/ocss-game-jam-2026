using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInput))]
public abstract class Entity : MonoBehaviour
{

    protected Rigidbody2D rigidbody;

    public string entityName = "Entity";
    public float entityHealth = 10f;

    // Attributes
    public float movementSpeed = 1f;

    public EntityAttribute[] entityAttributes = { };
    public EntitySkill[] entitySkills = { };

    private void Start()
    {

        rigidbody = GetComponent<Rigidbody2D>();
        
        foreach (var entityAttribute in entityAttributes)
        {

            entityAttribute.Add(this);

        }

    }

}
