# TRadeTurk Kullanma Kilavuzu

Bu kilavuz, TRadeTurk uygulamasini yerel ortamda calistirmak ve temel ozelliklerini kullanmak icin hazirlanmistir. TRadeTurk, sanal bakiye ile kripto piyasalarinda alim-satim ve emir denemeleri yapabileceginiz demo bir trading platformudur.

## 1. Uygulamayi Calistirma

TRadeTurk iki parcadan olusur:

- Backend API: .NET Web API
- Frontend: React/Vite arayuzu

Uygulamayi kullanmak icin once backend API'yi, sonra frontend'i calistirmaniz gerekir.

### Gereksinimler

Yerel ortamda calistirmak icin asagidaki araclar kurulu olmalidir:

- .NET SDK
- Node.js ve npm
- SQL Server LocalDB veya SQL Server

Kurulumlari kontrol etmek icin terminalde su komutlari calistirabilirsiniz:

```bash
dotnet --version
node --version
npm --version
```

### Backend API'yi Calistirma

Proje ana klasorunde terminal acin:

```bash
cd %USERPROFILE%\Desktop\YmUygulamalar2\TradeTurk
```

Paketleri yukleyip projeyi derleyin:

```bash
dotnet restore TRadeTurk.slnx
dotnet build TRadeTurk.slnx
```

Ardindan backend API'yi baslatin:

```bash
dotnet run --project src/TRadeTurk.WebAPI --urls http://localhost:5129
```

Backend basarili sekilde acildiginda API su adreste calisir:

```text
http://localhost:5129
```

API'nin calistigini kontrol etmek icin tarayicida veya terminalde su endpoint'i deneyebilirsiniz:

```text
http://localhost:5129/api/markets/symbols
```

Bu endpoint desteklenen kripto sembollerini JSON olarak dondurmelidir.

### SQL Server LocalDB Notu

Varsayilan gelistirme ayarlarinda uygulama su connection string ile LocalDB kullanir:

```text
Server=(localdb)\mssqllocaldb;Database=TRadeTurkDb;Trusted_Connection=True;MultipleActiveResultSets=true
```

Backend calisirken LocalDB hatasi alirsaniz asagidaki komutu deneyin:

```bash
sqllocaldb start MSSQLLocalDB
```

Daha sonra backend komutunu tekrar calistirin:

```bash
dotnet run --project src/TRadeTurk.WebAPI --urls http://localhost:5129
```

Uygulama acilirken EF Core migration'lari otomatik calistirir ve veritabani semasini gunceller.

### Frontend'i Calistirma

Ayrica yeni bir terminal acin ve frontend klasorune gecin:

```bash
cd %USERPROFILE%\Desktop\YmUygulamalar2\TradeTurk\frontend
```

Ilk kez calistiriyorsaniz npm paketlerini yukleyin:

```bash
npm install
```

Frontend dev server'i baslatin:

```bash
npm run dev -- --host 127.0.0.1 --port 5173
```

Frontend basarili sekilde acildiginda tarayicidan su adrese gidin:

```text
http://127.0.0.1:5173/
```

Frontend varsayilan olarak backend API'ye su adres uzerinden baglanir:

```text
http://localhost:5129
```

Bu ayar `frontend/src/config.ts` dosyasinda tanimlidir. Farkli bir API adresi kullanmak isterseniz `VITE_API_BASE_URL` ortam degiskenini ayarlayabilirsiniz.

### Docker ile Calistirma

Docker kullanmak isterseniz proje ana klasorunde su komutu calistirabilirsiniz:

```bash
docker compose up --build
```

Docker Compose su servisleri baslatir:

- SQL Server
- Backend API
- Frontend

Docker ile calistirdiktan sonra adresler:

```text
Frontend: http://localhost:5173
Backend:  http://localhost:5129
```

Docker kullanirken `.env` dosyalarini olusturmaniz gerekebilir:

```bash
copy .env.example .env
copy frontend\.env.example frontend\.env
```

### Hizli Calistirma Ozeti

Backend:

```bash
cd %USERPROFILE%\Desktop\YmUygulamalar2\TradeTurk
sqllocaldb start MSSQLLocalDB
dotnet run --project src/TRadeTurk.WebAPI --urls http://localhost:5129
```

Frontend:

```bash
cd %USERPROFILE%\Desktop\YmUygulamalar2\TradeTurk\frontend
npm install
npm run dev -- --host 127.0.0.1 --port 5173
```

Tarayicida:

```text
http://127.0.0.1:5173/
```

## 2. Uygulamaya Giris

Uygulama calistiginda tarayicidan asagidaki adrese gidin:

