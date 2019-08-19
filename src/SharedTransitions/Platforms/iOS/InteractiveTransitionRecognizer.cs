using System;
using System.Collections.Generic;
using System.Text;

namespace Plugin.SharedTransitions.Platforms.iOS
{
	public class InteractiveTransitionRecognizer
	{
		readonly ITransitionRenderer _renderer;

		public InteractiveTransitionRecognizer(ITransitionRenderer renderer)
		{
			_renderer = renderer;
		}
	}
}
