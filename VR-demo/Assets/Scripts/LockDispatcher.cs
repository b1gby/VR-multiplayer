using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockDispatcher : MonoBehaviour
{
    public bool hasLock = false;
    public void GetLock()
    {
        hasLock = true;
    }

    public void ReleaseLock()
    {
        hasLock = false;
    }
}
