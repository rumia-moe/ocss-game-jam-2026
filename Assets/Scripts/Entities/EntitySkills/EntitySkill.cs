public abstract class EntitySkill
{

    public virtual string EntitySkillName { get; set; } = "Entity Skill";

    public virtual float Cooldown { get; set; } = 1f;

    public abstract void Use(Entity source, Entity target);

}
