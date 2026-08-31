using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;

namespace RW_CustomPawnGeneration
{
	public class Module
	{
		private static readonly HashSet<Module> all = new HashSet<Module>();

		private bool initialized = false;
		private readonly string key = null;
		private readonly MethodInfo method = null;
		private readonly HarmonyMethod prefix = null;
		private readonly HarmonyMethod postfix = null;
		private readonly HarmonyMethod transplier = null;
		private readonly HarmonyMethod finalizer = null;

		public bool IsPatched { get; private set; } = true;

		public Module
			(string key,
			MethodInfo method,
			HarmonyMethod prefix = null,
			HarmonyMethod postfix = null,
			HarmonyMethod transplier = null,
			HarmonyMethod finalizer = null)
		{
			all.Add(this);

			this.key = key;
			this.method = method;
			this.prefix = prefix;
			this.postfix = postfix;
			this.transplier = transplier;
			this.finalizer = finalizer;

			Initialize();
		}

		private void Initialize()
		{
			if (RW_CustomPawnGeneration.patcher == null)
				return;

			if (initialized)
				return;

			initialized = true;

			if (!Settings.State.GLOBAL.Bool(key))
				Unpatch();
		}

		public void Unpatch()
		{
			if (!IsPatched)
				return;

			IsPatched = false;

			RW_CustomPawnGeneration.patcher.Unpatch(
				method,
				HarmonyPatchType.All,
				RW_CustomPawnGeneration.ID
			);
		}

		public void Patch()
		{
			if (IsPatched)
				return;

			IsPatched = true;

			RW_CustomPawnGeneration.patcher.Patch(
				method,
				prefix,
				postfix,
				transplier,
				finalizer
			);
		}

		public static void InitializeAll()
		{
			foreach (Module module in all)
				module.Initialize();
		}
	}
}
