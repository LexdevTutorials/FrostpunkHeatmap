using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI button element for the heaters
/// </summary>
public class ThermoButton : MonoBehaviour
{
    /// <summary>
    /// Slider rect used for rotating it on click
    /// </summary>
    public RectTransform SliderRect;
    /// <summary>
    /// Background Image used for colour shifts on click
    /// </summary>
    public Image Background;
    /// <summary>
    /// icon Image used for colour shifts on click
    /// </summary>
    public Image Thermometer;

    private bool isEnabled;

    /// <summary>
    /// Button event
    /// </summary>
    public void OnClick()
    {
        isEnabled = !isEnabled;
        StopAllCoroutines();
        StartCoroutine(UIAnimationCoroutine());
    }

    /// <summary>
    /// Animate the transitions between the different button states
    /// </summary>
    /// <returns>Yield</returns>
    private IEnumerator UIAnimationCoroutine()
    {
        float startingTime = Time.time;
        float duration = 0.25f;
        float timeVal = 0.0f;
        Quaternion startingRot = SliderRect.rotation;
        Color startingBackgroundCol = Background.color;
        Color startingThermoCol = Thermometer.color;
        while (timeVal < 1.0f)
        {
            if (isEnabled)
            {
                SliderRect.rotation = Quaternion.Lerp(startingRot, Quaternion.Euler(0, 0, -28.5f), timeVal);
                Background.color = Color.Lerp(startingBackgroundCol, Color.black, timeVal);
                Thermometer.color = Color.Lerp(startingThermoCol, new Color(240.0f/255.0f,189.0f / 255.0f, 0.0f / 255.0f, 1.0f), timeVal);
            }
            else
            {
                SliderRect.rotation = Quaternion.Lerp(startingRot, Quaternion.Euler(0, 0, 28.5f), timeVal);
                Background.color = Color.Lerp(startingBackgroundCol, new Color(0.0f, 0.0f, 0.0f, 0.5f), timeVal);
                Thermometer.color = Color.Lerp(startingThermoCol, Color.white, timeVal);
            }
            timeVal = (Time.time - startingTime) / duration;
            yield return null;
        }
    }
}
