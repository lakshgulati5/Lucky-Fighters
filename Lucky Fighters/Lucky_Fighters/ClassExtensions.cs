using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Lucky_Fighters
{
    /// <summary>
    /// Helpful method for working with rectangles
    /// </summary>
    public static class ClassExtensions
    {
        /// <summary>
        /// Calculates the signed depth of intersection between two rectangles.
        /// </summary>
        /// <returns>
        /// The amount of overlap between two intersecting rectangles. These
        /// depth values can be negative depending on wihch wides the rectangles
        /// intersect. This allows callers to determine the correct direciton 
        /// to push objects in order to resolve collisions. 
        /// If the rectangles are not intersecting, Vector2.Zero is returned 
        /// </returns>
        public static Vector2 GetIntersectionDepth(this Rectangle _rectA, Rectangle _rectB)
        {
            // Calculate half sizes.
            float halfWidthA = _rectA.Width / 2.0f;
            float halfHeightA = _rectA.Height / 2.0f;
            float halfWidthB = _rectB.Width / 2.0f;
            float halfHeightB = _rectB.Height / 2.0f;

            // Calculate centers.
            Vector2 centerA = new Vector2(_rectA.Left + halfWidthA, _rectA.Top + halfHeightA);
            Vector2 centerB = new Vector2(_rectB.Left + halfWidthB, _rectB.Top + halfHeightB);

            // Calculate current and minimum-non-intersecting distances between centers.
            float distanceX = centerA.X - centerB.X;
            float distanceY = centerA.Y - centerB.Y;
            float minDistanceX = halfWidthA + halfWidthB;
            float minDistanceY = halfHeightA + halfHeightB;

            // If we are not intersecting at all, return (0, 0).
            if (Math.Abs(distanceX) >= minDistanceX || Math.Abs(distanceY) >= minDistanceY)
                return Vector2.Zero;

            // Calculate and return intersection depths.
            float depthX = distanceX > 0 ? minDistanceX - distanceX : -minDistanceX - distanceX;
            float depthY = distanceY > 0 ? minDistanceY - distanceY : -minDistanceY - distanceY;
            return new Vector2(depthX, depthY);
        }

        public static float GetElapsedSeconds(this GameTime gameTime)
        {
            return (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public static bool StartKeyPressed(this KeyboardState kb)
        {
            return kb.IsKeyDown(Keys.Enter);
        }

        public static bool BackKeyPressed(this KeyboardState kb)
        {
            return kb.IsKeyDown(Keys.Back);
        }

        public static bool MenuUpPressed(this KeyboardState kb)
        {
            return kb.IsKeyDown(Keys.W) || kb.IsKeyDown(Keys.Up);
        }

        public static bool MenuDownPressed(this KeyboardState kb)
        {
            return kb.IsKeyDown(Keys.S) || kb.IsKeyDown(Keys.Down);
        }

        public static bool MenuLeftPressed(this KeyboardState kb)
        {
            return kb.IsKeyDown(Keys.A) || kb.IsKeyDown(Keys.Left);
        }

        public static bool MenuRightPressed(this KeyboardState kb)
        {
            return kb.IsKeyDown(Keys.D) || kb.IsKeyDown(Keys.Right);
        }

        public static bool GamePadAPressed(this KeyboardState kb)
        {
            return kb.IsKeyDown(Keys.Space);
        }

        public static bool GamePadBPressed(this KeyboardState kb)
        {
            return kb.IsKeyDown(Keys.Tab);
        }
    }
}
