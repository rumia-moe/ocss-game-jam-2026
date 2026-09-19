public abstract class EntitySkill
{

    public virtual string EntitySkillName { get; } = "Entity Skill";

    public virtual float Cooldown { get; } = 1f;

    public abstract void Use(Entity source, Entity target);

}
