# SAP_WoNo_Infomation_MES

## 簡介

本專案為一個用於處理 SAP 匯出資料（Work Order / WoNo）並輸出或整理給 MES 使用的小型 .NET 應用程式。專案名稱為 `SAP_WoNo_Infomation_MES`，主要功能請參見程式原始碼（例如 `Program.cs`、`db.cs`）以取得實作細節。

> 注意：此 README 為快速導覽與開發/執行說明。要了解業務邏輯與輸入/輸出格式，請直接閱讀 `Program.cs` 與相關類別。

## 目的與適用範圍

- 處理從 SAP 匯出的工單或製令資料。
- 轉換、檢查並輸出供 MES 系統消費的資訊。
- 面向開發人員與維護人員，提供本地建置與測試指南。

## 前置需求

- 已安裝 .NET SDK（請以專案檔 `SAP_WoNo_Infomation_MES/SAP_WoNo_Infomation_MES.csproj` 中的 TargetFramework 為準，使用相容的 SDK 版本）。
- 建議在 Windows PowerShell 下操作（本專案在 Windows 環境下被開發與測試）。

## 快速開始（PowerShell）

1. 從專案根目錄執行建置：

```powershell
dotnet build "SAP_WoNo_Infomation_MES/SAP_WoNo_Infomation_MES.csproj"
```

2. 直接執行專案（透過 dotnet run）：

```powershell
dotnet run --project "SAP_WoNo_Infomation_MES/SAP_WoNo_Infomation_MES.csproj"
```

3. 或執行已編譯的可執行檔（Debug 輸出範例）：

```powershell
& .\SAP_WoNo_Infomation_MES\bin\Debug\SAP_WoNo_Infomation_MES.exe
```

（如果專案用其他組態編譯，請改為相對應的資料夾，例如 `bin\Release\`。）

## 專案結構（摘要）

- `SAP_WoNo_Infomation_MES/Program.cs` — 應用程式進入點。
- `SAP_WoNo_Infomation_MES/db.cs` — 可能與資料處理或資料庫相關的實作。
- `SAP_WoNo_Infomation_MES/SAP_WoNo_Infomation_MES.csproj` — 專案檔（請檢查 TargetFramework 與相依性）。
- `SAP_WoNo_Infomation_MES/bin/` — 編譯後的輸出（包含 `wo_info/` 範例資料夾，在開發版中可能存在測試資料）。
- `Properties/AssemblyInfo.cs` — 組件資訊。

若要更完整地理解輸入/輸出格式，請開啟 `Program.cs` 與 `db.cs`，尋找檔案路徑或檔案讀寫程式碼。

## 執行時注意事項

- 若程式預期從特定資料夾（例如 `wo_info/`）讀取檔案，請確認該資料夾與檔案存在於執行目錄或以絕對/相對路徑提供。
- 執行期間若發生錯誤，請查看終端機輸出或在 Visual Studio 中執行以取得除錯資訊（如 StackTrace）。

## 開發者與貢獻者指南

- 使用分支（feature branch）工作流程。提交前請確保能在本地成功建置。
- 新增功能或修正時，請在 PR 描述中包含變更摘要與測試方法。
- 程式註解請使用繁體中文說明「為何」而非「做什麼」，變數與函式命名請使用英文（專案慣例）。

## 測試

本專案目前未包含自動化測試腳本（若要加入，建議新增單元測試專案並在 CI 中執行）。

## 授權

此專案尚未加入 LICENSE 檔案。請在倉庫中加入合適的授權（例如 MIT、Apache-2.0 等），以明確授權條件。

## 聯絡

如需協助或要提供回饋，請在 Pull Request 或 Issue 中描述問題與重現步驟，或直接聯絡專案負責人（請在此填入聯絡資訊）。

## 備註（假設與檢查事項）

- 假設：本 README 假設您在 Windows 開發環境並安裝相容的 .NET SDK。若您需要我自動檢查 `*.csproj` 以填入精確的 TargetFramework 與建置指令，我可以接著為您執行該檢查並更新本檔案。
