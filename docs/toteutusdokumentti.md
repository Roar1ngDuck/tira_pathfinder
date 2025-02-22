# Toteutusdokumentti

Laajoja kielimalleja ei ole käytetty projektissa.

## Suorituskykyvertailu

Algoritmien suorituskyky toisiinsa nähden on se mitä niiden kuuluisikin olla. Lähes kaikissa kartoissa mitä testasin on Dijkstra hitain, A* toiseksi nopein ja JPS nopein. Joissain kartoissa, kuten suurissa monimutkaisissa sokkeloissa, joissa A* heurestiikkafunktiosta ei ollut merkittävästi hyötyä ja se joutui käymään lähes yhtä monta pistettä läpi kuin Dijkstra, oli Dijkstra jopa hieman A* algoritmia nopeampi. Reittien hakuun kuluva aika myös vaihtelee aika paljon jokaisen suorituskerran välillä.

## Reitinhakualgoritmit

### Dijkstra

Koodi on [täällä](/src/Pathfinder/Pathfinding/Algorithms/Dijkstra.cs)

### A*
Koodi on [täällä](/src/Pathfinder/Pathfinding/Algorithms/AStar.cs)

### Jump Point Search
Koodi on [täällä](/src/Pathfinder/Pathfinding/Algorithms/JumpPointSearch.cs)

## Viitteet

[A* search algorithm](https://en.wikipedia.org/wiki/A*_search_algorithm)
[Online Graph Pruning for Pathfinding on Grid Maps](https://users.cecs.anu.edu.au/~dharabor/data/papers/harabor-grastien-aaai11.pdf)
[ICAPS 2014: Daniel Harabor on "Improving Jump Point Search"](https://www.youtube.com/watch?v=NmM4pv8uQwI)
[A Visual Explanation of Jump Point Search](https://zerowidth.com/2013/a-visual-explanation-of-jump-point-search/)
[Jump Search Algorithm in Python – A Helpful Guide](https://www.youtube.com/watch?v=afoQvbXvaiQ)