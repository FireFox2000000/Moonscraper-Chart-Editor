// https://github.com/DaVikingCode/UnityRuntimeSpriteSheetsGenerator with modifications by FireFox

using DaVikingCode.RectanglePacking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

namespace DaVikingCode.AssetPacker {

	public class AssetPacker : MonoBehaviour {

		public UnityEvent OnProcessCompleted;
		public float pixelsPerUnit = 100.0f;

		public bool useCache = false;
		public string cacheName = "";
		public int cacheVersion = 1;
		public bool deletePreviousCacheVersion = true;
        public Vector2 spritePivot = Vector2.zero;
        public bool mipChain = false;

		protected Dictionary<string, Sprite> mSprites = new Dictionary<string, Sprite>();
		protected List<TextureToPack> itemsToRaster = new List<TextureToPack>();

        protected bool allow4096Textures = false;

		public void AddTextureToPack(string file, string customID = null) {

			itemsToRaster.Add(new TextureToPack(file, customID != null ? customID : Path.GetFileNameWithoutExtension(file)));
		}

        public void AddTextureToPack(string file, TextureToPack.GridSlice sliceParams, string customID = null) {

            itemsToRaster.Add(new TextureToPack(file, customID != null ? customID : Path.GetFileNameWithoutExtension(file), sliceParams));
        }

        // For pre-loaded texture or custom loaded textures. Useful for loading unconventional texture formats.
        public void AddTextureToPack(Texture2D texture, string customID = null) {
            itemsToRaster.Add(new TextureToPack(texture, !string.IsNullOrEmpty(customID) ? customID : texture.name));
        }

        // For pre-loaded texture or custom loaded textures. Useful for loading unconventional texture formats.
        public void AddTextureToPack(Texture2D texture, TextureToPack.GridSlice sliceParams, string customID = null)
        {
            itemsToRaster.Add(new TextureToPack(texture, !string.IsNullOrEmpty(customID) ? customID : texture.name, sliceParams));
        }

        public void AddTexturesToPack(string[] files) {

			foreach (string file in files)
				AddTextureToPack(file);
		}

		public void Process(bool allow4096Textures = false) {

			this.allow4096Textures = allow4096Textures;

			if (useCache) {

				if (cacheName == "")
					throw new Exception("No cache name specified");

				string path = Application.persistentDataPath + "/AssetPacker/" + cacheName + "/" + cacheVersion + "/";

				bool cacheExist = Directory.Exists(path);

				if (!cacheExist)
					StartCoroutine(createPack(path));
				else
					StartCoroutine(loadPack(path));
				
			} else
				StartCoroutine(createPack());
			
		}

