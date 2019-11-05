# Vrh.ApplicationContainer
![](http://nuget.vonalkod.hu/content/projectavatars/applicationcontainer.png)

Ez a leírás a komponens a leírtakat tekintve **v1.1.2** kiadásáig bezáróan naprakész.
**A dokumentáció nem teljes!**
Igényelt minimális framework verzió: **4.5**
Teljes funkcionalitás és hatékonyság kihasználásához szükséges legalacsonyabb framework verzió: **4.5**

**Ez a komponens alkalmazás funkciók hostolására szolgál.** 

A Nuget csomagját jellemzően nem telepítjük önállóan, hanem a VSIX instalerrel hozzáadjuk a VRH ApplicationContainer project template extensiont a Visual Studio példányunkhoz. Ez telepíti a VRH ApplicationContainer project templatet. Amikor ApplicationContainer plugint fejlesztünk, akkor egy **VRH ApplicationContainer Plugin** projecttípust adunk a solusion-höz.

Az alkalmazások terjedelmére, jellegére nincs megkötés, de a megoldás jellegéből fakadóan ez egy microservice arhitektura, melyben az alkalmazás funkciókat önálló kis egységekként, az azok közötti kapcsolatokat pedig szolgáltatásként fogalmazzuk meg. 
A hostolt funkciók egységes elvek mentén építhetőek fel, pluginokként. A plugin példányok mindegyike önálló konfigurációval rendelkezhet.

Maga az ApplicationContainer keret szolgáltatásokat nyújt a benne hostolt pluginok kezelésére, állapotok lekérdezésére és információk kinyerésére.

Az alaklmazás modulok (pluginok) közti kommznikációt, ha erre szükség van, minden esetben Vrh.EventHub alapú megoldásra kell építeni. Egyedi plugin közti kommunikáció kialakítása nem megengedett!!!

## Alapfogalmak
### ApplicationContainer, vagy AppliucationContainer core:
Maga az ApplicationContainer általános keret.
### Összeállítás:
Az ApplicationContainer keret, és a benne hostolt funkciókat megvalósító pluginok összesége, továbbá a működést befolyásoló beállítások (konfiguráció). Valamint a konfiguráció szerint használt InstanceFactory.
### ApplicationContainer Plugin, vagy Plugin:
A tényleges funkciókat megvalósító egységek, melyeket az ApplicationContainer képes példányosítani és hostolni. A Pluginoknak meg kell felelni bizonyos előírásoknak, IPlugin interface-t kell implementálniuk, legalább azon keresztül, hogy kötelezően PluginAncestor leszármazottak.
### InstanceFactory, vagy InstanceFactory plugin:
Az ApplicationContainer  keret egy olyan szabadon cserélhető (pluginolható) része, mely azért felleős, hogy egy a programozáson kivüli eszlközkészletet biztosítson annak leírására, hogy milyen funkciókat (Pluginok) hostol az összeállítás, és milyen beállítások melett. Az InstanceFactory felleős ezen leírás alapján az összetevők betöltéért.
## ApplicationContainer keret használata
Ez a rész az ApplicationContainer alapvető használatát, konfigurálását tárgyalja.
### Copnsole és Service host paraméterek
Mind a Console mind  aWindows service host indítható az alábbi paraméterekkel.
#### APPCONFIG
Segítségével megadható egy fájlnév (relatív, vagy abszolút), amit a futó példány app.configként fog használni. A fájlnak érvényes, szabvány app.confignak kell lennie.

Használata:
```shell
Vrh.ApplicationContainer.ConsoleHost.exe -APPCONFIG /MyAppConfig.xml
```
#### INUSEBY
Segítségével, megadható, hogy az indított példány melyik plugin példányokat (instance) hostolja (töltse be), azok közül, melyek az InstanceFactory.FromXML Factory pluginhoz tartozó xml-ben definiálva vannak. Azok kerülnek betöltésre, amelyek Inuse attribútuma egyezik az INUSEBY indítási paraméterben megadottal.

Használata:
```shell
Vrh.ApplicationContainer.ConsoleHost.exe -INUSEBY workplace1
```
### Logolással kapcsolatos konfiguráció
Az ApplicationContainer a **Vrh.Logger** komponenst használja a Logolásra, minden logolással kapcsolatos beállítás a **Vrh.Logger** komponens dokumentációjában leírtak szerint tehető meg.
### Szabvány alkalmazás Config fájl, és az ott megtehető beállítások
#### 1. Appsettings kulcsok
Az alábbi beállítások az appSettings szekció alá szabványos add-okkal definiálhatóak az AppliactionContainer keretet hostoló alaklamazás config fájljában.
##### 1.  Vrh.ApplicationContainer:ConfigurationFile
Segítségével definiálhatjuk, hogy az AppliactionContainer keret hol keresse a saját beállításait tartalmazó XML configurációs fájlt, vagy XML fájl fregmentumot. Amennyiben nem adjuk meg ezt  abeállítást, akkor az alapértelmezés szerint az alkalmazás munkakönyvtárában keres egy ApplicationContainer.Config.xml fájlt. Elhagyható a beállítás akkor, is, ha VRH stílusú XML config fájl helyett, az azokat tartalmazó beállításokat a szabvány .Net alkalmazás .config fájlban kívánjuk megtenni, az ezen fejezet további részében tárgyalt módon.

Használata:
```xml
<appSettings>    
    <add key="Vrh.ApplicationContainer:ConfigurationFile" value="ApplicationContainer.Config.xml"/>
</appSettings>
```
##### 2. Vrh.ApplicationContainer:InstanceFactoryAssembly
Az ApplicationContainer-ben szabadon definiálható, hogy melyik ugynevezett Instance Factory plugint használja a hostolt Plugin példányok definiálására (lásd később). Ez a beállítás definiálja azt a .Net Assembly-t, amelyik a használni kívánt Instance Factory-t tartalmazza.

Használata:
```xml
  <appSettings>    
    <add key="Vrh.ApplicationContainer:InstanceFactoryAssembly" value="InstanceFactory.FromXML.dll"/>
  </appSettings>
```
##### 3. Vrh.ApplicationContainer:InstanceFactoryType
Ez a beállítás definiálja hogy az ApplicationContainer keret a *Vrh.ApplicationContainer:InstanceFactoryAssembly* kulccsal definiált .Net Assembly-ből, melyik típust (.Net type) használja, mint Instance Factory-t. 

Használata:
```xml
  <appSettings>    
    <add key="Vrh.ApplicationContainer:InstanceFactoryType" value="InstanceFactory.FromXML.IntsnceFactoryFromXML"/>
  </appSettings>
```
##### 4. Vrh.ApplicationContainer:InstanceFactoryVersion
Ez a beállítás definiálja hogy az ApplicationContainer keret a *Vrh.ApplicationContainer:InstanceFactoryAssembly* kulccsal definiált .Net Assembly-ből, melyik verziót használja, mint Instance Factory-t.

Használata:
```xml
  <appSettings>    
    <add key="Vrh.ApplicationContainer:InstanceFactoryVersion" value="0.1.0-preAlpha"/>
  </appSettings>
```
##### 5. Vrh.ApplicationContainer:MessageStackSize
Az ApplicationContainer, egyik szolgáltatása, hogy egy tárolót biztosít, azoknak az üzeneteinek a tárolására, melyek a belső működéséről, állapotáról, a fellépő hibákról nyújt tájékoztatást. Jelenleg két elkülönült tárolót használ. Az egyiket a fellépett hibák tárolására, a másikat mindenb egyéb müködési információü tárolására. Ez a beállítás jelenleg egységesen vonatkozik a hiba tároló és az információs tároló méretére. Ha tehát az értékét 100-re állítjuk, akkor az ApplicationContainer példány minden pillanatban a működése során felépett 1000 utolsó hibát, és 1000 utolsó működéi üzenetet tartalmazza.
Mikor a tároló betelt, akkor egy esetleges újabb üzenet a legrégebbi helyére kerül be.

Használata:
```xml
  <appSettings>    
    <add key="Vrh.ApplicationContainer:MessageStackSize" value="1000"/>
  </appSettings>
```
#### WCF Config
Az ApplicationContainer runtime szolgáltatásai egy WCF szolgáltatáson keresztül érhetőek el egy külső alaklmazás (pl. Menedzsment felület) számára. Az ezzel kapcsolatos konfigurációt szabványosan kell megtenni a hostoló alaklmazáshoz tartozó .Net alkalmazás config fájlban, mintr bármely más WCF alklamzás esetén. (Lásd az ezzel kapcsolatos Microsoft-os dokumentációkat)

Az alábbi példán látszik, hogy a **Vrh.ApplicationContainer.IApplicationContainer** contractot kell a szolgáltatához kötni. A szolgáltatás bármilyen egyirányú WCF felületen keresztül publikálható (az alábbi példában basicHttp-n működik):
```xml
<system.serviceModel>
    <bindings>
      <basicHttpBinding>
        <binding name="BasicHttpBinding" closeTimeout="00:01:00" openTimeout="00:01:00" receiveTimeout="00:10:00" sendTimeout="00:01:00" allowCookies="false" bypassProxyOnLocal="false" hostNameComparisonMode="StrongWildcard" maxBufferPoolSize="524288" maxBufferSize="65536" maxReceivedMessageSize="65536" textEncoding="utf-8" transferMode="Buffered" useDefaultWebProxy="true" messageEncoding="Text">
          <readerQuotas maxDepth="32" maxStringContentLength="8192" maxArrayLength="16384" maxBytesPerRead="4096" maxNameTableCharCount="16384"/>
          <security mode="None">
            <transport clientCredentialType="None" proxyCredentialType="None" realm=""/>
            <message clientCredentialType="UserName" algorithmSuite="Default"/>
          </security>
        </binding>
      </basicHttpBinding>
    </bindings>
    <services>
      <service behaviorConfiguration="ServiceBehavior" name="Vrh.ApplicationContainer.ApplicationContainerService">
        <endpoint bindingName="BasicHttpBinding" address="" binding="basicHttpBinding" contract="Vrh.ApplicationContainer.IApplicationContainer">
          <identity>
            <dns value="localhost"/>
          </identity>
        </endpoint>
        <endpoint address="mex" binding="mexHttpBinding" contract="IMetadataExchange"/>
        <host>
          <baseAddresses>
            <add baseAddress="http://localhost:8777/Vrh.ApplicationContainer"/>
          </baseAddresses>
        </host>
      </service>
    </services>
    <behaviors>
      <serviceBehaviors>
        <behavior name="ServiceBehavior">
          <!-- To avoid disclosing metadata information, set the value below to false and remove the metadata endpoint above before deployment -->
          <serviceMetadata httpGetEnabled="true"/>
          <!-- To receive exception details in faults for debugging purposes, set the value below to true.  Set to false before deployment to avoid disclosing exception information -->
          <serviceDebug includeExceptionDetailInFaults="true"/>
        </behavior>
        <behavior name="netPipeServiceBehavior">
          <!-- To avoid disclosing metadata information, set the value below to false and remove the metadata endpoint above before deployment -->
          <serviceMetadata httpGetEnabled="false"/>
          <!-- To receive exception details in faults for debugging purposes, set the value below to true.  Set to false before deployment to avoid disclosing exception information -->
          <serviceDebug includeExceptionDetailInFaults="true"/>
        </behavior>
      </serviceBehaviors>
    </behaviors>
</system.serviceModel>
```
### ApplicationContainer.Config.XML fájl, vagy XML fregmentum
Ez a Vrh stílusú XML configurációs fájl tartalmazza az ApplicationContainer keret közvetlen beállításait. Az itt szereplő értékek csak akkor jutnak érvényre, ha az alkalmazás szabványos config fájljában nem definiáltunk rá vonatkozó kulcsot.

```xml
<?xml version="1.0" encoding="utf-8" ?>
<Vrh.ApplicationContainer>
  <Config>
    <InstanceFactoryAssembly>InstanceFactory.FromXMLConfig.dll</InstanceFactoryAssembly>
    <InstanceFactoryType>InstanceFactory.FromXMLConfig.IntsnceFactoryFromXMLConfig</InstanceFactoryType>
    <InstanceFactoryVersion>0.1.0-preAlpha</InstanceFactoryVersion>
    <MessageStackSize>1000</MessageStackSize>
  </Config>
</Vrh.ApplicationContainer>
```
#### 1. InstanceFactoryAssembly
Az ApplicationContainer-ben szabadon definiálható, hogy melyik ugynevezett Instance Factory plugint használja a hostolt Plugin példányok definiálására (lásd később). Ez a beállítás definiálja azt a .Net Assembly-t, amelyik a használni kívánt Instance Factory-t tartalmazza.
#### 2. InstanceFactoryType
Ez a beállítás definiálja hogy az ApplicationContainer keret a *InstanceFactoryAssembly* tag-gel definiált .Net Assembly-ből, melyik típust (.Net type) használja, mint Instance Factory-t.
#### 3. InstanceFactoryVersion
Ez a beállítás definiálja hogy az ApplicationContainer keret a *InstanceFactoryAssembly* kulccsal definiált .Net Assembly-ből, melyik verziót használja, mint Instance Factory-t.
#### 4. MessageStackSize
Az ApplicationContainer, egyik szolgáltatása, hogy egy tárolót biztosít, azoknak az üzeneteinek a tárolására, melyek a belső működéséről, állapotáról, a fellépő hibákról nyújt tájékoztatást. Jelenleg két elkülönült tárolót használ. Az egyiket a fellépett hibák tárolására, a másikat mindenb egyéb müködési információü tárolására. Ez a beállítás jelenleg egységesen vonatkozik a hiba tároló és az információs tároló méretére. Ha tehát az értékét 100-re állítjuk, akkor az ApplicationContainer példány minden pillanatban a működése során felépett 1000 utolsó hibát, és 1000 utolsó működéi üzenetet tartalmazza.
Mikor a tároló betelt, akkor egy esetleges újabb üzenet a legrégebbi helyére kerül be.
### Pluginokkal kapcsolatos konfiguráció
Az ApplicationContainer a különféle funkciókat, pluginok formájában képes magába hostolni. Ezen pluginokból egymás mellett 1..n darab példány létezhet, melyek teljesen független működést valósítanak meg.
Azt, hogy az ApplicationContainer keret milyen szabályok, és konfigurácoó mentén hostol magába funkciókat (pluginokat) az ApplicationContainer keretnek egy speciális része, a hazsnált InstanceFactory határozza meg.
Az InstanceFactory maga is egy plugin a keretben (de nem normál ApplicationContainer plugin), így egy összeállításban szabadon lecserélhető.

**[Jelen fejezet alatt kell dokumentálni minden a keret fejlődése során megvalósított InstanceFactory plugin használatát!]**

Jelenleg az alábbi InstanceFactory plugin megvalósítások léteznek:
#### 1. InstanceFactory.FromXML
Ez az InstanceFactory egy XML fájlban leírt konfiguráció alapján tölti be a pluginiokat. 
Az XML fájlt alapértelmezésben a munkakönyvtárban keresi **Plugins.Config.xml** néven. Ha ettől el kívánunk térni, akkor a hostoló alaklamzás szabvány .Net config fájljában hozzá kell adnunk a **Vrh.ApplivationContainer.InstanceFactory.FromXML:ConfigurationFile** app settings kulcsot:
```xml
  <appSettings>    
    <add key="Vrh.ApplivationContainer.InstanceFactory.FromXML:ConfigurationFile" value="Plugins.Config.xml"/>
  </appSettings>
```
Az Pluginok konfigurációját leíró XMl fájlt ekkor az alábbi XML példa szemlélteti:
```xml
<?xml version="1.0" encoding="utf-8" ?>
<Plugins.Config>
  <Plugins StackSize="50">
	<Plugin Description="Scheduler Monitor" 
                Assembly="iSchedulerMonitor.dll" 
                Type="iSchedulerMonitor.MonitorPlugin" 
                Version="1.0.0-alpha" 
                Singletone="false" 
                AutoStart ="true" 
                PluginDirectory="iSchedulerMonitor">
    <Instance	Id="Monitor1"
                Name="SchedulerMonitor_for_LearALM"
                Description="Időzített feladatok figyelése a LearALM-ben."
                InstanceConfig="d:\Google Drive\!Dev\VRH\iScheduler\Vrh.iScheduler\App_Data\iScheduler\iScheduler.xml"
                InstanceData="~/App_Data/iScheduler/iScheduler.xml"
                Inuse="schedulerhost"/>
      <Parameters>        
      </Parameters>
    </Instance>
  </Plugin>
  <Plugin Description="IV Connevctor plugin"
              Assembly="IVConnector.Plugin.dll"
              Type ="IVConnector.Plugin.IVConnectorPlugin"
              Version="2.0.0"
              Singletone="false"
              AutoStart="true"
              PluginDirectory="IVConnector"
              PluginConfig="IVConnector.Config.xml">
      <Instance	Id="MSMQIVConnector"
            Name="IVConnector for MSMQ"
            Description="MSQMQ-n érkező bevartkozások kezelőfelülete LearALM-ben."
            InstanceConfig="\IVConnector\IVConnector.Config.xml/IVConnectorConfig/Configuration_M1,\IVConnector\IVConnector.Config.xml/IVConnectorConfig/MessageDefinitions"
            Inuse="connectorhost"/>
      <Instance	Id="TCPIVConnector"
            Name="IVConnector for TCP"
            Description="TCP-n érkező bevartkozások kezelőfelülete LearALM-ben."
            InstanceConfig="\IVConnector\IVConnector.Config.xml/IVConnectorConfig/Configuration_T1,\IVConnector\IVConnector.Config.xml/IVConnectorConfig/MessageDefinitions"
            Inuse="connectorhost"/>
    </Plugin>
  </Plugins>
</Plugins.Config>
``` 
## AppliactionContainer plugin fejlesztés
Az ApplicationContainer-ben a funkciókat alklamazás pluginok formájában fogalmazzuk meg, a megvalósított alkalmazás pluginoknak természetesen meg kell felelnie bizonyos követelményeket. Ezeket, és a plugin fejlesztéssel kapcsolatos tudnivalókat tárgyalja ez a fejezet.
### PluginAncestor
Minden ApplicationContainer plugin kötelezően a PluginAncestor leszármazott, és így IPlugin interfészt implementál!

### Általános programozási szabályok
1. Soha ne használj, a pluginokban static classokat, és singletone mintákat, hacsak ez nem egy Singletone-nak szánt plugin! A normál ApplicationContainer pluginokra igaznak kell lennie, hogy ugyananank a pluginnak az összes példánya (instance) egymástól függetlenül működik. (Kivéve a szolgáltatás kapcsolatokat, melyek EventHub Core alapúak!)

## Templatek

## Létező instanceFactory pluginok
###FromXML instancefactory plugin

## InstanceFactory pluginek fejlesztése

## Application container WCF szolgáltatás


<hr></hr>

# Version History:

## 3.1.0 (2019.11.05):
### Compatibility API changes:
1. Új WCF cím konfiguráció megadási lehetőség beépítése (konfiguráció: új appsettings kulcs lehetőség: WCFBaseAddressList)

## 3.0.4 (2019.11.05):
### Patches:
1. Kódtisztításnál hibásan readonly-ra állított field javítása (MEF Import, csak nem érti meg a Roslyn...)

## 3.0.3 (2019.10.31)
1. Sok inkompatibilis változtatás a felhasznált saját Nuget elemek inkompatibilis megváltoztatása miatt

## 1.1.2 (2018.12.03)
### Patches: 
1. Dokumentáció kiegészítése az 1.1.0-ban létrehozott váltzozásokhoz

## 1.1.1 (2018.12.03)
### Patches: 
1. VISX, NUGET igazítás, target framework rendezés, projekt atríbútumok az 1.1.0-modósításaihoz

## 1.1.0 (2018.11.30)
### Compatibility API Changes:
1. Console és Service Hostokra egyaránt bevezetésre kerül az APPCONFIG és INUSEBY indítási paraméter.
2.  InstanceFactory.FromXML: Plugin.Config.xml-ben az Inuse attríbutúm bevezetése az Instance tag-ekl alá

## 1.0.0 (2018.04.09)
Initial release version

## 1.0.0-alpha (2017.02.21)
Working prototype

