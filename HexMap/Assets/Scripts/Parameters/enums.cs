public enum ENUM_HexDirection {
	NE = 0, 
	E, SE, SW, W, NW
}

public static class HexDirectionExtensions {
	public static ENUM_HexDirection Opposite(this ENUM_HexDirection p_direction) {
		return (int)p_direction < 3 ? (p_direction + 3) : (p_direction - 3);
	}
}