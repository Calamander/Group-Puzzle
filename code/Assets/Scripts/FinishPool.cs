using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishPool : MonoBehaviour
{
    public uint unit_number_condition = 0;
    // Start is called before the first frame update
    private void Awake()
    {
        if (unit_number_condition > 0)
        {
            Unit.OnUnitDied.AddListener(OnUnitDied);
            LevelEngine.OnRestart.AddListener(Reset);
            foreach (Transform child in transform)
                child.gameObject.SetActive(false);
        }
    }
    private void OnUnitDied()
    {
        if(Spawner.all_spawned + Spawner.all_left_to_spawn <= unit_number_condition)
        {
            foreach (Transform child in transform)
                child.gameObject.SetActive(true);
        }
    }
    private void Reset()
    {
        foreach (Transform child in transform)
            child.gameObject.SetActive(false);
    }
}
