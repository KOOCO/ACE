using DG.Tweening;
using QRCodeShareMain;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateTransactionManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField]
    private Image walletQR_Img, addressQR_Img;
    [SerializeField]
    private Button walletQR_Btn, addressQR_Btn;
    [SerializeField]
    private RectTransform walletContent, addressContent;
    [SerializeField]
    private Transform walletArrow, addressArrow;
    [SerializeField]
    private TextMeshProUGUI address_Txt;
    [SerializeField]
    private Button copy_Btn;

    private float walletHeight = 200, addressHeight = 210;
    private TabName currentTab;
    private void Start()
    {
        EventListener();
    }
    private void EventListener()
    {
        walletQR_Btn.onClick.AddListener(() =>
        {
            if(currentTab != TabName.Wallet)
            {
                walletContent.DOSizeDelta(new Vector2(358, walletHeight), 0.5f).SetEase(Ease.InOutCubic);
                addressContent.DOSizeDelta(new Vector2(358, 0), 0.5f).SetEase(Ease.InOutCubic);
                walletArrow.DORotate(new Vector3(180, 0, 0), 0.5f).SetEase(Ease.InOutCubic);
                addressArrow.DORotate(new Vector3(0, 0, 0), 0.5f).SetEase(Ease.InOutCubic);
                currentTab = TabName.Wallet;
            }
            else
            {
                walletContent.DOSizeDelta(new Vector2(358, 0), 0.5f).SetEase(Ease.InOutCubic);
                walletArrow.DORotate(new Vector3(0, 0, 0), 0.5f).SetEase(Ease.InOutCubic);
                currentTab = default;
            }
        });
        addressQR_Btn.onClick.AddListener(() =>
        {
            if(currentTab!= TabName.Address)
            {
                walletContent.DOSizeDelta(new Vector2(358, 0), 0.5f).SetEase(Ease.InOutCubic);
                addressContent.DOSizeDelta(new Vector2(358, addressHeight), 0.5f).SetEase(Ease.InOutCubic);
                walletArrow.DORotate(new Vector3(0, 0, 0), 0.5f).SetEase(Ease.InOutCubic);
                addressArrow.DORotate(new Vector3(180, 0, 0), 0.5f).SetEase(Ease.InOutCubic);
                currentTab = TabName.Address;
            }
            else
            {
                addressContent.DOSizeDelta(new Vector2(358, 0), 0.5f).SetEase(Ease.InOutCubic);
                addressArrow.DORotate(new Vector3(0, 0, 0), 0.5f).SetEase(Ease.InOutCubic);
                currentTab = default;
            }
            
        });
        copy_Btn.onClick.AddListener(() =>
        {
#if UNITY_EDITOR
            TextEditor editor = new TextEditor
            {
                text = address_Txt.text
            };
            editor.SelectAll();
            editor.Copy();
#endif

            JSBridgeManager.Instance.CopyString(address_Txt.text);
            ViewManager.Instance.OpenTipMsgView(transform, messageStatus.Succesful, LanguageManager.Instance.GetText("Copy Success!"));
        });
    }
    public void SetQRcode()
    {
        string content = "tron:THq3gnaRJLuQNt3vDsZc3qaw2aFBoAjPf4?token=TR7NHqjeKQxGTCi8q8ZY4pL8otSzgjLj6t&amount=100";
        GenerateQRCode(walletQR_Img, content);
        content = "THq3gnaRJLuQNt3vDsZc3qaw2aFBoAjPf4";
        address_Txt.text = content;
        GenerateQRCode(addressQR_Img, content);
    }
    private void GenerateQRCode(Image image, string text)
    {
        QRImageProperties properties = new QRImageProperties(500, 500, 50);
        Texture2D QRCodeImage = QRCodeShare.CreateQRCodeImage(text, properties);
        if (QRCodeImage != null)
        {
            ShowImage(image, QRCodeImage);
        }
    }
    private void ShowImage(Image showImage, Texture2D image)
    {
        showImage.sprite = ImageProcessing.ConvertTexture2DToSprite(image);
        float imageSize = Mathf.Max(showImage.GetComponent<RectTransform>().sizeDelta.x, showImage.GetComponent<RectTransform>().sizeDelta.y);

        showImage.GetComponent<RectTransform>().sizeDelta = image.width <= image.height ?
            new Vector2(imageSize / image.height * image.width, imageSize) :
            new Vector2(imageSize, imageSize * image.height / image.width);
    }
}
enum TabName
{
    None,
    Wallet,
    Address,
}
