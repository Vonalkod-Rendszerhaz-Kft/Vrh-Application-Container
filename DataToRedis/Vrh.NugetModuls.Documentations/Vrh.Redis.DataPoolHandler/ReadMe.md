# Vrh.Redis.DataPoolHandler
Ez a leírás a komponens **v0.0.0** kiadásáig bezáróan naprakész.
Igényelt minimális framework verzió: **4.5**
Teljes funkcionalítás és hatékonyság kihasználásához szükséges legalacsonyabb framework verzió: **4.5**
## Redis Vrh.DatatPool koncepció

# TODO: Megírandó a komponens dokumentációja!!!
  

<hr></hr>

# Version History:
## 1.7.4
### Patches:
1. JSON szerializáció esetén is int az egészek típusa (mint XML szérializációnál) és nem long
## 1.7.3
### Patches:
1. Szerializáció ASETimeCounter típust tartalmazó OneData-ták deserializációja helyesen megtörténik JSON serilaizáció esetén is
## 1.7.2
### Patches:
1. Szerializáció szükségeségének felismerése kiegészítve a JSON verzióval
## 1.7.1
### Patches:
1. Nem szándékolt breaking change-ek felszámolása az 1.7.0-a verzión
## 1.7.0
### Compatibility API changes:
1. JSON serializáció bevezetése. A komponens mostantól képes JSON serializációt is használni az XML helyett.         
### Patches:
1. Annak a hibának az elfedése, hogy a System.Xml.Serialization.XmlSerializer Serialize oldalon string forrásből XML enkódolja az XML-ben tiltott karaktereket, majd a saját deserialize oldala exceptiont dob ezekre (érvénytelen karakter). A javítás annyi, hogy az eredeti problémás karaktereket ASCII nevekre váltja, és ezt string forrást deserializálja. Tehát megváltoztatja a string tartalmakat!
## 1.6.0
### Compatibility API changes:
1. Generikus WriteKeyValue fügvény az értéket ezentúl mindik OneData típusra konvertálja, és azt menti a Redis kulcsba
### Patches:
1.: Hibavédelem a ChangeManager.ProcessValue() és a ChangeManager.ProcessSchema() metódusokba, hogy ne termináljon  avégrehajtó száljuk, ha a futás közben hiba lép fel a redis elérésben.
## V1.5.2 (2017.11.23)
### Patches:
1. Hibavédelem a ChangeManager.ProcessValue() és a ChangeManager.ProcessSchema() metódusokba, hogy ne termináljon  a végrehajtó száljuk, ha a futás közben hiba lép fel a redis elérésben.
## V1.5.1 (2017.11.17)
### Patches:
1. Annak ahibának ajavítása, hogy a change manager tekeri a procit, ha van beállított csillapítás
## V1.5.0 (2017.10.16)
### Compatibility API change:
1. InstanceReader: DataChangeEventDebouncing propertivel beállíthatóü, hogy mennyi legyen a csillapítása a DataChange Event elsütésének. (Millisecundum, ennyit vár, mielőtt eventet küld.) 
### Patches:
1. Update StackExchange.Redis kliens: v1.2.3 --> v1.2.6
2. Az egyik writekey overrideban elmaradt érték ellenőrzés hozzáadása (csak akkor küldjön change eventet, ha ténylegesen megváltozott  a tárolt érték)
3. Nuget csomaggá alakítás## V1.4.2 (2017.06.22)
## V1.4.2 (2017.06.22)
### Patches:
1. A Redis műveletekhez védelem beépítése a véletlen fellépő egyszeri Redis elérési hibák kivédéséhez: Retry pattern védi a műveleteket, ha exception jön róluk, akkor N szer újrapróbálkozik egy 10*N! milliszekundumos késleltetés függvénnyel. Az N értéke jelenleg 9.
2. StackExchange.Redis refresh to v1.2.3
## V1.4.1 (2017.04.25)
### Patches:
1. StackExchange.Redis refresh to v1.2.1
## V1.4.0 (2016.11.21)
### Compatibility API changes:
1. Olyan string mezőkre működő eltávolítás megvalósítása, amely garantáltan biztonságosan eltávolít egy részt  a stringmezőből párhuzamos konkurens írások esetén is. Az InstanceWriter api-ja kapott egy új SafeStringRemove hívást:
```java
        /// <summary>
        /// Thread Safe string remover
        ///     
        /// </summary>
        /// <param name="keyName">Key name (key type must be string!)</param>
        /// <param name="valueToRemove">remove this</param>
        /// <param name="listSeparator">use this as separator (empty is not alowed)</param>
        public void SafeStringRemove(string keyName, string valueToRemove, string listSeparator = ",")
```
## V1.3.0 (2016.11.17)
### Compatibility API changes:
1. Olyan string mezőkre működő összefűzés megvalósítása, amely garantáltan biztonságos párhuzamos konkurens írások esetén is. Az InstanceWriter api-ja kapott egy új SafeStringValueConcatenate hívást:
```java
        /// <summary>
        /// Thread Safe string concatenator
        ///     
        /// </summary>
        /// <param name="keyName">Key name (key type must be string!)</param>
        /// <param name="valueToConcatenate">concattenate this</param>
        /// <param name="listSeparator">use this as separator (empty is alowed)</param>
        public void SafeStringValueConcatenate(string keyName, string valueToConcatenate, string listSeparator = ",")
```
## V1.2.0 (2016.08.23)
### Compatibility API changes:
1. A PoolHandler szolgáltatás objektum bővítése a public Dictionary<string, DataType> GetRegisteredKeys() hívással, ami visszaadja az adott Pool alá regisztrált kulcsok listáját, azok metaadataival.
## V1.1.0 (2016.08.19)
### Compatibility API changes:
1. Új szolgáltatás objektum bevezetése: Pools. Egyetlen szolgáltatása a public List<string> GetExistPools() hívás, amely a regisztrált poolok listáját adja vissza.
2. A PoolHandler szolgáltatás objektum bővítése a public List<string> GetExistPoolInstance() hívással, ami visszaadja az adott Pool alá tartozó instance-ok listáját.
## V1.0.9 (2016.08.16)
### Patches:
1. Annak a hibának a javítása hogyha null értéket tartalmazó ASETimeCounter típusú kulcsot írunk felül, akkor jön egy NullReferenceException és nem megy ki a Change event rá (a kulcs értéke beállítódott)
## V1.0.8 (2016.08.08)
### Patches:
1. Annak a hibának a javítása hogy a PoolHandlernél nincs feliratkozás és ezért a pub/sub csatornákra küldött üzeneteknél üres lehetett a csatorna referencia, ezért exceptiont dobott.
## V1.0.7 (2016.07.21)
### Patches:
1. pub/sub hibajavítása. (Dispose leiratkozások a teljes pub/sub channelről leiratkoztak, nem csak a saját nevükben, így egy alkalmazástérben a teljes ConnectionMultiplexerre megszüntette a PUB/SUB csatorna feliratkozásokat.)
## V1.0.6 (2016.07.19)
### Patches:
1. ChangeManager ProcessValue-ban a belső while, ami a changeken megy végig is figyeli már a dispose-t, így megszüntek a dispose közben tapasztalható nullreference exception-ök
2. PoolContext: időbélyeggel nevezi el a Redis klienst, a jobb debugolhatósághoz
3. PoolContext: _redis (ConnectionMultiplexer) Lazy-vé és static-ká alakítva
4. Redis pub/sub kezeléshez a RedisChanel referenciák letárolva egy privát fieldbe, hogy felhasználjuk őket a dipose ágon a leiratkozáshoz. Dispose ágba bekerült a leiratkozás. Így megszűnt a komponens jelentős memóriaszivárgása.
5. RedisConnectionStore static class hozzáadva, a CoinnectionMultiplexer újrahasznosítás kényszerítéséhez, és tetszőleges számú Redis szerver eléréséhez.
## V1.0.5 (2016.06.20)
### Patches:
1. ConnectionMultiplexer zárása a PoolContext objektum Dispose logikájában.
2. ChangeManager Dispose ágán a thread Abortok helyett normál szálkifttatás Dispose a logikában. (Ennek megfelelően átírva a worker methodusok is.)## V1.0.4 (2016.05.24)
### Patches
1. Annak ahibának a javítása, hogy az ASETimeCounter típus StartedTimeStamp-je nem volt serializálva, így a Redisből kiolvasott counter nem hordozta ezt az értéket, így a Value értéke hibás adatot adott.