		protected IEnumerator createPack(string savePath = "") {

			if (savePath != "") {

				if (deletePreviousCacheVersion && Directory.Exists(Application.persistentDataPath + "/AssetPacker/" + cacheName + "/"))
					foreach (string dirPath in Directory.GetDirectories(Application.persistentDataPath + "/AssetPacker/" + cacheName + "/", "*", SearchOption.AllDirectories))
						Directory.Delete(dirPath, true);

				Directory.CreateDirectory(savePath);
			}

			List<Texture2D> textures = new List<Texture2D>();
			List<string> images = new List<string>();

			foreach (TextureToPack itemToRaster in itemsToRaster) {
                Texture2D baseTexture = itemToRaster.preLoadedTexture;

                if (!baseTexture)
                {
                    WWW loader = new WWW("file:///" + itemToRaster.file);

                    yield return loader;

                    baseTexture = loader.texture;
                }

                if (itemToRaster.sliceParams != null)
                {
                    TextureToPack.GridSlice sliceParams = itemToRaster.sliceParams;

                    bool isValidStandaloneSprite = sliceParams.smallerSizeValid && baseTexture.width <= sliceParams.width && baseTexture.height <= sliceParams.height;

                    if (!isValidStandaloneSprite && (sliceParams.width > baseTexture.width || sliceParams.height > baseTexture.height))
                    {
                        Debug.LogError(string.Format("Skipping loading of texture {0}. Width and/or height of texture is less than the provided slice parameters.", itemToRaster.id));
                        continue;
                    }

                    if (!isValidStandaloneSprite && !(baseTexture.width % sliceParams.width == 0 && baseTexture.height % sliceParams.height == 0))
                    {
                        Debug.LogError(string.Format("Skipping loading of texture {0}. Width and/or height of texture is not a multiple of the provided slice parameters.", itemToRaster.id));
                        continue;
                    }

                    int totalRows = isValidStandaloneSprite ? 1 : baseTexture.height / sliceParams.height;
                    int totalColumns = isValidStandaloneSprite ? 1 : baseTexture.width / sliceParams.width;

                    int textureIndex = 0;

                    int singleSpriteWidth = baseTexture.width / totalColumns;
                    int singleSpriteHeight = baseTexture.height / totalRows;

                    // Scan each slice left to right, top to bottom
                    for (int rowIndex = 0; rowIndex < totalRows; ++rowIndex)
                    {
                        for (int columnIndex = 0; columnIndex < totalColumns; ++columnIndex)
                        {
                            int x = columnIndex * singleSpriteWidth;
                            int y = (totalRows - rowIndex - 1) * singleSpriteHeight;    // Starting from the top, not the bottom. Specific to GH3 sprite sheets, not nesacarily universal.

                            Color[] pixels = baseTexture.GetPixels(x, y, singleSpriteWidth, singleSpriteHeight);
                            Texture2D texture = new Texture2D(singleSpriteWidth, singleSpriteHeight);
                            texture.SetPixels(pixels);
                            texture.Apply();

                            textures.Add(texture);
                            images.Add(string.Format("{0}_{1}", itemToRaster.id, textureIndex.ToString("D8")));

                            ++textureIndex;
                        }
                    }
                }
                else
                {
                    textures.Add(baseTexture);
                    images.Add(itemToRaster.id);
                }
			}

			int textureSize = allow4096Textures ? 4096 : 2048;

			List<Rect> rectangles = new List<Rect>();
			for (int i = 0; i < textures.Count; i++)
				if (textures[i].width > textureSize || textures[i].height > textureSize)
					throw new Exception("A texture size is bigger than the sprite sheet size!");    // TODO, remove this, can't catch exceptions in coroutines without tedious workarounds!
				else
					rectangles.Add(new Rect(0, 0, textures[i].width, textures[i].height));

			const int padding = 1;

			int numSpriteSheet = 0;
			while (rectangles.Count > 0) {

				Texture2D texture = new Texture2D(textureSize, textureSize, TextureFormat.ARGB32, mipChain);
				Color32[] fillColor = texture.GetPixels32();
				for (int i = 0; i < fillColor.Length; ++i)
					fillColor[i] = Color.clear;

				RectanglePacker packer = new RectanglePacker(texture.width, texture.height, padding);

				for (int i = 0; i < rectangles.Count; i++)
					packer.insertRectangle((int) rectangles[i].width, (int) rectangles[i].height, i);

				packer.packRectangles();

				if (packer.rectangleCount > 0) {

					texture.SetPixels32(fillColor);
					IntegerRectangle rect = new IntegerRectangle();
					List<TextureAsset> textureAssets = new List<TextureAsset>();

					List<Rect> garbageRect = new List<Rect>();
					List<Texture2D> garabeTextures = new List<Texture2D>();
					List<string> garbageImages = new List<string>();

					for (int j = 0; j < packer.rectangleCount; j++) {

						rect = packer.getRectangle(j, rect);

						int index = packer.getRectangleId(j);

						texture.SetPixels32(rect.x, rect.y, rect.width, rect.height, textures[index].GetPixels32());

						TextureAsset textureAsset = new TextureAsset();
						textureAsset.x = rect.x;
						textureAsset.y = rect.y;
						textureAsset.width = rect.width;
						textureAsset.height = rect.height;
						textureAsset.name = images[index];

						textureAssets.Add(textureAsset);

						garbageRect.Add(rectangles[index]);
						garabeTextures.Add(textures[index]);
						garbageImages.Add(images[index]);
					}

					foreach (Rect garbage in garbageRect)
						rectangles.Remove(garbage);

					foreach (Texture2D garbage in garabeTextures)
						textures.Remove(garbage);

					foreach (string garbage in garbageImages)
						images.Remove(garbage);

					texture.Apply();

					if (savePath != "") {

						File.WriteAllBytes(savePath + "/data" + numSpriteSheet + ".png", texture.EncodeToPNG());
						File.WriteAllText(savePath + "/data" + numSpriteSheet + ".json", JsonUtility.ToJson(new TextureAssets(textureAssets.ToArray()), true));
						++numSpriteSheet;
					}

                    foreach (TextureAsset textureAsset in textureAssets)
                    {
                        Sprite sprite = Sprite.Create(texture, new Rect(textureAsset.x, textureAsset.y, textureAsset.width, textureAsset.height), spritePivot, pixelsPerUnit, 0, SpriteMeshType.FullRect);
                        sprite.name = textureAsset.name;
                        mSprites.Add(textureAsset.name, sprite);
                    }
				}

			}

			OnProcessCompleted.Invoke();
		}

		protected IEnumerator loadPack(string savePath) {
			
			int numFiles = Directory.GetFiles(savePath).Length;

			for (int i = 0; i < numFiles / 2; ++i) {

				WWW loaderTexture = new WWW("file:///" + savePath + "/data" + i + ".png");
				yield return loaderTexture;

				WWW loaderJSON = new WWW("file:///" + savePath + "/data" + i + ".json");
				yield return loaderJSON;

				TextureAssets textureAssets = JsonUtility.FromJson<TextureAssets> (loaderJSON.text);

				Texture2D t = loaderTexture.texture; // prevent creating a new Texture2D each time.
                foreach (TextureAsset textureAsset in textureAssets.assets)
                {
                    Sprite sprite = Sprite.Create(t, new Rect(textureAsset.x, textureAsset.y, textureAsset.width, textureAsset.height), spritePivot, pixelsPerUnit, 0, SpriteMeshType.FullRect);
                    sprite.name = textureAsset.name;
                    mSprites.Add(textureAsset.name, sprite);
                }
			}

			yield return null;

			OnProcessCompleted.Invoke();
		}

		public void Dispose() {
            Debug.Assert(false);    // Testing whether this is actually hit or not
			foreach (var asset in mSprites)
				Destroy(asset.Value.texture);

			mSprites.Clear();
		}

		void Destroy() {

			Dispose();
		}

		public Sprite GetSprite(string id) {

			Sprite sprite = null;

			mSprites.TryGetValue (id, out sprite);

			return sprite;
		}

		public Sprite[] GetSprites(string prefix) {

			List<string> spriteNames = new List<string>();
			foreach (var asset in mSprites)
				if (asset.Key.StartsWith(prefix))
					spriteNames.Add(asset.Key);

			spriteNames.Sort(StringComparer.Ordinal);

			List<Sprite> sprites = new List<Sprite>();
			Sprite sprite;
			for (int i = 0; i < spriteNames.Count; ++i) {

				mSprites.TryGetValue(spriteNames[i], out sprite);

				sprites.Add(sprite);
			}

			return sprites.ToArray();
		}
	}
}
