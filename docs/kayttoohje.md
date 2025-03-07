## Käynnistys

Asenna .NET SDK 8 tai uudempi: https://dotnet.microsoft.com/en-us/download/dotnet/8.0

Aja juurihakemistossa: ```dotnet run --project src/Pathfinder/Pathfinder.csproj```

## Käyttö

Karttana toimii esimerkiksi mikä tahansa kartta täältä: https://www.movingai.com/benchmarks/grids.html. Ohjelma myös hyväksyy karttana mustavalkoisen kuvan, jossa vaaleat sävyt (pikselin arvo alle 128) on kuljettavia pisteistä ja tummat sävyt (pikselin arvo vähintään 128) on seiniä.

Reitin voi valita vetämällä viivan halutusta aloituspisteestä haluttuun lopetuspisteeseen.