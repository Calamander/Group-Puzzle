using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ClosedList
{
    public static int PrevIndex<T>(this List<T> list, int index)
    {
        return index > 0 ? index - 1 : list.Count-1;
    }
    public static int NextIndex<T>(this List<T> list, int index)
    {
        return index < list.Count-1 ? index + 1 : 0;
    }
    public static void InsertBetween<T>(this List<T> list, int index1, int index2, T item)
    {
        int max_index = index1 > index2 ? index1 : index2;
        if (max_index + 1 == list.Count)
            max_index = list.Count;
        list.Insert(max_index, item);
    }
    public static void MergeListByIndexes<T>(this List<T> list1, int list1_i1, int list1_i2, List<T> list2, int list2_i1, int list2_i2)
    {
        if (list1.PrevIndex(list1_i2) != list1_i1)
        {
            int temp = list1_i2;
            list1_i2 = list1_i1;
            list1_i1 = temp;
            temp = list2_i2;
            list2_i2 = list2_i1;
            list2_i1 = temp;
        }
        if (list2.PrevIndex(list2_i2) == list2_i1)
        {
            int list2_icur = list2.NextIndex(list2_i2);
            while (list2_icur != list2_i1)
            {
                list1.Insert(list1_i2, list2[list2_icur]);
                list2_icur = list2.NextIndex(list2_icur);
            }
        }
        else
        {
            int list2_icur = list2.PrevIndex(list2_i2);
            while (list2_icur != list2_i1)
            {
                list1.Insert(list1_i2, list2[list2_icur]);
                list2_icur = list2.PrevIndex(list2_icur);
            }
        }
    }
    public static T PopAt<T>(this List<T> list, int index)
    {
        T r = list[index];
        list.RemoveAt(index);
        return r;
    }
}

