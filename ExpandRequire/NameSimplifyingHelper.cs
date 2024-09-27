using JCNET.字符串处理;

namespace ExpandRequire;

/// <summary>
///		名称化简
/// </summary>
internal static class NameSimplifyingHelper
{
	/// <summary>
	///		化简 lua_code_content 中所有函数的名称。
	/// </summary>
	/// <param name="lua_code_content"></param>
	/// <returns></returns>
	public static string SimplifyFunctionName(this string lua_code_content)
	{
		StringReader reader = new(lua_code_content);
		HashSet<string> name_set = lua_code_content.CollectFunctionName();
		foreach (string name in name_set)
		{
			lua_code_content = lua_code_content.ReplaceTwoWord("function", name,
				$"{name} = function").ToString();
		}

		ReadOnlyMemory<char> memory = lua_code_content.AsMemory();
		foreach (string name in name_set)
		{
			ulong index = _name_index++;
			Console.WriteLine($"{name} => {index}");
			memory = memory.ReplaceWholeMatch(name, $"G[{index}]");
		}

		memory = memory.ReplaceTwoWord("local", "G", "G");
		lua_code_content = memory.ToString();
		return lua_code_content;
	}

	private static ulong _name_index = 0;

	/// <summary>
	///		收集 lua_code_content 中定义的所有函数的名称。
	///		<br/>* 必须是通过 function FunctionName() end 这种格式定义的函数，否则不会被收集。
	/// </summary>
	/// <param name="lua_code_content"></param>
	/// <returns></returns>
	private static HashSet<string> CollectFunctionName(this string lua_code_content)
	{
		StringReader reader = new(lua_code_content);
		HashSet<string> name_set = [];
		while (true)
		{
			string? line = reader.ReadLine();
			if (line is null)
			{
				break;
			}

			string function_name = line.GetBetween("function", "(").Trim().ToString();
			if (function_name != string.Empty)
			{
				name_set.Add(function_name);
			}
		}

		return name_set;
	}
}
