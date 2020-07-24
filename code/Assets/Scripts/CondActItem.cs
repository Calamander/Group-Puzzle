using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CondActItem : MonoBehaviour
{
    public EventProperty prop;
    public Trigger trigger;
    public Text text;
    public GameObject dropdown_prefab;
    public GameObject input_prefab;
    public Transform description_block;
    // Start is called before the first frame update
    void Start()
    {
        text.text = prop.text;
        Dictionary<string, Parameter> parameters;
        if (prop.type == EventProperty.TYPE.ACTION)
        {
            parameters = GameData.actions[prop.key].parameters;
            if (!trigger.acts.ContainsKey(prop.key))
                trigger.acts[prop.key] = new Dictionary<string, string>();
        }
        else
        {
            parameters = GameData.conditions[prop.key].parameters;
            if (!trigger.conds.ContainsKey(prop.key))
                trigger.conds[prop.key] = new Dictionary<string, string>();
        }
        if (parameters != null)
        {
            foreach (KeyValuePair<string, Parameter> prm in parameters)
            {
                if (prm.Value.type == Parameter.TYPE.LIST)
                {
                    GameObject new_item = Instantiate(dropdown_prefab);
                    Dropdown dropdown = new_item.GetComponent<Dropdown>();
                    dropdown.onValueChanged.AddListener(delegate { OnParameterChange(new_item, prm.Key, prm.Value.type); });
                    List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
                    foreach (string opt in prm.Value.list)
                    {
                        options.Add(new Dropdown.OptionData(opt));
                    }
                    dropdown.AddOptions(options);
                    if (prop.type == EventProperty.TYPE.ACTION)
                    {
                        if (!trigger.acts[prop.key].ContainsKey(prm.Key))
                            trigger.acts[prop.key][prm.Key] = options[0].text;
                        else
                            dropdown.value = options.FindIndex(x => x.text == trigger.acts[prop.key][prm.Key]);

                    }
                    else
                    {
                        if (!trigger.conds[prop.key].ContainsKey(prm.Key))
                            trigger.conds[prop.key][prm.Key] = options[0].text;
                        else
                            dropdown.value = options.FindIndex(x => x.text == trigger.conds[prop.key][prm.Key]);

                    }
                    new_item.transform.SetParent(description_block);
                }
                else
                {
                    GameObject new_item = Instantiate(input_prefab);
                    InputField input = new_item.GetComponent<InputField>();
                    input.onValueChanged.AddListener(delegate { OnParameterChange(new_item, prm.Key, prm.Value.type); });
                    if (prop.type == EventProperty.TYPE.ACTION)
                    {
                        if (!trigger.acts[prop.key].ContainsKey(prm.Key))
                            trigger.acts[prop.key][prm.Key] = prm.Value.def_val.ToString();
                        input.text = trigger.acts[prop.key][prm.Key];
                    }
                    else
                    {
                        if (!trigger.conds[prop.key].ContainsKey(prm.Key))
                            trigger.conds[prop.key][prm.Key] = prm.Value.def_val.ToString();
                        input.text = trigger.conds[prop.key][prm.Key];
                    }
                    new_item.transform.SetParent(description_block);

                }
            }
        }
    }
    public void OnParameterChange(GameObject obj, string key, Parameter.TYPE type)
    {
        string val;
        if (type == Parameter.TYPE.LIST)
        {
            Dropdown dropdown = obj.GetComponent<Dropdown>();
            val = dropdown.options[dropdown.value].text;
        }
        else
        {
            InputField input = obj.GetComponent<InputField>();
            val = input.text;
        }
        //Debug.Log(val+";"+key);
        if (prop.type == EventProperty.TYPE.ACTION)
            trigger.acts[prop.key][key] = val;
        else
            trigger.conds[prop.key][key] = val;
    }
    public void OnClickRemove()
    {
        if (prop.type == EventProperty.TYPE.ACTION)
        {
            trigger.acts.Remove(prop.key);
        }
        else
        {
            trigger.conds.Remove(prop.key);
        }
        Destroy(gameObject);
    }
}
