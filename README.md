# ⛳ Velocity Mini-Golf 3D (Mobile & Web Edition)

Modern, optimize ve mobil öncelikli (Mobile-First) 3 Boyutlu Mini-Golf Oyunu.  
Three.js / WebGL ve Unity destekli, iPhone Safari ve masaüstü tarayıcılarda 60–120 FPS akıcı oynanış sunar.

![License](https://img.shields.io/badge/license-MIT-green.svg)
![Platform](https://img.shields.io/badge/platform-iOS%20%7C%20Web%20%7C%20macOS-blue.svg)
![Three.js](https://img.shields.io/badge/Three.js-r128-orange.svg)

---

## 🎮 Özellikler

### 1. 3 Boyutlu Sahalar ve Parkur Dinamikleri
- **1. Delik - Çam Vadisi:** Geniş fairway, hafif eğim, pinball tamponu ve kum tuzağıyla klasik başlangıç parkuru.
- **2. Delik - Değirmen & Kum Tuzağı:** Köşe dönüşünde gerçek zamanlı dönen ahşap kanatlı engel ve dar viraj.
- **3. Delik - Zirve & Pinball Tamponları:** Eğimli rampa tırmanışı, üst plato ve çift tampon engelleri.

### 2. İleri Seviye 3D Görsellik
- **Gerçekçi 3D Çukur ve Hedef Halkası:** Delik yalnızca düz bir siyah yuvarlak değildir; içine topun fiziksel olarak düştüğü silindirik bir çukura, beyaz turnuva halkasına ve şampiyona bayrağına sahiptir.
- **Sedir Ağacı Ahşap Duvarlar:** Belirgin ahşap damarları, 3 kademeli kalas pahları, doğal budaklar ve metalik perçin detayları.
- **Turnuva Fairway Çimi:** Alternatif çim biçme yön çizgileri (mower striping), 4 tonlu 1.800 adet organik çim yaprağı ve sıfır Moiré / dalgalanma optimizasyonu.

### 3. Profesyonel Sapan Nişan Alma Sistemi
- Topa dokunup geri çekildiğinde güç oranına göre dinamik olarak 1'den 9'a kadar artan **3 Boyutlu Chevron Oklar**.
- Düşük güçte yeşil, orta güçte elektrik sarısı, yüksek güçte ateş kırmızısı renk geçişi.
- Kamera nişan anında kilitlenir; ekranda serbest kaydırmayla kamera açısı (Orbit) pürüzsüzce döndürülebilir.

### 4. Ses ve Haptic (Titreşim) Motoru
- Web Audio API tabanlı sentetik putt vuruş sesi, ahşap sekme sesi, pinball tamponu sesi ve delik tamamlama fanfarı.
- iOS Safari & Native iOS Taptic Engine uyumlu fiziksel titreşim desteği.

---

## 🚀 Canlı Oynama / Kurulum

### Doğrudan Tarayıcıda Çalıştırma:
Herhangi bir statik sunucuyla `index.html` dosyasını sunmanız yeterlidir:

```bash
# Python ile yerel sunucu başlatma
python3 -m http.server 8080
```

Ardından tarayıcınızda açın:
- **Masaüstü:** `http://localhost:8080`
- **Mobil:** `http://<YEREL_IP_ADRESINIZ>:8080`

### 📱 iPhone'da Tam Ekran (PWA) Olarak Oynama:
1. iPhone Safari'de oyunu açın.
2. Alttaki **Paylaş** butonuna dokunun.
3. **Ana Ekrana Ekle (Add to Home Screen)** seçeneğini seçin.
4. Oyun ana ekranınızdan adres çubuğu olmadan tam ekran olarak açılır.

---

## 📂 Proje Yapısı

```
MiniGolf3D/
├── index.html           # Ana WebGL / Three.js oyun motoru ve arayüzü
├── WebBuild/            # Mobil & Web dağıtım paketi
├── Assets/              # Unity C# kaynak betikleri ve modeller
├── Packages/            # Unity paket yapılandırmaları
├── ProjectSettings/     # Proje ayarları
└── README.md            # Proje dokümantasyonu
```

---

## 🛠️ Teknolojiler
- **Three.js (r128)** - 3D Sahne, gölgelendirme, kamera ve materyal yönetimi
- **Web Audio API** - Düşük gecikmeli gerçek zamanlı ses sentezi
- **HTML5 / CSS3 / JavaScript** - Responsive arayüz, dokunmatik kontroller
- **Unity 6 & C#** - Alternatif yerel motor altyapısı

---

## 📄 Lisans
Bu proje [MIT](LICENSE) lisansı ile lisanslanmıştır.
