# Vrh.Redis.ConnectionStore
Ez a leírás a komponens **v1.0.0** kiadásáig bezáróan naprakész.
Igényelt minimális framework verzió: **4.5**
Teljes funkcionalitás és hatékonyság kihasználásához szükséges legalacsonyabb framework verzió: **4.5**

##### A komponens arra szolgál, hogy egységes, egyszerű, de az előírt újrahasznosítási mintának megfelelő megvalósítása legyen a Redis szerverek felé való kapcsolatokkal való gazdálkodásnak (ConnectionMultiplexer), a StackExchange.Redis kliensen keresztül.

## Használata
A használathoz a **Vrh.Redis.ConnectionStore** névtér using-olandó. Egyetlen **RedisConnectionStore** nevű static class érhető el, melynek egyetlen GetConnection publikus metódusa van, az alábbi szignatúrával:

```csharp
public static ConnectionMultiplexer GetConnection(string alias, string connectionString = null)
```
Ahol:
* **alias**: A kapcsolat azonosítására használt egyedi név, megadása kötelező!
* **connectionString**: Csak akkor kell megadni, ha az alkalmazásban nem kívánjuk használni a Vrh.Redis.ConnectionStore komponens konfigurációs megközelítését (pl. mert más megoldást használunk helyette). Ekkor a connectionstring paraméter adja meg a StackExchange.Redis connectionstringet. De ekkor is csak az első hívásban kötelező megadni (további szerepeltetése opcionális). Amennyiben a Vrh.Redis.ConnectionStore komponens konfigurációs megközelítésével írjuk le az elérhető Redis kapcsolat(ok)at, akkor mindig hagyjuk kitöltetlenül ezt a paramétert.

Minden alkalommal, amikor egy alkalmazásban egy Redis felé irányuló műveletre van szükség, akkor a Redis kapcsolati objektumot (StackExchange.Redis.ConnectionMultiplexer) Ezen keresztül kell megszerezni!

A megoldás megközelítési módja alias alapú (az ADO.Net SQL connectionstringjeihez hasonló). Tehát egy adott alias (név) egy adott kapcsolatot azonosít.
## Konfiguráció mentes használat
Az előző pontban leírtak szerint ekkor a felhasználási kódban kell gondoskodni a **GetConnection** hívások connectionString paraméterének kitöltéséről.
Egyes, a komponens működésére vonatkozó paraméterek megadása ekkor is a saját konfigurációs megoldásban történik, a következő fejezetben leírtak szerint. De a Redis szerverek kapcsolati konfigurációjának (StackExchange.Redis connectionstringek) leírása ekkor nem itt történik.
## Vrh.Redis.ConnectionStore komponens beépített konfigurációs szolgáltatásai
A komponens szolgáltatásokat nyújt arra vonatkozóan, hogy a Redis kapcsolatokat leíró konfigurációt, mint külső szabadon változtatható beállítást megadjuk Redis kapcsolatokat felhasználó alkalmazáshoz.
Emellett itt történhet bizonyos a komponens működését megadó beállítások megadása is.
### Appliaction.config
A komponens minden beállítása megadható az alkalmazás szabványos config fájljában is. Azt az **appSettings** szekcióban kell megtenni, új beállítás kulcsok hozzáadásával (add tagek).
A használható kulcsok **Vrh.Redis.ConnectionStore** prefixet kapnak. A kulcs egyedi nevét ettől kettősponttal (:) elválasztva kell megadni. A használható kulcsok köre, és magyarázatuk:
#### Vrh.Redis.ConnectionStore:Configuration
```xml
<appSettings>    
    <add key="Vrh.Redis.ConnectionStore:Configuration" value="Vrh.Redis.ConnectionStore.Config.xml/RedisConnectionStore"/>
</appSettings>
```
Segítségével definiálhatjuk, hogy a komponens, melyik XML fájlban keresse a beállításait. Ha abszolút útvonalat kívánnunk megadni, akkor használjuk a @ prefixet, egyébként az alkalmazás munkakönyvtárához képest relatív útvonalnak tekinti a beírtakat. A definícióban lehetőség van az XML-en belüli útvonal definiálására is, ha a beállításokat nem közvetlenül a megadott fájl Root eleme tartalmazza. A könyvtárak/fájl esetén vissza-perjelelet (\\), az XML útvonalban perjelet (/) kell alkalmazni. A könyvtár/fájl útvonalat és az XML útvonalat egymástól szintén a perjel (/) választja el. 

Amennyiben a beállítás kulcsot elhagyjuk, akkor az a Vrh.Redis.ConnectionStore.Config.xml használatával egyenértékű. 

