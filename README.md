# 📖 Dyslexa — Aplikasi Edukasi Disleksia

Dyslexa adalah aplikasi game edukasi berbasis Unity untuk membantu anak-anak penderita disleksia melalui latihan interaktif yang adaptif.

---

## 🎯 Tujuan Aplikasi

Membantu anak-anak disleksia melatih kemampuan membaca melalui dua jalur latihan utama:
- **Fonologis** — kesadaran bunyi dan suku kata
- **Visual** — pengenalan huruf dan kesadaran spasi kata

---

## 🧩 Mode Gameplay (4 Mode dalam 1 Scene)

| Mode | Kategori | Stimulus | Cara Jawab |
|---|---|---|---|
| **Visual Letter Recognition** | Visual | Huruf teks besar | Klik huruf yang sama dari 4 pilihan |
| **Visual Spacing Awareness** | Visual | Kata utuh (BUKU) | Klik ejaan spasi yang benar |
| **Fonologis Blending** | Fonologis | Audio suku kata (auto-play) | Klik gambar benda yang sesuai |
| **Fonologis Segmenting** | Fonologis | Gambar benda | Drag suku kata ke slot urutan |

Saat memilih mode **Fonologis** → 1 sesi berisi 15 soal mix Blending + Segmenting.  
Saat memilih mode **Visual** → 1 sesi berisi 15 soal mix Letter Recognition + Spacing.

---

## 🗺️ Alur Navigasi

```
HomeScreen
 ├── [New Game] → NewGame → OnboardingAge → OnboardingGender → ChooseMode
 ├── [Continue] → ContinueGame → (pilih profil) → ChooseMode
 └── [Settings] → Settings Window → Export Excel / PDF

ChooseMode → [Fonologis / Visual] → LevelMap → [Klik Node] → GameSession
```

---

## 👤 Sistem Profil

- Setiap anak punya profil sendiri (Nama, Umur, Gender)
- Profil disimpan di `player_profiles.json` (offline, lokal)
- Profil dipilih dari layar **Continue Game** sebelum bermain
- Data profil bisa diekspor ke **Excel (CSV)** atau **HTML Report (PDF)** via Settings

---

## 📈 Sistem Adaptive

- Difficulty otomatis naik/turun berdasarkan akurasi sesi
- Jika akurasi ≥ 85% → difficulty naik
- Jika akurasi < 60% → difficulty turun
- Node berikutnya unlock jika akurasi ≥ 80%
- Semua progress tersimpan di `game_progress.json`

---

## 📁 Struktur Utama

Lihat [FILE_STRUCTURE.md](FILE_STRUCTURE.md) untuk detail lengkap semua script dan setup Unity.

---

## 🛠️ Tech Stack

- **Engine**: Unity (UI Toolkit: UGUI + TextMeshPro)
- **Language**: C#
- **Storage**: JSON (offline, `Application.persistentDataPath`)
- **Audio**: Unity AudioSource + Resources loader
- **Platform Target**: Android

---

## 🚀 Quick Start (Unity Editor)

1. Buka project di Unity
2. Tambahkan scene ke Build Settings (urutan: HomeScreen → ... → GameSession)
3. Pastikan folder `Resources/Images/` dan `Resources/Audio/` sudah berisi asset
4. Assign semua referensi di Inspector sesuai [FILE_STRUCTURE.md](FILE_STRUCTURE.md)
5. Tekan Play dari scene `HomeScreen`

### Debug Keys (saat Play Mode)
```
P → Tampilkan progress saat ini di Console
R → Reset semua progress
U → Unlock semua node
M → Set difficulty ke maksimum (5)
```
