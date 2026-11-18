# Circus Match — Mimari Rehberi

> Projenin mimarisini, dosyaların yerini ve bir hamlenin sistem içindeki yolculuğunu anlatan Türkçe rehber. Kısa özet için [README](../README.md). İngilizce sürümü: [ARCHITECTURE.md](ARCHITECTURE.md).

## 1. Büyük resim

Oyun iki ana katmandan oluşur: **Core** oyunun kurallarını hesaplar, **Presentation** bu hesabı ekranda oynatır. İkisi birbirine sadece veri ile konuşur. Üçüncü, küçük bir katman olan **Level** bir level dosyasını okuyup başlangıç tahtasını kurar (bölüm 7).

Bir futbol maçı düşün. **Core hakemdir**: gol oldu mu, ofsayt var mı, skor kaç, hepsine karar verir ve sonucu bir kâğıda yazar. **Presentation televizyon yayınıdır**: o kâğıdı alır, tekrar görüntülerini, kamera açılarını, tezahüratı ekler. Yayın ne kadar yavaş ya da süslü olursa olsun skor değişmez. Hakem de kameranın nerede olduğunu bilmez.

Bu projede o kâğıdın adı `List<TurnStepData>`'dır: bir hamlede olan her şeyin sıralı listesi.

Bu ayrımın üç faydası var:

- **Test edilebilir.** Core, Unity'ye hiç dokunmadığı için sahne açmadan test edilir. 51 test bu yüzden saniyeler içinde çalışır.
- **Animasyon kuralları bozmaz.** Bir animasyonu hızlandırmak ya da efekt eklemek, oyunun sonucunu değiştiremez.
- **Tekrarlanabilir.** Rastgelelik `IRandom` üzerinden gelir. Aynı tohum (seed) verilirse aynı hamle her seferinde aynı sonucu üretir. Bot testleri bu sayede mümkün.

```mermaid
flowchart LR
    Input(["Player input<br/>swap · tap · booster"])
 subgraph System["System"]
    subgraph Core["Core — pure C#, no UnityEngine, deterministic"]
        direction TB
        CMD["ICommand<br/>Swap · Tap · Booster"]
        TR["TurnResolver<br/>match → clear → specials<br/>→ fall → refill → collect"]
        REG["Registries<br/>Shape · Special · Combo · Booster"]
        BRD["Board + Pieces<br/>PieceFlags · IRandom"]
        STEPS[["List&lt;TurnStepData&gt;<br/>the whole turn as data"]]
        CMD --> TR
        REG --- TR
        BRD --- TR
        TR --> STEPS
    end

   
    subgraph Pres["Presentation — Unity"]
        direction TB
        VCF["ViewCommandFactory<br/>steps → timed waves"]
        TP["TurnPlayer<br/>DOTween timeline"]
        PV["PieceView · Effects<br/>Activation/Combo Presenters"]
        UI["HUD · GoalFlyer<br/>MascotView · Popups"]
        VCF --> TP --> PV
        TP --> UI
    end

    BCtrl["BoardController<br/>input · turn flow · camera fit"]

    subgraph Lvl["Level — level data, no scene"]
        direction TB
        LS["ILevelSource<br/>LevelCatalog → LevelAsset"]
        LB["LevelBoardBuilder<br/>LevelElements · LevelValidator"]
        LS --> LB
    end

    Input --> BCtrl
    BCtrl -->|"0 · load level"| LS
    LB -->|"builds"| BRD
    BCtrl -->|"1 · build command"| CMD
    STEPS -->|"2 · steps (data only)"| VCF
    TP -->|"3 · turn done → next input"| BCtrl

    classDef core fill:#fde8e8,stroke:#c0392b,color:#222
    classDef pres fill:#e8f0fd,stroke:#2c5aa0,color:#222
    classDef ctrl fill:#fff4d6,stroke:#b8860b,color:#222
    class CMD,TR,REG,BRD,STEPS core
    class VCF,TP,PV,UI pres
    class BCtrl ctrl
    classDef lvl fill:#e8f6ea,stroke:#2e7d32,color:#222
    class LS,LB lvl
  end
```

Okları sırayla oku: bir level açılınca verisi yüklenir ve tahtaya dönüştürülür (0). Sonra dokunuş komuta döner, Core turu hesaplar, sonuç bir veri listesi olarak Presentation'a gider ve orada oynatılır. Animasyon bitince `BoardController` yeni girdiyi kabul eder.

## 2. Klasör haritası

Kodun tamamı `Assets/Scripts` altında, üç klasörde durur: `Core` (kurallar), `Level` (level verisi) ve `Presentation` (ekran). Her klasör kendi assembly'sidir: `CoreAssembly`, `LevelAssembly` ve Presentation için Unity'nin varsayılan assembly'si. Bağlar tek yönlüdür: Presentation Level'ı ve Core'u görür, Level sadece Core'u görür, Core hiçbir şeyi görmez. Bir dosya yanlış klasördeyse proje derlenmez. Bir şeyi bulmak için önce "bu kural mı, level verisi mi, görüntü mü?" diye sor.

**Core: oyunun kuralları (Unity yok)**

| Klasör | İçinde ne var | En önemli dosya |
| --- | --- | --- |
| `Core/` (kök) | Tahta, hedef türleri, yardımcı mantık | `Board.cs`, `LogicLibrary.cs` |
| `Core/Pieces` | Her taş türü ve bayrakları | `Piece.cs`, `PieceFlags.cs` |
| `Core/Command` | Oyuncu eylemleri: swap, tap, booster | `SwapCommand.cs` |
| `Core/TurnResolveSystem` | Turu hesaplayan beyin ve adım verileri | `TurnResolver.cs` |
| `Core/ShapeSystem` | Hangi şekil hangi özel taşı üretir | `ShapeDefinitions.cs` |
| `Core/SpecialActivation` | Özel taşların ve komboların etki alanları | `ActivationArea.cs` |
| `Core/Booster` | Booster'ların etkileri | `BoosterRegistry.cs` |
| `Core/RandomSystem` | Rastgelelik arayüzü | `IRandom.cs` |

**Level: level verisi ve tahta kurucu (sahne yok)**

| Klasör | İçinde ne var | En önemli dosya |
| --- | --- | --- |
| `Level/` (kök) | Level verisinden `Board` kurar | `LevelBoardBuilder.cs` |
| `Level/Data` | Level formatı, level asset'i, JSON, hedefler, validator | `LevelData.cs`, `LevelValidator.cs` |
| `Level/Elements` | Level'a konabilen her şey için bir sınıf ve registry'leri | `LevelElements.cs` |

**Presentation: ekranda görünen her şey (Unity)**

| Klasör | İçinde ne var | En önemli dosya |
| --- | --- | --- |
| `BoardView` | Tahtayı kuran, girdiyi alan, turu oynatan ana sınıflar | `BoardController.cs`, `TurnPlayer.cs` |
| `Command` | Adımları animasyon komutlarına çeviren fabrika ve komutlar | `ViewCommandFactory.cs` |
| `Presenter` | Her özel taşın ve kombonun animasyon tarifi | `ActivationPresenterRegistry.cs` |
| `EffectViews`, `Effects` | Roket, ışın, patlama, kıvılcım, kamera sarsıntısı | `EffectSpawner.cs` |
| `Level` | Level kataloğu ve ilerleme, hedef takibi, HUD, uçan ikonlar | `LevelCatalog.cs`, `GoalTracker.cs` |
| `Booster` | Booster butonları ve envanteri | `BoosterBarController.cs` |
| `Mascot` | Bobo'nun ruh hali | `MascotView.cs` |
| `Popups` | Kazanma ve kaybetme pencereleri | `PopupManager.cs` |
| `Menu`, `Scene` | Ana menü, açılış (Boot), sahne geçişi, yükleme ekranı | `Boot.cs`, `SceneLoader.cs`, `LoadingScreen.cs` |
| `Setting`, `Audio` | Ayarlar, titreşim, sesler | `GameSettings.cs`, `AudioManager.cs` |
| `VisualConfig`, `AnimationConfig` | Görsel ve zamanlama ayarları (ScriptableObject) | `GameAssets.cs`, `AnimationSettings.cs` |
| `Utils` | Referansları otomatik bulan yardımcı | `AutoRef.cs` |

