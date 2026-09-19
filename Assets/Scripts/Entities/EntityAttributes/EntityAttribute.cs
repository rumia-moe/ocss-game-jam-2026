public abstract class EntityAttribute
{

    public virtual string EntityAttributeName { get; } = "Entity Attribute";

    public abstract void Add(Entity entity);

    public abstract void Remove(Entity entity);

}
