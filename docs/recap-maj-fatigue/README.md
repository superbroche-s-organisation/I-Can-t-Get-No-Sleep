# Récapitulatif de la mise à jour « Fatigue progressive »

**I Can't Get No Sleep — ICGNS**

Reconstitué le **31 octobre 2026** à partir des sources récupérées de GitHub.

Branche : `codex/progressive-fatigue` — commit de récupération : `b0511703044d955a7b527fb3496e434e6a51d443`. Ce récap comprend aussi l'ajout de la barre de fatigue réalisé ensuite sur cette branche.

Ce dossier est une nouvelle synthèse du travail présent dans le dépôt. Il ne reproduit pas à l'identique le document resté sur le PC portable.

## État de la mise à jour

La fatigue progressive est implémentée dans les sources, avec ses traductions et ses tests. Elle figure dans la section **Unreleased** du changelog ; le paquet public `ICGNS-1.3.0.zip` ne contient pas cette fonctionnalité.

Les **38 vérifications de logique indépendantes du jeu ont réussi**. Après ajout de la barre, la branche a également été compilée contre les DLL réelles de Stardew Valley/SMAPI : **594 contrôles automatisés ont réussi, rendu GPU inclus**. Une vraie session de jeu, notamment en multijoueur, reste à valider. Les détails sont dans [VALIDATION.md](VALIDATION.md).

## Objectif

Donner un coût progressif aux nuits blanches sans rétablir l'évanouissement automatique à 2 h. Le joueur peut continuer à jouer, mais plusieurs journées sans sommeil réduisent progressivement sa capacité d'énergie et sa vitesse.

La première nuit blanche ouvre un délai de grâce : atteindre 6 h ne donne pas immédiatement de malus. Le compteur démarre à cet instant et le premier palier arrive douze heures de jeu plus tard, à **18 h**.

## Progression et équilibrage

Dans ce tableau, **J** désigne le jour qui commence à 6 h après la première nuit blanche. La progression suppose que le joueur continue sans dormir et reste connecté.

| Instant | Temps depuis la première aube éveillée | Palier | Réduction d'énergie maximale | Malus de vitesse programmé |
| --- | --- | --- | --- | --- |
| J à 6 h | 0 h | 0 | Aucun | Aucun |
| J à 18 h | 12 h | 1 | 10 % | Aucun |
| J+1 à 6 h | 24 h | 2 | 20 % | Aucun |
| J+1 à 18 h | 36 h | 3 | 30 % | 0,25 |
| J+2 à 6 h | 48 h | 4 | 40 % | 0,50 |
| J+2 à 18 h | 60 h | 5 | 50 % | 0,75 |
| J+3 à 6 h | 72 h | 6 | 50 % | 1,00 |
| J+3 à 18 h | 84 h | 7 | 50 % | 1,25 |
| J+4 à 6 h | 96 h | 8 | 50 % | 1,50 |
| J+4 à 18 h | 108 h | 9 | 50 % | 1,75 |
| J+5 à 6 h | 120 h | 10 | 50 % | 2,00 |
| Ensuite | Un palier par tranche de 12 h | Continue à augmenter | Plafonné à 50 % | Plafonné à 2,00 |

Les valeurs de vitesse sont des **points de vitesse natifs**, pas des pourcentages. Leur application réelle, en particulier les fractions de 0,25, reste à contrôler dans le jeu.

Pour une capacité normale de 270 points d'énergie, les cinq premiers malus valent respectivement 27, 54, 81, 108 et 135 points. Le calcul utilise le maximum normal du joueur, y compris ses autres bonus, avec un arrondi du malus à l'entier inférieur.

## Récupération et interactions

| Situation | Comportement prévu dans les sources |
| --- | --- |
| Sommeil nocturne réellement accepté | Efface la fatigue et retire son effet avant la récupération native du jeu. |
| Véritable évanouissement nocturne par épuisement | Efface la fatigue lors du passage au lendemain. |
| Passage automatique à 6 h après une nuit blanche | Conserve le compteur et ses malus. |
| Vente du bac à 6 h activée | Ne compte pas comme du sommeil ; la fatigue continue. |
| Nourriture | Peut rendre de l'énergie jusqu'au maximum réduit, sans guérir la fatigue. |
| Autres bonus d'énergie ou de vitesse | Restent indépendants ; la fatigue recalcule son propre malus. |
| Chargement de partie ou reconnexion | Conserve la progression et recale l'horloge sans facturer le temps passé déconnecté. |
| Désactivation du mod | Retire l'effet et met le compteur en pause. |
| Nuit terminée avec le mod désactivé | Efface le compteur. |

Retirer le malus ne donne pas directement d'énergie. Un vrai sommeil conserve la récupération habituelle du jeu. La baisse du plafond ne transforme pas non plus une énergie négative en énergie positive.

Les sauvegardes existantes commencent à suivre la fatigue à partir de leur première nuit blanche avec cette fonctionnalité installée ; les nuits antérieures ne sont pas reconstituées.

## Affichage et configuration

- Un effet **« Fatigue — palier N »** affiche le niveau courant à partir du palier 1.
- Son infobulle indique la réduction d'énergie maximale et le malus de vitesse.
- Une notification accompagne une montée de palier observée pendant la partie.
- Les textes existent en français et en anglais.
- Une **barre violette F** apparaît près des jauges de vie et d'énergie uniquement à partir du palier 1. Elle est placée à gauche de la vie lorsque celle-ci est visible, sinon à gauche de l'énergie.
- Son remplissage augmente entre les paliers et atteint son maximum au palier 10, lorsque les deux malus sont plafonnés. Son infobulle conserve le véritable niveau, même au-delà de 10.
- La case **Afficher la barre de fatigue**, activée par défaut dans GMCM, correspond à `ShowFatigueBar: true` dans `config.json`. C'est une préférence visuelle propre à chaque joueur.
- Masquer la barre ne désactive pas les malus ni l'effet natif. Il n'y a actuellement **aucun interrupteur de gameplay propre à la fatigue** : celle-ci suit **Activer le mod**.
- Les données et l'affichage sont séparés par joueur et par écran. La validation d'une session multijoueur réelle reste à faire.

## Contenu du dossier

- [REPRISE-TECHNIQUE.md](REPRISE-TECHNIQUE.md) : fichiers concernés, stockage, points d'intégration et contexte pour reprendre le développement.
- [VALIDATION.md](VALIDATION.md) : résultat vérifié, commandes de tests et scénarios à contrôler avant une publication.

## Références du projet

- [Modèle de progression](../../ICGNS/FatigueProgress.cs)
- [Intégration de la fatigue dans le jeu](../../ICGNS/Fatigue.cs)
- [Tests de logique](../../tests/FatigueChecks.cs)
- [Documentation de développement en français](../DEVELOPMENT.fr.md)
- [Changelog](../../CHANGELOG.md)
