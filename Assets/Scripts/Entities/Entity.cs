using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public abstract class Entity : MonoBehaviour, IInteractable
{

    protected Dictionary<int, float> entitySkillLastUsed = new();

    protected Rigidbody2D rigidbody;

    public Collider2D overtnessCollider;
    public Collider2D hitboxCollider;

    public virtual string EntityName { get; set; } = "Entity";
    public virtual float EntityHealth { get; set; } = 10f;

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

    protected virtual void Start()
    {

        rigidbody = GetComponent<Rigidbody2D>();
        
        foreach (var entityAttribute in EntityAttributes)
        {

            entityAttribute.Add(this);

        }

    }

    protected virtual void Update() { }

    protected virtual void UseSkill(EntitySkill skill) {

        if (entitySkillLastUsed.TryGetValue(skill.GetHashCode(), out var lastUsed))
        {
            if (lastUsed + skill.Cooldown - Time.time > 0)
                return;
        }

        entitySkillLastUsed[skill.GetHashCode()] = Time.time;

        skill.Use(this, FindAnyObjectByType<Player>());
    
    }

    public virtual void Interact() { }

    protected virtual void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.collider != this.hitboxCollider) return;
        var entity = collision.otherCollider.GetComponent<Entity>();
        if (entity != null) return;
        if (collision.otherCollider != entity.hitboxCollider) return;
        entity.EntityHealth -= this.damage;
        Debug.Log(entity.gameObject.name + " at " + entity.EntityHealth);
    }

}