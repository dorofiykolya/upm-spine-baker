using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Spine.Unity.Editor
{
  public class SpineAnimationBakeEditor : EditorWindow
  {
    [OnOpenAsset]
    public static bool OpenAssetData(int instanceID, int line)
    {
      var obj = EditorUtility.InstanceIDToObject(instanceID) as SkeletonDataAsset;
      if (obj != null)
      {
        var window = GetWindow<SpineAnimationBakeEditor>("Spine Baker");
        _asset = obj;
        _baker = null;
        window.Show(true);
      }

      return obj != null;
    }

    private static SkeletonDataAsset _asset;
    private static int _bakeFrames = 20;
    private static bool _hdr;
    private static SpineAnimationBaker _baker;
    private static bool _vertexColor;
    private static bool _alpha;

    void OnGUI()
    {
      if (_asset != null)
      {
        if (_baker == null)
        {
          _baker = SpineAnimationBaker.Create(_asset);
        }

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        _bakeFrames = EditorGUILayout.IntField("Bake Frames:", _bakeFrames);
        _hdr = EditorGUILayout.Toggle("HDR:", _hdr);
        _alpha = EditorGUILayout.Toggle("Alpha:", _alpha);
        _vertexColor = EditorGUILayout.Toggle("VertexColor:", _vertexColor);
        GUILayout.Label("Animations: " + _baker.Animations.Length, "box");
        foreach (var animation in _baker.Animations)
        {
          EditorGUILayout.BeginHorizontal();
          GUILayout.Label(animation.Name + " : " + animation.Duration);
          if (GUILayout.Button("Bake Meshes", EditorStyles.miniButton))
          {
            _baker.BakeMesh(animation.Name, CreateConfiguration()).SaveToAsset();
          }
          if (GUILayout.Button("Bake Textures", EditorStyles.miniButton))
          {
            _baker.BakeTexture(animation.Name, CreateConfiguration()).SaveToAsset();
          }
          EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();
      }
    }

    private Configuration CreateConfiguration()
    {
      return new Configuration
      {
        Hdr = _hdr,
        Frames = _bakeFrames,
        AlphaChannel = _alpha,
        VertexColor = _vertexColor
      };
    }
  }
}