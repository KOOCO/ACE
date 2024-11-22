mergeInto(LibraryManager.library, {

    //獲取玩家IP位置
    JS_GetPlayerIPAddress: function() {
        fetch('https://api.ipify.org?format=json')
            .then(response => response.json())
            .then(data => {
                window.unityInstance.SendMessage('Entry', 'GetPlayerIPAddressCallback', data.ip);
            })
            .catch(error => {
                console.error('Error fetching IP address:', error);
                window.unityInstance.SendMessage('Entry', 'GetPlayerIPAddressCallback', "error");
            });
    },

    //清除URL資料
    JS_ClearUrlQueryString: function() {
        // 获取当前的URL
        var url = window.location.href;

        // 如果URL中包含查询字符串，则清除它
        if (url.indexOf('?') > -1) {
            // 使用history.replaceState来替换当前的URL
            var newUrl = url.split('?')[0];
            window.history.replaceState({}, document.title, newUrl);
        }
    },

    //分享
    JS_Share: function(titleStr, contentStr, urlStr){
        var title = UTF8ToString(titleStr);
        var content = UTF8ToString(contentStr);
        var url = UTF8ToString(urlStr);

        if (navigator.share) {
            navigator.share({
                title: title,                          //示例標題
                text: content,                         //示例文本。
                url: url          
            }).then(() => {
                console.log('分享成功');
            }).catch((error) => {
                console.log('分享失敗', error);
            });
        } else {
            alert('分享不支持這個瀏覽器');
        }
    },

    //複製文字
    JS_CopyString: function(strPtr) {
        var str = UTF8ToString(strPtr);
        var textarea = document.createElement("textarea");
        textarea.value = str;
        document.body.appendChild(textarea);
        textarea.select();
        document.execCommand("copy");
        document.body.removeChild(textarea);
        console.log("Copied text: " + str);
    },

    //本地頁面跳轉
    JS_LocationHref: function(url){
        window.location.href = UTF8ToString(url);
    },

    //關閉頁面
    JS_WindowClose: function(){
        window.open('url', '_self', '');
        window.close();
    },


    //重新整理頁面
    JS_Reload: function() {
        window.location.reload();
    },

    //斷開連接
    JS_WindowDisconnect: function() {
        if (typeof window.ethereum !== 'undefined') {
            if (window.ethereum.isConnected()) {
                window.ethereum
                    .request({ method: 'eth_requestAccounts' })
                    .then(() => {
                        window.ethereum.disconnect();
                        console.log("錢包连接已断开");
                    })
                    .catch((e) => {
                        console.error(e);
                    });
            } 
        }
    },

    //撤銷權限
    JS_RevokePermissions: function() {
        async function Revoke() {
            try {
                await window.ethereum.request({
                    method: 'wallet_revokePermissions',
                    params: [{ eth_accounts: {}, },],
                });
                console.log('權限已移除');
            } catch (error) {
                console.error('權限移除錯誤:' + error);
            }
        }

        Revoke();        
    },

    //獲取瀏覽器訊息
    JS_GetBrowserInfo: function(){
        window.unityInstance.SendMessage('Entry', 'HtmlDebug', 'GetBrowserInfo');
        const userAgent = navigator.userAgent;
        const appName = navigator.appName;
        const appVersion = navigator.appVersion;
        const platform = navigator.platform;
        const language = navigator.language;

        window.unityInstance.SendMessage('Entry', 'HtmlDebug', 'userAgent:'+userAgent);
        window.unityInstance.SendMessage('Entry', 'HtmlDebug', 'appName:'+appName);
        window.unityInstance.SendMessage('Entry', 'HtmlDebug', 'appVersion:'+appVersion);
        window.unityInstance.SendMessage('Entry', 'HtmlDebug', 'platform:'+platform);
        window.unityInstance.SendMessage('Entry', 'HtmlDebug', 'language:'+language);
        
        //const userAgent = navigator.userAgent;
        let browserName, fullVersion;

        if (userAgent.indexOf("Chrome") > -1) {
            browserName = "Chrome";
            fullVersion = userAgent.substring(userAgent.indexOf("Chrome") + 7);
            fullVersion = fullVersion.substring(0, fullVersion.indexOf(" "));
        } else if (userAgent.indexOf("Safari") > -1) {
            browserName = "Safari";
            fullVersion = userAgent.substring(userAgent.indexOf("Version") + 8);
            fullVersion = fullVersion.substring(0, fullVersion.indexOf(" "));
        } else if (userAgent.indexOf("Firefox") > -1) {
            browserName = "Firefox";
            fullVersion = userAgent.substring(userAgent.indexOf("Firefox") + 8);
        } else if (userAgent.indexOf("MSIE") > -1 || !!document.documentMode) {
            browserName = "IE";
            fullVersion = userAgent.substring(userAgent.indexOf("MSIE") + 5);
            fullVersion = fullVersion.substring(0, fullVersion.indexOf(" "));
        } else {
            browserName = "Unknown";
            fullVersion = "Unknown";
        }      
    },

    //離開預設瀏覽器開啟chrome瀏覽器
    JS_OpenNewBrowser: function(mailStr, igIdAndName){
        const _linemail = UTF8ToString(mailStr);
        const _igIdAndName = UTF8ToString(igIdAndName);
        const targetUrl = `${window.callbackUrl}?` +
                          `linemail=${encodeURIComponent(_linemail)}&` +
                          `igIdAndName=${encodeURIComponent(_igIdAndName)}`;
        const intentUrl = `intent://${targetUrl.replace(/^https?:\/\//, '')}#Intent;scheme=http;package=com.android.chrome;end;`;
        window.location.href = intentUrl;
    },

    //開啟下載錢包分頁
    JS_OpenDownloadWallet: function(walletName) {
        const wallet = UTF8ToString(walletName);

        if (wallet == 'Metamask') {
            window.open('https://metamask.io/download.html', '_blank');
        }         
        else if (wallet == 'TrustWallet') {
            window.open('https://trustwallet.com/', '_blank');
        }
        else if (wallet == 'OKX') {
            window.open('https://www.okx.com/web3', '_blank');
        }
        else if (wallet == 'Binance') {
            window.open('https://www.binance.com/zh-TC/download', '_blank');
        }
        else if (wallet == 'Coinbase') {
            window.open('https://www.binance.com/zh-TC/download', '_blank');
        }
    },

    //Window檢查錢包
    JS_WindowCheckWallet: function(walletName) {
        const wallet = UTF8ToString(walletName);

        if (wallet == 'Metamask') {
            if (typeof window.ethereum !== 'undefined') {
                return true;
            }else {
                window.open('https://metamask.io/download.html', '_blank');
                return false;              
            }  
        }         
        else if (wallet == 'TrustWallet') {
            if (typeof window.trustwallet !== 'undefined') {
                return true;
            }else {
                window.open('https://trustwallet.com/', '_blank');
                return false;
            } 
        }
        else if (wallet == 'OKX') {
            if (typeof window.okexchain !== 'undefined') {
                return true;
            }else {
                window.open('https://www.okx.com/web3', '_blank');
                return false;
            } 
        }
        else if (wallet == 'Binance') {
            if (typeof window.BinanceChain !== 'undefined') {
                return true;
            }else {
                window.open('https://www.binance.com/zh-TC/download', '_blank');
                return false;
            } 
        }
        else if (wallet == 'Coinbase') {
            if (typeof window.coinbaseWallet !== 'undefined') {
                return true;
            }else {
                //Coinbase 使用Third web開啟
                //window.open('https://www.binance.com/zh-TC/download', '_blank');
                return false;
            } 
        }    
    },

    //Line加客服測試
    JS_LineService: function(Url){
        const url = UTF8ToString(Url);
        var newTab =  window.open(url,'_blank','width = 500,height = 500');
    },



    myStoredVariable: null, // Initialize the variable
    pipeDreamUrl: null,
    MemberId: null,

    // Function to store a value
    storeVariable: function(value,pipeUrl,MemberId) {
        this.myStoredVariable = UTF8ToString(value);
        this.pipeDreamUrl = UTF8ToString(pipeUrl);
        this.MemberId = UTF8ToString(MemberId);
        console.log("Variable URL:", this.myStoredVariable);
        console.log("Variable PipeDreamURL:", this.pipeDreamUrl);
    },

    clearStoredVariable: function() {
        this.myStoredVariable = null;
        this.pipeDreamUrl = null;
        this.MemberId = null;
        console.log("Stored Variable is cleared");
    },

    sendBeaconRequest: function() {
       console.log('The page is about to be unloaded!');

            if (!self.myStoredVariable) {
                console.log("No data to send, skipping.");
                return;
            }

            // Send a beacon to the provided Firebase URL before unloading the page
            const url = self.myStoredVariable;

            const data = { 
                            memberId: self.MemberId // Send only a success message
                         };


            // Use the Beacon API to send data to Firebase before the page unloads
            if (navigator.sendBeacon) {
                const payload = JSON.stringify(data); // Convert data to JSON string
                navigator.sendBeacon(url, payload); // Send the request asynchronously
                console.log("Payload Sent:", payload);
                console.log("Data Sent:", data);
            }
            else
            {
                console.log("Request not Sent");
            }
    },

    // Function to handle the page load and unload events
    onPageLoad: function() {
        const self = this; // Save reference to the current context

        // Register the 'load' event listener on the window object
        window.addEventListener('load', function() {
            console.log('The page has fully loaded!');
        });

        // Register the 'beforeunload' event listener on the window object
        window.addEventListener('beforeunload', function(event) {
            console.log('The page is about to be unloaded!');

            if (!self.myStoredVariable) {
                console.log("No data to send, skipping.");
                return;
            }

            // Send a beacon to the provided Firebase URL before unloading the page
            const apiUrl = self.myStoredVariable;
            const pipeDream = self.pipeDreamUrl;

            const data = { 
                            memberId: self.MemberId // Send only a success message
                         };


            // Use the Beacon API to send data to Firebase before the page unloads
            if (navigator.sendBeacon) {
                const payload = JSON.stringify(data); // Convert data to JSON string
                navigator.sendBeacon(apiUrl, payload); // Send the request asynchronously
                navigator.sendBeacon(pipeDream,payload);
                console.log("Payload Sent:", payload);
                console.log("Data Sent:", data);
            }
            else
            {
                console.log("Request not Sent");
            }

            // Optionally, set a confirmation message (browser dependent)
           // event.returnValue = 'Are you sure you want to leave?'; // Some browsers show this message to the user
        });
    },

    onPageLoadWithVisibilityChange: function() {
        const self = this; // Save reference to the current context

        // Check if the device is a mobile device
        const isMobile = /Mobi|Android/i.test(navigator.userAgent);

        if (!isMobile) {
            console.log("Not a mobile device, skipping visibility change listener.");
            return; // Exit if not on mobile
        }

        // Add the visibility change listener only on mobile devices
        document.addEventListener('visibilitychange', function() {
            if (document.visibilityState === 'hidden') {
                if (!self.myStoredVariable) {
                    console.log("No data to send, skipping.");
                    return;
                }

                 const apiUrl = self.myStoredVariable;
                 const pipeDream = self.pipeDreamUrl;

                const data = { 
                                memberId: self.MemberId // Send only a success message
                             };

                // Send the data using navigator.sendBeacon
                    if (navigator.sendBeacon) {
                        const payload = JSON.stringify(data);
                        navigator.sendBeacon(url, payload);
                        navigator.sendBeacon(pipeDream,payload);
                    }
                console.log("Data sent before page becomes hidden:", data);
            }
        });

        console.log("Visibility change listener added for mobile.");
    }
    
});