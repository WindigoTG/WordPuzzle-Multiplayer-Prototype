using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WordPuzzle
{
    public class PhotoSelectionHandler
    {
        #region Fields

        private static int _pictureMaxSize = 256;

        #endregion

        #region Methods

        public static void GetPictureFromGallery(Action<Texture2D> callback, Action fallback)
        {
            Debug.Log("Profile service  |  Picture selection start");
            if (NativeGallery.IsMediaPickerBusy())
            {
                fallback.Invoke();
                return;
            }
            Debug.Log("Profile service  |  Media Picker is OK");

            Debug.Log("Profile service  |  Before permission check");
            NativeGallery.Permission permission = NativeGallery.GetImageFromGallery((path) =>
            {
                Debug.Log("Profile service  |  Picture selectuon...");
                if (path != null)
                {
                    Debug.Log("Profile service  |  Path selected");
                    Texture2D picture = NativeGallery.LoadImageAtPath(path, _pictureMaxSize, false);

                    Debug.Log("Profile service  |  Before picture cropping");
                    var croppedPicture = CropImage(picture, 250, 250);

                    Debug.Log("Profile service  |  Picture loaded, Before callback call");
                    callback.Invoke(croppedPicture);
                }
                else
                {
                    Debug.Log("Profile service  |  Before Fallback call");
                    fallback.Invoke();
                }
            });
		}

        public static void TakePhotoWithCamera(Action<Texture2D> callback, Action fallback)
        {
            if (NativeCamera.IsCameraBusy())
            {
                fallback.Invoke();
                return;
            }

            NativeCamera.Permission permission = NativeCamera.TakePicture((path) =>
			{
				if (path != null)
				{
					Texture2D picture = NativeCamera.LoadImageAtPath(path, _pictureMaxSize, false);

                    var croppedPicture = CropImage(picture, 250, 250);

                    callback.Invoke(croppedPicture);
                }
                else
                    fallback.Invoke();
            }, _pictureMaxSize, true, NativeCamera.PreferredCamera.Front);
        }

        private static Texture2D CropImage(Texture2D source, int targetWidth, int targetHeight)
        {
            int sourceWidth = source.width;
            int sourceHeight = source.height;
            float sourceAspect = (float)sourceWidth / sourceHeight;
            float targetAspect = (float)targetWidth / targetHeight;
            int xOffset = 0;
            int yOffset = 0;
            float factor = 1;
            if (sourceAspect > targetAspect)
            { // crop width
                factor = (float)targetHeight / sourceHeight;
                xOffset = (int)((sourceWidth - sourceHeight * targetAspect) * 0.5f);
            }
            else
            { // crop height
                factor = (float)targetWidth / sourceWidth;
                yOffset = (int)((sourceHeight - sourceWidth / targetAspect) * 0.5f);
            }
            Color32[] data = source.GetPixels32();
            Color32[] data2 = new Color32[targetWidth * targetHeight];
            for (int y = 0; y < targetHeight; y++)
            {
                for (int x = 0; x < targetWidth; x++)
                {
                    var p = new Vector2(Mathf.Clamp(xOffset + x / factor, 0, sourceWidth - 1), Mathf.Clamp(yOffset + y / factor, 0, sourceHeight - 1));
                    // bilinear filtering
                    var c11 = data[Mathf.FloorToInt(p.x) + sourceWidth * (Mathf.FloorToInt(p.y))];
                    var c12 = data[Mathf.FloorToInt(p.x) + sourceWidth * (Mathf.CeilToInt(p.y))];
                    var c21 = data[Mathf.CeilToInt(p.x) + sourceWidth * (Mathf.FloorToInt(p.y))];
                    var c22 = data[Mathf.CeilToInt(p.x) + sourceWidth * (Mathf.CeilToInt(p.y))];
                    var f = new Vector2(Mathf.Repeat(p.x, 1f), Mathf.Repeat(p.y, 1f));
                    data2[x + y * targetWidth] = Color.Lerp(Color.Lerp(c11, c12, p.y), Color.Lerp(c21, c22, p.y), p.x);
                }
            }

            var tex = new Texture2D(targetWidth, targetHeight);
            tex.SetPixels32(data2);
            tex.Apply(true);
            return tex;
        }

        #endregion
    }
}