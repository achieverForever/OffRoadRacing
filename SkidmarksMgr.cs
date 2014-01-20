using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshFilter))]
public class SkidmarksMgr : MonoBehaviour {

	public float sectionLength = 0.8f;			// Set this to the length of a skidmark section.
	public float tireWidth = 0.6f;			// Set this to the width of the tires.
	public float distanceToGround = 0.02f;
	public int maxSectionsNum = 1024;		// Maximum number of skidmarks.

	class Section{
		public Vector3 hitPoint = Vector3.zero;
		public Vector3 posl = Vector3.zero;
		public Vector3 posr = Vector3.zero;
		public Vector3 normal = Vector3.zero;
		public float intensity = 1.0f;
		public bool ended = false;
	}

	private Section[] _sections = null;
	private int _last = -1, _curr = 0, _sectionCount = 0, _quadCount = 0;
	private bool _updated = false;
	private float _lastAddTime;

	// Use this for initialization
	void Start () {
		InitSkidmarks();
		_sectionCount = 0;
		_lastAddTime = Time.time;
	}
	
	// Update is called once per frame
	void LateUpdate () {
		UpdateSkidmarks();
	}


	public void InitSkidmarks()
	{
		_sections = new Section[maxSectionsNum];
		for(int i=0; i<maxSectionsNum; i++)
			_sections[i] = new Section();
	}

	// Gather information needed to generate a skidmark mesh.
	public void AddSkidmarks(Vector3 hitPoint, Vector3 normal, Vector3 sidewayDir, float intensity)
	{
		intensity = Mathf.Clamp01(intensity);
		if(_sectionCount == 0)
		{
			_curr = _last = 0;

			hitPoint += normal * distanceToGround;		// Lift the skidmark a bit higher than the ground.
			_sections[_curr].hitPoint = hitPoint;
			_sections[_curr].posl = hitPoint-sidewayDir*.5f*tireWidth;
			_sections[_curr].posr = hitPoint+sidewayDir*.5f*tireWidth;
			_sections[_curr].normal = normal;
			_sections[_curr].intensity = intensity;

			_sectionCount++;		
			_updated = true;
			_lastAddTime = Time.time;
			print("AddSkidmarks Called " + _curr + "  " + _last);
		}else if( (hitPoint - _sections[_last].hitPoint).sqrMagnitude >= sectionLength*sectionLength )
		{
			_last = _curr;
			_curr = (_curr + 1) % maxSectionsNum;

			hitPoint += normal * distanceToGround;		// Lift the skidmark a bit higher than the ground.
			_sections[_curr].hitPoint = hitPoint;
			_sections[_curr].posl = hitPoint-sidewayDir*.5f*tireWidth;
			_sections[_curr].posr = hitPoint+sidewayDir*.5f*tireWidth;
			_sections[_curr].normal = normal;
			_sections[_curr].intensity = intensity;
			if(Time.time - _lastAddTime >= 0.5f)
			{
				_sections[_curr].ended = true;
				_quadCount--;
			}

			_sectionCount++;
			_quadCount++;		
			_updated = true;
			_lastAddTime = Time.time;
			print("AddSkidmarks Called " + _curr + "  " + _last);

		}
	}

	void UpdateSkidmarks()
	{
		// Generate a new mesh if update is needed.
		if(_updated)
		{
			_updated = false;
			Mesh mesh = GetComponent<MeshFilter>().mesh;
			mesh.Clear();

			if(_sectionCount < 2)	// We need at least 2 sections to build the mesh.
				return;
			
			Vector3[] vertices = new Vector3[2 * _sectionCount];
			Vector2[] uvs = new Vector2[2 * _sectionCount];
			Vector3[] normals = new Vector3[2 * _sectionCount];
			Color[] colors = new Color[2 * _sectionCount];
			int[] triangles = new int[_quadCount * 6];

			Matrix4x4 worldToLocal = transform.worldToLocalMatrix;
			for(int i=0; i<_sectionCount; i++)	// Generate vertices and uvs.
			{
				vertices[i*2+0] = worldToLocal.MultiplyPoint(_sections[i].posl);
				vertices[i*2+1] = worldToLocal.MultiplyPoint(_sections[i].posr);

				if( (i+1)%2 == 1){
					uvs[i*2] = new Vector2(0.0f, 1.0f);
					uvs[i*2+1] = new Vector2(1.0f, 1.0f);					
				}else{
					uvs[i*2] = new Vector2(0.0f, 0.0f);
					uvs[i*2+1] = new Vector2(1.0f, 0.0f);
				}

				normals[i*2+0] = _sections[i].normal;
				normals[i*2+1] = _sections[i].normal;
				colors[i*2+0] = new Color(0.0f, 0.0f, 0.0f, _sections[i].intensity);
				colors[i*2+1] = new Color(0.0f, 0.0f, 0.0f, _sections[i].intensity);
			}
			// Generate triangle indices.
			int j, basis;
			int brokenCount = 0;
			for(j=0; j < _sectionCount; j++)
			{
				int quadIndex;
				if(_sections[j].ended)
					brokenCount++;
				quadIndex = j - brokenCount;

				basis = (quadIndex + brokenCount) * 2;
				triangles[quadIndex*6+0] = basis;  
				triangles[quadIndex*6+1] = basis + 3;  
				triangles[quadIndex*6+2] = basis + 1;

				triangles[quadIndex*6+3] = basis;  
				triangles[quadIndex*6+4] = basis + 2;  
				triangles[quadIndex*6+5] = basis + 3;  

			}
			mesh.vertices = vertices;
			mesh.normals = normals;
			mesh.colors = colors;
			mesh.uv = uvs;
			mesh.triangles = triangles;
		}
	}

}
