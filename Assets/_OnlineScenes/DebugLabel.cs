using UnityEngine;
using System.Collections;

public class DebugLabel : MonoBehaviour {

    public static DebugLabel Instance;
    public UILabel label;

    void Awake()
    {
        if (!Instance)
        {
            Instance = this;
        }
    }

	public void ShowMsg(string msg)
    {
        label.text = msg;
    }
}
