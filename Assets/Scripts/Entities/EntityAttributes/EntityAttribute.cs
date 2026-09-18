using UnityEngine;

public abstract class EntityAttribute : MonoBehaviour
{

    public string entityAttributeName = "Entity Attribute";

    public abstract void Add(Entity entity);

    public abstract void Remove(Entity entity);

}
