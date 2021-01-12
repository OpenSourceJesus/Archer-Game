#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;
using UnityEditor;
using System;
using Object = UnityEngine.Object;
using UnityEngine.InputSystem;

namespace ArcherGame
{
	// [ExecuteInEditMode]
	public class WorldArtMaker : MonoBehaviour, IUpdatable
	{
		public SpriteRenderer spriteRenderer;
		public CameraScript cameraScript;
		public string exportPath;
		public Vector2Int maxCameraPosition;
		public int maxTextureSize;
		public Texture2D combinedImage;
		public bool PauseWhileUnfocused
		{
			get
			{
				return true;
			}
		}
		bool mKeyPressed;
		bool previousMKeyPressed;
		bool cKeyPressed;
		bool previousCKeyPressed;

		public virtual void OnEnable ()
		{
			maxTextureSize = Mathf.Min(maxTextureSize, SystemInfo.maxTextureSize);
			// GameManager.updatables = GameManager.updatables.Add(this);
		}

		public virtual void DoUpdate ()
		{
			mKeyPressed = Keyboard.current.mKey.isPressed;
			cKeyPressed = Keyboard.current.cKey.isPressed;
			if (mKeyPressed && !previousMKeyPressed)
				GameManager.Instance.StartCoroutine(MakeImagesRoutine ());
			else if (cKeyPressed && !previousCKeyPressed)
				GameManager.Instance.StartCoroutine(CombineImagesRoutine ());
			previousMKeyPressed = mKeyPressed;
			previousCKeyPressed = cKeyPressed;
		}

// #if UNITY_EDITOR
// 		public virtual void Update ()
// 		{
// 			// InputSystem.Update();
// 			DoUpdate ();
// 		}
// #endif

		public virtual IEnumerator MakeImagesRoutine ()
		{
			WorldMap.Instance.Open ();
			WorldMap.Instance.unexploredTilemap.gameObject.SetActive(false);
			PauseMenu.Instance.canvas.enabled = false;
			Player.instance.lifeIconsParent.gameObject.SetActive(false);
			WorldMapCamera.Instance.gameObject.SetActive(false);
			cameraScript = GameCamera.Instance;
			cameraScript.camera.cullingMask = LayerMaskExtensions.AddToMask(cameraScript.camera.cullingMask, "Map");
			maxCameraPosition = Vector2Int.zero;
			for (float x = World.Instance.worldBoundsRect.xMin + cameraScript.viewSize.x / 2; x < World.Instance.worldBoundsRect.xMax - cameraScript.viewSize.x / 2; x += cameraScript.viewSize.x)
			{
				maxCameraPosition.y = 0;
				for (float y = World.Instance.worldBoundsRect.yMin + cameraScript.viewSize.y / 2; y < World.Instance.worldBoundsRect.yMax - cameraScript.viewSize.y / 2; y += cameraScript.viewSize.y)
				{
					cameraScript.trs.position = new Vector2(x, y).SetZ(cameraScript.trs.position.z);
					cameraScript.camera.Render();
					ScreenCapture.CaptureScreenshot(exportPath + maxCameraPosition + ".png", 1);
					yield return new WaitForEndOfFrame();
					maxCameraPosition.y ++;
				}
				maxCameraPosition.x ++;
			}
		}

		public virtual IEnumerator CombineImagesRoutine ()
		{
			print("Start");
			Object imageObj = AssetDatabase.LoadMainAssetAtPath(exportPath + new Vector2Int(0, 0) + ".png");
			Texture2D image = imageObj as Texture2D;
			// Texture2D combinedImage = new Texture2D(Mathf.Min(maxCameraPosition.x * image.width, maxTextureSize), Mathf.Min(maxCameraPosition.y * image.height, maxTextureSize));
			Color[] combinedImageColors = new Color[combinedImage.width * combinedImage.height];
			int horizontalBorderWidth = 53;
			for (int x = 0; x < maxCameraPosition.x; x ++)
			{
				for (int y = 0; y < maxCameraPosition.y; y ++)
				{
					imageObj = AssetDatabase.LoadMainAssetAtPath(exportPath + new Vector2Int(x, y) + ".png");
					image = imageObj as Texture2D;
					Color[] imageColors = image.GetPixels();
					Vector2Int offset = new Vector2Int(x * image.width + x * horizontalBorderWidth * 2, y * image.height);
					for (int x2 = horizontalBorderWidth; x2 < image.width - horizontalBorderWidth; x2 ++)
					{
						for (int y2 = 0; y2 < image.height; y2 ++)
						{
							if ((x2 + offset.x) + (y2 + offset.y) * combinedImage.width < combinedImageColors.Length)
								combinedImageColors[(x2 + offset.x) + (y2 + offset.y) * combinedImage.width] = imageColors[x2 + y2 * image.width];
							else
								print(x + ", " + y + ", " + x2 + ", " + y2 + ", " + (x2 + offset.x) + (y2 + offset.y) * combinedImage.width);
						}
					}
					yield return new WaitForEndOfFrame();
				}
			}
			combinedImage = Instantiate(combinedImage);
			combinedImage.SetPixels(combinedImageColors);
			combinedImage.Apply();
			AssetDatabase.CreateAsset(combinedImage, exportPath + ".asset");
			print("Done");
			yield break;
		}

		// public static void SetTextureImporterFormat (Texture2D texture, bool isReadable)
		// {
		// 	string assetPath = AssetDatabase.GetAssetPath(texture);
		// 	TextureImporter tImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
		// 	if (tImporter != null)
		// 	{
		// 		tImporter.textureType = TextureImporterType.Default;
		// 		tImporter.isReadable = isReadable;
		// 		AssetDatabase.ImportAsset(assetPath);
		// 		AssetDatabase.Refresh();
		// 	}
		// }

		public virtual void OnDisable ()
		{
			GameManager.updatables = GameManager.updatables.Remove(this);
		}
	}
}
#endif
