using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class SwordfishEntity : Entity
{

    [HideInInspector]
    public override string EntityName { get; set; } = "Swordfish";

    public override float EntityMaxHealth { get; set; } = 2f;

    private NavMeshAgent agent;

    protected override void Start()
    {

        base.Start();

        this.agent = GetComponent<NavMeshAgent>();

        this.agent.updateRotation = false;
        this.agent.updateUpAxis = false;

    }

    protected override void Update()
    {

        base.Update();

        //this.agent.SetDestination(FindAnyObjectByType<Player>().transform.position);

    }

}
