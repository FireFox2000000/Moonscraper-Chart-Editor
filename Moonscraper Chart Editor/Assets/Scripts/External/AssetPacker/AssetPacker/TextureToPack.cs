// https://github.com/DaVikingCode/UnityRuntimeSpriteSheetsGenerator with modifications by FireFox

namespace DaVikingCode.AssetPacker {

	public class TextureToPack {
        public class GridSlice
        {
            public int width;
            public int height;

            public GridSlice(int width, int height)
            {
                this.width = width;
                this.height = height;
            }
        }

		public string file;
        public UnityEngine.Texture2D preLoadedTexture;
		public string id;
        public GridSlice sliceParams;

        public TextureToPack(string file, string id, GridSlice sliceParams = null) {

			this.file = file;
			this.id = id;
            this.sliceParams = sliceParams;
            this.preLoadedTexture = null;
        }

        public TextureToPack(UnityEngine.Texture2D preLoadedTexture, string id, GridSlice sliceParams = null)
        {

            this.file = string.Empty;
            this.id = id;
            this.sliceParams = sliceParams;
            this.preLoadedTexture = preLoadedTexture;
        }
    }
}
