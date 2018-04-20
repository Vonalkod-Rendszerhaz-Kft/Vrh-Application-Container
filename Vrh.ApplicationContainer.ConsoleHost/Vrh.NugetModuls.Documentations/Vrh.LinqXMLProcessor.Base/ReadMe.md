# Vrh.LinqXMLProcessor.Base
Ez a leírás a komponens **v1.2.1** kiadásáig bezáróan naprakész.
Igényelt minimális framework verzió: **4.0**
Teljes funkcionalítás és hatékonyság kihasználásához szükséges legalacsonyabb framework verzió: **4.0**
#### Ez a komponens arra szolgál, hogy az XML-ben tárolt beállítások, paraméterek feldolgozása egységes legyen, és a következő elveket kényszerítse:
* ##### A paraméter feldolgozás tisztán OOP-elvű legyen
* ##### Kerüljön kiemelésre egy külön osztályba, amelynek ez a scopja. Így a megvalósítás még véletlenül se legyen összeépítve a felhasználási hely feladataival, megsértve ezzel a Single Responsibility elvet.
* ##### A használt megoldás semmilyen módon ne legyen változás érzékeny, hibatürő legyen. (Ne serializációra épüljön.)
* ##### Kevés implementáció függő kód írásával adjon eredményt.
  
Használatára ez a kód minta (a kód részlet copyzható az üjabb paraméter fájlfeldolgoztó kiindulási mintajéként):

