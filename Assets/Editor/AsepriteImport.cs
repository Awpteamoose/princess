using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEditor.Animations;

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
		var thisFolder = Path.GetDirectoryName(path);
		var folderAbove = Path.GetDirectoryName(thisFolder);
		var json = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
		var data = JsonUtility.FromJson<AsepriteData>(json.text);
		var controller = AnimatorController.CreateAnimatorControllerAtPath(folderAbove + "/" + new DirectoryInfo(folderAbove).Name + ".anim");
		var stateMachine = controller.layers[0].stateMachine;

		foreach (var anim in data.meta.frameTags) {
			var clip = new AnimationClip();
			clip.wrapMode = WrapMode.Loop;
			clip.frameRate = 60;

			var keyframes = new ObjectReferenceKeyframe[anim.to - anim.from + +1];
			float time = 0;
			for (var i = anim.from; i <= anim.to; i++) {
				var frame = data.frames[i];
				var relativeIndex = i - anim.from;
				var spritename = anim.name + "_" + (relativeIndex + 1).ToString("D3") + ".png";
				var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(thisFolder + "/" + spritename);
				keyframes[relativeIndex] = new ObjectReferenceKeyframe {
					time = time,
					value = sprite
				};
				time = time + (frame.duration / 1000.0f);
			}
			var refCurve = new EditorCurveBinding {
				type = typeof(SpriteRenderer),
				path = "",
				propertyName = "m_Sprite"
			};
			AnimationUtility.SetObjectReferenceCurve(clip, refCurve, keyframes);
			AssetDatabase.CreateAsset(clip, folderAbove + "/" + anim.name + ".anim");
			var state = stateMachine.AddState(anim.name);
			state.motion = clip;
		}
		AssetDatabase.SaveAssets();
	}

	[MenuItem("Assets/Aseprite import", true)]
	private static bool NewMenuOptionValidation() {
		if (!Selection.activeObject) return false;
		return Selection.activeObject.GetType() == typeof(TextAsset);
	}
}
