using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JointHinge : MonoBehaviour {

    public bool deactivateJoint = false;
    public bool useRotationLimits = true;

    [Range(-90, 90)] public float minAngle = -90;
    [Range(-90, 90)] public float maxAngle = 90;
    [Range(0f, 1f)]  public float weight = 1.0f;

    public Vector2 rotationPointOffset = Vector2.zero;

    private float currentAngle = 0f;

    public void applyRotation(float angle) {
        if (deactivateJoint) return;

        angle = angle % 360;
        if (angle == -180) angle = 180;
        if (angle > 180)   angle -= 360;
        if (angle < -180)  angle += 360;

        if (useRotationLimits)
            angle = Mathf.Clamp(currentAngle + angle, minAngle, maxAngle) - currentAngle;

        transform.RotateAround(getRotationPoint(), Vector3.forward, angle);
        currentAngle += angle;
    }

    public Vector2 getRotationPoint() {
        return transform.position;
    }

    public float getWeight() { return weight; }
    public float getMinAngle() { return minAngle; }
    public float getMaxAngle() { return maxAngle; }
    public float getCurrentAngle() { return currentAngle; }
}
