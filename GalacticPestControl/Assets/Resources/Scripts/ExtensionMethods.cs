using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class ExtensionMethods
{
    /// <summary>
    /// An extension method by Joe. Takes a Rigidbody2D and moves it like Rigidbody.Move, but using force (so that the Rigidbody is not moved kinematically). 
    /// </summary>
    /// <param name="rb"></param>
    /// <param name="targetVelocity"></param>
    /// <param name="accelerationDuration"></param>
    public static void ForceMove(this Rigidbody2D rb, Vector2 targetVelocity)
    {
        Vector3 Vf = targetVelocity;
        Vector3 Vi = rb.velocity;

        Vector3 F = (rb.mass * (Vf - Vi)) / 0.05f;      //0.05 is a good time. Feels just like Rigidbody.Move           
        rb.AddForce(F, ForceMode2D.Force);
    }

    /// <summary>
    /// Extension method by Joe. Returns the angle between this Vector and another Vector, assuming Z-axis as axis of rotation and Vector2.Up as 0 degrees (i.e, NORTH). Increases clockwise until 360 degrees.
    /// </summary>
    /// <param name="thisVector"></param>
    /// <param name="otherVector"></param>
    /// <returns></returns>
    public static float Angle360To(this Vector3 thisVector, Vector3 otherVector)
    {
        return Vector3.Angle(thisVector, otherVector) * (Vector3.Cross(thisVector, otherVector).z > 0 ? -1f : 1f);
    }
}

