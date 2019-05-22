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
#### 1.5.1 (2019.05.22) Patches:
- VRH.ConnectionStringStore 1.3.0 változat visszaállítása, hogy szinkronban legyen az Application.Container-rel.
- Frissítés a Newtonsoft.Json 12.0.1 változatára.
- Frissítés a Vrh.iScheduler.Report 1.5.1 változatára.
- Frissítés a Vrh.Web.Common.Lib 1.11.1 változatra.
- Teszt környezet pontosítása.

#### 1.5.0 (2019.04.24) Compatible changes:
- SchedulerPlugin szolgáltatás beépítése.
- Frissítés a Microsoft.AspNet.Mvc 5.2.7 változatra.

#### 1.4.0 (2018.05.05) Compatible changes:
- ScheduleExecute osztály beépítése. A "Run" metódus futtat egy adott időzített feladatot.
- A ScheduleExecute.Run metódus az időzítésben megadott művelet futási idejét méri és eltárolja. 
- Az eredményablak az eredményhez igazodik.

#### 1.3.12 (2018.04.25) Patches:
- MultiLanguageManager NuGet csomag bevezetése.
- A Vrh.Common.Serialization.Structures lecserélése a Vrh.Web.Common.Lib Nuget csomagra.

#### 1.3.11 (2017.10.21) Patches:
- NuGet csomaggá alakítás, dokumentáció kezdeti állapotának (ReadMe.md) létrehozása.

#### 1.3.10 (2017.09.26) Patches:
- Naplózásba minden létrejött URL belekerül a ScheduleExecute esetén.
- ReturnValue és ReturnMessage nevű mezők létrehozása, hogy majd a Monitor ide tudja betenni ezeket az értékeket.
- Nyomógomboknál beállítható, hogy az elindított akció válasza egy ablakban jelenjen meg.
- Az aktuális nézetben a nézetet előhívó gombot mostantól elrejtjük.

#### 1.3.9 (2017.09.22) Patches:
- Az iScheduler controller mostantól a Vrh.Common.Serialization.Structures BaseController-ből származik.

#### 1.3.8 (2017.08.31) Patches:
- A smalot-datetimepicker javított változata (mert az eredeti hibádzik, amikor a pozíciót ki kell számolni).
- EditorConstans nevű prototype és a globális névtér beli változó átnevezése, mert ütközött az iSchedulerReport változójával.

#### 1.3.7 (2017.08.18) Patches:
- Az Editor is feltölti magának a datetimepicker plugint.

#### 1.3.6 (2017.08.15) Patches:
- Az OperationTime bottstrap-es datepicker lett.
- Mostantól az ütemezéseknek 3 állapota lehet: Active, Success, Failed.

#### 1.3.5 (2017.08.11) Patches:
- Editor teszt futtatás válaszképernyő formája.

#### 1.3.4 (2017.08.10) Patches:
- ResponseTimeout XML elem bevezetése, és az időzítés beállítása.

#### 1.3.3 (2017.08.09) Patches:
- Némely esetben az Editor nem záródott be, ha teszt futtatás be volt jelölve.

#### 1.3.2 (2017.08.08) Patches:
- A ScheduleExecute-ba bekerült egy részletes naplózás, ami a ReturnMessage-ben 
jelenik meg, ha nem "saját" hibát dob.

#### 1.3.1 (2017.08.08) Patches:
- Apró javítás a manager.cshtml-ben.

#### 1.3.0 (2017.08.04) Compatible API changes:
- A Manager mostantól tudja a naptár nézetet.

#### 1.2.1 (2017.07.31) Compatible API changes:
- Az Editor mostantól el tudja indítani az ütemezett objektum Editor-át.

#### 1.2.0 (2017.07.28) Compatible API changes:
- A heti ütemezés megvalósítása.
##### Patches:
- Az XML-re tesz egy filewatchert, és csak akkor tölti be újra, ha megváltozott a file. Így a GetRootElement hívás kb 3000-szer gyorsabb lett, ha nincs file változás.
- Unit test projekt hozzáadása a projekthez 

#### 1.1.2 (2017.07.24) Patches:
- A CookieWebClient psztály LastException tulajdonság bevezetése miatti változások átvezetése.

#### 1.1.1 (2017.07.05) Patches:
- A vrh.bootboxAction.js módosulása miatt kellett egy aprót módosítani az Editor.cshtml-ben.

#### 1.1.0 (2017.06.29) Compatible API changes:
- ScheduleExecute akció lérehozása. 
- Editor-ban lett egy checkbox. "Pipa" esetén az Editor saját maga is meghívja ezt az akciót tesztelés céljából.

#### 1.0.3 (2017.06.28) Compatible API changes:
- Új "sys" típusú nyomogom létrehozása "ObjectManager" névvel, mely az ütemezett objektum kezelőjét hívja meg.
##### Patches:
- Egyes változók nem helyettesítődtek be az URL-ekben.
- Nyomógomb (Button) feldolgozási hiba javítása.

#### 1.0.2 (2017.06.18) Patches:
- A Vrh.Common.Serialization.Structures létrehozásával és beépítésével kapcsolatos változások átvezetése.

#### 1.0.1 (2017.05.31) Patches:
- A konzisztencia ellenőrzésen kellett javítani.

#### 1.0.0 (2017.05.26) Initial version

