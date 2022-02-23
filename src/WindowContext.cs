﻿using OpenTK.Windowing.Desktop;
using System;

namespace PirateCraft
{
   //Graphics Contxt + Window Frame Sync
   public class WindowContext
   {
      private long _lastTime = Gu.Nanoseconds();

      public Gpu Gpu { get; private set; } = null;
      public GameWindow GameWindow { get; set; } = null;
      public PCKeyboard PCKeyboard = new PCKeyboard();
      public PCMouse PCMouse = new PCMouse();
      public Int64 FrameStamp { get; private set; }
      public double UpTime { get; private set; } = 0; //Time since engine started.
      private DateTime _startTime = DateTime.Now;
      public double Fps { get; private set; } = 60;
      public double Delta { get; private set; } = 1 / 60;
      public Renderer Renderer { get; private set; } = null;

      public WindowContext(GameWindow g)
      {
         GameWindow = g;
         Gpu = new Gpu();
         Renderer = new Renderer(); 
      }
      
      public void Update()
      {
         //For first frame run at a smooth time.
         long curTime = Gu.Nanoseconds();
         if (FrameStamp > 0)
         {
            Delta = (double)((decimal)(curTime - _lastTime) / (decimal)(1000000000));
         }
         _lastTime = curTime;
         FrameStamp++;

         Fps = 1 / Delta;

         UpTime = (DateTime.Now - _startTime).TotalSeconds; 

         PCKeyboard.Update();
         PCMouse.Update();
      }
   }
}
