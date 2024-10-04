using JCNET.Lua;
using JCNET.字符串处理;
using NLua;
using System.Text;

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
	if (true)
	{
		string code = workspace_content.SigleContent.Code;
		code = code.ReplaceWholeMatch(name, $"G[{name_id++}]").ToString();
		workspace_content.SigleContent.Code = code;
	}
}

StringBuilder sb = new();
sb.AppendLine("local G={}");
sb.AppendLine(workspace_content.SigleContent.Code);
workspace.Output(sb.ToString());
