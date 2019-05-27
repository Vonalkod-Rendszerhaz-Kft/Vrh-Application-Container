# Vrh.iSchedulerReport - Jelentéscsomagok kezelése és elküldése
> Fejlesztve és tesztelve **4.5** .NET framework alatt. Más framework támogatása további tesztelés függvénye, 
> de nem kizárt alacsonyabb verziókban való működőképessége.

A modul az iScheduler alkalmazásból használható, riportok háttérben való futtatását, file-okba való eltárolását és emailben való terítését biztosítja.

A riportcsomag egy névvel és leírással ellátott objektum, riportok összessége.
A riportcsomaghoz hozzárendelésre kerül egy szerep azzal a céllal,
hogy az elkészített riportcsomaghoz (a legenerált riportokhoz) való
felhasználói hozzáférést szabályozni lehessen: csak azok a felhasználók 
férjenek hozzá, akik a megadott szereppel rendelkeznek. A riport csomag 
végrehajtásakor legenerálásra és elmentésre kerülnek a riportok a file
rendszerben olyan alkönyvtár és file-nevekkel, hogy a Vrh.Web.FileManager 
funkcionalitását kihasználva az egyes riportokat csak azok a felhasználók
láthassák, akik a riportcsomaghoz hozzárendelt szereppel rendelkeznek.

A komponens akcióit az **iSchedulerReport area** és **iSchedulerReport controller** 
elemeken keresztűl lehet elérni. Példa egy URL-re: *[application]/iSchedulerReport/iSchedulerReport/Manager*

## Akciók
* Riport csomag kezelő (**Manager** akció)
* Riport csomag szerkesztő (**Editor** akció)
* Riport csomagok listája (**List** akció)
* Riport csomag műveletek listája (**ListOperations** akció) 
* Riport objektumok ellenőrzése (**Check** akció)
* Riport csomagon művelet végrehajtása (**Execute** akció)
  
****

## Version History:

#### 1.5.2 (2019.05.27) Patches - debug:
- Frissítés a Vrh.OneReport 1.4.2 változatára.

#### 1.5.1 (2019.05.22) Patches - debug:
- Frissítés a Newtonsoft.Json 12.0.1 változatára.
- Frissítés a Vrh.OneMessage 1.1.4 változatára.
- Frissítés a Vrh.OneReport 1.4.1 változatára.
- Frissítés a Vrh.Web.Membership 2.5.1 változatára.
- Frissítés a Vrh.Web.Common.Lib 1.11.1 változatára.

#### 1.5.0 (2019.04.24) Compatible API changes (debug):
- SchedulerPlugin osztály létreozása az ISchedulerPlugin intergész megvalósításával és annak használata Controller-ben is.
- Frissítés a Microsoft.AspNet.Mvc 5.2.7 változatára.
- Frissítés a Vrh.Web.Common.Lib 1.11.0 változatára.

#### 1.4.5 (2018.05.05) Patches (debug):
- Javítás a teszt futtatás körül.

#### 1.4.4 (2018.04.25) Patches:
- MultiLanguageManager NuGet csomag bevezetése.
- A Vrh.Common.Serialization.Structures lecserélése a Vrh.Web.Common.Lib Nuget csomagra.

#### 1.4.3 (2017.11.01) Compatible API changes:
- Naplózási adatokban szerepel az összes URL.

#### 1.3.1 (2017.10.01) Patches:
- ViewUsersOfRole function-ben a getuserlist ajax hívás POST-ról GET-re változtatása
- #9103

#### 1.0.1 (2017.05.22) Patches:
- Ha egy riport csomag sorában a ceruzára kattintok, akkor a korábban a csomagba bevitt riportok nem jelennek meg a riport csomag editorban.
- ObjectEditorban az Új sor nyomógombhoz nincs szókód

#### 1.0.0 (2017.05.16) Initial version