[ExecuteInEditMode]
public class EditorAutoEdgeCollider : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
    }
    private void ApplyEdgeCollider()
    {
        LineRenderer line = gameObject.GetComponent<LineRenderer>();
        if (line != null)
        {
            EdgeCollider2D collider = gameObject.GetComponent<EdgeCollider2D>();
            if (collider == null)
                collider = gameObject.AddComponent<EdgeCollider2D>();
            Vector3[] line_points = new Vector3[line.positionCount];
            Vector2[] collider_points = new Vector2[line.positionCount + 1];
            line.GetPositions(line_points);
            for (int i = 0; i < line.positionCount; i++)
            {
                collider_points[i] = (Vector2)line_points[i];
            }
            collider_points[line.positionCount] = collider_points[0];
            collider.points = collider_points;
        }
        else
        {
            SpriteRenderer sprite_r = gameObject.GetComponent<SpriteRenderer>();
            if (sprite_r != null)
            {
                int actual_vertices = sprite_r.sprite.vertices.Length;
                ushort[] triangles = (ushort[]) sprite_r.sprite.triangles.Clone();
                for (int i=0; i < sprite_r.sprite.vertices.Length; i++)
                {
                    bool found_double = false;
                    for (int j = i+1; j < sprite_r.sprite.vertices.Length && !found_double; j++)
                    {
                        if (sprite_r.sprite.vertices[i] == sprite_r.sprite.vertices[j])
                        {
                            found_double = true;
                            for (int k=0; k < triangles.Length; k++)
                            {
                                if (triangles[k] == i)
                                    triangles[k] = (ushort)j;
                            }
                            //Debug.Log($"indexes {i} and {j} are the same");
                        }
                    }
                    if (found_double)
                        actual_vertices -= 1;
                }
                //collider.points = sprite_r.sprite.vertices;
                //for (int i=0; i < triangles.Length; i += 3)
                //{
                //    EdgeCollider2D collider = gameObject.AddComponent<EdgeCollider2D>();
                //    collider.points = new Vector2[] { sprite_r.sprite.vertices[triangles[i]], sprite_r.sprite.vertices[triangles[i+1]], sprite_r.sprite.vertices[triangles[i+2]] };
                //}
                List<List<ushort>> polygons = new List<List<ushort>>();
                if (triangles.Length > 3)
                    GeneratePolygons(triangles, polygons);
                Debug.Log($"vertices={sprite_r.sprite.vertices.Length};actual_vertices={actual_vertices};triangles={triangles.Length};polygons={polygons.Count}");
                
                foreach (List<ushort> polygon in polygons)
                {
                    EdgeCollider2D collider = gameObject.AddComponent<EdgeCollider2D>();
                    Vector2[] points = new Vector2[polygon.Count + 1];
                    for (int i = 0; i < polygon.Count; i++)
                    {
                        points[i] = sprite_r.sprite.vertices[polygon[i]];
                    }
                    points[points.Length - 1] = points[0];
                    collider.points = points;
                }
            }
        }
    }
    private void GeneratePolygons(ushort[] triangles, List<List<ushort>> polygons)
    {
        List<ushort> cur_poly = new List<ushort>(new ushort[] { triangles[0], triangles[1], triangles[2] });
        for (int i = 3; i < triangles.Length; i += 3)
        {
            bool merged = MergePolygonAndTriangle(cur_poly, new ushort[] { triangles[i], triangles[i + 1], triangles[i + 2] });
            if (!merged)
            {
                polygons.Add(cur_poly);
                cur_poly = new List<ushort>(new ushort[] { triangles[i], triangles[i + 1], triangles[i + 2] });
            }
            //Debug.Log(String.Format("{0};{1};{2}",new ushort []{ sprite.triangles[i], sprite.triangles[i+1], sprite.triangles[i+2] }));
            //Debug.Log($"{triangles[i]};{triangles[i + 1]};{triangles[i + 2]}");
        }
        polygons.Add(cur_poly);
        List<List<ushort>> unmerged_polygons = new List<List<ushort>>();
        while (polygons.Count > 0 && polygons.Count+unmerged_polygons.Count > 1)
        {
            for (int i = 0; i < polygons.Count; i++)
            {
                bool merged_at_least_one = false;
                for (int j = polygons.Count - 1; j > i; j--)
                {
                    bool is_success = MergePolygons(polygons[i], polygons[j]);
                    if (is_success)
                    {
                        polygons.RemoveAt(j);
                        merged_at_least_one = true;
                    }
                }
                if (!merged_at_least_one)
                    unmerged_polygons.Add(polygons.PopAt(i));
            }
            for (int i = 0; i < polygons.Count; i++)
            {
                for (int j = unmerged_polygons.Count - 1; j >= 0; j--)
                {
                    bool is_success = MergePolygons(polygons[i], unmerged_polygons[j]);
                    if (is_success)
                        unmerged_polygons.RemoveAt(j);
                }
            }
        }
        polygons.AddRange(unmerged_polygons);
    }
    private bool MergePolygonAndTriangle(List<ushort> polygon, ushort[] triangle)
    {
        ushort v3 = 0;
        int i1 = polygon.IndexOf(triangle[0]), i2 = -1;
        if (i1 != -1)
        {
            i2 = polygon.PrevIndex(i1);
            if (polygon[i2] == triangle[1])
                v3 = triangle[2];
            else if (polygon[i2] == triangle[2])
                v3 = triangle[1];
            else
            {
                i2 = polygon.NextIndex(i1);
                if (polygon[i2] == triangle[1])
                    v3 = triangle[2];
                else if (polygon[i2] == triangle[2])
                    v3 = triangle[1];
                else
                    i2 = -1;
            }
        }
        if (i2 == -1)
        {
            i1 = polygon.IndexOf(triangle[1]);
            if (i1 != -1)
            {
                i2 = polygon.PrevIndex(i1);
                if (polygon[i2] == triangle[2])
                    v3 = triangle[0];
                else
                {
                    i2 = polygon.NextIndex(i1);
                    if (polygon[i2] == triangle[2])
                        v3 = triangle[0];
                    else
                        return false;
                }
            }
            else
                return false;
        }
        polygon.Insert(polygon.PrevIndex(i2) == i1 ? i2 : i1, v3);
        return true;
    }
    private bool MergePolygons(List<ushort> polygon1, List<ushort> polygon2)
    {
        for (int i1=0; i1 < polygon1.Count; i1++)
        {
            int i2 = polygon2.IndexOf(polygon1[i1]);
            if (i2 != -1)
            {
                int i1_2 = polygon1.NextIndex(i1), i2_2 = polygon2.NextIndex(i2);
                if (polygon1[i1_2] == polygon2[i2_2])
                {
                    polygon1.MergeListByIndexes(i1, i1_2, polygon2, i2, i2_2);
                    return true;
                }
                else
                {
                    i2_2 = polygon2.PrevIndex(i2);
                    if (polygon1[i1_2] == polygon2[i2_2])
                    {
                        polygon1.MergeListByIndexes(i1, i1_2, polygon2, i2, i2_2);
                        return true;
                    }
                }
            }
        }
        return false;
    }
    private void OnEnable()
    {
        ApplyEdgeCollider();
    }
}