```java
using System.Collections.Generic;
using System.Xml.Linq;
using Vrh.LinqXMLProcessor.Base;

/// TODO: Változtasd meg a felhasználási helynek megfelelő névtérre 
namespace YourNameSpace
{
    // TODO: Töröld az instrukciós megjegyzéseket (TODO) a production-level kódból, miután elvégezted a bennük foglaltakat!

    /// <summary>
    /// TODO: Mindig nevezd át ezt az osztályt!!! 
    ///         Használd az osztályra vonatkozó elnevezési konvenciókat, beszédes neveket használj a DDD (Domain Driven Development) alapelveinek megfelelően!
    ///         Naming pattern: {YourClassName}XmlProcessor 
    ///         Mindig használd az XmlProcessor suffix-et!
    /// </summary>
    public class YourClassNameXmlConfigProcessor : LinqXMLProcessorBaseClass
    {
        /// <summary>
        /// Constructor 
        ///     TODO: Nevezd át az osztály nevére!
        /// </summary>
        /// <param name="parameterFile">XML fájl aminek a feldolgozására az osztály készül</param>
        public YourClassNameXmlConfigProcessor(string parameterFile)
        {
            _xmlFileDefinition = parameterFile;
        }

        #region Retrive all information from XML

        // TODO: Írd át vagy töröld ezeket a meglévő példa implemenmtációkat! Az alapelveket bent hagyhatod, hogy később is szem előtt legyenek!
        //  Alapelvek:
        //      - Mindig csak olvasható property-ket használj getter megvalósítással, ha az adat visszanyeréséhez nem szükséges paraméter átadása!
        //      - Csak akkor használj függvényeket, ha a paraméterek átadására van szükség az információ visszanyeréséhez!
        //      - Mindig légy típusos! Az alaposztály jelenlegi implementációja (v1.1.X) az alábbi típusokat kezeli: int, string, bool, Enumerátor (generikusan)! Ha típus bővítésre lenne szükséged, kérj fejlesztést rá (change request)! 
        //      - Bonyolultabb típusokat elemi feldolgozással építs! Soha ne használj XML alapú szérializációt, amit depcreatednek tekintünk a fejlesztési környezeteinkben!
        //      - A bonyolultabb típusok kódját ne helyezd el ebben a fájlban, hanem külső definíciókat használj!
        //      - Ismétlődő információk visszanyerésére (listák, felsorolások), generikus kollekciókat használj (Lis<T>, Dictonary<Tkey, TValue>, IEnumerable<T>, stb...) 

        /// <summary>
        /// TODO: Írd felül, vagy töröld ezt a példát!
        ///     Egyszerű boolean érték visszanyerése adott element értékéből
        /// </summary>
        public bool Property1
        {
            get
            {
                return GetExtendedBoolElementValue(GetXElement(PROPERTY1_ELEMENT_NAME), true);
            }
        }

        /// <summary>
        /// TODO: Írd felül, vagy töröld ezt a példát!
        ///     Egyszerű boolean érték visszanyerése adott element alatti atribute értékéből
        /// </summary>
        public bool Property2
        {
            get
            {
                return GetExtendedBoolAttribute(GetXElement(PROPERTY1_ELEMENT_NAME), ATTRIBUTE1_ATTRIBUTE_IN_PROPERTY1_ELEMENT, false, "1", "yes");
            }
        }

        /// <summary>
        /// TODO: Írd felül, vagy töröld ezt a példát!
        ///     Lista visszaadása ismétlődő XML elemekből.
        /// </summary>
        public List<string> AllStrings
        {
            get
            {
                List<string> returnList = new List<string>();
                foreach (var item in GetAllXElements(STRINGS_ELEMENT_NAME))
                {
                    returnList.Add(GetStringElementValue(GetXElement(STRING_ELEMENT_NAME)));    
                }
                return returnList;
            }
        }

        /// <summary>
        /// TODO: Írd felül, vagy töröld ezt a példát!
        ///     Arra példa, hogy mikor használjunk metódust, property helyett: ha az adott információ visszanyeréséhez valamilyen paramétert akarunk felhasználni.
        ///     Használjunk Get prefixet ezen metódusok nevében! Adjunk DDD leveknek megfelelő beszédes neveket!
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        public List<string> GetStringsUnderGroup(string group)
        {
            List<string> returnList = new List<string>();
            foreach (var item in GetAllXElements(STRINGS_ELEMENT_NAME))
            {
                if (GetStringAttribute(item, GROUP_ATTRIBUTE_IN_STRING_ELEMENT) == group)
                {
                    returnList.Add(GetStringElementValue(GetXElement(STRING_ELEMENT_NAME)));
                }
            }
            return returnList;
        }

        #endregion

        #region Defination of namming rules in XML
        // A szabályok:
        //  - Mindig konstansokat használj, hogy az element és az attribútum neveket azon át hivatkozd! 
        //  - Az elnevezések feleljenek meg a konstansokra vonatkozó elnevetési szabályoknak!
        //  - Az Attribútumok neveiben mindig jelöld, mely elem alatt fordul elő.
        //  - Az elemekre ne használj, ilyet, mert az elnevezések a hierarchia mélyén túl összetetté (hosszúvá) válnának! Ez alól kivétel, ha nem egyértelmű az elnevezés e nélkül.
        // 

        //TODO: Töröld, vagy írd felül ezeket a példa konstansokat! 
        private const string PROPERTY1_ELEMENT_NAME = "Property1";
        private const string ATTRIBUTE1_ATTRIBUTE_IN_PROPERTY1_ELEMENT = "Attribute1";        
        private const string STRINGS_ELEMENT_NAME = "Strings";
        private const string STRING_ELEMENT_NAME = "String";
        private const string GROUP_ATTRIBUTE_IN_STRING_ELEMENT = "Group";

        #endregion
    }
}
``` 

Felhasználáskor: 
* Töröld a TODO megjegyzéseket, miután végrehajtottad őket!
* Csak valós kódot hagyj az osztályban, a példákat töröld, vagy írd át valós elemekre!
* A szabályokra vonatkozó instrukciókat célszerűen hagyd meg a kommentekben, hogyha bárki a jövőben bővíti a feldolgozó osztályt, akkor a szeme előtt legyenek a szabályok.
* Mindig hazsnálj XML commenteket! (Ahogy minden production Level code-ban! Ez egy erős kóddal kapcsolkatos "DONE" feltétel nálunk!)

