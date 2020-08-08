using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Collections.Generic;

namespace ARKitTest
{
    /// <summary>
    /// タッチでオブジェクトを生成する。
    /// </summary>
    [RequireComponent(typeof(ARPlaneManager))]
    [RequireComponent(typeof(ARRaycastManager))]
    public class TouchSpawn : MonoBehaviour
    {
        /// <summary>
        /// レイキャスト管理
        /// </summary>
        private ARRaycastManager Raycast = null;

        protected void Awake()
        {
            Raycast = GetComponent<ARRaycastManager>();
        }

        protected void Update()
        {
            if (Input.touchCount == 0) { return; }

            Touch Info = Input.GetTouch(0);
            if (Info.phase != TouchPhase.Began) { return; }

            List<ARRaycastHit> Hits = new List<ARRaycastHit>();
            if (!Raycast.Raycast(Info.position, Hits)) { return; }
            Debug.Log(Hits[0].pose.position);
        }
    }
}
