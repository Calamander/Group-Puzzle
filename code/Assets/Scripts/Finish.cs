using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Finish : MonoBehaviour
{
    private List<GameObject> finishers = new List<GameObject>();
    // Start is called before the first frame update
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.CompareTag("Player") && finishers.IndexOf(collision.transform.gameObject) == -1)
        {
            Unit unit = collision.transform.gameObject.GetComponent<Unit>();
            unit.Finish();
            LevelEngine.current.OnUnitFinished();
        }
    }
}
