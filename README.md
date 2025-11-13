# ADUserStatus (V2)

A fast, lightweight Windows desktop utility for checking Active Directory user account status in bulk.  
This project is the modern C# evolution of the original PowerShell-based tool  
➡️ **[AD-User-Status (PowerShell version)](https://github.com/webbie003/AD-User-Status)**

This version is published as **ADUserStatus V2** in this repository:  
➡️ **https://github.com/webbie003/AD-User-Status-V2**

![ADUserStatus Logo](Help/images/binoculars.png)

---

## 📌 Overview

**ADUserStatus** helps administrators, service teams, auditors, and support staff quickly determine:

- Whether an account exists in Active Directory  
- Whether it‘s **enabled**, **disabled**, or **not found**  
- Whether the user is **external** (UPN mismatch)  
- Whether the account requires attention during onboarding, offboarding, or audit cycles  

The tool accepts **Excel**, **CSV**, or **plain text lists** and provides instant results using LDAPS/LDAP lookups.

No elevated privileges required — works entirely under the current user’s domain permissions.

---

## ✨ Features

- ✔️ Bulk AD account checks from Excel, CSV, or text
- ✔️ Integrated Windows Authentication — no stored credentials
- ✔️ LDAPS/LDAP query support
- ✔️ Clear results for:
  - Enabled  
  - Disabled  
  - External  
  - Not Found  
- ✔️ Export results to Excel (ClosedXML)
- ✔️ Built-in Help system (HTML-based)
- ✔️ Portable single-file EXE (no install)
- ✔️ Fast, modern UI (WinForms + WebView2)
- ✔️ Fully offline — no external APIs

---

## 🖼️ Screenshots

> **You can send me your screenshots and I will format them perfectly**,  
> or you can replace these placeholders with your actual images.

---

## 🚀 Installation

1.Download the latest release:
👉 https://github.com/webbie003/AD-User-Status-V2/releases  
2. Extract the ZIP (or run the single-file EXE if using the standalone build)
3. Run `ADUserStatus.exe`

No installation required.  
No admin rights needed.  
Runs on Windows 10/11 with .NET 8 runtime.

---

## 📖 Built-In Help

ADUserStatus includes a full HTML-based offline help system covering:

- Getting Started  
- Tabs & Results  
- Connection Status  
- LDAP/LDAPS  
- Security & Data Handling  
- About & Licensing  

Open Help via:

- **F1**  
- The **Help** button  
- The **Help → Contents** menu

---

## 🧩 Technology Stack

- **C# / .NET 8**
- **WinForms**
- **WebView2** (for embedded help)
- **ClosedXML** (Excel export)
- **ExcelDataReader** (Excel parsing)
- **System.DirectoryServices.Protocols** (LDAP/LDAPS)
- **DnsClient.NET** (domain controller discovery)

---

## 🧬 Project Background

This application is an evolution of my earlier project:

### 🔗 Previous Version — PowerShell  
**AD-User-Status**  
https://github.com/webbie003/AD-User-Status  
Originally built using PowerShell and packaged with **ps2exe**.

### 🔗 Current Version — C#  
Rewritten from the ground up with:

- Better performance  
- Stronger architecture  
- Better UI/UX  
- Richer help/documentation  
- Single-file deployment  
- Stronger maintainability  

This rewrite was also a personal development challenge and a way to deepen understanding of application design, architecture, and domain service interaction.

---

## 🤖 AI-Assisted Development

Portions of this project — including help-text refinement, code organisation, UI polishing, and architectural guidance — were assisted by **OpenAI ChatGPT** as a productivity and learning tool.

All engineering decisions, implementation, debugging, and final structure were done manually.

---

## 👤 Developer

**Webbie_003**

ADUserStatus is an independent, personal project.  
It is **not** developed for, endorsed by, or affiliated with any employer or organization.

### Contact  
📧 **aduserstatus@proton.me**

---

## 📜 License

This project is released under the **MIT License**.  
See [`LICENSE`](LICENSE) for details.

---

## ⭐ Support the Project

If you find this tool useful:

- Star the repository ⭐  
- Open issues or feature requests  
- Share feedback  
- Contribute ideas  

Your support helps shape future improvements.

---
