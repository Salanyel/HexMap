namespace HexMapGenerator {

public enum ENUM_HexDirection {
	NE = 0, 
	E, SE, SW, W, NW
}

public static class ENUM_HexDirectionExtensions {
	public static ENUM_HexDirection Opposite(this ENUM_HexDirection p__direction) {
		return (int)p__direction < 3 ? (p__direction + 3) : (p__direction - 3);
	}

	public static ENUM_HexDirection Previous(this ENUM_HexDirection p__direction) {
		return p__direction == ENUM_HexDirection.NE ? ENUM_HexDirection.NW : (p__direction - 1);
	}

	public static ENUM_HexDirection Next(this ENUM_HexDirection p__direction) {
		return p__direction == ENUM_HexDirection.NW ? ENUM_HexDirection.NE : (p__direction + 1);
	}

	public static ENUM_HexDirection Previous2 (this ENUM_HexDirection p_direction) {
		p_direction -= 2;
		return p_direction >= ENUM_HexDirection.NE ? p_direction : (p_direction + 6);
	}

	public static ENUM_HexDirection Next2 (this ENUM_HexDirection p_direction) {
		p_direction += 2;
		return p_direction <= ENUM_HexDirection.NW ? p_direction : (p_direction - 6);
	}
}
}