Példák:
* **Vrh.Redis.ConnectionStore.Config.xml**: Az alkalmazás munkakönyvtára tartalmaz egy Vrh.Redis.ConnectionStore.Config.xml fájlt, amelyben a beállítások közvetlen a root alatt találhatóak (hívják a root-ot akárminek is).
* **Settings\Vrh.Redis.ConnectionStore.Config.xml/RedisConnectionStore**: Az alkalmazás munkakönyvtára tartalmaz egy  Vrh.Redis.ConnectionStore.Config.xml fájlt, emelylben a beállítások a RedisConnectionStore nevű root elem alatt találhatóak. (Mikor a konfigurációs XML fregmentum a root alatt található, akkor annak útvonalként való kiírása, vagy elhagyása opcionális, de ha megadjuk, akkor valóban azon a néven kell szerepelnie a root elemnek.)
    * **@C:\MyApplication\Settings\settings.xml/ApplicationSettings/RedisConnectionSettings**: A beállításokat egy a C:\MyApplication\Settings könyvtár alatt található, settings.xml fájl tartalmazza, a fájlon belül az ApplicationSettings nevű root elem alatt egy RedisConnectionSettings XML tag található, és az tartalmazza a komponens beállításait.

Amennyiben a megadott fájl vagy az azon belüli útvonal nem érvényes definíció, akkor a Vrh.LinqXMLProcessor.Base beállításának megfelelően vagy kivételeket dob, vagy alapértelmezéseket ad a komponens.
#### Vrh.Redis.ConnectionStore:ThrowExceptions
```xml
<appSettings>    
    <add key="Vrh.Redis.ConnectionStore:ThrowExceptions" value="true"/>
</appSettings>
```
Azt lehet a segítségével előírni, hogy a komponens dobjon-e exception-öket a fatális hibák esetén (melyek azt eredményezik, hogy a GetConnection hívás nem adhat vissza érvényes ConnectionMultiplexert), vagy csak egyszerűen ilyenkor null legyen a GetConnection visszatérési értéke. Az értéke true (exception-öket dob), vagy false lehet (hibatűrő).
Amennyiben az értékét definiáljuk az Application.config kulcsban, úgy az itt definiált érték jut érvényre függetlenül attól, hogy használunk-e konfigurációs XML-t, és ott megadjuk-e ennek a beállításnak az értékét.
#### Redis Connection-ök
Amennyiben az Application.config appsettings szekciójában kívánjuk felsorolni a definiált Redis Connection-jeinket, akkor ezt úgy kell megtenni, hogy a Vrh.Redis.ConnectionStore modul prefix után (a kettőspont (:) szeparátort használva) megadjuk az alias-t, és a value tartalmazza magát a redis connectionstrringet.
```xml
<appSettings>
    <add key="Vrh.Redis.ConnectionStore:cache" value="localhost:6753"/>
    <add key="Vrh.Redis.ConnectionStore:pubsub" value="localhost:4444"/>
</appSettings>
```
Tetszőleges connection definiálható, de a használt alias neveknek egyedinek kell lenniük.

Amennyiben az application config fájlban megadunk alias definíciókat, akkor az itt megadottak jutnak érvényre, akkor is, ha XML configot is használunk!!!

**Minden Application.config settings kulcs az alkalmazás indulásakor töltődik be, így az itt végrehajtott változtatások nem jutnak menet közben (az alkalmazás futása közben) érvényre!**
### A komponens XML konfigurációja
Lehetőség van a fent tárgyalt beállítások XML configban való megadására.
A releváns XML konfiguráció az alábbi:
```xml
<RedisConnections ThrowExceptions="true">
  <Connection Name="cache" ConnectionString="localhost"/>
  <Connection Name="pubsub" ConnectionString="localhost"/>
</RedisConnections>
```
A fenti XML fregmentum bármilyen XML-ben elhelyezhető, amelynek legalább egy root tagje van, csupán az XML helyes definíciójáról kell gondoskodni a Vrh.Redis.ConnectionStore:Configuration appsettings kulcs alatt. A RedisConnections elem nem lehet egyben root tag is a helyes működéshez! A Vrh.Redis.ConnectionStore:Configuration appsettings kulcs alapértelmezése (elhagyása esetén jelen lévő értéke) az alábbi XML-t feltételezi:
```xml
<?xml version="1.0" encoding="utf-8" ?>
<RedisConnectionStore>
  <RedisConnections ThrowExceptions="true">
    <Connection Name="cache" ConnectionString="localhost"/>
    <Connection Name="pubsub" ConnectionString="localhost"/>
  </RedisConnections>
</RedisConnectionStore>
```
(Az xml metatag megléte a komponens oldaláról nem követelmény, de ez esetben a fájl kódolása csak utf-8 lehet!)

A fenti konfiguráció részletes magyarázata:
#### RedisConnections element
Alatt sorolódnak fel a konfigurációban definiált Redis connection-ök.
#### RedisConnections element ThrowExceptions attribútum:
Definiálja, a komponens hibatűrő viselkedését a Vrh.Redis.ConnectionStore:ThrowExceptions appsettings kulcsnál tárgyaltak szerint.
#### Connection elementek
A RedisConnections tag alatt elhelyezkedő Connection lementek definiálnak egy-egy redis connectiont.
Az alias-t (connection neve) a **Name attribútum** adja meg, míg maga a hozzá tartozó Redis kapcsolat connection stringje a **Connectionstring attribútum** segítségével definiálható (StackExchange.Redis szerinti connectionstringek hazsnálhatóak.).
Tetszőleges számú Connection tag definiálható, az aliasoknak kell egyedinek lenniük.
<hr></hr>

# Version History:
## v1.0.0 (2018.03.06)
### Initial version
