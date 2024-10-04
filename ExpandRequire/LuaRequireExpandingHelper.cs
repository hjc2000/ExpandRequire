using JCNET.Lua;
using JCNET.字符串处理;
using NLua;
using System.Text;

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

#if DEBUG
		LuaWorkspace workspace = new("F:/repos/ElectricBatch/");
#else
		LuaWorkspace workspace = new(Directory.GetCurrentDirectory());
#endif

		workspace.RequiredModuleSearchPaths.Add(lua_libs_path);

		/* 设置好工作区路径和模块搜索路径后，利用工作区，构造出 LuaWorkspaceContent
		 * 和 lua 虚拟机对象。
		 */
		LuaWorkspaceContent workspace_content = workspace.GetContent();
		Lua lua = workspace.GetLuaVm();

		workspace_content.SigleContent.ExpandRequire();
		workspace_content.SigleContent.ChangeFunctionDefinitionFormat();

		// 执行非 main.lua 的代码
		lua.DoString(workspace_content.OtherFileContents);

		// 获取全局变量
		Dictionary<string, object> globals = lua.GetCustomGlobalTableContentsRecurse();
		List<string> global_variable_paths = [.. globals.Keys];
		StringLengthComparer comparer = new(StringLengthComparer.OrderEnum.FromLongToShort);
		global_variable_paths.Sort(comparer);

		ulong name_id = 0;
		foreach (string path in global_variable_paths)
		{
			// 去除开头的点号，得到全局变量名
			string name = path[1..];
			Console.WriteLine(name);
			workspace_content.SigleContent.Code = workspace_content.SigleContent.Code.ReplaceWholeMatch(name, $"G[{name_id++}]").ToString();
		}

		StringBuilder sb = new();
		sb.AppendLine("local G={}");
		sb.AppendLine(workspace_content.SigleContent.Code);
		sb.ToString().Output();
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

		using FileStream out_file = File.Open("out/out.lua",
			FileMode.Create,
			FileAccess.ReadWrite,
			FileShare.Read);

		using StreamWriter writer = new(out_file);
		writer.Write(lua_code_content);
	}
}
