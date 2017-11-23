using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer (typeof(HexCoordinates))]
public class HexCoordinatesDrawer : PropertyDrawer {

	#region Unity_Methods

	public override void OnGUI (Rect p_position, SerializedProperty p_property, GUIContent p_label)
	{
		HexCoordinates hexCoordinates = new HexCoordinates (
			                                 p_property.FindPropertyRelative ("_x").intValue,
			                                 p_property.FindPropertyRelative ("_z").intValue
		                                 );

		p_position = EditorGUI.PrefixLabel (p_position, p_label);
		GUI.Label (p_position, hexCoordinates.ToString ());

	}

	#endregion

}