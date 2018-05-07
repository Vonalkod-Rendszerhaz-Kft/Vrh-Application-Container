# Vrh.Web.Common.Lib
Általános célú ASP.NET MVC fejlesztéssel kapcsolatos 
és fordításra kerülő összetevők gyűjteménye.

> A komponens **v1.3.0** változatának kiadásakor kezdődött e leírás elkészítése, ám jelenleg nem naprakész.
> Fejlesztve és tesztelve **4.5** .NET framework alatt. 
> Teljes funkcionalitás és hatékonyság kihasználásához szükséges legalacsonyabb framework verzió: **4.5**

> #TODO: Véglegesítendő a komponens dokumentációja!!!


## Főbb összetevők
* **XML feldolgozás támogatása**: VRH-s XML paraméter fájlok feldolgozásának támogatása
hasznos függvényekkel, osztályokkal és rendszeresen előforduló szerkezetek leképezésével.

  > * [XmlLinqBase osztály](###XmlLinqBase-osztaly) Xml feldogozásokhoz készülő osztályok absztrakt alaposztálya. 
  > * [VariableCollection](###VariableCollection) Behelyettesítendő változók összegyűjtése és a behelyettesítés végrehajtása. 
  > * [XmlCondition osztály](###XmlCondition-osztaly) A ```<Condition>``` elem feldogozását segítő osztály. 
  > * [XmlVariable osztály](###XmlVariable-osztaly) Az ```<XmlVar>``` vagy bármely Name, LCID attribútummal és értékkel rendelkező elem feldogozását segítő osztály. 
  > * [XmlParser osztály](###XmlParser-osztaly) Az ```<XmlParser>``` elem feldogozását elvégző osztály. 
 
* **BaseController**: MVC-s kontroller alaposztály, hasznos és nélkülözhetetlen szolgáltatásokkal.
* **CookieWebClient**: MVC-s WebClient osztály kiterjesztése, mely a Cookie-kat is kezeli.
 

## XML feldolgozás támogatása

### XmlLinqBase osztály
Egy absztrakt alaposztály, mely minden specialitás nélkül alkalmas egy XML állomány 
elemeinek és attribútumainak beolvasására és a típusos értékek előállítására. Az osztály
a System.Xml.Linq névtér beli xml kezeléshez nyújt segédeszközöket. Az osztályban minden 
tulajdonság és metódus protected hatáskörű.

Felhasználási minta:
```javascript
public class MyClass : XmlLinqBase { ... }
```

Tulajdonságok|Leírás
:----|:----
CurrentFileName|A Load metódus által legutóbb sikeresen betöltött xml fájl neve. Csak olvasható.
RootElement|GetXElement metódusban megadott útvonal ettől az elemtől számítódik. A Load metódus beállítja (felülírja) ezt az elemet.
EnableTrim|A GetValue metódusok számára engedélyezi a whitespace karakterek eltávolítását a megtalált érték elejéről és végéről. Alapértelmezett értéke: true.

Metódusok|Leírás
:----|:----
void Load(string xmlFileName)|A megadott fájlból betölti az XML struktúrát. Beállítja a CurrentFileName tulajdonságot.
XElement GetXElement(params string[] elementPath)|A RootElement-ben lévő elemtől a sorban megadott elemeken keresztül elérhető elemet adja vissza.
XElement GetXElement(string elementPath, char separator = '/')|A RootElement-ben lévő elemtől a sorban megadott elemeken keresztül elérhető elemet adja vissza.
T GetValue<T>(XElement xelement, T defaultValue, bool isThrowException = false, bool isRequired = false)|Visszad egy element értéket a defaultValue típusának megfelelően.
T GetValue<T>(string attributeName, XElement xelement, T defaultValue, bool isThrowException = false, bool isRequired = false)|Visszadja az xelement elem alatti, megnevezett attribútum értékét.
T GetValue<T>(System.Xml.Linq.XAttribute xattribute, T defaultValue, bool isThrowException = false, bool isRequired = false)|Visszadja az attribútum értékét a kért típusban.
T GetValue<T>(string stringValue, T defultValue)|A megadott típusra konvertál egy stringet, ha a konverzió lehetséges.
TEnum GetEnumValue<TEnum>(XAttribute xattribute, TEnum defaultValue, bool ignoreCase = true)|Attribútum értéket enum típus értékké konvertálja.
TEnum GetEnumValue<TEnum>(XElement xelement, TEnum defaultValue, bool ignoreCase = true)|Elem értéket enum típus értékké konvertálja.
string GetXmlPath(XAttribute xattribute)|Xattribute útvonala '/Element0/Element1/Element2.Attribute' formában.
string GetXmlPath(XElement xelement)|XElement útvonala 'Element0/Element1/Element2' formában.
void ThrEx(string mess, params object[] args)|System.ApplicationException dobása a megadott formázott üzenettel.

#### Beépített kiterjeszthető osztályok

##### XmlLinqBase.ElementNames osztály
Általában használatos elemnevek kiterjeszthető osztálya.

Állandó|Értéke|Leírás
:----|:----|:----
VALUE|"Value"|XML tagokban lehetséges 'Value' elem eléréséhez hasznos állandó.

##### XmlLinqBase.Messages osztály
Általában használatos üzenetek kiterjeszthető osztálya.

Állandó|Értéke
:----|:----
ERR_FILENOTEXIST|"File does not exist! File = {0}"
ERR_XMLROOT|"The root element is missing or corrupt! File = {0}"
ERR_MISSINGELEMENT|"The '{0}' element is missing !"
ERR_MISSINGATTRIBUTE|"The '{0}' attribute is missing in the '{1}' element!"
ERR_REQUIREDELEMENT|"Value of the '{0}' element is null or empty!"
ERR_REQUIREDATTRIBUTE|"Value of the '{0}' attribute is null or empty in the '{1}' element!"
ERR_PARSETOTYPE|"The '{0}' string is not {1} type! Place='{2}'"


### VariableCollection
Egy System.Collections.Specialized.NameValuCollection alapú osztály, mely kiterjesztésre került abból a célból, 
hogy alkalmas legyen Név és Érték kulcspárok tárolására, és azok behelyettesítésére
egy megadott cél szövegben. 
Az osztály példányosításakor szükséges megadni egy érvényes nyelvi kódot (LCID).
A példányosításkor egyből létrejönnek a rendszer változók is a ...BACK változók
kivételével, azok ugyanis behelyettesítéskor értékelődnek ki. A [rendszerváltozók](####A-rendszervaltozok) nevei
a SystemVariableNames statikus osztályban érhetőek el. A behelyettesítéskor a változókat
az osztály NameSeparator tulajdonság közé illesztve keresi. A NameSeparator tulajdonság 
alapértelmezett értéke "@@", de átállítható, ha a környezetben más használatos. 

> A táblázatokban csak a NameValueCollection osztályt kiterjesztő elemeket mutatjuk be.

Tulajdonságok|Leírás
:----|:----
CurentCultureInfo|Az LCID változó beállításakor kap értéket. Csak olvasható.
NameSeparator|A változó neveket e separátorok közé illesztve keresi a szövegben.

Ha a NameSeparator hossza 1, akkor a változót keretező kezdő és befejező karakter azonos, egyébként a 
2. karakter lesz a befejező jel. Alapértelmezett értéke "@@". Amennyiben szükség van egy alternatív
elválasztó párra, akkor párosával növelhető az elválasztó párok száma. Példa: "@@##". 
Ilyenkor elsőként a "@" jelek közés zárt neveket keres, ha ilyet nem talál, akkor kísérletet tesz a "#"
jelek közé zárt változó név keresésre. Ha a Name separator nem 1, 2 vagy páros hoszzúságú, akkor hiba 
keletkezik.

Metódusok|Leírás
:----|:----
void Add((NameValueCollection collection)|Név-érték párok hozzáadása egy létező NameValueCollection-ból.
void Add(string name, string value)|Egy darab név-érték pár hozzáadása a gyűjteményhez.
void Set(string name, string value)|Megadott nevű változó értékének módosítása.
bool ContainsVariable(string name)|Igaz értékkel jelzi, ha a név már szerepel a gyűjteményben.
void ResetSystemVariables()|Rendszerváltozók értékének beállítása.
string Substitution(string text)|A szövegbe behelyettesíti a gyűjteményben található változók értékét.

#### A rendszerváltozók
A rendszerváltozók neve egy statikus SystemVariableNames nevű osztályban érhetőek el.
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

#### Osztály használatára egy minta
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


### XmlCondition osztály
Az alábbi xml struktúra feldolgozását segítő osztály:
```xml
<Condition Type="equal" Test="@VAR1@" With="one">
  four
  <Conditions>
    <Condition Type="equal" Test="@VAR1@" With="xxx">xxx</Condition>
    <Condition Type="equal" Test="@VAR1@" With="one">five</Condition>
  </Conditions>
</Condition>
```
A 'Condition' XML elem megszerzése után egy példányosítással előáll egy XmlCondition
típus.
```javascript
XmlCondition condition = new XmlCondition(XML.Element("Condition"));
```
Tulajdonság|Típus|Leírás
:----|:----|:----
Type|```string```|A feltétel típusa. Types enum értékek valamelyike. Alapértelmezett érték: Types.Equal.
Test|```string```|A egyik tesztelendő karakterlánc. Lehet benne XML változó. Types.Match esetén a kiértékelendő karakterléncot tartalmazza.
With|```string```|A másik tesztelendő karakterlánc. Lehet benne XML változó. Types.Match esetén a kiértékelő reguláris kifejezést tartalmazza.
Value|```string```|Igaz értékű feltétel esetén, ez lesz a releváns érték.
Conditions|```List<XmlCondition>```|XmlCondition típusú elemek listája. Ha a releváns érték más feltétel(ek)től is függ.

Metódus|Leírás
:----|:----
bool Evaluation(VariableCollection xmlVars)|A feltétel kiértékelése. Mivel létezhetnek hivatkozások, ezért meg kell adni a változók gyűjteményét.

### XmlVariable osztály
Az alábbi struktúra feldolgozását segítő osztály:
```xml
<XmlVar Name="VAR3" LCID="hu-HU">
  <Conditions>
    <Condition Type="equal" Test="@VAR1@" With="one">
        four
        <Conditions>
        <Condition Type="equal" Test="@VAR1@" With="xxx">xxx</Condition>
        <Condition Type="equal" Test="@VAR1@" With="one">five</Condition>
        </Conditions>
    </Condition>
  </Conditions>
  three
</XmlVar>

VAGY

<XmlVar Name="VAR4" LCID="hu-HU">érték</XmlVar>
```
Xml változókat leképező osztály. Tulajdonképpen minden elem, melynek 'Name', 'LCID' attribútuma lehetséges, 
és van 'Value' tulajdonsága, és lehetnek benne feltétek (Conditions).
Az 'XmlVar' XML elem megszerzése után egy példányosítással előáll egy XmlVariable típus.
```javascript
XmlVariable variable = new XmlVariable(XML.Element("XmlVar"));
```

Tulajdonság|Típus|Leírás
:----|:----|:----
Name|```string```|A változó neve. Csak olyan változó jön létre a példányosításkor, amelynek létezik a megnevezése.
LCID|```string```|A változó értékét, melyik nyelvi környezet esetén lehet felhasználni. Ha nincs vagy üres, akkor mindegyikben.
Value|```string```| A változó értéke.
Conditions|```List<XmlCondition>```|A változó végleges értékét befolyásoló feltételek listája. Az első igaz érték lesz a végleges érték.

Metódus|Leírás
:----|:----
bool Evaluation(VariableCollection xmlVars)|A változó kiértékelése. Mivel létezhetnek hivatkozások a feltételekben, ezért meg kell adni a változók gyűjteményét.

#### Beépített kiterjeszthető osztályok

##### XmlVariable.ElementNames osztály
Általában használatos elemnevek kiterjeszthető osztálya. Az XmlLinqBase hasonló osztályát terjeszti ki.

Állandó|Értéke|Leírás
:----|:----|:----
XMLVAR|"XmlVar"|XML tagokban lehetséges 'XmlVar' elem eléréséhez hasznos állandó.
CONNECTIONSTRING|"ConnectionString"|XML tagokban lehetséges 'ConnectionString' elem eléréséhez hasznos állandó.

##### XmlVariable.AttributeNames osztály
Általában használatos attribútumnevek kiterjeszthető osztálya.

Állandó|Értéke|Leírás
:----|:----|:----
NAME|"Name"|XML tagokban lehetséges 'Name' attribútum eléréséhez hasznos állandó.
LCID|"LCID"|XML tagokban lehetséges 'LCID' attribútum eléréséhez hasznos állandó.

### XmlParser osztály
Az 'XmlParser' XML elem feldolgozását elvégző absztrakt osztály. A VRH paraméterező XML
állományainak egységes szerkezetű eleme, mely definiálja az állomány részére a változókat, és kapcsolatokat. 
Valamint meghatározza a paraméterezés nyelvi környezetét, ha azt nem adják meg a programban.
Az XmlParser elem általános felépítése és logikája a következő:
```xml
<ParserConfig LCID="" NameSeparator="">        <!-- NameSeparator opcionális -->
  <XmlParser>                              
    <ConnectionString Name="" LCID="">...VALUE...</ConnectionString><!-- LCID opcionális -->
    <XmlVar Name="" LCID="">...VALUE...</XmlVar>
    <XmlVar Name="" LCID="">...VALUE...</XmlVar>
    <XmlVar Name="" LCID="">...VALUE...
      [<Conditions>
        <Condition Type="" Test="" With="">...VALUE...
          [<Conditions>...</Conditions>]
        </Condition>
      </Conditions>]
    </XmlVar>
  </XmlParser>
  <Configurations>
    <Configuration Name="Sample1" File="Sample1File" Element="FirstElement/SecondElement">
    <Configuration Name="Sample2" Element="Sample2Element">        <!-- File,Element valamelyik kötelező -->
    <Configuration Name="Sample3" File="Sample3File">
  </Configurations>
  <Sample2Element>
    [<XmlParser>...<XmlParser>]
    .
    .
    .
  </Sample2Element>
</ParserConfig>
```
Sample1File:
```xml
<Root>
  [<XmlParser>...</XmlParser>]
  <FirstElement>
    <SecondElement>
      [<XmlParser>...</XmlParser>]
      .
      .
      .
    </SecondElement>
  </FirstElement>
</Root>
```
Sample3File:
```xml
<Root>
  [<XmlParser>...</XmlParser>]
  .
  .
  .
</Root>
```
Az osztály a lenti sorrendben és helyeken elvégzi az XmlParser elemek feldoglozását:
* Az XmlParser.xml fájl gyökerében elhelyezett XmlParser elem
* A konfiguráció által meghatározott fájl gyökér eleme alatti XmlParser elem
* A konfiguráció által meghatározott fájl és egy ottani elem alatti XmlParser elem
Az XmlVar elemek (a továbbiakban változók) egy későbbi XmlParser-ban felülírhatóak.
Másképpen fogalmazva: ha a feldolgozás során olyan változót talál, mely már egy korábbi 
XmlParser-ban szereplet, akkor annak értéke felülíródik a későbben megtalált ilyen nevű
változó értékével. Az XmlParser elemek feldolgozása után az előfeldolgozó a paraméterező XML
állományban elvégzi a változókra való hivatkozások feloldását, azaz behelyettesíti a hivatkozások
helyére a változók értékét. Utána a saját xml feldolgozó részére megtartja a tartalmat.
Az XmlParser.RootElement (XmlLinqBase.RootElement) tulajdonságában már egy olyan XML struktúra van, 
amelyben a behelyettesítések el lettek végezve. Az XmlParser (XmlLinqBase) által szolgáltatott 
metódusok már mind ezen az elemen dolgoznak.

Az osztály egy absztrakt osztály, felhasználása a következő módon lehetséges:
```javascript
public class MyXmlProcessor : XmlParser
{
    public MyXmlProcessor() : base(fileName, configName, lcid)
}
```
* **fileName** Az XmlParser.xml állomány helye a fizikai elérési útvonalával együtt
* **configName** Annak a konfigurációnak a neve, amelyet elő kell készíteni a MyXmlProcessor számára.
* **lcid** A nyelvi környezetet meghatárázó nyelvi kód. Ha üres, akkor az XmlParser.xml-ben megadott 
nyelv kód lesz alkalmazva.

Tulajdonság|Típus|Leírás
:----|:----|:----
XmlVars|```VariableCollection```|Változók gyűjteménye, mely tartalmazza az összes változót az értékével együtt.
ConnectionStrings|```VariableCollection```|Kapcsolatok gyűjteménye, mely tartalmazza a struktúra összes különböző nevű kapcsolatát.
CurrentFileName|```string```|Az épp feldolgozás alatt álló Xml fájl a teljes fizikai elérési útvonalával.
Configuration|```ConfigurationType```|A megadott nevű komponens Configuration elemének értékei.

#### Beépített static osztályok
##### XmlParser.Defaults osztály

Állandó|Értéke|Leírás
:----|:----|:----
XMLFILE|@"~\App_Data\XmlParser\XmlParser.xml"|Az XMLParser.xml fájl meghatározott helyének állandója.

A controllerben a következő utasítással feloldható: ```Server.MapPath(XmlParser.Defaults.XMLFILE);```

#### Beépített osztályok
Ha szükséges vagy hasznos, akkor a felhasználó osztály kiterjesztheti ezeket.

##### XmlParser.ElementNames osztály
Általában használatos elemnevek kiterjeszthető osztálya. Az XmlLinqBase hasonló osztályát terjeszti ki.

Állandó|Értéke|Leírás
:----|:----|:----
XMLPARSER|"XmlParser"|XML tagokban lehetséges 'XmlVar' elem eléréséhez hasznos állandó.
XMLVAR|"XmlVar"|XML tagokban lehetséges 'XmlVar' elem eléréséhez hasznos állandó.
CONNECTIONSTRING|"ConnectionString"|XML tagokban lehetséges 'ConnectionString' elem eléréséhez hasznos állandó.
CONFIGURATIONS|"Configurations"|XML tagokban lehetséges 'Configurations' elem eléréséhez hasznos állandó.
CONFIGURATION|"Configuration"|XML tagokban lehetséges 'Configuration' elem eléréséhez hasznos állandó.

##### XmlParser.AttributeNames osztály
Általában használatos attribútumnevek kiterjeszthető osztálya.

Állandó|Értéke|Leírás
:----|:----|:----
NAME|"Name"|XML tagokban lehetséges 'Name' attribútum eléréséhez hasznos állandó.
LCID|"LCID"|XML tagokban lehetséges 'LCID' attribútum eléréséhez hasznos állandó.
NAMESEPARATOR|"NameSeparator"|XML tagokban lehetséges 'NameSeparator' attribútum eléréséhez hasznos állandó.
FILE|"File"|XML tagokban lehetséges 'File' attribútum eléréséhez hasznos állandó.
ELEMENT|"Element"|XML tagokban lehetséges 'Element' attribútum eléréséhez hasznos állandó.

***
# Version History:
## V1.5.0 (2018.04.13)
### Compatible API changes:
1. CookieApplicationSettings osztály létrehozása.
2. A WebCommon static osztály létrehozása, a VRH web alkalmazásokban alapvetően vagy sokszor használt tulajdonságok és metódusok eléréséhez.
3. ViewModes enum létrehozása (Desktop, Mobile, Touch) értékekkel.

## V1.4.3 (2018.03.21)
### Patches:
1. VariableCollection.Substitution nem dob hibát, ha null értékű sztringet kap a behelyettesítéshez. Null-t add vissza ilyenkor.

## V1.4.2 (2018.03.19)
### Patches:
1. XmlParser újra abstract. 

## V1.4.1 (2018.03.07)
### Compatible API changes:
1. XmlParser az érték nélküli változókat is létrehozza üres string értékkel. 
2. Rendszerváltozó nevű XmlVar esetén hiba keletkezik.
3. Dokumentácó bővítése, javítása. 

## V1.4.0 (2018.03.03)
### Compatible API changes:
1. XmlLinqBase, XmlCondition, XmlVariable és XmlParser osztály létrehozása, az XML feldolgozás egységesítéséhez.
2. Dokumentácó bővítése, javítása. 

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
