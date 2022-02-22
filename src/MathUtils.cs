﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
//using Microsoft.xna.Framework;

//This is a very simple and useful math class in just one file
//Copied this stuff from legend of kevin
//Copied from VulkanGame
namespace PirateCraft
{
   public class MathUtils
   {
      public const float M_PI = (float)(Math.PI);
      public const float M_2PI = (float)(Math.PI * 2.0f);
      public const float M_PI_2 = (float)(Math.PI * 2.0f);
      public static float sqrtf(float f)
      {
         return (float)Math.Sqrt(f);
      }
      public static float sinf(float f)
      {
         return (float)Math.Sin(f);
      }
      public static float cosf(float f)
      {
         return (float)Math.Cos(f);
      }
      public static float tanf(float f)
      {
         return (float)Math.Tan(f);
      }
      public static float asinf(float f)
      {
         return (float)Math.Asin(f);
      }
      public static float acosf(float f)
      {
         return (float)Math.Acos(f);
      }
      public static float atanf(float f)
      {
         return (float)Math.Atan(f);
      }
      public static float Clamp(float f, float a, float b)
      {
         return Math.Max(Math.Min(f, b), a);
      }
      public static mat4 m4f16(float[] mv)
      {
         if (mv.Length != 16)
            throw new Exception("matrix was not 16 elements wide.");

         return new mat4(
               mv[0],
               mv[1],
               mv[2],
               mv[3],
               mv[4],
               mv[5],
               mv[6],
               mv[7],
               mv[8],
               mv[9],
               mv[10],
               mv[11],
               mv[12],
               mv[13],
               mv[14],
               mv[15]
               );
      }

      public static float ToDegrees(float rads)
      {
         return (rads / ((float)Math.PI)) * 180.0f;
      }
      public static float ToRadians(float degs)
      {
         return (degs / 180.0f) * (float)Math.PI;
      }
      public static double ToDegrees(double rads)
      {
         return (rads / ((double)Math.PI)) * 180.0f;
      }
      public static double ToRadians(double degs)
      {
         return (degs / 180.0f) * (double)Math.PI;
      }
      public static vec2 DecomposeRotation(float r)
      {
         //Turn a rotation into a vector (for chars mostly)
         float r2 = r - (float)Math.PI * 0.5f;
         vec2 dxy = new vec2(
             (float)Math.Cos(r2),
             (float)Math.Sin(r2)
             );

         return dxy;
      }
      public static float GetRotationFromLine(float x, float y, float x2, float y2)
      {
         //https://stackoverflow.com/questions/270138/how-do-i-draw-lines-using-xna
         //this returns the angle between two points in radians 
         float adj = x - x2;
         float opp = y - y2;
         float tan = opp / adj;
         float res = MathUtils.ToDegrees((float)Math.Atan2(opp, adj));
         res = (res - 180) % 360;
         if (res < 0) { res += 360; }
         res = MathUtils.ToRadians(res);
         return res;
      }
      //http://xboxforums.create.msdn.com/forums/t/34356.aspx
      public static bool IsPowerOfTwo(ulong x)
      {
         return (x & (x - 1)) == 0;
      }
   }
   public struct RaycastHit
   {
      public bool _bHit;    // Whether the ray intersected the box.
      public bool _p1Contained;
      public bool _p2Contained;
      public float _t; // - Time to hit [0,1]
                       //  public void* _pPickData; // picked object (BvhObject3*)
      public vec2 _vNormal; //The normal of the plane the raycast hit.
                            //Do not include ray data for optimization.

      //public RaycastHit()
      //{
      //    reset();
      //}
      public bool trySetClosestHit(ref float closest_t)
      {
         //Easy way of iterating a closest hit.
         if (_bHit && (_t < closest_t))
         {
            closest_t = _t;
            return true;
         }
         return false;
      }
      public void reset()
      {
         _bHit = false;
         _p1Contained = false;
         _p2Contained = false;
         _t = float.MaxValue;
         //  _pPickData = NULL;
      }
      public void copyFrom(RaycastHit bh)
      {
         _bHit = bh._bHit;
         _p1Contained = bh._p1Contained;
         _p2Contained = bh._p2Contained;
         _t = bh._t;
      }
   }
   public class PickRay2D
   {
      public vec2 Origin;
      public vec2 Dir;
      public float Length = float.MaxValue;// Length of the ray NOT the maximum length of the pick ray
      public float _t = float.MaxValue;
      public vec2 InvDir;// Found the following two cool optimizations on WIlliams et. al (U. Utah)
      public int[] Sign = new int[2];

      public bool DidHit { get { return _t >= 0 && _t <= 1; } }
      public vec2 PickedPoint { get { return Origin + Dir * _t; } }

      public PickRay2D(vec2 origin, vec2 dir, float length)
      {
         Origin = origin;
         Dir = dir;
         Length = length;
         InvDir.x = 1.0f / Dir.x;
         InvDir.y = 1.0f / Dir.y;
         Sign[0] = (InvDir.x < 0) ? 1 : 0;
         Sign[1] = (InvDir.y < 0) ? 1 : 0;
      }

   }
   public class BoxAAHit
   {
      public bool _bHit;  // Whether the ray intersected the box.
      public bool _p1Contained;
      public bool _p2Contained;
      public float _t; // - Time to hit [0,1]
   };
   public class PickRay3D
   {
      public vec3 Origin;
      public vec3 Dir; //Ray direction
      public float Length = float.MaxValue;
      public float _t = float.MaxValue;
      public vec3 InvDir;
      public int[] Sign = new int[3];

      public bool DidHit { get { return _t >= 0 && _t <= 1; } }
      public vec3 PickedPoint { get { return Origin + Dir * _t; } }

      public vec3 Project(vec3 p)
      {
         //Project p onto ray returning projected point
         vec3 ret;
         vec3 p2 = Origin + Dir * Length;
         ret = Origin + Dir * Length * Line3f.pointOnLine_t(Origin, p2, p);
         return ret;
      }

      public PickRay3D(Line3f line)
      {
         Init(line.p0, (line.p1 - line.p0).normalized(), (line.p1 - line.p0).length());
      }
      public PickRay3D(vec3 origin, vec3 dir, float length)
      {
         Init(origin, dir, length);
      }
      private void Init(vec3 origin, vec3 dir, float length)
      {
         Origin = origin;
         Dir = dir;
         Length = length;
         InvDir.x = 1.0f / Dir.x;
         InvDir.y = 1.0f / Dir.y;
         InvDir.z = 1.0f / Dir.z;
         Sign[0] = Convert.ToInt32(InvDir.x < 0);
         Sign[1] = Convert.ToInt32(InvDir.y < 0);
         Sign[2] = Convert.ToInt32(InvDir.z < 0);
      }
   }
   [StructLayout(LayoutKind.Sequential)]
   public class Plane3f
   {
      public Plane3f() { }
      public float d;
      public vec3 n;
      public Plane3f(vec3 dn, vec3 dpt)
      {
         d = -n.dot(dpt);
         n = dn;
      }
      public Plane3f(vec3 tri_p1, vec3 tri_p2, vec3 tri_p3, vec3 tri_p4)
      {
         //The TBN is not needed for this - copied from VulkanGame::PlaneEx3
         float u = 1.0f;
         vec3 origin = tri_p1;

         vec3 t = (tri_p2 - tri_p1);
         vec3 b = (tri_p3 - tri_p1);

         t /= u;
         t.normalize();
         n = b.cross(t);  //20161129 - NOTE: CHANGED THIS FOR THE RHS COORDINATES

         n.normalize();
         b = n.cross(t);

         d = -n.dot(origin);
      }
      public float IntersectLine(vec3 p1, vec3 p2)
      {
         float t = -(n.dot(p1) + d) / ((p2 - p1).dot(n));
         return t;
      }
      public float dist(vec3 p)
      {
         return (float)(n.dot(p) + d);
      }

   }
   [StructLayout(LayoutKind.Sequential)]
   public struct Line2f
   {
      public vec2 p0;
      public vec2 p1;
   }
   [StructLayout(LayoutKind.Sequential)]
   public struct Line3f
   {
      public vec3 p0;
      public vec3 p1;

      public static float pointOnLine_t(vec3 p0, vec3 p1, vec3 pt)
      {
         //Returns closest point on this line.
         vec3 dP = pt - p0;
         vec3 dL = p1 - p0;
         float dPdL = dP.dot(dL);
         float dLdL = dL.dot(dL);
         float t = dPdL / dLdL;

         return t;
      }
      public static float pointOnRay_t(vec3 ray, vec3 pt)
      {
         //Returns the point on ray between [0,1] of the ray.
         vec3 AP = pt - ray;
         vec3 AB = ray * -1.0f;
         float ab2 = AB.x * AB.x + AB.y * AB.y + AB.z * AB.z;
         float ap_ab = AP.x * AB.x + AP.y * AB.y + AP.z * AB.z;
         float t = ap_ab / ab2;
         return t;
      }

   }
   [StructLayout(LayoutKind.Sequential)]
   public struct vec2
   {
      public vec2(Point p)
      {
         x = (float)p.X;
         y = (float)p.Y;
      }
      public float x, y;
      public vec2 construct(float a, float b) { x = a; y = b; return this; }
      //public Vec2f() { }
      public vec2(OpenTK.Vector2 dxy) { x = dxy.X; y = dxy.Y; }
      public vec2(vec2 dxy) { x = dxy.x; y = dxy.y; }
      public vec2(float dx, float dy) { x = dx; y = dy; }
      //  public vec2(OpenTK.Vector2 v) { x = v.x; y = v.y; }//From XNA's Vector2
      public float Len() { return (float)Math.Sqrt((x * x) + (y * y)); }

      public vec2 Perp()
      {
         //Perpendicular
         return new vec2(y, -x);
      }
      public void Normalize()
      {
         float l = Len();
         if (l != 0)
         {
            x /= l;
            y /= l;
         }
         else
         {
            x = 0; y = 0;
         }

      }
      public vec2 Normalized()
      {
         vec2 v = new vec2(this);
         v.Normalize();
         return v;

      }
      public float Len2() { return Dot(this, this); }
      public OpenTK.Vector2 toXNA() { return new OpenTK.Vector2(x, y); }


      static public implicit operator vec2(float f)
      {
         return new vec2(f, f);
      }
      //public static Vec2f operator =(Vec2f a, float f)
      //{
      //    return new Vec2f(f, f);
      //}
      public static float Dot(vec2 a, vec2 b)
      {
         return (a.x * b.x) + (a.y * b.y);
      }
      public float Dot(vec2 b)
      {
         return (x * b.x) + (y * b.y);
      }
      public static vec2 operator -(vec2 d)
      {
         return new vec2(-d.x, -d.y);
      }
      public static vec2 operator +(vec2 a, vec2 b)
      {
         return new vec2(a.x + b.x, a.y + b.y);
      }
      public static vec2 operator -(vec2 a, vec2 b)
      {
         return new vec2(a.x - b.x, a.y - b.y);
      }
      public static vec2 operator *(vec2 a, float b)
      {
         return new vec2(a.x * b, a.y * b);
      }
      public static vec2 operator *(vec2 a, vec2 b)
      {
         return new vec2(a.x * b.x, a.y * b.y);
      }
      public static vec2 operator /(vec2 a, float b)
      {
         return new vec2(a.x / b, a.y / b);
      }
      public static vec2 operator -(vec2 a, float f)
      {
         return new vec2(a.x - f, a.y - f);
      }
      public static vec2 Minv(vec2 a, vec2 b)
      {
         vec2 ret = new vec2();
         ret.x = (float)Math.Min(a.x, b.x);
         ret.y = (float)Math.Min(a.y, b.y);

         return ret;
      }
      public static vec2 Maxv(vec2 a, vec2 b)
      {
         vec2 ret = new vec2();
         ret.x = (float)Math.Max(a.x, b.x);
         ret.y = (float)Math.Max(a.y, b.y);
         return ret;
      }

   }
   [StructLayout(LayoutKind.Sequential)]
   public struct vec3
   {
      public float x;
      public float y;
      public float z;

