using System.IO;
using UnityEditor;
using UnityEngine;

namespace Spine.Unity.Editor
{
  public class BakedMeshes : ISaveToAsset
  {
    private readonly Mesh[] _meshes;
    private readonly SpineAnimationBaker _asset;
    private readonly Configuration _configuration;
    private readonly string _animationName;

    public BakedMeshes(Mesh[] meshes, SpineAnimationBaker asset, Configuration configuration, string animationName)
    {
      _meshes = meshes;
      _asset = asset;
      _configuration = configuration;
      _animationName = animationName;
    }

    public MeshSheet CreateAsset()
    {
      var meshSheet = ScriptableObject.CreateInstance<MeshSheet>();
      meshSheet.Meshes = _meshes;
      return meshSheet;
    }

    public void SaveToAsset()
    {
      var fileName = _asset.AssetPathWithoutExtension + "_(" + _animationName + ")_meshsheet.asset";
      if (File.Exists(fileName))
      {
        AssetDatabase.DeleteAsset(fileName);
      }

      var meshSheet = CreateAsset();
      AssetDatabase.CreateAsset(meshSheet, fileName);
      foreach (var mesh in meshSheet.Meshes)
      {
        AssetDatabase.AddObjectToAsset(mesh, meshSheet);
      }
      AssetDatabase.SaveAssets();
    }
  }
}