**Kod dışındaki önemli klasörler**

- `Assets/Data/LevelAssets`: 20 level dosyası (`LevelAsset` asset'leri).
- `Assets/Data/VisualConfig`, `AnimationConfig`: görsel ve zamanlama ayar dosyaları.
- `Assets/Resources`: kodun adıyla yüklediği dosyalar. Şu an sadece `AudioConfig` burada. Klasör adı birebir `Resources` olmalı.
- `Assets/Sprites`: kullanıldıkları yere göre ayrılmış sprite'lar (`boards`, `particule`, `user_interface/common`, `user_interface/game`, `bobo_sprites`, `backgrounds`) ve sprite atlasları (bölüm 9).
- `Assets/AddressableAssetsData`: Addressables ayarları ve grupları (bölüm 9).
- `Assets/Prefabs`: taş, hücre, efekt ve UI prefab'ları.
- `Assets/Scenes`: `Boot` (build listesindeki tek sahne), ayrıca Addressables ile yüklenen `Menu` ve `Game`.
- `Assets/Tests`: Core ve Level testleri.
- `Assets/Scripts/Editor`: sadece editörde çalışan araçlar.

## 3. Bir hamlenin yolculuğu

Oyuncu iki taşı kaydırdığında tur, ekranda tek bir şey kıpırdamadan önce Core'da baştan sona hesaplanır. Sonra hesap animasyona çevrilip oynatılır. Adım adım:

1. **Parmak ekrana dokunur.** `BoardController.Update()` dokunulan dünya noktasını `TurnPlayer.WorldToCell()` ile hücreye çevirir.
   - Parmak yarım hücreden fazla kayarsa bu bir **swap**'tır. Hangi yöne en çok kaydıysa o komşu seçilir ve `TrySwap()` çağrılır. Parmağı kaldırmak gerekmez.
   - Parmak kaymadan kalkarsa bu bir **tap**'tir: özel taşa dokunulduysa `TryActivate()`, booster seçiliyse `TryUseBooster()` çağrılır.
   - Bir popup açıksa ya da önceki tur henüz oynuyorsa (`TurnPlayer.IsBusy`) girdi yok sayılır.
2. **Kontroller yapılır.** `TrySwap()` iki hücrenin tahtada olduğunu, taşların hareket edebildiğini (kilitli değil, delik değil) kontrol eder. Biri geçersizse hiçbir şey olmaz.
3. **Komut oluşturulur.** `new SwapCommand(board, x1, y1, x2, y2)`. Tap için `TapCommand`, booster için `BoosterCommand`. Komut "ne yapılmak isteniyor"un paketidir.
4. **Core turu hesaplar.** `TurnResolver.Resolve(board, command)` çağrılır. Bu tek çağrı bütün turu simüle eder (ayrıntısı bölüm 4'te): taşlar yer değiştirir, eşleşmeler bulunur, özel taşlar patlar, zincirleme patlamalar olur, taşlar düşer, yenileri gelir, toplar toplanır, yeni eşleşme kalmayana kadar döngü devam eder.
   - Sonuç: `List<TurnStepData>`, olan her şeyin sıralı listesi. Her adımın bir zamanı (milisaniye) vardır.
   - Swap eşleşme üretmediyse komut geri alınır (`Undo`) ve liste boş döner. Ekranda taşlar gidip geri gelir.
5. **Sonuç uygulanır.** `ApplyTurnResult()` hamle sayısını bir azaltır (booster'da azaltmaz) ve listedeki `GoalProgressData`'yı `GoalTracker`'a verir. Hedef sayıları veride hemen güncellenir.
6. **Olay yayınlanır.** `TurnStarted` event'i atılır. Maskot bunu dinler ve tur büyükse sevinir.
7. **Animasyona çevrilir.** `TurnPlayer.PlaySwapTurn()` önce iki taşın yer değiştirme animasyonunu oynatır, sonra `ViewCommandFactory.Build(steps)` ile adım listesini zaman damgalı animasyon komutlarına çevirir: "0,00 sn'de şu taşları patlat", "0,25 sn'de şu sütunu düşür" gibi.
8. **Animasyon oynar.** `TurnPlayer.PlayCommands()` komutları zamanı gelince başlatır. Her komut DOTween ile taşları hareket ettirir, efekt çağırır, ses çalar. Patlayan taşlardan hedefe giden ikonlar `GoalFlyer` ile uçar.
9. **Tur kapanır.** Animasyonlar bitince `PlayTurnAndCheckShuffle()` tahtada geçerli hamle kaldı mı diye bakar. Kalmadıysa tahta karıştırılır. Sonra `FinishTurn()`: hedefler bitti mi, hamle bitti mi?
   - Bittiyse uçan ikonların hepsinin varması beklenir, sonra Win ya da Lose popup'ı açılır.
   - Bitmediyse oyuncu bir sonraki hamleyi yapabilir.

**En önemli kural:** 4. adımda Core işini bitirdiğinde tahtanın yeni hali zaten belli. 7. ve 8. adımlar sadece bu sonucu gösteriyor. Animasyon sırasında hiçbir kural hesaplanmıyor.

## 4. Core katmanı: kuralların evi

Core'da Unity yoktur: `MonoBehaviour`, `GameObject`, `Vector3` kullanılamaz. Assembly ayarı (`noEngineReferences`) bunu zorunlu kılar, yanlışlıkla Unity kodu yazılırsa proje derlenmez.

### Tahta: `Board`

Tahta üç katmanlı bir ızgaradır. Bir pastanın katları gibi düşün:

| Katman | Ne tutar | Erişim | Örnek |
| --- | --- | --- | --- |
| Zemin (floor) | Taşın altındaki katman | `GetFloor(x, y)` | Çimen |
| Hücre (cell) | Taşın kendisi, hücre başına bir tane | `GetCell(x, y)` | Renkli taş, roket, kutu, top, delik |
| Üst katman (overlay) | Taşın üstüne kapanan katman, birden fazla hücre kaplayabilir | `GetOverlayPieceAt(x, y)` | Kilit, kafes |

- Koordinatlar: `x` soldan sağa, `y` **yukarıdan aşağı** artar. `y = 0` en üst sıradır. Taşlar `y`'si büyüyen yöne düşer.
- Her taşın benzersiz bir `Id`'si vardır (`board.NextId()`). Presentation taşları konumla değil bu Id ile tanır. Bir taş düştüğünde konumu değişir ama Id'si aynı kalır.
- `Board.Colors` bu tahtadaki renklerin listesidir. Yukarıdan gelen yeni taşlar sadece bu renklerden olur. Buna level karar verir (2 ile 4 renk arası); varsayılan dört rengin hepsidir.

### Taşlar ve bayraklar

Her taş `Piece` sınıfından türer ve şunları taşır: `Id`, `Color`, `Health` (kaç darbede kırılır), `GoalKind` (hangi hedefe sayılır) ve en önemlisi **`Flags`**.

Kurallar taşın türüne değil bayraklarına bakar. Kod "bu bir kutu mu?" diye sormaz, "bu taş hareket edebilir mi?" diye sorar. Yeni bir engel çoğu zaman sadece yeni bir bayrak kombinasyonudur.

| Bayrak | Anlamı |
| --- | --- |
| `Movable` | Kaydırılabilir ve düşebilir |
| `CanMatch` | Eşleşmeye katılır |
| `DamageableByNeighbor` | Yanında eşleşme olunca hasar alır |
| `DamageableBySpecial` | Özel taş patlaması hasar verir |
| `Locked` | Üstünde kilit ya da kafes var, hareket edemez |
| `Void` | Burada taş yok, delik |
| `Collectable` | Tahtanın dibine ulaşınca toplanır |

| Taş | Sınıf | Bayraklar | Can |
| --- | --- | --- | --- |
| Renkli taş | `ColoredPiece` | Movable, CanMatch | 1 |
| Özel taş (roket, TNT, pervane, ışık topu) | `SpecialPiece`, `RocketPiece` | Movable | 1 |
| Kutu | `BoxPiece` | DamageableByNeighbor | 3 |
| Top (oyuncak) | `ToyPiece` | Movable, Collectable | 1 |
| Delik | `HolePiece` | Void | – |
| Kilit (overlay) | `LockPiece` | DamageableByNeighbor, DamageableBySpecial | 1 |
| Kafes (overlay, 2×2) | `CagePiece` | DamageableBySpecial | 3 |
| Çimen (zemin) | `GrassPiece` | – | 1 |

### Turu hesaplayan beyin: `TurnResolver.Resolve()`

Bu fonksiyon bir komut alır ve turun tamamını hesaplayıp adım listesi döner. Sırasıyla şunları yapar:

1. **Komutu doğrula ve uygula.** Swap ya da booster geçersizse boş liste döner. Geçerliyse `command.Execute()` tahtayı değiştirir.
2. **İki özel taş swap edildiyse kombo.** İkisi birleşir (`ComboMergeData`), `ComboActivationRegistry` hangi kombo olacağını söyler.
3. **Bekleyen patlamaları işle.** Patlayacak özel taşlar bir kuyruğa (`pending`) girer. Her biri sırayla patlar. Patlama başka bir özel taşa denk gelirse o da kuyruğa eklenir. Zincirleme patlama böyle olur. Bu işi `ClearCellsAndEnqueueChains()` yapar.
4. **Eşleşme döngüsü.** `MatchLogic.FindMatches()` tahtadaki tüm eşleşmeleri bulur:
   - Eşleşen taşlar silinir, yanındaki kutu ve kilitler hasar alır, altındaki çimen kırılır.
   - Büyük şekiller (4'lü, 5'li, L/T, 2×2) yeni bir özel taş üretir (`SpecialCreatedData`).
   - `AddFillSteps()` taşları aşağı düşürür ve boş yerlere yenilerini koyar.
   - `TryCollectAtExits()` sütunun dibine ulaşan topları toplar. Boşalan yer tekrar doldurulur.
   - Yeni eşleşme kaldıysa döngü başa döner (kaskad). Kalmadıysa biter. Güvenlik için en fazla 100 tur döner.
5. **Eşleşme yoksa geri al.** İlk turda hiç eşleşme çıkmadıysa ve özel taş da patlamadıysa swap geri alınır (`Undo`).
6. **Hedef ilerlemesini ekle.** Listenin sonuna `GoalProgressData` eklenir: bu turda kaç kırmızı taş, kaç kutu, kaç top...

### Adım verileri: `TurnStepData`

Listedeki her eleman bir adımdır. Hepsi sadece veri taşır, hiçbiri bir şey çizmez.

| Adım | Ne anlatır |
| --- | --- |
| `ClearPieceData` | Şu taşlar şu zamanda silindi |
| `DamagePieceData` | Şu taş hasar aldı, kalan canı şu |
| `ClearOverlayData` | Kilit, kafes ya da çimen kırıldı |
| `SpecialCreatedData` | Şu taşlar birleşip şu özel taşı oluşturdu |
| `SpecialActivationData` | Şu özel taş patladı, şu hücreleri vurdu |
| `ComboMergeData` | İki özel taş birleşti |
| `BoosterActivationData` | Booster kullanıldı |
| `FallStepData` | Şu taşlar şuradan şuraya düştü |
| `RefillStepData` | Yukarıdan şu yeni taşlar geldi |
| `CollectStepData` | Şu toplar toplandı |
| `ShuffleStepData` | Tahta karıştırıldı |
| `GoalProgressData` | Bu turun hedef sayıları |

Patlama adımlarındaki her vuruşun bir zamanı vardır (`HitData.TimeMs`). Roket soldan sağa giderken her hücreyi biraz daha geç vurur. Bu zamanların hepsi `ActivationTiming.cs`'te durur.

### Yardımcı mantık: `LogicLibrary.cs`

| Sınıf | İşi |
| --- | --- |
| `BoardLogic` | Hücre geçerli mi, taş hareket edebilir mi, sütunun çıkışı nerede (`GetExitY`) |
| `MatchLogic` | Eşleşmeleri ve hasar alacak komşuları bulur |
| `OrganizationLogic` | Düşme (düz ve çapraz) ve yeni taşlarla doldurma |
| `SpecialActivationLogic` | Özel taşın hangi hücreleri vuracağını hesaplar |
| `MoveLogic` | Tahtada geçerli hamle var mı, ipucu için hangisi |
| `ShuffleLogic` | Hamle kalmayınca tahtayı karıştırır (topları hariç tutar) |

## 5. Özel taşlar, kombolar ve booster'lar: registry sistemi

Her özel taş, kombo ve booster kendi küçük sınıfıdır. Bir **registry** (kayıt defteri) "şu tür için şu sınıfı kullan" listesini tutar. Kod uzun `switch` blokları yerine registry'e sorar.

Bir restoranın menüsü gibi düşün. Garson (`TurnResolver`) yemeğin nasıl yapıldığını bilmez. Menüye bakar, siparişi doğru aşçıya iletir. Yeni bir yemek eklemek için garsonu eğitmezsin, menüye bir satır ve mutfağa bir aşçı eklersin.

| Registry | Anahtar | Kimi döndürür | Nerede |
| --- | --- | --- | --- |
| `ShapeRegistry` | Eşleşme şekli | Hangi özel taşın üretileceği (`ShapeTemplate.Produces`) | `Core/ShapeSystem` |
| `SpecialActivationRegistry` | `PieceKind` | Patlama etki alanı (`ISpecialActivation`) | `Core/SpecialActivation` |
| `ComboActivationRegistry` | İki `PieceKind` | Kombo etkisi (`IComboActivation`) | `Core/SpecialActivation` |
| `BoosterRegistry` | `BoosterType` | Booster'ın vurduğu hücreler (`IBoosterEffect`) | `Core/Booster` |
| `ActivationPresenterRegistry` | `PieceKind` | Patlamanın animasyon tarifi (`IActivationPresenter`) | `Presentation/Presenter` |
| `ComboPresenterRegistry` | İki `PieceKind` | Kombonun animasyon tarifi | `Presentation/Presenter` |
| `LevelElements` | Katman + harf | Level'a konan bir şeyi kontrol edip yerleştiren element (`ILevelElement`) | `Level/Elements` |

**Her özel taşın iki yarısı vardır.** Core yarısı (`RocketActivation`) "hangi hücreler, hangi zamanda vurulur" sorusunu cevaplar. Presentation yarısı (`RocketPresenter`) "bu ekranda nasıl görünür" sorusunu. İkisi birbirini tanımaz, aralarında sadece `SpecialActivationData` adımı gider.

### Şekiller

`ShapeDefinitions.RegisterAll()` tüm şekilleri `true/false` matrisleri olarak tanımlar. Her şeklin bir **önceliği** vardır: aynı taşlar birden fazla şekle uyuyorsa önceliği yüksek olan kazanır. Böylece 5'li bir sıra hem 4'lü hem 5'li sayılmaz, sadece ışık topu üretir.

| Şekil | Ürettiği |
| --- | --- |
| 4'lü sıra | Roket (sıranın yönüne göre yatay ya da dikey) |
| 5'li sıra | Işık topu |
| L ya da T | TNT |
| 2×2 kare | Pervane |

### Kombolar

`ComboActivationRegistry` anahtarı sıralar: Roket + TNT ile TNT + Roket aynı kayıttır. 10 kombinasyonun hepsi kayıtlı. Işık topunun roket, TNT ve pervaneyle komboları aynı sınıfı (`LightBallDefaultCombo`) kullanır: en yaygın rengi o türe çevirir.

Patlama alanlarının boyutları `ActivationArea.cs`'te sabit olarak durur (örneğin `TntRadius = 1` → 3×3, `TntComboRadius = 3` → 7×7). Bir alanı değiştirmek için sadece bu sayıyı değiştirirsin.

### Booster'lar

- **Hammer, Row, Column** Core'da `BoosterRegistry` üzerinden çalışır. `BoosterCommand` vurulacak hücreleri registry'den alır, sonra özel taş patlamasıyla aynı fonksiyonu (`ClearCellsAndEnqueueChains`) kullanır. Bu yüzden booster bir özel taşa denk gelirse onu da patlatır.
- **Shuffle** hücre seçmez. Butona basılınca hemen çalışır: `BoosterBarController` onu `InstantBoosterRequested` event'iyle bildirir, `BoardController` da `TurnResolver.ResolveShuffle()` çağırır.
- Booster sayıları `BoosterInventory`'de (`PlayerPrefs`) tutulur, her booster 3 adetle başlar. Booster kullanmak hamle harcamaz.

## 6. Presentation katmanı: ekrandaki oyun

Presentation, Core'un ürettiği adım listesini alıp zamanlanmış animasyonlara çevirir. Dört ana parçası vardır.

### `BoardController`: orkestra şefi

Game sahnesinin merkezidir ve her şeyi birbirine bağlar:

- **Level başı (`Start`):** `LevelProgress.CurrentIndex`'e göre `LevelCatalog`'dan level verisini alır, `Board`'u `LevelBoardBuilder` ile kurar, kamerayı tahtaya oturtur, her hücrenin, zeminin ve overlay'in görselini oluşturur (`SpawnBoardViews`), `GoalTracker`'ı ve HUD'u kurar.
- **Girdi (`Update`):** dokunuşları swap, tap ya da booster'a çevirir (bölüm 3).
- **Tur akışı:** komutu oluşturur, `TurnResolver`'a verir, sonucu uygular, `TurnPlayer`'a oynatır.
- **İpucu:** oyuncu bir süre hamle yapmazsa `MoveLogic` ile geçerli bir hamle bulup gösterir.
- **Oyun sonu:** kazanma ya da kaybetmeyi kontrol eder, popup'ı açar, yıldızı hesaplar (`CalculateStars`), +5 hamle devamını yönetir.
- **Kamera (`FitCameraToBoard`):** üst ve alt panelin ekrandaki gerçek kenarlarını ölçer, tahtayı aradaki boşluğa sığdırır. Her ekran oranında tahta panellerin altında kalmaz.
- **Event'ler:** `TurnStarted`, `MovesChanged`, `HintShown`, `LevelEnded`, `LevelResumed`. Maskot gibi parçalar bunları dinler. `BoardController` maskotu tanımaz.

### `TurnPlayer`: sahne yönetmeni

- Her taşın ekrandaki karşılığını (`PieceView`) Id'siyle bir sözlükte tutar: `GetView(pieceId)`. Adımlar taşları Id ile anlattığı için doğru görsel böyle bulunur.
- Hücre ile dünya koordinatı arasında çeviri yapar: `CellToWorld(x, y)` ve `WorldToCell(pos)`.
- Taş görsellerini ve efektleri havuzda (pool) tutar. Silinen taş yok edilmez, havuza döner ve yeni gelen taş için tekrar kullanılır. Mobilde çöp toplayıcının (GC) takmasını azaltır.
- Turu oynatır (`PlaySwapTurn`, `PlayTapTurn`, `PlayShuffleTurn`). Oynatma sürerken `IsBusy` true'dur ve yeni girdi kabul edilmez.

### `ViewCommandFactory`: zaman çizelgesi yapıcı

Adım listesini `(başlangıç zamanı, animasyon komutu)` çiftlerine çevirir. Bir film kurgucusu gibi çalışır: hangi sahnenin hangi saniyede başlayacağına karar verir.

Tur **dalgalar** (wave) halinde ilerler. Bir dalga şöyle işler:

1. **Vuruşlar.** Patlamalar ve silinmeler, Core'un verdiği milisaniyelerle yerleştirilir: `At(timeMs) = dalganın başı + timeMs / 1000`. Aynı anda vurulan taşlar tek bir `ClearPiecesViewCommand`'da toplanır. Her vuruş `MarkHit` ile "en son vuruş ne zaman" bilgisini günceller.
2. **Düşme.** Son vuruştan kısa bir süre sonra (`FallDelay`) taşlar düşmeye ve yenileri gelmeye başlar (`FallViewCommand`, `RefillViewCommand`).
3. **Dalga biter.** En uzun düşme bitince bir sonraki dalga (kaskad) oradan başlar (`EndWave`).

Sonuç: önce patlamalar biter, sonra taşlar düşer, sonra yeni eşleşmeler patlar. Her şey üst üste binmeden sırayla olur.

### Animasyon komutları: `IViewCommand`

Her komutun iki üyesi vardır: `Execute()` (animasyonu başlat) ve `Duration` (ne kadar sürer). `TurnPlayer.PlayCommands()` hepsini tek bir DOTween `Sequence`'e zamanı gelince çalışacak callback olarak dizer ve dizinin bitmesini bekler.

| Komut | Ne yapar |
| --- | --- |
| `ClearPiecesViewCommand` | Taşları patlatır, efekt ve ses çıkarır |
| `DamageViewCommand` | Kutu ya da kilide hasar animasyonu |
| `FallViewCommand`, `RefillViewCommand` | Taşları düşürür, yenileri yukarıdan getirir |
| `SpawnSpecialViewCommand`, `MergeToAnchorViewCommand` | Taşlar birleşip özel taşı oluşturur |
| `RocketFlyViewCommand`, `PropellerFlyViewCommand`, `TntExplosionViewCommand`, `LightBeamStartViewCommand`... | Özel taşların patlama gösterileri |
| `HammerStrikeViewCommand` | Çekiç darbesi |
| `CollectViewCommand` | Top zıplar ve hedefe uçar |
| `ShuffleViewCommand` | Tahta karışır |
| `ShakeViewCommand` | Kamera sarsılır |

Özel taşların gösterilerini factory doğrudan yazmaz. `ActivationPresenterRegistry`'den ilgili presenter'ı alır ve ondan komut ister (bölüm 5).

### Görsel ayarlar

- **`GameAssets`** (ScriptableObject): tüm prefab referansları ve level kataloğu tek yerde. Ona sadece Game sahnesi referans verebilir (bölüm 9).
- **`AnimationSettings`**: düşme hızı, patlama süresi, uçuş süresi gibi zamanlamalar. Hissi ayarlamak için koda değil buraya dokunursun.
- **`*VisualConfig`**: taş, kutu, kafes, kilit, çimen sprite'ları ve kırılma efektleri.
- **`AutoRef`**: sınıflar `OnValidate` içinde çocuklarını isimle (`AutoRef.Child<T>(this, "ui_text_moves")`), sahnedeki nesneleri (`AutoRef.InScene<T>()`) ve asset'leri (`AutoRef.Asset<T>()`) kendileri bulur. Inspector'da elle sürükleme neredeyse yok. Bu yüzden **obje isimleri önemli**: bir objeyi yeniden adlandırırsan referans kopar ve Console'da `[AutoRef] ... not found` uyarısı çıkar.

## 7. Level, hedefler ve HUD

Bir level sade bir veridir: tahtanın boyutu, hamle sayısı, renkler, hedefler ve her hücrede ne olduğu. Aynı veri projedeki bir asset'ten ya da ileride sunucudaki bir JSON dosyasından gelebilir. Hedeflerin takibi veride anında, ekranda ise ikonlar yerine vardıkça güncellenir.

### Level verisi: `LevelData`

Bir level metin satırları olarak kaydedilir: tahtanın her sırası için bir string, her hücre için bir harf. Tahtanın her katmanı için ayrı bir satır listesi vardır. Böylece bir hücrede aynı anda bir taş, altında bir zemin ve üstünde bir overlay olabilir.

```json
{
  "width": 5, "height": 4, "moves": 20,
  "colors": ["Red", "Yellow", "Blue", "Green"],
  "stars": { "two": 0, "three": 0 },
  "cells":    ["#...#", ".BOB.", ".....", "r...y"],
  "overlays": [".....", ".....", ".L.C.", "....."],
  "floors":   [".....", "g...g", ".....", "....."],
  "goals": [
    { "type": "ClearBox", "count": 2 },
    { "type": "CollectColor", "color": "Red", "count": 15 }
  ]
}
```

| Katman | Harf | Anlamı |
| --- | --- | --- |
| Hücre | `.` | Rastgele renkli taş |
| Hücre | `#` | Delik (hücre yok) |
| Hücre | `B` | Kutu |
| Hücre | `O` | Top |
| Hücre | `r` `y` `b` `g` | Sabit kırmızı, sarı, mavi ya da yeşil taş |
| Hücre | `R` `V` | Roket (yatay, dikey) |
| Hücre | `T` `P` `X` | TNT, pervane, ışık topu |
| Üst katman | `L` | Kilit |
| Üst katman | `C` | Kafes. Harf, 2×2 kafesin sol üst hücresidir |
| Zemin | `g` | Çimen |

Her katmanda `.` "burada özel bir şey yok" demektir. Aynı harf farklı katmanlarda farklı anlama gelebilir (`g` hücre katmanında yeşil taş, zemin katmanında çimen), çünkü katman da anahtarın parçasıdır.

- `colors`: bu level'ın renkleri, isimleriyle yazılır. Rastgele taşlar ve yukarıdan gelen yeni taşlar sadece bu renklerden olur.
- `stars`: 2 ve 3 yıldız için kaç hamle kalması gerektiği. `0` "varsayılanı kullan" demektir (bölüm 8).
- `goals`: hedef türü metin olarak yazılır. `color` sadece `CollectColor`'da kullanılır.

Hedef türleri (`GoalType`): `CollectColor` (renk topla), `ClearBox` (kutu kır), `ClearLock` (kilit kır), `ClearGrass` (çimen temizle), `CollectToy` (top topla).

### Level sisteminin dosyaları

| Dosya | Assembly | Görevi |
| --- | --- | --- |
| `LevelData` | Level | Yukarıdaki level formatı. Sade C#, JSON'a yazılabilir |
| `LevelAsset` | Level | Bir `LevelData` tutan ScriptableObject. Level'lar projede böyle saklanır |
| `ILevelSource` | Level | "Bana N. level'ı ver" arayüzü. Oyun sadece bunu bilir, level'ın nereden geldiğini bilmez |
| `LevelJson` | Level | `LevelData` ↔ JSON metni, online level'lar için |
| `ILevelElement`, `LevelElements` | Level | Yerleştirilebilen her şey için bir sınıf ve onu katman + harfle bulan registry |
| `LevelValidator` | Level | Bir level'ı kaydedilmeden ya da oynanmadan önce kontrol eder |
| `LevelBoardBuilder` | Level | `LevelData`'dan bir `Board` ve hedef listesi kurar |
| `LevelGoal` | Level | Oyunun kullandığı haliyle tek bir hedef: tür, renk, hedef sayısı |
| `LevelCatalog` | Presentation | Build'deki level'lar, sırasıyla. `ILevelSource`'u uygular. Kendini `Data/LevelAssets`'ten doldurur |
| `LevelProgress` | Presentation | Oyuncunun hangi level'da olduğu (`CurrentIndex`) ve yıldızları (`PlayerPrefs`) |

### Element'ler: her şey için bir sınıf

Bir level'a konabilen her şey bir **element**'tir: `HoleElement`, `BoxElement`, `ToyElement`, `ColorElement`, `SpecialElement`, `LockElement`, `CageElement`, `GrassElement`. Bir element kendisiyle ilgili her şeyi bilir:

| Üye | Örnek: kilit |
| --- | --- |
| `Code` ve `Layer` | Üst katmanda `'L'` |
| `DisplayName` | "Lock" (level editöründe görünür) |
| `AutoGoal` | `ClearLock`: kilit koymak bu hedefi önerir |
| `CanPlace(level, x, y, out reason)` | Delikte, kutunun üstünde ve kafesin içinde olamaz |
| `Place(board, x, y)` | Bir `LockPiece` ekler ve altındaki taşı kilitler |

Builder, validator ve level editörü kilidin ne olduğunu bilmez. `LevelElements`'ten element'i ister ve onu çağırır. Bölüm 5'teki registry fikrinin aynısı.

### Bir level nasıl tahtaya dönüşür

`LevelBoardBuilder.Build(level, random)` şu sırayla çalışır:

1. Boş bir `Board` oluşturur ve renklerini ayarlar.
2. **Hücre** katmanını yerleştirir: delikler, kutular, toplar, sabit renkler, özel taşlar.
3. Boş kalan her hücreyi level'ın renklerinden rastgele taşlarla doldurur (`FillAllEmptyCells`). Doldurma, başlangıçta eşleşme oluşturmaz.
4. **Zemin** katmanını yerleştirir (çimen).
5. **Üst katmanı** yerleştirir (kilit, kafes). Overlay'ler en son gelir, çünkü altlarındaki taşı kilitlerler. O taşın önceden var olması gerekir.

Bilinmeyen bir harf atlanmaz, hata fırlatır. Bozuk bir level yanlış oynanmamalı, hemen görünmeli. `BuildGoals(level)` metin hedefleri `GoalTracker` için `LevelGoal`'lara çevirir.

Sonra `BoardController.SpawnBoardViews()` tahtada ne varsa onun görselini oluşturur. Belirli bir engele özel kod yoktur: bir overlay'in boyutu ve konumu kapladığı hücrelerden hesaplanır, bu yüzden 2×2 kafes için özel bir şey gerekmez.

### Validator: `LevelValidator`

`Validate(level)` bir hata listesi döndürür. Liste boşsa level geçerlidir. Kontrol ettikleri:

- Boyut 3 ile 10 arasında, hamle 0'dan büyük, en az 2 geçerli renk var.
- Her satır listesinde doğru sayıda satır var, her satır doğru uzunlukta.
- Her harf bilinen bir element, ve her element'in `CanPlace` kuralı geçiyor.
- Her hedefin türü biliniyor ve sayısı 0'dan büyük. Renk hedefi level'ın renklerinden birini kullanıyor. `ClearBox 5` gibi bir hedef, tahtada 5'ten az kutu varsa reddedilir.

### `GoalTracker`: hedef muhasebecisi

- Her turun sonundaki `GoalProgressData`'yı alır (`ApplyProgress`) ve kalan sayıları düşer. `IsComplete()` bütün hedefler bitti mi diye cevap verir.
- Ayrıca Core'a **hedef önceliği** verir (`ITargetPriority.GetPriority`). Pervane nereye uçacağını seçerken bunu kullanır. Core `GoalTracker`'ı tanımaz, sadece `ITargetPriority` arayüzünü tanır.

| Öncelik | Hücre |
| --- | --- |
| 3 | Altı açılınca topu düşürecek hücre; sadece özel taşla kırılan overlay |
| 2 | Hedef olan kutu, kilit ya da çimen |
| 1 | Toplanacak renkteki taş |
| 0 | Hedefle ilgisi olmayan hücre (topun kendisi de 0'dır, çünkü kırılamaz) |

### HUD ve uçan ikonlar

- `HudController` üst paneldeki level numarasını, kalan hamleyi ve hedef kutucuklarını (`GoalSlotView`) yönetir.
- Bir hedef taşı silinince `GoalFlyer.TryFly()` onun ikonunu taşın yerinden kendi kutucuğuna uçurur. Kilit ve çimen gibi overlay'ler ikon yerine bir kıvılcım (`GoalSpark`) gönderir. Hangisinin ne göndereceğini `GoalIconConfig` söyler.
- Kutucuktaki sayı ikon **vardığında** azalır (`OnIconArrived`). Oyuncu böylece sayının ikonla birlikte düştüğünü görür.
- Level biterken `WaitForFlights()` havadaki tüm ikonların inmesini bekler (en fazla 2 sn). Sonra `SyncGoals()` sayıları veriyle eşitler ve popup açılır.
- Uçan ikonlar UI'ın önünde görünsün diye Canvas "Screen Space - Camera" modundadır ve uçanlar `SortingOrders.Flight` sırasında çizilir.

### Maskot: `MascotView`

Bobo, `BoardController`'ın event'lerini dinleyip ruh hali değiştirir. Kimse ona "sevin" demez, kendi karar verir.

- **Büyük tur** (ışık topu kombosu ya da 25+ taş) → çok sevinir. **İyi tur** (kombo, özel taş, top toplama ya da 10+ taş) → sevinir.
- **3 hamle kaldı** → endişeli. **1 hamle kaldı** → üzgün.
- Kazanınca şapka çıkarır, kaybedince üzülür. İpucu gösterildiğinde tepki verir. Uzun süre bir şey olmazsa kendi kendine hareket eder.
- Tüm eşikler ve sprite'lar `MascotConfig`'te durur.

## 8. Oyun akışı ve ekranlar

Oyun üç sahneden oluşur: **Boot**, **Menu** ve **Game**. Boot sadece oyunu başlatır. Menu ile Game arasındaki her geçiş `SceneLoader` üzerinden ve yükleme ekranıyla yapılır.

### Ekranlar arası yolculuk

1. **Uygulama açılır → Boot sahnesi.** Build listesindeki tek sahne bu. `Boot` Addressables'ı başlatır, yükleme ekranı prefab'ını yükleyip `LoadingScreen.Register()`'a verir, sonra `SceneLoader.LoadMenu()`'yü çağırır.
2. **Menu sahnesi.** `MenuController` logoyu ve Play butonunu animasyonla getirir. Butonda bir sonraki level yazar: "LEVEL 7". Online level'lar gelene kadar, son level bitince oyun Level 14'e döner (`LoopStartIndex`).
3. **Play'e basılır → yükleme ekranı → Game sahnesi.** `SceneLoader.LoadGame()` → `LoadingScreen.LoadScene("Game")`.
4. **Oyun oynanır.** Bölüm 3'teki döngü her hamlede tekrarlanır.
5. **Kazanılır → Win popup.** Yıldızlar gösterilir. Continue'ya basılınca `LevelProgress.CurrentIndex` bir artar ve Menu'ye dönülür.
6. **Kaybedilir → Lose popup.** Kalan hedefler gösterilir. İki seçenek var:
   - **+5 hamle:** level kaldığı yerden devam eder (`ContinueWithExtraMoves`). Level başına bir kez kullanılır. İkinci kayıpta buton görünür ama soluk ve basılamaz.
   - **Retry:** Game sahnesi yükleme ekranıyla baştan yüklenir.

### Yıldız hesabı

Level'ın kendi yıldız değerleri varsa (`stars.three` ve `stars.two` 0'dan büyükse) onlar kullanılır. Örneğin `three = 4` ise 3 yıldız için en az 4 hamle kalmalıdır. Yoksa varsayılan oranlar kullanılır:

| Kalan hamle (baştaki hamleye oranı) | Yıldız |
| --- | --- |
| %15 ve üstü | 3 |
| %5 ve üstü | 2 |
| Daha az | 1 |

Oyuncu +5 hamle kullandıysa level her zaman 1 yıldız alır.

### Yükleme ekranı: `LoadingScreen`

- Prefab'ı `LoadingScreen` adresiyle Addressables'ta durur. Hiçbir sahneye konmaz: `Boot` onu yükleyip kaydeder, ilk sahne geçişinde oluşturulur ve `DontDestroyOnLoad` ile sahneler arasında yaşar.
- Sıra: ekran kararan bir perde gibi gelir (fade in) → yeni sahne arka planda yüklenir (`Addressables.LoadSceneAsync`) → bar dolar → sahne açılır → perde kalkar (fade out).
- En az 0,6 sn görünür, böylece hızlı yüklemede yanıp sönmez. Yükleme sürerken tıklamalar engellenir ve ikinci bir yükleme başlatılamaz.
- Gizliyken Canvas tamamen kapalıdır, çizim maliyeti yoktur.
- Bir sahne yüklenemezse (eksik ya da bozuk paket) hata yazar ve takılı kalmak yerine kendini gizler.

### Popup'lar: `PopupManager`

- `PopupManager.Show<WinPopup>()` gibi bir çağrıyla açılır. Her popup `PopupView`'dan türer ve aynı açılış animasyonunu kullanır.
- `PopupManager.Awake()` oyun başlarken tüm popup'ları gizler. Bu yüzden popup'lar sahnede açık ya da kapalı kaydedilebilir.
- Bir popup açıkken `IsAnyOpen` true olur ve `BoardController` tahtaya dokunmayı yok sayar.

### Ayarlar, ses ve titreşim

- **Ayarlar menüsü** sadece Menu sahnesindedir. `SettingsDropdownView` dişli butonuna basılınca müzik, ses ve titreşim butonlarını aşağı kayarak açar. Dışarı tıklayınca kapanır.
- **`GameSettings`** her ayarı `PlayerPrefs`'te tutar ve değişince `Changed` event'i atar. `SettingToggleView` her butonun açık/kapalı görünümünü günceller. Butonun hangi ayar olduğunu obje adından anlar.
- **`AudioManager`** ilk çağrıldığında kendini oluşturur ve sahneler arasında yaşar. Ses listesini `Resources/AudioConfig`'ten okur.
  - Sesler `AudioManager.Play(SoundId.X)` ile çalınır. 8 kanal (AudioSource) dönüşümlü kullanılır.
  - Aynı ses çok sık çalınmaz (`MinInterval`), her seferinde perdesi hafifçe değişir (`PitchVariation`). 30 taş aynı anda patlayınca kulak tırmalayan bir gürültü yerine dolgun bir ses çıkar.
  - `UIButtonSound` bir butona eklenince tıklama sesi çalar.
- **`Haptics.Vibrate()`** titreşim ayarı açıksa telefonu titretir. Kombo birleşmesi, ışık topu ve kazanma anında kullanılır.

### Kaydedilen veriler

Hepsi `PlayerPrefs`'te tutulur, uygulama kapanıp açılınca kaybolmaz:

| Veri | Kim tutar |
| --- | --- |
| Şu anki level | `LevelProgress.CurrentIndex` |
| Level yıldızları | `LevelProgress.GetStars` / `SaveStars` |
| Booster sayıları | `BoosterInventory` |
| Müzik, ses, titreşim | `GameSettings` |

## 9. Asset'ler ve bellek: atlaslar ve Addressables

Menü sadece menünün ihtiyacı olanı yükler. Oyun görselleri bir level açılınca yüklenir, oyuncu menüye dönünce bellekten çıkar.

### Addressables grupları

| Grup | İçerik | Bundle Mode | Neden |
| --- | --- | --- | --- |
| `Scenes` | `Menu`, `Game` (adresler `Menu`, `Game`) | Pack Separately | Her sahne ve sadece onun kullandığı asset'ler ayrı bir pakete girer |
| `UI` | `LoadingScreen` prefab'ı | Pack Together | `Boot` bir kez yükler, oturum boyunca yaşar |
| `Shared` | `atlas_ui_common`, `bobo_juggle`, `menu-background` | Pack Together | Menü, yükleme ekranı ve oyun ortak kullanıyor. Bu grup olmazsa her paket kendi kopyasını alır |

Prefab, sprite ve ScriptableObject'ler gruplara tek tek eklenmez. Addressables onları kullanan sahnenin bağımlılığı olarak kendisi paketler.

### Sprite atlasları

| Atlas | Klasör | Ne zaman yüklü |
| --- | --- | --- |
| `atlas_board` | `Sprites/boards` | Level'da |
| `atlas_ui_game` | `Sprites/user_interface/game` | Level'da |
| `atlas_ui_common` | `Sprites/user_interface/common` | Her zaman (Shared grubu) |
| `bobo_sprite_atlas` | `Sprites/bobo_sprites` | Level'da (`bobo_juggle` atlasın dışında, çünkü yükleme ekranı kullanıyor) |

Atlaslar klasörün tamamını paketler. Yeni bir sprite'ı doğru klasöre koyarsan doğru atlasa kendiliğinden girer. Arkaplanlar ve logo atlasta değil: büyükler ve tek başlarına kullanılıyorlar. Particle texture'ları (`Sprites/particule`) da atlasta değil, çünkü particle materyalleri texture'ı doğrudan kullanıyor.

### Belleği düşük tutan kurallar

- **Menü oyun asset'lerine referans vermemeli.** `MenuController`, `GameAssets` değil `LevelCatalog` kullanır (sadece level verisi). `GameAssets`'e tek bir referans, menüde bütün oyun görsellerini yükler.
- **Birden fazla yerde kullanılan asset `Shared` grubuna gider.** Yoksa Addressables onu kullanan her pakete kopyalar.
- **`LevelDebugger` sadece editörde çalışır.** `OnGUI` yazısı Unity'nin bellekte kalan yazı materyalleri oluşturmasına yol açıyor ve cihaz ölçümlerini bozuyordu.
- **URP'de HDR ve MSAA kapalı.** 2D sprite oyununun bunlara ihtiyacı yok, render texture'ların boyutunu iki katına çıkarıyorlardı.

### Telefonda ölçülen değerler (Memory Profiler)

| | Menü | Level | Level'dan sonra menü |
| --- | --- | --- | --- |
| Texture'lar | 19.9 MB | 33.8 MB | 22.9 MB (fazladan 3 MB, bir kez oluşturulan URP 2D ışık tabloları) |
| Render texture'lar | 21.1 MB | 23.8 MB | 21.3 MB |

Diğer sonuçlar: Shared grubu build'den 5.7 MB'lık kopya asset'i kaldırdı. HDR ve MSAA'yı kapatmak render texture'ları 51.3 MB'tan 21.1 MB'a indirdi.

### Nasıl kontrol edilir

- **Analyze** (Window → Asset Management → Addressables → Analyze): *Check Duplicate Bundle Dependencies* kuralını çalıştır. Orada sadece TextMesh Pro'nun varsayılan fontu kalmalı.
- **Build Report** (Window → Asset Management → Addressables → Build Reports, Preferences'ta *Debug Build Layout* açık olmalı): hangi asset'in hangi pakette olduğunu gösterir.
- **Telefonda Memory Profiler**: editördeki sayılar editörün kendi belleğini de içeriyor, güvenilir değil. Telefona bağlı bir Development Build kullan.

### Editörde çalışırken

- **Play'e `Boot` sahnesinden bas.** Game ya da Menu'den başlatırsan hata alırsın, çünkü yükleme ekranını Boot kaydediyor.
- Günlük çalışmada **Play Mode Script → Use Asset Database** (en hızlısı). Gerçek paketleri denemek için önce Build → New Build → Default Build Script, sonra **Use Existing Build**.
- Player build'i Addressables içeriğini de derler (*Build Addressables on Player Build* açık).

## 10. Testler

Projede `Assets/Tests/EditMode` altında 9 dosyada 51 test var. Core'u ve Level'ı test ederler, sahne açmazlar, saniyeler içinde biterler.

**Çalıştırmak için:** Window → General → Test Runner → EditMode sekmesi → Run All.

| Dosya | Test | Neyi kontrol eder |
| --- | --- | --- |
| `CoreCommandTests` | 11 | Swap, eşleşme bulma (düz, L, T, 5'li), merdiven şeklinin eşleşmemesi, komşu hasarının bir kez verilmesi |
| `SpecialActivationTest` | 10 | Roket, TNT, kombolar, zincirleme patlama, tap ile patlatma, aynı tohumla aynı pervane hedefi |
| `BoosterTest` | 6 | Çekiç, satır, sütun; kafese vurma, topu yok etmeme, delikte reddedilme |
| `TimingAndFixesTest` | 5 | Patlama zamanlarının mesafeyle artması, zincir gecikmesi, eşleşmeyen swap'ın boş dönmesi |
| `FloorPieceTest` | 4 | Çimenin üstündeki eşleşmeyle kırılması, yanındaki eşleşmeyle kırılmaması |
| `ToyPieceTest` | 4 | Topun dipte toplanması, roketten etkilenmemesi, kafes içindeyken toplanmaması, altı delikse çıkışın yukarı kayması |
| `BoardColorsTest` | 2 | Yeni taşların sadece tahtanın renklerinden gelmesi |
| `LevelBoardBuilderTest` | 7 | Harflerin doğru taşlara dönüşmesi, sadece level renklerinin kullanılması, kilit ve kafesin taşları kilitlemesi, zeminler, hedefler; validator'ın kutu üstündeki kilidi ve fazla hedefi reddetmesi, geçerli level'ı kabul etmesi |
| `LevelJsonTest` | 2 | JSON'a yazılıp geri okunan level'ın aynı kalması; boş metnin level döndürmemesi |

### Bir test nasıl okunur

Her test üç adımlıdır: **kur, yap, kontrol et.**

1. **Kur:** küçük bir tahta harflerle çizilir. Örneğin `"RRB"` bir sırada kırmızı, kırmızı, mavi demektir. Top `O`, delik `H` ile gösterilir.
2. **Yap:** bir komut verilir: `TurnResolver.Resolve(board, new SwapCommand(...))`.
3. **Kontrol et:** dönen adım listesine ve tahtaya bakılır: "top toplandı mı?", "hedef sayısı 1 arttı mı?".

İsim de ne test edildiğini söyler: `Toy_HitByRocket_IsNotDestroyed` = "top rokete vurulunca yok olmaz".

## 11. Tarifler: yeni bir şey nasıl eklenir

Mimarinin asıl testi yeni içerik eklemenin kolaylığıdır. Aşağıdaki tariflerin hiçbirinde `TurnResolver`'ı değiştirmen gerekmez.

### Yeni bir level

1. `Assets/Data/LevelAssets` klasöründe sağ tık → Create → Match3 → **Level Asset**. Sıralama doğru olsun diye numaralı adlandır (`Level-21`).
2. Inspector'da `Data` alanını doldur: boyut, hamle, renkler, `cells` satırları (gerekirse `overlays` ve `floors`) ve hedefler. Her satır tam `width` harf uzunluğunda olmalı.
3. `LevelCatalog` asset'ine sağ tık → **Refresh From Data/LevelAssets**. Yeni level listeye eklenir.
4. Editörde `LevelDebugger` ile doğrudan o level'a atlayıp dene (`[` ve `]` tuşları).

Yapım aşamasındaki level editörü penceresi 1. ve 2. adımların yerini alacak: bir ızgara üzerine çizeceksin, kaydetmeden önce level'ı `LevelValidator` ile kontrol edecek.

### Yeni bir engel

1. **Core:** `Core/Pieces` altına yeni bir sınıf yaz. Doğru katmandan türet: hücrede duruyorsa `Piece`, üste kapanıyorsa `OverlayPiece`, altta duruyorsa `FloorPiece`. Bayraklarını, canını ve (hedefse) `GoalKind`'ını ver.
2. `PieceVisualType`'a yeni bir değer ekle. Hedef olacaksa `GoalType`'a da ekle.
3. **Test:** davranışını kontrol eden bir test yaz (örneğin "roket hasar veriyor mu").
4. **Yerleştirme:** `Level/Elements` altına `ILevelElement`'i uygulayan bir sınıf yaz. Boş bir harf ve katmanını seç, `CanPlace` kurallarını yaz, hedefse `AutoGoal`'u ayarla, `Place`'te taşı oluştur. `LevelElements`'e onun için bir satır, `LevelBoardBuilderTest`'e bir test ekle.
5. **Görüntü:** sprite'ları için bir `VisualConfig` ekle ya da mevcut olanı genişlet. Hedefse `GoalIconConfig`'e ikonunu ekle. Görsel oluşturma kodu gerekmez: `SpawnBoardViews` tahtadaki her taşın görselini kendisi oluşturur.

Bayrakları doğru seçersen eşleşme, hasar, düşme ve karıştırma kuralları yeni engele kendiliğinden uygulanır.

### Yeni bir özel taş

1. `PieceKind`'a yeni bir değer ekle.
2. Hangi şeklin onu ürettiğini `ShapeDefinitions.RegisterAll()`'a bir matris ve öncelikle ekle.
3. `ISpecialActivation`'ı uygulayan bir sınıf yaz: hangi hücreleri, hangi milisaniyede vurduğunu döner. `SpecialActivationRegistry`'e kaydet. Alan boyutunu `ActivationArea`'ya, zamanlamayı `ActivationTiming`'e koy.
4. Diğer özel taşlarla komboları için `IComboActivation` sınıfları yaz ve `ComboActivationRegistry`'e kaydet.
5. **Görüntü:** `IActivationPresenter`'ı uygulayan bir presenter yaz, `ActivationPresenterRegistry`'e kaydet. Gerekirse yeni `IViewCommand`'lar ekle. Sprite'ı `SpecialVisualConfig`'e ekle.
6. Testlerini yaz.

### Yeni bir booster

1. `BoosterType`'a yeni bir değer ekle.
2. Hücre seçen bir booster'sa `IBoosterEffect`'i uygulayan bir sınıf yaz ve `BoosterRegistry`'e kaydet. Hasar ve zincirleme patlama kendiliğinden gelir.
3. Booster bar'a butonunu ve sprite'ını ekle.
4. Testini yaz.

### Yeni bir sprite

1. Kullanıldığı yere göre doğru klasöre koy: `Sprites/boards` (tahtada), `Sprites/user_interface/game` (oyun arayüzü), `Sprites/user_interface/common` (menü ve oyun), `Sprites/backgrounds` (büyük tek görseller).
2. O klasörün atlası onu kendiliğinden alır, elle eklemen gerekmez.
3. Menü ve oyun ikisi de kullanıyorsa `common` klasöründe olmalı. Sonra Analyze ile kopya kontrolü yap.

### Yeni bir sahne

1. Sahneyi `Scenes` Addressables grubuna ekle ve kısa bir adres ver (örneğin `Shop`).
2. Yeni bir `SceneLoader` metodu üzerinden `LoadingScreen.LoadScene("Shop")` ile yükle. Build listesine ekleme.
3. Addressables içeriğini yeniden derle ve Analyze çalıştır.

### Yeni bir ses

1. `SoundId`'ye yeni bir değer ekle (değerleri açıkça yaz, mevcutların sayısını değiştirme).
2. `Resources/AudioConfig`'e o id için bir satır ekle ve ses dosyasını sürükle.
3. Kodda `AudioManager.Play(SoundId.YeniSes)` çağır.

### Oyun hissini ayarlamak

- **Hızlar ve süreler** (düşme, patlama, uçuş): `AnimationSettings` asset'i. Kod yazmadan, oyun açıkken bile değiştirebilirsin.
- **Patlama zamanlamaları** (roketin hücre başına gecikmesi, zincir gecikmesi): `ActivationTiming.cs`. Bunlar Core'da olduğu için testlerden geçmesi gerekir.
- **Maskot tepkileri:** `MascotConfig`.

## 12. Sözlük

| Terim | Anlamı |
| --- | --- |
| Core | Oyun kurallarını hesaplayan, Unity'ye hiç bağlı olmayan C# katmanı |
| Presentation | Core'un sonucunu ekranda gösteren Unity katmanı |
| Komut (`ICommand`) | Oyuncunun bir eylemi: swap, tap ya da booster |
| Tur (turn) | Bir komutla başlayıp tahta durulana kadar olan her şey |
| Adım (`TurnStepData`) | Turda olan tek bir olayın verisi: silinme, düşme, patlama... |
| Kaskad | Taşlar düştükten sonra kendiliğinden oluşan yeni eşleşmeler |
| Dalga (wave) | Animasyonda bir patlama + düşme döngüsü; her kaskad yeni bir dalgadır |
| Bayrak (`PieceFlags`) | Taşın ne yapabildiğini söyleyen özellik: hareket eder mi, eşleşir mi... |
| Overlay | Taşın üstüne kapanan katman: kilit, kafes |
| Floor | Taşın altındaki katman: çimen |
| Registry | "Şu tür için şu sınıfı kullan" kayıt defteri; `switch` yerine kullanılır |
| Presenter | Bir özel taşın ya da kombonun animasyon tarifi |
| View command (`IViewCommand`) | Belli bir zamanda başlayan tek bir animasyon |
| Pool (havuz) | Silinen görselleri yok etmek yerine saklayıp tekrar kullanmak |
| Deterministik | Aynı girdiyle her seferinde aynı sonucu üretmek |
| `IRandom` / seed | Rastgelelik arayüzü; aynı seed aynı rastgele sayıları verir |
| Hedef önceliği (`ITargetPriority`) | Pervane gibi hedef seçen taşların hangi hücreyi seçeceği |
| Çıkış (exit) | Bir sütunun en alttaki deliksiz hücresi; top buraya gelince toplanır |
| ScriptableObject | Unity'de kodu değiştirmeden ayar tutan asset dosyası (level'lar, config'ler) |
| Level verisi (`LevelData`) | Sade veri olarak bir level: boyut, hamle, renkler, hedefler ve her katman için harf satırları |
| Element (`ILevelElement`) | Level'a konabilen tek bir şey (kutu, kilit, çimen...); harfi, kuralları ve yerleştirme kodu |
| Validator | Bir level'ı kontrol edip neyin yanlış olduğunu listeleyen sınıf |
| `ILevelSource` | Level'ların nereden geldiği: bugün build'deki level'lar, ileride bir sunucu |
| Assembly (`.asmdef`) | Birlikte derlenen kod grubu. Hangi kodun hangi kodu görebileceğine karar verir |
| `AutoRef` | Referansları obje ismiyle otomatik bulan yardımcı |
| Event | Bir sınıfın "şu oldu" diye duyurması; dinleyenler kendi tepkisini verir |
| Addressables | Asset'leri adresle, paketlerden ve sadece gerektiğinde yükleyen Unity sistemi |
| Bundle (paket) | Birlikte yüklenen ve birlikte boşaltılan asset grubu |
| Sprite atlas | Birçok küçük sprite'ın tek bir texture'a paketlenmiş hali |
| Boot sahnesi | Addressables'ı başlatıp menüyü açan, neredeyse boş ilk sahne |
