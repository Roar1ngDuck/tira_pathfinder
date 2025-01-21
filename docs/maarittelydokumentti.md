# Määrittelydokumentti

Opinto-ohjelma: Tietojenkäsittelytieteen kandidaatti.

Dokumentaation kieli: Suomi.

## Aihe
Projektin aiheena on reitinhakualgoritmien visuaalinen vertailu pikselikartoilla. Reittien visualisointi tapahtuu Avalonia UI frameworkin avulla.

Ohjelma saa syötteenä:
 - Polun tiedostoon, jossa on pikselikartta
 - Aloituspisteen koordinaatit
 - Maalipisteen koordinaatit

Ohjelma etsii lyhimmän reitin aloituspisteestä maalipisteeseen annetussa pikselikartassa. Karttoina käytän [Moving AI Lab](https://www.movingai.com/benchmarks/grids.html) sivulta löytyviä karttoja

## Algoritmit

Toteutan projektissa [A*](https://en.wikipedia.org/wiki/A*_search_algorithm) ja [JPS](https://users.cecs.anu.edu.au/~dharabor/data/papers/harabor-grastien-aaai11.pdf) reitinhakualgoritmit.

A* worst-case aika- ja tilavaatimukset on O(b^d), joissa d on lyhyimmän reitin pituus ja b on haarautumistekijä (branching factor) eli solmujen keskimääräinen lasten lukumäärä. A* aikavaatimus riippuu hyvin paljon käytetystä heurestiikkafunktiosta, ja optimaalisella heurestiikkafunktiolla b=1.

## Ydin
Harjoitustyön ydin on tehokkaiden A* ja JPS reitinhakualgoritmien toteutus.

## Kielet
Projektin ohjelmointikieli on C#.
Muut kielet joilla tehtyjä projekteja voin vertaisarvioida: Python, JS, Java, C++

## Viitteet
 - https://en.wikipedia.org/wiki/A*_search_algorithm
 - https://users.cecs.anu.edu.au/~dharabor/data/papers/harabor-grastien-aaai11.pdf
 - https://www.movingai.com/benchmarks/grids.html