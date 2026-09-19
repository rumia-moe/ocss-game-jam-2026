using UnityEngine;

public static class FishSpeciesLibrary
{
    public static FishProfile Clownfish()
    {
        var p = ScriptableObject.CreateInstance<FishProfile>();
        p.followSpeed = 0.13f;
        p.solverIterations = 5;
        p.segments = new FishProfile.SegmentData[]
        {
            new() { length = 0.18f, height = 0.28f, lateralBend = 15f },
            new() { length = 0.16f, height = 0.38f, lateralBend = 20f },
            new() { length = 0.15f, height = 0.36f, lateralBend = 25f },
            new() { length = 0.14f, height = 0.28f, lateralBend = 30f },
            new() { length = 0.12f, height = 0.18f, lateralBend = 35f },
            new() { length = 0.10f, height = 0.10f, lateralBend = 40f },
            new() { length = 0.09f, height = 0.04f, lateralBend = 45f },
        };
        return p;
    }

    public static FishProfile Eel()
    {
        var p = ScriptableObject.CreateInstance<FishProfile>();
        p.followSpeed = 0.09f;
        p.solverIterations = 8;
        int count = 18;
        p.segments = new FishProfile.SegmentData[count];
        for (int i = 0; i < count; i++)
        {
            float t = i / (float)(count - 1);
            p.segments[i] = new FishProfile.SegmentData
            {
                length      = 0.12f,
                height      = Mathf.Lerp(0.14f, 0.03f, t * t),
                lateralBend = Mathf.Lerp(20f, 50f, t)
            };
        }
        return p;
    }

    public static FishProfile Shark()
    {
        var p = ScriptableObject.CreateInstance<FishProfile>();
        p.followSpeed = 0.18f;
        p.solverIterations = 4;
        p.segments = new FishProfile.SegmentData[]
        {
            new() { length = 0.35f, height = 0.30f, lateralBend = 8f  },
            new() { length = 0.30f, height = 0.42f, lateralBend = 10f },
            new() { length = 0.28f, height = 0.40f, lateralBend = 12f },
            new() { length = 0.25f, height = 0.35f, lateralBend = 15f },
            new() { length = 0.22f, height = 0.28f, lateralBend = 20f },
            new() { length = 0.18f, height = 0.20f, lateralBend = 28f },
            new() { length = 0.15f, height = 0.12f, lateralBend = 38f },
            new() { length = 0.12f, height = 0.05f, lateralBend = 45f },
        };
        p.fins = new FishFin[]
        {
            // Dorsal — tall, swept back
            new() {
                attachSegment   = 3,
                offset          = new Vector2(-0.05f, 0f),
                size            = new Vector2(0.15f, 0.38f),
                mirrorOtherSide = false,
                axis            = FinAxis.Dorsal
            },
        };
        return p;
    }

    public static FishProfile Minnow()
    {
        var p = ScriptableObject.CreateInstance<FishProfile>();
        p.followSpeed = 0.22f;
        p.solverIterations = 3;
        p.segments = new FishProfile.SegmentData[]
        {
            new() { length = 0.08f, height = 0.10f, lateralBend = 20f },
            new() { length = 0.08f, height = 0.14f, lateralBend = 25f },
            new() { length = 0.07f, height = 0.13f, lateralBend = 30f },
            new() { length = 0.06f, height = 0.09f, lateralBend = 35f },
            new() { length = 0.05f, height = 0.05f, lateralBend = 42f },
        };
        return p;
    }
}