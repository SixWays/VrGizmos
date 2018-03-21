using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sigtrap {
	/// <summary>
	/// Calling any VrGizmo static function will add this to Camera.main.
	/// Or attach to a VR camera to manually enable VR gizmo drawing.
	/// </summary>
	[RequireComponent(typeof(Camera))]
	public class VrGizmos : MonoBehaviour {
		#region Consts and types
		const string SHADER = "Hidden/SigtrapKit/VrGizmoOverlay";
		
		private struct DrawCommand {
			Mesh _mesh;
			Matrix4x4 _matrix;
			Color _color;

			public DrawCommand(Mesh mesh, Color color, Matrix4x4 matrix){
				_mesh = mesh;
				_matrix = matrix;
				_color = color;
			}

			public void Draw(Material m){
				_color.a *= alpha;
				m.color = _color;
				m.SetPass(0);
				Graphics.DrawMeshNow(_mesh, _matrix);
			}
		}
		#endregion

		#region Static
		static List<VrGizmos> _drawers = new List<VrGizmos>();
		static Dictionary<PrimitiveType, Mesh> _meshes;

		static bool _initd = false;

		static void Init(){
			if (_initd) return;

			alpha = 1;

			_meshes = new Dictionary<PrimitiveType, Mesh>();
			foreach (PrimitiveType pt in EnumTools.EnumValues<PrimitiveType>()){
				GameObject go = GameObject.CreatePrimitive(pt);
				Mesh m = go.GetComponent<MeshFilter>().sharedMesh;
				Object.DestroyImmediate(go);
				_meshes.Add(pt, m);
			}

			_initd = true;
		}
		static bool AddDrawer(Camera cam){
			if (cam == null) return false;
			if (cam.stereoTargetEye != StereoTargetEyeMask.None){
				cam.gameObject.AddComponent<VrGizmos>();
				Debug.LogWarningFormat("Automatically added VrGizmo component to camera {0}", cam.name);
				return true;
			}
			return false;
		}

		static void Draw(PrimitiveType primitiveType, Color color, Matrix4x4 matrix){
			if (!active || !Application.isPlaying) return;
			if (_drawers.Count == 0){
				bool added = AddDrawer(Camera.main);
				if (!added){
					foreach (var cam in Camera.allCameras){
						added = AddDrawer(cam);
						if (added) break;
					}
				}
				if (!added){
					Debug.LogWarning("No VrGizmo components detected on cameras and no valid VR cameras found - nothing will be drawn");
				}
			}

			foreach (var d in _drawers){
				d._cmds.Add(new DrawCommand(_meshes[primitiveType], color, matrix));
			}
		}

		#region API
		public static float alpha;
		public static bool active = true;

		public static void DrawSphere(Vector3 position, float radius, Color color){
			Draw(
				PrimitiveType.Sphere, color,
				Matrix4x4.TRS(position, Quaternion.identity, Vector3.one * radius)
			);
		}
		/// <summary>
		/// Draw a sphere using Gizmos.color
		/// </summary>
		public static void DrawSphere(Vector3 position, float radius){
			DrawSphere(position, radius, Gizmos.color);
		}

		public static void DrawBox(Vector3 position, Quaternion rotation, Vector3 size, Color color){
			Draw(PrimitiveType.Cube, color, Matrix4x4.TRS(position, rotation, size));
		}
		/// <summary>
		/// Draw a box using Gizmos.color
		/// </summary>
		public static void DrawBox(Vector3 position, Quaternion rotation, Vector3 size){
			DrawBox(position, rotation, size, Gizmos.color);
		}
		#endregion
		#endregion

		#region Instance
		Material _mat;
		List<DrawCommand> _cmds = new List<DrawCommand>();

		void Awake(){
			Init();
			_drawers.Add(this);
			_mat = new Material(Shader.Find(SHADER));
			_mat.hideFlags = HideFlags.HideAndDontSave;
		}
		void OnDestroy(){
			_drawers.Remove(this);
			Destroy(_mat);
		}
		void OnPostRender(){
			foreach (var c in _cmds){
				c.Draw(_mat);
			}
			_cmds.Clear();
		}
		#endregion
	}
}