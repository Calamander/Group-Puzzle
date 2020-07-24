using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TriggerBlock : MonoBehaviour
{
    public Dropdown event_dropdown;
    public Dropdown condition_dropdown;
    public GameObject conditions_container;
    public Dropdown action_dropdown;
    public GameObject actions_container;
    public Button add_condition_button;
    public Button add_action_button;
    public GameObject prop_prefab;
    //    public int trigger_id = -1;
    public Trigger trigger;
    public ProgramMenu program_menu;
    // Start is called before the first frame update
    void Start()
    {
        add_condition_button.onClick.AddListener(delegate { OnClickAddCondition(); });
        add_action_button.onClick.AddListener(delegate { OnClickAddAction(); });
        InitTrigger();
    }
    void InitTrigger()
    {
        if (trigger == null)
        {
            trigger = new Trigger();
            GameData.triggers.Add(trigger);
        }
        List<Dropdown.OptionData> evt_options = new List<Dropdown.OptionData>();
        foreach (Event evt in GameData.events.Values)
        {
            evt_options.Add(new Dropdown.OptionData(evt.key));
        }
        event_dropdown.AddOptions(evt_options);
        if (trigger.evt == null)
            trigger.evt = evt_options[0].text;
        else
        {
            event_dropdown.value = evt_options.FindIndex(x => x.text == trigger.evt);
        }
        InitConditions();
        InitActions();
    }
    void InitConditions()
    {
        List<Dropdown.OptionData> cond_options = new List<Dropdown.OptionData>();
        foreach (EventProperty cond in GameData.conditions.Values)
        {
            cond_options.Add(new Dropdown.OptionData(cond.key));
        }
        condition_dropdown.AddOptions(cond_options);
        if (trigger.conds.Count > 0)
        {
            foreach(string cond in trigger.conds.Keys)
            {
                OnClickAddCondition(cond);
            }
        }
    }
    void InitActions()
    {
        List<Dropdown.OptionData> act_options = new List<Dropdown.OptionData>();
        foreach (EventProperty act in GameData.actions.Values)
        {
            act_options.Add(new Dropdown.OptionData(act.key));
        }
        action_dropdown.AddOptions(act_options);
        if (trigger.acts.Count > 0)
        {
            foreach (string act in trigger.acts.Keys)
            {
                OnClickAddAction(act);
            }
        }

    }
    public void OnClickRemoveTrigger()
    {
        GameData.triggers.Remove(trigger);
        Destroy(gameObject);
        program_menu.ResetScroll(1f,true);
    }
    public void OnChangeEvent()
    {
        trigger.evt = event_dropdown.options[event_dropdown.value].text;
    }
    public void OnClickAddCondition(string cond = null)
    {
        EventProperty prop = GameData.conditions[cond ?? condition_dropdown.options[condition_dropdown.value].text];
        GameObject new_item = Instantiate(prop_prefab);
        CondActItem new_item_script = new_item.GetComponent<CondActItem>();
        new_item_script.prop = prop;
        new_item_script.trigger = trigger;
        new_item.transform.SetParent(conditions_container.transform);
    }
    public void OnClickAddAction(string act = null)
    {
        EventProperty prop = GameData.actions[act ?? action_dropdown.options[action_dropdown.value].text];
        GameObject new_item = Instantiate(prop_prefab);
        CondActItem new_item_script = new_item.GetComponent<CondActItem>();
        new_item_script.prop = prop;
        new_item_script.trigger = trigger;
        new_item.transform.SetParent(actions_container.transform);
    }
}
