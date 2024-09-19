using ExpandRequire;

string main_file_content = LuaRequireExpandingExtension.GetMainFileContent();
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
		if (main_file_content.Contains("require"))
		{
			throw new Exception("未展开干净");
		}

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
		main_file_content = $"{sr.ReadToEnd()}\r\n{main_file_content}";
	}
}

