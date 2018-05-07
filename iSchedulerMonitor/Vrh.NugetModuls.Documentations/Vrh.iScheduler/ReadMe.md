# Vrh.iScheduler - az ütemező komponens
A komponens **v1.3.10** kiadásakor kezdődött e leírás elkészítése, ám jelenleg nem naprakész.
Az akciók listája teljes, de használatuk részletes bemutatása még hiányzik. Az adatok része csak minta állapotú.
> Fejlesztve és tesztelve **4.5** .NET framework alatt. Más framework támogatása további tesztelés függvénye, 
> de nem kizárt alacsonyabb verziókban való működőképessége.

Az iScheduler feladata, hogy objektumokon (azok belső tulajdonságai ismerete nélkül)
végrehajtható műveletek ütemezését lehessen beállítani. Az objektumoknak meghatározott 
interfésszel kell rendelkezniük az ütemezővel való együttműködés érdekében.

A komponens akcióit az **iScheduler area** és **iScheduler controller** 
elemeken keresztűl lehet elérni. Példa egy URL-re: *[application]/iScheduler/iScheduler/Manager*

## Akciók
* Ütemezés kezelő (**Manager** akció)
* Ütemezés szerkesztő (**Editor** akció)
* Ütemezés törlése (**Delete** akció)
* Ütemezés létezésének ellenőrzése (**CheckSchedule** akció) 
* Ütemezés (teszt célú) végrehajtása (**ScheduleExecute** akció)
  
## Adatok
A komponens az adatbázisban az iScheduler sémán lévő táblák létrehozásáért és migrációjáért is felelős.
Az *iSchedulerAreaRegistration.cs* fájlban található a hasonló nevű osztály, melyben elvégezhető a 
migráció az alábbi módon:

```javascript
/// <summary>
/// Az area regisztrációjának belépési pontja.
/// </summary>
/// <remarks>
/// Ide kell tenni minden olyan dolgot, 
/// melyet az area felélesztésekor el kell végezni.
/// Például:
/// - a szókódok inicializálása
/// - vagy a komponens működéséhez szükséges adatbázis migrációjának elvégzése
/// </remarks>
/// <param name="context"></param>
public override void RegisterArea(AreaRegistrationContext context)
{
    RegisterRoutes(context);
    RegisterBundles(BundleTable.Bundles);

    //nyelvi fordítások inicializálása
    VRH.Log4Pro.MultiLanguageManager.MultiLanguageManager.InitializeWordCodes(typeof(WordCodes));

    //itt érdemesebb elvégeztetni a migráció ellenőrzést, mint minden context példányosításakor
    System.Data.Entity.Database.SetInitializer(new System.Data.Entity.MigrateDatabaseToLatestVersion<iSchedulerDB, Vrh.iScheduler.Migrations.Configuration>(true));
}
```
A komponens által kezelt táblák:
### iScheduler.Schedules tábla
Mező|Leírás
:----|:------
Id|Egy ütemezés belső egyedi azonosítója.
ScheduleObjectId|Objektum azonosítója, amelyre az ütemezés vonatkozik. Az *iScheduler.ScheduleObjects* tábla Id mezőjére hivatkozik.
ScheduleOperationId|Objektummal elvégzendő művelet azonosítója. Az *iScheduler.ScheduleOperations* tábla Id mezőjére hivatkozik.
OperationTime|Az ütemezés végrehajtásának időpontja.
State|Az ütemezés állapota. 0=aktív, 1=sikeresen végrehajtott, 2=sikertelenül végrehajtott
ScheduleSeriesId|Ha az ütemezés egy ütemezés sorozat része, akkor annak azonosítója. Az *iScheduler.ScheduleSeries* tábla Id mezőjére hivatkozik.
****

# Version History:
## V1.4.0 (2018.05.05)
### Compatible changes:
- ScheduleExecute osztály beépítése. A "Run" metódus futtat egy adott időzített feladatot.
- A ScheduleExecute.Run metódus az időzítésben megadott művelet futási idejét méri és eltárolja. 
- Az eredményablak az eredményhez igazodik.

## V1.3.12 (2018.04.25)
### Patches:
- MultiLanguageManager NuGet csomag bevezetése.
- A Vrh.Common.Serialization.Structures lecserélése a Vrh.Web.Common.Lib Nuget csomagra.

