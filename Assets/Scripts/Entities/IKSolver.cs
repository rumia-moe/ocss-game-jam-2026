using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct TargetInfo{
    public Vector2 position;
    public Vector2 normal;

    public TargetInfo(Vector2 _position, Vector2 _normal){
        position = _position;
        normal = _normal;
    }
}

public class IKSolver : MonoBehaviour
{
    private static int maxIteration = 10;
    private static float weight = 1.0f;
    private static float footAngleToNormal = 20.0f;
    
    public static void solveChainFABRIK(JointHinge[] joints, Transform endEffector, TargetInfo target, float tolerance){
     int n = joints.Length;
    
    // Build position array: joints + end effector
    Vector2[] positions = new Vector2[n + 1];
    float[] lengths = new float[n];
    
    for (int i = 0; i < n; i++)
        positions[i] = joints[i].getRotationPoint();
    positions[n] = endEffector.position;
    
    // Cache segment lengths (do this once elsewhere ideally)
    for (int i = 0; i < n; i++)
        lengths[i] = Vector2.Distance(positions[i], positions[i + 1]);
    
    // Move tip to target
    positions[n] = target.position;
    
    // Each segment follows the one in front of it, back to front
    for (int i = n - 1; i >= 0; i--) {
        Vector2 dir = (positions[i + 1] - positions[i]).normalized; // flipped
        positions[i] = positions[i + 1] + dir * lengths[i];
    }
    
    // Apply rotations
    for (int i = 0; i < n; i++) {
        Vector2 dir = positions[i + 1] - positions[i];
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        joints[i].transform.SetPositionAndRotation(
            new Vector3(positions[i].x, positions[i].y, joints[i].transform.position.z),
            Quaternion.Euler(0, 0, angle)
        );
    }
    
    endEffector.position = new Vector3(positions[n].x, positions[n].y, endEffector.position.z);
   }
}
