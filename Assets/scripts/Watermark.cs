using System;
using System.IO;
using UnityEngine;

public class Watermark : MonoBehaviour
{
    private Texture2D logo;
    private Texture2D finalPicture;
    private const int nbLogos = 4;

    Vector2[] logosTopLeftCorners = new Vector2[nbLogos];

    public void CreateWatermark(byte[] picture)
    {
        // Creates a new texture for the final picture
        if(finalPicture == null)
            finalPicture = new Texture2D(2, 2);
        finalPicture.LoadImage(picture);
        logo = Resources.Load("logo_blanc") as Texture2D;
    }

    public Texture2D GetTexture()
    {
        return finalPicture;
    }

    /**
	 * Adds Instant Sphere logo as a watermark on the final picture
	 */
    public void AddWatermark()
    {
        SetLogosPositions();
        InsertLogos();
    }

    /**
	 * Initializes the four logos positions on the final picture
	 */
    private void SetLogosPositions()
    {
        // x-coordinate
        logosTopLeftCorners[0].x = (finalPicture.width / 8) - (logo.width / 2);
        logosTopLeftCorners[1].x = ((finalPicture.width / 2) - (finalPicture.width / 8)) - (logo.width / 2);
        logosTopLeftCorners[2].x = ((finalPicture.width / 2) + (finalPicture.width / 8)) - (logo.width / 2);
        logosTopLeftCorners[3].x = (finalPicture.width - finalPicture.width / 8) - (logo.width / 2);

        // y-coordinate
        for (int i = 0; i < nbLogos; i++)
        {
            logosTopLeftCorners[i].y = logo.height;
        }
    }

    /**
	 * Inserts the four logos on the final picture by replacing the corresponding pixels
	 */
    private void InsertLogos()
    {
        // For each logo, replaces final picture pixels by logo pixels (at the correct position)
        for (int k = 0; k < logosTopLeftCorners.Length; k++)
        {
            int logo_x = 0;
            for (int final_x = (int)logosTopLeftCorners[k].x; final_x < (int)logosTopLeftCorners[k].x + logo.width; final_x++)
            {
                int logo_y = 0;
                for (int final_y = (int)logosTopLeftCorners[k].y; final_y < (int)logosTopLeftCorners[k].y + logo.height; final_y++)
                {
                    // Non transparent pixels only
                    if (logo.GetPixel(logo_x, logo_y).a > 0.01f)
                    {
                        finalPicture.SetPixel(final_x, final_y, logo.GetPixel(logo_x, logo_y));
                    }
                    logo_y++;
                }
                logo_x++;
            }
        }
        finalPicture.Apply();
    }
}
