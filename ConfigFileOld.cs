using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace YAMLEdit {
	public class ConfigFileOld {

		#region Attributes

		public string path;
		private string NEW_LINE = "\r\n";

		private string _content;
		public string content {
			get { return _content; }
			set {
				_content = value;
				contents = _content.Split(new[] { NEW_LINE }, StringSplitOptions.None);
			}
		}
		private string[] contents;

		private string _key;
		private string key {
			get { return _key; }
			set {
				_key = value.Replace(" ", "_");
				keyPart = _key.Split('.');
			}
		}
		private string[] keyPart;
		public bool createMissingKey = false;

		#endregion

		#region Construcor

		public ConfigFileOld(string filePath) {
			if(filePath.EndsWith("/"))
				filePath += "config.yml";
			else if(filePath.EndsWith("."))
				filePath += "yml";
			this.path = filePath;
			StreamReader reader = new StreamReader(this.path);
			content = reader.ReadToEnd();
			reader.Close();
		}

		#endregion

		#region Main Methods
		public void Write() {
			RemoveFirstLine();
			StreamWriter writer = new StreamWriter(this.path);
			writer.Write(content);
			writer.Close();
		}

		public void Write(string path) {
			RemoveFirstLine();
			if(!File.Exists(path))
				File.Create(path);
			StreamWriter writer = new StreamWriter(path);
			writer.Write(content);
			writer.Close();
		}

		#region Set

		public bool Set(string key, Object value) {
			if(value == null) {
				DeleteKey(key);
				return true;
			}
			string newValue = "";
			List<Object> compareList = new List<Object>();
			if(value is int || value is float) {
				newValue = value.ToString();
			} else if(value.GetType() == compareList.GetType()) {
				List<Object> list = (List<Object>) value;
				newValue = GetStringForList(key, list);
			} else {
				switch(value.GetType().Name) {
					case nameof(String):
						newValue = value.ToString();
						break;
					case nameof(Double):
						newValue = value.ToString();
						break;
					case nameof(Boolean):
						newValue = value.ToString();
						break;
					case nameof(Rectangle):
						Rectangle rec = (Rectangle) value;
						newValue = rec.X + ", " + rec.Y + ", " + rec.Width + ", " + rec.Height;
						break;
					case nameof(Color):
						Color color = (Color) value;
						newValue = color.A + ", " + color.R + ", " + color.G + ", " + color.B;
						break;
					case nameof(Point):
						Point point = (Point) value;
						newValue = point.X + ", " + point.Y;
						break;
					default:
						return false;
				}
			}
			if(newValue.StartsWith("- ")) {
				newValue = "_" + newValue.Substring(1);
			}
			DeleteKey(key);
			SetValueAtKey(key, newValue);
			return true;
		}

		private void SetValueAtKey(string key, string value) {
			this.key = key;
			string content = "";
			string tab = "";
			int index = 0;
			bool contained = false;
			for(int i = 0; i < keyPart.Length; i++) {
				for(int j = index + 1; j < contents.Length; j++) {
					if(!contents[j].StartsWith(tab))
						break;
					if(contents[j].Contains(tab + keyPart[i])) {
						index = j;
						contained = true;
						break;
					}
				}
				if(!contained) {
					for(int j = i; j < keyPart.Length; j++) {
						if(j + 1 == keyPart.Length) {
							contents[index] = contents[index].Substring(0, contents[index].IndexOf(":") + 1);
						}
						index++;
						ContentPushAt(index, tab + keyPart[j] + ":");
						tab += "  ";
					}
					break;
				}
				tab += "  ";
				contained = false;
			}
			contents[index] = contents[index].Substring(0, contents[index].IndexOf(":") + 1) + " " + value;
			for(int i = 0; i < contents.Length - 1; i++) {
				content += contents[i] + NEW_LINE;
			}
			this.content = content;
		}

		#endregion

		#region Delete
		private void DeleteKey(string key) {
			int index = GetIndexFromKey(key);
			if(index == -1)
				return;
			int x = 1;
			string tab = "  ";
			for(int i = 0; i < x; i++) {
				if(contents[index][i].Equals(' ')) {
					x++;
					tab += " ";
				}
			}
			contents[index] = null;
			for(int i = index + 1; i < contents.Length; i++) {
				if(contents[i].StartsWith(tab)) {
					contents[i] = null;
				} else if(contents[i].StartsWith(tab.Substring(2) + "- ")) {
					contents[i] = null;
				} else {
					break;
				}
			}
			contents = contents.Where(c => c != null).ToArray();
			string content = "";
			for(int i = 0; i < contents.Length - 1; i++) {
				content += contents[i] + NEW_LINE;
			}
			this.content = content;
		}
		#endregion

		#region Get

		public string GetString(string key) {
			int index = GetIndexFromKey(key);
			if(index < 0)
				return null;
			string s = this.contents[index];
			int from = s.IndexOf(":") + 2;
			int length = s.Length - from;
			if(length < 0)
				return null;
			s = s.Substring(from, length);
			return s;
		}

		public int GetInt(string key) {
			string s = GetString(key);
			if(s == null) {
				if(this.createMissingKey) {
					this.Set(key, 0);
					s = GetString(key);
				} else {
					throw new KeyNotFoundException("Could not find Element from Key");
				}
			}
			return int.Parse(s);
		}

		public float GetFloat(string key) {
			string s = GetString(key);
			if(s == null) {
				if(this.createMissingKey) {
					this.Set(key, 0.0f);
					s = GetString(key);
				} else {
					throw new KeyNotFoundException("Could not find Element from Key");
				}
			}
			return float.Parse(s);
		}

		public double GetDouble(string key) {
			string s = GetString(key);
			if(s == null) {
				if(this.createMissingKey) {
					this.Set(key, 0.0d);
					s = GetString(key);
				} else {
					throw new KeyNotFoundException("Could not find Element from Key");
				}
			}
			return double.Parse(s);
		}

		public bool GetBoolean(string key) {
			string s = GetString(key);
			if(s == null) {
				if(this.createMissingKey) {
					this.Set(key, false);
					s = GetString(key);
				} else {
					throw new KeyNotFoundException("Could not find Element from Key");
				}
			}
			return bool.Parse(s);
		}

		public Point GetPoint(string key) {
			string s = GetString(key);
			if(s == null) {
				if(this.createMissingKey) {
					this.Set(key, new Point(0, 0));
					s = GetString(key);
				} else {
					throw new KeyNotFoundException("Could not find Element from Key");
				}
			}
			return Converter.ToPoint(s);
		}

		public Color GetColor(string key) {
			string s = GetString(key);
			if(s == null) {
				if(this.createMissingKey) {
					this.Set(key, Color.FromArgb(255, 0, 0, 0));
					s = GetString(key);
				} else {
					throw new KeyNotFoundException("Could not find Element from Key");
				}
			}
			return Converter.ToColor(s);
		}

		public Rectangle GetRectangle(string key) {
			string s = GetString(key);
			if(s == null) {
				if(this.createMissingKey) {
					this.Set(key, new Rectangle(0, 0, 0, 0));
					s = GetString(key);
				} else {
					throw new KeyNotFoundException("Could not find Element from Key");
				}
			}
			return Converter.ToRectangle(s);
		}

		// TODO Generic T
		public List<Object> GetList(string key) {
			int index = GetIndexFromKey(key);
			string tab = "";
			int count = contents[index].TakeWhile(Char.IsWhiteSpace).Count();
			for(int i = 0; i < count; i++) {
				tab += " ";
			}
			List<Object> list = new List<object>();
			for(int i = index + 1; i < contents.Length; i++) {
				string s = contents[i];
				if(s.StartsWith(tab + "- ")) {
					list.Add(s.Substring(2));
				} else {
					break;
				}
			}
			return list;
		}

		#endregion

		#endregion

		#region Help Methods

		private void ContentPushAt(int index, string value) {
			string[] contents = new string[this.contents.Length + 1];
			int j = 0;
			for(int i = 0; i < contents.Length; i++) {
				if(i == index) {
					contents[i] = value;
					j--;
				} else {
					contents[i] = this.contents[i + j];
				}
			}
			this.contents = contents;
		}

		private int GetIndexFromKey(string key) {
			this.key = key;
			string tab = "";
			int index = 0;
			bool contained = false;
			for(int i = 0; i < keyPart.Length; i++) {
				for(int j = index; j < contents.Length; j++) {
					if(contents[j].StartsWith(tab + keyPart[i])) {
						index = j;
						contained = true;
						break;
					}
				}
				if(!contained)
					return -1;
				contained = false;
				tab += "  ";
			}
			return index;
		}

		public bool KeyExists(string key) {
			return GetIndexFromKey(key) >= 0;
		}

		private String GetStringForList(string key, List<Object> list) {
			string listString = "";
			string listSign = "- ";
			string tab = "";
			int count = key.Length - key.Replace(".", "").Length;
			for(int i = 0; i < count; i++)
				tab += "  ";
			foreach(Object listElement in list) {
				listString += NEW_LINE + tab + listSign + listElement;
			}
			return listString;
		}

		private void RemoveFirstLine() {
			contents[0] = null;
			contents = contents.Where(c => c != null).ToArray();
			string content = "";
			for(int i = 0; i < contents.Length - 1; i++) {
				content += contents[i] + NEW_LINE;
			}
			this.content = content;
		}

		#endregion

	}
}
