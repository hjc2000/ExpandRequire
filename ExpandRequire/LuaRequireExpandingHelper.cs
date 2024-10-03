using JCNET.Lua;
using System.Text;

namespace ExpandRequire;

/// <summary>
///		lua 依赖展开帮手。
/// </summary>
internal static class LuaRequireExpandingHelper
{
	/// <summary>
	///		将当前路径下的 main.lua 的依赖展开。
	/// </summary>
	/// <exception cref="Exception"></exception>
	public static void ExpandRequire()
	{
		string? lua_libs_path = Environment.GetEnvironmentVariable("lua_libs");
		if (lua_libs_path is null)
		{
			throw new Exception("不存在环境变量：lua_libs");
		}

		string main_file_content = GetMainFileContent();
		main_file_content = main_file_content.AddWorkspaceFiles();
		Console.WriteLine(main_file_content);
		LuaCodeContent main = new(main_file_content, [lua_libs_path]);
		main.ExpandRequire();
		main.ToString().Output();
	}

	/// <summary>
	///		获取本可执行文件当前路径下的 main.lua 的内容。
	/// </summary>
	/// <returns></returns>
	private static string GetMainFileContent()
	{
#if DEBUG
		string main_file_path = "F:/repos/ElectricBatch/main.lua";
#else
		string main_file_path = "main.lua";
#endif

		using FileStream main_file = File.Open(main_file_path, FileMode.Open,
			FileAccess.ReadWrite, FileShare.Read);

		using StreamReader reader = new(main_file);
		return reader.ReadToEnd();
	}

	/// <summary>
	///		添加工作区中的其他 lua 文件到 main_lua_file_content 中。
	/// </summary>
	/// <param name="main_lua_file_content">main.lua 的内容</param>
	/// <returns>返回 main_lua_file_content 添加了工作区中其他 lua 文件后的字符串。</returns>
	private static string AddWorkspaceFiles(this string main_lua_file_content)
	{
		EnumerationOptions options = new()
		{
			RecurseSubdirectories = true,
		};

#if DEBUG
		string workspace_directory = "F:/repos/ElectricBatch";
#else
		string workspace_directory = Directory.GetCurrentDirectory();
#endif

		IEnumerable<string> lua_file_paths = Directory.EnumerateFiles(workspace_directory,
			"*", options);

		StringBuilder sb = new();
		foreach (string path in lua_file_paths)
		{
			string? directory = Path.GetDirectoryName(path);
			if (directory is null)
			{
				continue;
			}

			if (directory.EndsWith("out"))
			{
				// out 目录下的文件不被收集
				continue;
			}

			if (path.EndsWith("main.lua"))
			{
				continue;
			}

			string? extension = Path.GetExtension(path);
			if (extension is null || extension != ".lua")
			{
				// 排除不是 .lua 的文件
				continue;
			}

			Console.WriteLine(path);
			using FileStream fs = File.OpenRead(path);
			using StreamReader reader = new(fs);
			sb.AppendLine(reader.ReadToEnd());
		}

		sb.AppendLine(main_lua_file_content);
		return sb.ToString();
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
