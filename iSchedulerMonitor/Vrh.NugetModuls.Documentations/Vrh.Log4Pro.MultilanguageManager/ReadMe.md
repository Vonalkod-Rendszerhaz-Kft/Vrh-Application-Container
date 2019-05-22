# Vrh.Log4Pro.MultilanguageManager
Ez a leírás a komponens **v0.0.0** kiadásáig bezáróan naprakész.
Igényelt minimális framework verzió: **4.5**
Teljes funkcionalitás és hatékonyság kihasználásához szükséges legalacsonyabb framework verzió: **4.5**
### A komponens a többnyelvű alkalamazások fejlesztésének elősegítésére szolgál


# TODO: Megirandó a komponens dokumentációja!!!

# Version History:

## v2.1.5 (2018.04.25)
### Patches
1. Update Newtonsoft.Json to Newtonsoft.Json 11.0.2
2. Translation cache nullvédelme inicializálatlan DB esetén
3. Framework references betéve a nuspec-be
4. Icon URL a nuspechez adva

## v2.1.4 (2017.11.07)
### Patches
1. Update EF to: 6.2.0
2. Update EntityFramework.Extended to 6.1.0.168
3. Update Newtonsoft.Json to Newtonsoft.Json 10.0.3
4. Nuget csomaggá alalkítás
## V2.1.3 (2016.10.27)
### Patches
1. Connection StringStore componens beépítése: Saját settings kulcs és CS name: MultilanguageManager, default settibngs kulcs:  connectionString, default CS name: DbConnection
## V2.1.2 (2016.09.06)
### Patches
1. AddOrModifyTranslation, ha a canOverwriteTranslation paramétere false, akkor mielőtt az adatbázishoz nyúlna, előbb leellenőrzi a cache-ben, hogy létezik-e az adott fordítás. Ha ott megvan akkor kilép. Így a kezdeti Wordcode initeken sokkal gyorsabban jut túl, a már inicializált fordítások átugrásával, mintha mindig a db ellenőrzésénél ugrana ki az exception ágon...
## V2.1.1 (2016.05.20)
### Patches
1. Lockolások finomítása (Multithreading környezetben való konzisztensebb működésért)
2. Fel nem oldható szókódra  a szókódot adja a GetTranslation vissza 
## V2.1.0 (2016.03.23)
### Copatible API Changes:
#### 1. **System.ComponentModel.DataAnnotations** Override-ok betétele a komponensbe #5965
> Így nem kell a használt projektben újraírni, vagy odamásolni őket forrás szinten, használhatóak innen.
Ez a 4 darab van:
>* **DisplayNameWithTrueWordCodesAttribute**
* **RequiredWithTrueWordCodesAttribute**
* **StringLengthWithTrueWordCodesAttribute**
* **RangeWithTrueWordCodesAttribute**
> **Használati minta:**
```java
[DisplayNameWithTrueWordCodes(typeof(TrueWordCodes.MasterData.AndonRequestCode.Columns.Code))]
[RequiredWithTrueWordCodes(typeof(TrueWordCodes.MasterData.DataAnnotations.RequiredWithName))]
[StringLengthWithTrueWordCodes(2, typeof(TrueWordCodes.MasterData.DataAnnotations.StringLengthWithNameAndBetween), MinimumLength = 2)]
[RangeWithTrueWordCodes(1, 5, typeof(TrueWordCodes.MasterData.DataAnnotations.Range))]
```
#### 2. **GetLanguagesInSelectItemList** bővítés #5964
> Kapott egy nem köztelező paramétert (default=true) (így nem okoz kompatibilitási problémát, eddig mindig csak az aktívakat adta). Amely jelzi, hogy csak az aktív nyelvek, vagy mindegyik szerepeljen-e a visszaadott SelectList-ben.
**Használati minta:**
>> Minden nyelvet visszaad:
```java
GetLanguagesInSelectItemList(false)
```
>> Csak az aktívakat:
```java
GetLanguagesInSelectItemList()
GetLanguagesInSelectItemList(true)
```
#### 3. Elírások javítása #5961
> Sajnos volt pár elírás ami átcsúszott  a kettes  verzióba is. Ezek ebben a verzióban Obsolete-nek lettek jelölve és létre van hozva a helyes írású verziójuk:
>* ~~**GetCashe**~~ helyesen **GetCache**, ezt kell használni helyette
* ~~**GetWordCodesAtGoup**~~  helyesen **GetWordCodesAtGroup**, ezt kell használni helyette
### Patches
1. CacheToList függvény NullReferencExceptiont dobott, ha nem volt még létrehozva Cache #5881
2. Ha a default nyelv üres adatbázisnál jött létre a default nyelv property beállításával, akkor inaktív volt a nyelvkód, "kézzel" kellett korrigálni. Most aktívnak jön létre ekkor is. #5882
3. WordCode és Translation létrehozás hibák bizonyos esetekben ha egy WordCode GetTranslation híváson át jött létre. #5962
4. Ha a lekért nyelven nincs fordítás akkor a Defult nyelv fordítása helyett inkább a WordCode sttringet adja vissza. Így a fordítási hiányosságok könnyebben észrevehetőek. #5963
## V2.0.0 (2016.02.29)
### Incompatibility API changes:
1. 2-es verzió! Sok minden újragondolva átalakítva az eddigi használati tapasztalatok alapján. Az V1.X.X verziók felé inkompatibilis! (ahogy a verziózásból is látszik.)
## V1.0.4 (2015.01.08)
### Patches:
1. Add AddNewFirstTagToWordCode public function. Add EntityFramework.Extensions nuget package
## V1.0.3 (2015.01.08)
### Patches:
1. Ne ellenőrizze minden alaklommal az init a DB oldalon, hogy megvan-e a az adott nyelvet definiáló rekord.
## V1.0.2 (2014.11.18)
### Patches:
1. Update EF 6.1.1 (System.Data --> System.Data.Entity.Core Namespace változás miati módosításaok vannak csak benne, funkcionálisan nem változott semmi az 1.0.1-hez képest)
## V1.0.1 (2014.11.10)
### Patches:
1. Meglévő fordítást nem kötelezően ír felül (EF V5)
## V1.0.0 (2014.09.03)
Initial version
