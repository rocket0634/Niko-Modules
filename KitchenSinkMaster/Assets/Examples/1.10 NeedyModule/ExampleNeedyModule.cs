using UnityEngine;
using System.Collections;

public class ExampleNeedyModule : MonoBehaviour
{
    public KMSelectable SolveButton;

    void Awake()
    {
        GetComponent<KMNeedyModule>().OnNeedyActivation += OnNeedyActivation;
        GetComponent<KMNeedyModule>().OnNeedyDeactivation += OnNeedyDeactivation;
        SolveButton.OnInteract += Solve;
        GetComponent<KMNeedyModule>().OnTimerExpired += OnTimerExpired;
    }

    protected bool Solve()
    {
        float time = GetComponent<KMNeedyModule>().GetNeedyTimeRemaining();
        float TargetTime = 10;
            if (time > TargetTime - 1)
            if (time < TargetTime + 1)

                GetComponent<KMNeedyModule>().OnPass();
                return false;
    }

    protected void OnNeedyActivation()
    {
        

            GetComponent<KMNeedyModule>().SetNeedyTimeRemaining(50);
    }

    protected void OnNeedyDeactivation()
    {

    }

    protected void OnTimerExpired()
    {
        GetComponent<KMNeedyModule>().OnStrike();
    }

    protected bool AddTime()
    {


        return false;
    }
}