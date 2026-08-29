using UnityEngine;
using Verse;

namespace RW_CustomPawnGeneration
{
	public class SettingsController : Mod
	{
		public SettingsController(ModContentPack content) : base(content)
		{
			GetSettings<Settings>();
		}

		public override string SettingsCategory()
		{
			return Strings.Labels.CPGName;
		}

		public override void DoSettingsWindowContents(Rect inRect)
		{
			Settings.DoWindowContents(inRect);
		}
	}
}
