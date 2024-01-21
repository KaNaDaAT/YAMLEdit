using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace YAMLEdit {
	class Converter {

		public static Point ToPoint(String s) {
			int[] point = new int[2];
			for(int i = 0; i < point.Length; i++) {
				point[i] = int.Parse(s.Split(',')[i]);
			}
			return new Point(point[0], point[1]);
		}

		public static Color ToColor(String s) {
			int[] color = new int[4];
			for(int i = 0; i < color.Length; i++) {
				color[i] = int.Parse(s.Split(',')[i]);
			}
			return Color.FromArgb(color[0], color[1], color[2], color[3]);
		}

		public static Rectangle ToRectangle(String s) {
			int[] rect = new int[4];
			for(int i = 0; i < rect.Length; i++) {
				rect[i] = int.Parse(s.Split(',')[i]);
			}
			return new Rectangle(rect[0], rect[1], rect[2], rect[3]);
		}

	}
}
