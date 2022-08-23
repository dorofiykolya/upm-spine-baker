using UnityEngine;

namespace Spine.Unity
{
  public class MeshAnimation : MonoBehaviour
  {
    public MeshFilter MeshFilter;
    public MeshSheet Sheet;

    public bool Pause;
    public float Speed;
    public int CurrentFrame;
    private float _passedTime;

    private void Update()
    {
      //Fps = Fps < 0 ? 0 : Fps;
      if (MeshFilter != null && Sheet != null && Sheet.Meshes != null && Sheet.Meshes.Length != 0)
      {
        if (!Pause)
        {
          _passedTime += Time.deltaTime * Speed;
          var count = (int)(_passedTime);
          _passedTime -= count;
          CurrentFrame += count;
        }

        CurrentFrame = CurrentFrame % Sheet.Meshes.Length;

        var mesh = Sheet.Meshes[CurrentFrame];
        MeshFilter.sharedMesh = mesh;

      }
    }
  }
}
