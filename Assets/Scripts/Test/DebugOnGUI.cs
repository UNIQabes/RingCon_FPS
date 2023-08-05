using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugOnGUI : MonoBehaviour
{
    static DebugOnGUI singleton=null;
    Dictionary<string,object> LogMessage;
    private static bool dictKeysDirtyFlag = false;
    // Start is called before the first frame update
    void Start()
    {
        singleton = this;
        LogMessage = new Dictionary<string, object>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void LateUpdate()
    {
        //LogMessage.Clear();
    }

    public static void Log(object message,string key)
    {
        if (singleton)
        {
            if (singleton.LogMessage.ContainsKey(key))
            {
                singleton.LogMessage[key] = message;
            }
            else
            {
                singleton.LogMessage.Add(key,message);
                dictKeysDirtyFlag = true;
            }
        }
        
    }

    void OnGUI()
    {
        
        if (singleton)
        {
            if (dictKeysDirtyFlag)
            {
                if (Event.current.type != EventType.Layout)
                {
                    return;
                }
                else
                {
                    dictKeysDirtyFlag = false;
                }
                
            }
            //GUILayout.Label($"ddddd");
            GUIStyle style = GUI.skin.GetStyle("label");
            style.fontSize = 36;
            style.padding = new RectOffset(0, 0, 0, 0);
            GUILayout.BeginHorizontal(GUILayout.Width(960));
            GUILayout.BeginVertical(GUILayout.Width(960));
            //GUILayout.Label($"ddddd");
            foreach (object aMessage in LogMessage.Values)
            {
                //Debug.Log("dd");
                GUILayout.Label($"{aMessage}");
            }
            
            
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }
        
    }
}
