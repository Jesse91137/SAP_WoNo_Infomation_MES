# SAP 匯出的工單 — 讀取 TXT 檔並寫入 AMES_DB 控制台

## 專案概覽

此專案為以 .NET 建置的控制台應用程式，功能為自動化處理 SAP 匯出的工單文字檔（TXT）：

- 從網路共用（SMB）取得當日 SAP 匯出的 TXT 檔清單
- 將目標 TXT 下載到本機執行目錄下的 `wo_info` 資料夾
- 逐行解析 TXT（以 tab 分隔），當行首為 `INFO` 時將欄位寫入資料庫（table: `JH_SAP_TO_MES`）
- 可選擇刪除本機 `wo_info` 下的處理檔案

重要程式檔案：

```
SAP_WoNo_Infomation_MES/
  Program.cs       # 程式主流程（取得檔案、下載、解析、寫入）
  db.cs            # 資料庫操作封裝（含 connStr）
```

---

## 1. 啟動流程（對應 Program.cs）

程式啟動時會依序執行下列工作：

1. Dir_File()
   - 連線到網路共用路徑：\\192.168.4.85\saploc\SAP_Export\PP\WO_info\
   - 尋找檔名中 substring(5,8) 等於今日日期（yyyyMMdd）的檔案，並把匹配檔名放入內部清單 `strList`。

2. Download_TXT()
   - 以 `ImpersonateUser`（程式內以帳號 `sap-mes`、domain `\\192.168.4.85\` 與密碼 `Eversun@888` 範例）對網路共用進行身份代替，將 `strList` 中的檔案複製到本機 `Environment.CurrentDirectory + "\\wo_info\\"`。
   - 注意程式原始碼中迴圈存在一個實務上常見的 bug：迴圈內使用 `strList[0]` 進行複製，理想行為應為 `strList[i]`，若處理多個檔案需修正。

3. TxT_Read_Write()
   - 讀取本機 `wo_info` 資料夾中所有 TXT 檔案，逐行讀取並以 `\t` 分割。
   - 當第一欄為 `INFO` 時：
     - 欄位 2 = 工作單號（WO_NO）
     - 欄位 3 = 工程序（ENG_SR）
     - 檢查資料表 `JH_SAP_TO_MES` 是否已有相同組合（WO_NO, ENG_SR），若無則以 `insert` 新增 (同時寫入 rctime)

4. Delete_File() （目前註解，需時可啟用）
   - 刪除 `wo_info` 資料夾下的 *.txt 檔案

程式完成後會在主控台印出處理記錄，並顯示「寫入完畢,按任意建關閉!!」。

---

## 2. 路徑與參數說明

- 網路來源：\\192.168.4.85\saploc\SAP_Export\PP\WO_info\
- 本機下載資料夾：執行目錄下的 `wo_info`（等同於 `Environment.CurrentDirectory + "\\wo_info\\"`）
- 篩選規則：程式以檔名的第 6 到第 13 字元（0-based index 中 substring(5,8)）與當日 `yyyyMMdd` 比對
- 資料表：JH_SAP_TO_MES（程式直接查詢 / 插入）

注意：程式目前無 CLI 參數支援（例如自訂來源路徑或日期），若要彈性使用請考慮改造 Program.cs 以支援參數或設定檔。

---

## 3. 資料庫連線與安全性

- 在 `db.cs` 內目前存在硬編碼的連線字串（connStr），範例如下（請勿在生產環境使用明碼密碼）：

```
// 本機測試
server=192.168.6.57;database=AMES_DB;uid=sa;pwd=A12345678;Connect Timeout=10

