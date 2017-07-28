public enum ENUM_HexDirection {
	NE = 0, 
	E, SE, SW, W, NW
}

public static class HexDirectionExtensions {
	public static ENUM_HexDirection Opposite(this ENUM_HexDirection p_direction) {
		return (int)p_direction < 3 ? (p_direction + 3) : (p_direction - 3);
	}

	public static ENUM_HexDirection Previous(this ENUM_HexDirection p_direction) {
		return p_direction == ENUM_HexDirection.NE ? ENUM_HexDirection.NW : (p_direction - 1);
	}

	public static ENUM_HexDirection Next(this ENUM_HexDirection p_direction) {
		return p_direction == ENUM_HexDirection.NW ? ENUM_HexDirection.NE : (p_direction + 1);
	}
}