using UnityEngine;

public sealed class skybox_manager : MonoBehaviour
{
    Material mDefaultSkybox;

    // Use this for initialization
    private void Start()
    {
        mDefaultSkybox = RenderSettings.skybox;
    }

    // Update is called once per frame
    private void Update()
    {

    }

    public void DefineNewSkybox(byte[] data)
    {
        Texture2D t = new Texture2D(2, 2);
        t.LoadImage(data);

        // TODO : convert equirectangular to cubemap and don't create a new skybox from the shader "Skybox/Equirectangular"
        /*Cubemap cmap = new Cubemap(t.height, TextureFormat.RGB24, false);
        cmap.SetPixels(t.GetPixels(), CubemapFace.PositiveX);
        cmap.filterMode = FilterMode.Trilinear;
        cmap.Apply();*/

        Material m = new Material(Shader.Find("Skybox/Equirectangular"));
        m.SetTexture("_Tex", t);

        Destroy(RenderSettings.skybox.GetTexture("_Tex"));
        Destroy(RenderSettings.skybox);
        RenderSettings.skybox = m;
    }

    public void ResetSkybox()
    {
        Destroy(RenderSettings.skybox);
        RenderSettings.skybox = mDefaultSkybox;
    }
}
