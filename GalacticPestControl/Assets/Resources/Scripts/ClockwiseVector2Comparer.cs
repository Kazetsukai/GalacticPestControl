using System;
using System.Collections.Generic;
using UnityEngine;

public class ClockwiseVector2Comparer : IComparer<Vector2>
{
    public int Compare(Vector2 v1, Vector2 v2)
    {
        float angleV1 = Angle(v1);
        float angleV2 = Angle(v2);

        return Comparer<float>.Default.Compare(angleV1, angleV2);

        /*
        if (v1.x >= 0)
        {
            if (v2.x < 0)
            {
                return -1;
            }
            return -Comparer<float>.Default.Compare(v1.y, v2.y);
        }
        else
        {
            if (v2.x >= 0)
            {
                return 1;
            }
            return Comparer<float>.Default.Compare(v1.y, v2.y);
        }*/
    }

    public static float Angle(Vector2 v1)
    {
        float angleV1 = Vector3.Angle(Vector3.up, v1) * (Vector3.Cross(Vector3.up, v1).z > 0 ? -1f : 1f);
        if (angleV1 < 0)
        {
            angleV1 = 360 + angleV1;
        }
        return angleV1;
    }
}