using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GP7.Prodigy.Combat
{
    public class Testing : MonoBehaviour
    {
        [SerializeField] Transform targetTransform;
        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                QueuedNotificationManager.Instance.QueueNotification("testing", targetTransform, Vector3.up, Vector3.one, Color.white);
            }
        }
    }
}