      public static vec3 VEC3_MIN()
      {
         return new vec3(float.MinValue, float.MinValue, float.MinValue);
      }
      public static vec3 VEC3_MAX()
      {
         return new vec3(float.MaxValue, float.MaxValue, float.MaxValue);
      }

      public vec3(OpenTK.Vector3 v)
      {
         x = v.X;
         y = v.Y;
         z = v.Z;
      }
      public vec3(vec3 rhs)
      {
         this.x = rhs.x;
         this.y = rhs.y;
         this.z = rhs.z;
      }
      public vec3(float dx, float dy, float dz)
      {
         x = dx;
         y = dy;
         z = dz;
      }
      public vec3(int rhs)
      {
         x = (float)rhs;
         y = (float)rhs;
         z = (float)rhs;
      }
      public vec3(float rhs)
      {
         x = rhs;
         y = rhs;
         z = rhs;
      }
      public vec3 construct(float dx, float dy, float dz)
      {
         x = dx; y = dy; z = dz;
         return this;
      }
      public override string ToString()
      {
         return "" + x + "," + y + "," + z;
      }
      //    public static vec3 minv(in vec3 v_a, in vec3 v_b) {
      //      vec3 outt = new vec3();

      //      outt.x = Math.Min(v_a.x, v_b.x);
      //      outt.y = Math.Min(v_a.y, v_b.y);
      //      outt.z = Math.Min(v_a.z, v_b.z);

      //      return outt;
      //    }
      //public static vec3 maxv(in vec3 v_a, in vec3 v_b)
      //{
      //      vec3 outt = new vec3();

      //      outt.x = Math.Max(v_a.x, v_b.x);
      //      outt.y = Math.Max(v_a.y, v_b.y);
      //      outt.z = Math.Max(v_a.z, v_b.z);

      //      return outt;
      //}
      // template <class Tx>
      // vec3(const Vec2x<float>& rhs) {
      //   x = (float)rhs.x;
      //   y = (float)rhs.y;
      //   z = 0;  // This isn't correct. The problem is we get auto casted when we add.
      // }
      // template <class Tx>
      // vec3(const Vec2x<double>& rhs) {
      //   x = (float)rhs.x;
      //   y = (float)rhs.y;
      //   z = 0;  // This isn't correct. The problem is we get auto casted when we add.
      // }
      // template <class Tx>
      // vec3(const Vec4x<Tx>& rhs) {
      //   x = (float)rhs.x;
      //   y = (float)rhs.y;
      //   z = (float)rhs.z;
      // }

      //////////////////////////////////////////////////////////////////////////
      //////////////////////////////////////////////////////////////////////////
      //////////////////////////////////////////////////////////////////////////

      public static vec3 minv(in vec3 v_a, in vec3 v_b)
      {
         vec3 outv = new vec3();

         outv.x = Math.Min(v_a.x, v_b.x);
         outv.y = Math.Min(v_a.y, v_b.y);
         outv.z = Math.Min(v_a.z, v_b.z);

         return outv;
      }
      public static vec3 maxv(in vec3 v_a, in vec3 v_b)
      {
         vec3 outv = new vec3();

         outv.x = Math.Max(v_a.x, v_b.x);
         outv.y = Math.Max(v_a.y, v_b.y);
         outv.z = Math.Max(v_a.z, v_b.z);

         return outv;
      }
      public static vec3 maxv_a(in vec3 v_a, in vec3 v_b)
      {
         vec3 outv = new vec3();

         outv.x = Math.Max(Math.Abs(v_a.x), Math.Abs(v_b.x));
         outv.y = Math.Max(Math.Abs(v_a.y), Math.Abs(v_b.y));
         outv.z = Math.Max(Math.Abs(v_a.z), Math.Abs(v_b.z));
         return outv;
      }
      public static float maxf_a(in vec3 v_a, in vec3 v_b)
      {
         vec3 tmp = maxv_a(v_a, v_b);
         return Math.Max(Math.Abs(tmp.x), Math.Max(Math.Abs(tmp.y), Math.Abs(tmp.z)));
      }
      public vec2 xz()
      {
         return new vec2(x, z);
      }
      public vec2 xy()
      {
         return new vec2(x, y);
      }


      //vec3 VEC3X_MIN()
      //{
      //    return vec3(-COMP_MAX < float >::m(), -COMP_MAX < float >::m(), -COMP_MAX < float >::m());
      //}

      //vec3 VEC3X_MAX()
      //{
      //    return vec3(COMP_MAX < float >::m(), COMP_MAX < float >::m(), COMP_MAX < float >::m());
      //}
      //#define VEC3_MIN (vec3(-FLT_MAX,-FLT_MAX,-FLT_MAX))
      //#define VEC3_MAX (vec3(FLT_MAX,FLT_MAX,FLT_MAX))
      public float length()
      {
         return (float)Math.Sqrt(x * x + y * y + z * z);
      }
      public double lengthd()
      {
         double dx = (double)x;
         double dy = (double)y;
         double dz = (double)z;

         return Math.Sqrt(dx * dx + dy * dy + dz * dz);
      }
      public float length2()
      {
         return (x * x + y * y + z * z);
      }
      public float squaredLength()
      {
         return length2();
      }
      public vec3 normalize()
      {
         //we may be able to get away with rsqrt here...
         //but maybe not.
         float a = length();
         return normalize(a);
      }
      public vec3 normalize(float len)
      {
         //we may be able to get away with rsqrt here...
         //but maybe not.
         // We should allow the float to hit infinity if we try to divide zero
         if (len != 0)
         {
            float a1 = 1.0f / len;
            x *= a1;
            y *= a1;
            z *= a1;
         }
         else
         {
            x = y = z = 0;
         }
         return this;
      }
      public void len_and_norm(out vec3 n, out float len)
      {
         //Computes length and normal to avoid having do do len() then norm()
         len = length();
         n = this;
         n.normalize(len);
      }
      public vec3 normalized()
      {
         vec3 ret = new vec3(this);
         return ret.normalize();
      }
      public vec3 abs()
      {
         vec3 ret = new vec3(this);
         ret.x = Math.Abs(ret.x);
         ret.y = Math.Abs(ret.y);
         ret.z = Math.Abs(ret.z);
         return ret;
      }
      public float dot(in vec3 v)
      {
         return (x * v.x + y * v.y + z * v.z);
      }
      public float distance(in vec3 v1)
      {
         return ((this) - v1).length();
      }
      public float distance2(in vec3 v1)
      {
         return ((this) - v1).length2();
      }
      public vec3 cross(in vec3 v1)
      {
         vec3 vt;
         vt.x = (y * v1.z) - (v1.y * z);
         vt.y = (z * v1.x) - (v1.z * x);
         vt.z = (x * v1.y) - (v1.x * y);

         return vt;
      }
      public vec3 lerpTo(in vec3 v1, float t)
      {
         vec3 ret = this + (v1 - this) * t;
         return ret;
      }
      public vec3 clampTo(in vec3 vMin, in vec3 vMax)
      {
         //Technically we can just use the #define for clamp() to get the same result.
         //but my brain isn't working right now and i want to see this line for line
         vec3 outv = new vec3(this);

         if (outv.x < vMin.x)
         {
            outv.x = vMin.x;
         }
         if (outv.y < vMin.y)
         {
            outv.y = vMin.y;
         }
         if (outv.z < vMin.z)
         {
            outv.z = vMin.z;
         }

         if (outv.x > vMax.x)
         {
            outv.x = vMax.x;
         }
         if (outv.y > vMax.y)
         {
            outv.y = vMax.y;
         }
         if (outv.z > vMax.z)
         {
            outv.z = vMax.z;
         }

         return outv;
      }
      public static vec3 operator -(in vec3 d)
      {
         return new vec3(-d.x, -d.y, -d.z);
      }
      public static vec3 operator +(in vec3 a, in vec3 b)
      {
         return new vec3(a.x + b.x, a.y + b.y, a.z + b.z);
      }
      public static vec3 operator -(in vec3 a, in vec3 b)
      {
         return new vec3(a.x - b.x, a.y - b.y, a.z - b.z);
      }
      public static vec3 operator *(in vec3 a, in vec3 b)
      {
         return new vec3(a.x * b.x, a.y * b.y, a.z * b.z);
      }
      public static vec3 operator /(in vec3 a, in vec3 b)
      {
         return new vec3(a.x / b.x, a.y / b.y, a.z / b.z);
      }
      public static vec3 operator +(in vec3 a, float f)
      {
         return new vec3(a.x + f, a.y + f, a.z + f);
      }
      public static vec3 operator -(in vec3 a, float f)
      {
         return new vec3(a.x - f, a.y - f, a.z - f);
      }
      public static vec3 operator *(in vec3 a, float b)
      {
         return new vec3(a.x * b, a.y * b, a.z * b);
      }
      public static vec3 operator /(in vec3 a, float b)
      {
         return new vec3(a.x / b, a.y / b, a.z / b);
      }
      public static bool operator >(in vec3 v1, in vec3 v2)
      {
         return (v1.x > v2.x && v1.y > v2.y && v1.z > v2.z);
      }
      public static bool operator >=(in vec3 v1, in vec3 v2)
      {
         return (v1.x >= v2.x && v1.y >= v2.y && v1.z >= v2.z);
      }
      public static bool operator <(in vec3 v1, in vec3 v2)
      {
         return (v1.x < v2.x && v1.y < v2.y && v1.z < v2.z);
      }
      public static bool operator <=(in vec3 v1, in vec3 v2)
      {
         return (v1.x <= v2.x && v1.y <= v2.y && v1.z <= v2.z);
      }

      // template <class Tx>
      // Vec3x<Tx> Vec3x<Tx>::operator*(const Mat3x<Tx>& m) {
      //   Vec3x<Tx> ret;
      //   ret.x = (Tx)(m._m11 * x + m._m21 * x + m._m31 * x);
      //   ret.y = (Tx)(m._m12 * y + m._m22 * y + m._m32 * y);
      //   ret.z = (Tx)(m._m13 * z + m._m23 * z + m._m33 * z);
      //   return ret;
      // }
      //bool operator>(float f) {
      //  return (x > f && y > f && z > f);
      //}

      //bool operator>=(float f) {
      //  return (x >= f && y >= f && z >= f);
      //}

      //bool operator<(float f) {
      //  return (x<f && y<f && z<f);
      //}

      //bool operator<=(float f) {
      //  return (x <= f && y <= f && z <= f);
      //}

      //// Constructors

      //void construct(in vec3 rhs)
      //{
      //    x = rhs.x;
      //    y = rhs.y;
      //    z = rhs.z;
      //}

      //void construct(float dx, float dy, float dz)
      //{
      //    x = dx;
      //    y = dy;
      //    z = dz;
      //}

      //vec3 zero()
      //{
      //    return vec3(0, 0, 0);
      //}


      //uint32_t toUint() {
      //  uint32_t ret = (uint32_t)(
      //      ((uint32_t)0 << 16) |
      //      ((uint32_t)r() << 16) |
      //      ((uint32_t)g() << 8) |
      //      ((uint32_t)b()));
      //  return ret;
      //}

      //void fromUint(const uint32_t& i)
      //{
      //    r() = (i >> 16) & 0xFF;
      //    g() = (i >> 8) & 0xFF;
      //    b() = (i) & 0xFF;
      //}

      bool compareTo(in vec3 rhs)
      {
         vec3 lhs = this;

         if (lhs.x < rhs.x)
         {
            return true;
         }
         else if (lhs.x > rhs.x)
         {
            return false;
         }
         else
         {
            if (lhs.y < rhs.y)
            {
               return true;
            }
            else if (lhs.y > rhs.y)
            {
               return false;
            }
            else
            {
               if (lhs.z < rhs.z)
               {
                  return true;
               }
               else
               {//if(lhs->z > rhs->z)
                  return false;
               }
            }
         }
      }

      ////template < class float >
      ////vec3( const dvec3& rhs )
      ////{
      ////  x = (float)rhs.x;
      ////  y = (float)rhs.y;
      ////  z = (float)rhs.z;
      ////}

