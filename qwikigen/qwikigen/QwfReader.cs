using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace qwikigen
{
	static public class QwfReader
	{
		static public Dictionary<string, string> ReadQwi(List<string> text)
		{
			var result = new Dictionary<string, string>();
			for (int i = 0; i < text.Count; i += 2)
			{
				result.Add(text[i], text[i + 1]);
			}
			return result;
		}

		static public List<string> GetFileTextLines(string path, int fromLine = 0, int lineCount = -1)
		{
			var result = new List<string>();

			try
			{
				string line;
				StreamReader sr = new StreamReader(path);

				int i = 0;
				while(i < fromLine && sr.ReadLine() != null)
				{
					i++;
				}

				if (lineCount == -1)
				{
					while ((line = sr.ReadLine()) != null)
					{
						result.Add(line);
					}
				}
				else
				{
					i = 0;
					while ((line = sr.ReadLine()) != null && i < lineCount)
					{
						result.Add(line);
						i++;
					}
				}				
			}
			catch (IOException e)
			{
				Console.WriteLine("Error reading file:");
				Console.WriteLine(e.Message);
				Environment.Exit(1);
			}

			return result;
		}

		static public string ReadFileText(string path, int fromLine = 0, int lineCount = -1)
		{
			string result = "";

			try
			{
				string line;
				StreamReader sr = new StreamReader(path);

				int i = 0;
				while (sr.ReadLine() != null && i < fromLine)
				{
					i++;
				}

				if (lineCount == -1)
				{
					while ((line = sr.ReadLine()) != null)
					{
						result += line;
					}
				}
				else
				{
					i = 0;
					while ((line = sr.ReadLine()) != null && i < lineCount)
					{
						result += line;
						i++;
					}
				}
			}
			catch (IOException e)
			{
				Console.WriteLine("Error reading file:");
				Console.WriteLine(e.Message);
				Environment.Exit(1);
			}

			return result;
		}
	
		static public string MDToHTML(List<string> text)
		{
			string result = "";
			bool bold = false;
			bool italic = false;

			foreach (string line in text)
			{
				string newLine = line;

				// Bold and italics with *
				int index = 0;
				while (index != -1)
				{
					index = newLine.IndexOf("*", index);
					if (index != -1)
					{
						// Is there a backslash to escape?
						if (index == 0 ? true : newLine[index - 1] != '\\')
						{
							if (newLine[index + 1] == '*')
							{
								ReplaceTag(ref newLine, index, ref bold, "<b>", "</b>", 2);
							}
							else
							{
								ReplaceTag(ref newLine, index, ref italic, "<i>", "</i>", 1);
							}
						}
						index++;
					}
				}

				// Bold and italics with _
				index = 0;
				while (index != -1)
				{
					index = newLine.IndexOf("_", index);
					if (index != -1)
					{
						// Is there a backslash to escape?
						if (index == 0 ? true : newLine[index - 1] != '\\')
						{
							if (newLine[index + 1] == '_')
							{
								ReplaceTag(ref newLine, index, ref bold, "<b>", "</b>", 2);
							}
							else
							{
								ReplaceTag(ref newLine, index, ref italic, "<i>", "</i>", 1);
							}
						}
						index++;
					}
				}

				// Code
				index = 0;
				while (index != -1)
				{
					index = newLine.IndexOf("`", index);
					if (index != -1)
					{
						// Is there a backslash to escape?
						if (index == 0 ? true : newLine[index - 1] != '\\')
						{
							ReplaceTag(ref newLine, index, ref italic, "<code>", "</code>", 1);
						}
						index++;
					}
				}

				// Header1
				index = 0;
				while (index != -1)
				{
					index = newLine.IndexOf("#", index);
					if (index != -1)
					{
						// Is there a backslash to escape?
						if (index == 0 ? true : newLine[index - 1] != '\\')
						{
							ReplaceTag(ref newLine, index, ref italic, "<h1>", "</h1>", 1);
						}
						index++;
					}
				}

				// Header2
				index = 0;
				while (index != -1)
				{
					index = newLine.IndexOf("##", index);
					if (index != -1)
					{
						// Is there a backslash to escape?
						if (index == 0 ? true : newLine[index - 1] != '\\')
						{
							ReplaceTag(ref newLine, index, ref italic, "<h2>", "</h2>", 1);
						}
						index++;
					}
				}
				result += "<p>" + newLine + "</p>";
			}


			return result;
		}

		private static void ReplaceTag(ref string newLine, int index, ref bool tag, string openTag, string closeTag, int removeCount)
		{
			newLine = newLine.Remove(index, removeCount);
			if (!tag)
			{
				newLine = newLine.Insert(index, openTag);
			}
			else
			{
				newLine = newLine.Insert(index, closeTag);
			}
			tag = !tag;
		}

		public static string ReplaceAt(this string input, int index, char newChar)
		{
			if (input == null)
			{
				throw new ArgumentNullException("input");
			}
			char[] chars = input.ToCharArray();
			chars[index] = newChar;
			return new string(chars);
		}
	}
}
