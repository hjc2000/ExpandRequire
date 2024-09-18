using JCNET.字符串处理;

namespace ExpandRequire;

internal static class LuaRequireExpandingExtension
{
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
			lua_code_content = lua_code_content.Replace("\n\n", null);
		}

		while (lua_code_content.Contains("\r\n\r\n"))
		{
			lua_code_content = lua_code_content.Replace("\r\n\r\n", null);
		}

		lua_code_content = lua_code_content.Trim();
		return lua_code_content;
	}
}
