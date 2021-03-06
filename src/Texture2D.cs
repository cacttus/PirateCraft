
using OpenTK.Graphics.OpenGL4;

namespace PirateCraft
{
  public enum NormalMapFormat
  {
    Zup, Yup
  }
  public enum TexFilter
  {
    Nearest, //If mipmaps enabled, then this will also choose nearest mipmap (mipmapping should be specified if possible)
    Linear, //Simple linear filtering of texels (no mipmapping, regardless if it is set)
    Bilinear, //Linear filter texture, but use nearest mipmap (smooth with some noticable mipmap jumps). 
    Trilinear //Linear filter texture, linear filter between mipmaps. (very smooth / blurry).
      , Separate
  }
  public class Texture2D : OpenGLResource
  {
    public static NormalMapFormat NormalMapFormat { get; private set; } = NormalMapFormat.Yup;
    private static Dictionary<Shader.TextureInput, Texture2D> _defaults = new Dictionary<Shader.TextureInput, Texture2D>();

    private int _numMipmaps = 0; // if zero mipmapping disabledd
    public float Width { get; private set; } = 0;
    public float Height { get; private set; } = 0;
    private bool MipmappingEnabled { get { return _numMipmaps > 0; } }
    public TexFilter Filter { get; private set; } = TexFilter.Nearest;
    public TextureWrapMode WrapMode { get; private set; } = TextureWrapMode.ClampToEdge;
    public PixelFormat GlPixelFormat
    {
      get
      {
        return PixelFormat.Bgra;
      }
      private set
      {
      }
    }
    public Texture2D(Img32 img, bool mipmaps, TexFilter filter, TextureWrapMode wrap = TextureWrapMode.Repeat)
    {
      LoadToGpu(img, mipmaps, filter, wrap);
    }
    public Texture2D(FileLoc loc, bool mipmaps, TexFilter filter, TextureWrapMode wrap = TextureWrapMode.Repeat)
    {
      var bmp = ResourceManager.LoadImage(loc);
      LoadToGpu(bmp, mipmaps, filter, wrap);
    }
    public override void Dispose_OpenGL_RenderThread()
    {
      if (GL.IsTexture(GetGlId()))
      {
        GL.DeleteTexture(GetGlId());
      }
    }
    private int GetNumMipmaps(int w, int h)
    {
      int numMipMaps = 0;
      int x = System.Math.Max(w, h);
      for (; x > 0; x = x >> 1)
      {
        numMipMaps++;
      }
      return numMipMaps;
    }

