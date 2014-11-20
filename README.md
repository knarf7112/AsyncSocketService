SocketClient
============
(Console)
使用socket的測試客戶端(使用Design Pattern的 State方式儲存狀態,依據狀態(interface)選擇要做的工作)

第一階段連線:
  Client --> Server
  輸入假設的值0001(端末編號) + AAAA(pinCode) + 1000(FirmwareVersion) --> Server檢查(認證成功或失敗)
  
  Server --> Client
  傳送 目前空的字串(firmwareVersion) + 269160(檔案size大小bytes) + 0000(Token驗證碼) --> 客戶端得知檔案版本與檔案大小並獲得驗證碼(3分鐘有效) 
  
第二階段連線:
  Client --> Server
  傳入 0001(pinCode) + F0CA(Token驗證碼) + 0001(offset:檔案某塊位置) + 1024(檔案傳送的區塊大小bytes)
  Server --> Client
  傳送一個byte array的資料(大小1026),offset:檔案某塊位置 = byte[1024] + byte[1025],byte[1023]之前的數據即是Server傳來的檔案切割區塊

  
