# Vrh.Common
A modul a Vonalkód Rendszerház fejlesztési környezetében szabványosított és
hasznosan alkalmazható eszközeinek gyűjtőhelye.

> Igényelt minimális framework verzió: **4.5**

> Teljes funkcionalitás és hatékonyság kihasználásához szükséges legalacsonyabb framework verzió: **4.5**

# Főbb összetevők
### Interfészek
* **[IManage](##IManage)**

### Standard osztályok
* **[CheckListJSON](##CheckListJSON)**
* **[ReturnInfoJSON](##ReturnInfoJSON)**
* **[SelectListJSON](##SelectListJSON)**

## IManage
Generikus interfész, melyben azt a típust kell megadni, amely menedzselését végzi.
```javascript
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

## CheckListJSON
```javascript
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
```javascript
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
```javascript
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


# Version History:

## 2.10.0 (2019.10.08):      
### Compatibility API changes:      
- ToEnum string extension hozzáadása, amelyik egy stringből megadott tipusú enumra konvertál, vagy a típus (Enum) defaultját adja

## 2.9.0 (2019.09.12):      
### Compatibility API changes:      
- EntryAsseblyFixer static class hozzáadása, ami beállítja  amegfelelő EntryAssembly-t, ha az alakalmazás tér dinamikus hostolású, ahol null, vagy dynamic az EntryAssembly
- EntryAsseblyFixer egység tesztjei

## 2.8.1 (2019.09.07):
### Patch:
- Name property típusa javítva.

## v2.8.0 (2019.09.06):
### Compatibility API changes:
- ReturnDictJSON adatstruktúra hozzáadva.

## 2.7.0 (2019.08.15) Compatible changes:      
### Compatibility API changes:      
- Az EntityFramework extension bővítése az AlreadyOrdered függvénynel, amely megmondja egy IQueryable-ről, hogy rendezett-e már (tehát a további rendezéséhez az OrderBy, vagy a ThenBy-t kell-e használni)

## 2.6.0 (2019.08.01) Compatible changes:      
### Compatibility API changes:      
- Extensions bővítése az EntityFramework extension-nel, EntityFrameworkQueryHelper bővítő osztály, SmartOrder bővítő metódus (IQueryable típusra)

## 2.5.0 (2019.07.10) Compatible changes:      
### Compatibility API changes:      
- ExtensionMethods bővítése az Enum extensionssel, Enumdata attribútum osztály

## 2.3.0 (2019.05.14) Compatible changes:      
- IManage interfész áthelyezése Vrh.Web.Common.Lib 1.18.1-es változatából.
- Standard osztályok (CheckListJSON, ReturnInfoJSON, SelectListJSON) áthelyezése Vrh.Web.Common.Lib 1.18.1-es változatából.

## 2.2.0 (2018.12.17)
### Compatible changes:      
- VrhConvert.SerializeObject Private metódus publikussá tétele.

## 2.1.0 (2018.12.17)
### Compatible changes:      
- Új függvények beillesztése a conversion részbe. 

## 2.0.1 (2018.12.12)
### Patches:      
- ConnandLine osztály metóduasdinak kommentezése és egy apró javítás: a GetCommandLineArgument lekezeli, ha az argumentname paramétere üres string.

## 2.0.0 (2018.11.28)
### Braking change:
- 3.5-ös .Net framwork verzi támogatásának megszüntetése új target verzió: 4.5
### Compatibility API changes:      
- CommandLine static segéd osztály hozzáadása 

## 1.13.0 (2017.04.04)
### Compatibility API changes:      
- String Extension method: FromHexOrThis      

## 1.12.0 (2017.03.29)     
### Compatibility API changes:      
- FixStack class hozzáadása
### Patches:
- UnitTest method elnevezések javítása (konvenció kidolgozása)

## 1.11.0 (2017.03.28)     
### Compatibility API changes:      
- Assembly Extension methodok: Version, AssemblyAttribute

## 1.10.0 (2017.03.16)     
### Compatibility API changes:      
- StringBuilder Extension methodok: AppendWithSeparator, Reverse      
### Patches:
- Extension methodok helyének hozzáadása.
- Test projekt hozzáadása 

## V1.9.2 (2017.02.21)
### Patches:
- Projekt könyvrtárszerkezet rendbetétele.
- AutoBuild Vrh Nuget csomaggá alakítás (összes Nuget-tel kapcsolatos elvárás átvezetése)
- MinFramweork meghatározás 



 