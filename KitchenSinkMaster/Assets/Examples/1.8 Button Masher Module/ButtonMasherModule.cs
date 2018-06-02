using UnityEngine;

/// <summary>
/// A simple module that requires the player to push the exactly button 50 times, but only
/// when the timer has a "4" in any position.
/// </summary>
public class ButtonMasherModule : MonoBehaviour
{
    public KMBombInfo BombInfo;
    public KMBombModule BombModule;
    public KMAudio KMAudio;
    public KMSelectable Button;
    public TextMesh Counter;
    public MeshRenderer LED;


    protected int currentCount;

    public float myTimer = 0.0f;
    int myTimerRound = 0;

    
    protected void Start()
    {
        LED.material.color = Color.black;
        Button.OnInteract += HandlePress;
        
	}

    protected bool HandlePress()
    {
        KMAudio.PlaySoundAtTransform("tick", this.transform);

        string timerText = BombInfo.GetFormattedTime();

        if (currentCount < 50 && timerText.Contains("4"))
        {
            currentCount++;

            if (currentCount == 50)
            {
                BombModule.HandlePass();
            }
        }
        else
        {
            BombModule.HandleStrike();
        }

        Counter.text = currentCount.ToString();

        return false;
    }
    void Update ()
    {
        myTimer += Time.deltaTime;

        if (myTimer > myTimerRound)
        {
            LED.material.color = Color.red;
        }

        if (myTimer > (myTimerRound + 0.2))
        {
            LED.material.color = Color.black;
            myTimerRound++;
        }
    }

}
