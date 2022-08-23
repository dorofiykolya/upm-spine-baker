using System;
using UnityEngine;

namespace Spine.Unity.Editor
{
  public class SpineAnimationBakerTextureGenerator
  {
    private readonly Configuration _configuration;
    private Mesh _mesh;
    private Vector3 _minBounds;
    private Vector3 _maxBounds;
    private int _frames;
    private Texture2D _texture;
    private Texture2D _vertexColorTexture;

    public SpineAnimationBakerTextureGenerator(Mesh[] meshes, Configuration configuration)
    {
      _configuration = configuration;

      Parse(meshes);
    }

    public Mesh Mesh
    {
      get { return _mesh; }
    }

    public Vector3 MinBounds
    {
      get { return _minBounds; }
    }

    public Vector3 MaxBounds
    {
      get { return _maxBounds; }
    }

    public int Frames
    {
      get { return _frames; }
    }

    public Texture2D Texture
    {
      get { return _texture; }
    }

    public Texture2D VertexColorTexture
    {
      get { return _vertexColorTexture; }
    }

    private void Parse(Mesh[] meshes)
    {
      _mesh = GameObject.Instantiate(meshes[0]);
      _mesh.name = "Mesh";
      _frames = meshes.Length;

      ParseBounds(_mesh, meshes);
      ParseUV2(_mesh, meshes);
      ParseTexture(_mesh, meshes);
    }

    private void ParseBounds(Mesh initial, Mesh[] meshes)
    {
      _minBounds = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
      _maxBounds = new Vector3(float.MinValue, float.MinValue, float.MinValue);

      var initialVertices = initial.vertices;

      foreach (var mesh in meshes)
      {
        var vertexCount = mesh.vertexCount;
        var meshVertices = mesh.vertices;
        for (int i = 0; i < vertexCount; i++)
        {
          var diff = meshVertices[i] - initialVertices[i];

          if (_minBounds.x > diff.x) _minBounds.x = diff.x;
          if (_minBounds.y > diff.y) _minBounds.y = diff.y;
          if (_minBounds.z > diff.z) _minBounds.z = diff.z;

          if (_maxBounds.x < diff.x) _maxBounds.x = diff.x;
          if (_maxBounds.y < diff.y) _maxBounds.y = diff.y;
          if (_maxBounds.z < diff.z) _maxBounds.z = diff.z;
        }
      }
    }

    private void ParseUV2(Mesh mesh, Mesh[] meshes)
    {
      var uv2 = new Vector2[mesh.uv.Length];
      var xOffset = 1f / uv2.Length;
      var xOffsetHalf = xOffset / 2;

      var yOffset = 1f / meshes.Length;
      var yOffsetHalf = yOffset / 2f;

      for (int i = 0; i < uv2.Length; i++)
      {
        uv2[i] = new Vector2(xOffsetHalf + xOffset * i, yOffsetHalf);
      }

      mesh.uv2 = uv2;
    }

    private void ParseTexture(Mesh initial, Mesh[] meshes)
    {
      _texture = new Texture2D(initial.vertexCount, meshes.Length, GetTextureFormat(), false);
      _texture.wrapMode = TextureWrapMode.Repeat;
      _texture.filterMode = FilterMode.Point;
      _texture.name = "Animation";

      if (_configuration.VertexColor)
      {
        _vertexColorTexture = new Texture2D(initial.vertexCount, meshes.Length, GetTextureFormat(), false);
        _texture.wrapMode = TextureWrapMode.Repeat;
        _texture.filterMode = FilterMode.Point;
        _texture.name = "VertexColor";
      }

      var initialVertices = initial.vertices;
      var initialVerticesLength = initialVertices.Length;
      var meshCount = meshes.Length;

      var bounds = _maxBounds - _minBounds;

      for (int y = 0; y < meshCount; y++)
      {
        var meshVertices = meshes[y].vertices;
        var meshColors = meshes[y].colors;
        for (int x = 0; x < initialVerticesLength; x++)
        {
          var diff = meshVertices[x] - initialVertices[x];
          var currentDiff = diff - _minBounds;

          var r = Math.Abs(currentDiff.x / bounds.x);
          var g = Math.Abs(currentDiff.y / bounds.y);
          var b = Math.Abs(currentDiff.z / bounds.z);
          var a = meshColors != null ? meshColors[x].a : 1f;

          _texture.SetPixel(x, y, new Color(r, g, b, a));


          if (meshColors != null && _configuration.VertexColor)
          {
            _vertexColorTexture.SetPixel(x, y, meshColors[x]);
          }
        }
      }
    }

    private TextureFormat GetTextureFormat()
    {
      if (_configuration.Hdr)
      {
        if (_configuration.AlphaChannel)
        {
          return TextureFormat.RGBAFloat;
        }

        return TextureFormat.RGB9e5Float;
      }

      if (_configuration.AlphaChannel)
      {
        return TextureFormat.RGBA32;
      }

      return TextureFormat.RGB24;
    }
  }
}