      //// - Vector shorthands
      public static vec3 normalize(in vec3 v1)
      {
         return (new vec3(v1)).normalized();
      }
      public static vec3 cross(in vec3 v1, in vec3 v2)
      {
         return (new vec3(v1)).cross(new vec3(v2));
      }
      ////template < typename float >
      ////vec3 rotate(in vec3 v1, const float& angle, in vec3 normal)
      ////{
      ////  //TODO: test.
      ////  Mat3f m = Mat3f::getRotationRad(angle, normal);
      ////  vec3 ret = v1;
      ////  ret = ret*m;
      ////  return ret;
      ////}
      //template<typename Tx>
      public static float dot(in vec3 v1, in vec3 v2)
      {
         return (new vec3(v1)).dot(new vec3(v2));
      }
      //template<typename Tx>
      //void bilinear_interpolate(
      //    in vec3 a,
      //    in vec3 b,
      //    in vec3 c,
      //    in vec3 d,
      //    vec3& __out_ avg,
      //    float pct)
      //{
      //    vec3 v1, v2, v3;
      //    v1 = a + (b - a) * pct;
      //    v2 = c + (d - c) * pct;
      //    avg = v1 + (v2 - v1) * pct;
      //}
      //template<typename Tx>
      static void reflect(in vec3 v, in vec3 n, out vec3 v_r)
      {
         v_r = v - (n * n.dot(v)) * 2.0f;
      }
      //template<typename Tx>
      //void checkNormalOrZero()
      //{
      //    //Make sure the number is a normal FP number
      //    int cx = std::fpclassify(x);
      //    int cy = std::fpclassify(y);
      //    int cz = std::fpclassify(z);
      //    if (cx != FP_ZERO && cx != FP_NORMAL)
      //        x = 0.0f;
      //    if (cy != FP_ZERO && cy != FP_NORMAL)
      //        y = 0.0f;
      //    if (cz != FP_ZERO && cz != FP_NORMAL)
      //        z = 0.0f;
      //}
      //template<typename Tx>
      //bool isNormalFloat()
      //{
      //    bool b = true;

      //    //Make sure the number is a normal FP number
      //    int cx = std::fpclassify(x);
      //    int cy = std::fpclassify(y);
      //    int cz = std::fpclassify(z);
      //    //NAN
      //    if (cx == FP_NAN)
      //    {
      //        b = false;
      //    }
      //    if (cy == FP_NAN)
      //    {
      //        b = false;
      //    }
      //    if (cz == FP_NAN)
      //    {
      //        b = false;
      //    }
      //    ////DEN
      //    //If the number is too small who cares. Let it round to zero.
      //    //AssertOrThrow2(cx!= FP_SUBNORMAL);
      //    //AssertOrThrow2(cy!= FP_SUBNORMAL);
      //    //AssertOrThrow2(cz!= FP_SUBNORMAL);
      //    //INF
      //    if (cx == FP_INFINITE)
      //    {
      //        b = false;
      //    }
      //    if (cy == FP_INFINITE)
      //    {
      //        b = false;
      //    }
      //    if (cz == FP_INFINITE)
      //    {
      //        b = false;
      //    }

      //    return b;
      //}
      //template<typename Tx>
      //void checkNormalOrZeroAndLimitVector(float fMaxLength, bool bShowWarningMessage)
      //{
      //    //Normalize number
      //    checkNormalOrZero();

      //    // Make sure the vector length isn't too big.
      //    if (squaredLength() >= (fMaxLength * fMaxLength))
      //    {
      //        if (bShowWarningMessage == true)
      //            BRLogWarn("Object has launched into orbit: v=(", x, ", y, " ", z, ")");
      //        *this = normalized() * fMaxLength;
      //    }
      //}

      //class Vec3Basis : public VirtualMemory {
      //public:
      //    vec3 _x, _y, _z;
      //};
   }
   [StructLayout(LayoutKind.Sequential)]
   public struct vec4
   {
      public float x, y, z, w;

      public vec4(vec3 d, float dw) { x = d.x; y = d.y; z = d.z; w = dw; }
      public vec4(vec4 dxy) { x = dxy.x; y = dxy.y; z = dxy.z; w = dxy.w; }
      public vec4(float dx, float dy, float dz, float dw) { x = dx; y = dy; z = dz; w = dw; }
      public vec4(OpenTK.Vector4 v) { x = v.X; y = v.Y; z = v.Z; w = v.W; }//From XNA's Vector2

      public override string ToString() { return "(" + x + "," + y + "," + z + "," + w + ")"; }
      public vec4 construct(float dx, float dy, float dz, float dw)
      {
         x = dx; y = dy; z = dz; w = dw;
         return this;
      }

      public static vec4 Clamp(vec4 v, float a, float b)
      {
         vec4 ret = new vec4();
         ret.z = OpenTK.MathHelper.Clamp(v.z, a, b);
         ret.x = OpenTK.MathHelper.Clamp(v.x, a, b);
         ret.y = OpenTK.MathHelper.Clamp(v.y, a, b);
         ret.w = OpenTK.MathHelper.Clamp(v.w, a, b);
         return ret;
      }
      public void Clamp(float a, float b)
      {
         this = Clamp(this, a, b);
      }
      public void SetMinLightValue(float val)
      {
         //Make sure there's enough light for this.
         //Val = the minimum amount of light.
         //This isn't perfect
         float tot = x + y + z;
         if (tot < val)
         {
            float add = (2 - tot) / val;
            x += add;
            y += add;
            z += add;
            x = OpenTK.MathHelper.Clamp(x, 0, 1);
            y = OpenTK.MathHelper.Clamp(y, 0, 1);
            z = OpenTK.MathHelper.Clamp(z, 0, 1);
         }

      }
      public OpenTK.Vector4 toOpenTK()
      {
         return new OpenTK.Vector4(x, y, z, w);
      }
      public OpenTK.Graphics.Color4 toOpenTKColor()
      {
         var x = toOpenTK();
         return new OpenTK.Graphics.Color4(x.X, x.Y, x.Z, x.W);
      }
      public static vec4 operator -(vec4 d)
      {
         return new vec4(-d.x, -d.y, -d.z, -d.w);
      }
      public static vec4 operator +(vec4 a, vec4 b)
      {
         return new vec4(a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w);
      }
      public static vec4 operator -(vec4 a, vec4 b)
      {
         return new vec4(a.x - b.x, a.y - b.y, a.z - b.z, a.w - b.w);
      }
      public static vec4 operator *(vec4 a, vec4 b)
      {
         return new vec4(a.x * b.x, a.y * b.y, a.z * b.z, a.w * b.w);
      }
      public static vec4 operator *(vec4 a, float b)
      {
         return new vec4(a.x * b, a.y * b, a.z * b, a.w * b);
      }
      public static vec4 operator /(vec4 a, float b)
      {
         return new vec4(a.x / b, a.y / b, a.z / b, a.w / b);
      }
      public static vec4 Minv(vec4 a, vec4 b)
      {
         vec4 ret = new vec4();
         ret.x = (float)Math.Min(a.x, b.x);
         ret.y = (float)Math.Min(a.y, b.y);
         ret.z = (float)Math.Min(a.z, b.z);
         ret.w = (float)Math.Min(a.w, b.w);
         return ret;
      }
      public static vec4 Maxv(vec4 a, vec4 b)
      {
         vec4 ret = new vec4();
         ret.x = (float)Math.Max(a.x, b.x);
         ret.y = (float)Math.Max(a.y, b.y);
         ret.z = (float)Math.Max(a.z, b.z);
         ret.w = (float)Math.Max(a.w, b.w);
         return ret;
      }
      public vec3 xyz()
      {
         return new vec3(x, y, z);
      }
   }
   [StructLayout(LayoutKind.Sequential)]
   public struct ivec2
   {
      public ivec2(int dx, int dy) { x = dx; y = dy; }
      public Int32 x { get; set; }
      public Int32 y { get; set; }
      static public implicit operator ivec2(int f)
      {
         return new ivec2(f, f);
      }
      public static ivec2 operator -(ivec2 d)
      {
         return new ivec2(-d.x, -d.y);
      }
      public static ivec2 operator +(ivec2 a, ivec2 b)
      {
         return new ivec2(a.x + b.x, a.y + b.y);
      }
      public static ivec2 operator -(ivec2 a, ivec2 b)
      {
         return new ivec2(a.x - b.x, a.y - b.y);
      }
      public static ivec2 operator *(ivec2 a, int b)
      {
         return new ivec2(a.x * b, a.y * b);
      }
      public static ivec2 operator *(ivec2 a, ivec2 b)
      {
         return new ivec2(a.x * b.x, a.y * b.y);
      }
      public static ivec2 operator /(ivec2 a, int b)
      {
         return new ivec2(a.x / b, a.y / b);
      }
      public static ivec2 operator -(ivec2 a, int f)
      {
         return new ivec2(a.x - f, a.y - f);
      }
   }
   [StructLayout(LayoutKind.Sequential)]
   public struct uvec2
   {
      public uvec2(int dx, int dy) { x = dx; y = dy; }
      public Int32 x { get; set; }
      public Int32 y { get; set; }
      static public implicit operator uvec2(int f)
      {
         return new uvec2(f, f);
      }
      public static uvec2 operator -(uvec2 d)
      {
         return new uvec2(-d.x, -d.y);
      }
      public static uvec2 operator +(uvec2 a, uvec2 b)
      {
         return new uvec2(a.x + b.x, a.y + b.y);
      }
      public static uvec2 operator -(uvec2 a, uvec2 b)
      {
         return new uvec2(a.x - b.x, a.y - b.y);
      }
      public static uvec2 operator *(uvec2 a, int b)
      {
         return new uvec2(a.x * b, a.y * b);
      }
      public static uvec2 operator *(uvec2 a, uvec2 b)
      {
         return new uvec2(a.x * b.x, a.y * b.y);
      }
      public static uvec2 operator /(uvec2 a, int b)
      {
         return new uvec2(a.x / b, a.y / b);
      }
      public static uvec2 operator -(uvec2 a, int f)
      {
         return new uvec2(a.x - f, a.y - f);
      }
   }
   [StructLayout(LayoutKind.Sequential)]
   public struct ivec3
   {
      public Int32 x;
      public Int32 y;
      public Int32 z;
      public ivec3(Int32 dx, Int32 dy, Int32 dz)
      {
         x = dx; y = dy; z = dz;
      }
      public static ivec3 operator -(in ivec3 d)
      {
         return new ivec3(-d.x, -d.y, -d.z);
      }
      public static ivec3 operator +(in ivec3 a, in ivec3 b)
      {
         return new ivec3(a.x + b.x, a.y + b.y, a.z + b.z);
      }
      public static ivec3 operator -(in ivec3 a, in ivec3 b)
      {
         return new ivec3(a.x - b.x, a.y - b.y, a.z - b.z);
      }
      public static ivec3 operator *(in ivec3 a, in ivec3 b)
      {
         return new ivec3(a.x * b.x, a.y * b.y, a.z * b.z);
      }
      public static ivec3 operator /(in ivec3 a, in ivec3 b)
      {
         return new ivec3(a.x / b.x, a.y / b.y, a.z / b.z);
      }
      public static bool operator ==(in ivec3 a, in ivec3 b)
      {
         return (a.x == b.x) && (a.y == b.y) && (a.z == b.z);
      }
      public static bool operator !=(in ivec3 a, in ivec3 b)
      {
         return (a.x != b.x) || (a.y != b.y) || (a.z != b.z);
      }
      public override int GetHashCode()
      {
         return base.GetHashCode();
      }
      public override bool Equals(object obj)
      {
         return base.Equals(obj);
      }
      public class ivec3EqualityComparer : IEqualityComparer<ivec3>
      {
         public bool Equals(ivec3 a, ivec3 b)
         {
            return a.x == b.x && a.y == b.y && a.z == b.z;
         }

