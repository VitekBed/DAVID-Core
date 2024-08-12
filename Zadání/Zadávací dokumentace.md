# Jádro
## Cíl

Vyvinout modulární třívrstvou aplikaci, která umožní efektivní správu a organizaci dat v různých podnikových procesech. Aplikace by měla být navržena s ohledem na snadnou rozšiřitelnost, udržovatelnost a škálovatelnost.

## Architektura

Aplikace bude rozdělena do tří hlavních vrstev:

1. Prezentační vrstva (UI):
    -	Poskytuje uživatelské rozhraní, které umožňuje interakci uživatelů s aplikací.
    - Tato vrstva by měla být responzivní, uživatelsky přívětivá a dostupná na různých zařízeních (mobilní telefony, tablety, počítače).
    - Podpora různých autentizačních metod (např. OAuth, SAML).
2. Aplikační vrstva (Business Logic):
    - Obsahuje veškerou obchodní logiku aplikace.
    - Zpracovává požadavky z prezentační vrstvy, provádí potřebné operace a interaguje s datovou vrstvou.
    - Měla by být implementována tak, aby bylo možné snadno přidávat nové moduly nebo funkce.
    - Podpora pro provádění složitých operací, jako je validace dat, výpočty, a různé transformace dat.
3. Datová vrstva (Data Management):
	  - Odpovídá za ukládání a správu dat v databázích.
    - Zajišťuje integritu a bezpečnost dat.
    - Umožňuje snadný přístup k datům pro aplikační vrstvu a zajišťuje optimalizovaný výkon při vyhledávání a aktualizaci dat.
    - Měla by podporovat různé typy databází (relační, NoSQL) podle požadavků aplikace.

## Funkcionality

Aplikace by měla podporovat následující základní funkcionality:

1. Správa uživatelů a autentizace:
    - Systém správy uživatelských účtů s podporou pro více rolí.
	  -	Autentizace a autorizace uživatelů.
2. Modulární struktura:
    - Aplikace bude navržena tak, aby umožňovala přidávání a odebírání modulů dle potřeby.
	  - Každý modul by měl být nezávislý a snadno integrovatelný s ostatními částmi aplikace.
3. Reporting a analytika:
	  - Vestavěné nástroje pro generování reportů a analýzu dat.
	  - Možnost exportu dat v různých formátech (CSV, PDF, Excel).
4. Škálovatelnost a výkon:
    - Aplikace by měla být navržena s ohledem na škálovatelnost, aby mohla snadno růst s rostoucími potřebami.
	  - Optimalizace výkonu pro zpracování velkého množství dat a uživatelů.
5. Zabezpečení:
    - Implementace bezpečnostních opatření pro ochranu dat a aplikace.
	  - Šifrování citlivých dat a komunikace.
    - Mechanismy pro zálohování a obnovu dat.

## Testování a nasazení
  - Podpora automatizovaného testování v rámci CI/CD
	-	Unit testy pro jednotlivé části jádra.
	-	Nasazení aplikace v cloudovém prostředí v podobě kontejnerové aplikace

## Dokumentace
  - Kompletní dokumentace k API jádra
	-	Uživatelská dokumentace a průvodce pro koncové uživatele.

## Technologie
  - Frontend: WinForms klient pro vývoj s výhledem na vytvoření Webového klienta pro uživatele
	- Backend: .NET 8+, EntityFramework
	-	Databáze: PostgreSQL, MongoDB, MySQL, nebo libovolná jiná
	-	Nasazení: Docker, Kubernetes, nebo AWS/GCP/Azure

# Zadání pro Business Logiku

## 1. Úvod
Tato aplikace slouží k efektivní správě a plánování ubytování, stravování a rozdělování osob do skupin pro různá setkání a události. Business logika bude řešit integraci a provádění všech potřebných operací na základě požadavků uživatelů, správu dat a poskytování rozhraní pro další vrstvy aplikace.

## 2. Moduly Aplikace

Aplikace bude rozdělena do následujících modulů, které budou řízeny prostřednictvím business logiky:

1. **Správa ubytování**
2. **Správa stravování**
3. **Rozdělení osob do skupin**
4. **Správa setkání a akcí**
5. **Správa uživatelů a oprávnění**

## 3. Funkcionality Business Logiky

### 3.1. Správa ubytování
- **Rezervační systém**: 
  - Vytváření a správa rezervací pro jednotlivé místnosti na základě dostupných ubytovacích jednotek.
  - Přidělování místností jednotlivým osobám s ohledem na specifické požadavky (např. bezbariérovost, počet lůžek, pohlaví).
  - Automatická kontrola konfliktů v rezervacích (např. překrývání termínů).
  
- **Údržba a správa ubytovacích jednotek**:
  - Evidence a aktualizace informací o budovách, podlažích, skupinách jednotek a jednotlivých místnostech.
  - Kategorie a vlastnosti místností (např. dostupné vybavení, kapacita).

- **Uzávěrky ubytování**:
  - Možnost uzavření rezervací po určitém termínu s možností úprav pouze administrátorem.

### 3.2. Správa stravování
- **Evidence stravovacích preferencí**:
  - Sledování a správa stravovacích požadavků osob (např. diety, zdravotní omezení).

- **Generování stravenek**:
  - Automatické generování stravenek na základě ubytování nebo individuálních potřeb.
  - Možnost ruční úpravy stravenek podle specifických požadavků (např. výjimky ve stravování).

- **Uzávěrky stravování**:
  - Možnost uzávěrky stravovacích údajů a zajištění, že již uzavřené záznamy nelze editovat.

### 3.3. Rozdělení osob do skupin
- **Kategorie pro rozdělení**:
  - Automatické přiřazení osob do skupin na základě definovaných kritérií (např. věk, pohlaví, stav).
  - Možnost ruční úpravy přiřazení osob do skupin.

### 3.4. Správa setkání a akcí
- **Evidence setkání**:
  - Správa a evidence jednotlivých setkání s možností přidávání a úpravy informací.
  - Spojení setkání s ubytováním a stravováním.

- **Příprava podkladů**:
  - Automatizovaná příprava všech potřebných dokumentů a podkladů pro dané setkání (např. seznamy ubytování, stravování).

### 3.5. Správa uživatelů a oprávnění
- **Role a oprávnění**:
  - Definice uživatelských rolí s odpovídajícími oprávněními (např. administrátor, běžný uživatel).
  - Možnost přiřazování a správy oprávnění jednotlivým uživatelům.

## 4. Integrace a Interakce
- **Interakce mezi moduly**: 
  - Business logika zajistí plynulou integraci mezi jednotlivými moduly, např. synchronizaci informací mezi ubytováním a stravováním.
  
- **API a rozhraní pro další vrstvy**:
  - Poskytování API a dalších rozhraní pro interakci prezentační vrstvy s business logikou.

## 5. Výkon a Optimalizace
- **Optimalizace dotazů**:
  - Optimalizace dotazů do databáze, aby byla zajištěna co nejrychlejší odezva systému i při velkém množství dat.
  
- **Škálovatelnost**:
  - Business logika by měla být navržena tak, aby umožňovala snadné přidávání nových funkcionalit a rozšiřování aplikace.

## 6. Bezpečnost
- **Zabezpečení dat**:
  - Implementace bezpečnostních opatření pro ochranu citlivých dat (např. šifrování, přístupová oprávnění).
  
- **Audit a logování**:
  - Sledování a logování všech klíčových operací v systému pro potřeby auditu.