## Hibaérzékenység bekapcsolása:
A komponens alapértelmezésben hibatűrő müködésű, ha a konfigurációs fájlban hiányoznak a keresett információk, akkor defultokat ad.
Azonban bekapcsolhatjuk, hogy az adott környezetben kivételerket dobjon, mikor a konfiguráció oldalon nem talál meg valamit.
Ezt vagy ugy érjük el, ha a környezet konfigurációjában (App.config, Web.config) elérhető egy Vrh.LinqXMLProcessor.Base:ThrowExceptions app settings kulcs, amelynek értéke true.
Vagy a leszármazott osztályban true-ra állkítjuk a _throwException protected property értékét. Figyelem, a property értékének beállítása után nem veszi többé figyelembe a fent írt app settings kulcs értékét. 

Ha hibaérzékenység nem, de például logolás szükséges, akkor ezt a ConfigProcessorEvent eseményre építve lehet felépíteni. A kompones ezen az eseméynen át minden esetben jelzéstz küld a fellépő konfigurációs hibákról, amik miatt defult értékeket használ az adott konfigurációs beállításra. 

<hr></hr>

# Version History:
## V1.2.3 (2017.12.20)
### Patches:
1. GetRootElement javítása, hogy helyesen dolgozza fel a file.xml/tag1/tag2/tag3 jellegű definíciókat.
## V1.2.2 (2017.03.21)
### Patches:
1. GetElementPath, GetAttributePath nullvédelme
## V1.2.1 (2017.03.21)
### Patches:
1. Nuget függőség a Vrh.Common v1.10.0  csomagra
2. Dokumentáció kiegészítése a _throwException/Vrh.LinqXMLProcessor.Base:ThrowExceptions és a ConfigProcessorEvent-re vonatkozó információkkal.
## V1.2.0 (2017.03.20)
### Compatibility API changes:
1. GetElementValue és GetAttribute generikusok bevezetése
2. Nem generikus lekérő függvények Obsolete-nak jelőlése (kivéve a speciális/kiterjesztett működésűek (GetExtendedBool..., GetExtendedString...))
3. Esemény bevezetése a paraméterfeldolgozási események, hibák egységes kezelésének támogatásához, annak a logolásba való bekötéséhez. (ConfigProcessorEvent)
4. GetAllXElements-nél  acsillag bevezetése adott XML path alatti összes XML element lekérésére
### Patches:
1. Az XML-re tesz egy filewatchert, és csak akkor tölti be újra, ha megváltozott a file. Így a GetRootElement hívás kb 3000-szer gyorsabb lett, ha nincs file változás.
2. Unit test projekt hozzáadása a projekthez 
## V1.1.2 (2017.02.24)
### Patches:
1. A GetRootElement alapimplementáció javítása, hogy ha olyan XML-t dolgoz fel, ahol a root tag-be adtak meg namespacet, de azt nem alaklamazták a path minden elemén, akkor is megtalálja a path által definiált XML tagetet.
## V1.1.1 (2017.02.17)
### Patches:
1. A GetRootElement alapimplementáció javítása, hogy ne fusson hibára a @ kezdőkarakterrel definiált abszolút útvonalaknál.
## V1.1.0 (2017.02.15)
### Compatibility API changes:
1. A GetRootElement az alaposztályban nem abstract, hanem hanem virtual. Az alapimplementáció bekerült az alaposztályba. Így van egy alap viselkedése, és nem kell mindig  aszármazatott osztályban implementálni. 
### Patches:
1. Framework version támogatás rendbetétele. (Támogatott Framework: v4 és afelett)
1. Modul dokumentáció minta bevezetése (ReadMe.md hozzáadása)  
1. Sample implementáció átemelése a Readme.md fájlba 
1. XML dokumentáció javítása
## V1.0.1 (2017.02.09)
### Patches:
1. Sample implementációban használt GetRootElement override ne szálljon el, ha nem létező XML hivatkozást kap a feldolgozó
## V1.0.0 (2017.01.24)
### Initial version

