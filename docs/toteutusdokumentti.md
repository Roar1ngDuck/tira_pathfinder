# Toteutusdokumentti

## Rakenne

Ohjelma käyttää Avalonia UI kirjastoa käyttöliittymässä. Rakenteen voi jakaa korkealla tasolla seuraaviin:
- UI logiikka (reittien visualisointi, käyttäjän syötteet). On pääosin partial luokiksi jaetussa MainWindow luokassa.
- Reitinhakualgoritmit (Dijkstra, A*, JPS). Algoritmit on jaettu omiin luokkiinsa ```Pathfinder.Pathfinding.Algorithms``` alle.
- Apuluokat ja funktiot (karttojen lukeminen, etäisyyksien laskeminen, karttaesteiden tarkostus). Osa funktioista ```Pathfinder.Helpers``` ja osa ```Pathfinder.Pathfinding.Utils``` alla.

## Käyttöliittymä
1. MainWindow.Drawing
    - Lataa ja piirtää valitun kartan
    - Piirtää reitit kartalle
2. MainWindow.PathfindingLogic
    - Ajaa reitinhaun käyttäjän valitseman aloitus ja lopetuspisteen sekä algoritmin perusteella.
    - Mittaa ajan ja näyttää statistiikat läpi käydyistä pisteistä.

## Reitinhakualgoritmit

### Dijkstra

Koodi on [täällä](/src/Pathfinder/Pathfinding/Algorithms/Dijkstra.cs)

Käyttää PriorityQueue rakennetta johon läpi käytävien pisteiden naapurit lisätään ja josta valitaan pienimmällä kuljetulla etäisyydellä oleva piste seuraavaksi prosessoitavaksi.

### A*

Koodi on [täällä](/src/Pathfinder/Pathfinding/Algorithms/AStar.cs)

Hyvin samanlainen toteutus kuin Dijkstra, paitsi että käytetään heurestiikkafunktiota laskemaan pisteen pienin mahdollinen etäisyys tiedettyyn maalipisteeseen.

### Jump Point Search

Koodi on [täällä](/src/Pathfinder/Pathfinding/Algorithms/JumpPointSearch.cs)

Käyttää myös heurestiikkafunktiota kuten A*, mutta se eroaa siten, että se etsii "Jump Point" pisteitä ja vain nämä pisteet lisätään prioriteettijonoon. Algoritmi valitsee kulkusuunnan ja kulkee samaan suuntaan kunnes löytää Jump Pointin tai ei voi enää jatkaa. Jump Point pisteet on sellaisia pisteitä, joista reitti voi mahdollisesti kääntyä eri suuntaan. Algoritmi löytää ne etsimällä kohtia, josta kohtisuoraan liikkumalla kuljetaan seinän ohi. Reittejä jotka vain törmäävät seinään ei sen enempää tutkita. Alemmassa kuvassa algoritmi on liikkumassa vinottain ylös ja keltaisessa pisteessä algoritmi käy läpi pisteitä oikealle ja löytää kuvassa sinisen pisteen, jonka jälkeen reitti kulkee seinän ohi. Tilanteessa keltainen piste merkitään Jump Pointiksi. Kun algoritmi jatkaa eteenpäin niin myös sininen piste merkitään Jump Pointiksi.

![jps](jps1.png)
> **Kuvan lähde:** [A Visual Explanation of Jump Point Search](https://zerowidth.com/2013/a-visual-explanation-of-jump-point-search/)

## Suorituskykyvertailu

Algoritmien suorituskyky toisiinsa nähden on se mitä niiden kuuluisikin olla. Lähes kaikissa kartoissa mitä testasin on Dijkstra hitain, A* toiseksi nopein ja JPS nopein. Joissain kartoissa, kuten suurissa monimutkaisissa sokkeloissa, joissa A* heurestiikkafunktiosta ei ollut merkittävästi hyötyä ja se joutui käymään lähes yhtä monta pistettä läpi kuin Dijkstra, oli Dijkstra jopa hieman A* algoritmia nopeampi. Reittien hakuun kuluva aika myös vaihtelee aika paljon jokaisen suorituskerran välillä.

Pahimmassa tapauksessa aikavaatimus pikselikartoilla kaikilla algoritmeilla kun vinottaiset liikkeet sallitaan on O(8^d), jossa d on lyhimmän reitin pituus. Käytännössä kuitenkin A* ja JPS saavuttaa usein paremman tuloksen.

## Puutteet ja parannusehdotukset

Ohjelmassa voisi olla enemmän eri heurestiikkafunktioita joista voisi valita haluamansa. JPS algoritmiin voisi myös tehdä paljon parannuksia esim. [Improving Jump Point Search](https://users.cecs.anu.edu.au/~dharabor/data/papers/harabor-grastien-icaps14.pdf) julkaisun perusteella.

## Kielimallit

Laajoja kielimalleja ei ole käytetty projektissa.

## Viitteet

- [A* search algorithm](https://en.wikipedia.org/wiki/A*_search_algorithm)
- [Online Graph Pruning for Pathfinding on Grid Maps](https://users.cecs.anu.edu.au/~dharabor/data/papers/harabor-grastien-aaai11.pdf)
- [ICAPS 2014: Daniel Harabor on "Improving Jump Point Search"](https://www.youtube.com/watch?v=NmM4pv8uQwI)
- [A Visual Explanation of Jump Point Search](https://zerowidth.com/2013/a-visual-explanation-of-jump-point-search/)
- [Jump Search Algorithm in Python – A Helpful Guide](https://www.youtube.com/watch?v=afoQvbXvaiQ)
- [Heuristics](https://theory.stanford.edu/~amitp/GameProgramming/Heuristics.html)