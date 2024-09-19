using JCNET.字符串处理;

namespace ExpandRequire;

internal static class LuaRequireExpandingExtension
{
	/// <summary>
	///		获取本可执行文件当前路径下的 main.lua 的内容。
	/// </summary>
	/// <returns></returns>
	public static string GetMainFileContent()
	{
#if DEBUG
		string main_file_path = "D:/repos/ElectricBatch/main.lua";
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
	public static string AddWorkspaceFiles(this string main_lua_file_content)
	{
		EnumerationOptions options = new()
		{
			RecurseSubdirectories = true,
		};

#if DEBUG
		string workspace_directory = "D:/repos/ElectricBatch";
#else
		string workspace_directory = Directory.GetCurrentDirectory();
#endif

		IEnumerable<string> lua_file_paths = Directory.EnumerateFiles(workspace_directory, "*", options);

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
			main_lua_file_content = $"{reader.ReadToEnd()}\r\n{main_lua_file_content}";
		}

		return main_lua_file_content;
	}

	/// <summary>
	///		解析第一条 require 指令所导入的模块。
	/// </summary>
	/// <param name="lua_code_content"></param>
	/// <returns></returns>
	public static string? ParseFirstRequiredModule(this string lua_code_content)
	{
		string require_path = lua_code_content.Cut(@"require(""", @""")");
		if (require_path != string.Empty)
		{
			return require_path;
		}

		require_path = lua_code_content.Cut(@"require('", @"')");
		if (require_path != string.Empty)
		{
			return require_path;
		}

		return null;
	}

	/// <summary>
	///		解析第一条 require 指令所导入的模块的路径。
	/// </summary>
	/// <param name="lua_code_content"></param>
	/// <returns></returns>
	/// <exception cref="Exception"></exception>
	public static string? ParseFirstRequiredModulePath(this string lua_code_content)
	{
		string? module = ParseFirstRequiredModule(lua_code_content);
		if (module is null)
		{
			return null;
		}

		module = module.Replace('.', '/');

		string? lua_libs_path = Environment.GetEnvironmentVariable("lua_libs");
		if (lua_libs_path is null)
		{
			throw new Exception("不存在环境变量：lua_libs");
		}

		module = $"{lua_libs_path}/{module}.lua";
		return module;
	}

	/// <summary>
	///		移除第一条 require 指令所导入的模块的所有 require 指令。
	///		<br/>* 例如第一条 require 导入了模块 A，则除了会将第一条 require 移除意外，
	///			   还会将其他地方的所有导入模块 A 的 require 移除掉。
	/// </summary>
	/// <param name="lua_code_content"></param>
	/// <returns></returns>
	public static string RemoveFirstRequiredModule(this string lua_code_content)
	{
		string? module = ParseFirstRequiredModule(lua_code_content);
		if (module is null)
		{
			return lua_code_content;
		}

		lua_code_content = lua_code_content.Replace($"require(\"{module}\")", null);
		lua_code_content = lua_code_content.Replace($"require('{module}')", null);
		lua_code_content = lua_code_content.Trim();
		return lua_code_content;
	}

	/// <summary>
	///		取出连续的 2 个空行和开头、结尾的空行。
	/// </summary>
	/// <param name="lua_code_content"></param>
	/// <returns></returns>
	public static string TrimEmptyLine(this string lua_code_content)
	{
		while (lua_code_content.Contains("\n\n"))
		{
			lua_code_content = lua_code_content.Replace("\n\n", "\n");
		}

		while (lua_code_content.Contains("\r\n\r\n"))
		{
			lua_code_content = lua_code_content.Replace("\r\n\r\n", "\r\n");
		}

		lua_code_content = lua_code_content.Trim();
		return lua_code_content;
	}

	/// <summary>
	///		将 lua 代码内容字符串写入输出文件 out.lua 中。
	///		<br/>* out.lua 始终是新建的空白文件。
	/// </summary>
	/// <param name="lua_code_content"></param>
	public static void Output(this string lua_code_content)
	{
		using FileStream out_file = File.Open("out.lua", FileMode.Create,
			FileAccess.ReadWrite, FileShare.Read);

		using StreamWriter writer = new(out_file);
		writer.Write(lua_code_content);
	}
}
