# Ishga tushirish

## 1. Dasturlarni o'rnatish

Repo ildizidagi `setup.bat` ni ikki marta bosing. U Git LFS'ni yoqadi,
UnityYAMLMerge'ni ulaydi va kompyuter parametrlarini tekshirib
`system-report.txt` faylini yaratadi.

Kerakli dasturlar:

| Dastur | Versiya | Izoh |
|---|---|---|
| Unity Hub | oxirgi | https://unity.com/download |
| Unity Editor | 6000.0.x (Unity 6 LTS) | **Android Build Support** (OpenJDK + SDK & NDK) belgilangan holda |
| Visual Studio 2022 | 17.x | "Game development with Unity" workload |
| Meta Quest Link | ixtiyoriy | Play rejimida shlemni PC orqali sinash uchun |

Loyiha `ProjectSettings/ProjectVersion.txt` da `6000.0.51f1` deb yozilgan.
Sizda boshqa 6000.x versiya bo'lsa, Unity Hub ochishda ogohlantiradi —
"Continue" bosish yetarli, ziyon yo'q.

## 2. Loyihani Unity'da ochish

1. Unity Hub > Add > `E:\VRSimulator` papkasini tanlang.
2. Birinchi ochilish uzoq davom etadi: Package Manager `Packages/manifest.json`
   dagi paketlarni yuklab oladi va shaderlarni kompilyatsiya qiladi.
3. Konsolda pushti (magenta) materiallar ko'rinsa — bu normal, keyingi qadam tuzatadi.

## 3. Loyihani sozlash (bir marta)

Menyudan ketma-ket bajaring:

```
Tools > VRSimulator > 1. Loyihani sozlash
Tools > VRSimulator > 2. Namunaviy sahna yaratish
Tools > VRSimulator > 4. Sozlamalarni tekshirish
```

**1-qadam** URP aktivlarini yaratib ulaydi, Android/Quest Player sozlamalarini
qo'yadi (IL2CPP, ARM64, Vulkan, API 32+), sifat sozlamalarini VR uchun
kamtarona qiladi va XR Plug-in Management'da OpenXR yuklovchisini yoqadi.

**2-qadam** namunaviy sahnani yig'adi: xona, ish stoli, uchta ushlanadigan
asbob, ularni qo'yish joylari, VR ichidagi ko'rsatma paneli va topshiriq mantiqi.
Sahna `Assets/Scenes/VRSimulator.unity` ga saqlanadi va Build Settings ga qo'shiladi.

### Qo'lda bajarilishi kerak bo'lgan 2 ta qadam

Bu ikkisini Unity API orqali ishonchli avtomatlashtirib bo'lmaydi:

1. **Pult profili.** `Project Settings > XR Plug-in Management > OpenXR` >
   *Interaction Profiles* ga **Oculus Touch Controller Profile** qo'shing.
   Busiz pult tugmalari o'qilmaydi.
2. **XRI namunalari.** `Window > Package Manager > XR Interaction Toolkit >
   Samples` da **Starter Assets** ni Import qiling (standart input amallari va
   pult presetlari), shuningdek shlemsiz sinash uchun **XR Device Simulator** ni.

Shundan keyin 2-qadamni qayta ishga tushirsangiz, sahnadagi rig to'liq ulangan bo'ladi.

## 4. Sinash

- **Shlemsiz:** Play tugmasini bosing. XR Device Simulator ishlaydi —
  `WASD` yurish, sichqoncha qarash, `Enter` qadamni tasdiqlash, `R` qayta boshlash, `H` panelni yashirish.
- **Quest Link orqali:** shlemni USB/Air Link bilan ulang, Quest Link'ni ishga tushiring, Play bosing.
- **Qurilmada:** `Tools > VRSimulator > 3. Quest uchun APK yig'ish`.
  APK `Builds/Android/VRSimulator.apk` ga tushadi, keyin:

  ```
  adb install -r Builds\Android\VRSimulator.apk
  ```

Buyruq satridan build qilish (CI uchun):

```
"C:\Program Files\Unity\Hub\Editor\6000.0.51f1\Editor\Unity.exe" ^
  -quit -batchmode -projectPath . ^
  -executeMethod VRSimulator.Editor.QuestBuilder.BuildFromCommandLine
```

## Muammolarni bartaraf etish

| Belgi | Sabab va yechim |
|---|---|
| Hamma narsa pushti | URP aktivi ulanmagan — `1. Loyihani sozlash` ni bajaring |
| Play bosilganda ekran qora, shlem javob bermaydi | XR Plug-in Management > Android > OpenXR belgilanmagan |
| Pult ko'rinadi, lekin tugmalar ishlamaydi | Oculus Touch Controller Profile qo'shilmagan (yuqoriga qarang) |
| Pult umuman ko'rinmaydi | XRI **Starter Assets** import qilinmagan |
| Build "ARMv7 is not supported" deydi | Target Architectures = ARM64 bo'lsin (`1. Loyihani sozlash`) |
| APK o'rnatilmaydi: `INSTALL_FAILED_UPDATE_INCOMPATIBLE` | `adb uninstall com.aiziyrak.vrsimulator` keyin qayta o'rnatish |
| Sahna ochilganda skriptlar yo'qolgan | Paketlar yuklanmagan — Package Manager xatolarini tekshiring |
