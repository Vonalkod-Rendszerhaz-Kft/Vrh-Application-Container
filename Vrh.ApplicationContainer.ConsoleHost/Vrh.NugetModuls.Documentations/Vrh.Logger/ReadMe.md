# Vrh.Logger
Ez a leírás a komponens **v1.4.0** kiadásáig bezáróan naprakész. (1.5.0 nincs benne)
Igényelt minimális framework verzió: **4.0**
Teljes funkcionalitás és hatékonyság kihasználásához szükséges legalacsonyabb framework verzió: **4.5**
### A komponens arra szolgál, hogy egységes és egyszerű megvalósítása legyen a Logolásnak.

Pluginolható, a létező plugin megoldásaink a Vrh.Logger.PLUGINNAME komponens elnevezésekkel kerülnek implementációra.

## Használata logoláshoz
A használathoz a **Vrh.Logger névtér** using-olandó.
### VrhLogger static class:
Ez a static osztály adja a logoláshoz szükséges szűk eszközkészletet.
#### 1. Log\<T>():
Ezt hívjuk meg, ha bármit logolni akarunk a környezetben.

**Definíciója:**
```csharp
/// <summary>
/// Log bejegyzést ad fel (aszinkron!!!) a logoló modul számára
/// </summary>
/// <typeparam name="T">Típus</typeparam>
/// <param name="data">Log adat, meg kell feleljen a generikusan meghatározott típusnak, vagy ennek a típusa határozza meg a generikust, ha nem jelöljük explicit</param>
/// <param name="dataFields">A log bejegyzés adatai. Mezőnév/Mezőérték párok listájaként</param>
/// <param name="exception">Egy exception adható át a Lognak</param>
/// <param name="level">A logolás szintje. Az itt meghatározott szintűnek, vagy az alattinak kell a beállított logolási szintnek lennie, hogy a logolás meg is történjen a tényleges futási környezetben.</param>
/// <param name="source">Logolás forrása. Mindig egy típust (Type) kell átadni. Instance szintű tagoknál a this.GetType() kifejezést használjuk a paraméter értékadásához! 
///                         Static tagoknál a typeof(KonkrétClassNév) kifejezéssel adjuk át a típust.</param>
/// <param name="caller">4.5 Frameworktől automatikusan kap értéket a CallerMemberName attribútumon át, sose adjunk meg értéket ennek a paraméternek! 4.5 alatti verzióknál a hívás helyén kell gondoskodni a kitöltéséről, ha használni  akarjuk a logban ezt az adatot.</param>
/// <param name="line">4.5 Frameworktől  Automatikusan kap értéket a CallerLineNumber attribútumon át, sose adjunk meg kézzel értéket ennek a paraméternek! 4.5 alatti verzióknál a hívás helyén kell gondoskodni a kitöltéséről, ha használni  akarjuk a logban ezt az adatot.</param>
static public void Log<T>(T data, IDictionary<string, string> dataFields, Exception exception, LogLevel level, Type source, [CallerMemberName]string caller = "", [CallerLineNumber]int line = 0)     
```
Hogy a paraméterekkel a logolás során mi történik a használt logger pluginban, az erősen függ a használt plugin implementációjától.
Az erre vonatkozó részleteket az adott Vrh.Logger Plugin dokumentációja írja le. 
Alább a A Logger komponens fejlesztő támogatására szolgáló DefaultLogger implementációban megvalósuló viselkedést adjuk meg.
(Fontos megjegyzés a DefultLogger pluginnal kapcsolatban, hogy az tényleges logolásra nem alkalmas, csak a consolra és a csatlakoztatott debugger outputjára képes elküldeni a bedobott log bejegyzéseket.)
* A generikusba bármilyen típust bedobhatunk (a konkrét példányt a **data** paraméterben lehet átadni).
A defult logger JSON-ba serializálja a típust, de null is átadható neki.
* A **dataFileds**-ben kulcs értékpárral megadott adatok adhatóak át. A Defultlogger ezeket kulcs: érték formában jeleníti meg, null érték is átadható neki.
* Az **exception**-ben egy exceptiont dobhatunk be, amit át kívánunk adni logolásra. A DefultLogger a **LogHelper.GetExceptionInfo** segíytségével jeleníti meg az exception adatait. (Lásd lejebb a Loghelper dokumentációjában.)  
* A **level** megmondja, hogy a log bejegyzést melyik logszint beállítástól kezdődően kell ténylegesen a logba küldeni. Ha az aktuálisan beállított logolási szint legalább ekkora, akkor történik ténylegesen logolás (a logolásra használt ILogger interfészt implementáló plugin Log<T> metódusának hívása.)
* A **source** segítségével adhatjuk meg a logolás forrásának a típusát. Mindig adjuk meg a hívás helyén ezt az értéket, hogy a Logolás releváns információkat tartalmazhasson! Static tagoknál a typeof(MyStaticClass) a helyes átadási mód, míg instance szintű tagoknál a this.GetType()-ot használjuk! A Logger a pluginnak ezt az információt nem adja tovább, hanem kibontja belőle a típust definiáló Assembly nevét, annak definiált verzióját, és a típus teljes nevét (névtérrel együtt). Ezen információk adódnak át a használt Logger pluginnak. A DefaultLogger plugin ezeket megjeleníti.   
* A **caller** paramétert 4.5-ös Frameworktől kezdődően sose töltsük ki a hívásokban, az automatikusan értéket kap, és a metódus neve kerül bel, ahonnan a Log metódust meghívtuk. 4.5 alatti Fremwork esetén ezt az információt a hívási helyről át kell adni a hívásban, ha kívánunk a Logger pluginnak átadni ilyen információt.  
* A **line** paramétert 4.5-ös Frameworktől kezdődően sose töltsük ki a hívásokban, az automatikusan értéket kap, és a forrássor száma kerül bel, ahonnan a Log metódust meghívtuk. 4.5 alatti Fremwork esetén ezt az információt a hívási helyről át kell adni a hívásban, ha kívánunk a Logger pluginnak átadni ilyen információt.
 
 **Példák a használatára**:
 ```csharp    
catch (Exception ex)
{
    VrhLogger.Log<string>("error", null, ex, LogLevel.Error, this.GetType()));
}
 ```
 ```csharp
VrhLogger.Log<ApplicationTrust>(AppDomain.CurrentDomain.ApplicationTrust, null, null, LogLevel.Verbose, typeof(Program));
 ```
 ```csharp
Dictionary<string, string> logData = new Dictionary<string, string>()
{
    { "Vrh.Loger Version ", GetModulVersion(typeof(VrhLogger)) },
    { "Used Logger", _logger != null ? _logger.GetType().FullName : String.Format("Not found!!! {0}", loggerTypeSelector) },
    { "Used Logger version", _logger != null ? GetModulVersion(_logger.GetType()) : "?" },
};
logData.Add("Used Plugin == Defult Logger", (_defaultLogger == null).ToString());
Logger.Log<String>("Vrh.Logger Started", logData, null, _logger != null ? LogLevel.Information : LogLevel.Warning, typeof(VrhLogger));
```
#### 1.1 String logolása
Hogy ne kelljen a bonyolultabb formát beírni, egyszerű szöveges információ logolására használható az alábbi Log metódus is:
```csharp
/// <summary>
/// Egyszerű string adat logolása.
/// </summary>
/// <param name="logMessage">Log üzenet</param>
/// <param name="level">Logszint</param>
/// <param name="source">Logolás forrása. Mindig egy típust (Type) kell átadni. Instance szintű tagoknál a this.GetType() kifejezést használjuk a paraméter értékadásához! 
///                         Static tagoknál a typeof(KonkrétClassNév) kifejezéssel adjuk át a típust.</param>
/// <param name="caller">4.5 Frameworktől automatikusan kap értéket a CallerMemberName attribútumon át, sose adjunk meg értéket ennek a paraméternek! 4.5 alatti verzióknál a hívás helyén kell gondoskodni a kitöltéséről, ha használni  akarjuk a logban ezt az adatot.</param>
/// <param name="line">4.5 Frameworktől  Automatikusan kap értéket a CallerLineNumber attribútumon át, sose adjunk meg kézzel értéket ennek a paraméternek! 4.5 alatti verzióknál a hívás helyén kell gondoskodni a kitöltéséről, ha használni  akarjuk a logban ezt az adatot.</param>
static public void Log(string logMessage, LogLevel level, Type source, [CallerMemberName]string caller = "", [CallerLineNumber]int line = 0)
```
**Példák a használatára**:
```csharp
VrhLogger.Log("Simple stringlog", LogLevel.Information, typeof(Program));
```
##### 1.2. Exception logolása
Hogy ne kelljen a bonyolultabb formát beírni, kivétel logolására használható az alábbi Log metódus is:
```csharp
/// <summary>
/// Exception egyszerű logolása
/// </summary>
/// <param name="exception">Logolandó kivétel</param>
/// <param name="source">Logolás forrása. Mindig egy típust (Type) kell átadni. Instance szintű tagoknál a this.GetType() kifejezést hazsnáljuk a paraméter értékadásához! 
///                         Static tagoknál a typeof(KonkrétClassNév) kifejezéssel adjuk át a típust.</param>
/// <param name="level">Logszint</param>
/// <param name="caller">4.5 Frameworktől automatikusan kap értéket a CallerMemberName attribútumon át, sose adjunk meg értéket ennek a paraméternek! 4.5 alatti verzióknál a hívás helyén kell gondoskodni a kitöltéséről, ha használni  akarjuk a logban ezt az adatot.</param>
/// <param name="line">4.5 Frameworktől  Automatikusan kap értéket a CallerLineNumber attribútumon át, sose adjunk meg kézzel értéket ennek a paraméternek! 4.5 alatti verzióknál a hívás helyén kell gondoskodni a kitöltéséről, ha használni  akarjuk a logban ezt az adatot.</param>
static public void Log(Exception exception, Type source, LogLevel level = LogLevel.Error, [CallerMemberName]string caller = "", [CallerLineNumber]int line = 0)
```
**Példák a használatára**:
```csharp
catch (Exception ex)
{
    VrhLogger.Log(ex, typeof(Program));
}
```
#### 2. LoadLoggerPlugin():
Arra használható, hogy anélkül cseréljünk az alkalmazás térben használt Logger plugint, hogy az alkalmazást újraindítanánk. A Vrh.Logger működéséhez nem kell explicit meghívni, mert a Logger static típus static konstruktora is meghívja.
 
