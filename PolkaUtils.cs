using RDR2.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace PolkaUtilils {
	public class PolkaUtililty {
		public List<Keys> pressedKeys = new List<Keys>();
		/* Debug Stuff */
		private string _debug;
		private bool _showDebug = false;
		/* Sticky log that hangs around until manually cleared*/
		private string _log;

		public void ClearDebug() {
			_debug = string.Empty;
		}

		public void AddDebugMessage(Func<string> message) {
			if (_showDebug) {
				_debug += message();
			}
		}

		public void AddStickyDebugMessage(Func<string> message) {
			if (_showDebug) {
				_log += message();
			}
		}

		public void ClearStickyDebugMessage() {
			_log = string.Empty;
		}

		public void ToggleDebug() {
			_showDebug = !_showDebug;
		}

		public void ShowDebugMessage() {

			if (_showDebug) {
				TextElement textElement = new TextElement($"{_debug}\n\n{_log}\n\n{string.Join(", ", pressedKeys )}", new PointF(100.0f, 100.0f), 0.30f);
				textElement.Draw();
			}
		}
		public static float GetRandomFloat(float min = 0.5f, float max = 2.0f) {
			Random rand = new Random();
			double range = max - min;
			return (float)(min + rand.NextDouble() * range);
		}

		public void LogPressedKey(KeyEventArgs pressedKey) {
			if (!pressedKeys.Contains(pressedKey.KeyCode)) {
				pressedKeys.Add(pressedKey.KeyCode);
			}
		}

		public void UnpressKey(KeyEventArgs pressedKey) {
			pressedKeys.RemoveAll(p => p == pressedKey.KeyCode);
		}

		public static string ToOrdinal(int number) {
			if (number <= 0)
				return number.ToString();

			int lastTwoDigits = number % 100;
			int lastDigit = number % 10;

			if (lastTwoDigits >= 11 && lastTwoDigits <= 13)
				return number + "th";

			string suffix;
			switch (lastDigit) {
				case 1: suffix = "st"; break;
				case 2: suffix = "nd"; break;
				case 3: suffix = "rd"; break;
				default: suffix = "th"; break;
			}

			return number + suffix;
		}

	}
}
