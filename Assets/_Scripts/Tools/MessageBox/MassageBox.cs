using UnityEngine;
using TMPro;

namespace GP7.Prodigy.Combat
{
	[RequireComponent(typeof(Canvas))]
	
	public abstract class MessageBox : MonoBehaviour
	{
		[SerializeField] protected Canvas canvas;
	
		[SerializeField] protected TMP_Text content;

		[SerializeField] protected TMP_Text title;

		public bool IsShowingMassageBox { get; protected set; }

		public void Hide()
        {
           
            canvas.enabled = false;
            IsShowingMassageBox = false;
          
        }

	}
}