using UnityEngine;

/// <summary>
/// Used to toggle the heatmap UI
/// </summary>
public class ViewButton : MonoBehaviour
{
    /// <summary>
    /// Icon of the thermometer button
    /// </summary>
    public GameObject ThermoIcon;
    /// <summary>
    /// Icon of the close button
    /// </summary>
    public GameObject CloseIcon;

    private bool isEnabled;

    /// <summary>
    /// Called by the UI button on mouse click
    /// </summary>
    public void OnClick()
    {
        isEnabled = !isEnabled;
        ThermoIcon.SetActive(!isEnabled);
        CloseIcon.SetActive(isEnabled);
    }
}
