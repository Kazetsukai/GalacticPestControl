using UnityEngine;
using System.Collections;
using UnityEditor;

public class DuckCircleCollider : DuckCollider
{
    [Header("Properties")]
    [SerializeField] float radius = 1f;

    public float Radius
    {
        get
        {
            return radius;
        }      
    }       

    void OnValidate()
    {
        //Called by editor when this script's values are changed.
        RecalculateAABB();
    }

    void OnDrawGizmos()
    {
        //Draw collider bounds
        Handles.color = Color.green;
        Handles.DrawWireDisc(transform.position, Vector3.forward, Radius);

        //Draw AABB
        Handles.color = Color.yellow;
        Handles.DrawLine(WorldAABB.Min, new Vector3(WorldAABB.Max.x, WorldAABB.Min.y));
        Handles.DrawLine(new Vector3(WorldAABB.Max.x, WorldAABB.Min.y), WorldAABB.Max);
        Handles.DrawLine(WorldAABB.Max, new Vector3(WorldAABB.Min.x, WorldAABB.Max.y));
        Handles.DrawLine(new Vector3(WorldAABB.Min.x, WorldAABB.Max.y), WorldAABB.Min);
    }  

    void Awake()
    {
        RecalculateAABB();
    }

    public void SetRadius(float newRadius)
    {
        radius = newRadius;
        RecalculateAABB();
    }

    void RecalculateAABB()
    {
        aabb.Min = new Vector2(-radius, radius);
        aabb.Max = new Vector2(radius, -radius);
    }
}
