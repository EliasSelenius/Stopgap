﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Input;

using Nums;
using System.Diagnostics;
using System.Data.SqlTypes;

namespace Stopgap {
    public static class Input {
        public static vec2 Wasd => new vec2(KeyAxis(Key.D, Key.A), KeyAxis(Key.W, Key.S));

        //public static vec2 MouseDelta => MousePos - (IsFixedMouse ? screenCenter : PrevMousePos);
        public static vec2 MouseDelta { get; private set; }
        public static vec2 MousePos { get; private set; }
        public static vec2 MousePos_ndc {
            get {
                var p = new vec2(MousePos.x / Game.window.Width, MousePos.y / Game.window.Height) * 2f - vec2.one;
                p.y = -p.y;
                return p;
            }
        }

        
        public static vec2 PrevMousePos { get; private set; }

        public static float MouseWheelDelta { get; private set; }

        private static KeyboardState keyboard;
        private static MouseState mouse;

        public static bool IsFixedMouse => fixedmouse && Game.window.Focused;
        private static bool fixedmouse;

        private static ivec2 screenCenter => new ivec2(Game.window.Width / 2, Game.window.Height / 2);
        //private static vec2 screenCenter => new vec2(Game.window.X + (Game.window.Width / 2f), Game.window.Y + (Game.window.Height / 2f));

        public static bool LeftMousePressed { get; private set; }
        public static bool RightMousePressed { get; private set; }
        public static bool MiddelMousePressed { get; private set; }

        private static readonly Dictionary<Key, bool> pressedStates = new Dictionary<Key, bool>();

        internal static void InitEvents() {
            Game.window.MouseMove += Window_MouseMove;
            Game.window.MouseWheel += Window_MouseWheel;
            Game.window.MouseDown += Window_MouseDown;

            Game.window.KeyDown += Window_KeyDown;

            FixedMouse(false);
        }

        private static void Window_KeyDown(object sender, KeyboardKeyEventArgs e) {
            pressedStates[e.Key] = true;
        }

        private static void Window_MouseDown(object sender, MouseButtonEventArgs e) {
            switch (e.Button) {
                case MouseButton.Left:
                    LeftMousePressed = true;
                    break;
                case MouseButton.Middle:
                    MiddelMousePressed = true;
                    break;
                case MouseButton.Right:
                    RightMousePressed = true;
                    break;
                case MouseButton.Button1:
                    break;
                case MouseButton.Button2:
                    break;
                case MouseButton.Button3:
                    break;
                case MouseButton.Button4:
                    break;
                case MouseButton.Button5:
                    break;
                case MouseButton.Button6:
                    break;
                case MouseButton.Button7:
                    break;
                case MouseButton.Button8:
                    break;
                case MouseButton.Button9:
                    break;
                case MouseButton.LastButton:
                    break;
                default:
                    break;
            }
        }

        private static void Window_MouseWheel(object sender, MouseWheelEventArgs e) {
            MouseWheelDelta = e.DeltaPrecise;
        }

        public static void FixedMouse(bool v) {
            fixedmouse = v;
            ensureMousePos();
            //Game.window.CursorVisible = !fixedmouse;
        }

        public static void hideMouse(bool b) {
            Game.window.CursorVisible = !b;
        }

        private static void Window_MouseMove(object sender, MouseMoveEventArgs e) {
            PrevMousePos = MousePos;
            MousePos = new vec2(e.X, e.Y);
            MouseDelta = MousePos - (IsFixedMouse ? screenCenter : PrevMousePos);
        }

        private static void ensureMousePos() {
            if (IsFixedMouse) setMousePos(screenCenter);
        }

        public static void setMousePos(ivec2 pos) {
            var p = Game.window.PointToScreen(new System.Drawing.Point(pos.x, pos.y));
            Mouse.SetPosition(p.X, p.Y);
        }

        internal static void Update() {
            keyboard = Keyboard.GetState();
            mouse = Mouse.GetState();
            ensureMousePos();
        }

        internal static void reset() {
            MouseWheelDelta = 0;
            LeftMousePressed = RightMousePressed = MiddelMousePressed = false;

            for (int i = 0; i < pressedStates.Count; i++) pressedStates[pressedStates.ElementAt(i).Key] = false;
        }

        public static bool IsKeyDown(Key k) => keyboard.IsKeyDown(k);
        public static bool IsKeyUp(Key k) => keyboard.IsKeyUp(k);
        public static bool IsKeyPressed(Key k) => pressedStates.ContainsKey(k) && pressedStates[k];

        public static float KeyAxis(Key a, Key b) {
            var an = keyboard.IsKeyDown(a) ? 1 : 0;
            var bn = keyboard.IsKeyDown(b) ? -1 : 0;
            return an + bn;
        }

        public static bool MouseLeftButtonDown => mouse.IsButtonDown(MouseButton.Left);
        public static bool MouseRightButtonDown => mouse.IsButtonDown(MouseButton.Right);

    }
}
