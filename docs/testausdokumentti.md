Seuraavat asiat testataan:
- Dijkstra löytää oikean reitin pienessä kartassa.
- Dijkstra käy läpi oikeat pisteet pienessä kartassa.
- Dijkstra ei löydä mitään reittiä jos sellaista ei ole olemassa.
- Dijkstra löytää lyhyimmän reitin valitussa kartassa, jossa lyhyin reitti kulkee kaikkiin 8 suuntaan.
- A* löytää oikean reitin pienessä kartassa.
- A* käy läpi oikeat pisteet pienessä kartassa.
- A* ei löydä mitään reittiä jos sellaista ei ole olemassa.
- Jump Point Search löytää oikean reitin pienessä kartassa.
- Jump Point Search löytää oikeat Jump Pointit pienessä kartassa.
- Jump Point Search ei löydä mitään reittiä jos sellaista ei ole olemassa.
- Dijkstra, A* ja JPS löytää yhtä pitkät reitit 200 sattumanvaraisesti generoidussa 128x128 kartassa.
- Dijkstra, A* ja JPS löytää yhtä pitkät reitit 3 valmiissa kartassa MovingAI lab sivulta: Berlin_1_512, 16room_001, WinterConquest
- 200 sattumanvaraisesti generoidussa 128x128 kartassa keskimäärin Dijkstra on hitain, A* toiseksi nopein ja JPS nopein.

Testit voidaan ajaa käynnistämällä ensin komento ```dotnet build``` jonka jälkeen ```dotnet test```.

Kattavuusreporttia varten tarvitaan reportgenerator, jonka voi asentaa komennolla ```dotnet tool install -g dotnet-reportgenerator-globaltool```

Testeistä voidaan tallentaa kattavuus komennolla: ```dotnet test --collect:"XPlat Code Coverage"```

Tämän jälkeen kattavuusraportti voidaan luoda komennolla ```reportgenerator -reports:"src/Pathfinder.Tests/TestResults/*/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:MarkdownSummary```

Tai yhdellä rivillä:
```dotnet test --collect:"XPlat Code Coverage" && reportgenerator -reports:"src/Pathfinder.Tests/TestResults/*/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:MarkdownSummary```

Itse algoritmien osalta testikattavuus:
|**Name**|**Covered**|**Uncovered**|**Coverable**|**Total**|**Line coverage**|**Covered**|**Total**|**Branch coverage**|
|:---|---:|---:|---:|---:|---:|---:|---:|---:|
|AStar|110|1|111|242|99%|26|28|92.8%|
|Dijkstra|95|1|96|220|98.9%|18|20|90%|
|JumpPointSearch|253|9|262|497|96.5%|117|124|94.3%|

Koko testikattavuusraportti [täällä](/coveragereport/Summary.md)