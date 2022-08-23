using UnityEngine;

namespace Spine.Unity.Editor
{
  public class SpineAnimationMeshesGenerator
  {
    private readonly Configuration _configuration;
    private readonly Mesh[] _meshes;

    public SpineAnimationMeshesGenerator(SpineAnimationBaker baker, Configuration configuration, string animationName)
    {
      _configuration = configuration;

      var animation = baker.GetAnimation(animationName);

      var go = new GameObject("Bake");
      var meshFilter = go.AddComponent<MeshFilter>();
      go.AddComponent<MeshRenderer>();
      var skeletonAnimation = go.AddComponent<SkeletonAnimation>();
      skeletonAnimation.skeletonDataAsset = baker.Asset;
      skeletonAnimation.Initialize(false);
      skeletonAnimation.AnimationName = animationName;

      var frames = configuration.Frames;
      var elapsedTime = animation.Duration / frames;

      var meshes = new Mesh[frames];

      skeletonAnimation.Update(0);
      skeletonAnimation.LateUpdate();
      meshes[0] = GameObject.Instantiate(meshFilter.sharedMesh);
      meshes[0].name = 0.ToString();

      for (int i = 1; i < frames; i++)
      {
        skeletonAnimation.Update(elapsedTime);
        skeletonAnimation.LateUpdate();
        meshes[i] = GameObject.Instantiate(meshFilter.sharedMesh);
        meshes[i].name = (i).ToString();
      }

      _meshes = meshes;
      GameObject.DestroyImmediate(go);
    }

    public Mesh[] Meshes
    {
      get { return _meshes; }
    }
  }
}
