using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GameManager : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    void Start()
    {
        GameObject[] addOnlyOneTags = GameObject.FindGameObjectsWithTag("Onlyone");
        foreach(GameObject per_object in addOnlyOneTags)
        {
            per_object.AddComponent<LockDispatcher>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
