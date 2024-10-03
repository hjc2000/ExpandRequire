using JCNET.Lua;
using NLua;

namespace ExpandRequire;

/// <summary>
///		lua 依赖展开帮手。
/// </summary>
internal static class LuaRequireExpandingHelper
{
	/// <summary>
	///		将当前路径下的 single_lua.lua 的依赖展开。
	/// </summary>
	/// <exception cref="Exception"></exception>
	public static void ExpandRequire()
	{
		string? lua_libs_path = Environment.GetEnvironmentVariable("lua_libs");
		if (lua_libs_path is null)
		{
			throw new Exception("不存在环境变量：lua_libs");
		}

		LuaWorkspace workspace = new("F:/repos/ElectricBatch/");
		workspace.RequiredModuleSearchPaths.Add(lua_libs_path);

		LuaWorkspaceContent workspace_content = workspace.GetContent();
		workspace_content.SigleContent.ExpandRequire();
		workspace_content.SigleContent.ToString().Output();

		Lua lua = workspace.GetLuaVm();
		lua.DoString(workspace_content.OtherFileContents);
		Dictionary<string, object> globals = lua.GetCustomGlobalTableContentsRecurse();
		foreach (KeyValuePair<string, object> pair in globals)
		{
			Console.WriteLine(pair.Key);
		}
	}

	/// <summary>
	///		将 lua 代码内容字符串写入输出文件 out.lua 中。
	///		<br/>* out.lua 始终是新建的空白文件。
	/// </summary>
	/// <param name="lua_code_content"></param>
	private static void Output(this string lua_code_content)
	{
		if (!Directory.Exists("out"))
		{
			Directory.CreateDirectory("out");
		}

		using FileStream out_file = File.Open("out/out.lua", FileMode.Create,
			FileAccess.ReadWrite, FileShare.Read);

		using StreamWriter writer = new(out_file);
		writer.Write(lua_code_content);
	}
}
