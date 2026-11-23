# RunCraft

Juego de carreras infinitas en 3D con estilo de bloques, hecho en Unity 6 para Android.
Toni corre por una pradera, esquiva mobs y bloques, recoge minerales y escapa de un golem de piedra.

Todo el arte, los modelos y los sonidos se generan por código (son originales, sin archivos externos).

## Cómo probarlo

1. Abre la carpeta del proyecto con Unity Hub (Unity 6000.6.6f1).
2. Espera a que compile. En la consola verás `RunCraft: material base creado...`.
3. Abre `Assets/Scenes/SampleScene` y pulsa **Play**.
4. En la vista Game elige una resolución vertical (por ejemplo 1080x1920) para verlo como en el móvil.

### Controles

| Acción | Móvil | PC |
| --- | --- | --- |
| Cambiar de carril | deslizar ← → | flechas ← → o A / D |
| Saltar | deslizar ↑ | flecha ↑, W o Espacio |
| Agacharse / rodar | deslizar ↓ | flecha ↓ o S |
| Pausa | botón II | Esc o P |

En el editor también puedes arrastrar con el ratón como si fuera un dedo.

## Reglas

- Chocar de frente con un obstáculo = fin de la partida.
- Chocar de lado al cambiar de carril = tropiezas y el golem se acerca. Si tropiezas otra vez mientras está cerca, te atrapa.
- Obstáculos bajos (cerdos, ovejas, heno): salta. Altos (arañas colgando, telarañas): agáchate. Aldeanos, zombis y muros: cambia de carril o súbete por las rampas.
- Minerales: carbón 1, hierro 2, piedra roja 3, lapislázuli 4, oro 5, esmeralda 8, diamante 10.
- Power-ups (10 s): imán de gemas y botas de salto.

## Generar el APK para Android

1. En Unity Hub > Installs > tu versión > Add modules, instala **Android Build Support** (con OpenJDK y Android SDK & NDK).
2. En Unity: File > Build Profiles > Android > Switch Platform.
3. Activa el modo desarrollador y la depuración USB en tu móvil y conéctalo por cable.
4. Pulsa **Build and Run**.

## Estructura

```
Assets/RunCraft/
  Scripts/Core     GameBootstrap (arranque), GameManager (estados y puntos), Loc (idiomas y guardado)
  Scripts/Player   PlayerController (Toni), SwipeInput (controles), Golem, CameraRig
  Scripts/World    TrackGenerator (pradera y obstáculos), Obstacle, Pickup (minerales y power-ups)
  Scripts/Visual   Blocks, Models (Toni, golem, mobs), Textures (texturas pixeladas), Mats
  Scripts/UI       UIManager (menús en español e inglés)
  Scripts/Audio    AudioManager (sonidos y música generados por código)
  Editor           RunCraftSetup (prepara el material y los ajustes de Android)
  Resources/Audio  pon aquí tus propios sonidos si quieres cambiarlos (ver LEEME.txt)
```
