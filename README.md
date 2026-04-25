# Proiect ALJV - FPS Enemy AI

## Introducere

Proiectul își propune dezvoltarea și analiza unui agent AI capabil să simuleze comportamentul unui inamic într-un joc FPS, cu accent pe luarea deciziilor și comportamentul în luptă.

Prin acest proiect, voi implementa un sistem hibrid bazat pe **Behavior Trees** pentru decizii de nivel înalt și **Reinforcement Learnin** pentru optimizarea comportamentului de combat (mișcare și tragere).

## Motivație

Jocurile FPS reprezintă un mediu complex în care agenții AI trebuie să ia decizii rapide în condiții dinamice și incerte.

Am ales acest tip de joc deoarece dețin deja un prototip funcțional 3D, în care inamicii au comportamente simple bazate pe reguli (deplasare către jucător și tragere în funcție de distanță). Acest lucru oferă o bază practică pentru extinderea cu tehnici avansate de inteligență artificială.

## Scopul proiectului

Scopul acestui proiect este dezvoltarea și evaluarea unui agent AI pentru un joc FPS, utilizând o combinație între metode clasice și metode de învățare automată.

### Obiectivele principale

1. Implementarea unui sistem de decizie bazat pe Behavior Trees;
2. Dezvoltarea unui agent de combat utilizând Reinforced Learning;
3. Compararea performanței între AI-ul bazat pe reguli și AI-ul îmbunătățit;
4. Analiza impactului diferitelor funcții de recompensă asupra comportamentului agentului.

## Metodologie

### Configurarea mediului de testare

Mediul de testare va fi un joc FPS 3D dezvoltat în Unity, care include:

- un player controlat manual;
- unul sau mai mulți agenți inamici;
- sistem de deplasare bazat pe NavMesh;
- sistem de tragere bazat pe raycasting;
- detecție a jucătorului în funcție de distanță și vizibilitate.

Pentru experimente, vor fi definite scenarii controlate (ex: duel 1 vs 1 într-o hartă simplificată).

### Implementare AI

Agentul AI va fi structurat pe două niveluri:

#### 1. Behavior Tree (decizie high-level)

Behavior Tree-ul va controla stările principale ale agentului:

- **Patrol** (când nu detectează player-ul);
- **Chase** (când player-ul este detectat);
- **Attack** (când player-ul este în raza de tragere);
- **Retreat** (când HP este scăzut).

Deciziile vor fi luate pe baza:

- distanței față de player;
- vizibilității (*line-of-sight*);
- stării agentului (HP).

#### 2. Reinforcement Learning (combat behavior)

Algoritmul va fi utilizat pentru a învăța comportamente de luptă eficiente.

##### Intrări (stare)
- poziția relativă a player-ului;
- distanța până la player;
- direcția de deplasare a player-ului;
- HP agent;
- indicator de vizibilitate (*line-of-sight*).

##### Acțiuni
- deplasare laterală (stânga/dreapta);
- deplasare înainte/înapoi;
- tragere (*shoot / no shoot*);
- ajustare direcție de țintire.

##### Funcția de recompensă
- +1 pentru fiecare lovitură (*hit*);
- +5 pentru eliminarea player-ului;
- -1 pentru damage primit;
- -5 pentru moarte;
- penalizare mică pentru inactivitate.

### Performanțe

Performanța agentului va fi evaluată folosind următorii indicatori:

- rata de lovire (*hit accuracy*);
- rata de eliminare (*kill rate*);
- timpul mediu până la eliminarea adversarului;
- durata de supraviețuire;
- stabilitatea comportamentului (variația între episoade).

Rezultatele vor fi comparate între:

- AI bazat pe reguli (versiunea inițială);
- AI hibrid 

## Concluzie

Acest proiect va demonstra modul în care tehnicile moderne de inteligență artificială, precum Behavior Trees și Reinforcement Learning, pot fi integrate pentru a obține agenți credibili și eficienți într-un joc FPS.

Rezultatele vor evidenția avantajele metodelor de învățare automată față de abordările bazate exclusiv pe reguli și vor oferi o bază pentru dezvoltări ulterioare în domeniul AI pentru jocuri.
