using UnityEngine;

/**
 * This class is used to add watermarking to a photo
 * It adds 4 watermarks at the bottom
 **/
public sealed class Watermark : MonoBehaviour
{
    Texture2D mLogo;
    Texture2D mFinalPicture;
    const int mNbLogos = 4;
    Vector2[] logosTopLeftCorners = new Vector2[mNbLogos];  //position vectors of the logos

    /**
     * Loads the logo and the picture
     **/
    public void CreateWatermark(byte[] picture)
    {
        // Creates a new texture for the final picture
        if(mFinalPicture == null)
            mFinalPicture = new Texture2D(2, 2);
        mFinalPicture.LoadImage(picture);
        mLogo = Resources.Load("logo_blanc") as Texture2D;
    }

    /**
     * Returns the photo with watermarks as a Texture2D
     **/
    public Texture2D GetTexture()
    {
        return mFinalPicture;
    }

    /**
     * Returns the photo with watermarks as a byte array
     **/
    public byte[] GetBytes()
    {
        return mFinalPicture.EncodeToJPG(95);
    }

    /**
	 * Adds Instant Sphere logo as a watermark on the final picture
	 **/
    public void AddWatermark()
    {
        SetLogosPositions();
        InsertLogos();
    }

    /**
	 * Initializes the four logos positions on the final picture
	 **/
    private void SetLogosPositions()
    {
        // x-coordinate
        logosTopLeftCorners[0].x = (mFinalPicture.width / 8) - (mLogo.width / 2);
        logosTopLeftCorners[1].x = ((mFinalPicture.width / 2) - (mFinalPicture.width / 8)) - (mLogo.width / 2);
        logosTopLeftCorners[2].x = ((mFinalPicture.width / 2) + (mFinalPicture.width / 8)) - (mLogo.width / 2);
        logosTopLeftCorners[3].x = (mFinalPicture.width - mFinalPicture.width / 8) - (mLogo.width / 2);

        // y-coordinate
        for (int i = 0; i < mNbLogos; i++)
        {
            logosTopLeftCorners[i].y = mLogo.height;
        }
    }

    /**
	 * Inserts the four logos on the final picture by replacing the corresponding pixels
	 **/
    private void InsertLogos()
    {
        // For each logo, replaces final picture pixels by logo pixels (at the correct position)
        for (int k = 0; k < logosTopLeftCorners.Length; k++)
        {
            int logo_x = 0;
            for (int final_x = (int)logosTopLeftCorners[k].x; final_x < (int)logosTopLeftCorners[k].x + mLogo.width; final_x++)
            {
                int logo_y = 0;
                for (int final_y = (int)logosTopLeftCorners[k].y; final_y < (int)logosTopLeftCorners[k].y + mLogo.height; final_y++)
                {
                    // Non transparent pixels only
                    if (mLogo.GetPixel(logo_x, logo_y).a > 0.01f)
                    {
                        mFinalPicture.SetPixel(final_x, final_y, mLogo.GetPixel(logo_x, logo_y));
                    }
                    logo_y++;
                }
                logo_x++;
            }
        }
        mFinalPicture.Apply();
    }
}
