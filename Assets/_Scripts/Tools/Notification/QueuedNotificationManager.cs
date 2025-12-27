using System.Collections.Generic;
using UnityEngine;

namespace GP7.Prodigy.Combat
{
    public class QueuedNotificationManager : MonoBehaviour
    {
        public static QueuedNotificationManager Instance { get; set; }

        [SerializeField] FloatingNotification floatingNotificationPrefab;
        [SerializeField] float notificationIntervales;
        [SerializeField] bool isDebug;

        private Dictionary<Transform, Queue<NotificationInfo>> queuedNotificationsDictionary = new Dictionary<Transform, Queue<NotificationInfo>>();
        private List<Transform> busyTransformTarget = new List<Transform>();

        struct NotificationInfo
        {
            public NotificationInfo(string text, Vector3 textPosition, Vector3 textScale, Color textColor)
            {
                this.text = text;
                this.textPosition = textPosition;
                this.textScale = textScale;
                this.textColor = textColor;
            }

            public string text;
            public Vector3 textPosition;
            public Vector3 textScale;
            public Color textColor;
        }

        private void Awake()
        {
            Instance = this;
        }

        public void QueueNotification(string text, Transform targetTransform, Vector3 offset, Vector3 textScale, Color textColor)
        {
            if (!queuedNotificationsDictionary.ContainsKey(targetTransform))
            {
                if (isDebug) Debug.Log($"Adding {targetTransform} to dict");
                queuedNotificationsDictionary.Add(targetTransform, new Queue<NotificationInfo>());
            }

            queuedNotificationsDictionary[targetTransform].Enqueue(new NotificationInfo(text, targetTransform.transform.position + offset, textScale, textColor));
            if (isDebug) Debug.Log($"Enqueued {text}");
            TryInvokeNotification(targetTransform);
            //TextPopup textPopup = Instantiate(editableTextPopupPrefab, textPosition, Quaternion.identity);
            //textPopup.SetUp(text, textScale, relatedCamera, textColor);
        }

        private async void TryInvokeNotification(Transform targetTransform)
        {
            if (busyTransformTarget.Contains(targetTransform)) return;

            busyTransformTarget.Add(targetTransform);
            while (queuedNotificationsDictionary[targetTransform].Count > 0)
            {
                NotificationInfo info = queuedNotificationsDictionary[targetTransform].Dequeue();
                FloatingNotification notification = Instantiate(floatingNotificationPrefab, info.textPosition, Quaternion.identity);
                notification.SetUp(info.text, info.textScale, info.textColor);
                if (isDebug) Debug.Log($"notification {targetTransform.name}, {info.text}", targetTransform);
                await Awaitable.WaitForSecondsAsync(notificationIntervales);
                if (isDebug) Debug.Log(queuedNotificationsDictionary[targetTransform].Count);
            }
            busyTransformTarget.Remove(targetTransform);
        }
    }
}