// 正式環境範例（被註解）
server=192.168.4.200;database=AMES_DB;uid=fa;pwd=fa;Connect Timeout=10
```

建議：不要把連線字串或任何帳密硬編碼在程式內，請採下列做法之一：

- 使用環境變數（例如在 Windows PowerShell 設定）：

```powershell
#$env:AMES_DB_CONNECTION = 'Server=192.168.6.57;Database=AMES_DB;User Id=sa;Password=A12345678;'
```

- 或改為使用 `appsettings.json`（並在部署時以機器/CI 的祕密管理覆寫），或使用 Windows Credential / Key Vault 等安全存放機制。

若改動 `db.cs`，請改為從環境讀取 connStr，範例（C#）：

```csharp
// 範例讀取環境變數（請在 production 前完善錯誤處理）
string connStr = Environment.GetEnvironmentVariable("AMES_DB_CONNECTION");
```

---

## 4. TXT 檔案範例與資料格式

程式預期每一列以 `\t` (tab) 分隔，且感興趣的行以 `INFO` 開頭。範例：

```
INFO\tWO12345678\tENG001\t...其餘欄位可忽略
```

檢查重點：

- 第 1 欄為 `INFO`
- 第 2 欄為 `WO_NO`（工作單號）
- 第 3 欄為 `ENG_SR`（工程序）

若檔案使用其他編碼（Big5 / ANSI / UTF-8），請確保程式可以正確讀取；可在本機使用 Notepad++ 或 PowerShell 確認編碼。

---

## 5. 執行步驟（PowerShell 範例）

1) 切換到專案資料夾：

```powershell
cd 'd:\專案\SAP匯出資料處理\SAP匯出的工單-讀取TXT檔寫入AMES_DB控制台\SAP_WoNo_Infomation_MES'
```

2) 還原並建置：

```powershell
dotnet restore
dotnet build -c Debug
```

3) 執行（開發模式）：

```powershell
dotnet run --project .\SAP_WoNo_Infomation_MES.csproj
```

4) 或執行已編譯的可執行檔：

```powershell
& '.\SAP_WoNo_Infomation_MES\bin\Debug\SAP_WoNo_Infomation_MES.exe'
```

執行時請先確認：

- 主機對網路共用 `\\192.168.4.85\saploc\SAP_Export\PP\WO_info\` 有連線與存取權限
- 若程式使用 impersonation（程式碼內為 `sap-mes` 範例），需要該帳號具備讀取網路共用的權限

---

## 6. 輸出位置與驗證

- 本機下載與處理檔案位置：

  `SAP_WoNo_Infomation_MES\bin\Debug\wo_info\`

- 驗證資料庫是否成功寫入：

  - 檢查資料表 `JH_SAP_TO_MES` 是否新增對應的 `WO_NO` 與 `ENG_SR`
  - 可使用 SQL Server Management Studio 或簡單的 SELECT 查詢：

```sql
SELECT TOP 50 * FROM JH_SAP_TO_MES ORDER BY reTime DESC;
```

---

## 7. 常見問題與排解（進階）

- 無法連線到網路共用：
  - 確認網段、VPN 與防火牆設定
  - 測試從主機上以 PowerShell 讀取 `Get-ChildItem "\\192.168.4.85\saploc\SAP_Export\PP\WO_info\"`

- 權限被拒絕（Impersonate 或 File.Copy 拋例外）：
  - 確認 `sap-mes` 或執行程式的使用者在目標共用上有讀取權限
  - 程式內硬編密碼為安全風險，應移除並改用安全存取方式

- 解析不到資料（空檔或格式不符）：
  - 檢查檔案是否有 BOM 或使用非 UTF-8 編碼
  - 檢查是否以 Tab (`\t`) 分隔

- 多檔案只處理第一個（疑似 bug）：
  - 程式 `Download_TXT()` 迴圈使用 `strList[0]` 將導致只有第一個被複製，請改為 `strList[i]`。

---

## 8. 安全性與建議改進（建議優先度）

1. 高優先：移除 `db.cs` 內的硬編碼 connStr，使用環境變數或機密管理（KeyVault / Windows Credential）。
2. 高優先：不要在程式碼中硬編帳密（例如 `ImpersonateUser` 中的密碼），改由安全機制或部署時注入。
3. 中優先：修正 `Download_TXT` 的檔案複製迴圈 bug（使用 `strList[i]`）。
4. 中優先：加入更完整的錯誤日誌紀錄（例如 log 檔或使用 logging framework），而不是單純 Console.WriteLine。
5. 中優先：支援以 CLI 參數或設定檔指定來源路徑、日期或模式，提升彈性。

---

## 9. 測試與驗證建議

- 建立小型測試 TXT 檔（見下方範例），放入網路共用或本機模擬來源，測試解析與資料庫寫入
- 建立單元測試或整合測試（xUnit）模擬 `db` 行為（可用 mock 或 test DB）

### 範例測試檔內容（test1.txt）

```
INFO\tWO000001\tENG001
INFO\tWO000002\tENG002
```

---

## 10. 我可以幫你 建立 的項目（選一項回覆數字）

1. 建立 `appsettings.json` 範本（含註解與範例欄位）
2. 新增範例測試檔（TXT）與快速驗證腳本（PowerShell）
3. 建立簡單的單元測試專案（xUnit）並加入 1-2 個範例測試

若要我直接代為修改程式碼（例如修正迴圈 bug 或把 connStr 換成由環境讀取），請在回覆時註明要執行哪一項，我會在專案中為你 建立 或 修改 對應檔案並執行基本驗證。


