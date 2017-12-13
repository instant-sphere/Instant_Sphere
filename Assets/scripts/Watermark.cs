using System;
using UnityEngine;

public class Watermark : MonoBehaviour
{
	string logo_filename;
	Texture2D logo;
	Texture2D finalPicture;
	int nbLogos = 4;
	int logos_y;
	Vector2[] logosTopLeftCorners;

	public Watermark(byte[] picture)
	{
		// Creates a new texture for the final picture
		finalPicture = new Texture2D(0, 0);
		finalPicture.LoadImage(picture);

		logosTopLeftCorners = new Vector2[nbLogos];
	}

	public Texture2D GetTexture() {
		return finalPicture;
	}

	/**
	 * Adds Instant Sphere logo as a watermark on the final picture
	 */
	public void AddWatermark(string filename) {
		logo_filename = filename;
		LoadLogo ();
		SetLogosPositions ();
		InsertLogos ();
	}

	/**
	 * Loads Instant Sphere logo
	 */
	private void LoadLogo() {
		logo = Resources.Load(logo_filename) as Texture2D;
		logos_y = finalPicture.height - 2 * logo.height;
	}

	/**
	 * Initializes the four logos positions on the final picture
	 */
	private void SetLogosPositions() {
		// x-coordinate
		logosTopLeftCorners[0].x = (finalPicture.width / 8) - (logo.width / 2);
		logosTopLeftCorners[1].x = ((finalPicture.width / 2) - (finalPicture.width / 8)) - (logo.width / 2);
		logosTopLeftCorners[2].x = ((finalPicture.width / 2) + (finalPicture.width / 8)) - (logo.width / 2);
		logosTopLeftCorners[3].x = (finalPicture.width - finalPicture.width / 8) - (logo.width / 2);

		// y-coordinate
		for (int i = 0; i < nbLogos; i++) {
			logosTopLeftCorners[i].y = logos_y;
		}
	}

	/**
	 * Inserts the four logos on the final picture by replacing the corresponding pixels
	 */
	private void InsertLogos() {
		float x;
		float y;

		// For each logo
		for (int k = 0; k < nbLogos; k++) {
			// Gets (x,y) logo position on the final picture
			x = logosTopLeftCorners[k].x;
			y = logosTopLeftCorners[k].y;

			// Replaces final picture pixels by logo pixels, on the correct position
			for (int i = 0; i < logo.width; i++) {
				for (int j = 0; j < logo.height; j++) {
					finalPicture.SetPixel((int)x, (int)y, logo.GetPixel(i, j));
					y++;
				}
				x++;
			}
		}
	}

}
