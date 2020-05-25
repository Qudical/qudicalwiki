﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace qwikigen
{
	static public class SiteConstructor
	{
		// okay okay okay i know this looks absolutely fucking ridiculous but trust me okay it makes sense
		// its a dictionary of categories, each key is a category, with its value equal to a dictionary, with each key being a section, with each value being a list of page paths that are in there.
		// i know it's ridiculous but hey it works...i think
		public static Dictionary<string, Dictionary<string, List<string>>> categories;

		public static string ProcessArticle(string path, string projectRoot)
		{
			string result = QwfReader.ReadFileText(projectRoot + "\\layout\\template.html");
			// Get document info
			var lines = QwfReader.GetFileTextLines(path);
			List<string> qwiLines = new List<string>();
			List<string> articleLines = new List<string>();
			bool readingArticle = false;
			foreach (string line in lines)
			{
				if (line == "==")
				{
					readingArticle = true;
					continue;
				}
				if (readingArticle)
				{
					articleLines.Add(line);
				}
				else
				{
					qwiLines.Add(line);
				}
			}
			var qwi = QwfReader.ReadQwi(qwiLines);
			string article = QwfReader.MDToHTML(articleLines);
			result = result.Replace(";;TITLE;;", qwi["Title"]);
			result = result.Replace(";;ARTICLE;;", article);
			result = result.Replace(";;SIDEBAR_LINKS;;", GetSidebarLinks());
			return result;
		}

		public static string GenerateHomepage(string projectRoot, string resultDir, Dictionary<string, string> siteSettings)
		{
			string result = QwfReader.ReadFileText(projectRoot + "\\layout\\template.html");
			List<string> articleLines = new List<string>()
			{
				"#Welcome to the Qudical Developer Wiki!#",
				"Here you'll _(hopefully)_ find some useful information regarding the technical side of Qudical projects."
			};
			string article = QwfReader.MDToHTML(articleLines);
			result = result.Replace(";;TITLE;;", siteSettings["SiteName"]);
			result = result.Replace(";;ARTICLE;;", article);
			result = result.Replace(";;SIDEBAR_LINKS;;", GetSidebarLinks());
			return result;
		}

		private static string GetSidebarLinks()
		{
			// TODO: add logic. for now, just give a link to the home and 84 modkit page
			return "<a href=\"/index.html\">Home</a><a href=\"/articles/dsa1984/modkit.html\">DSa1984</a>";
		}

		public static void ConvertArticles(DirectoryInfo source, DirectoryInfo target, string projectRoot)
		{
			if (source.FullName.ToLower() == target.FullName.ToLower())
			{
				return;
			}

			// Check if the target directory exists, if not, create it.
			if (Directory.Exists(target.FullName) == false)
			{
				Directory.CreateDirectory(target.FullName);
			}

			// Translate each article in the directory into an html file
			foreach (FileInfo fi in source.GetFiles())
			{
				Console.WriteLine(@"Translating {0}\{1}", target.FullName, fi.Name);
				string articleText = ProcessArticle(fi.FullName, projectRoot);
				// Console.WriteLine();
				
				System.IO.File.WriteAllText(Path.Combine(target.ToString(), fi.Name.Replace(".qwa", ".html")), articleText);
			}

			// Copy and translate each subdirectory using recursion.
			foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
			{
				DirectoryInfo nextTargetSubDir =
					target.CreateSubdirectory(diSourceSubDir.Name);
				ConvertArticles(diSourceSubDir, nextTargetSubDir, projectRoot);
			}
		}

		public static void CreateCategories(DirectoryInfo source, DirectoryInfo target, string projectRoot, string resultDir)
		{
			if (source.FullName.ToLower() == target.FullName.ToLower())
			{
				return;
			}

			// Check if the target directory exists, if not, create it.
			if (Directory.Exists(target.FullName) == false)
			{
				Directory.CreateDirectory(target.FullName);
			}

			// Translate each article in the directory into an html file
			foreach (FileInfo fi in source.GetFiles())
			{
				Console.WriteLine(@"Searching for category and section in {0}\{1}", target.FullName, fi.Name);
				ProcessArticleCategory(fi.FullName, projectRoot, resultDir);
			}

			// Copy and translate each subdirectory using recursion.
			foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
			{
				DirectoryInfo nextTargetSubDir =
					target.CreateSubdirectory(diSourceSubDir.Name);
				CreateCategories(diSourceSubDir, nextTargetSubDir, projectRoot, resultDir);
			}
		}

		private static void ProcessArticleCategory(string path, string projectRoot, string resultDir)
		{
			// Get document info
			var lines = QwfReader.GetFileTextLines(path);
			List<string> qwiLines = new List<string>();
			bool readingArticle = false;
			foreach (string line in lines)
			{
				if (line == "==")
				{
					readingArticle = true;
					continue;
				}
				if (!readingArticle)
				{
					qwiLines.Add(line);
				}
			}
			var qwi = QwfReader.ReadQwi(qwiLines);
			string relativePath = Path.GetRelativePath(resultDir, path.Replace(".qwa", ".html"));

			// Add current page to category and section, create them if they don't exist already.
			if (categories.ContainsKey(qwi["Category"]))
			{
				if (categories[qwi["Category"]].ContainsKey(qwi["Section"]))
				{
					if (!categories[qwi["Category"]][qwi["Section"]].Contains(relativePath))
					{
						categories[qwi["Category"]][qwi["Section"]].Add(relativePath);
					}
				}
				else
				{
					categories[qwi["Category"]][qwi["Section"]] = new List<string>() { relativePath };
					Console.WriteLine("Creating section " + qwi["Section"]);
				}
			}
			else
			{
				categories[qwi["Category"]] = new Dictionary<string, List<string>>();
				Console.WriteLine("Creating category " + qwi["Category"]);

				if (categories[qwi["Category"]].ContainsKey(qwi["Section"]))
				{
					if (!categories[qwi["Category"]][qwi["Section"]].Contains(relativePath))
					{
						categories[qwi["Category"]][qwi["Section"]].Add(relativePath);
					}
				}
				else
				{
					categories[qwi["Category"]][qwi["Section"]] = new List<string>() { relativePath };
					Console.WriteLine("Creating section " + qwi["Section"]);
				}
			}
		}
	}
}
