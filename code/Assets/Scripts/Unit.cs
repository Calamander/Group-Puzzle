using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Unit : MonoBehaviour
{
    public static Dictionary<string, List<Trigger>> triggers = new Dictionary<string, List<Trigger>>();
    public static bool init = false;
    public static UnityEvent OnUnitDied = new UnityEvent();
    public Rigidbody2D body;
    public TextMesh number_mesh;
    public int number;
    private Collision2D current_collision;
    private float sideDistance = 0;
    private bool direction_switched = false;
    private HashSet<string> processed_triggers = new HashSet<string>();
    private bool falling = false;
    private bool can_move = true;
    private float dash_timeout_time = 0;
    public List<GameObject> group = new List<GameObject>();
    private void Awake()
    {
        if (!init)
        {
            init = true;
            Reset();
        }
    }
    void Start()
    {
        sideDistance = GetComponent<Collider2D>().bounds.extents.y * .75f;
        body = gameObject.GetComponent<Rigidbody2D>();
        number_mesh.text = number.ToString();
    }
    public void Reset()
    {
        group.Clear();
        // categorising triggers to process events faster
        foreach (var key in GameData.events.Keys)
            triggers[key] = new List<Trigger>();
        foreach (Trigger trigger in GameData.triggers)
            triggers[trigger.evt].Add(trigger);
    }
    bool IsCollidingWith(string side = null, string type = null, Collision2D collision = null)
    {
        if (side == "any") side = null;
        if (type == "any") type = null;
        ContactPoint2D[] contacts = new ContactPoint2D[collision == null ? 10 : collision.contactCount];
        int num = collision == null ? GetComponent<Collider2D>().GetContacts(contacts) : collision.GetContacts(contacts);
        bool colliding = false;
        if (num > 0)
        {
            if (type == null && side == null)
                colliding = true;
            else
                for(int i = 0; i < num; i++)
                {
                    var contact = contacts[i];
                    if (type != null)
                    {
                        bool is_player = contact.collider.CompareTag("Player");
                        if (type == "environment" && is_player || type == "unit" && !is_player)
                            continue;
                        if (side == null)
                        {
                            colliding = true;
                            break;
                        }

                    }
                    if (side != null)
                    {
                        if (side == "sides")
                        {
                            if (transform.position.x - sideDistance > contact.point.x
                                || transform.position.x + sideDistance < contact.point.x)
                            {
                                colliding = true;
                                break;
                            }
                        }
                        else if (side == "bottom")
                        {
                            if (transform.position.y - sideDistance > contact.point.y)
                            {
                                colliding = true;
                                break;
                            }
                        }
                        else //if (side == "top")
                        {
                            if (transform.position.y + sideDistance < contact.point.y)
                            {
                                colliding = true;
                                break;
                            }
                        }
                    }
                }
        }
        return colliding;
    }
    // Update is called once per frame
    void Update()
    {
        dash_timeout_time = dash_timeout_time > Time.deltaTime ? dash_timeout_time-Time.deltaTime : 0;
        processed_triggers.Clear();
        if (transform.position.y < -5)
        {
            Death();
        }
        else
        {
            foreach (Trigger trigger in triggers["Loop"])
                ProcessTrigger(trigger);
            if (!IsCollidingWith("bottom"))
            {
                if (!falling)
                {
                    falling = true;
                    foreach (Trigger trigger in triggers["Falling started"])
                        ProcessTrigger(trigger);
                }
            }
            else 
            {
                if (falling)
                {
                    falling = false;
                    foreach (Trigger trigger in triggers["Landed"])
                        ProcessTrigger(trigger);
                }
            }
        }
    }
    bool ConditionsSatisfied(Trigger trigger)
    {
        bool satisfied = true;
        if (!processed_triggers.Contains(trigger.evt))
        {
            foreach (var condition in trigger.conds)
            {
                switch (condition.Key)
                {
                    case "Group relation":
                        if (condition.Value["relation"] == "No")
                            satisfied = !satisfied || group.Count > (trigger.evt == "Collision" ? 1 : 0) ? false : true;
                        else
                            satisfied = !satisfied || group.Count <= (trigger.evt == "Collision" ? 1 : 0) ? false : true;
                        break;
                    case "Collider type & side":
                        if (trigger.evt == "Collision")
                            if (!IsCollidingWith(condition.Value["side"], condition.Value["type"], current_collision))
                                satisfied = false;
                        break;
                }
            }
            if (satisfied)
                processed_triggers.Add(trigger.evt);
        }
        else satisfied = false;
        return satisfied;
    }
    void ProcessTrigger(Trigger trigger)
    {
        if (ConditionsSatisfied(trigger))
            RunActions(trigger);
    }
    void RunActions(Trigger trigger)
    {
        //Vector2 force = Vector2.zero;
        float speedup;
        foreach (var action in trigger.acts)
        {
            switch (action.Key)
            {
                case "Move":
                    if(IsCollidingWith("bottom") && Math.Abs(body.angularVelocity) < LevelEngine.current.max_velocity)
                    {
                        speedup = Time.deltaTime * LevelEngine.current.move_speedup 
                            * (action.Value["direction"] == "right" && !direction_switched 
                            || action.Value["direction"] == "left" && direction_switched ? -1 : 1);
                        body.AddTorque(speedup,ForceMode2D.Force);
                        //force.x += speedup;
                    }
                    break;
                case "Dash":
                    if (trigger.evt != "Loop" && dash_timeout_time == 0)
                    {
                        dash_timeout_time = LevelEngine.current.dash_timeout;
                        Vector2 dir;
                        if (action.Value["direction"] == "left") dir = Vector2.left;
                        else if (action.Value["direction"] == "right") dir = Vector2.right;
                        else if (action.Value["direction"] == "up") dir = Vector2.up;
                        else dir = Vector2.down;
                        body.AddForce(dir * LevelEngine.current.dash_speed);
                    }
                    break;
                case "Switch direction":
                    direction_switched = !direction_switched;
                    break;
                case "Jump":
                    if (IsCollidingWith("bottom"))
                        body.AddForce(new Vector2(0,LevelEngine.current.jump_speed));
                    break;
                case "Toggle movement":
                    if (action.Value["toggle"] == "toggle") can_move = !can_move;
                    else can_move = action.Value["toggle"] == "start" ? true : false;
                    break;
            }
        }
        
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        current_collision = collision;
        foreach (Trigger trigger in triggers["Collision"])
        {
            ProcessTrigger(trigger);
        }
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        current_collision = collision;
        foreach (Trigger trigger in triggers["Collision"])
        {
            ProcessTrigger(trigger);
        }
    }
    private void Death()
    {
        //Debug.Log("DEAD");
        OnUnitDied.Invoke();
        Destroy(gameObject);
    }
    public void Finish()
    {
        //Debug.Log("FINISHED");
        Destroy(gameObject);
    }
}
