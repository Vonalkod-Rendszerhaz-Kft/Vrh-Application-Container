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
  > * [XmlConnection osztály](###XmlConnection-osztaly) XmlParser kapcsolati string feldogozása és kifejtése.
  > * [XmlParser osztály](###XmlParser-osztaly) Az ```<XmlParser>``` elem feldogozását elvégző osztály. 
 
* **[BaseController osztály](##BaseControlle-osztaly)**: MVC-s kontroller alaposztály, hasznos és nélkülözhetetlen szolgáltatásokkal.

 > * [ParameterSeparating metódus](###ParameterSeparating-metodus): MVC-s akciók Request.QueryString-jének szétválasztása a kért és egyéb paraméterekre.

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


### VariableDictionary
Egy ```System.Collections.Generic.Dictionary<string,string>``` alapú osztály, 
mely kiterjesztésre került abból a célból, hogy alkalmas legyen Név és Érték kulcspárok 
tárolására, és azok behelyettesítésére egy megadott cél szövegben. 
Az osztály példányosításakor szükséges megadni egy érvényes nyelvi kódot (LCID).
A példányosításkor egyből létrejönnek a rendszer változók is a ...BACK változók
kivételével, azok ugyanis behelyettesítéskor értékelődnek ki. A [rendszerváltozók](####A-rendszervaltozok) nevei
a ```SystemVariableNames``` statikus osztályban érhetőek el. A behelyettesítéskor a változókat
az osztály NameSeparator tulajdonság közé illesztve keresi. A NameSeparator tulajdonság 
alapértelmezett értéke "@@", de átállítható, ha a környezetben más használatos. 

> A táblázatokban csak a ```System.Collections.Generic.Dictionary<string,string>```
osztályt kiterjesztő elemeket mutatjuk be.

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
```void Add(string name, string value)```|Egy darab név-érték pár hozzáadása a gyűjteményhez.
```void Add((NameValueCollection collection)```|Név-érték párok hozzáadása egy létező ```NameValueCollection```-ból.
```void Add((Dictionary<string,string> dictionary)```|Név-érték párok hozzáadása egy létező ```Dictionary<string,string>```-ból.
```void Set(string name, string value)```|Megadott nevű változó értékének módosítása.
```bool ContainsVariable(string name)```|Igaz értékkel jelzi, ha a név már szerepel a gyűjteményben.
```void ResetSystemVariables()```|Rendszerváltozók értékének beállítása.
```string Substitution(string text)```|A szövegbe behelyettesíti a gyűjteményben található változók értékét.
```bool IsValidName(string name)```|Változónév ellenőrzés. A névnek meg kell felelnie az "[a-zA-Z_]\w*" reguláris kifejezésnek.

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
/// Minta a VariableDictionary osztály használatára.
/// </summary>
private static void VariableDictionaryTest()
{
    try
    {
        // Az osztály példányosítása. Nyelvi kód paraméter kötelező.
        VariableDictionary vd = new VariableDictionary("hu-HU", User.Identity.Name);
        Show(vd);

        System.Threading.Thread.Sleep(1000);

        vd.ResetSystemVariables();  //rendszerváltozók újra beállítása
        Show(vd);

        vd.Add("VLTZ", "vltz értéke");  //egy változó hozzáadása
        vd["TODAY"] = "ma";             //egy változó módosítása
        vd.Remove("NOW");               //egy változó törlése
        Show(vd);

        string text = String.Concat(
            "aaaaa@YESTERDAY@bbbbbbb@TODAY@cccccccc",
            "@LASTMONTHLASTDAY@ddddddddddd\neee@DAYSBACK3@ff",
            "ff@WEEKSBACK4@gggg@MONTHSBACK10@hhhh"
        );
        string result = vd.Substitution(text);  //szövegben lévő hivatkozások behelyettesítése
        Console.WriteLine();
        Console.WriteLine($"text= {result}");
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
    }
}

private static void Show(VariableDictionary vd)
{
    Console.WriteLine();
    foreach (KeyValuePair<string,string> s in vd)
        Console.WriteLine($"Name: {s.Key,-25}Value: {s.Value}");
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

### XmlConnection osztály
Az XmlPaser példányosításához egy kapcsolati sztring szükséges, amelyet ez az osztály ellenőriz és
kifejt. Az alábbi táblázat "Tulajdonság" oszlopában zárójelben az látható, hogy az adott összetevőnek
milyen néven kell szerepelnie a kapcsolati stringben.

Tulajdonság|Típus|Leírás
:----|:----|:----
Root (root)|```string```|A gyökér XML fájl az elérési útvonalával együtt, tartalmazhat relatív útvonalat is.
ConfigurationName (config)|```string```|A konfiguráció neve, amit keresünk a gyökér XmlParser fájlban.
File (file)|```string```|Ha ez van megadva a connection stringben, akkor itt van a komponens xml paraméter fájlja.
Element (element)|```string```|Ha ez van megadva a connection stringben, akkor a komponens xml paraméter fájljában ezen elem alatt található a struktúra.

#### Kapcsolati sztring felépítése:
A minimum igény, hogy a 'config' vagy a 'file' tagnak szerepelnie kell.
A config az erősebb, ha mindkettő szerepel. Pár minta:
* "root=D:\SandBox\XmlParser\XmlParser.xml;config=FileManager" vagy
* "root=D:\SandBox\XmlParser\XmlParser.xml;file=D:\aaa\bbb\FileManager.xml;element=RootAlattiElemNév"
Ha 'root' nem szerepel, akkor a gyökér fájl alapértelmezése: "~/App_Data/XmlParser/XmlParser.xml".
A felhasználó komponensek fogadhatnak üres kapcsolati stringet, ha számukra van érvényes alapértelmezett
konfiguráció név. Például a FileManager meghívható kapcsolati sztring nélkül, akkor a FilManager a 
következő sztringet generálja: "config:FileManager", és ezzel inicializálja az XmlParser-t.

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
    public MyXmlProcessor() : base(xmlConnectionString, appPath, lcid, otherVars)
}
```
* **xmlConnectionString** Az XmlParser kapcsolati sztringje. Lásd: [XmlConnection osztály](###XmlConnection-osztaly)
* **appPath** A felhasználó alkalmazásban érvényes alkalmazás mappa. (A '~' jel értéke.)
* **lcid** A nyelvi környezetet meghatárázó nyelvi kód. Ha üres, akkor az XmlParser.xml-ben megadott nyelv kód lesz alkalmazva.
* **otherVars** Egy szótár, mely név érték párokat tartalmaznak, melyek bekerülnek az XmlVars-ok közé. 

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




## BaseController osztály
MVC-s alkalmázokhoz használható kontroller alaposztály, hasznos és 
nélkülözhetetlen szolgáltatásokkal. Főbb szolgáltatások:
- Dispose megvalósítása
- DataTable-hez kapcsolódó hasznos szolgáltatások
- ThrEx: Egy nyelvi fordítóval kiegészített ApplicationException-t dobó metódus.
- Mlmgt: MultiLanguageManager.GetTranslation meghívása a ForcedLanguageCode nyelvi kóddal.


### ParameterSeparating metódus
Az MVC-s akciók Request.QueryString-jének szétválasztása a kért és egyéb paraméterekre.
Az akciókban létrejövő QueryString-et két részre osztja. Egy RequestedParameters és egy 
OtherParameters szótárra. Hogy mi kerüljön a RequestedParameters szótárba azt egy statikus
osztályban érdmes beállítani, melynek segítségével aztán hivatkozhatunk a szótár elemeire.

#### A kért (felhasználandó) paramétereket tartalmazó osztály
Az alábbi minta a Vrh.Web.FileManager Index akciójának lehetséges paraméterei:
```javascript
/// <summary>
/// Az akciók által átvehető url paraméterek nevei.
/// </summary>
public static class QParams
{
    /// <summary>
    /// XmlParser kapcsolati sztring (connection string).
    /// </summary>
    public const string Xml = "xml";

    /// <summary>
    /// A FileManager definíció azonosítója, amely alapján a keresés és megjelenítés megtörténik.
    /// </summary>
    public const string Id = "id";

    /// <summary>
    /// A hívó által kért nyelv kódja, ha üres, akkor a releváns nyelvi kód lesz.
    /// </summary>
    public const string LCID = "lcid";

    /// <summary>
    /// A definícióban megadott gyökér mappa alatti mappa útvonal.
    /// </summary>
    public const string Folder = "folder";

    /// <summary>
    /// A definícióban megadott gyökér mappa alatti mappa útvonal.
    /// </summary>
    public const string File = "filename";
}
```
#### Felhasználási minta
A ```base.ParameterSeparating``` metódus létrehozza és feltölti a
```base.RequestedParameters``` szótárat, melynek pont annyi eleme van,
ahány mezője a ```QParams``` statikus osztálynak, és a szótárban a kulcsok 
megegyeznek az osztály tulajdonságainak értékével. A szótárban az értékek az 
ugyanolyan nevű URL paraméterekben érkezett értéket kapják.
```javascript
public ActionResult Index()
{
    base.ParameterSeparating(typeof(QParams));

    if (String.IsNullOrWhiteSpace(base.RequestedParameters[QParams.LCID]))
    {   // A ForcedLanguageCode a BaseController konstruktorában megkapja a MultiLanguageManager.RelevantLanguageCode-ot
        base.RequestedParameters[QParams.LCID] = base.ForcedLanguageCode;
    }
    else
    {
        base.ForcedLanguageCode = base.RequestedParameters[QParams.LCID];
    }
    if (String.IsNullOrWhiteSpace(base.RequestedParameters[QParams.Folder]))
    {
        base.RequestedParameters[QParams.Folder] = WebCommon.SIGN_BACKSLASH;
    }
    return View(ACTION_INDEX, model);
}
```

Amennyiben nem az alapértelmezett ```Request.QueryString``` ```NameValueCollection```-t
kell feldolgozni, akkor létezik olyan túlterhelése a metódusnak, amelyben megadható a 
feldolgozandó ```NameValueCollection```. Példaként egy POST metódust van itt,
melyben a ```Request.Form``` kollekciót kéne feldolgozni:
```javascript
[HttpPost]
public ActionResult SetValami()
{
    base.ParameterSeparating(Request.Form,typeof(QParams));
}
```


***
## Version History:

#### 1.11.1 (2019.05.22) Patches:
- Frissítés a Microsoft.AspNet.Mvc 5.2.7 változatára.
- Frissítés a Newtonsoft.Json 12.0.1 változatára.

#### 1.11.0 (2018.08.13) Compatible API changes - debug:
- ```CookieApplicationSettings``` osztály kiegészült a "WelcomeUrl" tulajdonsággal, 
hogy a Layout be tudja állítani a logo, és login/logout link értékét.

#### 1.10.1 (2018.08.10) Patches - debug:
- Az XmlParser hibakezelésén kellet javítani. Hibát dob, ha a konfigurációban megadott elem
nem létezik.

#### 1.10.0 (2018.08.10) Compatible API changes - debug:
- ```CookieApplicationSettings``` osztály kiegészült a "ConfigurationName" és a 
"ReferenceName" tulajdonságokkal, hogy ez is megőrződjön az alkalmazás cookie-ban.

#### 1.9.2 (2018.08.08) Patches - debug:
- Az ```UrlElement.GetUrl()``` ```StringBuilder```-t használ, és a paraméterek nevében
vagy értékében előforduló "?&=" jeleket a szabványos URL encode értékre cseréli.
- A ```ParameterSeparating``` metódus kapott egy túlterhelést (overload),
amelyben megadható a feldolgozandó ```NameValueCollection```.

#### 1.9.1 (2018.08.01) Patches - debug:
- A WebCommon.RealPath metódust kellet pontosítani.

#### 1.9.0 (2018.07.27) Compatible API changes - debug:
- ```VariableDictionary``` osztály bevezetése. A ```VariableCollection``` a 2.0-ás
változattól már nem lesz használható.
- ```ParameterQuery``` osztály megszűnt. A szolgáltatások a ```BaseController``` osztály
```ParameterSeparating``` metódusába vándoroltak.
- 'USERNAME' rendszerváltozó automatikusan hozzáadódik az ```OtherParameters```szótárhoz.

#### 1.8.3 (2018.07.26) Patches - debug:
- URL paraméterek azonnal hozzáadódnak az XmlVars gyűjteményhez, és ezeknek az értékét nem
módosíthatja, ha van ilyen változó az xml paraméterfájlokban.

#### 1.8.2 (2018.07.25) Patches - debug:
- XmlParser és BaseController osztályokban történtek javítások.
- A VariableCollection csak olyan változókat tartalmazhat, amelyek neve
megfelel a "[a-zA-Z_]\w*" reguláris kifejezésnek.
- XmlLinqBase az IDisposable osztályból származtatva

#### 1.8.1 (2018.07.24) Patches - debug:
- XmlConnection konstruktorában történt javítás. Alapértelmezett fájl és 
konfigurációnév megadással kapcsolatban.

#### 1.8.0 (2018.07.16) Compatible API changes - debug:
- ParameterQuery osztály bevezetése. A .NET-es akciók Request.QueryString-jének szétválasztása
a kért és egyéb paraméterekre. 
- XmlConnection osztály bevezetése. XmlParser connection string feldolgozásához.
- XmlParser új konstruktorokkal bővült, melyek alkalmasak az XmlConnection,
ParameterQuery osztályok és az XmlParser connection string fogadására a példányosításkor. 

#### 1.7.1 (2018.06.30) Patches - debug:
- A CookieApplicationSettings osztályban a Set metódusban a ProductName és CopyRight
UrlEncode után kerül mentésre, és visszaolvasáskor UrlDecode történik. (A cookie nem
tud letárolni UNICODE-ot.)

#### 1.7.0 (2018.05.30) Compatible API changes - debug:
- Elkészült egy újabb XmlParser konstruktor, mely nem konfigurációs nevet vár, hanem
egy létező xml fájl nevét. Ez a konstruktor feldolgozza a gyökér XmlParser változóit,
de nem foglalkozik annak Configuration elemével.

#### 1.6.0 (2018.05.11) Compatible API changes:
- ValidationExtension static osztály hozzáadása (a régi DataTables.dll-ből átemelve)

#### 1.5.0 (2018.04.13) Compatible API changes:
- CookieApplicationSettings osztály létrehozása.
- A WebCommon static osztály létrehozása, a VRH web alkalmazásokban alapvetően 
vagy sokszor használt tulajdonságok és metódusok eléréséhez.
- ViewModes enum létrehozása (Desktop, Mobile, Touch) értékekkel.

#### 1.4.3 (2018.03.21) Patches:
- VariableCollection.Substitution nem dob hibát, ha null értékű sztringet kap a behelyettesítéshez. Null-t add vissza ilyenkor.

#### 1.4.2 (2018.03.19) Patches:
- XmlParser újra abstract. 

#### 1.4.1 (2018.03.07) Compatible API changes:
- XmlParser az érték nélküli változókat is létrehozza üres string értékkel. 
- Rendszerváltozó nevű XmlVar esetén hiba keletkezik.
- Dokumentácó bővítése, javítása. 

#### 1.4.0 (2018.03.03) Compatible API changes:
- XmlLinqBase, XmlCondition, XmlVariable és XmlParser osztály létrehozása, az XML
feldolgozás egységesítéséhez.
- Dokumentácó bővítése, javítása. 

#### 1.3.2 (2018.01.19) Patches:
- UrlElement osztályban javítás és módosítás (konstruktor).

#### 1.3.1 (2017.12.19) Patches:
- Dokumentáció bővítése, pontosítása.
- Új név került be a rendszerváltozók közé, a "USERNAME".

#### 1.3.0 (2017.12.08) Compatibility API changes::
- VariableCollection osztály létrehozása, az XML feldolgozáskor alkalmazott változók behelyettesítésére, és egységben tartására.
- SystemVariableNames statikus osztály létrehozása a rendszerváltozók egységes kezelése céljából.
- Dokumentációk bővítése és pontosítása.

#### 1.2.3 (2017.11.30) Patches:
- Dokumentáció bővítése, pontosítása.
- BaseController.ErrorMessageBuilder már magától levágja az utolsó soremelést.

#### 1.2.2 (2017.11.07) Patches:
- A Vrh.Common.Serialization.Structures Lib kimozgatása az iScheduler alól ebbe az önálló solutionbe, és átnevezése Vrh.Web.Common.Lib-re
- Nuget csomaggá alakítás
