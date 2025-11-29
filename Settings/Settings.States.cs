using System.Linq;
using Verse;

namespace RW_CustomPawnGeneration
{
	public partial class Settings
	{
		public struct State
		{
			/// <summary>
			/// Also used for males.
			/// </summary>
			public static State GLOBAL = new State(null);
			public static State FEMALE = new State(null, Gender.Female);

			public bool global;
			/// <summary>
			/// {Gender:Integer}|{Race:String:Optional}|{Category:String}|{SubCategory:String:Optional}
			/// <br></br>
			/// By default, females also use the male settings.
			/// To allow females to have separate settings, `1|{Race:String:Optional}|SeparateGender` must be enabled.
			/// <br></br>
			/// Example;
			/// <br></br>
			/// 1|Human|FilterBody|Fat =
			/// Fat body type filter settings for human males.
			/// <br></br>
			/// 1||FilterBody|Thin =
			/// Thin body type filter settings for all races.
			/// This only applies to any races that have body types, including race mods (humanoid alien races.)
			/// <br></br>
			/// 2|Human|FilterBody|Hulk =
			/// Hulk body type filter settings for human females.
			/// This will only be used if `1|Human|SeparateGender` is enabled.
			/// </summary>
			public string prefix;

			public State(ThingDef race, Gender gender = Gender.Male)
			{
				int i = (int)gender;
				global = race == null;
				prefix =
					race != null ?
						$"{i}|{race.defName}|" :
						$"{i}||";
			}

			/*public State(Gender gender, ThingDef race = null)
			{
				global = race == null;
				int i = (int)gender;

				prefix =
					race != null ?
						$"{i}|{race.defName}|" :
						$"{i}||";
			}*/

			public int Get(string key)
			{
				string key0 = prefix + key;

				if (IntStates != null && IntStates.ContainsKey(key0))
					return IntStates[key0];

				return global
					? GlobalIntDefaults.ContainsKey(key) ? GlobalIntDefaults[key] : 0
					: LocalIntDefaults.ContainsKey(key) ? LocalIntDefaults[key] : 0;
			}

			public void Set(string key, int value)
			{
				string key0 = prefix + key;
				int @default = global
					? GlobalIntDefaults.ContainsKey(key) ? GlobalIntDefaults[key] : 0
					: LocalIntDefaults.ContainsKey(key) ? LocalIntDefaults[key] : 0;

				if (value != @default)
					IntStates[key0] = value;
				else if (IntStates.ContainsKey(key0))
					IntStates.Remove(key0);
			}

			public void Remove(string key)
			{
				string key0 = prefix + key;

				if (IntStates.ContainsKey(key0))
					IntStates.Remove(key0);
			}

			public bool GBool(string key)
			{
				if (global)
					return Get(key) == 1;

				return Get(key) == 2;
			}

			public bool Bool(string key)
			{
				return Get(key) == 1;
			}

			public void Clear()
			{
				foreach (string key in IntStates.Keys.ToArray())
					if (key.StartsWith(prefix))
						IntStates.Remove(key);
			}
		}
	}
}
