string? lua_libs_path = Environment.GetEnvironmentVariable("lua_libs");
if (lua_libs_path is null)
{
	Console.WriteLine("不存在环境变量：lua_libs");
}

Console.WriteLine($"从环境变量 lua_libs 中得到 lua 库路径：{lua_libs_path}");
