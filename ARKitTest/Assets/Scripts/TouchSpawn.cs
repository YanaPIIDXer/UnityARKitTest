using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace ARKitTest
{
    /// <summary>
    /// タッチでオブジェクトを生成する。
    /// </summary>
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
    }
}
