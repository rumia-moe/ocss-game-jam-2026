using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public abstract class Entity : MonoBehaviour
{

    protected Dictionary<int, float> entitySkillLastUsed = new();

    protected Rigidbody2D rigidbody;

    public virtual string EntityName { get; set; } = "Entity";
    public virtual float EntityHealth { get; set; } = 10f;

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

    protected virtual void UseSkill(EntitySkill skill) {

        if (entitySkillLastUsed.TryGetValue(skill.GetHashCode(), out var lastUsed))
        {
            if (lastUsed + skill.Cooldown - Time.time > 0)
                return;
        }

        entitySkillLastUsed[skill.GetHashCode()] = Time.time;

        skill.Use(this, FindAnyObjectByType<Player>());
    
    }

}
