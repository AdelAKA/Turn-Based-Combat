using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

namespace GP7.Prodigy.Combat
{
    public class OkMessageBox : MessageBox
    {
        [SerializeField] private Button okButton;
        [SerializeField] private TMP_Text okButtonLabel;

        public static OkMessageBox Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else Destroy(gameObject);
        }

        private void _Show(string title, string content, string okLabel = null, UnityAction okAction = null)
        {

        
            canvas.enabled = true;
            this.content.text = content;
            this.title.text = title;
            IsShowingMassageBox = true;
            okButtonLabel.text = okLabel;

            okButton.onClick.RemoveAllListeners();

            okButton.onClick.AddListener(
                () =>
                {
                    if (IsShowingMassageBox)
                    {
                        Hide();
                        okAction?.Invoke();
                    }
                }
            );

           
        }

        // =================== STATIC FUNCTIONS ============================

        public static void Show(string title,string content, string okLabel = null, UnityAction okAction = null)
        {
            Instance._Show(title, content, okLabel, okAction);

        }
        //public static void Show(LocalizationToken title, LocalizationToken content, LocalizationToken okLabel = LocalizationToken.None, UnityAction okAction = null)
        //{
        //    Instance._Show(
        //        LocalizationManager.Get(title),
        //        LocalizationManager.Get(content),
        //        okLabel == LocalizationToken.None ? null : LocalizationManager.Get(okLabel), 
                
        //        okAction
        //    );
        //}
    }
}