         public int GetHashCode(ivec3 a)
         {
            return a.GetHashCode();//.x.GetHashCode() + a.y.GetHashCode() + a.z.GetHashCode();
         }
      }
      public vec3 toVec3() { return new vec3((float)x, (float)y, (float)z); }
   }
   [StructLayout(LayoutKind.Sequential)]
   public struct ivec4
   {
      public Int32 x;
      public Int32 y;
      public Int32 z;
      public Int32 w;
      public ivec4(Int32 dx, Int32 dy, Int32 dz, Int32 dw)
      {
         x = dx; y = dy; z = dz; w = dw;
      }
   }
   [StructLayout(LayoutKind.Sequential)]
   public struct mat2
   {
      float _m11, _m12, _m21, _m22;

      public static vec2 operator *(mat2 a, vec2 b)
      {
         vec2 r = new vec2(0, 0);

         r.x = a._m11 * b.x + a._m12 * b.y;
         r.y = a._m21 * b.x + a._m22 * b.y;
         return r;
      }
      public static mat2 getRotation(float theta)
      {
         mat2 ret = new mat2();

         //This is an incorrect rotation function - sin 10 shouldn't be negative.
         ret._m11 = (float)Math.Cos(theta);
         ret._m12 = (float)Math.Sin(theta);
         ret._m21 = -(float)Math.Sin(theta);
         ret._m22 = (float)Math.Cos(theta);

         return ret;
      }
   }
   [StructLayout(LayoutKind.Sequential)]
   public struct mat3
   {
      public float _m11, _m12, _m13;
      public float _m21, _m22, _m23;
      public float _m31, _m32, _m33;
      public static int CompSize() { return 9; }
      public mat3(
         float m11, float m12, float m13,
         float m21, float m22, float m23,
         float m31, float m32, float m33
         )
      {
         _m11 = m11;
         _m12 = m12;
         _m13 = m13;
         _m21 = m21;
         _m22 = m22;
         _m23 = m23;
         _m31 = m31;
         _m32 = m32;
         _m33 = m33;
      }
      public mat3(in mat4 rhs)
      {
         _m11 = rhs._m11;
         _m12 = rhs._m12;
         _m13 = rhs._m13;

         _m21 = rhs._m21;
         _m22 = rhs._m22;
         _m23 = rhs._m23;

         _m31 = rhs._m31;
         _m32 = rhs._m32;
         _m33 = rhs._m33;
      }
      public mat3 transposed()
      {
         mat3 mt = new mat3(
            _m11, _m21, _m31,
            _m12, _m22, _m32,
            _m13, _m23, _m33
            );
         return mt;
      }
      public void setIdentity()
      {
         _m11 = _m22 = _m33 = 1.0f;
         _m12 = _m13 = _m21 = _m23 = _m31 = _m32 = 0;
      }
      public static mat3 operator +(in mat3 a, in mat3 b)
      {
         mat3 ret = new mat3();
         for (int i = 0; i < CompSize(); ++i)
         {
            ret[i] = a[i] + b[i];
         }
         return ret;
      }
      //operator[]
      public float this[int i]
      {
         get
         {
            if (i == 0)
            {
               return this._m11;
            }
            else if (i == 1)
            {
               return this._m12;
            }
            else if (i == 2)
            {
               return this._m13;
            }
            else if (i == 3)
            {
               return this._m21;
            }
            else if (i == 4)
            {
               return this._m22;
            }
            else if (i == 5)
            {
               return this._m23;
            }
            else if (i == 6)
            {
               return this._m31;
            }
            else if (i == 7)
            {
               return this._m32;
            }
            else if (i == 8)
            {
               return this._m33;
            }
            else
            {
               throw new Exception("mat3 array index out of bounds.");
            }
         }
         set
         {
            if (i == 0)
            {
               this._m11 = value;
            }
            else if (i == 1)
            {
               this._m12 = value;
            }
            else if (i == 2)
            {
               this._m13 = value;
            }
            else if (i == 3)
            {
               this._m21 = value;
            }
            else if (i == 4)
            {
               this._m22 = value;
            }
            else if (i == 5)
            {
               this._m23 = value;
            }
            else if (i == 6)
            {
               this._m31 = value;
            }
            else if (i == 7)
            {
               this._m32 = value;
            }
            else if (i == 8)
            {
               this._m33 = value;
            }
            else
            {
               throw new Exception("mat3 array index out of bounds.");
            }
         }
      }
      public void set(float val, int index)
      {
         if (index == 0) { _m11 = val; }
         else if (index == 1) { _m12 = val; }
         else if (index == 2) { _m13 = val; }
         else if (index == 3) { _m21 = val; }
         else if (index == 4) { _m22 = val; }
         else if (index == 5) { _m23 = val; }
         else if (index == 6) { _m31 = val; }
         else if (index == 7) { _m32 = val; }
         else if (index == 8) { _m33 = val; }
         else throw new Exception("Mat3 index out of range");
      }
      public mat3 adj()
      {
         // - The expanded cofactor adjoint.
         mat3 m = new mat3();
         m._m11 = 0.0f * ((_m22 * _m33) - (_m23 * _m32));
         m._m12 = 1.0f * ((_m21 * _m33) - (_m23 * _m31));
         m._m13 = 0.5f * ((_m21 * _m32) - (_m22 * _m31));
         m._m21 = 1.0f * ((_m12 * _m33) - (_m13 * _m32));
         m._m22 = 0.5f * ((_m11 * _m33) - (_m13 * _m31));
         m._m23 = (float)Math.Pow(-1.00f, 2 + 1) * ((_m12 * _m31) - (_m11 * _m32));
         m._m31 = 0.5f * ((_m12 * _m23) - (_m13 * _m22));
         m._m32 = (float)Math.Pow(-1.00f, 2 + 1) * ((_m11 * _m23) - (_m13 * _m21));
         m._m33 = 0.25f * ((_m11 * _m22) - (_m12 * _m21));
         return m;
      }
      public float det()
      {
         return (
             _m11 * _m22 * _m33 +
             _m21 * _m32 * _m13 +
             _m12 * _m23 * _m31 - (_m13 * _m22 * _m31) - (_m12 * _m21 * _m33) - (_m23 * _m32 * _m11));
      }
      public mat3 inverse()
      {
         mat3 m = adj();

         float d = m.det();
         for (int i = 0; i < CompSize(); ++i)
         {
            m[i] /= d;
         }
         return m;
      }
      public static mat3 getRotation(in vec3 axis, in float angle)
      {
         mat3 Temp;

         vec3 xyz = axis.normalized();

         float x = xyz.x;
         float y = xyz.y;
         float z = xyz.z;

         float c = (float)Math.Cos(angle);
         float s = (float)Math.Sin(angle);
         float nc = 1 - c;
         // row matrix

         Temp._m11 = (x * x) * nc + c;
         Temp._m12 = (x * y) * nc + (z * s);
         Temp._m13 = (x * z) * nc - (y * s);

         Temp._m21 = (y * x) * nc - (z * s);
         Temp._m22 = (y * y) * nc + c;
         Temp._m23 = (y * z) * nc + (x * s);

         Temp._m31 = (z * x) * nc + (y * s);
         Temp._m32 = (z * y) * nc - (x * s);
         Temp._m33 = (z * z) * nc + c;

         return Temp;
      }
      public quat toQuat()
      {
         quat q = new quat();
         mat3 m = this.transposed();//Testing to see if this is thr problem..it is -- fix this

         float tr = m._m11 + m._m22 + m._m33;

         if (tr > 0)
         {
            float S = MathUtils.sqrtf(tr + 1.0f) * 2; // S=4*qw 
            q.w = 0.25f * S;
            q.x = (m._m32 - m._m23) / S;
            q.y = (m._m13 - m._m31) / S;
            q.z = (m._m21 - m._m12) / S;
         }
         else if ((m._m11 > m._m22) & (m._m11 > m._m33))
         {
            float S = MathUtils.sqrtf(1.0f + m._m11 - m._m22 - m._m33) * 2; // S=4*qx 
            q.w = (m._m32 - m._m23) / S;
            q.x = 0.25f * S;
            q.y = (m._m12 + m._m21) / S;
            q.z = (m._m13 + m._m31) / S;
         }
         else if (m._m22 > m._m33)
         {
            float S = MathUtils.sqrtf(1.0f + m._m22 - m._m11 - m._m33) * 2; // S=4*qy
            q.w = (m._m13 - m._m31) / S;
            q.x = (m._m12 + m._m21) / S;
            q.y = 0.25f * S;
            q.z = (m._m23 + m._m32) / S;
         }
         else
         {
            float S = MathUtils.sqrtf(1.0f + m._m33 - m._m11 - m._m22) * 2; // S=4*qz
            q.w = (m._m21 - m._m12) / S;
            q.x = (m._m13 + m._m31) / S;
            q.y = (m._m23 + m._m32) / S;
            q.z = 0.25f * S;
         }


         //float s0, s1, s2;
         //int k0, k1, k2, k3;
         //float[] q1 = new float[4];
         //if ((m._m11 + m._m22 + m._m33) > 0.0f)
         //{
         //   k0 = 3;
         //   k1 = 2;
         //   k2 = 1;
         //   k3 = 0;
         //   s0 = 1.0f;
         //   s1 = 1.0f;
         //   s2 = 1.0f;
         //}
         //else if ((m._m11 + m._m22 > 0.0f) && (m._m11 > m._m33))
         //{
         //   k0 = 0;
         //   k1 = 1;
         //   k2 = 2;
         //   k3 = 3;
         //   s0 = 1.0f;
         //   s1 = -1.0f;
         //   s2 = -1.0f;
         //}
         //else if (m._m22 > m._m33)
         //{
         //   k0 = 1;
         //   k1 = 0;
         //   k2 = 3;
         //   k3 = 2;
         //   s0 = -1.0f;
         //   s1 = 1.0f;
         //   s2 = -1.0f;
         //}
         //else
         //{
         //   k0 = 2;
         //   k1 = 3;
         //   k2 = 0;
         //   k3 = 1;
         //   s0 = -1.0f;
         //   s1 = -1.0f;
         //   s2 = 1.0f;
         //}
         //float t = (float)(s0 * m._m11 + s1 * m._m22 + s2 * m._m33 + 1.0f);
         ////assert(t>=0.0);
         ////if( t==0.0 ) t=1e-10f;
         //float s = (float)((1.0 / Math.Sqrt(t)) * 0.5f);

         //q1[k0] = s * t;

         //q1[k1] = (float)((m._m12 - s2 * m._m21) * s);
         //q1[k2] = (float)((m._m31 - s1 * m._m13) * s);
         //q1[k3] = (float)((m._m23 - s0 * m._m32) * s);

         //quat ret = new quat(q1[k0], q1[k1], q1[k2], q1[k3]);
         return q;
      }

   }
   [StructLayout(LayoutKind.Sequential)]
   public struct mat4
   {
      public float _m11, _m12, _m13, _m14;   // indexes: 0,1,2,3
      public float _m21, _m22, _m23, _m24;  // 4,5,6,7...
      public float _m31, _m32, _m33, _m34;  //
      public float _m41, _m42, _m43, _m44;  //
      public int CompSize() { return 16; }
      public mat4(in mat4 rhs)
      {
         _m11 = rhs._m11;
         _m12 = rhs._m12;
         _m13 = rhs._m13;
         _m14 = rhs._m14;

         _m21 = rhs._m21;
         _m22 = rhs._m22;
         _m23 = rhs._m23;
         _m24 = rhs._m24;

         _m31 = rhs._m31;
         _m32 = rhs._m32;
         _m33 = rhs._m33;
         _m34 = rhs._m34;

         _m41 = rhs._m41;
         _m42 = rhs._m42;
         _m43 = rhs._m43;
         _m44 = rhs._m44;

      }
      public mat4(in mat3 rhs)
      {
         _m11 = rhs._m11;
         _m12 = rhs._m12;
         _m13 = rhs._m13;
         _m14 = 0;

         _m21 = rhs._m21;
         _m22 = rhs._m22;
         _m23 = rhs._m23;
         _m24 = 0;

         _m31 = rhs._m31;
         _m32 = rhs._m32;
         _m33 = rhs._m33;
         _m34 = 0;

         _m41 = 0;
         _m42 = 0;
         _m43 = 0;
         _m44 = 1;

      }
      public mat4(float t0, float t1, float t2, float t3,
          float t4, float t5, float t6, float t7,
      float t8, float t9, float t10, float t11,
      float t12, float t13, float t14, float t15)
      {
         _m11 = t0;
         _m12 = t1;
         _m13 = t2;
         _m14 = t3;
         _m21 = t4;
         _m22 = t5;
         _m23 = t6;
         _m24 = t7;
         _m31 = t8;
         _m32 = t9;
         _m33 = t10;
         _m34 = t11;
         _m41 = t12;
         _m42 = t13;
         _m43 = t14;
         _m44 = t15;
      }
      public OpenTK.Matrix4 ToOpenTK()
      {
         //Note: this does not convert between row/column major. 
         //Simply allows this matrix to be used in OpenGL
         OpenTK.Matrix4 ret = new OpenTK.Matrix4(
         _m11,
         _m12,
         _m13,
         _m14,
         _m21,
         _m22,
         _m23,
         _m24,
         _m31,
         _m32,
         _m33,
         _m34,
         _m41,
         _m42,
         _m43,
         _m44
             );
         return ret;
      }
      public void set(float val, int index)
      {
         if (index == 0) { _m11 = val; }
         else if (index == 1) { _m12 = val; }
         else if (index == 2) { _m13 = val; }
         else if (index == 3) { _m14 = val; }
         else if (index == 4) { _m21 = val; }
         else if (index == 5) { _m22 = val; }
         else if (index == 6) { _m23 = val; }
         else if (index == 7) { _m24 = val; }
         else if (index == 8) { _m31 = val; }
         else if (index == 9) { _m32 = val; }
         else if (index == 10) { _m33 = val; }
         else if (index == 11) { _m34 = val; }
         else if (index == 12) { _m41 = val; }
         else if (index == 13) { _m42 = val; }
         else if (index == 14) { _m43 = val; }
         else if (index == 15) { _m44 = val; }
         else throw new Exception("Mat4 index out of range");
      }
      public static mat4 identity()
      {
         return new mat4(
             1, 0, 0, 0,
             0, 1, 0, 0,
             0, 0, 1, 0,
             0, 0, 0, 1);
      }
      public void clone(out mat4 to)
      {
         to._m11 = _m11;
         to._m12 = _m12;
         to._m13 = _m13;
         to._m14 = _m14;
         to._m21 = _m21;
         to._m22 = _m22;
         to._m23 = _m23;
         to._m24 = _m24;
         to._m31 = _m31;
         to._m32 = _m32;
         to._m33 = _m33;
         to._m34 = _m34;
         to._m41 = _m41;
         to._m42 = _m42;
         to._m43 = _m43;
         to._m44 = _m44;
      }
      public mat4 SetTranslation(float x, float y, float z)
      {
         _m41 = x;
         _m42 = y;
         _m43 = z;

         return this;
      }
      public quat toQuat()
      {
         mat3 m = new mat3(this);
         return m.toQuat();

      }
      public static mat4 getTranslation(in vec3 vTrans)
      {
         return getTranslation(vTrans.x, vTrans.y, vTrans.z);
      }
      public static mat4 getTranslation(float x, float y, float z)
      {
         mat4 m = identity();

         m._m41 = x;
         m._m42 = y;
         m._m43 = z;

         return m;
      }
      public static mat4 getRotation(in quat q)
      {
         return q.toMat4();
      }
      public static mat4 getRotation(in vec3 axis, in float angle)
      {
         var m = mat3.getRotation(axis, angle);
         mat4 Temp = new mat4(m);
         return Temp;
      }
      public static mat4 getScale(in vec3 vScale)
      {
         return getScale(vScale.x, vScale.y, vScale.z);
      }
      public static mat4 getScale(float x, float y, float z)
      {
         mat4 m = identity();
         m._m11 = x;
         m._m22 = y;
         m._m33 = z;

         return m;
      }
      public static mat4 projection(float fov_radians, float viewport_w, float viewport_h, float z_near, float z_far)
      {
         //setup a 3D projection matrix.
         //fov = field of view (radians)
         //viewport_w - width of viewport (swapchain image)
         //viewport_h - height of viewport.
         //near, far = near and far clipping planes.
         float e = (float)0.000001f;
         if (viewport_w == 0)
         {
            viewport_w = 1;
         }
         if (fov_radians > (float)Math.PI / 2.0f - e)
         {
            fov_radians = (float)Math.PI / 2.0f - e;
         }
         if (fov_radians < 1.0f + e)
         {
            fov_radians = 1.0f + e;
         }
         float vpWidth_2 = (float)Math.Tan(fov_radians * (float)0.5f) * z_near;
         float arat_1 = viewport_h / viewport_w;  // 1 / (w/h)
         float vw = vpWidth_2;
         float vh = vpWidth_2 * arat_1;

         return mat4.projection(
             z_near, z_far,
             vw, -vw,
             vh, -vh);
      }
      public static mat4 projection(float n, float f, float l, float r, float t, float b)
      {
         if (Gu.CoordinateSystem == CoordinateSystem.Lhs)
         {
            r = -r;
            l = -l;
         }

         mat4 m = new mat4();
         m._m11 = (float)(2 * n) / (r - l);
         m._m12 = (float)0;
         m._m13 = (float)0;
         m._m14 = (float)0;

         m._m21 = (float)0;
         m._m22 = (float)(2 * n) / (t - b);  // *-1.0f; // we added a neagtive here because IDK WHY this is not right
         m._m23 = (float)0;
         m._m24 = (float)0;

         m._m31 = (float)(r + l) / (r - l);
         m._m32 = (float)(t + b) / (t - b);
         m._m33 = (float)-(f + n) / (f - n);
         m._m34 = (float)-1;

         m._m41 = (float)0;
         m._m42 = (float)0;
         m._m43 = (float)-(2 * f * n) / (f - n);
         m._m44 = (float)0;

         return m;
      }
      public mat4 getOrientToVector(in vec3 iv, in vec3 iup)
      {
         mat4 m = identity();

         vec3 v = new vec3(iv);
         vec3 up = new vec3(iup);

         if (up.x + up.y + up.z != 1.0)
         {
            up = up.normalize();
         }

         vec3 s = v.cross(up);
         vec3 u = s.cross(v);

         m._m11 = s.x;
         m._m21 = u.x;
         m._m31 = -v.x;

         m._m12 = s.y;
         m._m22 = u.y;
         m._m32 = -v.y;

         m._m13 = s.z;
         m._m23 = u.z;
         m._m33 = -v.z;

         return m;
      }
      public static mat4 getLookAt(in vec3 eye, in vec3 center, in vec3 up)
      {
         vec3 zaxis = (center - eye).normalize();
         vec3 xaxis = zaxis.cross(up).normalize();//This produces -y,+z -> -x
         vec3 yaxis = xaxis.cross(zaxis);
         zaxis *= -1;

         //vec3 zaxis = (center - eye).normalize();
         //vec3 xaxis = up.cross(zaxis).normalize();//This produces -y,+z -> -x
         //vec3 yaxis = zaxis.cross(xaxis);

         mat4 mm = new mat4();
         mm._m11 = xaxis.x; mm._m12 = yaxis.x; mm._m13 = zaxis.x; mm._m14 = 0;
         mm._m21 = xaxis.y; mm._m22 = yaxis.y; mm._m23 = zaxis.y; mm._m24 = 0;
         mm._m31 = xaxis.z; mm._m32 = yaxis.z; mm._m33 = zaxis.z; mm._m34 = 0;
         mm._m41 = -xaxis.dot(eye); mm._m42 = -yaxis.dot(eye); mm._m43 = -zaxis.dot(eye); mm._m44 = 1;

         return mm;
      }
      public mat4 translate(float x, float y, float z)
      {
         this *= getTranslation(x, y, z);
         return this;
      }
      public mat4 translate(in vec3 v)
      {
         this *= getTranslation(v.x, v.y, v.z);
         return this;
      }
      public mat4 transpose()
      {
         mat4 ret = new mat4(
               _m11, _m21, _m31, _m41,
               _m12, _m22, _m32, _m42,
               _m13, _m23, _m33, _m43,
               _m14, _m24, _m34, _m44
             );
         ret.clone(out this);
         return this;
      }
      public mat4 transposed()
      {
         mat4 ret = new mat4(this);
         ret.transpose();
         return ret;
      }
      public void setIdentity()
      {
         _m11 = _m22 = _m33 = _m44 = 1.0f;
         _m21 = _m31 = _m41 = _m12 = 0.0f;
         _m32 = _m42 = _m13 = _m23 = 0.0f;
         _m43 = _m14 = _m24 = _m34 = 0.0f;
      }
      public float at(int row, int col)
      {
         return this[4 * row + col];
      }
      public int colNum(int ind)
      {
         return (int)Math.Floor((double)((double)ind / 4.0));
      }
      public int rowNum(int ind)
      {
         return ind % 4;
      }
      public static int nRows() { return 4; }
      public static int nCols() { return 4; }
      public mat3 minor(int r, int c)
      {
         //Returns the minor at the specified row and column.
         mat3 m = new mat3();
         m.setIdentity();
         if (r < 0 || r > 3 || c < 0 || c > 3)
         {
            return m;
         }

         int ind, mind = 0;
         for (ind = 0; ind < CompSize(); ++ind)
         {
            if (rowNum(ind) != r && colNum(ind) != c)
            {
               m[mind++] = this[ind];
            }
         }

         return m;
      }
      float cofactor(int r, int c)
      {
         //Returns the cofactor of this matrix at the specified row and column.
         // I.E. The determinant of the minor.
         return (float)Math.Pow(-1.00f, (float)r + (float)c) * minor(r, c).det();  // ** May be incorrect
      }
      public float det()
      {
         float a = _m11 * ((_m22 * _m33 * _m44) + (_m23 * _m34 * _m42) + (_m24 * _m32 * _m43) - (_m22 * _m34 * _m43) - (_m23 * _m32 * _m44) - (_m24 * _m33 * _m42));
         float b = -_m12 * ((_m21 * _m33 * _m44) + (_m23 * _m34 * _m41) + (_m24 * _m31 * _m43) - (_m21 * _m34 * _m43) - (_m23 * _m31 * _m44) - (_m24 * _m33 * _m41));
         float c = +_m13 * ((_m21 * _m32 * _m44) + (_m22 * _m34 * _m41) + (_m24 * _m31 * _m42) - (_m21 * _m34 * _m42) - (_m22 * _m31 * _m44) - (_m24 * _m32 * _m41));
         float d = -_m14 * ((_m21 * _m32 * _m43) + (_m22 * _m33 * _m41) + (_m23 * _m31 * _m42) - (_m21 * _m33 * _m42) - (_m22 * _m31 * _m43) - (_m23 * _m32 * _m41));
         return a + b + c + d;
      }
      public mat4 adj()
      {
         //TODO:Optimize (transpose)
         mat4 m = new mat4(
             cofactor(0, 0), cofactor(0, 1), cofactor(0, 2), cofactor(0, 3),
             cofactor(1, 0), cofactor(1, 1), cofactor(1, 2), cofactor(1, 3),
             cofactor(2, 0), cofactor(2, 1), cofactor(2, 2), cofactor(2, 3),
             cofactor(3, 0), cofactor(3, 1), cofactor(3, 2), cofactor(3, 3));
         return m;
      }
      public mat4 invert()
      {
         //**Note:
         //Transpose of an orthogonal matrix is it's inverse
         //If we're orthogonal return the transpose.
         //   return this.transposed();

         // - Convert the matrix to Reduced RE form
         mat4 m = new mat4();
         // - If the determinant is zero, return m.
         if (det() == 0)
         {
            return m;
         }
         // - Classical adjoint is favored here over Gaussian and reduced row-echlon form.
         m = adj();
         float d = m.det();
         for (int i = 0; i < 16; ++i)
         {
            m[i] /= d;
         }

         m.clone(out this);

         return this;
      }
      public mat4 inverseOf()
      {
         mat4 ret = new mat4(this);
         ret.invert();
         return ret;
      }
      public static mat4 operator +(in mat4 a, in mat4 b)
      {
         mat4 ret = new mat4();
         for (int i = 0; i < 16; ++i)
         {
            ret[i] = a[i] + b[i];
         }
         return ret;
      }
      public float this[int i]
      {
         //operator[]
         get
         {

            if (i == 0)
            {
               return this._m11;
            }
            else if (i == 1)
            {
               return this._m12;
            }
            else if (i == 2)
            {
               return this._m13;
            }
            else if (i == 3)
            {
               return this._m14;
            }
            else if (i == 4)
            {
               return this._m21;
            }
            else if (i == 5)
            {
               return this._m22;
            }
            else if (i == 6)
            {
               return this._m23;
            }
            else if (i == 7)
            {
               return this._m24;
            }
            else if (i == 8)
            {
               return this._m31;
            }
            else if (i == 9)
            {
               return this._m32;
            }
            else if (i == 10)
            {
               return this._m33;
            }
            else if (i == 11)
            {
               return this._m34;
            }
            else if (i == 12)
            {
               return this._m41;
            }
            else if (i == 13)
            {
               return this._m42;
            }
            else if (i == 14)
            {
               return this._m43;
            }
            else if (i == 15)
            {
               return _m44;
            }
            else
            {
               throw new Exception("mat4 array index out of bounds.");
            }

         }
         set
         {
            if (i == 0)
            {
               this._m11 = value;
            }
            else if (i == 1)
            {
               this._m12 = value;
            }
            else if (i == 2)
            {
               this._m13 = value;
            }
            else if (i == 3)
            {
               this._m14 = value;
            }
            else if (i == 4)
            {
               this._m21 = value;
            }
            else if (i == 5)
            {
               this._m22 = value;
            }
            else if (i == 6)
            {
               this._m23 = value;
            }
            else if (i == 7)
            {
               this._m24 = value;
            }
            else if (i == 8)
            {
               this._m31 = value;
            }
            else if (i == 9)
            {
               this._m32 = value;
            }
            else if (i == 10)
            {
               this._m33 = value;
            }
            else if (i == 11)
            {
               this._m34 = value;
            }
            else if (i == 12)
            {
               this._m41 = value;
            }
            else if (i == 13)
            {
               this._m42 = value;
            }
            else if (i == 14)
            {
               this._m43 = value;
            }
            else if (i == 15)
            {
               _m44 = value;
            }
            else
            {
               throw new Exception("mat4 array index out of bounds.");
            }

         }
      }
      public static mat4 operator *(in mat4 a, in mat4 b)
      {
         if (a == b)
         {
            return a;
         }

         mat4 tMat = new mat4();
         //|11 21 31 41|   |11 21 31 41|
         //|12 22 32 42|   |12 22 32 42|
         //|13 23 33 43| * |13 23 33 43|
         //|14 24 34 44|   |14 24 34 44|
         //64 mul
         //48 add
         tMat._m11 = (a._m11 * b._m11) + (a._m12 * b._m21) + (a._m13 * b._m31) + (a._m14 * b._m41);
         tMat._m21 = (a._m21 * b._m11) + (a._m22 * b._m21) + (a._m23 * b._m31) + (a._m24 * b._m41);
         tMat._m31 = (a._m31 * b._m11) + (a._m32 * b._m21) + (a._m33 * b._m31) + (a._m34 * b._m41);
         tMat._m41 = (a._m41 * b._m11) + (a._m42 * b._m21) + (a._m43 * b._m31) + (a._m44 * b._m41);

         tMat._m12 = (a._m11 * b._m12) + (a._m12 * b._m22) + (a._m13 * b._m32) + (a._m14 * b._m42);
         tMat._m22 = (a._m21 * b._m12) + (a._m22 * b._m22) + (a._m23 * b._m32) + (a._m24 * b._m42);
         tMat._m32 = (a._m31 * b._m12) + (a._m32 * b._m22) + (a._m33 * b._m32) + (a._m34 * b._m42);
         tMat._m42 = (a._m41 * b._m12) + (a._m42 * b._m22) + (a._m43 * b._m32) + (a._m44 * b._m42);

         tMat._m13 = (a._m11 * b._m13) + (a._m12 * b._m23) + (a._m13 * b._m33) + (a._m14 * b._m43);
         tMat._m23 = (a._m21 * b._m13) + (a._m22 * b._m23) + (a._m23 * b._m33) + (a._m24 * b._m43);
         tMat._m33 = (a._m31 * b._m13) + (a._m32 * b._m23) + (a._m33 * b._m33) + (a._m34 * b._m43);
         tMat._m43 = (a._m41 * b._m13) + (a._m42 * b._m23) + (a._m43 * b._m33) + (a._m44 * b._m43);

         tMat._m14 = (a._m11 * b._m14) + (a._m12 * b._m24) + (a._m13 * b._m34) + (a._m14 * b._m44);
         tMat._m24 = (a._m21 * b._m14) + (a._m22 * b._m24) + (a._m23 * b._m34) + (a._m24 * b._m44);
         tMat._m34 = (a._m31 * b._m14) + (a._m32 * b._m24) + (a._m33 * b._m34) + (a._m34 * b._m44);
         tMat._m44 = (a._m41 * b._m14) + (a._m42 * b._m24) + (a._m43 * b._m34) + (a._m44 * b._m44);

         return tMat;
      }
      public static vec4 operator *(in mat4 m, in vec4 v)
      {
         vec4 vret = new vec4(
                 m._m11 * v.x + m._m21 * v.y + m._m31 * v.z + m._m41 * v.w,
                 m._m12 * v.x + m._m22 * v.y + m._m32 * v.z + m._m42 * v.w,
                 m._m13 * v.x + m._m23 * v.y + m._m33 * v.z + m._m43 * v.w,
                 m._m14 * v.x + m._m24 * v.y + m._m34 * v.z + m._m44 * v.w);
         return vret;
      }
      public static bool operator ==(in mat4 lhs, in mat4 rhs)
      {
         for (int i = 0; i < 16; ++i)
         {
            if (lhs[i] != rhs[i])
            {
               return false;
            }
         }
         return true;
      }
      public static bool operator !=(in mat4 lhs, in mat4 rhs)
      {
         return !(lhs == rhs);
      }
      public override int GetHashCode()
      {
         return base.GetHashCode();
      }
      public override bool Equals(object obj)
      {
         return base.Equals(obj);
      }
      public vec3 extractTranslation()
      {
         vec3 ret = new vec3();
         ret.x = _m41;
         ret.y = _m42;
         ret.z = _m43;
         return ret;
      }
      public mat4 getOrtho(float left, float right, float top, float bottom, float neard, float fard)
      {
         mat4 mm = new mat4();

         float a1 = (float)2.0 / (right - left);
         float a2 = (float)2.0 / (top - bottom);   //IDK WY
         float a3 = (float)-2.0 / (fard - neard);  //IDK WY
         float t1 = (right + left) / (right - left) * (float)-1.0;
         float t2 = (top + bottom) / (top - bottom) * (float)-1.0;
         float t3 = (fard + neard) / (fard - neard) * (float)-1.0;

         //Row major order version
         //mm._m11 =a1, mm._m12 = 0, mm._m13 = 0, mm._m14 =t1,
         //mm._m21 = 0, mm._m22 =a2, mm._m23 = 0, mm._m24 =t2,
         //mm._m31 = 0, mm._m32 = 0, mm._m33 =a3, mm._m34 =t3,
         //mm._m41 = 0, mm._m42 = 0, mm._m43 = 0, mm._m44 = 1;

         // ** OpenGL version - the transpose of the former.
         mm._m11 = a1; mm._m12 = 0; mm._m13 = 0; mm._m14 = 0;
         mm._m21 = 0; mm._m22 = a2; mm._m23 = 0; mm._m24 = 0;
         mm._m31 = 0; mm._m32 = 0; mm._m33 = a3; mm._m34 = 0;
         mm._m41 = t1; mm._m42 = t2; mm._m43 = t3; mm._m44 = 1;

         return mm;
      }
      public void decompose(out vec4 pos, out mat4 rot, out vec4 scale)
      {
         //http://math.stackexchange.com/questions/237369/given-this-transformation-matrix-how-do-i-decompose-it-into-translation-rotati
         //11  21  31  41 << Don't use gl order
         //12  22  32  42
         //13  23  33  43
         //14  24  34  44
         pos = new vec4();
         rot = new mat4();
         scale = new vec4();

         pos = new vec4(extractTranslation(), 0);

         scale.x = new vec3(_m11, _m21, _m31).length();
         scale.y = new vec3(_m12, _m22, _m32).length();
         scale.z = new vec3(_m13, _m23, _m33).length();

         rot.setIdentity();
         rot._m11 = _m11 / scale.x;
         rot._m21 = _m21 / scale.x;
         rot._m31 = _m31 / scale.x;

         rot._m12 = _m12 / scale.y;
         rot._m22 = _m22 / scale.y;
         rot._m32 = _m32 / scale.y;

         rot._m13 = _m13 / scale.z;
         rot._m23 = _m23 / scale.z;
         rot._m33 = _m33 / scale.z;
      }
      public void decompose(out vec4 pos, out vec4 rot, out vec4 scale, bool bDegreeRotation)
      {
         pos = new vec4();
         rot = new vec4();
         scale = new vec4();

         mat4 mOut = new mat4();
         decompose(out pos, out mOut, out scale);

         quat q = mOut.toQuat();

         rot = q.toAxisAngle();
         if (bDegreeRotation)
         {
            rot.w = MathUtils.ToDegrees(rot.w);
         }
      }

