using UnityEngine;

public class AttackEntitySkill : EntitySkill
{

    public override string EntitySkillName { get; } = "Attack";

    public AttackEntitySkill() { }

    public override void Use(Entity source, Entity target)
    {

        target.changeHealth(-source.damage, source);

    }

}
