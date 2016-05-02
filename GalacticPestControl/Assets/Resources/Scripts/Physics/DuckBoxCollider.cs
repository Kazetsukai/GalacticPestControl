using UnityEngine;
using System.Collections;
using UnityEditor;

public class DuckBoxCollider : DuckCollider
{    
    [Header("Properties")]
    [SerializeField] Vector2 dimensions = new Vector2(1,1);


    void OnValidate()
    {
        //Called by editor when this script's values are changed.
        RecalculateAABB();
    }

    void OnDrawGizmos()
    {
        //Draw collider bounds
        Handles.color = Color.green;
        Handles.DrawLine(WorldAABB.Min, new Vector3(WorldAABB.Max.x, WorldAABB.Min.y));
        Handles.DrawLine(new Vector3(WorldAABB.Max.x, WorldAABB.Min.y), WorldAABB.Max);
        Handles.DrawLine(WorldAABB.Max, new Vector3(WorldAABB.Min.x, WorldAABB.Max.y));
        Handles.DrawLine(new Vector3(WorldAABB.Min.x, WorldAABB.Max.y), WorldAABB.Min);

        RecalculateAABB();
    }

    void Awake()
    {
        RecalculateAABB();
    }

    void Start()
    {

    }
    
    void Update()
    {

    }

    void RecalculateAABB()
    {
        aabb.Min = new Vector2(-dimensions.x * transform.localScale.x, dimensions.y * transform.localScale.y);
        aabb.Max = new Vector2(dimensions.x * transform.localScale.x, -dimensions.y * transform.localScale.y);    
    }
}
