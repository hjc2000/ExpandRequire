using JCNET.字符串处理;
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
		string main_file_content = GetMainFileContent();
		main_file_content = main_file_content.AddWorkspaceFiles();

		// 已经导入过了的路径就放到这里，导入前查重，避免重复导入
		HashSet<string> imported_lua_path_set = [];
		while (true)
		{
			string? lua_file_path = main_file_content.ParseFirstRequiredModulePath();
			if (lua_file_path is null)
			{
				// 找不到 require 指令了
				main_file_content = main_file_content.TrimEmptyLine();
				main_file_content = $"{main_file_content}\r\n";
				main_file_content = main_file_content.SimplifyFunctionName();
				main_file_content = $"local G={{}}\r\n{main_file_content}";
				main_file_content.Output();
				return;
			}

			// 仍然找得到 require 指令
			main_file_content = main_file_content.RemoveFirstRequiredModule();
			if (!imported_lua_path_set.Contains(lua_file_path))
			{
				// 此路径的 lua 文件还没导入过
				imported_lua_path_set.Add(lua_file_path);
				using FileStream fs = File.OpenRead(lua_file_path);
				using StreamReader sr = new(fs);
				main_file_content = $"{sr.ReadToEnd().RemoveComment()}\r\n{main_file_content}";
				main_file_content = main_file_content.AddTitle(lua_file_path);
			}
		}
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
		string main_file_content = reader.ReadToEnd().RemoveComment();
		main_file_content = main_file_content.AddTitle("main");
		return main_file_content;
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
			main_lua_file_content = $"{reader.ReadToEnd().RemoveComment()}\r\n{main_lua_file_content}";
			main_lua_file_content = main_lua_file_content.AddTitle(path);
		}

		return main_lua_file_content;
	}

	/// <summary>
	///		删除注释
	/// </summary>
	/// <param name="lua_code_content"></param>
	/// <returns></returns>
	private static string RemoveComment(this string lua_code_content)
	{
		StringBuilder sb = new();
		StringReader reader = new(lua_code_content);
		while (true)
		{
			string? line = reader.ReadLine();
			if (line is null)
			{
				break;
			}

			if (line.Trim().StartsWith("--"))
			{
				continue;
			}

			sb.AppendLine(line);
		}

		return sb.ToString();
	}

	/// <summary>
	///		解析第一条 require 指令所导入的模块。
	/// </summary>
	/// <param name="lua_code_content"></param>
	/// <returns></returns>
	private static string? ParseFirstRequiredModule(this string lua_code_content)
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
	private static string? ParseFirstRequiredModulePath(this string lua_code_content)
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
	private static string RemoveFirstRequiredModule(this string lua_code_content)
	{
		string? module = ParseFirstRequiredModule(lua_code_content);
		if (module is null)
		{
			return lua_code_content;
		}

		CuttingMiddleResult cutting_middle_result = lua_code_content.CutMiddle($"require(\"{module}\")");
		if (cutting_middle_result.Success)
		{
			lua_code_content = cutting_middle_result.Left.Trim()
				+ "\r\n"
				+ cutting_middle_result.Right.Trim();
		}

		cutting_middle_result = lua_code_content.CutMiddle($"require('{module}')");
		if (cutting_middle_result.Success)
		{
			lua_code_content = cutting_middle_result.Left.Trim()
				+ "\r\n"
				+ cutting_middle_result.Right.Trim();
		}

		return lua_code_content;
	}

	/// <summary>
	///		取出连续的 2 个空行和开头、结尾的空行。
	/// </summary>
	/// <param name="lua_code_content"></param>
	/// <returns></returns>
	private static string TrimEmptyLine(this string lua_code_content)
	{
		while (lua_code_content.Contains("\n\n\n"))
		{
			lua_code_content = lua_code_content.Replace("\n\n\n", "\n\n");
		}

		while (lua_code_content.Contains("\r\n\r\n\r\n"))
		{
			lua_code_content = lua_code_content.Replace("\r\n\r\n\r\n", "\r\n\r\n");
		}

		lua_code_content = lua_code_content.Trim();
		return lua_code_content;
	}

	/// <summary>
	///		给内容添加标题
	/// </summary>
	/// <param name="content"></param>
	/// <param name="title"></param>
	/// <returns></returns>
	private static string AddTitle(this string content, string title)
	{
		content = "\r\n\r\n\r\n\r\n\r\n" +
			$"------------------------------------------------------\r\n" +
			$"-- {title}\r\n" +
			$"------------------------------------------------------\r\n" +
			$"{content}";

		return content;
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
