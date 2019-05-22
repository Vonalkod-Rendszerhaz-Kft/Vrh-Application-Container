# Vrh.OneMessage
Ez a leírás a komponens **v0.0.0** kiadásáig bezáróan naprakész.
Igényelt minimális framework verzió: **4.5**
Teljes funkcionalitás és hatékonyság kihasználásához szükséges legalacsonyabb framework verzió: **4.5**

**A komponens egyetlen ddl-t tartalmaz, amely e-mail küldést tesz lehetővé.
Közvetlenül a OneMessage osztály példányosításával,
vagy az MVC alkalmazás alól a OneMessage area OneMessage controllerének 
SendSMTPMessage post action-jével.**

# TODO: Megírandó a komponens dokumentációja ebbe a fájlba!!!

# Version History:

#### 1.1.4 (2019.05.22) Patches - debug:
- Tesztkörnyezett finomítása.

#### 1.1.3 (2019.04.24) Patches - debug:
- Frissítés a Microsoft.AspNet.Mvc 5.2.7 változatára.

#### 1.1.2 (2017.04.28) Patches:
- Attachment esetén használja az XML-ben definiált paraméter szeparátort.
- SendSMTPMessage akció válaszában minden hiba esetén -1 lesz a ReturnValue.

#### 1.1.1 (2017.04.28) Patches:
- Névtér átnvezés: 'Vrh.OneMessage'-re a 'Vrh.OneMessage.Area.OneMessage'-ről.

#### v1.1.0 (2017.04.28) Compatible API changes:
- A ReceivedParameters osztály kibővítése, hogy az üzenet tárgya is érkezhessen
paraméterként. A SendSMTPMessage action url vagy dict paraméterként tudja 
fogadni 'subject' néven. Példányosítás esetén a ReceivedParameters osztály 
Subject tulajdonságába kell írni a paraméternek szánt tárgyat.
- Paraméterként érkező csatolmány elérési utak is tartalmazhatnak
behelyettesítendő hivatkozásokat.

#### v1.0.0 (2017.04.04) 
Initial Relase