      // bool parse(std::string tok, mat4 mOut) {
      //   // - Parse csv matrix string.

      //   size_t n = 0;
      //   char c;
      //   float mat[16];
      //   std::string val = "";
      //   int mat_ind = 0;

      //   while (n < tok.length()) {
      //     c = tok[n++];
      //     if (c == ',' || c == '\n' || n == tok.length()) {
      //       mat[mat_ind++] = TypeConv::strToFloat(val);
      //       val = "";
      //     }
      //     else if (isalnum(c) || c == '-' || c == '.' || c == '+' || c == 'e') {
      //       val += c;
      //     }
      //   }

      //   mOut = mat;

      //   return true;
      // }


   }
   [StructLayout(LayoutKind.Sequential)]
   public struct quat
   {
      public float x, y, z, w;

      public quat(float dx, float dy, float dz, float dw)
      {
         x = dx;
         y = dy;
         z = dz;
         w = dw;
      }
      public void construct(float dx, float dy, float dz, float dw)
      {
         x = dx;
         y = dy;
         z = dz;
         w = dw;
      }
      public float dot(in quat rhs)
      {
         return (x * rhs.x + y * rhs.y + z * rhs.z + w * rhs.w);
      }
      public float mag()
      {
         float ret = (float)Math.Sqrt(w * w + x * x + y * y + z * z);
         return ret;
      }
      public quat normalized()
      {
         quat ret;
         float len = mag();
         if (len != 0)
         {

            ret = new quat(
               this.x / len,
               this.y / len,
               this.z / len,
               this.w / len
               );
         }
         else
         {
            ret = quat.identity();
         }
         return ret;
      }
      public quat inverse()
      {
         float L = 1 / mag();
         quat outv;
         outv.w = L * w;
         outv.x = -L * x;
         outv.y = -L * y;
         outv.z = -L * z;
         return outv;
      }
      public static quat operator *(in quat lhs, in quat rhs)
      {

         // This is a more intuitive quat multiplication. It's the same result as below (tested).
         //quat ret2;
         //vec3 pv = new vec3(lhs.x, lhs.y,lhs.z);
         //vec3 qv = new vec3(rhs.x, rhs.y, rhs.z);
         //float ps = lhs.w;
         //float qs = rhs.w;
         //vec3 pqv = qv * ps + pv * qs + pv.cross(qv);
         //float pqs = ps * qs - pv.dot(qv);
         //ret2 = new quat(pqv.x, pqv.y, pqv.z, pqs);


         quat ret;
         //Normal quaternion multiplication
         //Q(w,v) = w=w1w2 - v1 dot v2, v= w1v2 + w2v1 + v2 cross v1.

         // - This method here is that of backwards multiplication
         // used in the 3D Math Primer For Graphics and Game Development:
         //    w=w1w2 - v1 dot v2, v= w1v2 + w2v1 + v1 cross v2.
         // this allows the quaternion rotations to be concatenated in a left to right order.
         ret.x = lhs.w * rhs.x + rhs.w * lhs.x + (lhs.y * rhs.z) - (rhs.y * lhs.z);
         ret.y = lhs.w * rhs.y + rhs.w * lhs.y + (lhs.z * rhs.x) - (rhs.z * lhs.x);
         ret.z = lhs.w * rhs.z + rhs.w * lhs.z + (lhs.x * rhs.y) - (rhs.x * lhs.y);
         ret.w = lhs.w * rhs.w - (lhs.x * rhs.x + lhs.y * rhs.y + lhs.z * rhs.z);
         return ret;
      }
      public static quat operator *(in quat q, float f)
      {
         quat outv;
         outv.x = q.x * f;
         outv.y = q.y * f;
         outv.z = q.z * f;
         outv.w = q.w * f;
         return outv;
      }
      public static quat operator *(in quat q, double f)
      {
         quat outv;
         outv.x = q.x * (float)f;
         outv.y = q.y * (float)f;
         outv.z = q.z * (float)f;
         outv.w = q.w * (float)f;
         return outv;
      }
      public static quat operator +(in quat lhs, in quat rhs)
      {
         quat outv;
         outv.x = lhs.x + rhs.x;
         outv.y = lhs.y + rhs.y;
         outv.z = lhs.z + rhs.z;
         outv.w = lhs.w + rhs.w;
         return outv;
      }
      public quat lerpTo(in quat rhs, float t)
      {
         quat ret = new quat(
            x * (1 - t) + t * rhs.x,
            y * (1 - t) + t * rhs.y,
            z * (1 - t) + t * rhs.z,
            w * (1 - t) + t * rhs.w
            ).normalized();
         return ret;
      }
      public quat slerpTo(in quat rhs, float t)
      {
         //SLERP Spherical Linear interpolate this quaternion to rhs.
         // @param rhs The Quat to slerp
         // @param t Interpolation value [0 to 1]
         quat ret;

         double theta = Math.Acos(this.dot(rhs));
         double sintheta = Math.Sin(theta);
         double wp = (Math.Sin(1 - t) * theta) / sintheta;
         double wq = (Math.Sin(t) * theta) / sintheta;

         ret = (this) * wp + rhs * wq;
         ret = ret.normalized();
         return ret;
      }
      public static quat operator *(in quat q, in vec3 v)
      {
         return new quat(q.x * v.x, q.y * v.y, q.z * v.z, q.w);
      }
      public vec3 vectorPart()
      {
         return new vec3(x, y, z);
      }
      public vec3 rotatePoint(in vec3 vin)
      {
         return ((inverse() * vin) * this).vectorPart();
      }
      public static quat fromAxisAngle(in vec3 axis, float angle)
      {
         //**NOTE: Axis must be normalized.
         quat q = new quat();
         float sina2 = MathUtils.sinf(angle * 0.5f);
         q.x = axis.x * sina2;
         q.y = axis.y * sina2;
         q.z = axis.z * sina2;
         q.w = MathUtils.cosf(angle * 0.5f);

         return q;
      }
      public vec4 toAxisAngle()
      {
         //http://www.euclideanspace.com/maths/geometry/rotations/conversions/quaternionToAngle/
         vec4 v = new vec4();

         if (w == 1.0f)
         {
            //Avoid divide by 0,( 1 - (cos(0) = 1)) =0
            v.x = v.z = v.w = 0;
            v.y = 1;
            return v;
         }

         v.w = 2.0f * (float)Math.Acos(w);

         float w2 = w * w;
         float srw2_1 = 1.0f / (float)Math.Sqrt(1.0f - w2);
         v.x = x * srw2_1;
         v.y = y * srw2_1;
         v.z = z * srw2_1;
         return v;
      }
      public mat3 toMat3()
      {
         mat3 m3 = new mat3();
         m3._m11 = 1 - 2 * y * y - 2 * z * z;
         m3._m12 = 2 * x * y + 2 * w * z;
         m3._m13 = 2 * x * z - 2 * w * y;
         m3._m21 = 2 * x * y - 2 * w * z;
         m3._m22 = 1 - 2 * x * x - 2 * z * z;
         m3._m23 = 2 * y * z + 2 * w * x;
         m3._m31 = 2 * x * z + 2 * w * y;
         m3._m32 = 2 * y * z - 2 * w * x;
         m3._m33 = 1 - 2 * x * x - 2 * y * y;
         return m3;
      }
      public mat4 toMat4()
      {
         mat4 m = new mat4(toMat3());
         return m;
      }
      public static quat identity()
      {
         return new quat(0, 0, 0, 1);
      }
   }
   [StructLayout(LayoutKind.Sequential)]
   public struct Box2f
   {
      public vec2 Min;
      public vec2 Max;

