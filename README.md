# VRSimulator

Meta Quest (mustaqil shlem, Android) uchun VR mashq simulyatori.

- **Dvigatel:** Unity 6 LTS, URP
- **XR:** XR Interaction Toolkit + OpenXR (Meta Quest profili)
- **Shlemsiz sinash:** XR Device Simulator

## Tez boshlash

1. `setup.bat` ni ikki marta bosing (Git LFS, UnityYAMLMerge, tizim tekshiruvi).
2. Loyihani Unity 6 LTS da oching.
3. `Tools > VRSimulator > 1. Loyihani sozlash`
4. `Tools > VRSimulator > 2. Namunaviy sahna yaratish`
5. Play bosing.

To'liq ko'rsatma va muammolarni bartaraf etish: **[docs/SETUP.md](docs/SETUP.md)**

## Nima ishlaydi

Namunaviy sahnada uchta asbobni belgilangan joylarga ketma-ket qo'yish mashqi bor.
Tizim vaqtni va xatolarni hisoblaydi, oxirida 100 ballik baho chiqaradi.

Mashqlar `SimulationTask` aktivlari sifatida saqlanadi — kod yozmasdan yangi
mashq qo'shish mumkin. Qanday qilib: **[docs/ARCHITECTURE.md](docs/ARCHITECTURE.md)**

## Menyu buyruqlari

| Buyruq | Vazifasi |
|---|---|
| 1. Loyihani sozlash | URP, Android/Quest Player sozlamalari, OpenXR yuklovchisi |
| 2. Namunaviy sahna yaratish | To'liq ishlaydigan mashq sahnasini yig'adi |
| 3. Quest uchun APK yig'ish | `Builds/Android/VRSimulator.apk` |
| 4. Sozlamalarni tekshirish | Quest uchun sozlamalar to'g'riligini tekshiradi |
