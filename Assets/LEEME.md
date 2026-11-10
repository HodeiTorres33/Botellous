# Estructura del proyecto Botellous

```
Assets/
  Escenas/      Escenas del juego, en el orden en que se juegan:
                MenuPrincipal → Level1Joel → Nivel2_Catacumbas → Nivel3
  Comun/        Lo que usan TODOS los niveles
    Jugador/    El jugador (Scripts, Animaciones, Sprites, Prefabs/Jugador.prefab)
    UI/         Menú principal, menú de pausa (Esc), controles táctiles de Android y barras de vida
  Nivel1/       Solo del nivel 1: Scripts, Prefabs, Sprites, Tiles, Audio
  Nivel2/       Solo del nivel 2: Scripts, Prefabs, Sprites, Animaciones, Tiles, Audio
  Nivel3/       Solo del nivel 3: Scripts (jefe), Jefe (sprites y animaciones), Fondos,
                AssetsExternos (packs descargados: karsiori, Bringer Of Death)
  Settings/     Ajustes de render de Unity (no tocar)
  Welcome/      Pantalla de bienvenida de la plantilla 2D de Unity (se puede borrar)
```

- Las carpetas `_SinUso` tienen scripts antiguos que ya no usa ninguna escena
  (el `PlayerController` del nivel 1 y el `MunecoPrueba` del nivel 2).
- Para mover archivos, hazlo siempre desde la ventana Project de Unity, no desde el explorador
  de Windows, así Unity mantiene las referencias.
