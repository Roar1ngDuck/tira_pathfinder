Seuraavat asiat testataan:
- A* löytää oikean reitin pienessä kartassa.
- A* käy läpi oikeat pisteet pienessä kartassa.
- A* ei löydä mitään reittiä jos sellaista ei ole olemassa.
- Dijkstra, A* ja JPS löytää yhtä pitkät reitit 100 sattumanvaraisesti generoidussa 256x256 kartassa.

Testit voidaan ajaa käynnistämällä ensin komento ```dotnet build``` jonka jälkeen ```dotnet test```.

Testieistä voidaan luoda kattavuusraportti seuraavalla komennolla:
```dotnet test --collect:"XPlat Code Coverage" && reportgenerator -reports:"src/Pathfinder.Tests/TestResults/*/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html```

