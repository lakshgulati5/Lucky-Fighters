using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lucky_Fighters
{
	class AnimatedSprite
	{
		/// <summary>
		/// Stores a key-value pair of animation names to their Animation
		/// </summary>
		protected Dictionary<string, Animation> SpriteAnimations;
		// The current animation name
		protected string currentAnim = "Idle";
		public int FrameWidth { get; }
		public int FrameHeight { get; }
		int FramesPerRow;

		/// <summary>
		/// The source rectangle, given the current frame index
		/// </summary>
		public Rectangle SourceRectangle
		{
			get
			{
				int frameIndex = SpriteAnimations[currentAnim].CurrentFrame;
				int x = frameIndex % FramesPerRow, y = frameIndex / FramesPerRow;
				return new Rectangle(x * FrameWidth, y * FrameHeight, FrameWidth, FrameHeight);
			}
		}

		public Vector2 Origin => new Vector2(FrameWidth, FrameHeight) / 2f;

		public AnimatedSprite(int frameWidth, int frameHeight, int framesPerRow)
		{
			SpriteAnimations = new Dictionary<string, Animation>();
			FrameWidth = frameWidth;
			FrameHeight = frameHeight;
			FramesPerRow = framesPerRow;
		}

		public virtual void Update(GameTime gameTime)
		{
			SpriteAnimations[currentAnim].Update(gameTime);
		}

		public void SetAndPlayAnimation(string animationName)
		{
			if (!SpriteAnimations.ContainsKey(animationName))
				return;
			SpriteAnimations[currentAnim].Stop();
			currentAnim = animationName;
			SpriteAnimations[animationName].Play();
		}

		public void PlayAnimationIfNotPlaying(string animationName)
		{
			if (currentAnim != animationName || !SpriteAnimations[currentAnim].IsPlaying)
			{
				SetAndPlayAnimation(animationName);
			}
		}
	}
}