      public float Width() { return Max.x - Min.x; }
      public float Height() { return Max.y - Min.y; }

      public vec2 TopRight() { return new vec2(Max.x, Min.y); }
      public vec2 BotRight() { return new vec2(Max.x, Max.y); }
      public vec2 BotLeft() { return new vec2(Min.x, Max.y); }
      public vec2 TopLeft() { return new vec2(Min.x, Min.y); }

      public void Construct(vec2 min, vec2 max)
      {
         Min = min; Max = max;
      }
      public Box2f(float x, float y, float w, float h)
      {
         Min = new vec2(x, y);
         Max = new vec2(w, h) + Min;
      }
      public Box2f(vec2 min, vec2 max)
      {
         Min = min;
         Max = max;
      }
      public vec2 Center()
      {
         return Min + (Max - Min) * 0.5f;
      }
      public static Box2f FlipBoxH(Box2f b, float w)
      {
         //Flip the box inside of a larger box (w)
         Box2f ret = new Box2f();
         ret.Min.x = w - b.Max.x;
         ret.Max.x = w - b.Min.x;

         ret.Min.y = b.Min.y;
         ret.Max.y = b.Max.y;
         return ret;
      }
      public static Box2f FlipBoxV(Box2f b, float h)
      {
         //Flip the box inside of a larger box (h)
         Box2f ret = new Box2f();
         ret.Min.y = h - b.Max.y;
         ret.Max.y = h - b.Min.y;

         ret.Min.x = b.Min.x;
         ret.Max.x = b.Max.x;
         return ret;
      }
      public Rectangle ToXNARect()
      {
         Rectangle r = new Rectangle();

         r.X = (int)(Min.x);
         r.Y = (int)(Min.y);
         r.Width = (int)(Max.x - Min.x);
         r.Height = (int)(Max.y - Min.y);

         return r;
      }

