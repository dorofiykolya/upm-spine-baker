using System.IO;
using UnityEditor;

namespace Spine.Unity.Editor
{
  public class SpineAnimationBaker
  {
    public static SpineAnimationBaker Create(SkeletonDataAsset asset)
    {
      return new SpineAnimationBaker(asset);
    }

    private readonly SkeletonDataAsset _asset;
    private readonly Animation[] _animations;
    private readonly string _assetPath;
    private readonly string _assetPathWithoutExtension;
    private readonly string _assetName;

    private SpineAnimationBaker(SkeletonDataAsset asset)
    {
      _asset = asset;
      _assetPath = AssetDatabase.GetAssetPath(_asset);
      _assetName = Path.GetFileNameWithoutExtension(_assetPath);
      _assetPathWithoutExtension = Path.Combine(Path.GetDirectoryName(_assetPath), _assetName);
      var data = _asset.GetSkeletonData(false);
      if (data != null)
      {
        _animations = data.Animations.ToArray();
      }
    }

    public Animation[] Animations { get { return _animations; } }
    public SkeletonDataAsset Asset { get { return _asset; } }
    public string AssetPath { get { return _assetPath; } }
    public string AssetName { get { return _assetName; } }
    public string AssetPathWithoutExtension { get { return _assetPathWithoutExtension; } }

    public Animation GetAnimation(string animationName)
    {
      foreach (var animation in Animations)
      {
        if (animation.Name == animationName) return animation;
      }
      return null;
    }

    public BakedMeshes BakeMesh(string animationName, Configuration configuration)
    {
      var meshGenerator = new SpineAnimationMeshesGenerator(this, configuration, animationName);
      var meshes = meshGenerator.Meshes;

      var result = new BakedMeshes(meshes, this, configuration, animationName);
      return result;
    }

    public BakedTexture BakeTexture(string animationName, Configuration configuration)
    {
      var meshGenerator = new SpineAnimationMeshesGenerator(this, configuration, animationName);
      var meshes = meshGenerator.Meshes;

      var textureGenerator = new SpineAnimationBakerTextureGenerator(meshes, configuration);
      var result = new BakedTexture(textureGenerator, this, configuration, animationName);

      return result;
    }
  }

  public interface ISaveToAsset
  {
    void SaveToAsset();
  }
}
