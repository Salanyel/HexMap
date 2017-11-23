using System.Collections;
using System.Collections.Generic;

public static class ListPool<T> {

	#region Variables

	static Stack<List<T>> _stack = new Stack<List<T>>();

	#endregion

	#region Methods

	public static List<T> Get() {
		if (_stack.Count > 0) {
			return _stack.Pop();
		}

		return new List<T>();
	}

	public static void Add(List<T> p_list) {
		p_list.Clear();
		_stack.Push (p_list);
	}

	#endregion
}