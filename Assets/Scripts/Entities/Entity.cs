using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.EventSystems.EventTrigger;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public abstract class Entity : MonoBehaviour, IInteractable
{

    protected Dictionary<int, float> entitySkillLastUsed = new();

    protected Rigidbody2D rigidbody;

    public Collider2D overtnessCollider;
    public Collider2D hitboxCollider;

    public virtual string EntityName { get; set; } = "Entity";
    public virtual float EntityMaxHealth { get; set; } = 10f;
    public virtual float EntityCurrentHealth { get; set; } = 10f;

    // Attributes
    [HideInInspector]
    public float movementSpeed = 1f;
    [HideInInspector]
    public float insight = 1f;
    [HideInInspector]
    public float damage = 1f;

    protected virtual EntityAttribute[] EntityAttributes { get; set; } = { };
    protected virtual EntitySkill[] EntitySkills { get; set; } = { };

    public Collider2D SelectableCollider => overtnessCollider;

    private float oceanBoundary = 22;

    protected virtual void Start()
    {

        rigidbody = GetComponent<Rigidbody2D>();

        foreach (var entityAttribute in EntityAttributes)
        {

            entityAttribute.Add(this);

        }

    }

    protected virtual void Update() { }

    protected virtual void FixedUpdate() { 
    
        if(transform.position.y >= oceanBoundary)
        {
            rigidbody.gravityScale = 2;
            
        }
        else
        {
            rigidbody.gravityScale = 0;
        }
    
    }

    protected virtual void UseSkill(EntitySkill skill)
    {

        if (entitySkillLastUsed.TryGetValue(skill.GetHashCode(), out var lastUsed))
        {
            if (lastUsed + skill.Cooldown - Time.time > 0)
                return;
        }

        entitySkillLastUsed[skill.GetHashCode()] = Time.time;

        skill.Use(this, FindAnyObjectByType<Player>());

    }

    public virtual void Interact() 
    {
        
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.otherCollider != this.hitboxCollider) return;

        var entity = collision.collider.GetComponent<Entity>();

        if (entity == null) return;
        if (collision.collider != entity.hitboxCollider) return;

        changeHealth(-this.damage);

        Vector2 forceDirection = collision.transform.position - transform.position;

        collision.collider.GetComponent<Rigidbody2D>().AddForce(forceDirection.normalized * 2f, ForceMode2D.Impulse);
        collision.otherCollider.GetComponent<Rigidbody2D>().AddForce(forceDirection.normalized * -2f, ForceMode2D.Impulse);
    }

    public virtual void changeHealth(float health)
    {
        EntityCurrentHealth -= this.damage;

        if(EntityCurrentHealth <= 0)
        {
            Destroy(this);
        }
    }

    public virtual void OnSelect() 
    { 
        
    }
    public virtual void OnUnselect() 
    {

    }

}
