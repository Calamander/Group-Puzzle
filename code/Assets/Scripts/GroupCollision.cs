using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroupCollision : MonoBehaviour
{
    private Unit parent_unit;
    private Collider2D parent_collider;
    private void Awake()
    {
        parent_unit = transform.parent.gameObject.GetComponent<Unit>();
        parent_collider = transform.parent.gameObject.GetComponent<Collider2D>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision != parent_collider && !parent_unit.group.Contains(collision.gameObject))
            parent_unit.group.Add(collision.gameObject);
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision != parent_collider && parent_unit.group.Contains(collision.gameObject))
            parent_unit.group.Remove(collision.gameObject);
    }
}
