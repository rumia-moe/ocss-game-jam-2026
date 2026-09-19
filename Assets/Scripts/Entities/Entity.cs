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

        randomizModel(0.8f);
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

     public virtual void randomizModel(float strength = 1f)
    {
        var chain = GetComponentInChildren<IKChain>();
        Debug.Log($"[RandomizeModel] chain found: {chain != null}, profile: {(chain != null ? chain.profile != null : false)}");
        if (chain == null || chain.profile == null) return;

        chain.profile = Instantiate(chain.profile);
        FishProfile profile = chain.profile;

        // strength = 0 → no change, strength = 1 → wild variation
        float s = Mathf.Clamp01(strength);

        // --- Behaviour ---
        profile.followSpeed = Mathf.Clamp(
            profile.followSpeed * Random.Range(1f - 0.4f * s, 1f + 0.6f * s),
            0.04f, 0.4f
        );

        profile.solverIterations = Mathf.Clamp(
            profile.solverIterations + Mathf.RoundToInt(Random.Range(-2f * s, 3f * s)),
            2, 12
        );

        // --- Global scale jitter ---
        // Apply one size multiplier to the whole fish so some spawn big, some small.
        float globalScale = Random.Range(1f - 0.35f * s, 1f + 0.5f * s);

        // --- Segments ---
        if (profile.segments != null)
        {
            for (int i = 0; i < profile.segments.Length; i++)
            {
                var seg = profile.segments[i];

                seg.length = Mathf.Max(
                    seg.length * globalScale * Random.Range(1f - 0.25f * s, 1f + 0.25f * s),
                    0.03f
                );

                seg.height = Mathf.Max(
                    seg.height * globalScale * Random.Range(1f - 0.35f * s, 1f + 0.45f * s),
                    0.01f
                );

                seg.lateralBend = Mathf.Clamp(
                    seg.lateralBend * Random.Range(1f - 0.3f * s, 1f + 0.4f * s),
                    5f, 70f
                );

                profile.segments[i] = seg;
            }
        }

        // --- Control points ---
        if (profile.controlPoints != null)
        {
            for (int i = 0; i < profile.controlPoints.Length; i++)
            {
                var cp = profile.controlPoints[i];
                cp.x *= globalScale * Random.Range(1f - 0.15f * s, 1f + 0.15f * s);
                cp.y *= globalScale * Random.Range(1f - 0.4f * s, 1f + 0.5f * s);
                profile.controlPoints[i] = cp;
            }
        }

        // --- Fins ---
        if (profile.fins != null)
        {
            for (int i = 0; i < profile.fins.Length; i++)
            {
                var fin = profile.fins[i];

                fin.size = new Vector2(
                    Mathf.Max(fin.size.x * globalScale * Random.Range(1f - 0.4f * s, 1f + 0.6f * s), 0.02f),
                    Mathf.Max(fin.size.y * globalScale * Random.Range(1f - 0.4f * s, 1f + 0.6f * s), 0.02f)
                );

                fin.offset = new Vector2(
                    fin.offset.x + Random.Range(-0.05f * s, 0.05f * s),
                    fin.offset.y + Random.Range(-0.03f * s, 0.03f * s)
                );

                profile.fins[i] = fin;
            }
        }
        Debug.Log($"[RandomizeModel] First CP after randomize: {profile.controlPoints[0]}, globalScale: {globalScale}");    
        chain.Init();
    }
}
