﻿using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Drawing.Imaging;
using Vec2f = OpenTK.Vector2;
using Vec3f = OpenTK.Vector3;
using Vec4f = OpenTK.Vector4;
using Mat4f = OpenTK.Matrix4;

namespace PirateCraft
{

  public class Camera3D : WorldObject
  {
    float _fov = MathUtils.ToRadians(70.0f);
    float _near = 1;
    float _far = 1000;
    private float _widthNear = 1;
    private float _heightNear = 1;
    private float _widthFar = 1;
    private float _heightFar = 1;

    Vec3f _nearCenter = new Vec3f(0, 0, 0);
    Vec3f _farCenter = new Vec3f(0, 0, 0);
    Vec3f _nearTopLeft = new Vec3f(0, 0, 0);
    Vec3f _farTopLeft = new Vec3f(0, 0, 0);
    Mat4f _projectionMatrix = Mat4f.Identity;
    Mat4f _viewMatrix = Mat4f.Identity;
    //ProjectionMode ProjectionMode = ProjectionMode.Perspective;

    public float FOV { get { return _fov; } set { _fov = value; } }
    public float Near { get { return _near; } private set { _near = value;  } }
    public float Far { get { return _far; } private set { _far = value;  } }
    public Vec3f NearCenter { get { return _nearCenter; } private set { _nearCenter = value; } }
    public Vec3f FarCenter { get { return _farCenter; } private set { _farCenter = value;  } }
    public Vec3f NearTopLeft { get { return _nearTopLeft; } private set { _nearTopLeft = value;  } }
    public Vec3f FarTopLeft { get { return _farTopLeft; } private set { _farTopLeft = value; } }
    public Mat4f ProjectionMatrix { get { return _projectionMatrix; } private set { _projectionMatrix = value;} }
    public Mat4f ViewMatrix { get { return _viewMatrix; } private set { _viewMatrix = value;  } }

    public int _view_x = 0, _view_y = 0, _view_w = 800, _view_h = 600;
    public int Viewport_X { get { return _view_x; } set { _view_x = value; } }
    public int Viewport_Y { get { return _view_y; } set { _view_y = value; } }
    public int Viewport_Width { get { return _view_w; } set { _view_w = value; } }
    public int Viewport_Height { get { return _view_h; } set { _view_h = value; } }

    public Camera3D(int w, int h, float near = 1, float far = 1000)
    {
      //Do not select camera (at least not active camera) since we wont be able to hit anything else.
      //SelectEnabled = false;
      _view_w = w;
      _view_h = h;
    }
    public override void Update(Box3f parentBoundBox = null)
    {
      base.Update(parentBoundBox);

      //Not really necessary to keep calling this unless we change window parameters
      ProjectionMatrix = Mat4f.CreatePerspectiveFieldOfView(FOV, Viewport_Width / Viewport_Height, Near, Far);
      ViewMatrix = Mat4f.LookAt(Position, Position + BasisZ.Normalized(), new Vec3f(0, 1, 0));

      //Frustum
      float tanfov2 = MathUtils.tanf(FOV / 2.0f);
      float ar = ((float)Viewport_Width / (float)Viewport_Height);

      //tan(fov2) = w2/near
      //tan(fov2) * near = w2
      //w/h = w2/h2
      //(w/h)*h2 = w2
      //w2/(w/h) = h2
      _widthNear = tanfov2 * Near * 2;
      _heightNear = _widthNear / ar;
      _widthFar = tanfov2 * Far * 2;
      _heightFar = _widthFar / ar;

      NearCenter = Position + BasisZ * Near;
      FarCenter = Position + BasisZ * Far;
      NearTopLeft = NearCenter - BasisX * _widthNear + BasisY * _heightNear;
      FarTopLeft = FarCenter - BasisX * _widthFar + BasisY * _heightFar;

      //    }
      //    _updating = false;
      //}
      //_dirty = false;
    }
    public void BeginRender()
    {
        GL.Viewport(0, 0, Viewport_Width, Viewport_Height);
        GL.Scissor(0, 0, Viewport_Width, Viewport_Height);
    }
    public void EndRender()
    {
    }
    //public override void Resize(Viewport vp) { }
    //public override void Update(double dt) { base.Update(dt); }
    //public override void Render(Renderer rm) { }
    //public override void Free() { }
    //public override void UpdateBoundBox()
    //{
    //    //BoundBoxComputed._vmax = BoundBoxComputed._vmin = Pos;
    //    //BoundBoxComputed._vmax.X += _mainVolume._radius;
    //    //BoundBoxComputed._vmax.Y += _mainVolume._radius;
    //    //BoundBoxComputed._vmax.Z += _mainVolume._radius;
    //    //BoundBoxComputed._vmin.X += -_mainVolume._radius;
    //    //BoundBoxComputed._vmin.Y += -_mainVolume._radius;
    //    //BoundBoxComputed._vmin.Z += -_mainVolume._radius;
    //}

    public Line3f ProjectPoint(Vec2f point_on_screen, TransformSpace space = TransformSpace.World, float additionalZDepthNear = 0)
    {
      //Note: we were using PickRay before because that's used to pick OOBs. We don't need that right now but we will in the future.
      Line3f pt = new Line3f();

      float left_pct = point_on_screen.X / (float)Viewport_Width;
      float top_pct = (point_on_screen.Y) / (float)Viewport_Height;

      if (space == TransformSpace.Local)
      {
        //Transform in local coordinates.
        Vec3f localX = new Vec3f(1, 0, 0);
        Vec3f localY = new Vec3f(0, 1, 0);
        Vec3f localZ = new Vec3f(0, 0, 1);
        Vec3f near_center_local = localZ * Near;
        Vec3f far_center_local = localZ * Far;
        Vec3f ntl = near_center_local - localX * _widthNear + localY * _heightNear;
        Vec3f ftl = far_center_local - localX * _widthFar + localY * _heightFar;
        pt.p0 = ntl + localX * _widthNear * left_pct + localY * _heightNear * top_pct;
        pt.p1 = ftl + localX * _widthFar * left_pct + localY * _heightFar * top_pct;
        pt.p0 += localZ * additionalZDepthNear;
      }
      else
      {
        pt.p0 = NearTopLeft + BasisX * _widthNear * left_pct + BasisY * _heightNear * top_pct;
        pt.p1 = FarTopLeft + BasisX * _widthFar * left_pct + BasisY * _heightFar * top_pct;
        pt.p0 += BasisZ * additionalZDepthNear;
      }

      return pt;
    }

  }
}