**Definiciója:**
```csharp
/// <summary>
/// Betölti a Logger által használt Plugint
///     A static construktor meghívja, azért public, hogy le lehessen programozni egy olyan alkalmazás logikát, ahol app restart nélkül logger plugint cserélünk.
/// </summary>
/// <param name="config">A használt konfigurációt jelöli ki</param>  
static public void LoadLoggerPlugin(string config = null)
```
* Egyetlen paramétert lehet neki itt átadni, azt a konfigurációt definiáló stringet, amelyet a plugin a működéséhez használjon. Ha ilyet nem adunk meg, akkor alapértelmezés szerinti konfigurációt használ. (A Vrh.Logger konfigurációjáról, és annak működéséről a dokumentum lejjebb tartalmaz információkat.)

**Használata:**
```csharp
VrhLogger.LoadLoggerPlugin(„LogConfig.xml/Vrh.Logger”);
```
#### 3. SetNoOfLogEntry():
Segítségével egy olyan szolgáltatást implementálhatunk a felhasználás helyén, amivel egy adott sorszámra állítjuk a Logsorszámot. Az ilyesmi például hibakereséshez jöhet jól.

**Definiciója:**
```csharp
/// <summary>
/// Segítségével implementálható a felhasználás helyén egy olyan szolgáltatás, amivel explicit módon beállítható, hogy mennyi legyen a Logsorszám 
/// </summary>
/// <param name="newNoOfLogEntry">Erre az értrékre áll be  asorszám, ha nincs mergadva, akkor 0</param>
public static void SetNoOfLogEntry(ulong newNoOfLogEntry=0)
```
* Egyetlen paramétert lehet neki átadni, hogy mennyi legyen a log sorszám aktuális értéke.

**Használata:**
```csharp
VrhLogger.SetNoOfLogEntry(9999);
```
 ### LogHelper static class:
 Az olyan hasznos szolgáltatásokat biztosít egyszerű metódusokon keresztül, amelyek a logolással kapcsolatban hasznosak lehetnek. Jelenlegi szolgáltatásai:
 * **GetAccurateDateTimeValue**: Egy időbélyeg jellegű adatot nagy pontosságot megjelenítő stringként ad vissza, időzóna információval (*yyyy.MM.dd HH:mm:ss.fffffff (zzz)*).
