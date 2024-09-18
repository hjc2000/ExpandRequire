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

string? lua_file_path = LuaRequireParser.ParseRequiredModulePath(lua_content);
Console.WriteLine(lua_file_path);

if (lua_file_path is null)
{
	Console.WriteLine("没有解析到 lua 文件路径");
	return;
}

using FileStream fs = File.OpenRead(lua_file_path);
using StreamReader sr = new(fs);
Console.WriteLine(sr.ReadToEnd());