```text
http://127.0.0.1:5173/
```

Backend API varsayilan olarak su adreste calisir:

```text
http://localhost:5129
```

Ilk ekranda giris ve kayit formu gorunur. Daha once hesap olusturmadiysaniz once kayit olmaniz gerekir.

## 3. Yeni Hesap Olusturma

Kayit ekraninda asagidaki bilgileri girin:

- Ad soyad
- Email adresi
- Kullanici adi
- Sifre

Sifre en az 8 karakter olmalidir.

Kayit tamamlandiginda sistem otomatik olarak:

- kullanici hesabi olusturur,
- 50.000 USDT demo bakiye tanimlar,
- sanal kart olusturur,
- sizi uygulamaya giris yapmis hale getirir.

Bu uygulama demo amaclidir. Gercek para veya gercek borsa islemi yapmaz.

## 4. Giris Yapma

Daha once hesap olusturduysaniz giris ekraninda:

- email veya kullanici adi,
- sifre

bilgilerini girerek oturum acabilirsiniz.

Oturum acildiktan sonra uygulama token bilgisini tarayici oturumunda saklar. Oturum gecersiz hale gelirse uygulama sizi tekrar giris ekranina yonlendirebilir.

## 5. Ana Panel

Giris yaptiktan sonra ana panel acilir. Bu ekranda genel olarak sunlari gorebilirsiniz:

- toplam portfoy degeri,
- kullanilabilir USDT bakiyesi,
- secili kripto paranin guncel fiyati,
- 24 saatlik degisim bilgileri,
- favori marketler,
- emir formu,
- acik emirler,
- son islemler,
- portfoy analizi,
- portfoy gecmis grafigi.

Ust kisimdaki arama alanindan desteklenen kripto marketleri arayabilirsiniz. Ornek aramalar:

```text
BTC
ETH
SOL
BNB
```

Desteklenen semboller:

- BTCUSDT
- ETHUSDT
- BNBUSDT
- SOLUSDT
- XRPUSDT
- ADAUSDT
- DOGEUSDT
- AVAXUSDT
- DOTUSDT
- LINKUSDT
- MATICUSDT
- LTCUSDT
- TRXUSDT
- ATOMUSDT
- NEARUSDT

## 6. Market Secme ve Favoriler

Bir marketi secmek icin:

1. Arama kutusuna sembol veya coin adini yazin.
2. Listeden ilgili marketi secin.
3. Secili market dashboard ve emir formunda aktif hale gelir.

Favori marketler, sik takip ettiginiz pariteleri daha hizli izlemenizi saglar. Yildiz butonlariyla marketleri favorilere ekleyebilir veya favorilerden cikarabilirsiniz.

## 7. Alim ve Satim Islemleri

Dashboard uzerindeki emir formunda once islem yonunu secin:

- Alis
- Satis

Ardindan:

- market sembolunu,
- emir tipini,
- miktari,
- gerekiyorsa fiyat veya trigger fiyatini

girin.

Islem tamamlandiginda cüzdan, portfoy, islem gecmisi ve emir listeleri otomatik olarak guncellenir.

## 8. Emir Tipleri

TRadeTurk dort emir tipini destekler.

### Market Emir

Market emir, mevcut guncel fiyat uzerinden hemen calistirilmaya calisilir.

Kullanilacak alanlar:

- sembol
- alis/satis yonu
- miktar

### Limit Emir

Limit emir, belirlediginiz fiyata ulasilinca calisir.

Alis limit emri:

- piyasa fiyati belirlediginiz limit fiyata veya altina indiginde calisir.

Satis limit emri:

- piyasa fiyati belirlediginiz limit fiyata veya ustune ciktiginda calisir.

Kullanilacak alanlar:

- sembol
- alis/satis yonu
- miktar
- limit fiyat

### Stop-Loss Emir

Stop-loss emir, zarar sinirlamak icin kullanilir.

Satis stop-loss emri:

- fiyat trigger seviyesine veya altina indiginde calisabilir.

Kullanilacak alanlar:

- sembol
- satis yonu
- miktar
- trigger fiyat

### Take-Profit Emir

Take-profit emir, kar almak icin kullanilir.

Satis take-profit emri:

- fiyat trigger seviyesine veya ustune ciktiginda calisabilir.

Kullanilacak alanlar:

- sembol
- satis yonu
- miktar
- trigger fiyat

## 9. Acik Emirler

Acik emirler bolumunde henuz tamamlanmamis bekleyen emirleri gorebilirsiniz.

Buradan:

- emrin sembolunu,
- yonunu,
- tipini,
- miktarini,
- fiyat veya trigger bilgisini,
- durumunu

takip edebilirsiniz.