```csharp
/// <summary>
/// Visszaadja a kapott időpont nagy pontosságú string reprezentánsát 
/// </summary>
/// <param name="value">Időbélyeg jellegű adat</param>
/// <returns>A kapott időbélyeg stringként</returns>
public static string GetAccurateDateTimeValue(DateTime value)
```
* **GetExceptionInfo**: Az átadott exception adatait emberi olvasásra formázott stringként adja vissza. A megadott indentLevel szerinti tab-ot tesz minden egyes sor elejére. 
```csharp
/// <summary>
/// Kiszedi a kapott kivétel adatait egy szöveges leírássá
/// </summary>
/// <param name="ex">Exception, amelynek az adatait kigyűjtjük</param>
/// <param name="indentLevel">Ennyi tab-bal indentálja az információ minden egyes sorát</param>
/// <returns>A kapott Exception adatai stringként</returns>
public static string GetExceptionInfo(Exception ex, int indentLevel = 0)
``` 
>A megjelenített adatok:
>* Az Exception konkrét típusa
>* A Message field tartalma
>* A StackTrace tartalma
>* Ha az InnerException nem null, akkor annak tartalma, ugyanígy megformázva, ezzel az adat tartalommal. Tetszőleges mélységig (amíg az InnerException-ök is tartalmaznak InnerExceptiont). Minden egyes InnerExceptiont egyel beljebb indentálva.
>* Ha az adott Exceptoion típusa ReflectionTypeLoadException, akkor annak LoaderExceptions gyűjteményén is végigiterál, kigyűjtve azok InnerException-jeit is.  
* **GetExceptionData**: Segítségével Exception.Data (vagy bármilyen más Idictionary) adatokat rendezhetünk emberi olvasásra szánt szöveggé, úgy, hogy minden kulcs-érték párnak új sort nyit, és előre a kulcsot, majd mögé az adatot írja.
```csharp
/// <summary>
/// Visszadja egy IDictionary-ben tárolt kulcs érték párokat (pl.: Exception.Data)
/// </summary>
/// <param name="data">Idictionary gyűjtemény</param>
/// <param name="indentLevel">Ennyivel lesz beindentálva</param>
/// <returns></returns>
public static string GetExceptionData(IDictionary data, byte indentLevel)
```
* **MakeIndent**: Segítségével egy strin g minden egyes sorát (/n karakterek, mint sorvégek) a megadott szinttel beljebb indentálja, úgy, hogy a sorok elejére /t karaktereket helyez el az indentLevel-nek megfelelő számban.
```csharp
/// <summary>
/// Tabokkal indentálja a kapott szöveg minden egyes sorát a paraméterben kapott számnak megfelelő számban
/// </summary>
/// <param name="input">Az indentálandó szöveg</param>
/// <param name="indentLevel">Az indentálás szintje</param>
/// <returns>Indentált szöveg</returns>
public static string MakeIndent(string input, int indentLevel)
```
* **GetDictionaryInfo**: Segítségével kulcs értékpárok listájaként átadott adatokat rendezhetünk emberi olvasásra szánt szöveggé, úgy, hogy minden kulcs-érték párnak új sort nyit, és előre a kulcsot, majd mögé az adatot írja.
```csharp
/// <summary>
/// Visszaadja a kapott Dictonary tartalmát emberi olvasásra formázott kulcs értékpárok listájaként
/// </summary>
/// <param name="dict">Az adatokat tartalmazó Dictionary</param>
/// <param name="IndentLevel">Ennyivel lesz beindentálva</param>
/// <returns>Az adatok formázott stringként</returns>
public static string GetDictionaryInfo(IDictionary<string, string> dict, byte IndentLevel = 0)
```
* **HexControlChars**: A kapott szövegben esetleg megtalálható nem vizuális karaktereket hexa karakterkód értékre alakítja, és ezeket a megadott jelek közt, mint keretben illeszti be az a szövegben eredetileg is elfoglalt helyükre.
```csharp
/// <summary>
/// A megadott sztring kontrolkaraktereit hex formátummá alakítja.
///  A kontrol karakterek jelölésére használt keret karaktereket (frameStart, frameEnd), szintén hexa karakterkódra alakítja, ha előfordulnak az eredeti szövegben.
/// </summary>
/// <param name="str">A vezérlőkaraktereket is tartalmazó szöveg (string)</param>
/// <param name="indentLevel">A formázott szöveg indentálásának a mértéke</param>
/// <param name="frameStart">A vezérlőkarakterek hexakódjainak keretezésére ezt használja nyitó karakternek</param>
/// <param name="frameEnd">A vezérlőkarakterek hexakódjainak keretezésére ezt használja záró karakternek</param>
/// <returns>A formázott szöveg</returns>
public static string HexControlChars(string str, byte indentLevel = 0, char frameStart = '{', char frameEnd = '}')
```
* **ArrayToHumanReadableString**: Konvertál egy tömböt emberi olvasásra alkalmas stringgé (a tömb értékei a megadott elválasztó karakterrel kerülnek egymástól elválasztásra). A tömbben tárolt típusnak a tárolt adat szöveges vizualizációjára nézve releváns ToString overloaddal kell rendelkeznie, a helyes működéshez.  
```csharp
/// <summary>
/// Konvertál egy tömböt emberi fogyasztásra alkalmas stringgé.
///     A T-nek a tárolt értékre nézve releváns ToString overloaddal kell rendelkeznie, a helyes működéshez.
/// </summary>
/// <typeparam name="T">Típus kijelölő</typeparam>
/// <param name="valueArray">Az értékeket tartalmazó tömb</param>
/// <param name="indentLevel">A formázott szöveg indentálásának a mértéke</param>
/// <param name="valueSeparator">Az értékeket egymástól elválasztó karakter a formázott szövegben.</param>
/// <returns>a formázott szöveg</returns>
public static string ArrayToHumanReadableString<T>(T[] valueArray, byte indentLevel = 0, char valueSeparator = ';') 
``` 
* **BytesToHumanReadableString**: A megadott bájt tömböt ascii encodolással szöveggé alakítja, az esetleges vezérlőkaraktereket hexa értékre cserélve. Tipikus felhasználási területe alacsony szintű kommunikációs pufferek tartalmának logolása.
```csharp
/// <summary>
/// A megadott bájt tömböt Ascii encodolással szöveggé alakítja, az esetleges vezérlőkaraktereket hexa értékre cserélve 
///  Tipikus felhasználási területe alacsony szintű kommunikációs pufferek tartalmának logolása
/// </summary>
/// <param name="bytearray">A konvertálandó bytok sorozata</param>
/// <param name="indentLevel">A formázott szöveg indentálásának a mértéke</param>
/// <returns>A formázott szöveg</returns>
public static string BytesToHumanReadableString(byte[] bytearray, byte indentLevel = 0)
``` 
* **BytesToHexBlock**: byte sorozatot HexBlock formázású, emberi olvasásra szánt stringgé alakít.
```csharp
/// <summary>
/// Egy byte sorozatot HexBlock formázású, emberi olvasásra szánt stringgé alakít
/// </summary>
/// <param name="data">a bemenő bájtsorozat</param>
/// <param name="header">megadja, hogy az első sor egy header legyen, amely magyarázza az adattartalmat</param>
/// <param name="footer">megadja, hogy legyen e egy  hexablock véget értét hangsúlyozó lezárás a block végén</param>
/// <param name="encoding">Beállítható vele, a használt karakter encoding, alapértelmezésben ASCII-t fog használni</param>
/// <param name="indentLevel">Annyivel indentálja a hexa blockot</param>
/// <returns>a formázott szöveg</returns>
public static string BytesToHexBlock(byte[] data, bool header = true, bool footer = false, Encoding encoding = null, byte indentLevel = 0)
```

