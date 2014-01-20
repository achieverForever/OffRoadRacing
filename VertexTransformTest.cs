using UnityEngine;
using System.Collections;

public class VertexTransformTest : MonoBehaviour {

	public Transform testCube;
	Matrix4x4 modelMatrix, viewMatrix, projMatrix, viewportMatrix;

	Vector3 posInModelSpace;
	// Use this for initialization
	void Start () {
		Vector4 temp;
		posInModelSpace = new Vector3(0.0f, 0.0f, 0.0f);
		temp = new Vector4(posInModelSpace.x, posInModelSpace.y, posInModelSpace.z, 1.0f);
		Debug.LogError("Coordinates in Model Space: " + ToString(temp));

		modelMatrix = Matrix4x4.TRS(testCube.position, Quaternion.identity, new Vector3(1.0f, 1.0f, 1.0f));
		Vector3 posInWorldSpace = modelMatrix.MultiplyPoint(posInModelSpace);

		float w = Vector4.Dot(modelMatrix.GetRow(3), temp);
		temp = new Vector4(posInWorldSpace.x, posInWorldSpace.y, posInWorldSpace.z, w);
		Debug.LogError("Coordinates in World Space: " + ToString(temp));

		viewMatrix = camera.worldToCameraMatrix;
		Vector3 posInViewSpace = viewMatrix.MultiplyPoint(posInWorldSpace);
		temp = new Vector4(posInWorldSpace.x, posInWorldSpace.y, posInWorldSpace.z, w);
		w = Vector4.Dot(viewMatrix.GetRow(3), temp);
		temp = new Vector4(posInViewSpace.x, posInViewSpace.y, posInViewSpace.z, w);
		Debug.LogError("Coordinates in View Space: " + ToString(temp));
		Debug.LogError("(With camera position being (0,0,0) in world space)");

		projMatrix = camera.projectionMatrix;
		Vector3 posInClipSpace = projMatrix.MultiplyPoint(posInViewSpace);
		temp = new Vector4(posInViewSpace.x, posInViewSpace.y, posInViewSpace.z, w);
		w = Vector4.Dot(projMatrix.GetRow(3), temp);
		temp = new Vector4(posInClipSpace.x * w, posInClipSpace.y * w, posInClipSpace.z * w, w);
		Debug.LogError("Coordinates in Clip Space: " + ToString(temp));
		if( Mathf.Abs(temp.x) > w || Mathf.Abs(temp.y) > w || Mathf.Abs(temp.z) > w )
			Debug.LogError("Vertex outside of the view frustum, thus will be clipped away!");

		// temp = new Vector4(posInClipSpace.x, posInClipSpace.y, posInClipSpace.z, w);
		temp = temp / temp.w;
		Debug.LogError("Coordinates in Normalized Device Coordinates: " + ToString(temp));

	}
	
	// Update is called once per frame
	void Update () {
	
	}

	string ToString(Vector4 v)
	{
		return string.Format("({0:F2},{1:F2},{2:F2},{3:F2})", v.x, v.y, v.z, v.w);
	}
}
