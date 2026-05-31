# 💻 Zakkistore SDK for C# (.NET)

**Official B2B Client Library for Zakki Store API Gateway**

Pustaka C# (.NET) resmi untuk memudahkan integrasi layanan Host-to-Host (H2H) prabayar/pascabayar, payment gateway QRIS otomatis, perbankan Virtual Account (VA), Noktel OTP virtual, mining reward, dan gacha koin Zakki Store ke dalam proyek .NET Anda (ASP.NET MVC, Web API, .NET Core Console, Xamarin, Unity, Windows Forms, dll).

---

## 🚀 Instalasi & Inisialisasi

Instal pustaka menggunakan NuGet Package Manager:

```bash
dotnet add package ZakkiStore.SDK
```

### Inisialisasi Klien

#### Mode 1: Inisialisasi Instan (Official Gateway by Default)
Sangat praktis! SDK otomatis mengarah ke gateway server resmi (`https://qris.zakki.store`).

```csharp
using System;
using System.Threading.Tasks;
using ZakkiStore;

class Program
{
    static async Task Main(string[] args)
    {
        // Klien otomatis mengarah ke server resmi!
        var zakki = new ZakkiStoreClient("API_TOKEN_MEMBER_ANDA");

        // Contoh: Melakukan Health Check Server
        var status = await zakki.StatusAsync();
        Console.WriteLine($"Status Server: {status["status"]}");
    }
}
```

#### Mode 2: Inisialisasi dengan Konfigurasi Kustom
Gunakan opsi ini jika Anda ingin melakukan kustomisasi base URL (migrasi domain) atau mengaktifkan fitur penarikan otomatis (Auto-Withdraw).

```csharp
var zakki = new ZakkiStoreClient(
    baseUrl: "https://qris.zakki.store", // Domain custom/resmi
    token: "API_TOKEN_MEMBER_ANDA",
    iduser: "IBO99",
    email: "member@gmail.com",
    pin: "123456",                       // Wajib untuk tabung & tarik
    autoWithdraw: true                   // Aktifkan auto-withdrawal saldo bank!
);
```

---

## 🛠️ Fitur Unggulan

### 🔄 Auto-Withdraw Saldo VA
Jika opsi `autoWithdraw: true` diaktifkan, SDK akan memicu penarikan dana VA bank otomatis secara *real-time* menjadi saldo utama aplikasi (BukaOlshop) ketika fungsi `zakki.CheckbankAsync()` dipanggil.

### 💡 Dual-Flow Pascabayar & Bebas Nominal
*   **Pascabayar (PLN/BPJS/PDAM):** Inquiry tagihan terlebih dahulu, lalu bayar dengan format tujuan `[ID_Pelanggan].[Nominal_Tagihan]` (Contoh: `122345678901.150000`).
*   **E-Wallet Bebas Nominal:** Kirim transfer E-Wallet nominal kustom dengan format tujuan `[No_HP].[Nominal]` (Contoh: `08123456789.25000`).

---

## 📑 Daftar Referensi Metode Lengkap

SDK C# ini mendukung secara penuh seluruh **25 fungsi resmi** dengan nama dan perilaku yang konsisten dengan SDK versi Node.js (NPM), Python (PyPI), PHP (Composer), dan Go:

### 1. Payment Gateway (QRIS Top Up)
*   `zakki.TopupAsync(int nominal)` — Membuat QRIS dinamis instan dengan nominal kode unik.
*   `zakki.CektopupAsync(string idtopup)` — Cek status pembayaran QRIS.
*   `zakki.CancelAsync(string idTransaksi, bool allPending)` — Batalkan transaksi pending.

### 2. Transaksi H2H
*   `zakki.ListkodeAsync(string jenis, string productType)` — Katalog kode produk aktif.
*   `zakki.H2HAsync(H2HParams paramsObj)` — Mengirim order transaksi H2H.
*   `zakki.H2HSimpleAsync(string kode, string tujuan, string refID)` — Versi sederhana posisional untuk memicu order H2H.
*   `zakki.Cekh2hAsync(string idTrx)` — Cek detail status pengisian, SN, dan harga beli.
*   `zakki.Myh2hAsync()` — Mengambil 20 riwayat pembelian H2H terupdate.

### 3. Perbankan & Transfer VA
*   `zakki.CheckbankAsync()` — Cek saldo, VA member, mutasi, dan pemicu Auto-Withdraw.
*   `zakki.ChecknameAsync(string number)` — Verifikasi nama asli pemilik VA Bank.
*   `zakki.TransferAsync(TransferParams paramsObj)` — Transfer saldo antar Virtual Account.
*   `zakki.TransferSimpleAsync(string to, int amount)` — Versi sederhana posisional untuk transfer saldo.
*   `zakki.TabungAsync(int jumlah)` — Menabung saldo ke Bank (butuh PIN).
*   `zakki.TarikAsync(int jumlah)` — Menarik dana tabungan ke saldo aplikasi (butuh PIN).
*   `zakki.CheckmutasiAsync(string mutasiType)` — Riwayat mutasi Tarik/Tabung.

### 4. Noktel Marketplace (OTP Virtual)
*   `zakki.NoktelStokAsync()` — Cek stok nomor virtual yang ready.
*   `zakki.NoktelBuyAsync(string category)` — Membeli nomor virtual baru untuk OTP.
*   `zakki.NoktelGetOtpAsync(string accountID)` — Menarik kode OTP Telegram secara real-time.
*   `zakki.NoktelCancelAsync(string invoiceID)` — Membatalkan nomor yang pending OTP & auto-refund.
*   `zakki.NoktelHistoryAsync()` — Mengambil daftar riwayat pembelian Noktel.

### 5. Reward Komputasi & Game
*   `zakki.CekminingAsync()` — Cek status kesulitan global, block reward, dan miner aktif.
*   `zakki.MyminingAsync()` — Riwayat koin mining SHA256 milik akun Anda.
*   `zakki.CekgachaAsync()` — Statistik poin, kemenangan, dan keuntungan gacha member.

### 6. Keamanan & Utilitas
*   `zakki.WhitelistipAsync(string ip)` — Whitelist IP server Anda untuk otorisasi API H2H.
*   `zakki.DelwhitelistipAsync(string ip)` — Hapus IP server dari whitelist.
*   `zakki.LeaderboardAsync(int limit, string period)` — Mengambil peringkat sultan topup teraktif.
*   `zakki.StatusAsync()` — Informasi beban CPU, metrik finansial, dan kesehatan sistem.

---

## 🛡️ Protokol Keamanan API

> [!WARNING]
> **Selalu jalankan SDK ini di sisi backend (Server-side)!**
> Jangan pernah mengekspos API Token dan PIN Anda di sisi frontend / client-side publik demi mencegah potensi pencurian saldo.