* **ExtractPropertyValuesToDataFields**: Visszadja a paraméterben kapott objektum propertijeinek értékeit név-érték párok litsájaként, amit közvetlenül odalehet adni a VrhLogger.Log hívásoknak, hogy kiírja az értékekeket. A property-ken a ToString-et hívja az értékhez, így az érték string reprezentánsa az adott típus ToString megvalósításának megfelelő lesz.
```csharp
        /// <summary>
        /// Visszadja a paraméterben kapott objektum propertijeinek értékeit név-érték párok litsájaként (a property-n a ToString-et hívja az értékhez)
        /// </summary>
        /// <typeparam name="TClass">Ezzel az objektum típussal dolgozik</typeparam>
        /// <param name="instance">Ezzel a példánnyal</param>
        /// <returns>név-érték párok litáájaként a property-k neve és értéke</returns>
        public static Dictionary<string, string> ExtractPropertyValuesToDataFields<TClass>(TClass instance)
```
### Konfiguráció:
A Vrh.Logger konfigurációjának megadásához kétféle módszert támogat:
1. Vagy egy az alkalmazástér konfigurációjából vesz fel appSettings kulcsokat.
2. Vagy egy XML fájlból olvassa ki a működési paramétereket.
 
#### Config XML használata:
Az alábbi XML-t érti meg, mint konfigurációt:
```xml
<Vrh.Logger>
  <LogLevel>Warning</LogLevel>
  <EnableConsoleLogging>True</EnableConsoleLogging>
  <EnableDebuggerLogging>True</EnableDebuggerLogging>
  <EnableFileLogging LogDirectory="Log" LogFile="Vrh.Logger.TxtLog.log">True</EnableFileLogging>
  <UsedLogger>DefaultLogger</UsedLogger>
  <LoggerErrorLogDirectory>Log</LoggerErrorLogDirectory>
  <LoggerErrorLogFile>Vrh.Logger.Errors.log</LoggerErrorLogFile>
</Vrh.Logger>
```
Az XML-t alapesetben az alkalmazástér futási könyvtárában keresi LogConfig.xml néven.
Ha ettől el akarunk térni, akkor erre két lehetőségünk van:
1. Meghívjuk a felhasználás helyén, még az első Log hívás előtt, a Logger class LoadLoggerPlugin() metódusát, paraméternek pedig átadjuk a használandó XML fájlt specifikáló stringet.
2. Az alklamazás standard (App.config, Web.config) konfigurációjában definiálunk egy Vrh.Logger:Config kulcsú appSettings kulcsot és az értékének a használandó XML fájlt specifikáló stringet adjuk.
```xml
<appSettings>      
    <add key="Vrh.Logger:Config" value="LogConfig.xml/Vrh.Logger"/>
</appSettings>
```
Mindkét esetben a "használandó XML fájlt specifikáló string"-re az alábbi szabályok vonatkoznak:
* Ha a stringet @ karakterrel kezdjük, akkor ezzel azt jelezzük, hogy a fájl teljes elérési útját specifikáljuk. (*Pl.: "@D:\ApplicationDirectory\Config.XML"*)
* Ha a string nem @ karakterrel kezdődik, akkor a beírt fájlnév az alaklamazás környezet könyvtárában van, vagy az attól értelmezett megadott relatív útvonalon. (*Pl.: LogConfigDir\Config.xml*)   
* Az útvonalban a szokásos módon **\\** jellel választjuk el egymástól az elemeket. 
* A definició kijelölhet egy XML tag útvonalat, amely azt adja meg, hogy az XML-en belül milyen tag-okon át jutunk el addig a tagig, amely ténylegesen tartalmazza a konfigurációt definiáló xml tag-eket. Ezeket a fájl név után attól is, és egymástól is **/** jellel kell elválasztani. Pl.: Config.xml\ComponentConfiguration\LoggerConfiguration egy ilyesmi XML-t dolgoz fel:
```xml
<ApplicationConfiguration>
    <ComponentConfiguration>
        <LoggerConfiguration>
            <LogLevel>Warning</LogLevel>
            <EnableConsoleLogging>True</EnableConsoleLogging>
            <EnableDebuggerLogging>True</EnableDebuggerLogging>
            <EnableFileLogging LogDirectory="Log" LogFile="Vrh.Logger.TxtLog.log">True</EnableFileLogging>
            <UsedLogger>DefaultLogger</UsedLogger>
            <LoggerErrorLogDirectory>Log</LoggerErrorLogDirectory>
            <LoggerErrorLogFile>Vrh.Logger.Errors.log</LoggerErrorLogFile>
        </LoggerConfiguration>
    </ComponentConfiguration>
</ApplicationConfiguration>
```
* Megfigyelhetőek az alábbiak:
>* Az XML-en belüli megadásban a Root elemet nem definiáljuk, csak az azon belüli útvonalat. 
>* Ha nem adunk meg semmit akkor a beállítások tagjeit a root elem alatt keresi.
>* A root elem lenevezése tetszőleges lehet a configurációt tartalmazó XML-ben.

#### Szabvány app settings kulcsok használata:
Ekkor a szabványos config állomáynban szabványos appSettings kulcsok megadásával specifikáljuk a fenti négy beállítást:

```xml
    <appSettings>      
      <add key="Vrh.Logger:Config" value="@D:\Temp\_t\LogConfig.xml/Vrh.Logger"/>
      <add key="Vrh.Logger:LogLevel" value="debug"/>
      <add key="Vrh.Logger:EnableConsoleLogging" value="True"/>
      <add key="Vrh.Logger:EnableDebuggerLogging" value="True"/>
      <add key="Vrh.Logger:UsedLogger" value="DefaultLogger"/>
      <add key="Vrh.Logger:LoggerErrorLogDirectory" value="@D:\Temp\_t\Log"/>
      <add key="Vrh.Logger:LoggerErrorLogFile" value="Vrh.Logger.Errors.log"/>      
      <add key="Vrh.Logger:EnableFileLogging" value="True"/>
      <add key="Vrh.Logger:LogDirectory" value="Log"/>
      <add key="Vrh.Logger:LogFile" value="Vrh.Logger.TxtLog.log"/>
    </appSettings>
```
#### Mi mit jelent fentiekből?
A két beállítási mód közül az AppSettings kulcsok használata kap prioritást. Tehát amit megtalál az AppSettingsek közt, azt a beállítást innen kiolvassa, és nem számít, hogy az XML konfigurációban is van-e rá definíció, és mi az ottani értéke.
Ez így arra is lehetőséget nyújt, hogy bizonyos beállításokat megadjunk az XML konfiguráció szintjén, míg másokat csak az .config-ban definiáljunk.
A fenti beállítások konkrét értelmezése:
* **LogLevel**: A logolás szintje. A Vrh.Logger koncepciója szerint a logszint kezelése nem a Pluginok hatásköre. A beállításban megadott logszint, vagy az annál magasabbak kerülnek logolásra. Az alapértelmezett érték, ha a beállítás nincs jelen az Error. A használható értékek, magassági sorrendben, és a szándékolt jelentéssel:
>* **Debug**: Azon log bejegyzések, amelyek olyan részletes, a program belső működésével, állapotával kapcsolatos információkat jelentenek, melyek fejlesztői szintű információnak tekinthetőek, értelmezésükhöz a program belső ismerete vagy/és fejlesztői szaktudás szükséges.
>* **Verbose**: Részletes információk a program működéséről és az e-közbeni állapotokról, de tartalmuk a program belső ismerete nélkül is értelmezhető.
>* **Information**: Olyan részletességű információk, amelyek részleteiben tájékoztatnak a program működéséről, de csak a funkciók szempontjából, ezért az üzleti/funkcionális ismeretekkel rendelkezőknek is értelmezhető információkat tartalmaznak csak.
>* **Warning**: Olyan log üzenetek, amelyek a program működésével kapcsolatos figyelmeztetések, fontos információk. Általában valamilyen veszélyre, a működést legalábbis főfunkció szintjén nem befolyásoló hibára, inkonzisztenciára, konfigurációs ellentmondásra ilyesmire figyelmeztetnek. Jellemzően ez az a szint, amivel az üzemeltető rá jöhet, hogy mi az oka annak, hogy nem azt a működést tapasztalja, amit elvárna.
>* **Error**: A program működése során fellépő nem kritikus mértékű hibák.
>* **Fatal**: Olyan kritikus hiba, amely a program működésének leállását okozza, vagy egy főfunkció működésének ellátását akadályozza.
>* **None**: Ez nem egy valódi log szint. Segítségével teljesen kikapcsolható a logolás.
* **EnableConsoleLogging**: Segítségével azt lehet előírni a Vrh.Logger-nek, hogy a benne található DefultLogger implementációt is üzemeltesse és az alkalmazás Console-ra is jelenítse meg a log bejegyzéseket. Egy logikai érték. Az igaz (True) értéket a true, yes, 1 értékek jelentik (a kis és nagybetűk közt nem tesz különbséget). Minden ezektől eltérő érték hamis értéket (False) jelent. Az alapértelmezett érték a False, ha a beállítás nincs jelen.
* **EnableDebuggerLogging**: Segítségével azt lehet előítrni a Vrh.Logger-nek, hogy a benne található DefultLogger implementációt is üzemeltesse és az alkalmazáshoz csatlakoztatott Debugger outputján is jelenítse meg a log bejegyzéseket. Egy logikai érték. Az igaz (True) értéket a true, yes, 1 értékek jelentik (a kis és nagybetük közt nem tesz különbséget). Minden ezektől eltérő érték hamis értéket (False) jelenet. Az alapértelmezett érték a False, ha a beállítás nincs jelen.
* **EnablEFileLogging**: Segítségével azt lehet előítrni a Vrh.Logger-nek, hogy a benne található DefultLogger implementációt is üzemeltesse és egy txt log fájlba írja ki a log bejegyzéseket. Egy logikai érték. Az igaz (True) értéket a true, yes, 1 értékek jelentik (a kis és nagybetük közt nem tesz különbséget). Minden ezektől eltérő érték hamis értéket (False) jelenet. Az alapértelmezett érték a False, ha a beállítás nincs jelen.
* **LogDirectory**: Ha a txt log file írás engedélyezve van, akkor az itt megadott könyvtárat használja a Log file célkönyvtáraként. Ha a könyvtár nem létezik, akkor létrehozza. Ha nincs megadva, akkor az alkalmazás futási könyvtárában keletkezik a fájl.
* **LogFile**: Segítségével lehet definiálni, hogy a fent leírt text log fájlt milyen néven hozza létre a Vrh.Logger. Ha nincs megadva, akkor a fájl Vrh.Logger.TxtLog.log néven jön létre.
* **UsedLogger**: Ha a Vrh.Logger működési környezetében több Logger plugin is található. Akkor ennek segítségével írhatjuk elő, hogy ezek közül melyiket használja  a konfiguráció logolásra. Ha ez a beállítás nincs jelen, akkor mindig azt a plugint használja maleiket először megtalál a működési környezetben. Így egyetlen plugin jelenléte esetén a beállítást nem szükséges megadni. Az értéknek  a használandó plugin pontos osztály nevét, vagy teljes (névterekkel együtt) osztálynevét kell megadni, ezt mindig az adott Plugin dokumentációjából derül ki.
* **LoggerErrorLogDirectory**: Ha a Vrh.Logger működésében valami hiba lép fel, amely meggátolja a logolást, akkor egy saját text fájlba írja a hiba tényét, és az üzenet segítségével a hiba oka kideríthető. Magasabb logolási szinteken bejegyzéseket készít egyéb információkról is ide, a Logger indulásáról, leállásáról, a használt plugin betöltéséről. Ez a beállítás mondja, meg, hogy melyik könyvtárba kerüljön ez a txt Log. Ha nincs megadva, akkor az alkalmazás futási könyvtárában keletkezik a fájl. 
* **LoggerErrorLogFile**: Segítségével lehet definiálni, hogy a fent leírt saját hiba text fájlt milyen néven hozza létre a Vrh.Logger. Ha nincs megadva, akkor a fájl Vrh.Logger.Errors.log néven jön létre.
## Információk Plugin fejlesztéshez
Minden Vrh.Logger egyetlen Log\<T> metódust valósít meg az Vrh.Logger.Ilogger intefész implementációjával. Az interfész használathoz a **Vrh.Logger névtér** using-olandó. Az ILogger interfész teljes definíciója:
```csharp
    /// <summary>
    /// Logger interfész, amelyet a Vrh.Logger használ
    ///  A konkrét logger megoldásnak, vagy annak  a wraperének ezt kell implementálnia.
    ///  Az implementzáló típust meg kell jelőlni Export attribútummal!
    /// </summary>
    public interface ILogger : IDisposable
    {
        /// <summary>
        /// Azt fogja hívni a Vrh.Logger a logok átadásához.
        ///     A koncepció szerint a LogLevel-t a Vrh.Logger kezeli a konkrét logger plugin felett!
        /// </summary>
        /// <typeparam name="T">Típuskijelölő</typeparam>
        /// <param name="noOfLogEntry">Log sorszáma a Logger létrejötte óta az adott alkalmazástérben</param>
        /// <param name="data">A kijelölt típusú adat</param>
        /// <param name="dataFields">Kulcs érték párok listájaként átadott adatok</param>
        /// <param name="exception">Egy exceptiont lehet ide bedobni, hogy a Logger az alapján logolja az exception fontos részleteit</param>
        /// <param name="level">A log szintje ennek a bejegyzésnek</param>
        /// <param name="sourceModul">Forrásmodul a Logger static Log hívásnak átadott típust deklaráló assembly neve kerül bele</param>
        /// <param name="sourceModulVersion">Forrásmodul verziója. Az assemblyben definiált AssemblyInformationalVersion, annak hiányában a definiált AssemblyVersion</param>
        /// <param name="sourceClass">Az osztály teljes neve, ahonnan a Log bejegyzést bedobták</param>
        /// <param name="sourceMethode">A metódus, neve, ahonnan a log bejegyzés érkezett (4.5 Framework-tól automatikus, az alatt  a hívási helyen kell gondoskodni az átadásáról)</param>
        /// <param name="line">A forrás sor száma, ahonnan a Log hívás származik (4.5 Framework felett automatikus, az alatt a hívási helyen kell gondoskodni az átadásáról)</param>
        /// <param name="callTimeStamp">A hívás időbéjege</param>
        void Log<T>(UInt64 noOfLogEntry, T data, IDictionary<string, string> dataFields, Exception exception, LogLevel level, string sourceModul, string sourceModulVersion, string sourceClass, string sourceMethode, int line, DateTime callTimeStamp);

        /// <summary>
        /// Esemény,a melyen keresztül a plugin visszajelzést tud küldeni a működésében bekövetkező kritikus hibákról.
        ///     Minden esetben Dispose hívást, és recreatet eredményez a Pluginra nézve a Vrh.Logger keret szintjén!!!     
        /// </summary>
        event FatalErrorInPluginEventHandler FatalErrorInPlugin;
```
Az alábbi szabályok és elvek elvárások a Pluginnal szemben:
* Implementáció:
>* A Plugin megvalósítás implementálja az ILoggert, amely egyetlen Log<T> metódus megvalósítását jelenti.
>* De az ILogger egy IDispose leszármazott, ezért a plugin disposolható. És kötelezően Dispose mintát implementál. 
>* A Pluginnak kötelessége megvalósítani tényleges teljes erőforrás felszabadítást a Dispose ágon. A Vrh.Logger keret bizonyos esetekben a publikus Dispose metodusát hívja. Ekkor minden a logoláshoz használt erőforrást el kell engednie, és az objektumnak meg kell semmisülnie a Dispose visszatérésére. Tehát maradéktalanul működnie kell egy ilyen teszt kódnak rá (ahol iterationCount egy tetszőleges számú ismétlést definiál):
>```csharp
>    for (int i = 0; i < iterationCount; i++)
>    {
>       ILogger logger = new MyLoggerPlugin();
>       logger.Log<string>((ulong)i, "test", null, null, LogLevel.Debug, "TEST", "V1.0.0", "TestClass", "TestMethode", 1, DateTime.Now);
>    }
>```
* Hibavédettség: 
>* A plugin implementációnak le kell kezelnie, hogy a Log<T> hívás során bármilyen paraméter értéket is kapjon bármelyik paraméterben, beleértve a null értékeket is a nullozható típusok esetén. Megengedett, hogy a Logolás ne sikerüljön (ne keletkezzen log bejegyzés), de hogy a Plugin működése leálljon az nem.
>* Fenti paraméterek közül az alábbiak Null értéktől különböző kitöltöttségét mindig biztosítja a Vrh.Logger keret a nullozható típusok esetén is:
>>* dataFields (üres Dictonary formájában)  
>>* sourceModul
>>* sourceModulVersion
>>* sourceClass
* További részletek:
>* Mivel a Vrh.Logger keret a Plugint a MEF keretrendszerrel tölti be, ezért meg kell jelölni Export attribútummal, a plugin Vrh.Logger.ILogger interfészt implementáló típusát, mégpedig az Ilogger contract típussal:
>```csharp
>    [Export(typeof(ILogger))]
>    public class DefaultLogger : ILogger
>    {
>       ...
>```
>* Az Export attribútum a **System.ComponentModel.Composition** névtérben található.
>* Mivel a Vrh.Logger keret a plugint egy egyszerű MEF Import-ként injektálja, így minden működéséhez szükséges inicializációt el kell végezni az alapértelmezett (paraméter nélküli) konstruktorában. Sem konstruktor paraméter átadására, sem inicializáló függvény meghívására nincs lehetőség az automatikus Plugin töltés folyamatában.
>* Mivel a Vrh Logger keret a Plugin Log<T> implementációját aszinkron, egy külön háttérszálra dobva hívja (és nem vár a visszatérésére), ezért ez további követelményeket és sajátosságokat jelent:
>>* A Log\<T> metódusnak, illetve a Plugin ennek a hívásnak a kezelésével összefüggő működésének szálvédettnek kell lennie! Hiszen a Logger keret változó számú, párhuzamos konkurens hívást fog intézni a plugin felé a Log\<T> híváson keresztül.
>>* Ennek megfelelően a Log\<T> hívással semmilyen mód nem biztosított futási eredmények, vagy hibák visszadobására a logolásból. A Le nem kezelt kivételek annyit eredményeznek, hogy a végrehajtására indított tread nem normál státusszal terminál. Ennek ellenére kívánatos a Plugin Log metódusra hibavédelmet implementálni, és a logolásban fellépő kritikus hibák okáról a lentebb leírt módon értesíteni a Vrh.Logger komponenst.
>* Amennyiben a Plugin működését akadályozandó hiba áll fenn, vagy ilyen keletkezik, a Plugin felelőssége, hogy erről értesítse a Vrh.Logger plugint. Ennek a módja, a Vrh.Logger.ILogger interfészben definiált **FatalErrorInPlugin** esemény implementálása:
>```csharp
>    /// <summary>
>    /// Esemény,a melyen keresztül a plugin visszajelzést tud küldeni a működésében bekövetkező kritikus hibákról.
>    ///     Minden esetben Dispose hívást, és recreatet eredményez a Pluginra nézve a Vrh.Logger keret szintjén!!!     
>    /// </summary>
>    event FatalErrorInPluginEventHandler FatalErrorInPlugin;
>```
>* Az eseményt a **FatalErrorInPluginEventHandler** delegate-nek megfelelő signaturával iratkozik fel a Vrh.Logger keret, amely teljes definíciója az alábbi:
>```csharp  
>    /// <summary>
>    /// Delegeta definició, a FatalErrorInPlugin event kezeléséhez
>    /// </summary>
>    /// <param name="pluginType">A plugin típusa, mindig adjuk át, hogy a Vrh.Logger keret releváns információt menthessen a hiba fellépésének helyéről</param>
>    /// <param name="e">Az esemény argumentumai, egy szöveges üzenetet, és egy tetszőleges szabványos Exception átadását támogatja</param>
>    public delegate void FatalErrorInPluginEventHandler(Type pluginType, PluginFatalErrorEventArgs e);
>```
>* A használt **PluginFatalErrorEventArgs** typus teljes definiciója az alábbi:
>```csharp
    /// <summary>
    /// EventArgs a FatalErrorInPlugin eseméynhez
    /// </summary>
    public class PluginFatalErrorEventArgs : EventArgs
    {
        /// <summary>
        /// Szöveges információ a fellépett hibáról 
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Itt adhatjuk át a feléppő, vagy a kiváltott Exception-t
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// Jelzi a logger keretnek, hogy indítsa újra a plugint 
        /// </summary>
        public bool RestartMe { get; set; }
    } >```
>* Fentiek alapján a Pluginban az alábbi minta alapján kell helyesen implementálni az esemény visszajelzést:
>```csharp
>        /// <summary>
>        /// FatalErrorInPlugin esemény (ILogger interfész member)
>        /// </summary>
>        public event FatalErrorInPluginEventHandler FatalErrorInPlugin;
>
>        /// <summary>
>        /// Metódus a FatalErrorInPlugin esemény elsütésére
>        /// </summary>
>        /// <param name="exception">Menteni kívánt kivétel</param>
>        /// <param name="message">Szabadon beállítható üzenet a hibával kapcsolatban</param>
>        private void OnFatalErrorInPlugin(Exception exception = null, string message = "")
>        {
>            var ea = new PluginFatalErrorEventArgs()
>            {
>                Message = message,
>                Exception = exception,
>	   RestartMe = true,
>            };
>            FatalErrorInPlugin?.Invoke(this.GetType(), ea);
>        }
>```
>* A hiba esemény kiváltásához egyszerűen az OnFatalErrorInPlugin metódust hívjuk a plugin implementációjában azon a helyen, ahonnan a hiba fellépéséről értesítést akarunk küldeni a keretnek. 
>* Cészerű kihasználni, hogy  konkrét Exception típust tudunk átadni. Az emellé mellékelt szöveges üzenet inkább egy opcionális lehetőség.
>* Az esemény kritikus hibák jelzésére szolgál. A Vrh.Logger keret a saját logjába mindig errorként menti az ezen át érkezett információkat. Ne használjuk a plugin működésével kapcsolatos ionformációk átadására, csak ténylegesen olyan súlyos hibák jelzésére, amely megakadályozzák az adott Logger plugin helyes működését. A RestartMe tag segítségével utasíthatjuk a keretet, hogy töltse be újra a Plugint. Célszerű olyan implementáció megvalósítása, amely biztosítja, hogy a Log\<T> hívások csak a hiba fellépésekor jelentsenek Error-t, és ne minden egyes Log\<T> híváskor, ezzel biztosítani lehet, hogy a Logolás infrastrukturális ellehetetlenítése ne legyen befolyással a program eredeti működésére.
* Logszintek:
>* A koncepció szerint a logolási szintek kezelése nem a Plugin, hanem a Vrh.Logger keret hatásköre. Ezért a Vrh.Logger keret a saját beállításának megfelelően, nem is intéz Log\<T> hívást a plugin felé, ha a logolandó információ nem éri le az aktuálisan beállított logolási szintet. Ennek ellenére a Pluginban szükség lehet a Logolási szint szerepeltetésére, például, hogy mentsük a log bejegyzéssel ezt információt. Lehetőleg ne végezzünk szemantikai tranzformációt a logszintekkel kapcsolatban, és törekedjünk rá, hogy konkrétan a Vrh.Logger által definiált LogLevel enumeratort használjuk. Az itt definiált None érték nem egy valós logszint, a logolás teljes kikapcsolására használja a keret. Ennek megfelelően, ilyen log szint megjelöléssel sose fog a Vrh.Logger keret hívást intézni a Plugin felé. 
>* A logszintek teljes definíciója az alábbi a LogLevel enumban:
>```csharp
>    /// <summary>
>    /// A logolás lehetséges szintjei
>    /// </summary>
>    public enum LogLevel
>    {
>        /// <summary>
>        /// Debug részletességet célzó logbejegyzések a működés részletes követésére, hibakeresésre
>        /// </summary>
>        Debug = 1,
>        /// <summary>
>        /// Részletes, bőbeszédű információk szintje
>        /// </summary>
>        Verbose = 2,
>        /// <summary>
>        /// Információk a rendszer működéséről
>        /// </summary>
>        Information = 3,
>        /// <summary>
>        /// Figyelmeztetések szintje
>        /// </summary>
>        Warning = 4,
>        /// <summary>
>        /// A hibák szintje
>        /// </summary>
>        Error = 5,
>        /// <summary>
>        /// A kritikus hibák szintje
>        /// </summary>
>        Fatal = 6,
>        /// <summary>
>        /// None segítségével kikapcsolható a teljes logolás, sose használjuk logbejegyzés szintjeként, mert a VRH.Logger az ilyen bejegyzéseket mindig elutasítja!
>        /// </summary>
>        None = 7,
>    }
>```

<hr></hr>

# Version History:
## 2.0.3 (2019.11.06)
### Patches:
1. Webes környezetben való működésképtelenség (path felvételek miatt) hibamentessé tétele
2. None a defult loglevel, ha config error van
3. Nem ír Fatal logot sem, ha none a loglevel
## v2.0.2 (2019.11.04)
### Patches:
1. Load error információk megjelenésének javítása (Error log txt-ben)
## v2.0.1 (2019.10.25)
### Patches:
1. A nuget csomag telepítéskor elkészíti azt a minimális konfigurációt, ami a telepítés helyén való instant működőképességhez szükséges. (**Minden Nuget csomagnak ez lenne a célja a belézárt komponenst illetően: A telepítés teljes automatizálása, ami azonnali használatbavételt tesz lehetővé a telepítés helyén a komponens mélyebb ismerete (pl. konfigurálás módja) nélkül!**)
2. Főverzió emeléskor az Obsolete-nek jelölt elemeket ki kell dobni a kódból, ezt most pótoltam
3. Instanmt kipróbálhatóság futatahatóság a repoba felrakott állapotban (konfig fájlok tartalma, stb.)

## v1.5.7 (2019.06.04)
### Patch: 
1. Windows Service-ben a Log bejegyzések előállítása túl időköltséges volt (már a task oldalon) a PID lekérdezések miatt. Állandó lekérdezés helyett áttéve egy Lazy fieldbe.
## v1.5.6 (2019.05.27)
### Patch: 
1. DefualtLogger Txt log implementáció: Logbejegyzések kiegészítése TimeStamp-pel
2. DefualtLogger Txt log implementáció: A létrehozott logfájl nevébe bekerül prefxként a dátumbélyeg (YYYYMMDD_), így naponta új txt fájl keletkezik, és nem lesznek használhatatlanul nagyok a log fájlok.
## v1.5.5 (2019.05.23)
### Patch:
1. compatible_Lear branch!!! Nugetek Learhez igazítása
## v1.5.4 (2019.05.21)
### Patch: 
1. To upgrade all Nugets to latest
## V1.5.2 (2018.12.11)
### Patch:
1. Service név lekérdezésében hiba javítása
## V1.5.1 (2018.12.11)
### Patch:
1. CallSignature osztály láthatósága .public
## V1.5.0 (2018.12.11)
### Compatibility API changes:
1. CallSignature osztály bevezetése.
## V1.4.0 (2017.12.06)
### Compatibility API changes:
1. Logger class funkciója áttéve a VrhLogger-class alá. Ott csak annak a public (API) funkcionalitása maradt, oly módon, hogy áthív a VrhLogger-be. (Hogy ne legyen incompatibility a change.)
2. A Logger class Obsolote-tal jelölve --> Depcreated!
### Patches:
1. LogHelper.ExtractPropertyValuesToDataFields null védelme
## V1.3.0 (2017.12.05)
### Compatibility API changes:
1. LogHelper kiegészítése az ExtractPropertyValuesToDataFields metódussal
## V1.2.0 (2017.08.10)
### Compatibility API changes:
1. DefaultLogger kiegészítése TXT log file írási kéápességgel
2. Lehetséges app settings kulcsok kezelésének beépítése a default plugin txt file logolás képesség konfigurációjához (Kulcsok: "Vrh.Logger:EnableFileLogging", "Vrh.Logger:LogDirectory" "Vrh.Logger:LogFile")
3. LogConfig.xml (config xml) feldolgozó kiegészítése a default plugin txt file logolás képesség kapcsán szükésge súj beállítások kezelésével (Tag: EnableFileLogging (Extended boolean: true|yes|1/false|no|0) lehetséges attríbutumai: LogDirectory="Log" LogFile="Vrh.Logger.TxtLog.log)
### Patches:
1. LoggerErrorLogFile beállítást valóban figyelembe vegye a belső error log file elnevezésében
2. A Default logger valóban figyelembe veszi a három leheteséges log target-re engedélyezés van-e konfigurálva, és csak arra ír, amelyikre igen
## V1.1.0 (2017.03.21)      
### Compatibility API changes:
1. LogHelper.GetExceptionData bevezetése
### Patches:
1. Newtonsoft.Json nuget dependency növelése 10.0.1-re
2. LogHelper.GetExceptionInfo kiegészítése az Exception.Data tartalmának kibontásával 
## V1.0.1 (2017.03.21)
### Patches:
1. Új Vrh.LinqXmlProcesser.Base beépítés
2. Nuspec fájl javításaok
## V1.0.0 (2017.02.21)
Initial version
