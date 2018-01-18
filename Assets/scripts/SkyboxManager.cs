using UnityEngine;

public sealed class SkyboxManager : MonoBehaviour
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

    private void DestroyCurrentSkybox()
    {
        if (RenderSettings.skybox != mDefaultSkybox)
        {
            Destroy(RenderSettings.skybox.GetTexture("_Tex"));
            Destroy(RenderSettings.skybox);
        }
    }

	public void DefineNewSkybox(byte[] data) {
		Texture2D t = new Texture2D(2, 2);
		t.LoadImage(data);
		DefineNewSkybox(t);
	}


	public void DefineNewSkybox(Texture2D t)
    {
        // TODO : convert equirectangular to cubemap and don't create a new skybox from the shader "Skybox/Equirectangular"
        /*Cubemap cmap = new Cubemap(t.height, TextureFormat.RGB24, false);
        cmap.SetPixels(t.GetPixels(), CubemapFace.PositiveX);
        cmap.filterMode = FilterMode.Trilinear;
        cmap.Apply();*/

        Material m = new Material(Shader.Find("Skybox/Equirectangular"));
        m.SetTexture("_Tex", t);

        DestroyCurrentSkybox();
        RenderSettings.skybox = m;
    }

    public void ResetSkybox()
    {
        DestroyCurrentSkybox();
        RenderSettings.skybox = mDefaultSkybox;
    }
}