    class BoundTexture2DState
    {
      public TextureUnit Unit;
      public int BindingId;
    }
    static Stack<BoundTexture2DState> _states = new Stack<BoundTexture2DState>();
    private static TextureUnit GetActiveTexture()
    {
      int tex_unit = 0;
      GL.GetInteger(GetPName.ActiveTexture, out tex_unit);
      return (TextureUnit)tex_unit;
    }
    private static void PushState()
    {
      TextureUnit tex_unit = GetActiveTexture();
      int tex_binding = 0;
      GL.GetInteger(GetPName.TextureBinding2D, out tex_binding);

      _states.Push(new BoundTexture2DState() { Unit = tex_unit, BindingId = tex_binding });
    }
    private static void PopState()
    {
      BoundTexture2DState state = _states.Pop();
      GL.ActiveTexture(state.Unit);
      GL.BindTexture(TextureTarget.Texture2D, state.BindingId);
    }
    public void SetWrap(TextureWrapMode wrap)
    {
      Gu.Assert(GL.IsTexture(this.GetGlId()));
      WrapMode = wrap;
      PushState();
      Bind(GetActiveTexture());
      GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)WrapMode);
      GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)WrapMode);
      PopState();
    }
    public void SetFilter(TextureMinFilter min, TextureMagFilter mag)
    {
      Gu.Assert(GL.IsTexture(this.GetGlId()));
      Filter = TexFilter.Separate;
      PushState();
      Bind(GetActiveTexture());
      GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)min);//LinearMipmapLinear
      GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)mag);
      PopState();
    }
    public void SetFilter(TexFilter filter)
    {
      Filter = filter;

      TextureMinFilter min = TextureMinFilter.Nearest;
      TextureMagFilter mag = TextureMagFilter.Nearest;
      if (filter == TexFilter.Linear)
      {
        min = TextureMinFilter.Linear;
        mag = TextureMagFilter.Linear;
      }
      else if (filter == TexFilter.Nearest)
      {
        if (MipmappingEnabled)
        {
          //Mipmap nearest is smoother than simply nearest, it looks better
          min = TextureMinFilter.NearestMipmapNearest;
        }
        else
        {
          min = TextureMinFilter.Nearest;
        }
        mag = TextureMagFilter.Nearest;
      }
      else if (filter == TexFilter.Bilinear)
      {
        if (MipmappingEnabled)
        {
          min = TextureMinFilter.LinearMipmapNearest;
          mag = TextureMagFilter.Linear;
        }
        else
        {
          Gu.Log.Warn("No mipmaps specified for texure but bilinear / trilinear filtering was specified.");
        }
      }
      else if (filter == TexFilter.Trilinear)
      {
        if (MipmappingEnabled)
        {
          min = TextureMinFilter.LinearMipmapLinear;
          mag = TextureMagFilter.Linear;
        }
        else
        {
          Gu.Log.Warn("No mipmaps specified for texure but bilinear / trilinear filtering was specified.");
        }
      }
      else
      {
        Gu.BRThrowNotImplementedException();
      }
      SetFilter(min, mag);

    }
    private void LoadToGpu(Img32 bmp, bool mipmaps, TexFilter filter, TextureWrapMode wrap)
    {
      Width = bmp.Width;
      Height = bmp.Height;
      Filter = filter;
      WrapMode = WrapMode;

      int ts = Gu.Context.Gpu.GetMaxTextureSize();
      if (Width >= ts)
      {
        Gu.BRThrowException("Texture is too large");
      }
      if (Height >= ts)
      {
        Gu.BRThrowException("Texture is too large");
      }

      _numMipmaps = 1;
      if (mipmaps)
      {
        _numMipmaps = GetNumMipmaps((int)Width, (int)Height);
      }

      _glId = GL.GenTexture();
      GL.BindTexture(TextureTarget.Texture2D, GetGlId());
      SetFilter(filter);
      SetWrap(wrap);

      GL.TexStorage2D(TextureTarget2d.Texture2D, _numMipmaps, SizedInternalFormat.Rgba8, (int)Width, (int)Height);

      //var raw = Gpu.SerializeGPUData(bmp.Data);
      var raw = Gpu.GetGpuDataPtr(bmp.Data);

      GL.TexSubImage2D(TextureTarget.Texture2D,
          0, //mipmap level
          0, 0, //x.y
          bmp.Width,
          bmp.Height,
          GlPixelFormat,
          PixelType.UnsignedByte,
          raw.Lock());
      raw.Unlock();

      if (mipmaps)
      {
        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
      }
    }
    public static Texture2D Default(Shader.TextureInput input)
    {
      if (!_defaults.TryGetValue(input, out Texture2D texture))
      {
        Texture2D tex = null;
        if (input == Shader.TextureInput.Albedo)
        {
          //White albedo
          Img32 b = Img32.Default1x1(255, 255, 255, 255);// new Img32(1, 1, new byte[] { 255, 255, 255, 255 });
          tex = new Texture2D(b, false, TexFilter.Nearest);
        }
        else if (input == Shader.TextureInput.Normal)
        {
          //Normal texture pointing up from surface (default) 
          byte[] dat = null;
          if (NormalMapFormat == NormalMapFormat.Yup)
          {
            dat = new byte[] { 0, 255, 0, 255 };
          }
          else if (NormalMapFormat == NormalMapFormat.Zup)
          {
            dat = new byte[] { 0, 0, 255, 255 };
          }
          else
          {
            Gu.BRThrowNotImplementedException();
          }

          Img32 b = new Img32(1, 1, dat);
          tex = new Texture2D(b, false, TexFilter.Nearest);
        }
        else
        {
          Gu.Log.WarnCycle("Default texture not handled for Texture2D::BindDefault");
        }
        if (tex != null)
        {
          _defaults.Add(input, tex);
        }
      }
      return texture;
    }
    TextureUnit _boundUnit = TextureUnit.Texture0;
    public void Bind(TextureUnit unit)
    {
      if (GetGlId() == 0)
      {
        throw new System.Exception("Texture ID was 0 when binding texture.");
      }

      _boundUnit = unit;
      GL.ActiveTexture(unit);
      GL.BindTexture(TextureTarget.Texture2D, GetGlId());
    }
    public void Unbind()
    {
      GL.ActiveTexture(_boundUnit);
      GL.BindTexture(TextureTarget.Texture2D, 0);
    }

  }
}
