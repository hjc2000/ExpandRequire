using ExpandRequire;

string lua_content = @"require(""Servo.EI"")
require(""Servo.Param"")
require(""Servo.EO"")
require(""Servo.Core"")
require(""Servo.Feedback"")
require(""Servo.Mode7"")
require(""Servo.Monitor"")
require(""Detector.AccelerationDetector"")
";

while (true)
{
	string? lua_file_path = lua_content.ParseFirstRequiredModulePath();
	if (lua_file_path is null)
	{
		lua_content = lua_content.TrimEmptyLine();
		Console.WriteLine(lua_content);
		return;
	}

	using FileStream fs = File.OpenRead(lua_file_path);
	using StreamReader sr = new(fs);
	string required_module_content = sr.ReadToEnd();
	lua_content = lua_content.RemoveFirstRequiredModule();
	lua_content = $"{required_module_content}\r\n{lua_content}";
}
