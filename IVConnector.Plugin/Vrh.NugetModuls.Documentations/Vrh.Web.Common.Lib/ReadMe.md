# Vrh.Web.Common.Lib
Általános célú ASP.NET MVC fejlesztéssel kapcsolatos 
és fordításra kerülő összetevők gyűjteménye.

> A komponens **v1.3.0** változatának kiadásakor kezdődött e leírás elkészítése, ám jelenleg nem naprakész.
> Fejlesztve és tesztelve **4.5** .NET framework alatt. 
> Teljes funkcionalitás és hatékonyság kihasználásához szükséges legalacsonyabb framework verzió: **4.5**

> #TODO: Véglegesítendő a komponens dokumentációja!!!


## Főbb összetevők
* **Általános XML struktúrák**: Több XML-ben is rendszeresen használt szerkezetek leírói.
* **BaseController**: MVC-s kontroller alaposztály, hasznos és nélkülözhetetlen szolgáltatásokkal.
* **CookieWebClient**: MVC-s WebClient osztály kiterjesztése.
* **VariableCollection**: Egy NameValuCollection alapú osztály, az XML feldolgozáshoz kapcsolódó változók kezeléséhez.
* 

## VariableCollection
Egy NameValuCollection alapú osztály, mely kiterjesztésre került abból a célból, 
hogy alkalmas legyen Név és Érték kulcspárok tárolására, és azok behelyettesítésére
egy megadott cél szövegben. 
Az osztály példányosításakor szükséges megadni egy érvényes nyelvi kódot (LCID).
A példányosításkor egyből létrejönnek a rendszer változók is a ...BACK változók
kivételével, azok ugyanis behelyettesítéskor értékelődnek ki. A rendszerváltozók
a SystemVariableNames statikus osztályban érhetőek el. A behelyettesítéskor a változókat
az osztály NameSeparator tulajdonság közé illesztve keresi. A NameSeparator tulajdonság 
alapértelmezett értéke "@", de átállítható, ha a környezetben más használatos.

#### Jelenlegi rendszerváltozók 
> Az XML fájban való hivatkozás bemutatásánál az alapértelmezett név elválasztót használtuk.
 
Név|XML hivatkozás|Leírás
:----|:----|:------
LCID|@LCID@|A nyelvi kódot tartalmazó rendszer változó neve.
USERNAME|@USERNAME@|Egy felhasználó név, melyet példányosításkor vagy később is be lehet állítani.
TODAY|@TODAY@|Mai nap rendszer változó neve.
YESTERDAY|@YESTERDAY@|Tegnap rendszer változó neve.
NOW|@NOW@|Most rendszerváltozó neve.
THISWEEKMONDAY|@THISWEEKMONDAY@|E hét hétfő rendszer változó neve.
THISWEEKFRIDAY|@THISWEEKFRIDAY@|E hét péntek rendszer változó neve.
LASTWEEKMONDAY|@LASTWEEKMONDAY@|Múlt hét hétfő rendszer változó neve.
LASTWEEKFRIDAY|@LASTWEEKFRIDAY@|Múlt hét péntek rendszer változó neve
THISMONTH1STDAY|@THISMONTH1STDAY@|E hónap első napja rendszer változó neve.
THISMONTHLASTDAY|@THISMONTHLASTDAY@|E hónap utolsó napja rendszer változó neve.
LASTMONTH1STDAY|@LASTMONTH1STDAY@|Múlt hónap első napja rendszer változó neve.
LASTMONTHLASTDAY|@LASTMONTHLASTDAY@|Múlt hónap utolsó napja rendszer változó neve.
DAYSBACK|@DAYSBACK#@|Valahány(#) nappal korábbi nap.
WEEKSBACK|@WEEKSBACK#@|Valahány(#) héttel (1hét=7nap) korábbi nap.
MONTHSBACK|@MONTHSBACK#@|Valahány(#) hónappal korábbi nap.

### Osztály használatára egy minta
```javascript
/// <summary>
/// Minta a VariableCollection osztály használatára.
/// </summary>
private static void VariableCollectionTest()
{
    try
    {
        // Az osztály példányosítása. Nyelvi kód paraméter kötelező.
        VariableCollection vc = new VariableCollection("hu-HU", User.Identity.Name);
        Show(vc);

        System.Threading.Thread.Sleep(1000);

        vc.ResetSystemVariables();  //rendszerváltozók újra beállítása
        Show(vc);

        vc.Add("VLTZ", "vltz értéke");  //egy változó hozzáadása
        vc.Set("TODAY", "ma");          //egy változó módosítása
        vc.Remove("NOW");               //egy változó törlése
        Show(vc);

        string text = String.Concat(
            "aaaaa@YESTERDAY@bbbbbbb@TODAY@cccccccc",
            "@LASTMONTHLASTDAY@ddddddddddd\neee@DAYSBACK3@ff",
            "ff@WEEKSBACK4@gggg@MONTHSBACK10@hhhh"
        );
        string result = vc.Substitution(text);  //szövegben lévő hivatkozások behelyettesítése
        Console.WriteLine();
        Console.WriteLine($"text= {result}");
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
    }
}

private static void Show(VariableCollection vc)
{
    Console.WriteLine();
    foreach (string s in vc.AllKeys)
        Console.WriteLine($"Name: {s,-25}Value: {vc[s]}");
}
```
<hr></hr>

# Version History:

## V1.3.2 (2018.01.19)
### Patches:
1. UrlElement osztályban javítás és módosítás (konstruktor).

## V1.3.1 (2017.12.19)
### Patches:
1. Dokumentáció bővítése, pontosítása.
2. Új név került be a rendszerváltozók közé, a "USERNAME".

## V1.3.0 (2017.12.08)
### Compatibility API changes::
1. VariableCollection osztály létrehozása, az XML feldolgozáskor alkalmazott változók behelyettesítésére, és egységben tartására.
1. SystemVariableNames statikus osztály létrehozása a rendszerváltozók egységes kezelése céljából.
2. Dokumentációk bővítése és pontosítása.

## V1.2.3 (2017.11.30)
### Patches:
1. Dokumentáció bővítése, pontosítása.
2. BaseController.ErrorMessageBuilder már magától levágja az utolsó soremelést.

## V1.2.2 (2017.11.07)
### Patches:
1. A Vrh.Common.Serialization.Structures Lib kimozgatása az iScheduler alól ebbe az önálló solutionbe, és átnevezése Vrh.Web.Common.Lib-re
2. Nuget csomaggá alakítás
