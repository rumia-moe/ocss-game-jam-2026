using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class SwordfishEntity : Entity
{

    [HideInInspector]
    public override string EntityName { get; set; } = "Swordfish";

    public override float EntityMaxHealth { get; set; } = 2f;

    protected override EntitySkill[] EntitySkills { get; set; }

    private NavMeshAgent agent;

    protected override void Start()
    {

        base.Start();

        this.agent = GetComponent<NavMeshAgent>();

        this.agent.updateRotation = false;
        this.agent.updateUpAxis = false;

        this.EntitySkills = new EntitySkill[] { new PierceEntitySkill(this.agent) };

    }

    protected override void Update()
    {

        base.Update();

        base.UseSkill(EntitySkills[0]);

        this.agent.SetDestination(FindAnyObjectByType<Player>().transform.position);

    }

}
