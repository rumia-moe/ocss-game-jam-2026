using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class BraindeadEntity : Entity
{

    private NavMeshAgent agent;

    public float wanderRange = 3f;

    protected override void Start()
    {

        base.Start();

        agent = GetComponent<NavMeshAgent>();

    }

    protected override void Update()
    {

        base.Update();

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {

        }

    }

}
