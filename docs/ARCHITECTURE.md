# Loyiha tuzilishi

```
Assets/VRSimulator/
  Resources/
    VRSimulatorControls.inputactions   Mashqni boshqarish tugmalari
  Scripts/
    Runtime/                           O'yin mantiqi (VRSimulator.Runtime assembly)
      Core/        Topshiriq tizimi, hodisa hubi, input
      Interaction/ Buyumlar va ularni qo'yish joylari
      Platform/    Quest uchun ish vaqti sozlamalari
      UI/          VR ichidagi ko'rsatma paneli
    Editor/                            Avtomatlashtirish (VRSimulator.Editor assembly)
  Settings/                            URP aktivlari, topshiriqlar, materiallar
Assets/Scenes/VRSimulator.unity        Namunaviy mashq sahnasi (generatsiya qilinadi)
tools/                                 Windows sozlash skriptlari
```

## Asosiy qaror: XRI versiyasidan mustaqillik

`VRSimulator.Runtime` assembly **XR Interaction Toolkit'ga havola qilmaydi**.
XRI 2.x va 3.x orasida turlar nom maydonlari o'zgargan, shuning uchun unga
to'g'ridan-to'g'ri bog'lanish paket yangilanganda kompilyatsiya xatosi beradi.

Buning o'rniga:

- Buyum qo'yish **trigger kollider** orqali aniqlanadi (`PlacementZone`),
  XRI socket'lari orqali emas.
- Ushlash effektlari `InteractionFeedback` dagi ommaviy metodlarga ulanadi;
  ularni `XRGrabInteractable` ning `Select Entered` / `Select Exited`
  hodisalari chaqiradi. Ulanish sahna ichida bo'ladi, kodda emas.
- Editor skriptlari XRI turlarini nomi bo'yicha topadi
  (`EditorTypeUtility.FindType`), shuning uchun ikkala versiyada ham ishlaydi.

## Ma'lumot oqimi

```
PlacementZone (to'g'ri buyum qo'yildi)
      │  SimulationBus.RaiseCompleted("joyla_kalit")
      ▼
TaskRunner  ──► SimulationSession  (vaqt, xatolar, baho)
      │
      │  UnityEvent<string> / UnityEvent<float>
      ▼
TaskHud  ──►  VR ichidagi panel
```

`SimulationBus` — statik hodisa hubi. Zonalar va TaskRunner bir-birini
bilmaydi, shuning uchun yangi turdagi tekshiruv qo'shish uchun mavjud kodni
o'zgartirish shart emas: yangi komponent shunchaki `SimulationBus.RaiseCompleted`
ni chaqiradi.

## Yangi mashq qo'shish

1. `Assets > Create > VRSimulator > Mashq topshirig'i` — yangi `SimulationTask`.
2. Qadamlarni to'ldiring. Har bir qadamning `completionEventId` si sahnadagi
   biror `PlacementZone` ning `completionEventId` si bilan bir xil bo'lsin.
3. Sahnadagi `Mashq boshqaruvi` obyektidagi `TaskRunner > Task` maydoniga
   yangi topshiriqni qo'ying.

## Quest uchun ishlash bo'yicha qarorlar

| Sozlama | Qiymat | Sabab |
|---|---|---|
| Fixed Timestep | 1/72 s | Quest displey chastotasi bilan mos, fizika sakramaydi |
| Grafika API | faqat Vulkan | Quest 2/3 da eng tez, GLES3 zaxira sifatida kerak emas |
| Scripting backend | IL2CPP + ARM64 | Meta talabi |
| MSAA | 4x | VR'da qirralar juda ko'rinadi, mobil GPU'da MSAA arzon |
| HDR | o'chirilgan | Mobil GPU'da tekin emas, bu sahnada foyda bermaydi |
| Soyalar | faqat qattiq, past aniqlik, 25 m | Soft soyalar Quest uchun qimmat |
| Camera Depth/Opaque texture | o'chirilgan | Qo'shimcha o'tish (pass) tejaladi |
| vSync | o'chirilgan | Shlemning o'zi kadrlarni sinxronlaydi |
| Blit type | Never | Ortiqcha to'liq ekranli nusxa olish yo'q |

## Git bilan ishlash

`.gitattributes` Unity YAML fayllarini `UnityYAMLMerge` bilan birlashtiradi va
og'ir binar aktivlarni Git LFS ga yuboradi. `setup.bat` bularni sozlaydi.

Unity birinchi ochilishda har bir fayl uchun `.meta` yaratadi. Ular
**repoga qo'shilishi kerak** — GUID'lar shu fayllarda saqlanadi:

```
git add -A
git commit -m "Unity tomonidan yaratilgan .meta fayllar"
```
