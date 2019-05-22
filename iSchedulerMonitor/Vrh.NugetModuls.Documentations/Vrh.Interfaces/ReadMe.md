# Vrh.Interfaces - Alapértelmezett és hasznos interfészek
> Fejlesztve és tesztelve **4.5** .NET framework alatt. Más framework támogatása további tesztelés függvénye, 
> de nem kizárt alacsonyabb verziókban való működőképessége.

A modul a Vonalkód Rendszerház fejlesztési környezetében szabványosított és
hasznosan alkalmazható interfészeinek, struktúráinak gyűjtőhelye.

## Interfészek
* **[IManage](##IManage)**
* **[ISchedulerPlugin](##ISchedulerPlugin)**
* **[ITranslation](##ITranslation)**

## Osztályok
* **[CheckListJSON](##CheckListJSON)**
* **[ReturnInfoJSON](##ReturnInfoJSON)**
* **[SelectListJSON](##SelectListJSON)**

## IManage
Generikus interfész, melyben azt a típust kell megadni, amely menedzselését végzi.
```csharp
/// <summary>
/// Meghatározza és előírja egy karbantartást és hozzáférést 
/// biztosító osztály elvárt tulajdonságait és módszereit.
/// </summary>
public interface IManage<T>
{
    /// <summary>
    /// A kezelt típust össze egyedét szolgáltató tulajdonság.
    /// </summary>
    List<T> All { get; }

    /// <summary>
    /// A kezelt típus egy elemét adja vissza az egyedi azonosító segítségével.
    /// </summary>
    /// <param name="id">Az elem egyedi azonosítója.</param>
    /// <returns></returns>
    T Get(int id);
    /// <summary>
    /// A kezelt típus egy elemét adja vissza a megadott név segítségével.
    /// </summary>
    /// <param name="name">Az elem egyedi neve.</param>
    /// <returns></returns>
    T Get(string name);

    /// <summary>
    /// Létrehozza a kezelt típus egy elemét.
    /// </summary>
    /// <param name="item">A kezelt típus egy eleme, amit hozzá kell adni.</param>
    void Create(T item);

    /// <summary>
    /// A kezelt típus egy elemét törli az egyedi azonosító alapján.
    /// </summary>
    /// <param name="id">A törlendő elem egyedi azonosítója.</param>
	void Delete(int id);
    /// <summary>
    /// A kezelt típus egy elemét törli az egyedi neve alapján.
    /// </summary>
    /// <param name="name">A törlendő elem egyedi neve.</param>
	void Delete(string name);

    /// <summary>
    /// A kezelt típus egy elemének módosítása.
    /// Ha nem létezik az hiba.
    /// </summary>
    /// <param name="item">A kezelt típus egy eleme.</param>
    void Update(T item);
}
```


## ISchedulerPlugin
Az ütemező komponens objektumai számára tartalmazza azokat az kapcsolati pontokat,
melyek megvalósításával részt vehetnek az ütemezett feladatok végrehajtásában.

```csharp
/// <summary>
/// iScheduler beépülők számára készült interface.
/// </summary>
public interface ISchedulerPlugin
{
    /// <summary>
    /// A beépülő neve. Kötelező.
    /// Ha üres, akkor a beépülő betöltésekor kivétel keletkezik.
    /// </summary>
    /// <remarks>
    /// Jelenleg egy ilyen van az 'iSchedulerReport'. De a lényeg, hogy az ütemező
    /// ez alapján tudja majd megtalálni több beépülő esetén, melyiket is kellene 
    /// használnia.
    /// </remarks>
    string Type { get; }

    /// <summary>
    /// A végrehajtás eredményének értéke.
    /// A 0 jelzi, hogy nem volt hiba, egyéb érték hibát jelöl.
    /// </summary>
    int ResultValue { get; }

    /// <summary>
    /// A végrehajtás (Execute) során keletkezett üzenetek listája.
    /// </summary>
    List<string> ResultMessages { get; }

    /// <summary>
    /// Ütemezett feladat végrehajtása.
    /// </summary>
    /// <param name="groupId">Az ütemezett objektum csoport azonosítója.</param>
    /// <param name="id">Az ütemezett objektum azonosítója.</param>
    void Execute(string groupId, string id);
}
```

## ITranslation
```csharp
/// <summary>
/// Meghatározza és előírja egy fordítási szolgáltatásokat 
/// biztosító osztály elvárt tulajdonságait és módszereit.
/// </summary>
public interface ITranslation
{
    /// <summary>
    /// Az aktuálisan érvényes nyelvi kódot tartalmazó tulajdonság.
    /// </summary>
    string LCID { get; set; }

    /// <summary>
    /// A nyelvi fordítást elvégző metódus.
    /// A szókód típusának megadásával és ha van egyéb paraméter, 
    /// azok behelyettesítésével.
    /// </summary>
    /// <param name="wordCodeType">Egy típus, amely a szókódot jelképezi.</param>
    /// <param name="pars">Objektumok listája, melyek behelyettesítésre kerülnek a <c>String.Format()</c> szabályai szerint.</param>
    /// <returns>A szókódnak megfelelő szöveg a behelyettesítésekkel.</returns>
    string TransFormat(Type wordCodeType, params object[] pars);

    /// <summary>
    /// A nyelvi fordítást elvégző metódus.
    /// A szövegs szókód megadásával és ha van egyéb paraméter, 
    /// azok behelyettesítésével.
    /// </summary>
    /// <param name="wordCodeString">Egy szöveges szókód.</param>
    /// <param name="pars">Objektumok listája, melyek behelyettesítésre kerülnek a <c>String.Format()</c> szabályai szerint.</param>
    /// <returns>A szókódnak megfelelő szöveg a behelyettesítésekkel.</returns>
    string TransFormat(string wordCodeString, params object[] pars);

    /// <summary>
    /// Nyelvi fordítás elvégzése szókód típusa alapján.
    /// </summary>
    /// <param name="wordCodeType"></param>
    /// <returns>A szókódnak megfelelő szöveg.</returns>
    string Trans(Type wordCodeType);

    /// <summary>
    /// Nyelvi fordítás elvégzése szöveges szókód alapján.
    /// </summary>
    /// <param name="wordCodeString">Szöveges szókód.</param>
    /// <param name="defaultTrans">Ha nincs érvényes fordítás, akkor a behelyettesítendő szöveg.</param>
    /// <returns>
    /// A <paramref name="wordCodeString"/> szókódnak megfelelő szöveg, 
    /// vagy a <paramref name="defaultTrans"/> szerinti alapértelmezés.
    /// </returns>
    string Trans(string wordCodeString, string defaultTrans = "");
}
```

## CheckListJSON
```csharp
/// <summary>
/// Egy meghívott akció válaszüzenetének egy lehetséges meghatározott szerkezete.
/// Valamely lista ellenőrzéshez használható, amelyben a Checked oszlopban jelölhető az ellenőrzés eredménye.
/// </summary>
public class CheckListJSON
{
    /// <summary>
    /// Az ellenőrzendő illetve ellenőrzött azonosító.
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    /// Az ellenőrzéskor megtalált név vagy leíró.
    /// </summary>
    public string Text { get; set; }

    /// <summary>
    /// Az ellenőrzés eredményét jelző logikai érték, mely a felhasználáskor
    /// az üzleti logikától függ.
    /// </summary>
    public bool Checked { get; set; }
}
```

## ReturnInfoJSON
```csharp
/// <summary>
/// Egy meghívott akció válaszüzenetének egy lehetséges meghatározott szerkezete.
/// A válasz érték (ReturnValue) és üzenet (ReturnMessage) formájú. 
/// Sikeres végrehajtás esetén mindig 0 legyen a ReturnValue.
/// Sikertelen esetben ettől eltérő, de ha nincs egyéb ok, akkor hiba esetén legyen -1.
/// Alapértelmezett érték: 0, "Az indított akció sikeresen lezajlott!" }
/// </summary>
public class ReturnInfoJSON
{
    /// <summary>
    /// Egy reprezentatív értéke, mely a sikerességtől függ.
    /// Ha nincs hiba az akció végrehajtásában, akkor 0 legyen az értéke.
    /// Alapértelmezett értéke: 0
    /// </summary>
    public int ReturnValue { get; set; } = 0;

    /// <summary>
    /// Az akció üzenete. Hiba esetén a hibaüzenet szövege.
    /// Alapértelmezett értéke: "Az indított akció sikeresen lezajlott!"
    /// </summary>
    public string ReturnMessage { get; set; } = "Az indított akció sikeresen lezajlott!";
}
```

## SelectListJSON
```csharp
/// <summary>
/// Egy meghívott akció válaszüzenetének egy lehetséges meghatározott szerkezete.
/// Egy listához használható, mely értékeit és azonosítóit fel lehet használni.
/// </summary>
/// <remarks>
/// Egyenértékű a System.Web.Mvc.SelectListItem osztállyal, de nem onnan származik.
/// Az ott szereplő leírás: 
/// "Represents the selected item in an instance of the System.Web.Mvc.SelectList class."
/// </remarks>
public class SelectListJSON 
{
    /// <summary>
    /// Jelzi, hogy ez az elem a listában letiltott.
    /// </summary>
    public bool Disabled { get; set; }

    /// <summary>
    /// A csoport jelölése. Alapértelmezett értéke: null
    /// </summary>
    public SelectListGroup Group { get; set; }

    /// <summary>
    /// Jelzi, hogy ez az elem a listában kiválasztott.
    /// </summary>
    public bool Selected { get; set; }

    /// <summary>
    /// A listelem szövege, ami megjelenik.
    /// </summary>
    public string Text { get; set; }

    /// <summary>
    /// A listelem értéke.
    /// </summary>
    public string Value { get; set; }
}

#region SelectListGroup public class
/// <summary>
/// Represents the optgroup HTML element and its attributes. In a select list, 
/// multiple groups with the same name are supported. 
/// They are compared with reference equality.
/// </summary>
/// <remarks>
/// A System.Mvc.SelectListItem-mel való kompatibilitás miatt van itt.
/// A 'summary' szövege is onnan másolt.
/// </remarks>
public class SelectListGroup
{
    /// <summary>
    /// Beállítja, hogy az adott csoport engedélyezett-e.
    /// </summary>
    public bool Disabled { get; set; }

    /// <summary>
    /// A csoport neve.
    /// </summary>
    public string Name { get; set; }
}
#endregion SelectListGroup public class
```

***
# Version History:

#### 1.0.0 (2019.04.01) Initial version
- ISchedulerPlugin interfész létrehozása.
- IManage és ITranslation interfész átemelése ide a Vrh.WEb.Common.Lib-ből. (Egyelőre ott is meglesz a kompatibilitás miatt.)
- CheckListJSON, ReturnInfoJSON, és SelectListJSON osztályok átemelése ide a Vrh.WEb.Common.Lib-ből. (Egyelőre ott is meglesz a kompatibilitás miatt.)
- Dokumentáció létrehozása.


