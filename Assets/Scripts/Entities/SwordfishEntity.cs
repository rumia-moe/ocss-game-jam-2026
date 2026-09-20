using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class BraindeadEntity : Entity
{

    [HideInInspector]
    public override string EntityName { get; set; } = "Braindead";

    public override float EntityMaxHealth { get; set; } = 2f;

    protected override EntitySkill PrimarySkill { get; set; }

    private NavMeshAgent agent;

    protected override void Start()
    {

        base.Start();

        this.agent = GetComponent<NavMeshAgent>();

        this.agent.updateRotation = false;
        this.agent.updateUpAxis = false;

        this.PrimarySkill = new PierceEntitySkill(this.agent);

    }

    protected override void Update()
    {

        base.Update();

        base.UseSkill(this.PrimarySkill);

        this.agent.SetDestination(FindAnyObjectByType<Player>().transform.position);

    }

}