      public static Box2f GetIntersectionBox_Inclusive(Box2f a, Box2f b)
      {
         Box2f ret = new Box2f();

         ret.Min.x = Single.MaxValue;
         ret.Min.y = Single.MaxValue;
         ret.Max.x = -Single.MaxValue;
         ret.Max.y = -Single.MaxValue;


         if (a.Min.x >= b.Min.x && a.Min.x <= b.Max.x)
         {
            ret.Min.x = Math.Min(ret.Min.x, a.Min.x);
         }
         if (a.Max.x <= b.Max.x && a.Max.x >= b.Min.x)
         {
            ret.Max.x = Math.Max(ret.Max.x, a.Max.x);
         }
         if (a.Min.y >= b.Min.y && a.Min.y <= b.Max.y)
         {
            ret.Min.y = Math.Min(ret.Min.y, a.Min.y);
         }
         if (a.Max.y <= b.Max.y && a.Max.y >= b.Min.y)
         {
            ret.Max.y = Math.Max(ret.Max.y, a.Max.y);
         }

         if (b.Min.x >= a.Min.x && b.Min.x <= a.Max.x)
         {
            ret.Min.x = Math.Min(ret.Min.x, b.Min.x);
         }
         if (b.Max.x <= a.Max.x && b.Max.x >= a.Min.x)
         {
            ret.Max.x = Math.Max(ret.Max.x, b.Max.x);
         }
         if (b.Min.y >= a.Min.y && b.Min.y <= a.Max.y)
         {
            ret.Min.y = Math.Min(ret.Min.y, b.Min.y);
         }
         if (b.Max.y <= a.Max.y && b.Max.y >= a.Min.y)
         {
            ret.Max.y = Math.Max(ret.Max.y, b.Max.y);
         }
         return ret;
      }

      public void GenResetExtents()
      {
         Min = new vec2(Single.MaxValue, Single.MaxValue);
         Max = new vec2(-Single.MaxValue, -Single.MaxValue);
      }
      public void ExpandByPoint(vec2 v)
      {
         Min = vec2.Minv(Min, v);// Vec2f.ComponentMin(Min, v);// Vec2f.Minv(Min, v);
         Max = vec2.Maxv(Max, v); //Vec2f.ComponentMax(Max, v);// Vec2f.Maxv(Max, v);
      }
      public bool BoxIntersect_EasyOut_Inclusive(Box2f cc)
      {
         return cc.Min.x <= Max.x && cc.Min.y <= Max.y && Min.x <= cc.Max.x && Min.y <= cc.Max.y;
      }
      public bool ContainsPointInclusive(vec2 point)
      {
         if (point.x < Min.x)
         {
            return false;
         }
         if (point.y < Min.y)
         {
            return false;
         }
         if (point.x > Max.x)
         {
            return false;
         }
         if (point.y > Max.y)
         {
            return false;
         }
         return true;
      }
      private vec2 bounds(int x)
      {
         if (x == 0)
         {
            return Min;
         }
         return Max;
      }
      public bool RayIntersect(PickRay2D ray, ref RaycastHit bh)
      {
         float txmin, txmax, tymin, tymax;
         bool bHit;

         txmin = (bounds(ray.Sign[0]).x - ray.Origin.x) * ray.InvDir.x;
         txmax = (bounds(1 - ray.Sign[0]).x - ray.Origin.x) * ray.InvDir.x;

         tymin = (bounds(ray.Sign[1]).y - ray.Origin.y) * ray.InvDir.y;
         tymax = (bounds(1 - ray.Sign[1]).y - ray.Origin.y) * ray.InvDir.y;

         if ((txmin > tymax) || (tymin > txmax))
         {
            // if (bh != null)
            // {
            bh._bHit = false;
            // }
            return false;
         }
         if (tymin > txmin)
         {
            txmin = tymin;
         }
         if (tymax < txmax)
         {
            txmax = tymax;
         }

         bHit = ((txmin >= 0.0f) && (txmax <= ray.Length));

         //**Note changed 20151105 - this is not [0,1] this is the lenth along the line in which 
         // the ray enters and exits the cube, so any value less than the maximum is valid

         // if (bh != null)
         // {
         bh._bHit = bHit;
         bh._t = txmin;
         // }

         return bHit;
      }
   }
   [StructLayout(LayoutKind.Sequential)]
   public struct Box2i
   {
      public ivec2 _min;
      public ivec2 _max;
      public Box2i(in ivec2 min, in ivec2 max)
      {
         _min = min;
         _max = max;
      }
      public int left() { return _min.x; }
      public int top() { return _min.y; }
      public int right() { return _max.x; }
      public int bottom() { return _max.y; }

