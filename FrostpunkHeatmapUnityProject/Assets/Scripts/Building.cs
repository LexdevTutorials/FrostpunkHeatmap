using System.Collections;
using UnityEngine;

/// <summary>
/// Building class used to store heat level and range of the building and adding it to the mask renderer
/// </summary>
public class Building : MonoBehaviour
{
    /// <summary>
    /// Different heat levels used 
    /// </summary>
    public enum HeatLevel
    {
        Low,
        Medium,
        High
    }
    /// <summary>
    /// Heat level for this building when heater is on
    /// </summary>
    public HeatLevel HeatLvl;

    /// <summary>
    /// Ranges from 0 to 1 with 0 indicating that there's no heat emited from that building
    /// </summary>
    public float Heat { get; private set; }
    /// <summary>
    /// Range of the emitted heat
    /// </summary>
    [Range(0, 40.0f)]
    public float Range;

    private bool heaterOn;

    /// <summary>
    /// Register this building in the mask rendering script
    /// </summary>
    private void Start()
    {
        MaskRenderer.RegisterBuilding(this);
    }

    /// <summary>
    /// Button event
    /// </summary>
    public void OnClick()
    {
        StopAllCoroutines();
        if(heaterOn)
        {
            StartCoroutine(HeatChangeCoroutine(0.0f));
        }
        else
        {
            StartCoroutine(HeatChangeCoroutine(0.25f * ((int)HeatLvl + 1)));
        }
        heaterOn = !heaterOn;
    }

    /// <summary>
    /// Animate the change in heat for a proper transition effect
    /// </summary>
    /// <param name="newHeat">Target heat value</param>
    /// <returns>Yield</returns>
    private IEnumerator HeatChangeCoroutine(float newHeat)
    {
        float startingHeat = Heat;
        float startingTime = Time.time;
        float duration = 0.25f;
        float timeVal = 0.0f;
        while(timeVal < 1.0f)
        {
            timeVal = (Time.time - startingTime) / duration;
            Heat = Mathf.Lerp(startingHeat, newHeat, timeVal);
            yield return null;
        }
        Heat = newHeat;
    }

    /// <summary>
    /// Debug Draw - Displays the radius without the transition area
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, Range);
    }
}
