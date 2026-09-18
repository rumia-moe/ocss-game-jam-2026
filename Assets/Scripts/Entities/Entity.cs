using UnityEngine;

public abstract class Entity : MonoBehaviour
{

    public string entityName = "Entity";
    public float entityHealth = 10f;

    public EntityAttribute[] entityAttributes = { };
    public EntitySkill[] entitySkills = { };

}
