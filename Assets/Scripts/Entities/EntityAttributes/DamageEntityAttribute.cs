public class DamageEntityAttribute : EntityAttribute
{

    public override string EntityAttributeName { get; } = "Damage";

    private float damageMultiplier;

    public DamageEntityAttribute(float damageMultiplier)
    {
        this.damageMultiplier = damageMultiplier;
    }

    public override void Add(Entity entity)
    {
        entity.damage *= damageMultiplier;   
    }

    public override void Remove(Entity entity) {
        entity.damage /= damageMultiplier;
    }

}