## V1.3.11 (2017.10.21)
### Patches:
1. NuGet csomaggá alakítás, dokumentáció kezdeti állapotának (ReadMe.md) létrehozása.

## V1.3.10 (2017.09.26)
### Patches:
1. Naplózásba minden létrejött URL belekerül a ScheduleExecute esetén.
2. ReturnValue és ReturnMessage nevű mezők létrehozása, hogy majd a Monitor ide tudja betenni ezeket az értékeket.
3. Nyomógomboknál beállítható, hogy az elindított akció válasza egy ablakban jelenjen meg.
4. Az aktuális nézetben a nézetet előhívó gombot mostantól elrejtjük.

## V1.3.9 (2017.09.22)
### Patches:
1. Az iScheduler controller mostantól a Vrh.Common.Serialization.Structures BaseController-ből származik.

## V1.3.8 (2017.08.31)
### Patches:
1. A smalot-datetimepicker javított változata (mert az eredeti hibádzik, amikor a pozíciót ki kell számolni).
2. EditorConstans nevű prototype és a globális névtér beli változó átnevezése, mert ütközött az iSchedulerReport változójával.

## V1.3.7 (2017.08.18)
### Patches:
1. Az Editor is feltölti magának a datetimepicker plugint.

## V1.3.6 (2017.08.15)
### Patches:
1. Az OperationTime bottstrap-es datepicker lett.
2. Mostantól az ütemezéseknek 3 állapota lehet: Active, Success, Failed.

## V1.3.5 (2017.08.11)
### Patches:
1. Editor teszt futtatás válaszképernyő formája.

## V1.3.4 (2017.08.10)
### Patches:
1. ResponseTimeout XML elem bevezetése, és az időzítés beállítása.

## V1.3.3 (2017.08.09)
### Patches:
1. Némely esetben az Editor nem záródott be, ha teszt futtatás be volt jelölve.

## V1.3.2 (2017.08.08)
### Patches:
1. A ScheduleExecute-ba bekerült egy részletes naplózás, ami a ReturnMessage-ben 
jelenik meg, ha nem "saját" hibát dob.

## V1.3.1 (2017.08.08)
### Patches:
1. Apró javítás a manager.cshtml-ben.

## V1.3.0 (2017.08.04)
### Compatible API changes:
1. A Manager mostantól tudja a naptár nézetet.

## V1.2.1 (2017.07.31)
### Compatible API changes:
1. Az Editor mostantól el tudja indítani az ütemezett objektum Editor-át.

## V1.2.0 (2017.07.28)
### Compatible API changes:
1. A heti ütemezés megvalósítása.
### Patches:
1. Az XML-re tesz egy filewatchert, és csak akkor tölti be újra, ha megváltozott a file. Így a GetRootElement hívás kb 3000-szer gyorsabb lett, ha nincs file változás.
2. Unit test projekt hozzáadása a projekthez 

## V1.1.2 (2017.07.24)
### Patches:
1. A CookieWebClient psztály LastException tulajdonság bevezetése miatti változások átvezetése.

## V1.1.1 (2017.07.05)
### Patches:
1. A vrh.bootboxAction.js módosulása miatt kellett egy aprót módosítani az Editor.cshtml-ben.

## V1.1.0 (2017.06.29)
### Compatible API changes:
1. ScheduleExecute akció lérehozása. 
2. Editor-ban lett egy checkbox. "Pipa" esetén az Editor saját maga is meghívja ezt az akciót tesztelés céljából.

## V1.0.3 (2017.06.28)
### Compatible API changes:
1. Új "sys" típusú nyomogom létrehozása "ObjectManager" névvel, mely az ütemezett objektum kezelőjét hívja meg.
### Patches:
1. Egyes változók nem helyettesítődtek be az URL-ekben.
1. Nyomógomb (Button) feldolgozási hiba javítása.

## V1.0.2 (2017.06.18)
### Patches:
1. A Vrh.Common.Serialization.Structures létrehozásával és beépítésével kapcsolatos változások átvezetése.

## V1.0.1 (2017.05.31)
### Patches:
1. A konzisztencia ellenőrzésen kellett javítani.

## V1.0.0 (2017.05.26)
### Initial version

