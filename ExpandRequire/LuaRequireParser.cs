using JCNET.字符串处理;

namespace ExpandRequire;

internal static class LuaRequireParser
{
	/// <summary>
	///		解析 require 指令所导入的模块。
	///		只能解析出第一条 require。
	/// </summary>
	/// <param name="lua_code_content"></param>
	/// <returns></returns>
	public static string? ParseRequiredModule(string lua_code_content)
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

	public static string? ParseRequiredModulePath(string lua_code_content)
	{
		string? module = ParseRequiredModule(lua_code_content);
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
}
