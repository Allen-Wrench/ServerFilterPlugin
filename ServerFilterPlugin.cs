using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using Sandbox.Game;
using VRage.Game;
using VRage.GameServices;
using VRage.Plugins;

namespace ServerFilterPlugin
{
	public class ServerFilterPlugin : IDisposable, IPlugin
	{
		public void Dispose()
		{
		}

		public void Init(object gameInstance)
		{
			new Harmony("ServerFilterPlugin").Patch(AccessTools.Method("Sandbox.Game.Gui.MyGuiScreenJoinGame:FilterSimple"), transpiler: new HarmonyMethod(typeof(ServerFilterPlugin), nameof(ServerFilterTranspiler)));
		}

		public void Update()
		{
		}

		public static IEnumerable<CodeInstruction> ServerFilterTranspiler(IEnumerable<CodeInstruction> instructions)
		{
			foreach (CodeInstruction code in instructions)
			{
				if (code.LoadsField(AccessTools.Field(typeof(MyCachedServerItem), "Server")))
				{
					yield return code;
					yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patches), nameof(FixServerInfo)));
				}
				else
					yield return code;
			}
		}

		public static MyGameServerItem FixServerInfo(MyGameServerItem server)
		{
			if (string.IsNullOrWhiteSpace(server.Map))
				server.Map = server.Name;
			if (server.ServerVersion == 0)
				server.ServerVersion = MyFinalBuildConstants.APP_VERSION;
			return server;
		}
	}
}