Bekleyen emirleri iptal edebilirsiniz. Sadece bekleyen emirler iptal edilebilir; tamamlanmis veya basarisiz emirler iptal edilemez.

## 10. Islem Gecmisi

Islem gecmisi ekraninda tamamlanan alim-satim islemlerini gorebilirsiniz.

Bu ekranda:

- islem tipi,
- sembol,
- miktar,
- fiyat,
- toplam deger,
- tarih,
- durum

bilgileri listelenir.

Filtreleme secenekleriyle tum islemleri, sadece alislari veya sadece satislari gorebilirsiniz.

## 11. Cuzdanim

Cuzdanim ekraninda demo bakiyenizi ve varliklarinizi takip edebilirsiniz.

Bu ekranda:

- toplam portfoy degeri,
- kullanilabilir USDT bakiyesi,
- sanal kart bilgileri,
- sahip olunan kripto varliklar,
- ortalama maliyet,
- guncel varlik degeri

goruntulenir.

Sanal kart demo amaclidir ve gercek odeme islemlerinde kullanilmaz.

## 12. Portfoy Analizi

Portfoy bolumleri, varliklarinizin genel durumunu anlamaniza yardimci olur.

Gorebileceginiz metrikler:

- toplam portfoy degeri,
- toplam varlik degeri,
- kullanilabilir USDT,
- toplam kar/zarar,
- gerceklesmemis kar/zarar,
- en iyi performans gosteren varlik,
- en zayif performans gosteren varlik,
- varlik dagilimi.

Portfoy gecmis grafiginde farkli zaman araliklari secilebilir:

- 1D
- 7D
- 1M
- 3M
- 1Y

## 13. Ayarlar

Ayarlar ekraninda profil ve guvenlik bilgilerinizi guncelleyebilirsiniz.

Profil bolumunde:

- ad soyad,
- email,
- kullanici adi,
- tercih edilen para birimi

bilgileri guncellenebilir.

Guvenlik bolumunde mevcut sifrenizi girerek yeni sifre belirleyebilirsiniz.

## 14. Cikis Yapma

Sol menudeki veya kullanici panelindeki cikis butonunu kullanarak oturumu kapatabilirsiniz.

Cikis yapildiginda:

- refresh token iptal edilmeye calisilir,
- tarayicidaki oturum bilgileri temizlenir,
- tekrar giris ekranina yonlendirilirsiniz.

## 15. Sik Karsilasilan Durumlar

### API sunucusuna ulasilamiyor hatasi

Backend servisinin calistigindan emin olun:

```text
http://localhost:5129
```

### Fiyatlar gelmiyor

Backend calisiyor olsa bile Binance verisi gecici olarak alinamayabilir. Uygulamada demo fallback fiyat mekanizmasi vardir; kisa sure sonra fiyatlar gorunebilir.

### Kayit veya giris basarisiz

Sunlari kontrol edin:

- email formati dogru mu?
- sifre en az 8 karakter mi?
- kullanici adi en az 3 karakter mi?
- ayni email veya kullanici adi daha once kullanilmis mi?

### Emir olusturulamiyor

Olası nedenler:

- miktar sifir veya negatif girilmis olabilir,
- alis icin yeterli USDT bakiyesi olmayabilir,
- satis icin yeterli kripto varlik olmayabilir,
- limit veya trigger fiyat girilmemis olabilir,
- secilen sembol desteklenmiyor olabilir.

## 16. Demo Kullanim Senaryosu

Asagidaki akisi izleyerek uygulamayi hizlica deneyebilirsiniz:

1. Yeni hesap olusturun.
2. Dashboard ekraninda BTCUSDT marketini secin.
3. Market emir tipiyle kucuk bir BTC alimi yapin.
4. Cuzdanim ekraninda BTC varliginizin eklendigini kontrol edin.
5. Islem gecmisi ekraninda alim islemini gorun.
6. Dashboard'a donup SOLUSDT veya ETHUSDT gibi baska bir market secin.
7. Limit emir olusturun.
8. Acik emirler bolumunde emri takip edin.
9. Isterseniz bekleyen emri iptal edin.
10. Ayarlar ekranindan profil bilginizi guncelleyin.

## 17. Onemli Notlar

- TRadeTurk demo ve egitim amaclidir.
- Gercek para transferi yapmaz.
- Gercek borsa emri gondermez.
- Fiyat verileri Binance uzerinden alinabilir, ancak sistem fallback demo fiyatlara da donebilir.
- Portfoy ve kar/zarar hesaplari demo islem verilerine gore hesaplanir.
- Uygulama yerel gelistirme ortami icin tasarlanmistir.
