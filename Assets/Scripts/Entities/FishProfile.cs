using UnityEngine;

[CreateAssetMenu(fileName = "FishProfile", menuName = "IK/Fish Profile")]
public class FishProfile : ScriptableObject
{
    [System.Serializable]
    public struct SegmentData
    {
        public float length;
        public float height;
        public float lateralBend;
    }

    [Header("Authoring")]
    public Vector2[] controlPoints;

    [Header("Body")]
    public SegmentData[] segments;

    [Header("Fin Attachments")]
    public FishFin[] fins;

    [Header("Behaviour")]
    [Range(0f, 1f)]
    public float followSpeed = 0.12f;

    public int solverIterations = 5;

    public static FishProfile GenerateFromCurve(AnimationCurve heightCurve, AnimationCurve lengthCurve, int segCount, float totalLength, float maxHeight)
    {
        var p = CreateInstance<FishProfile>();

        p.segments = new SegmentData[segCount];

        for (int i = 0; i < segCount; i++)
        {
            float t = i / (float)(segCount - 1);

            p.segments[i] = new SegmentData
            {
                length      = lengthCurve.Evaluate(t) * (totalLength / segCount),
                height      = heightCurve.Evaluate(t) * maxHeight,
                lateralBend = 35f
            };
        }

        return p;
    }
}

[System.Serializable]
public struct FishFin
{
    public int attachSegment;
    public Vector2 offset;
    public Vector2 size;
    public bool mirrorOtherSide;
    public FinAxis axis;
}

public enum FinAxis
{
    Dorsal,
    Ventral
}
