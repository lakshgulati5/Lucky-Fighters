using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lucky_Fighters
{
	class Animation
	{
		int[] frames;
		int frameIndex;
		float secondsPerFrame;
		bool loop;
		float totalElapsed;
		Action callback;

		public bool IsPlaying { get; private set; }
		public int CurrentFrame => frames[frameIndex];

		public Animation(int[] frames, float framesPerSecond, bool loop = false, Action callback = null)
		{
			this.frames = frames;
			frameIndex = 0;
			secondsPerFrame = 1f / framesPerSecond;
			this.loop = loop;
			this.callback = callback;

			IsPlaying = false;
			totalElapsed = 0f;
		}

		public void Play()
		{
			frameIndex = 0;
			totalElapsed = 0f;
			IsPlaying = true;
		}

		public void Stop()
		{
			IsPlaying = false;
		}

		public void Update(GameTime gameTime)
		{
			if (!IsPlaying)
				return;
			float elapsed = gameTime.GetElapsedSeconds();
			totalElapsed += elapsed;
			if (totalElapsed >= secondsPerFrame)
			{
				frameIndex = (frameIndex + 1) % frames.Length;
				totalElapsed -= secondsPerFrame;
				if (!loop && frameIndex == 0)
				{
					IsPlaying = false;
					if (callback != null)
						callback();
				}
			}
		}
	}
}
