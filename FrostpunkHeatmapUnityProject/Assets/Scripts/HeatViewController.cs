using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

/// <summary>
/// Handles the visual transition of all gameplay objects involved in the effect
/// </summary>
public class HeatViewController : MonoBehaviour
{
    /// <summary>
    /// Light used for the default view
    /// </summary>
    public Light Moon;
    /// <summary>
    /// Light used for the heatmap view
    /// </summary>
    public Light HeatviewLight;

    /// <summary>
    /// Post processing used for the default view
    /// </summary>
    public PostProcessLayer PostProcessLayer;

    /// <summary>
    /// All lampposts in the scene
    /// </summary>
    public Light[] Lampposts;

    /// <summary>
    /// All materials used for objects in the scene
    /// </summary>
    public Material[] Materials;

    /// <summary>
    /// All UI elements for the heaters in the scene
    /// </summary>
    public GameObject[] UI;

    /// <summary>
    /// Snow particle effect used in the default view
    /// </summary>
    public GameObject SnowParticles;

    private bool isEnabled;

    /// <summary>
    /// Disable heatmap view on start
    /// </summary>
    private void Start()
    {
        EnableHeatView(false);
    }

    /// <summary>
    /// Disable heatmap view when exiting the scene to reset it for the edit mode
    /// </summary>
    private void OnApplicationQuit()
    {
        EnableHeatView(false);
    }

    /// <summary>
    /// Button event
    /// </summary>
    public void OnClick()
    {
        EnableHeatView(!isEnabled);
    }

    /// <summary>
    /// Swap the state of every object to the heatmap or regular view's state
    /// </summary>
    /// <param name="enable">True if heatmap is on</param>
    private void EnableHeatView(bool enable)
    {
        isEnabled = enable;

        Moon.enabled = !enable;
        HeatviewLight.enabled = enable;

        PostProcessLayer.enabled = !enable;

        foreach(Light l in Lampposts)
        {
            l.enabled = !enable;
        }

        foreach(Material m in Materials)
        {
            if (enable)
                m.EnableKeyword("RENDER_HEAT");
            else
                m.DisableKeyword("RENDER_HEAT");
        }

        SnowParticles.SetActive(!enable);

        foreach (GameObject go in UI)
        {
            go.SetActive(enable);
        }
    }
}
