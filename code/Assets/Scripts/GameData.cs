using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Parameter
{
    public enum TYPE { FLOAT, LIST }
    public TYPE type;
    public string[] list;
    public float[] range;
    public float def_val = 0;
    public Parameter(float def = 0, float[] range = null)
    {
        type = TYPE.FLOAT;
        this.range = range;
        def_val = def;
        if (def != 0 && range != null)
        {
            def_val = range[0];
        }
    }
    public Parameter(string[] list)
    {
        type = TYPE.LIST;
        this.list = list;
    }
}
public class EventProperty : IEquatable<EventProperty>
{
    public string key;
    public string text;
    public string description;
    public enum TYPE { ACTION, CONDITION };
    public TYPE type;
    public Dictionary<string, Parameter> parameters;
    public EventProperty(string key, string text = null)
    {
        this.key = key;
        if (text == null)
            this.text = key;
        else
            this.text = text;
    }
    public bool Equals(EventProperty other)
    {
        if (other == null) return false;
        return type == other.type && key == other.key;
    }
}
public class Event : IEquatable<Event>
{
    public string key;
    public string text;
    public List<string> allowed_conditions = new List<string>();
    public List<string> denied_conditions = new List<string>();
    public List<string> allowed_actions = new List<string>();
    public List<string> denied_actions = new List<string>();
    public Event(string key, string text = null)
    {
        this.key = key;
        if (text == null)
            this.text = key;
        else
            this.text = text;
    }
    public bool Equals(Event other)
    {
        if (other == null) return false;
        return key == other.key;
    }
}
public class Trigger {
    public string evt = null;
    public Dictionary<string,Dictionary<string,string>> conds = new Dictionary<string, Dictionary<string, string>>();
    public Dictionary<string, Dictionary<string, string>> acts = new Dictionary<string, Dictionary<string, string>>();
}
public class Goal {
    public string text = null;
    public int save = -1; // -1 - disabled, 0 - all, 1+ - other
    public int kill = -1; // -1 - disabled, 0 - all, 1+ - other
    public int group_min = 0;
    public int group_max = 0;
    public int time_limit = 0;
    public bool allowing_props = true;
    public List<string> cond_list = null;
    public List<string> act_list = null;
    public bool finished = false;
    public bool failed = false;
    public bool is_primary;
    private int saved = 0;
    private int killed = 0;
    private float time = 0;
    private float group = 0;
    public void Reset(int group)
    {
        saved = killed = 0;  time = 0; this.group = group;
    }
    public void OnUnitSaved()
    {
        saved += 1;
        if (kill == 0 && saved > save || kill > 0 && group - saved < kill)
            failed = true;
    }
    public void OnUnitDied()
    {
        killed += 1;
        if (save == 0 && killed > kill || save > 0 && group - killed < save)
            failed = true;
    }
    public void OnUpdate()
    {
        time += Time.deltaTime;
        if (time >= time_limit)
            failed = true;
    }
    public void CheckResults()
    {
        bool done = true;
        if (!failed && !finished)
        {
            if (kill > -1 && save > -1)
                done = kill == 0 && saved == save && killed == group - save || save == 0 && killed == kill && saved == group - kill;
            else if (save > -1 && (save == 0 && saved < group || save != 0 && saved < save))
                done = false;
            else if (kill > -1 && (kill == 0 && killed < group || kill != 0 && killed < kill))
                done = false;
            finished = done;
        }
    }
    public string[] GetText()
    {
        CheckResults();
        string[] ret;
        if (finished) ret = new string[] { text };
        else if (failed) ret = new string[] { text, "failed" };
        else ret = new string[] { text, save > -1? $"s:{saved}/{(save>0?save:group)}" : "", kill > -1? $"k:{killed}/{(kill>0?kill:group)}" : "", time_limit > 0? $"t:{time.ToString("F0")}/{time_limit}":"" };
        return ret;
    }
}
public class Level
{
    public int id = 0;
    public string name = null;
    public Goal primary;
    public List<Goal> optional = new List<Goal>();
    private List<Goal> all_goals;
    private List<Goal> save_goals = new List<Goal>();
    private List<Goal> kill_goals = new List<Goal>();
    private List<Goal> time_goals = new List<Goal>();
    public Level(int id, string name, Goal primary, List<Goal> optional)
    {
        this.id = id;
        this.name = name;
        this.primary = primary;
        if (optional != null)
            this.optional = optional;
        all_goals = new List<Goal>(this.optional);
        all_goals.Insert(0, primary);
        primary.is_primary = true;
        foreach(var goal in this.optional)
        {
            goal.is_primary = false;
        }
    }
    public void Reset(int group)
    {
        save_goals.Clear();
        kill_goals.Clear();
        time_goals.Clear();
        foreach (var goal in all_goals)
        {
            if (group >= goal.group_min && (goal.group_max == 0 || group <= goal.group_max))
            {
                goal.Reset(group);
                goal.failed = false;
                if (goal.save > -1) save_goals.Add(goal);
                if (goal.kill > -1) kill_goals.Add(goal);
                if (goal.time_limit > 0) time_goals.Add(goal);
            }
            else
            {
                goal.failed = true;
            }
        }
    }
    public bool OnUnitSaved()
    {
        foreach (var goal in save_goals)
            goal.OnUnitSaved();
        return save_goals.Count > 0;
    }
    public bool OnUnitDied()
    {
        foreach (var goal in save_goals)
            goal.OnUnitDied();
        return save_goals.Count > 0;
    }
    public bool OnUpdate()
    {
        foreach (var goal in time_goals)
            goal.OnUpdate();
        return time_goals.Count > 0;
    }
}
public class GameData : MonoBehaviour
{
    public static int level = 0;
    public static Dictionary<string, Event> events = new Dictionary<string, Event>();
    public static Dictionary<string, EventProperty> conditions = new Dictionary<string, EventProperty>();
    public static Dictionary<string, EventProperty> actions = new Dictionary<string, EventProperty>();
    public static List<Trigger> triggers = new List<Trigger>();
    public static Dictionary<string, Level> levels = new Dictionary<string, Level>();
    public enum MENU { START, LEVEL, PROGRAM };
    public static MENU to_load = MENU.START;
    public static GameData instance = null;
    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            /// INIT EVENTS
            AddEvent("Loop", "Game Loop");
            AddEvent("Collision");
            AddEvent("Falling started");
            AddEvent("Landed");
            //AddEvent("Death");
            /// INIT CONDITIONS
            AddCondition("Group relation", "Part of a group", new Dictionary<string, Parameter>()
            { { "relation", new Parameter(new string[] { "Yes", "No" }) }
            });
            AddCondition("Collider type & side", "[Collision event only] Collider (type) and collision (side)", new Dictionary<string, Parameter>()
                { { "type", new Parameter(new string[] { "any", "unit", "environment" }) },
                  { "side", new Parameter(new string[] { "any", "sides", "bottom", "top" }) } });
            //AddCondition("Delay", "Delay from first event trigger until actions start (seconds)", new Dictionary<string, Parameter>()
                //{ { "seconds", new Parameter(0,new float[]{0, 5 }) } }); // no time to implement
            /// INIT ACTIONS
            AddAction("Move", "Move (direction)", new Dictionary<string, Parameter>()
                { { "direction", new Parameter(new string[] {"left","right"}) } });
            AddAction("Dash", "[Non-loop] Dash (direction)", new Dictionary<string, Parameter>()
                { { "direction", new Parameter(new string[] {"left","right","up","down"}) } });
            AddAction("Jump");
            AddAction("Switch direction", "[Non-loop events] Switch movement direction");
            AddAction("Toggle movement", "[Move action required] (Toggle) movement", new Dictionary<string, Parameter>()
                { { "toggle", new Parameter(new string[] { "toggle","stop","restart"}) } });
            /// INIT LEVEL GOALS
            AddLevel("The Pit",
                new Goal {
                    text = "Can anyone overcome this?",
                    save = 1,
                },
                new List<Goal> {
                    new Goal
                    {
                        text = "Everybody want to live!",
                        save = 0,
                    },
                    new Goal
                    {
                        text = "Only 20 seconds left to leave this place. Alone.",
                        time_limit = 20,
                        save = 1
                    }
                }
            );
            AddLevel("Connor MacLeod",
                new Goal {
                    text = "There Can Be Only One",
                    save = 1,
                    kill = 0
                },
                new List<Goal> {
                    new Goal
                    {
                        text = "...Or two",
                        save = 2,
                        kill = 0
                    }});
            AddLevel("Vertigo",
                new Goal { 
                    text = "Three medals await their champions",
                    save = 3
                    }
                );
            AddLevel("Labyrinth",
                new Goal {
                    text = "Long before Theseus only few lived to tell the story of Minotaurus",
                    save = 1
                });
        }
    }
    public static void ResetTriggers()
    {
        triggers.Clear();
    }
    public static void AddEvent(string key, string text = null)
    {
        events[key] = new Event(key, text);
    }
    public static void AddCondition(string key, string text = null, Dictionary<string, Parameter> prms = null)
    {
        conditions[key] = new EventProperty(key, text)
        {
            type = EventProperty.TYPE.CONDITION,
            parameters = prms
        };
    }
    public static void AddAction(string key, string text = null, Dictionary<string, Parameter> prms = null)
    {
        actions[key] = new EventProperty(key, text)
        {
            type = EventProperty.TYPE.ACTION,
            parameters = prms
        };
    }
    public static void AddLevel(string name, Goal primary, List<Goal> optional = null, int id = -1)
    {
        if (id == -1) id = levels.Count + 1;
        string key = "Level" + id.ToString("D2");
        levels[key] = new Level(id, name, primary, optional);
    }
}
