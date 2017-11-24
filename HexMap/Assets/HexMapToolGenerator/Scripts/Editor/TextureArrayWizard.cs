using UnityEditor;
using UnityEngine;

public class TextureArrayWizard : ScriptableWizard {

	#region Variables

	public Texture2D[] _textures;

	#endregion

	#region Unity_Methods

	[MenuItem("HexMapGenerator/Create/TextureArray")]
	static void CreateWizard() {
		ScriptableWizard.DisplayWizard<TextureArrayWizard> ("Create Texture Array", "Create");
	}

	void OnWizardCreate() {
		if (!SystemInfo.supports2DArrayTextures) {
			Debug.Log ("The current platforme does not support the Texture2D array");
			return;
		}

		if (_textures.Length == 0) {
			return;		
		}

		string path = EditorUtility.SaveFilePanelInProject ("Save Texture Array", "Texture Array", "asset", "Save Texture Array");

		if (path.Length == 0) {
			return;
		}

		Texture2D t = _textures [0];
		Texture2DArray textureArray = new Texture2DArray (t.width, t.height, _textures.Length, t.format, t.mipmapCount > 1);
		textureArray.anisoLevel = t.anisoLevel;
		textureArray.filterMode = t.filterMode;
		textureArray.wrapMode = t.wrapMode;

		for (int i = 0; i < _textures.Length; ++i) {
			for (int m = 0; m < t.mipmapCount; ++m) {
				Graphics.CopyTexture (_textures [i], 0, m, textureArray, i, m);
			}
		}

		AssetDatabase.CreateAsset (textureArray, path);
	}


	#endregion

	#region Methods


	#endregion
}
