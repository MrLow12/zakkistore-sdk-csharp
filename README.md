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
Jika opsi `autoWithdraw: true` diaktifkan, SDK akan memicu penarikan dana VA bank otomatis secara *real-time* menjadi saldo utama aplikasi zakki store ketika fungsi `zakki.CheckbankAsync()` dipanggil.

### 💡 Dual-Flow Pascabayar & Bebas Nominal
*   **Pascabayar (PLN/BPJS/PDAM):** Inquiry tagihan terlebih dahulu, lalu bayar dengan format tujuan `[ID_Pelanggan].[Nominal_Tagihan]` (Contoh: `122345678901.150000`).
*   **E-Wallet Bebas Nominal:** Kirim transfer E-Wallet nominal kustom dengan format tujuan `[No_HP].[Nominal]` (Contoh: `08123456789.25000`).

---

## 📑 Daftar Referensi Metode Lengkap & Struktur Pengelompokan (36 Fungsi Resmi)

Seluruh fungsi yang didukung oleh SDK ini dikelompokkan secara rapi ke dalam 7 kategori layanan utama demi mempermudah pemahaman dan integrasi:

### 1. ⚡ Layanan Payment Gateway (QRIS Topup) — [4 Fungsi]
*   **`await zakki.TopupAsync(nominal)`** — Membuat tiket pembayaran QRIS dinamis instan dengan nominal kode unik.
*   **`await zakki.CektopupAsync(idtopup)`** — Mengecek status pembayaran tiket QRIS tertentu secara real-time.
*   **`await zakki.MytopupAsync()`** — Mengambil seluruh riwayat transaksi topup QRIS akun Anda.
*   **`await zakki.CancelAsync(idTransaksi, allPending)`** — Membatalkan satu atau seluruh tiket topup pending.

### 2. 🏪 Layanan Transaksi Host-to-Host (H2H) — [4 Fungsi]
*   **`await zakki.ListkodeAsync(jenis, productType)`** — Mengambil katalog produk prabayar/pascabayar aktif beserta daftar harga beli.
*   **`await zakki.H2HAsync(params)`** — Mengirimkan order transaksi H2H (pulsa, paket data, PLN kustom, dll).
*   **`await zakki.Cekh2hAsync(idTrx)`** — Mengecek status transaksi, Serial Number (SN), dan harga beli riil dari order H2H.
*   **`await zakki.Myh2hAsync()`** — Mengambil 20 riwayat transaksi H2H terupdate milik akun Anda.

### 3. 🏦 Layanan Perbankan & Transfer Saldo VA — [8 Fungsi]
*   **`await zakki.CheckbankAsync()`** — Memeriksa detail Virtual Account (VA), saldo bank VA, serta memicu Auto-Withdraw jika diaktifkan.
*   **`await zakki.ChecknameAsync(number)`** — Memverifikasi nama asli pemilik rekening Virtual Account tujuan sebelum melakukan transfer.
*   **`await zakki.TransferAsync(params)`** — Mengirimkan saldo antar-VA member secara instan dan bebas biaya admin.
*   **`await zakki.TabungAsync(jumlah)`** — Menyetorkan saldo aktif aplikasi ke rekening bank Virtual Account terhubung Anda.
*   **`await zakki.TarikAsync(jumlah)`** — Menarik dana dari bank Virtual Account ke saldo aktif aplikasi Zakki Store Anda.
*   **`await zakki.CheckmutasiAsync(mutasiType)`** — Melihat riwayat mutasi tabung/tarik saldo bank VA (`all`, `tarik`, `tabung`).
*   **`await zakki.ChecktransferAsync(idtransfer)`** — Mengecek status pengiriman dana transfer tertentu secara detail.
*   **`await zakki.MytransferAsync(type)`** — Mengambil riwayat pengiriman dan penerimaan transfer saldo (`all`, `kirim`, `terima`).

### 4. 📱 Layanan Noktel Marketplace (OTP Virtual) — [5 Fungsi]
*   **`await zakki.NoktelStokAsync()`** — Memeriksa ketersediaan stok nomor virtual aktif per kategori layanan/aplikasi.
*   **`await zakki.NoktelBuyAsync(category)`** — Membeli nomor virtual baru untuk penerimaan kode verifikasi/OTP.
*   **`await zakki.NoktelGetOtpAsync(accountId)`** — Mengambil kode verifikasi/OTP yang masuk ke nomor virtual secara real-time.
*   **`await zakki.NoktelCancelAsync(invoiceId)`** — Membatalkan order nomor virtual yang pending OTP dan memicu auto-refund saldo.
*   **`await zakki.NoktelHistoryAsync()`** — Mengambil daftar riwayat lengkap pemesanan nomor virtual.

### 5. ⛏️ Layanan Reward Komputasi SHA-256 (Mining) & Game — [5 Fungsi]
*   **`await zakki.MiningStartAsync()`** — Meminta challenge penambangan SHA-256 serta target kesulitan (difficulty) dari server.
*   **`await zakki.MiningSubmitAsync(nonce, signature)`** — Mengirimkan hasil kerja hashing SHA-256 (Proof-of-Work) untuk mendapatkan koin.
*   **`await zakki.CekminingAsync(idmining)`** — Mengecek status audit dan persetujuan dari blok mining yang telah Anda selesaikan.
*   **`await zakki.MyminingAsync()`** — Melihat riwayat penambangan koin dan total reward hashing akun Anda.
*   **`await zakki.CekgachaAsync()`** — Mengecek jumlah tiket gacha, riwayat kemenangan, dan detail koin keberuntungan Anda.

### 6. 🔒 Layanan Keamanan IP & Utilitas — [6 Fungsi]
*   **`await zakki.WhitelistipAsync(ip)`** — Mendaftarkan IP server/host Anda agar diizinkan melakukan transaksi H2H via API (Maksimal 3 IP).
*   **`await zakki.DelwhitelistipAsync(ip)`** — Menghapus alamat IP terdaftar dari whitelist API.
*   **`await zakki.CekmyipAsync()`** — Mendeteksi alamat IP publik host/server Anda saat ini yang terbaca oleh sistem.
*   **`await zakki.CekipAsync(ip)`** — Mengecek detail status IP whitelisting tertentu.
*   **`await zakki.LeaderboardAsync(limit, period)`** — Melihat daftar Sultan topup teraktif secara global.
*   **`await zakki.StatusAsync()`** — Memeriksa beban CPU server, statistik finansial global, dan kesehatan sistem.

### 7. 🔗 Layanan Webhook Callback & Notifikasi Bot — [4 Fungsi]
*   **`await zakki.SetcallbackAsync(site)`** — Memasang URL callback real-time untuk menerima laporan status transaksi H2H.
*   **`await zakki.DelcallbackAsync()`** — Menghapus URL callback yang terpasang di sistem.
*   **`await zakki.SetnotifbotAsync(telegramId)`** — Memasang ID Telegram Anda untuk menerima notifikasi otomatis transaksi sukses/gagal.
*   **`await zakki.DelnotifbotAsync()`** — Menonaktifkan bot notifikasi Telegram.


## 🛡️ Protokol Keamanan API

> [!WARNING]
> **Selalu jalankan SDK ini di sisi backend (Server-side)!**
> Jangan pernah mengekspos API Token dan PIN Anda di sisi frontend / client-side publik demi mencegah potensi pencurian saldo.
