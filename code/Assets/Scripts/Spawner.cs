using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public float spawn_period = 1;
    public GameObject unit_prefab;
    public GameObject vortex;
    private float time = 0;
    private int left_to_spawn = 0;
    public static int all_left_to_spawn = 0;
    public static int all_spawned = 0;
    private static int max_number = 0;
    private float vortex_deg_per_sec = 90;
    private static bool can_spawn = true;
    private static bool init = false;
    // Start is called before the first frame update
    void Start()
    {
        if (!init)
        {
            init = true;
            Unit.OnUnitDied.AddListener(OnUnitDied);
        }
        LevelEngine.OnRestart.AddListener(Reset);
        Reset();
    }
    public void Reset()
    {
        time = 0;
        max_number = 0;
        all_spawned = 0;
        all_left_to_spawn = LevelEngine.current.group_size;
        foreach (Transform child in transform)
        {
            if(child.CompareTag("Player"))
                Destroy(child.gameObject);
        }
        int spawns_num = GameObject.FindGameObjectsWithTag("Respawn").Length;
        left_to_spawn = LevelEngine.current.group_size / spawns_num;
        if (spawns_num > 1 && transform.GetSiblingIndex() == 0 && left_to_spawn * spawns_num != LevelEngine.current.group_size)
            left_to_spawn += LevelEngine.current.group_size - left_to_spawn * spawns_num;
        time = spawn_period;
        if (left_to_spawn > 0)
        {
            //Spawn();
        }

    }
    // Update is called once per frame
    void Update()
    {
        if (left_to_spawn > 0)
        {
            vortex.transform.Rotate(new Vector3(0, 0, vortex_deg_per_sec * Time.deltaTime));
            if (can_spawn)
            {
                time += Time.deltaTime;
                if (time > spawn_period)
                {
                    time -= spawn_period;
                    Spawn();
                    if (LevelEngine.current.simultaneous_max_group != 0 && all_spawned >= LevelEngine.current.simultaneous_max_group)
                    {
                        can_spawn = false;
                    }
                }
            }
        }
    }
    void Spawn()
    {
        all_spawned += 1;
        left_to_spawn -= 1;
        all_left_to_spawn -= 1;
        GameObject unit = Instantiate(unit_prefab);
        unit.GetComponent<Unit>().number = ++max_number;
        unit.name = "Unit "+max_number;
        unit.transform.SetParent(transform, false);
    }
    public static void OnUnitDied()
    {
        all_spawned -= 1;
        if (all_spawned < LevelEngine.current.simultaneous_max_group)
            can_spawn = true;
    }
}
