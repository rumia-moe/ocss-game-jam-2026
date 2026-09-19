using UnityEngine;

public class PierceEntitySkill : EntitySkill
{

    public override string EntitySkillName { get; set; } = "Pierce";

    public override void Use(Entity source, Entity target)
    {

        var rigidbody = source.GetComponent<Rigidbody2D>();

        var direction = target.transform.position - source.transform.position;

        rigidbody.AddForce(direction.normalized * 200f);

    }

}
