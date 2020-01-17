using System;
using UnityEngine;
using VisualPinball.Engine.VPT;
using VisualPinball.Unity.Importer;
namespace VisualPinball.Unity.Extensions
{
	public static class Texture
	{

		private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

		public static Texture2D ToUnityTexture(this Engine.VPT.Texture vpTex)
		{
			Texture2D unityTex;
			if (vpTex.Data.Bitmap != null) {
				var bmp = vpTex.Data.Bitmap;
				var data = bmp.Bytes;
				var pitch = bmp.Pitch();
				var colArr = new Color32[bmp.Width * bmp.Height];
				for (var y = 0; y < bmp.Height; y++) {
					for (var x = 0; x < bmp.Width; x++) {
						if (bmp.Format == Bitmap.RGBA) {
							colArr[y * vpTex.Width + x] = new Color32(
								data[(bmp.Height - y - 1) * pitch + 4 * x],
								data[(bmp.Height - y - 1) * pitch + 4 * x + 1],
								data[(bmp.Height - y - 1) * pitch + 4 * x + 2],
								data[(bmp.Height - y - 1) * pitch + 4 * x + 3]
							);
						} else {
							throw new NotImplementedException();
						}
					}
				}
				unityTex = new Texture2D(bmp.Width, bmp.Height, TextureFormat.RGBA32,true);
				unityTex.name = vpTex.Name;
				unityTex.SetPixels32(colArr);
				unityTex.Apply();

			} else {
				unityTex = new Texture2D(vpTex.Width, vpTex.Height, TextureFormat.RGBA32, true);
				unityTex.LoadImage(vpTex.FileContent);
			}
			return unityTex;
		}


		public static Texture2D ToUnityHDRTexture(this Engine.VPT.Texture vpTex) {	
			Texture2D unityTex;			
			unityTex = new Texture2D(vpTex.Width, vpTex.Height, TextureFormat.Alpha8,false);
			unityTex.LoadRawTextureData(vpTex.FileContent);									
			return unityTex;
		}



		public static void ValidateForTransparentPixels(this Engine.VPT.Texture vpTex, Texture2D uTex) {
			Color32[] pixels = uTex.GetPixels32();
			
			int approximationIndex = 0;
			int approximationStepDistance = Mathf.FloorToInt(pixels.Length/ 10);//this is how maany pixels the aproximationIndex is incremented by
			float approximationStepDistancescalar = 0.8f;
			bool mustCalculateApproximationIndexStartValue = true;
			bool foundAlpha = false;
			bool approximationBeatBruteForce = false;
			//the loop is brute force to  default maximum steps required to check every pixel
			//within the loop a second guessing approximation index is used. It tries to look ahead in larger steps to find a hit
			//while brute force continues if this index becomes greater than the array length , the approximationStepDistance is scaled.
			// so the guessing starts wildly for a chance of a early out hit , but if not successful  , it keeps guessing , but refines the distance.
			//when the guessing restarts it does not recheck the pixel already check via bruteforce , so it always starts at the bruteForceIndex+2, 
			//+ 2 and not + 1  is to avoid a overlap on the loop after setting aproximationIndex;
			//using 254 instead of 255 , just to count out miniscule hits or even precision errors
			int i = 0;
			for (i = 0; i < pixels.Length; i++) {
				//bruteforce
				if (pixels[i].a < 254) {
					foundAlpha = true;
					break;
				}
				//approximation
				if (mustCalculateApproximationIndexStartValue) {
					approximationIndex = Mathf.Min(pixels.Length-1, i + 2);
					mustCalculateApproximationIndexStartValue = false;
				}

				if (pixels[approximationIndex].a < 254) {
					foundAlpha = true;
					approximationBeatBruteForce = true;
					break;
				}
				approximationIndex += approximationStepDistance;
				//need to see if the index of approximation is larger than array
				//if so then refine the guessing distance and start again at new aproximationIndex;
				if (approximationIndex >= pixels.Length) {
					mustCalculateApproximationIndexStartValue = true;
					approximationStepDistance = Mathf.FloorToInt((float)approximationStepDistance * approximationStepDistancescalar);
				}

			}
			Logger.Info(vpTex.Name + " HasTranparentPixels " + foundAlpha);
			if (foundAlpha) {				
				if (approximationBeatBruteForce) {
					Logger.Info("approximation beat brute force - index = "+i+ " approximationIndex = "+ approximationIndex + " maximum iterations = "+ pixels.Length+" percent = " + (((float)i / (float)pixels.Length) * 100f));
				} else {
					Logger.Info("brute force beat approximation  - index = " + i + " maximum iterations = " + pixels.Length + " percent = " + (((float)i / (float)pixels.Length) * 100f));
				}
			}
			vpTex.HasTranparentPixels = foundAlpha;

		}

		public static string GetUnityFilename(this Engine.VPT.Texture vpTex, string extensionFormat, string folderName = null)
		{
			return folderName != null
				? $"{folderName}/{AssetUtility.StringToFilename(vpTex.Name)}" + extensionFormat
				: $"{AssetUtility.StringToFilename(vpTex.Name)}" + extensionFormat;
		}
	}
}
