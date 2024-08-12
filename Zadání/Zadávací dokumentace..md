# Cíl

Vyvinout modulární třívrstvou aplikaci, která umožní efektivní správu a organizaci dat v různých podnikových procesech. Aplikace by měla být navržena s ohledem na snadnou rozšiřitelnost, udržovatelnost a škálovatelnost.

# Architektura

Aplikace bude rozdělena do tří hlavních vrstev:

1. Prezentační vrstva (UI):
    -	Poskytuje uživatelské rozhraní, které umožňuje interakci uživatelů s aplikací.
    - Tato vrstva by měla být responzivní, uživatelsky přívětivá a dostupná na různých zařízeních (mobilní telefony, tablety, počítače).
    - Podpora různých autentizačních metod (např. OAuth, SAML).
2.	Aplikační vrstva (Business Logic):
    - Obsahuje veškerou obchodní logiku aplikace.
    - Zpracovává požadavky z prezentační vrstvy, provádí potřebné operace a interaguje s datovou vrstvou.
    - Měla by být implementována tak, aby bylo možné snadno přidávat nové moduly nebo funkce.
    - Podpora pro provádění složitých operací, jako je validace dat, výpočty, a různé transformace dat.
3. Datová vrstva (Data Management):
	  - Odpovídá za ukládání a správu dat v databázích.
    - Zajišťuje integritu a bezpečnost dat.
    - Umožňuje snadný přístup k datům pro aplikační vrstvu a zajišťuje optimalizovaný výkon při vyhledávání a aktualizaci dat.
    - Měla by podporovat různé typy databází (relační, NoSQL) podle požadavků aplikace.

# Funkcionality

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

# Testování a nasazení
  - Podpora automatizovaného testování v rámci CI/CD
	-	Unit testy pro jednotlivé části jádra.
	-	Nasazení aplikace v cloudovém prostředí v podobě kontejnerové aplikace

# Dokumentace
  - Kompletní dokumentace k API jádra
	-	Uživatelská dokumentace a průvodce pro koncové uživatele.

# Technologie
  - Frontend: WinForms klient pro vývoj s výhledem na vytvoření Webového klienta pro uživatele
	- Backend: .NET 8+, EntityFramework
	-	Databáze: PostgreSQL, MongoDB, MySQL, nebo libovolná jiná
	-	Nasazení: Docker, Kubernetes, nebo AWS/GCP/Azure
