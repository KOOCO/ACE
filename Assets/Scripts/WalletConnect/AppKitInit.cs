using UnityEngine;
using Reown.AppKit.Unity;
using Reown.Core.Common.Logging;
using Reown.Sign.Unity;


namespace WalletCoonect
{
    public class AppKitIit : MonoBehaviour
    {
        public GameObject obj;
        public async void Start()
        {
            ReownLogger.Instance = new UnityLogger();

            // The very basic configuration of SIWE
            var siweConfig = new SiweConfig
            {
                GetMessageParams = () => new SiweMessageParams
                {
                    Domain = "example.com",
                    Uri = "https://example.com/login"
                },
                SignOutOnChainChange = false
            };

            // Subscribe to SIWE events
            siweConfig.SignInSuccess += _ => Debug.Log("SIWE登入成功");
            siweConfig.SignOutSuccess += () => Debug.Log("SIWE登出成功");
            var appKitConfig = new AppKitConfig
            {
                // Project ID from https://cloud.reown.com/
                projectId = "494966a24ec973483487e3eb9c1c5a91",
                metadata = new Metadata(
                   "AppKit Unity",
                   "AppKit Unity Sample",
                   "https://reown.com",
                   "https://raw.githubusercontent.com/reown-com/reown-dotnet/main/media/appkit-icon.png",
                   new RedirectData
                   {
                       // Used by native wallets to redirect back to the app after approving requests
                       Native = "appkit-sample-unity://"
                   }
               ),
                // Assign the SIWE configuration created above. Can be null if SIWE is not used.
                siweConfig = siweConfig
            };
            if(!AppKit.IsInitialized) await AppKit.InitializeAsync(appKitConfig);
            obj.SetActive(true);

            Debug.Log($"[AppKit Init] AppKit initialized. Loading menu scene...");
        }
    }
}