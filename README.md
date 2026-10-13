# SİP Portalı

USOM SİP API ile etkileşim kurmak amacıyla geliştirilmiş, **ASP.NET Core Blazor Web App (Interactive Server) ve .NET 10** tabanlı web portalı.

> **Proje durumu:** v1 — geliştirme aşamasındadır. Bu proje, ilgili resmi kurumların ürünü veya resmi uygulaması olduğu iddiasını taşımaz. Kurumsal ortamda kullanıma alınmadan önce erişim yetkilendirmesi, sır yönetimi, ağ gereksinimleri ve kurum politikaları gözden geçirilmelidir.

## Amaç

SİP API üzerinden erişilebilen kayıtların web arayüzünde görüntülenmesini ve desteklenen işlemlerin tek bir portal üzerinden yürütülmesini kolaylaştırmak.

## Özellikler

- **Dashboard:** Kurum bildirimleri ve siber olaylar için özet göstergeler.
- **Siber olay bildirimleri:** Bildirimleri listeleme, ayrıntıları görüntüleme ve desteklenen yeni bildirim işlemleri.
- **Zafiyetler:** Zafiyet kayıtlarını listeleme ve ayrıntılarını görüntüleme.
- **Güvenlik bildirimleri:** USOM güvenlik bildirimlerini görüntüleme.
- **Duyurular ve mesajlar:** Listeleme, ayrıntı görüntüleme ve desteklenen ekleri indirme.
- **Kullanıcılar ve SOME'ler:** İlgili kayıtları listeleme ve ayrıntılarını görüntüleme.
- **Tablolar:** Sayfalama, filtreleme ve mevcut sayfadaki kayıtları sıralama.
- **API ve proxy ayarları:** Token yapılandırması, bağlantı testi ve proxy adresi/portu ayarları.
- **Hata toleransı:** Proxy bağlantısı başarısız olduğunda hatayı kontrollü biçimde ele alma.

## Teknolojiler

- C# ve .NET 10
- ASP.NET Core Blazor Web App
- Interactive Server render mode
- `HttpClient` ile API iletişimi
- JSON yapılandırması ve ortam değişkenleri

## Gereksinimler

- .NET 10 SDK
- SİP API'ye ağ erişimi
- Yetkili ve geçerli bir SİP API token'ı
- Kurum ağı gerekiyorsa doğru proxy adresi, portu ve kimlik doğrulama ayarları

## Kurulum ve çalıştırma

Depoyu klonlayın:

```bash
git clone <REPOSITORY_URL>
cd <REPOSITORY_DIRECTORY>/SipPortal
```

Bağımlılıkları geri yükleyin ve uygulamayı başlatın:

```bash
dotnet restore
dotnet run
```

Konsolda gösterilen yerel URL'yi tarayıcıda açın. Visual Studio ile çalıştırmak için `SipPortal.csproj` dosyasını açıp projeyi başlatabilirsiniz.

## SİP API token yapılandırması

Token'ı kaynak koduna, `appsettings.json` dosyasına veya Git deposuna eklemeyin. Geliştirme ortamında .NET User Secrets kullanılabilir.

Proje dizininde terminal açın:

```bash
dotnet user-secrets set "SipApi:Token" "SIP_API_TOKENINIZ"
```

Gerekirse kimlik doğrulama başlığı ve şeması ortam değişkenleriyle tanımlanabilir.

**PowerShell:**

```powershell
$env:SIP_API_AUTH_HEADER = "Authorization"
$env:SIP_API_AUTH_SCHEME = "Bearer"
$env:SIP_API_TOKEN = "SIP_API_TOKENINIZ"
```

User Secrets, geliştirme kolaylığı içindir; şifreli bir kasa değildir ve üretim ortamında kurumsal sır yönetimi çözümünün yerini tutmaz. Üretimde uygun bir secret manager veya güvenli dağıtım yöntemi kullanın.

## Proxy yapılandırması

Uygulamanın Ayarlar ekranından proxy kullanımını, sunucu adresini ve portunu yapılandırabilirsiniz. Kurumunuzun verdiği gerçek proxy bilgilerini kullanın. Proxy adı DNS üzerinden çözümlenemiyorsa veya sunucuya erişilemiyorsa bağlantı başarısız olur; bu durum için önce adres, port, DNS ve ağ erişimini kontrol edin.

## Güvenlik notları

- Gerçek API token'larını, parolaları, kurum içi adresleri veya hassas kayıtları GitHub'a yüklemeyin.
- Paylaşımdan önce `appsettings.json`, günlükler, ekran görüntüleri ve örnek yapılandırmaları hassas bilgi açısından kontrol edin.
- Uygulamayı internete açık biçimde yayımlamadan önce kimlik doğrulama, yetkilendirme, güvenli hata yönetimi, günlükleme ve dağıtım ayarlarını değerlendirin.
- Sadece erişmeye yetkili olduğunuz API ve verilerle kullanın.

## İsim, marka ve kurumsal haklar

**SOME ve USOM adları, kısaltmaları, logoları, kurumsal kimlikleri ve bunlarla ilişkili haklar bu yazılım lisansı kapsamında devredilmez.** Bu isim ve işaretlerin kullanımına ilişkin haklar ilgili hak sahiplerinde saklıdır.

Bu projeye ilişkin metinlerde, lisans bildirimlerinde ve proje dokümantasyonunda belirtilen çerçevede, **SOME/USOM isim hakları ile proje üzerinde değişiklik yapılmasına ilişkin nihai yetki ve inisiyatifin Siber Güvenlik Başkanlığı tarafından saklı tutulması amaçlanmaktadır.** Bu ifade, Başkanlığın resmi onayı veya hukuki yetkilendirmesi bulunduğuna dair bir beyan değildir; yayımdan önce kurumun yetkili birimleri tarafından onaylanmalıdır.

Proje, aksi açıkça ve yazılı olarak belirtilmedikçe, USOM'un, SOME'lerin veya Siber Güvenlik Başkanlığının resmi ürünü ya da onaylı bir uygulaması olarak sunulmamalıdır.

## Lisans

Bu proje, depodaki [`LICENSE`](LICENSE) dosyasında belirtilen **SİP Portalı Kullanım ve Atıf Lisansı** koşullarıyla sunulmaktadır.

Lisansın temel amacı, yazılımın kullanımına ve paylaşılmasına izin verirken geliştirici/proje atfının ve lisans bildirimlerinin korunmasını şart koşmaktır. SOME/USOM isim ve marka hakları ile değişikliklere ilişkin kurum yetkisindedir.

> **Not:** Bu, standart MIT/Apache-2.0 gibi OSI onaylı bir açık kaynak lisansı değildir. Kurumsal isim hakları ve değişiklik yetkisiyle ilgili hükümlerin hukuken uygulanabilirliği için yayımdan önce hukuk ve ilgili kurum birimlerinden görüş alınması önerilir.

## Katkı ve hata bildirimi

Hata bildirimi veya öneri paylaşırken API token'ı, kişisel veri, kurum içi bilgi ya da hassas kayıt eklemeyin. Değişiklik göndermeden önce lisans koşullarını ve kurumun isim/marka kullanım kurallarını inceleyin.

---

**Sürüm:** v1  