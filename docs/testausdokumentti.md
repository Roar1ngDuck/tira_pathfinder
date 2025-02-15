Seuraavat asiat testataan:
- A* löytää oikean reitin pienessä kartassa.
- A* käy läpi oikeat pisteet pienessä kartassa.
- A* ei löydä mitään reittiä jos sellaista ei ole olemassa.
- Dijkstra löytää oikean reitin pienessä kartassa.
- Dijkstra käy läpi oikeat pisteet pienessä kartassa.
- Dijkstra ei löydä mitään reittiä jos sellaista ei ole olemassa.
- Jump Point Search löytää oikean reitin pienessä kartassa.
- Jump Point Search löytää oikeat Jump Pointit pienessä kartassa.
- Jump Point Search ei löydä mitään reittiä jos sellaista ei ole olemassa.
- Dijkstra, A* ja JPS löytää yhtä pitkät reitit 100 sattumanvaraisesti generoidussa 128x128 kartassa.

Testit voidaan ajaa käynnistämällä ensin komento ```dotnet build``` jonka jälkeen ```dotnet test```.

Testeistä voidaan tallentaa kattavuus komennolla: ```dotnet test --collect:"XPlat Code Coverage"```

Tämän jälkeen kattavuusraportti voidaan luoda komennolla ```reportgenerator -reports:"src/Pathfinder.Tests/TestResults/*/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html```

Tai yhdellä rivillä:
```dotnet test --collect:"XPlat Code Coverage" && reportgenerator -reports:"src/Pathfinder.Tests/TestResults/*/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html```

Testien kattavuus [täällä](/coveragereport/Summary.txt)