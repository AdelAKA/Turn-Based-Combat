using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

namespace GP7.Prodigy.Combat
{
    public class YesNoMessageBox : MessageBox
    {
        public static YesNoMessageBox Instance { get; private set; }
        [SerializeField] private Button yesButton;
        [SerializeField] private TMP_Text yesButtonLabel;
       

        [SerializeField] private Button noButton;
        [SerializeField] private TMP_Text noButtonLabel;
        [SerializeField] private TMP_Text titleText;

        [SerializeField]
        Sprite GreenButtonSprite;
        [SerializeField]
        Sprite RedButtonSprite;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else Destroy(gameObject);

            
        }

        private void _Show(
            string title,
            string content,
            string yesLabel = null,
            string noLabel = null,
            UnityAction yesAction = null,
            UnityAction noAction = null,
            bool isYesPositive = true)
        {
            titleText.text = title;
            this.content.text = content;
            IsShowingMassageBox = true;
            noButtonLabel.text = noLabel;
            yesButtonLabel.text = yesLabel;
            canvas.enabled = true;
            noButton.onClick.RemoveAllListeners();
            yesButton.onClick.RemoveAllListeners();


            noButton.image.sprite= isYesPositive?RedButtonSprite:GreenButtonSprite;
            yesButton.image.sprite = isYesPositive ? GreenButtonSprite : RedButtonSprite;
            noButton.onClick.AddListener(
                () =>
                {
                    if (IsShowingMassageBox)
                    {
                        Hide();
                        noAction?.Invoke();
                    }
                }
            );

            yesButton.onClick.AddListener(
                () =>
                {
                    if (IsShowingMassageBox)
                    {
                        Hide();
                        yesAction?.Invoke();
                    }
                }
            );

          
        }

        // =================== STATIC FUNCTIONS ============================ //

        public static void Show(
             string title,
            string content,
           
            string yesLabel = null,
            string noLabel = null,
            UnityAction yesAction = null,
            UnityAction noAction = null)
        {
            Instance._Show(title, content, yesLabel, noLabel, yesAction, noAction);
        }

        //public static void Show(
        //    LocalizationToken title,
        //    LocalizationToken content,
        //    LocalizationToken yesToken = LocalizationToken.Yes,
        //    LocalizationToken noToken = LocalizationToken.No,
        //    UnityAction yesAction = null,
        //    UnityAction noAction = null,
        //    bool isYesPositive=true)
        //{
            

        //    Instance._Show(
        //         LocalizationManager.Get(title),
        //        LocalizationManager.Get(content),
        //        LocalizationManager.Get(yesToken),
        //        LocalizationManager.Get(noToken),
        //        yesAction,
        //        noAction,
        //        isYesPositive
        //    );
        //}
    }
}