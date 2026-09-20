using UnityEngine;

public class DashEntitySkill : EntitySkill
{

    public override string EntitySkillName { get; } = "Dash";

    public DashEntitySkill() { }

    public override void Use(Entity source, Entity target)
    {

        var rb = source.GetComponent<Rigidbody2D>();
        rb.AddForce(rb.linearVelocity * 10f);

    }

}
