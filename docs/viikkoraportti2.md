# Viikkoraportti 2

Tällä viikolla toteutin A* algoritmin ja toteutin toimivan käyttöliittymän ohjelmaan. A* algoritmi tukee reittien etsintää sekä sillon kun vinottaiset siirrot on sallittuja ja sillon kun ne ei ole. Lisäsin myös Dijkstran algoritmin, koska se oli hyvin lähellä A* algoritmin toteutusta, jolloin se meni aika lailla samalla. Testasin myös A* algoritmin nopeutta hieman eri tyylisillä toteutuksilla. Nykyinen toteutus on optimoitu toimimaan juuri 2d pikselikartoissa ja sitä ei voisi ihan suoraan edes käyttää muunlaisissa tilanteissa.

Tein myös muutamia testejä, jotka katsoo pienessä kartassa että löytääkö A* lyhyimmän reitin, käykö se läpi oikeat pisteet, ja toimiiko oikein kun reittiä ei ole olemassa. Tein myös pari testiä mitkä vertaa että löytääkö A* ja Dijkstra samanpituiset reitit isommissa 256x256 generoiduissa kartoissa.

Seuraavaksi alan toteuttamaan JPS algoritmia. A* koodia myös pitää vähän siistiä.

Aikaa käytin yhteensä noin 15 tuntia.