      public int Width()
      {
         return _max.x - _min.x;
      }
      public int Height()
      {
         return _max.y - _min.y;
      }
      public void Construct(int minx, int miny, int maxx, int maxy)
      {
         _min.x = minx;
         _min.y = miny;
         _max.x = maxx;
         _max.y = maxy;
      }
   }
   [StructLayout(LayoutKind.Sequential)]
   public struct Box3i
   {
      public ivec3 _min;
      public ivec3 _max;
      public Box3i(in ivec3 min, in ivec3 max)
      {
         _min = min;
         _max = max;
      }
   }
   [StructLayout(LayoutKind.Sequential)]
   public struct Box3f
   {
      public vec3 _min;
      public vec3 _max;
      public Box3f(in vec3 min, in vec3 max)
      {
         _min = min;
         _max = max;
      }
      public vec3 center()
      {
         vec3 v = _min + (_max - _min) * 0.5f;
         return v;
      }
      public float Height()
      {
         return _max.y - _min.y;
      }
      public float Width()
      {
         return _max.x - _min.x;
      }
      public float Depth()
      {
         return _max.z - _min.z;
      }
      public void Validate()
      {
         if (_max.x < _min.x)
         {
            throw new Exception("Bound box X was invalid.");
         }
         if (_max.y < _min.y)
         {
            throw new Exception("Bound box Y was invalid.");
         }
         if (_max.z < _min.z)
         {
            throw new Exception("Bound box Z was invalid.");
         }
      }
      /**
      *   @fn RayIntersect
      *   @brief Returns true if the given ray intersects this Axis aligned
      *   cube volume.
      *   @param bh - Reference to a BoxHit structure.
      *   @prarm ray - The ray to test against the box.
      *   @return true if ray intersects, false otherwise.
      */
      public bool LineOrRayIntersectInclusive_EasyOut(in PickRay3D ray, ref BoxAAHit bh)
      {
         if (RayIntersect(ray, ref bh))
            return true;
         // - otherwise check for points contained.
         if (containsInclusive(ray.Origin))
         {
            bh._p1Contained = true;
            bh._bHit = true;
            return true;
         }

         if (containsInclusive(ray.Origin + ray.Dir))
         {
            bh._p2Contained = true;
            bh._bHit = true;
            return true;
         }

         return false;
      }
      private vec3 bounds(int in__)
      {
         if (in__ == 0)
         {
            return _min;
         }
         else
         {
            return _max;
         }
      }
      private bool RayIntersect(PickRay3D ray, ref BoxAAHit bh)
      {
         float txmin, txmax, tymin, tymax, tzmin, tzmax;

         txmin = (bounds(ray.Sign[0]).x - ray.Origin.x) * ray.InvDir.x;
         txmax = (bounds(1 - ray.Sign[0]).x - ray.Origin.x) * ray.InvDir.x;

         tymin = (bounds(ray.Sign[1]).y - ray.Origin.y) * ray.InvDir.y;
         tymax = (bounds(1 - ray.Sign[1]).y - ray.Origin.y) * ray.InvDir.y;

         if ((txmin > tymax) || (tymin > txmax))
         {
            bh._bHit = false;
            return false;
         }
         if (tymin > txmin)
            txmin = tymin;
         if (tymax < txmax)
            txmax = tymax;

         tzmin = (bounds(ray.Sign[2]).z - ray.Origin.z) * ray.InvDir.z;
         tzmax = (bounds(1 - ray.Sign[2]).z - ray.Origin.z) * ray.InvDir.z;

         if ((txmin > tzmax) || (tzmin > txmax))
         {
            bh._bHit = false;
            return false;
         }
         if (tzmin > txmin)
            txmin = tzmin;
         if (tzmax < txmax)
            txmax = tzmax;

         bh._bHit = ((txmin > 0.0f) && (txmax <= ray.Length));
         bh._t = txmin;

         return bh._bHit;
      }

      public bool containsInclusive(vec3 v)
      {
         return (
             (v.x >= _min.x) && (v.x <= _max.x) &&
             (v.y >= _min.y) && (v.y <= _max.y) &&
             (v.z >= _min.z) && (v.z <= _max.z)
             );
      }
      public bool containsBottomLeftInclusive(in vec3 v)
      {
         return (
           (v.x >= _min.x) && (v.x < _max.x) &&
           (v.y >= _min.y) && (v.y < _max.y) &&
           (v.z >= _min.z) && (v.z < _max.z)
           );
      }
      public void genResetLimits()
      {
         _min = new vec3(float.MaxValue, float.MaxValue, float.MaxValue);
         _max = new vec3(float.MinValue, float.MinValue, float.MinValue);
      }
      public void genExpandByPoint(in vec3 pt)
      {
         _min = vec3.minv(_min, pt);// Vec3f.ComponentMin(_min, pt);
         _max = vec3.maxv(_max, pt);// Vec3f.ComponentMax(_max, pt);
      }
      public void genExpandByBox(in Box3f pc)
      {
         genExpandByPoint(pc._min);
         genExpandByPoint(pc._max);
      }
      public bool getHasVolume(float epsilon)
      {
         if (getVolumePositiveOnly() == 0.0)
         {
            return false;
         }
         return true;
      }
      private float getVolumePositiveOnly()
      {
         float ax = (_max.x - _min.x);
         float ay = (_max.y - _min.y);
         float az = (_max.z - _min.z);
         if (ax < 0.0f) ax = 0.0f;
         if (ay < 0.0f) ay = 0.0f;
         if (az < 0.0f) az = 0.0f;
         return ax * ay * az;
      }
      private float getVolumeArbitrary()
      {
         return (_max.x - _min.x) * (_max.y - _min.y) * (_max.z - _min.z);
      }
   }
   public static class BinaryWriterExtensions
   {
      public static void Write(this System.IO.BinaryWriter writer, vec2 v)
      {
         writer.Write((float)v.x);
         writer.Write((float)v.y);
      }
      public static void Write(this System.IO.BinaryWriter writer, vec3 v)
      {
         writer.Write((float)v.x);
         writer.Write((float)v.y);
         writer.Write((float)v.z);
      }
      public static void Write(this System.IO.BinaryWriter writer, vec4 v)
      {
         writer.Write((float)v.x);
         writer.Write((float)v.y);
         writer.Write((float)v.z);
         writer.Write((float)v.w);
      }
      public static void Write(this System.IO.BinaryWriter writer, ivec2 v)
      {
         writer.Write((Int32)v.x);
         writer.Write((Int32)v.y);
      }
      public static void Write(this System.IO.BinaryWriter writer, ivec3 v)
      {
         writer.Write((Int32)v.x);
         writer.Write((Int32)v.y);
         writer.Write((Int32)v.z);
      }
      public static void Write(this System.IO.BinaryWriter writer, ivec4 v)
      {
         writer.Write((Int32)v.x);
         writer.Write((Int32)v.y);
         writer.Write((Int32)v.z);
         writer.Write((Int32)v.w);
      }
      public static void Write(this System.IO.BinaryWriter writer, mat3 v)
      {
         writer.Write((float)v._m11);
         writer.Write((float)v._m12);
         writer.Write((float)v._m13);

         writer.Write((float)v._m21);
         writer.Write((float)v._m22);
         writer.Write((float)v._m23);

         writer.Write((float)v._m31);
         writer.Write((float)v._m32);
         writer.Write((float)v._m33);
      }
      public static void Write(this System.IO.BinaryWriter writer, mat4 v)
      {
         writer.Write((float)v._m11);
         writer.Write((float)v._m12);
         writer.Write((float)v._m13);
         writer.Write((float)v._m14);

         writer.Write((float)v._m21);
         writer.Write((float)v._m22);
         writer.Write((float)v._m23);
         writer.Write((float)v._m24);

         writer.Write((float)v._m31);
         writer.Write((float)v._m32);
         writer.Write((float)v._m33);
         writer.Write((float)v._m34);

         writer.Write((float)v._m41);
         writer.Write((float)v._m42);
         writer.Write((float)v._m43);
         writer.Write((float)v._m44);
      }
      public static void Write(this System.IO.BinaryWriter writer, quat v)
      {
         writer.Write((float)v.x);
         writer.Write((float)v.y);
         writer.Write((float)v.z);
         writer.Write((float)v.w);
      }
      public static void Write(this System.IO.BinaryWriter writer, Box3f box)
      {
         writer.Write((vec3)box._min);
         writer.Write((vec3)box._max);
      }
      public static void Write(this System.IO.BinaryWriter writer, Box2f box)
      {
         writer.Write((vec2)box.Min);
         writer.Write((vec2)box.Max);
      }

      public static vec2 ReadVec2(this System.IO.BinaryReader reader)
      {
         vec2 ret = new vec2();
         ret.x = reader.ReadSingle();
         ret.y = reader.ReadSingle();
         return ret;
      }
      public static vec3 ReadVec3(this System.IO.BinaryReader reader)
      {
         vec3 ret = new vec3();
         ret.x = reader.ReadSingle();
         ret.y = reader.ReadSingle();
         ret.z = reader.ReadSingle();
         return ret;
      }
      public static vec4 ReadVec4(this System.IO.BinaryReader reader)
      {
         vec4 v = new vec4();
         v.x = reader.ReadSingle();
         v.y = reader.ReadSingle();
         v.z = reader.ReadSingle();
         v.w = reader.ReadSingle();
         return v;
      }
      public static ivec2 ReadIVec2(this System.IO.BinaryReader reader)
      {
         ivec2 v = new ivec2();
         v.x = reader.ReadInt32();
         v.y = reader.ReadInt32();
         return v;
      }
      public static ivec3 ReadIVec3(this System.IO.BinaryReader reader)
      {
         ivec3 v = new ivec3();
         v.x = reader.ReadInt32();
         v.y = reader.ReadInt32();
         v.z = reader.ReadInt32();
         return v;
      }
      public static ivec4 ReadIVec4(this System.IO.BinaryReader reader)
      {
         ivec4 v = new ivec4();
         v.x = reader.ReadInt32();
         v.y = reader.ReadInt32();
         v.z = reader.ReadInt32();
         v.w = reader.ReadInt32();
         return v;
      }
      public static mat3 ReadMat3(this System.IO.BinaryReader reader)
      {
         mat3 ret = new mat3();
         ret._m11 = reader.ReadSingle();
         ret._m12 = reader.ReadSingle();
         ret._m13 = reader.ReadSingle();
         ret._m21 = reader.ReadSingle();
         ret._m22 = reader.ReadSingle();
         ret._m23 = reader.ReadSingle();
         ret._m31 = reader.ReadSingle();
         ret._m32 = reader.ReadSingle();
         ret._m33 = reader.ReadSingle();
         return ret;
      }
      public static mat4 ReadMat4(this System.IO.BinaryReader reader)
      {
         mat4 ret = new mat4();
         ret._m11 = reader.ReadSingle();
         ret._m12 = reader.ReadSingle();
         ret._m13 = reader.ReadSingle();
         ret._m14 = reader.ReadSingle();
         ret._m21 = reader.ReadSingle();
         ret._m22 = reader.ReadSingle();
         ret._m23 = reader.ReadSingle();
         ret._m24 = reader.ReadSingle();
         ret._m31 = reader.ReadSingle();
         ret._m32 = reader.ReadSingle();
         ret._m33 = reader.ReadSingle();
         ret._m34 = reader.ReadSingle();
         ret._m41 = reader.ReadSingle();
         ret._m42 = reader.ReadSingle();
         ret._m43 = reader.ReadSingle();
         ret._m44 = reader.ReadSingle();
         return ret;
      }
      public static quat ReadQuat(this System.IO.BinaryReader reader)
      {
         quat v = new quat();
         v.x = reader.ReadSingle();
         v.y = reader.ReadSingle();
         v.z = reader.ReadSingle();
         v.w = reader.ReadSingle();
         return v;
      }
      public static Box2f ReadBox2f(this System.IO.BinaryReader reader)
      {
         Box2f box = new Box2f();
         box.Min = reader.ReadVec2();
         box.Max = reader.ReadVec2();
         return box;
      }
      public static Box3f ReadBox3f(this System.IO.BinaryReader reader)
      {
         Box3f box = new Box3f();
         box._min = reader.ReadVec3();
         box._max = reader.ReadVec3();
         return box;
      }

   }
}