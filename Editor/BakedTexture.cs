using System.IO;
using UnityEditor;
using UnityEngine;

namespace Spine.Unity.Editor
{
  public class BakedTexture : ISaveToAsset
  {
    private readonly SpineAnimationBakerTextureGenerator _textureGenerator;
    private readonly SpineAnimationBaker _asset;
    private readonly Configuration _configuration;
    private readonly string _animationName;

    public BakedTexture(SpineAnimationBakerTextureGenerator textureGenerator, SpineAnimationBaker asset, Configuration configuration, string animationName)
    {
      _textureGenerator = textureGenerator;
      _asset = asset;
      _configuration = configuration;
      _animationName = animationName;
    }

    public TextureAnimationAsset CreateAsset()
    {
      var textureAnimationAsset = ScriptableObject.CreateInstance<TextureAnimationAsset>();
      textureAnimationAsset.Mesh = _textureGenerator.Mesh;
      textureAnimationAsset.Frames = _textureGenerator.Frames;
      textureAnimationAsset.MaxBounds = _textureGenerator.MaxBounds;
      textureAnimationAsset.MinBounds = _textureGenerator.MinBounds;
      textureAnimationAsset.Speed = 1 / _asset.GetAnimation(_animationName).Duration;
      textureAnimationAsset.Texture = _textureGenerator.Texture;
      textureAnimationAsset.VertexColorTexture = _textureGenerator.VertexColorTexture;
      textureAnimationAsset.Mesh.name = _asset.AssetName;

      var material = new Material(Shader.Find("Unlit/TextureAnimation"));
      material.name = _asset.AssetName;
      material.SetVector("_BoundingMin", textureAnimationAsset.MinBounds);
      material.SetVector("_BoundingMax", textureAnimationAsset.MaxBounds);
      material.SetInt("_NumOfFrames", textureAnimationAsset.Frames);
      material.SetTexture("_PosTex", textureAnimationAsset.Texture);
      material.SetTexture("_VertexColorTex", textureAnimationAsset.VertexColorTexture);
      material.SetFloat("_Speed", textureAnimationAsset.Speed);

      textureAnimationAsset.Material = material;

      return textureAnimationAsset;
    }

    public void SaveToAsset()
    {
      var textureAnimationAsset = CreateAsset();

      var assetFileName = _asset.AssetPathWithoutExtension + "_(" + _animationName + ").asset";
      if (File.Exists(assetFileName))
      {
        AssetDatabase.DeleteAsset(assetFileName);
      }

      var textureBytes = Encode(textureAnimationAsset.Texture);
      var textureFileName = _asset.AssetPathWithoutExtension + "_(" + _animationName + ")" + GetExtension();
      File.WriteAllBytes(textureFileName, textureBytes);

      var vertexColorFileName = _asset.AssetPathWithoutExtension + "_(" + _animationName + ")_vertexcolor" + GetExtension();

      if (_configuration.VertexColor)
      {
        var vertexColorTextureBytes = Encode(textureAnimationAsset.VertexColorTexture);
        File.WriteAllBytes(vertexColorFileName, vertexColorTextureBytes);
      }

      AssetDatabase.Refresh();

      SetupTextureImporter(textureFileName);
      SetupTextureImporter(vertexColorFileName);

      textureAnimationAsset.Texture = AssetDatabase.LoadAssetAtPath<Texture2D>(textureFileName);
      textureAnimationAsset.VertexColorTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(vertexColorFileName);

      textureAnimationAsset.Material.SetTexture("_PosTex", textureAnimationAsset.Texture);
      textureAnimationAsset.Material.SetTexture("_VertexColorTex", textureAnimationAsset.VertexColorTexture);

      AssetDatabase.CreateAsset(textureAnimationAsset, assetFileName);
      AssetDatabase.AddObjectToAsset(textureAnimationAsset.Mesh, textureAnimationAsset);
      AssetDatabase.AddObjectToAsset(textureAnimationAsset.Material, textureAnimationAsset);

      var prefabFileName = _asset.AssetPathWithoutExtension + "_(" + _animationName + ").prefab";
      var go = new GameObject(_asset.AssetName + "_" + _animationName);
      go.AddComponent<MeshFilter>().sharedMesh = textureAnimationAsset.Mesh;
      go.AddComponent<MeshRenderer>().sharedMaterial = textureAnimationAsset.Material;
      var prefab = PrefabUtility.CreatePrefab(prefabFileName.Replace("\\", "/"), go);
      PrefabUtility.ReplacePrefab(go, prefab, ReplacePrefabOptions.ConnectToPrefab);
      GameObject.DestroyImmediate(go);

      AssetDatabase.SaveAssets();
    }

    private void SetupTextureImporter(string filePath)
    {
      var textureImporter = AssetImporter.GetAtPath(filePath) as TextureImporter;
      if (textureImporter != null)
      {
        textureImporter.filterMode = FilterMode.Point;
        textureImporter.wrapMode = TextureWrapMode.Repeat;
        textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
        textureImporter.npotScale = TextureImporterNPOTScale.None;
        textureImporter.mipmapEnabled = false;
        textureImporter.alphaIsTransparency = true;
        textureImporter.SaveAndReimport();
      }
    }

    private string GetExtension()
    {
      return _configuration.Hdr ? ".exr" : ".png";
    }

    private byte[] Encode(Texture2D texture2D)
    {
      if (_configuration.Hdr)
      {
        return texture2D.EncodeToEXR();
      }

      return texture2D.EncodeToPNG();
    }
  }
}
