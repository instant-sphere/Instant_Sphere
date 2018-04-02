using UnityEngine;

/**
 * This class manages the skybox (where the 360° photo is projected)
 **/
public sealed class SkyboxManager : MonoBehaviour
{
    Material mDefaultSkybox;

    /**
     * Save the default photo
     **/
    private void Start()
    {
        mDefaultSkybox = RenderSettings.skybox;
    }

    /**
     * Frees the resources of the current skybox except the default skybox
     **/
    private void DestroyCurrentSkybox()
    {
        if (RenderSettings.skybox != mDefaultSkybox)
        {
            Destroy(RenderSettings.skybox.GetTexture("_Tex"));
            Destroy(RenderSettings.skybox);
        }
    }

    /**
     * Creates a new skybox from the photo passed as parameter (byte array or texture)
     **/
	public void DefineNewSkybox(byte[] data) {
		Texture2D t = new Texture2D(2, 2);
		t.LoadImage(data);
		DefineNewSkybox(t);
	}

	public void DefineNewSkybox(Texture2D t)
    {
        // TODO : convert equirectangular to cubemap and don't create a new skybox from the shader "Skybox/Equirectangular" for better quality

        /*Cubemap cmap = new Cubemap(t.height, TextureFormat.RGB24, false);
        cmap.SetPixels(t.GetPixels(), CubemapFace.PositiveX);
        cmap.filterMode = FilterMode.Trilinear;
        cmap.Apply();*/

        Material m = new Material(Shader.Find("Skybox/Equirectangular"));
        m.SetTexture("_Tex", t);
        DestroyCurrentSkybox();
        RenderSettings.skybox = m;
    }

    /**
     * Reset to the default skybox
     **/
    public void ResetSkybox()
    {
        DestroyCurrentSkybox();
        RenderSettings.skybox = mDefaultSkybox;
    }
}
