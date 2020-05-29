using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

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
	
		static public string MDToHTML(string text)
		{
			bool bold = false;
			bool italic = false;

			string newText = text;

			// Bold and italics with *
			int index = 0;
			while (index != -1)
			{
				index = newText.IndexOf("*", index);
				if (index != -1)
				{
					// Is there a backslash to escape?
					if (index == 0 ? true : newText[index - 1] != '\\')
					{
						if (newText[index + 1] == '*')
						{
							ReplaceTag(ref newText, index, ref bold, "<b>", "</b>", 2);
						}
						else
						{
							ReplaceTag(ref newText, index, ref italic, "<i>", "</i>", 1);
						}
					}
					index++;
				}
			}

			// Bold and italics with _
			index = 0;
			while (index != -1)
			{
				index = newText.IndexOf("_", index);
				if (index != -1)
				{
					// Is there a backslash to escape?
					if (index == 0 ? true : newText[index - 1] != '\\')
					{
						if (newText[index + 1] == '_')
						{
							ReplaceTag(ref newText, index, ref bold, "<b>", "</b>", 2);
						}
						else
						{
							ReplaceTag(ref newText, index, ref italic, "<i>", "</i>", 1);
						}
					}
					index++;
				}
			}

			// Code block
			index = 0;
			bool codeBlock = false;
			while (index != -1)
			{
				index = newText.IndexOf("~", index);
				if (index != -1)
				{
					// Is there a backslash to escape?
					if (index == 0 ? true : newText[index - 1] != '\\')
					{
						ReplaceTag(ref newText, index, ref codeBlock, "<div class=\"codeblock\"><code>", "</code></div>", 1);
					}
					index++;
				}
			}

			// Code
			index = 0;
			bool code = false;
			while (index != -1)
			{
				index = newText.IndexOf("`", index);
				if (index != -1)
				{
					// Is there a backslash to escape?
					if (index == 0 ? true : newText[index - 1] != '\\')
					{
						ReplaceTag(ref newText, index, ref code, "<code>", "</code>", 1);
					}
					index++;
				}
			}

			// Header1
			index = 0;
			bool header1 = false;
			while (index != -1)
			{
				index = newText.IndexOf("#", index);
				if (index != -1)
				{
					// Is there a backslash to escape?
					if (index == 0 ? true : newText[index - 1] != '\\')
					{
						// Bit of a workaround, but since the margins in headers are quite big, we're removing the linebreaks. Same goes for Header2.
						if (newText.Length >= index + 4)
						{
							if (newText.Substring(index + 1, 4) == "<br>")
							{
								newText = newText.Remove(index + 1, 4);
							}
						}
						ReplaceTag(ref newText, index, ref header1, "<h1>", "</h1>", 1);
					}
					index++;
				}
			}

			// Header2
			index = 0;
			bool header2 = false;
			while (index != -1)
			{
				index = newText.IndexOf("##", index);
				if (index != -1)
				{
					// Is there a backslash to escape?
					if (index == 0 ? true : newText[index - 1] != '\\')
					{
						if (newText.Length >= index + 4)
						{
							if (newText.Substring(index + 2, 4) == "<br>")
							{
								newText = newText.Remove(index + 2, 4);
							}
						}
						ReplaceTag(ref newText, index, ref header2, "<h2>", "</h2>", 1);
					}
					index++;
				}
			}

			// Image
			index = 0;
			bool image = false;
			int lastIndex = 0;
			while (index != -1)
			{
				index = newText.IndexOf("$", index);
				if (index != -1)
				{
					// Is there a backslash to escape?
					if (index == 0 ? true : newText[index - 1] != '\\')
					{
						// i think this is....incredibly messy....but it works...
						string url = "";
						if (image)
						{
							url = newText.Substring(lastIndex + "<a href=\"".Length, index - (lastIndex + "<a href=\"".Length));
						}
						
						ReplaceTag(ref newText, index, ref image, "<a href=\"", $"\"><img src=\"{url}\" style=\"max-height:200px;\"></a>", 1);
						lastIndex = index;
					}
					index++;
				}
			}

			return $"<p>{newText}</p>";
		}

		static public string MDToHTML(List<string> text)
		{
			string input = "";
			foreach (string line in text)
			{
				// Ideally we would be entering a \n here instead of a <br>, and instead switch to a <br> in the MDToHTML function, but that doesn't seem to work for me, so this will have to do.
				input += line + "<br>";
			}
			if (input.EndsWith("<br>"))
			{
				input = input.Substring(0, input.LastIndexOf("<br>"));
			}
			return MDToHTML(input);
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
