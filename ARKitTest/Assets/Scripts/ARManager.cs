using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

/// <summary>
/// ARマネージャ
/// </summary>
public class ARManager : MonoBehaviour
{
	/// <summary>
	/// SpawnするオブジェクトのPrefab
	/// </summary>
	[SerializeField]
	private GameObject SpawnPrefab = null;

	/// <summary>
	/// レイキャストマネージャ
	/// </summary>
	private ARRaycastManager RaycastManager = null;

	/// <summary>
	/// レイキャストの結果を放り込む配列
	/// </summary>
	private List<ARRaycastHit> RayResults = new List<ARRaycastHit>();

	protected void Awake()
	{
		RaycastManager = GetComponent<ARRaycastManager>();
	}

	protected void Update()
	{
		if(Input.touchCount <= 0) { return; }

		Touch Touched = Input.GetTouch(0);
		if(Touched.phase == TouchPhase.Ended) { return; }

		if(!RaycastManager.Raycast(Touched.position, RayResults)) { return; }

		GameObject.Instantiate(SpawnPrefab, RayResults[0].pose.position, RayResults[0].pose.rotation);
	}

}
