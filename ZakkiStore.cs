using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ZakkiStore
{
    public class H2HParams
    {
        public string kode { get; set; }
        public string tujuan { get; set; }
        public string refID { get; set; }
    }

    public class TransferParams
    {
        public string to { get; set; }
        public int amount { get; set; }
    }

    public class ZakkiStoreClient
    {
        private readonly string _baseUrl;
        private readonly string _token;
        private readonly string _iduser;
        private readonly string _email;
        private readonly string _pin;
        private bool _autoWithdraw;
        private readonly HttpClient _httpClient;

        public ZakkiStoreClient(string token, string iduser = null, string email = null, string pin = null, bool autoWithdraw = false)
            : this("https://qris.zakki.store", token, iduser, email, pin, autoWithdraw)
        {
        }

        public ZakkiStoreClient(string baseUrl, string token, string iduser = null, string email = null, string pin = null, bool autoWithdraw = false)
        {
            if (string.IsNullOrEmpty(token))
                throw new ArgumentException("token wajib disertakan dalam konfigurasi SDK.");

            if (string.IsNullOrEmpty(baseUrl))
                baseUrl = "https://qris.zakki.store";

            _baseUrl = baseUrl.TrimEnd('/');
            _token = token;
            _iduser = iduser;
            _email = email;
            _pin = pin;
            _autoWithdraw = autoWithdraw;
            _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
        }

        public void EnableAutoWithdraw(bool status)
        {
            _autoWithdraw = status;
        }

        private async Task<Dictionary<string, object>> RequestAsync(string endpoint, HttpMethod method, object data = null)
        {
            var url = $"{_baseUrl}{endpoint}";
            var request = new HttpRequestMessage(method, url);

            if (data != null)
            {
                if (method == HttpMethod.Get)
                {
                    var jsonStr = JsonSerializer.Serialize(data);
                    var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonStr);
                    var queryList = new List<string>();
                    foreach (var kvp in dict)
                    {
                        queryList.Add($"{kvp.Key}={Uri.EscapeDataString(kvp.Value?.ToString() ?? "")}");
                    }
                    if (queryList.Count > 0)
                    {
                        url += "?" + string.Join("&", queryList);
                        request = new HttpRequestMessage(method, url);
                    }
                }
                else
                {
                    var jsonPayload = JsonSerializer.Serialize(data);
                    request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                }
            }

            try
            {
                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, object>>(responseContent);

                if (!response.IsSuccessStatusCode)
                {
                    string errMsg = result != null && result.ContainsKey("message") ? result["message"]?.ToString() : $"HTTP Error! Status: {(int)response.StatusCode}";
                    if (response.StatusCode == System.Net.HttpStatusCode.Forbidden || errMsg.ToLower().Contains("ip"))
                    {
                        errMsg += "\n⚠️ [IP BLOCKED / UNREGISTERED] IP Anda diblokir atau belum terdaftar di whitelist API. Silakan hubungi developer via WhatsApp (https://wa.me/6283844082339) atau Telegram (https://t.me/zakki_store) untuk mendapatkan bantuan.";
                    }
                    throw new Exception($"[ZakkiStore SDK Error] {errMsg}");
                }

                return result;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("[ZakkiStore SDK Error]")) throw;
                throw new Exception($"[ZakkiStore SDK Error] Koneksi Gagal: {ex.Message}");
            }
        }

        // ==========================================================
        // --- 1. PAYMENT GATEWAY (QRIS TOPUP) ---
        // ==========================================================

        public async Task<Dictionary<string, object>> TopupAsync(int nominal)
        {
            return await RequestAsync("/topup", HttpMethod.Post, new { token = _token, nominal });
        }

        public async Task<Dictionary<string, object>> CektopupAsync(string idtopup)
        {
            return await RequestAsync("/cektopup", HttpMethod.Get, new { idtopup });
        }

        public async Task<Dictionary<string, object>> CancelAsync(string idTransaksi = null, bool allPending = false)
        {
            var payload = new Dictionary<string, object> { { "token", _token } };
            if (!string.IsNullOrEmpty(idTransaksi)) payload.Add("id_transaksi", idTransaksi);
            if (allPending) payload.Add("all", true);

            return await RequestAsync("/cancel", HttpMethod.Post, payload);
        }

        // ==========================================================
        // --- 2. TRANSAKSI H2H (HOST-TO-HOST) ---
        // ==========================================================

        public async Task<Dictionary<string, object>> ListkodeAsync(string jenis = null, string productType = null)
        {
            var payload = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(jenis)) payload.Add("jenis", jenis);
            if (!string.IsNullOrEmpty(productType)) payload.Add("type", productType);

            return await RequestAsync("/listkode", HttpMethod.Get, payload);
        }

        public async Task<Dictionary<string, object>> H2HAsync(H2HParams paramsObj)
        {
            return await RequestAsync("/h2h", HttpMethod.Post, new { token = _token, kode = paramsObj.kode, tujuan = paramsObj.tujuan, refID = paramsObj.refID });
        }

        public async Task<Dictionary<string, object>> H2HSimpleAsync(string kode, string tujuan, string refID)
        {
            return await H2HAsync(new H2HParams { kode = kode, tujuan = tujuan, refID = refID });
        }

        public async Task<Dictionary<string, object>> Cekh2hAsync(string idTrx)
        {
            return await RequestAsync("/cekh2h", HttpMethod.Get, new { id = idTrx });
        }

        public async Task<Dictionary<string, object>> Myh2hAsync()
        {
            return await RequestAsync("/myh2h", HttpMethod.Get, new { token = _token });
        }

        // ==========================================================
        // --- 3. PERBANKAN & TRANSFER SALDO ---
        // ==========================================================

        public async Task<Dictionary<string, object>> CheckbankAsync()
        {
            var payload = new Dictionary<string, string> { { "token", _token } };
            if (!string.IsNullOrEmpty(_iduser)) payload.Add("iduser", _iduser);
            else if (!string.IsNullOrEmpty(_email)) payload.Add("email", _email);

            var bankRes = await RequestAsync("/checkbank", HttpMethod.Get, payload);

            if (_autoWithdraw && bankRes.ContainsKey("data"))
            {
                var dataStr = bankRes["data"]?.ToString();
                if (!string.IsNullOrEmpty(dataStr))
                {
                    try
                    {
                        var data = JsonSerializer.Deserialize<Dictionary<string, object>>(dataStr);
                        if (data != null && data.ContainsKey("bank_detail"))
                        {
                            var bankDetailStr = data["bank_detail"]?.ToString();
                            if (!string.IsNullOrEmpty(bankDetailStr))
                            {
                                var bankDetail = JsonSerializer.Deserialize<Dictionary<string, object>>(bankDetailStr);
                                if (bankDetail != null && bankDetail.ContainsKey("balance"))
                                {
                                    double balance = 0.0;
                                    double.TryParse(bankDetail["balance"]?.ToString() ?? "0", out balance);

                                    if (balance > 0)
                                    {
                                        var withdrawRes = await TarikAsync((int)balance);
                                        var updatedRes = await RequestAsync("/checkbank", HttpMethod.Get, payload);
                                        bankRes = updatedRes;
                                        bankRes.Add("auto_withdraw_executed", true);
                                        bankRes.Add("auto_withdraw_amount", (int)balance);
                                        bankRes.Add("auto_withdraw_message", withdrawRes.ContainsKey("message") ? withdrawRes["message"]?.ToString() : "Auto-withdraw berhasil dijalankan.");
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        bankRes.Add("auto_withdraw_executed", false);
                        bankRes.Add("auto_withdraw_error", ex.Message);
                    }
                }
            }

            return bankRes;
        }

        public async Task<Dictionary<string, object>> ChecknameAsync(string number)
        {
            return await RequestAsync("/checkname", HttpMethod.Get, new { number = number.Trim() });
        }

        public async Task<Dictionary<string, object>> TransferAsync(TransferParams paramsObj)
        {
            return await RequestAsync("/transfer", HttpMethod.Post, new { token = _token, to = paramsObj.to, amount = paramsObj.amount });
        }

        public async Task<Dictionary<string, object>> TransferSimpleAsync(string to, int amount)
        {
            return await TransferAsync(new TransferParams { to = to, amount = amount });
        }

        public async Task<Dictionary<string, object>> TabungAsync(int jumlah)
        {
            if (string.IsNullOrEmpty(_pin))
                throw new Exception("[ZakkiStore SDK Error] PIN transaksi diperlukan untuk melakukan transaksi tabung");

            var payload = new Dictionary<string, object> {
                { "token", _token },
                { "jumlah", jumlah },
                { "pin", _pin }
            };

            if (!string.IsNullOrEmpty(_iduser)) payload.Add("iduser", _iduser);
            if (!string.IsNullOrEmpty(_email)) payload.Add("email", _email);

            return await RequestAsync("/tabung", HttpMethod.Post, payload);
        }

        public async Task<Dictionary<string, object>> TarikAsync(int jumlah)
        {
            if (string.IsNullOrEmpty(_pin))
                throw new Exception("[ZakkiStore SDK Error] PIN transaksi diperlukan untuk melakukan transaksi tarik");

            var payload = new Dictionary<string, object> {
                { "token", _token },
                { "jumlah", jumlah },
                { "pin", _pin }
            };

            if (!string.IsNullOrEmpty(_iduser)) payload.Add("iduser", _iduser);
            if (!string.IsNullOrEmpty(_email)) payload.Add("email", _email);

            return await RequestAsync("/tarik", HttpMethod.Post, payload);
        }

        public async Task<Dictionary<string, object>> CheckmutasiAsync(string mutasiType = "all")
        {
            var payload = new Dictionary<string, object> { { "token", _token }, { "type", mutasiType } };
            if (!string.IsNullOrEmpty(_iduser)) payload.Add("iduser", _iduser);
            if (!string.IsNullOrEmpty(_email)) payload.Add("email", _email);

            return await RequestAsync("/checkmutasi", HttpMethod.Get, payload);
        }

        // ==========================================================
        // --- 4. NOKTEL MARKETPLACE (OTP VIRTUAL) ---
        // ==========================================================

        public async Task<Dictionary<string, object>> NoktelStokAsync()
        {
            return await RequestAsync("/noktel/stok", HttpMethod.Get, new { token = _token });
        }

        public async Task<Dictionary<string, object>> NoktelBuyAsync(string category)
        {
            return await RequestAsync("/noktel/buy", HttpMethod.Post, new { token = _token, category = category.Trim() });
        }

        public async Task<Dictionary<string, object>> NoktelGetOtpAsync(string accountID)
        {
            return await RequestAsync("/noktel/getotp", HttpMethod.Get, new { token = _token, account_id = accountID.Trim() });
        }

        public async Task<Dictionary<string, object>> NoktelCancelAsync(string invoiceID)
        {
            return await RequestAsync("/noktel/cancel", HttpMethod.Post, new { token = _token, invoice_id = invoiceID.Trim() });
        }

        public async Task<Dictionary<string, object>> NoktelHistoryAsync()
        {
            return await RequestAsync("/noktel/history", HttpMethod.Get, new { token = _token });
        }

        // ==========================================================
        // --- 5. REWARD KOMPUTASI & GAME ---
        // ==========================================================

        public async Task<Dictionary<string, object>> CekminingAsync()
        {
            return await RequestAsync("/cekmining", HttpMethod.Get, new { token = _token });
        }

        public async Task<Dictionary<string, object>> MyminingAsync()
        {
            return await RequestAsync("/mymining", HttpMethod.Get, new { token = _token });
        }

        public async Task<Dictionary<string, object>> CekgachaAsync()
        {
            return await RequestAsync("/cekgacha", HttpMethod.Get, new { token = _token });
        }

        // ==========================================================
        // --- 6. UTILITY & SECURITY ---
        // ==========================================================

        public async Task<Dictionary<string, object>> WhitelistipAsync(string ip)
        {
            return await RequestAsync("/whitelistip", HttpMethod.Post, new { token = _token, ip = ip.Trim() });
        }

        public async Task<Dictionary<string, object>> DelwhitelistipAsync(string ip)
        {
            return await RequestAsync("/delwhitelistip", HttpMethod.Post, new { token = _token, ip = ip.Trim() });
        }

        public async Task<Dictionary<string, object>> LeaderboardAsync(int limit = 10, string period = "all")
        {
            return await RequestAsync("/leaderboard", HttpMethod.Get, new { limit, period = period.Trim() });
        }

        public async Task<Dictionary<string, object>> StatusAsync()
        {
            return await RequestAsync("/status", HttpMethod.Get);
        }
    }
}
