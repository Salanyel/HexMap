using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HexMapGenerator {
	
public class PathForMap : MonoBehaviour {

		#region Variables

		GameObject _parent;

		#endregion

		#region Unity

		void Awake() {
			_parent = transform.parent.gameObject;
		}

		#endregion

		#region Methods

		public void OnClick() {
			float x = _parent.GetComponent<RectTransform> ().sizeDelta.x;
			float y = _parent.GetComponent<RectTransform> ().sizeDelta.y;

			if (x < 100) {
				x = 250;
			} else {
				x = 24;
			}

			if (y < 50) {
				y = 100;
			} else {
				y = 24;
			}

			_parent.GetComponent<RectTransform> ().sizeDelta = new Vector2 (x, y);
		}

		#endregion

}

}