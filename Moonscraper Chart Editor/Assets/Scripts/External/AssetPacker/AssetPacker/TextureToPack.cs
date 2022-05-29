// https://github.com/DaVikingCode/UnityRuntimeSpriteSheetsGenerator with modifications by FireFox

namespace DaVikingCode.AssetPacker {

	public class TextureToPack {
        public class GridSlice
        {
            public int width;
            public int height;
            public bool smallerSizeValid;   // if a file is passed in that is smaller than the provided slice params we allow it to pass as a single sprite

            public GridSlice(int width, int height, bool smallerSizeValid = true)
            {
                this.width = width;
                this.height = height;
                this.smallerSizeValid = smallerSizeValid;
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
