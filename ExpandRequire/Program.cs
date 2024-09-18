using ExpandRequire;

string lua_code_content = @"require(""Servo.EI"")
require(""Servo.Param"")
require(""Servo.EO"")
require(""Servo.Core"")
require(""Servo.Feedback"")
require(""Servo.Mode7"")
require(""Servo.Monitor"")
require(""Detector.AccelerationDetector"")
";

// 已经导入过了的路径就放到这里，导入前查重，避免重复导入
HashSet<string> imported_lua_path_set = [];

while (true)
{
	string? lua_file_path = lua_code_content.ParseFirstRequiredModulePath();
	if (lua_file_path is null)
	{
		// 找不到 require 指令了
		lua_code_content = lua_code_content.TrimEmptyLine();
		Console.WriteLine(lua_code_content);
		if (lua_code_content.Contains("require"))
		{
			throw new Exception("未展开干净");
		}

		return;
	}

	// 仍然找得到 require 指令
	lua_code_content = lua_code_content.RemoveFirstRequiredModule();
	if (!imported_lua_path_set.Contains(lua_file_path))
	{
		// 此路径的 lua 文件还没导入过
		imported_lua_path_set.Add(lua_file_path);
		using FileStream fs = File.OpenRead(lua_file_path);
		using StreamReader sr = new(fs);
		lua_code_content = $"{sr.ReadToEnd()}\r\n{lua_code_content}";
	}
}
