mergeInto(LibraryManager.library, {
    JS_GetPlayerIPAddress: function (objNamePtr, callbackFunPtr) {
        fetch('https://api.ipify.org?format=json')
            .then(response => response.json())
            .then(data => {
                const gameObjectName = Pointer_stringify(objNamePtr);
                const methodName = Pointer_stringify(callbackFunPtr);
                unityInstance.SendMessage(gameObjectName, methodName, data.ip);
            })
            .catch(error => {
                console.error('Error fetching IP address:', error);
            })
    },

    // 獲取儲存的IP地址
    GetUserIPAddress: function() {
        return userIpAddress;
    },

    // 清除URL資料
    JS_ClearUrlQueryString: function() {
        var url = window.location.href;

        if (url.indexOf('?') > -1) {
            var newUrl = url.split('?')[0];
            window.history.replaceState({}, document.title, newUrl);
        }
    },

    // 分享
    JS_Share: function(titleStr, contentStr, urlStr){
        var title = UTF8ToString(titleStr);
        var content = UTF8ToString(contentStr);
        var url = UTF8ToString(urlStr);

        if (navigator.share) {
            navigator.share({
                title: title,
                text: content,
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

    // 複製文字
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

    // 本地頁面跳轉
    JS_LocationHref: function(url){
        window.location.href = UTF8ToString(url);
    },

    // 關閉頁面
    JS_WindowClose: function(){
        window.open("","_self").close();
    },

    // 重新整理頁面
    JS_Reload: function() {
        window.location.reload();
    },

    // 斷開連接
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

    // 撤銷權限
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

    // 獲取瀏覽器訊息
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
        window
