using UnityEngine;
using UnityEngine.AI;

public class PierceEntitySkill : EntitySkill
{

    private NavMeshAgent agent;

    public override string EntitySkillName { get; } = "Pierce";

    public override float Cooldown { get; } = 3f;

    public PierceEntitySkill(NavMeshAgent agent) {
        this.agent = agent;
    }

    public override void Use(Entity source, Entity target)
    {

        var rigidbody = source.GetComponent<Rigidbody2D>();

        rigidbody.AddForce((Vector2) this.agent.velocity.normalized * 350f);

    }

}
