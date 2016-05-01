using UnityEngine;
using System.Collections;
namespace PoolKit
{
    //the main menui pool game
    public class DummyLoad : MonoBehaviour
    {
        void Start()
        {
            load("8Ball");
        }
        void OnEnable()
        {
            PoolKit.BaseGameManager.onGameOver += onGameOver;
        }
        void OnDisable()
        {
            PoolKit.BaseGameManager.onGameOver -= onGameOver;
        }

        void onGameOver(string vic)
        {
            if (Application.loadedLevel == 0)
            {
                load("8Ball");
            }
        }

        void load(string str)
        {
            Destroy(GameObject.Find("8Ball"));
            Destroy(GameObject.Find("9Ball"));

            GameObject go = (GameObject)Instantiate(Resources.Load(str), Vector3.zero, Quaternion.identity);
            if (go)
            {
                go.name = str;
            }
        }

    }
}