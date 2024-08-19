mergeInto(LibraryManager.library, {

    // 驗證OTP 
    // code = 驗證碼
    // objNamePtr = 回傳物件名
    // callbackFunPtr = 回傳方法名
    JS_FirebaseVerifyCode: function(codeStr, objNamePtr, callbackFunPtr) {
        const code = UTF8ToString(codeStr);
        const gameObjectName = UTF8ToString(objNamePtr);
        const callbackFunctionName = UTF8ToString(callbackFunPtr);

        window.confirmationResult.confirm(code).then((result) => {
            console.log("User signed in successfully!!!");
            const user = result.user;

            window.unityInstance.SendMessage(gameObjectName, callbackFunctionName, "true");
        }).catch((error) => {
            console.log("Verify Code Error : " + error);

            window.unityInstance.SendMessage(gameObjectName, callbackFunctionName, "false");
        });
    },

    // 開始監聽資料
    // pathPtr = 資料路徑
    // objNamePtr = 回傳物件名
    // callbackFunPtr = 回傳方法名
    JS_StartListeningForDataChanges: function(pathPtr, objNamePtr, callbackFunPtr) {
        const refPath = UTF8ToString(pathPtr);
        const ListeningGameObjectName = UTF8ToString(objNamePtr);
        const ListeningCallbackFunctionName = UTF8ToString(callbackFunPtr);

        var dbRef = firebase.database().ref(refPath);
        dbRef.on('value', function(snapshot) {
            var jsonData = JSON.stringify(snapshot.val());
            window.unityInstance.SendMessage(ListeningGameObjectName, ListeningCallbackFunctionName, jsonData);
        });
    },

    // 停止監聽資料
    // pathPtr = 資料路徑
    JS_StopListeningForDataChanges: function(pathPtr) {
        var dbRef = firebase.database().ref(UTF8ToString(pathPtr));
        dbRef.off('value');
    },

    // 寫入資料
    // refPathPtr = 資料路徑
    // jsonDataPtr = json資料
    JS_WriteDataFromFirebase: function(refPathPtr, jsonDataPtr, objNamePtr = null, callbackFunPtr = null) {
        const refPath = UTF8ToString(refPathPtr);
        const jsonData = UTF8ToString(jsonDataPtr);
        const data = JSON.parse(jsonData);

        let gameObjectName = null;
        let callbackFunctionName = null;
        if (objNamePtr && callbackFunPtr) {
            gameObjectName = UTF8ToString(objNamePtr);
            callbackFunctionName = UTF8ToString(callbackFunPtr);
        }

        firebase.database().ref(refPath).set(data, (error) => {
            if (error) {
                console.error("The write failed... : " + error);

                if (gameObjectName != null && callbackFunctionName != null) {
                    window.unityInstance.SendMessage(gameObjectName, callbackFunctionName, "false");
                }
            } else {
                console.log("Data saved successfully!");

                if (gameObjectName != null && callbackFunctionName != null) {
                    window.unityInstance.SendMessage(gameObjectName, callbackFunctionName, "true");
                }
            }
        });
    },

    // 修改與擴充資料
    // refPathPtr = 資料路徑
    // jsonDataPtr = json資料
    // objNamePtr = 回傳物件名
    // callbackFunPtr = 回傳方法名
    JS_UpdateDataFromFirebase: function(refPathPtr, jsonDataPtr, objNamePtr = null, callbackFunPtr = null) {
        const refPath = UTF8ToString(refPathPtr);
        const jsonData = UTF8ToString(jsonDataPtr);

        let gameObjectName = null;
        let callbackFunctionName = null;
        if (objNamePtr && callbackFunPtr) {
            gameObjectName = UTF8ToString(objNamePtr);
            callbackFunctionName = UTF8ToString(callbackFunPtr);
        }

        const data = JSON.parse(jsonData);
        firebase.database().ref(refPath).update(data, (error) => {
            if (error) {
                console.error("The update failed... : " + error);
                if (gameObjectName != null && callbackFunctionName != null) {
                    window.unityInstance.SendMessage(gameObjectName, callbackFunctionName, "false");
                }
            } else {
                console.log("Data updated successfully!");
                if (gameObjectName != null && callbackFunctionName != null) {
                    window.unityInstance.SendMessage(gameObjectName, callbackFunctionName, "true");
                }
            }
        });
    },

    // 讀取資料
    // refPathPtr = 資料路徑
    // objNamePtr = 回傳物件名
    // callbackFunPtr = 回傳方法名
    JS_ReadDataFromFirebase: function(refPathPtr, objNamePtr, callbackFunPtr) {
        const refPath = UTF8ToString(refPathPtr);
        const gameObjectName = UTF8ToString(objNamePtr);
        const callbackFunctionName = UTF8ToString(callbackFunPtr);

        firebase.database().ref(refPath).once('value').then(function(snapshot) {
            const data = snapshot.val();
            const jsonData = JSON.stringify(data);
            window.unityInstance.SendMessage(gameObjectName, callbackFunctionName, jsonData);
        }).catch(function(error) {
            console.error("The read failed... : " + error);
            window.unityInstance.SendMessage(gameObjectName, callbackFunctionName, JSON.stringify({ error: error.message }));
        });
    },

    // 移除資料
    // refPathPtr = 資料路徑
    // objNamePtr = 回傳物件名
    // callbackFunPtr = 回傳方法名
    JS_RemoveDataFromFirebase: function(refPathPtr, objNamePtr, callbackFunPtr) {
        const refPath = UTF8ToString(refPathPtr);
        const gameObjectName = UTF8ToString(objNamePtr);
        const callbackFunctionName = UTF8ToString(callbackFunPtr);

        firebase.database().ref(refPath).remove().then(function() {
            window.unityInstance.SendMessage(gameObjectName, callbackFunctionName, JSON.stringify({ success: true }));
        }).catch(function(error) {
            console.error("The delete failed... : " + error);
            window.unityInstance.SendMessage(gameObjectName, callbackFunctionName, JSON.stringify({ error: error.message }));
        });
    },

    // 檢查用戶資料是否已存在
    // keyPtr = 查詢關鍵字
    // valuePtr = 查詢的值
    // releaseTypePtr = 發布環境
    // objNamePtr = 回傳物件名
    // callbackFunPtr = 回傳方法名
    JS_CheckUserDataExist: async function(keyPtr, valuePtr, releaseTypePtr, objNamePtr, callbackFunPtr) {
        const key = UTF8ToString(keyPtr);
        const valueToSearch = UTF8ToString(valuePtr);
        const releaseType = UTF8ToString(releaseTypePtr);
        const gameObjectName = UTF8ToString(objNamePtr);
        const callbackFunctionName = UTF8ToString(callbackFunPtr);

        let foundPhoneNumber = "";

        try {
            const userRef = firebase.database().ref(releaseType + '/user');
            const snapshot = await userRef.get();

            if (snapshot.exists()) {
                const userData = snapshot.val();
                console.log("Fetched userData:", userData);  // 調試輸出

                // Check phoneUser
                if (userData.phoneUser) {
                    for (const phoneNumberKey in userData.phoneUser) {
                        const phoneNumberData = userData.phoneUser[phoneNumberKey];
                        if (phoneNumberData[key] && phoneNumberData[key] === valueToSearch) {
                            foundPhoneNumber = phoneNumberData.phoneNumber;
                            console.log("Found phoneNumber in phoneUser:", foundPhoneNumber);  // 調試輸出
                            break;
                        }
                    }
                }

                // Check walletUser
                if (userData.walletUser) {
                    for (const walletUserKey in userData.walletUser) {
                        const walletUserData = userData.walletUser[walletUserKey];
                        for (const phoneNumberKey in walletUserData) {
                            const phoneNumberData = walletUserData[phoneNumberKey];
                            if (phoneNumberData[key] && phoneNumberData[key] === valueToSearch) {
                                foundPhoneNumber = phoneNumberData.phoneNumber;
                                console.log("Found phoneNumber in walletUser:", foundPhoneNumber);  // 調試輸出
                                break;
                            }
                        }
                    }
                }
            } else {
                console.log('No data available');
            }

            // Output result
            if (foundPhoneNumber) {
                console.log(`Found phoneNumber for ${key}:`, foundPhoneNumber);
                window.unityInstance.SendMessage(gameObjectName, callbackFunctionName, JSON.stringify({exists: "true", phoneNumber: foundPhoneNumber}));
            } else {
                console.log(`No phoneNumber found for ${key}`);
                window.unityInstance.SendMessage(gameObjectName, callbackFunctionName, JSON.stringify({exists: "false", phoneNumber: ""}));
            }

        } catch (error) {
            console.error('Error fetching data:', error);
            window.unityInstance.SendMessage(gameObjectName, callbackFunctionName, JSON.stringify({exists: "error", phoneNumber: "", error: error.toString()}));
        }
    },

    // 初始化在線狀態監測
    // pathPtr = 監測路徑
    JS_StartListenerConnectState: function(pathPtr) {
        const path = UTF8ToString(pathPtr);
        window.initializePresence(path, path);
    },

    // 移除在線狀態監測
    // idPtr = 監測ID
    JS_RemoveListenerConnectState: function(idPtr) {
        const id = UTF8ToString(idPtr);
        window.removePresenceListener(id);
    },

    // 加入遊戲房間查詢
    // pathPtr = 查詢路徑
    // maxPlayerPtr = 最大遊戲人數
    // idPtr = 玩家ID
    // objNamePtr = 回傳物件名
    // callbackFunPtr = 回傳方法名
    JS_JoinRoomQueryData: async function(pathPtr, maxPlayerPtr, idPtr, objNamePtr, callbackFunPtr) {
        const path = UTF8ToString(pathPtr);
        const maxPlayer = Number(UTF8ToString(maxPlayerPtr));
        const id = UTF8ToString(idPtr);
        const gameObjectName = UTF8ToString(objNamePtr);
        const callbackFunctionName = UTF8ToString(callbackFunPtr);

        let getRoomName = "false";
        let roomCount = 0;

        try {
            const roomRef = firebase.database().ref(path);
            const snapshot = await roomRef.once('value');
            if (snapshot.exists()) {
                const roomsType = snapshot.val();
                roomCount = Object.keys(roomsType).length;
                console.log("房間重複!!!" + roomCount);

                for (let roomName in roomsType) {
                    const room = roomsType[roomName];

                    // 新增检查所有玩家是否都离线
                    let allPlayersOffline = true;
                    for (let playerKey in room.playerDataDic) {
                        const player = room.playerDataDic[playerKey];
                        if (player.online === true) {
                            allPlayersOffline = false;
                            break;
                        }
                    }

                    if (allPlayersOffline) {
                        console.log("所有玩家都已离线，移除房间: " + roomName);
                        await roomRef.child(roomName).remove();
                        roomCount--;
                        continue; // 继续检查下一个房间
                    }

                    // 检查是否有空位并且玩家不在房间中
                    if (Object.keys(room.playerDataDic).length < maxPlayer) {
                        let playerFound = false;

                        for (let playerKey in room.playerDataDic) {
                            const player = room.playerDataDic[playerKey];
                            if (player.userId == id) {
                                playerFound = true;
                                break;
                            }
                        }

                        if (!playerFound) {
                            getRoomName = roomName;
                            break;
                        }
                    }
                }
            }
            window.unityInstance.SendMessage(gameObjectName, callbackFunctionName, JSON.stringify({getRoomName: getRoomName, roomCount: roomCount}));
        } catch (error) {
            window.unityInstance.SendMessage(gameObjectName, callbackFunctionName, JSON.stringify({error: error.message}));
        }
    },

});
