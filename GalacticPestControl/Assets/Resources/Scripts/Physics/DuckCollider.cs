using UnityEngine;
using System.Collections;

public class DuckCollider : MonoBehaviour
{
    [HideInInspector]
    public AABB aabb = new AABB();

    public AABB WorldAABB
    {
        get
        {
            return new AABB((Vector2)transform.position + aabb.Min, (Vector2)transform.position + aabb.Max);
        }    
    }

    void Start()
    {

    }
    
    void Update()
    {

    }
}
