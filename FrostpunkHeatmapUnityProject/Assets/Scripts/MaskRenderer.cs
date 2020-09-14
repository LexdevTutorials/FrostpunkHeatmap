using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;

/// <summary>
/// Component that controls the compute shader and assigns the necessary variables
/// </summary>
public class MaskRenderer : MonoBehaviour
{
    private static List<Building> buildings;

    /// <summary>
    /// Each building registers itself at startup using this function
    /// I really wouldn't do it like this in a large game project but it is 
    /// easy to expand and thus a good fit for a tutorial
    /// </summary>
    /// <param name="building">The building to add to the list</param>
    public static void RegisterBuilding(Building building)
    {
        buildings.Add(building);
    }

    //Properties

    /// <summary>
    /// The compute shader used for rendering the mask
    /// </summary>
    [SerializeField]
    private ComputeShader computeShader = null;

    /// <summary>
    /// The size the mask should have
    /// Idealy this is a power of two
    /// </summary>
    [Range(64, 4096)]
    [SerializeField]
    private int TextureSize = 2048;

    /// <summary>
    /// The size of the map in actual units
    /// </summary>
    [SerializeField]
    private float MapSize = 0;

    [SerializeField]
    private float BlendDistance = 4.0f;

    /// <summary>
    /// Color used for coldest temperature
    /// </summary>
    public Color MaskColor0;
    /// <summary>
    /// Color used for second coldest temperature
    /// </summary>
    public Color MaskColor1;
    /// <summary>
    /// Color used for second hottest temperature
    /// </summary>
    public Color MaskColor2;
    /// <summary>
    /// Color used for hottest temperature
    /// </summary>
    public Color MaskColor3;
    /// <summary>
    /// Perlin Noise Texture
    /// </summary>
    public Texture2D NoiseTexture;
    /// <summary>
    /// UV scale used when sampling the noise texture
    /// </summary>
    [Range(0.0f, 5.0f)]
    public float NoiseDetail = 4.0f;

    private RenderTexture maskTexture;

    //Store those properties so we can avoid string lookups in Update
    private static readonly int textureSizeId = Shader.PropertyToID("_TextureSize");
    private static readonly int buildingCountId = Shader.PropertyToID("_BuildingCount");
    private static readonly int mapSizeId = Shader.PropertyToID("_MapSize");
    private static readonly int blendId = Shader.PropertyToID("_Blend");

    private static readonly int color0Id = Shader.PropertyToID("_Color0");
    private static readonly int color1Id = Shader.PropertyToID("_Color1");
    private static readonly int color2Id = Shader.PropertyToID("_Color2");
    private static readonly int color3Id = Shader.PropertyToID("_Color3");

    private static readonly int noiseTexId = Shader.PropertyToID("_NoiseTex");
    private static readonly int noiseDetailId = Shader.PropertyToID("_NoiseDetail");

    private static readonly int maskTextureId = Shader.PropertyToID("_Mask");

    private static readonly int buildingBufferId = Shader.PropertyToID("_BuildingBuffer");

    //This is the struct we parse to the compute shader for each building
    private struct BuildingBufferElement
    {
        public float PositionX;
        public float PositionY;
        public float Range;
        public float Heat;
    }

    private List<BuildingBufferElement> bufferElements;
    private ComputeBuffer buffer = null;

    /// <summary>
    /// Initialization
    /// </summary>
    private void Awake()
    {
        //It is important that this is in Awake and the Building's are getting added in Start()
        buildings = new List<Building>();

        //Create a new render texture for the mask
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        maskTexture = new RenderTexture(TextureSize, TextureSize, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear) 
#else
        maskTexture = new RenderTexture(TextureSize, TextureSize, 0, RenderTextureFormat.ARGB32)
#endif
        { 
            enableRandomWrite = true 
        };
        maskTexture.Create();

        //Set the texture dimension and the mask texture in the compute shader
        computeShader.SetInt(textureSizeId, TextureSize);
        computeShader.SetTexture(0, maskTextureId, maskTexture);

        //Set the blend distance
        computeShader.SetFloat(blendId, BlendDistance);

        //Set the mask colors
        computeShader.SetVector(color0Id, MaskColor0);
        computeShader.SetVector(color1Id, MaskColor1);
        computeShader.SetVector(color2Id, MaskColor2);
        computeShader.SetVector(color3Id, MaskColor3);

        //Set the noise texture
        computeShader.SetTexture(0, noiseTexId, NoiseTexture);
        computeShader.SetFloat(noiseDetailId, NoiseDetail);

        //We are using the mask texture and the map size in multiple materials
        //Setting it as a global variable is easier in this case
        //For a full scale game this should be set in the specific materials rather than globally
        Shader.SetGlobalTexture(maskTextureId, maskTexture);
        Shader.SetGlobalFloat(mapSizeId, MapSize);

        bufferElements = new List<BuildingBufferElement>();
    }

    /// <summary>
    /// Cleanup
    /// </summary>
    private void OnDestroy()
    {
        buffer?.Dispose();

        if (maskTexture != null)
            DestroyImmediate(maskTexture);
    }

    //Setup all buffers and variables
    private void Update()
    {
        //Recreate the buffer
        bufferElements.Clear();
        foreach (Building building in buildings)
        {
            BuildingBufferElement element = new BuildingBufferElement
            {
                PositionX = building.transform.position.x,
                PositionY = building.transform.position.z,
                Range = building.Range,
                Heat = building.Heat
            };

            bufferElements.Add(element);
        }

        buffer?.Release();
        buffer = new ComputeBuffer(bufferElements.Count * 4, sizeof(float));

        //Set the buffer data and parse it to the compute shader
        buffer.SetData(bufferElements);
        computeShader.SetBuffer(0, buildingBufferId, buffer);

        //Set other variables needed in the compute function
        computeShader.SetInt(buildingCountId, bufferElements.Count);

        //Execute the compute shader
        //Our thread group size is 8x8=64, 
        //thus we have to dispatch (TextureSize / 8) * (TextureSize / 8) thread groups
        computeShader.Dispatch(0, Mathf.CeilToInt(TextureSize / 8.0f), Mathf.CeilToInt(TextureSize / 8.0f), 1);
    }
}
