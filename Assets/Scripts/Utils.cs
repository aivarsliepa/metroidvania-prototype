using System;
using System.Collections;
using UnityEngine;

public static class Utils
{
    public static IEnumerator WaitForAction(float waitTime, Action action)
    {
        yield return new WaitForSeconds(waitTime);
        action.Invoke();
    }
}
