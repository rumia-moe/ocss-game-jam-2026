using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class BraindeadEntity : Entity
{

    [HideInInspector]
    public override string EntityName { get; set; } = "Braindead";

    public override float EntityMaxHealth { get; set; } = 2f;

    private NavMeshAgent agent;

    protected override void Start()
    {

        base.Start();

        this.agent = GetComponent<NavMeshAgent>();

        this.agent.updateRotation = false;
        this.agent.updateUpAxis = false;

        agent.SetDestination(GoRandom());

    }

    Vector2 GoRandom()
    {
        Vector2 target = Random.insideUnitCircle * 5f + (Vector2)transform.position;

        if (NavMesh.SamplePosition(target, out NavMeshHit hit, 5f, NavMesh.AllAreas))
        {
            return hit.position;
        }

        return transform.position;
    }

    protected override void Update()
    {

        base.Update();

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {

            agent.SetDestination(GoRandom());
        }
    }
}
