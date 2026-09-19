using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class SwordfishEntity : Entity
{

    [HideInInspector]
    public override string EntityName { get; set; } = "Swordfish";

    protected override EntitySkill[] EntitySkills { get; set; } = { new PierceEntitySkill() };

    private NavMeshAgent agent;

    protected override void Start()
    {

        base.Start();

        agent = GetComponent<NavMeshAgent>();

        agent.updateRotation = false;
        agent.updateUpAxis = false;

    }

    protected override void Update()
    {

        base.Update();

        base.UseSkill(EntitySkills[0]);

        agent.SetDestination(FindAnyObjectByType<Player>().transform.position);

    }

}
