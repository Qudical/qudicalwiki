using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace qwikigen
{
	class Program
	{
		public static Dictionary<string, string> siteSettings;

		static int Main(string[] args)
		{
			// Find site .qwi file (neesd to be first argument) and get the project settings.
			if (args.Length == 0)
			{
				Console.WriteLine("Please provide a valid path to a qudicalwiki folder.");
				Console.WriteLine("(qwikigen.exe path/to/folder)");
				return 1;
			}

			string projectRoot = args[0];

			siteSettings = QwfReader.ReadQwi(QwfReader.GetFileTextLines(projectRoot + "\\siteinfo.qwi"));

			// Set up some folders and copy assets and layout over

			string resultDir = projectRoot + "\\result";
			if (Directory.Exists(resultDir))
			{
				Directory.Delete(resultDir, true);
			}
			Directory.CreateDirectory(resultDir);
			Directory.CreateDirectory(resultDir + "\\assets");
			Directory.CreateDirectory(resultDir + "\\styles");
			Directory.CreateDirectory(resultDir + "\\articles");
			Directory.CreateDirectory(resultDir + "\\categories");

			File.Copy(projectRoot + "\\layout\\style.css", resultDir + "\\styles\\style.css");

			CopyAll(new DirectoryInfo(projectRoot + "\\assets"), new DirectoryInfo(resultDir + "\\assets"));

			SiteConstructor.categories = new Dictionary<string, Dictionary<string, List<string>>>();
			SiteConstructor.categoryDescriptions = new Dictionary<string, string>();
			SiteConstructor.CreateCategories(new DirectoryInfo(projectRoot + "\\articles"), new DirectoryInfo(resultDir + "\\articles"), projectRoot, resultDir);
			SiteConstructor.CreateCategoryPages(projectRoot, resultDir);

			System.IO.File.WriteAllText(resultDir + "\\index.html", SiteConstructor.GenerateHomepage(projectRoot, resultDir, siteSettings));

			SiteConstructor.ConvertArticles(new DirectoryInfo(projectRoot + "\\articles"), new DirectoryInfo(resultDir + "\\articles"), projectRoot);

			Console.WriteLine("Site generated!");

			return 0;
		}

		public static void CopyAll(DirectoryInfo source, DirectoryInfo target)
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

			// Copy each file into it's new directory.
			foreach (FileInfo fi in source.GetFiles())
			{
				Console.WriteLine(@"Copying {0}\{1}", target.FullName, fi.Name);
				fi.CopyTo(Path.Combine(target.ToString(), fi.Name), true);
			}

			// Copy each subdirectory using recursion.
			foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
			{
				DirectoryInfo nextTargetSubDir =
					target.CreateSubdirectory(diSourceSubDir.Name);
				CopyAll(diSourceSubDir, nextTargetSubDir);
			}
		}
	}
}
