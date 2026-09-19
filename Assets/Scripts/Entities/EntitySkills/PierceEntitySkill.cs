using UnityEngine;

public class PierceEntitySkill : EntitySkill
{

    public override string EntitySkillName { get; } = "Pierce";

    public override float Cooldown { get; } = 3f;

    public override void Use(Entity source, Entity target)
    {

        var rigidbody = source.GetComponent<Rigidbody2D>();

        Vector2 direction = target.transform.position - source.transform.position;

        rigidbody.AddForce(direction.normalized * 350f);

    }

}
