using System.Collections.Generic;
using System;
using System.IO;

namespace Azalea.Core
{
	public static class Data
	{
		public delegate int TestCurve(int value, int value2, int quality);
		public delegate int QualityLevelCurve(int rawValue);

		public static Dictionary<string, TestCurve> TestCurves = new Dictionary<string, TestCurve>();
		public static TestCurve DefaultTestCurve;
		public static Dictionary<string, QualityLevelCurve> LevelCurves = new Dictionary<string, QualityLevelCurve>();
		public static QualityLevelCurve DefaultLevelCurve;

		public static Dictionary<string, Storylet> Storylets = new Dictionary<string, Storylet>();
		public static Dictionary<string, Quality> Qualities = new Dictionary<string, Quality>();
		public static Random GenericRandom = new Random();

		public static Game CurrentGame;

		public static void Init()
		{
			TestCurves.Clear();
			LevelCurves.Clear();
			Storylets.Clear();
			Qualities.Clear();
		}

		public static void Log(string s, params object[] arg)
		{
			Console.WriteLine(s, arg);
		}

		public static Storylet GetStorylet(string name)
		{
			if (name == null)
				return null;

			if (name == "@Location")
				return CurrentGame.CurrentLocation;

			if (Storylets.ContainsKey(name))
				return Storylets[name];

			return null;
		}

		public static Quality GetQuality(string name)
		{
			if (name == null)
				return null;

			if (Qualities.ContainsKey(name))
				return Qualities[name];

			return null;
		}

		public static void LoadStoryletFile(string path)
		{
			var loader = new StoryletLoader();
			loader.LoadTextFile(path, Storylets);
		}

		public static void LoadQualitiesFile(string path)
		{
			var loader = new QualityLoader();
			loader.LoadTextFile(path, Qualities);
		}

		public static string GetRandomString()
		{
			char[] availableChars = "abcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();
			int charCount = availableChars.Length;
			int num = 12;
			var result = new char[num];
			while (num-- > 0)
			{
				result[num] = availableChars[Data.GenericRandom.Next(charCount)];
			}
			return new string(result);
		}
	}
}