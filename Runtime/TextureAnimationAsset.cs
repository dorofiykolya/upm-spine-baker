using UnityEngine;

namespace Spine.Unity
{
  public class TextureAnimationAsset : ScriptableObject
  {
    public Mesh Mesh;
    public Texture2D Texture;
    public Texture2D VertexColorTexture;
    public Vector3 MinBounds;
    public Vector3 MaxBounds;
    public int Frames;
    public float Speed;
    public Material Material;
  }
}
