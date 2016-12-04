using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.ComponentModel;

public class AsepriteImport {
	[Serializable]
	struct Size {
		public int w, h;
	}

	[Serializable]
	struct FullSize {
		public int x, y, w, h;
	}

	[Serializable]
	struct FrameData {
		public string filename;
		public FullSize frame;
		public bool rotated;
		public bool trimmed;
		public FullSize spriteSourceSize;
		public Size sourceSize;
		public int duration;
	}

	[Serializable]
	struct FrameTag {
		public string name;
		public int from, to;
		public string direction;
	}

	[Serializable]
	struct MetaData {
		public string app;
		public string version;
		public string format;
		public Size size;
		public string scale;
		public FrameTag[] frameTags;
	}

	[Serializable]
	struct AsepriteData {
		public FrameData[] frames;
		public MetaData meta;
	}

	[MenuItem("Assets/Aseprite import")]
	static void Import() {
		var path = AssetDatabase.GetAssetPath(Selection.activeObject);
		var json = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
		var data = JsonUtility.FromJson<AsepriteData>(json.text);

		foreach (var anim in data.meta.frameTags) {
			Debug.Log("WORKING ON ANIMATION " + anim.name);
			var clip = new AnimationClip();
			EditorCurveBinding curveBinding = new EditorCurveBinding();
			curveBinding.type = typeof(SpriteRenderer);
			curveBinding.path = "";
			curveBinding.propertyName = "sprite";

			// An array to hold the object keyframes
			ObjectReferenceKeyframe[] keyFrames = new ObjectReferenceKeyframe[10];

			for (int i = 0; i < 10; i++) {
				keyFrames[i] = new ObjectReferenceKeyframe();
				// set the time
				keyFrames[i].time = timeForKey(i);
				// set reference for the sprite you want
				keyFrames[i].value = spriteForKey(i);

			}

			AnimationUtility.SetObjectReferenceCurve(animClip, curveBinding, keyFrames);
			for (var i = anim.from; i <= anim.to; i++) {
				var frame = data.frames[i];
				var spritename = anim.name + "_" + (i - anim.from + 1).ToString("D3") + ".png";
				Debug.Log("Actual file name: " + spritename + ", duration " + frame.duration);
			}
			AssetDatabase.CreateAsset(clip, "Assets/TEST/" + anim.name + ".anim");
		}
		AssetDatabase.SaveAssets();
	}

	[MenuItem("Assets/Aseprite import", true)]
	private static bool NewMenuOptionValidation() {
		return Selection.activeObject.GetType() == typeof(TextAsset);
	}
}
