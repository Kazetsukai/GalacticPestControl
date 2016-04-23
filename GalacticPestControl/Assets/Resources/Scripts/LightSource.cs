using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class LightSource : MonoBehaviour
{
    [SerializeField] int RayCount = 200;
    [SerializeField] float RayDistance = 100f;
    [SerializeField] bool DebugRays = true;
    [SerializeField] bool DebugPoints = true;

    List<PolygonCollider2D> colliders;
    List<Vector2> raycastHitPoints;

    float rot = 0;

    Mesh lightMesh;
    MeshFilter meshFilter;

    void Start ()
    {
        meshFilter = GetComponent<MeshFilter>();
  	}
	
	void Update ()
    {             
        //Get list of all polygons
        colliders = FindObjectsOfType<PolygonCollider2D>().ToList();
        raycastHitPoints = new List<Vector2>();

        //Cast rays for each polygon
        foreach (PolygonCollider2D polygon in colliders)
        {           
            foreach(Vector2 vertex in polygon.points)
            {
                //Cast a ray to each end-point on line segments on polygon
                Vector2 dir = (polygon.transform.TransformPoint(vertex) - transform.position).normalized;
                RaycastHit2D hitDirect = Physics2D.Raycast(transform.position, dir);

                //Plus plus two more rays offset by +/- 0.00001 radians. This is needed to hit the wall(s) behind any given segment corner.
                Vector2 dirLeft = Quaternion.Euler(0, 0, -Mathf.Rad2Deg * 0.0001f) * dir;
                RaycastHit2D hitLeft = Physics2D.Raycast(transform.position, dirLeft);

                Vector2 dirRight = Quaternion.Euler(0, 0, Mathf.Rad2Deg * 0.0001f) * dir;
                RaycastHit2D hitRight = Physics2D.Raycast(transform.position, dirRight);

                if (hitDirect.collider != null)
                {
                    raycastHitPoints.Add(hitDirect.point);

                    if (DebugRays)
                    {
                        Debug.DrawLine(transform.position, hitDirect.point, Color.red);
                    }
                }

                if (hitLeft.collider != null)
                {
                    raycastHitPoints.Add(hitLeft.point);

                    if (DebugRays)
                    {
                        Debug.DrawLine(transform.position, hitLeft.point, Color.blue);
                    }
                }

                if (hitRight.collider != null)
                {
                    raycastHitPoints.Add(hitRight.point);

                    if (DebugRays)
                    {
                        Debug.DrawLine(transform.position, hitRight.point, Color.green);
                    }
                }                
            }           
        }

        //Sort raycastHits by their projection angle (relative to Vector2.up)
        raycastHitPoints = raycastHitPoints.Select(r => (Vector2)transform.InverseTransformPoint(r)).ToList();
        raycastHitPoints.Sort(new ClockwiseVector2Comparer());

        //Add central point to raycastHits
        raycastHitPoints.Add(Vector2.zero);
                
        //Generate polygon vertices:
        Vector3[] vertices = raycastHitPoints.Select(v => (Vector3)v).ToArray();

        //Generate polygon triangle indices:
        int[] indices = new int[vertices.Count() * 3];
        for (int i = 0; i < vertices.Length-1; i++)
        {
            var idx = i * 3;
            indices[idx] = vertices.Length - 1;
            indices[idx + 1] = i % (vertices.Length - 1);
            indices[idx + 2] = (i + 1) % (vertices.Length-1);
        }
        
        // Create the mesh
        lightMesh = new Mesh();
        lightMesh.vertices = vertices;
        lightMesh.triangles = indices;
        lightMesh.RecalculateNormals();
        lightMesh.RecalculateBounds();
        meshFilter.mesh = lightMesh;
    }
}
